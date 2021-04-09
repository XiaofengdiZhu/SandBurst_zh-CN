using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Pipes;
using System.IO;

namespace SandBurst
{
    class CoreServer
    {
        /// <summary>
        /// 共有メモリようファイルマップ
        /// </summary>
        private IntPtr fileMap;

        /// <summary>
        /// CoreDllに渡す値
        /// Dllとの共有メモリに展開される
        /// </summary>
        unsafe private CORRECTION_INFORMATION* correctionInfo;

        unsafe public CoreServer()
        {
            fileMap = Win32.API.CreateFileMapping(
                (IntPtr)(-1),
                IntPtr.Zero,
                Win32.Constants.PAGE_READWRITE,
                0,
                0x1000,
                "SandBurstShareMemory"
            );

            if (fileMap == IntPtr.Zero)
                throw new System.IO.IOException("共有メモリの作成に失敗しました");

            correctionInfo = (CORRECTION_INFORMATION*)(Win32.API.MapViewOfFile(
                fileMap, 
                Win32.Constants.FILE_MAP_ALL_ACCESS, 
                0, 0, 0x1000
                ));
        }

        unsafe ~CoreServer()
        {
            Win32.API.UnmapViewOfFile((IntPtr)correctionInfo);
            Win32.API.CloseHandle(fileMap);
        }

        /// <summary>
        /// Coreライブラリをwindowを保持するプロセスに注入する
        /// </summary>
        /// <param name="window">WHND</param>
        /// <returns></returns>
        unsafe private bool InjectCoreDll(IntPtr window)
        {
            
            uint processId;
            Win32.API.GetWindowThreadProcessId(window, out processId);

            IntPtr process = Win32.API.OpenProcess(Win32.Constants.PROCESS_ALL_ACCESS, false, processId);
            if (process == IntPtr.Zero)
            {
                return false;
            }

            IntPtr kernel = IntPtr.Zero;
            IntPtr wow64 = IntPtr.Zero;

            IntPtr[] modules = new IntPtr[256];
            uint size;

            if (!Win32.API.EnumProcessModulesEx(process, modules, (uint)sizeof(IntPtr) * 256, out size, Win32.Constants.LIST_MODULES_ALL))
            {
                ErrorHelper.ShowErrorMessage("モジュール一覧の取得に失敗しました");
                return false;
            }

            StringBuilder modulePath = new StringBuilder(260);
            for (int i = 0; i < size / sizeof(IntPtr); i++)
            {
                Win32.API.GetModuleFileNameEx(process, modules[i], modulePath, (uint)modulePath.Capacity);

                string moduleName = System.IO.Path.GetFileName(modulePath.ToString()).ToLower();
                if (moduleName == "kernel32.dll")
                {
                    kernel = modules[i];
                    if (wow64 != IntPtr.Zero)
                    {
                        break;
                    }
                }
                else if (moduleName == "wow64.dll")
                {
                    wow64 = modules[i];
                    if (kernel != IntPtr.Zero)
                    {
                        break;
                    }
                }
            }

            if (kernel == IntPtr.Zero)
            {
                ErrorHelper.ShowErrorMessage("kernel32.dllのアドレスの取得に失敗しました");
                Win32.API.CloseHandle(process);
                return false;
            }


            ulong threadID = 0;
            IntPtr kernel2 = Win32.API.GetModuleHandle("kernel32.dll");

            IntPtr mem = Win32.API.VirtualAllocEx(process, IntPtr.Zero, 0x1000, Win32.Constants.MEM_COMMIT, Win32.Constants.PAGE_READWRITE);
            if (mem == IntPtr.Zero)
            {
                ErrorHelper.ShowErrorMessage("VirtualAllocExに失敗しました");
                Win32.API.CloseHandle(process);
                return false;
            }

            Win32.API.GetModuleFileName(IntPtr.Zero, modulePath, (uint)modulePath.Capacity);

            uint written = 0;
            string hookDllName;

            // 現段階では強制的にWow64にさせる
            wow64 = (IntPtr)1;
            if (wow64 == IntPtr.Zero)
            {
                hookDllName = System.IO.Path.GetDirectoryName(modulePath.ToString()) + "\\SandBurstCore64.dll\0\0";
            }
            else
            {
                hookDllName = System.IO.Path.GetDirectoryName(modulePath.ToString()) + "\\SandBurstCore.dll\0\0";
            }


            char[] c = hookDllName.ToCharArray();
            IntPtr buffer = Marshal.AllocHGlobal(c.Length * Marshal.SystemDefaultCharSize);

            Marshal.Copy(c, 0, buffer, c.Length);

            Win32.API.WriteProcessMemory(process, mem, buffer, (uint)(hookDllName.Length * Marshal.SystemDefaultCharSize), out written);

            Marshal.FreeHGlobal(buffer);


            IntPtr loadLibrary;

            if ((wow64 == IntPtr.Zero) || (!Environment.Is64BitOperatingSystem))
            {
                loadLibrary = (IntPtr)((ulong)Win32.API.GetProcAddress(kernel2, "LoadLibraryW") - (ulong)kernel2 + (ulong)kernel);
            }
            else 
            {
                string winDir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Windows);
                string kernelPath = winDir + "\\SysWOW64\\kernel32.dll";
                IntPtr peFile = PeInit();
                PeLoadFromFile(peFile, kernelPath);
                loadLibrary = (IntPtr)((ulong)PeGetProcRVA(peFile, "LoadLibraryW") + (ulong)kernel);
                PeFree(peFile);
            }


            IntPtr thread = Win32.API.CreateRemoteThread(process, IntPtr.Zero, 0, loadLibrary, mem, 0, ref threadID);

            if (thread == IntPtr.Zero)
            {
                ErrorHelper.ShowErrorMessage("CrateRemoteThreadに失敗しました");
                Win32.API.VirtualFreeEx(process, mem, 0x1000, Win32.Constants.MEM_DECOMMIT);
                return false;
            }

            Win32.API.WaitForSingleObject(thread, 3000);
            Win32.API.CloseHandle(thread);
            Win32.API.CloseHandle(process);

            Win32.API.VirtualFreeEx(process, mem, 0x1000, Win32.Constants.MEM_DECOMMIT);

            return true;
        }

        /// <summary>
        /// Windowハンドルからパイプ名を作成
        /// </summary>
        /// <param name="window"></param>
        /// <returns></returns>
        private string GetPipeName(IntPtr window)
        {
            uint processId;

            Win32.API.GetWindowThreadProcessId(window, out processId);

            IntPtr process = Win32.API.OpenProcess(Win32.Constants.PROCESS_ALL_ACCESS, false, processId);

            StringBuilder modulePath = new StringBuilder(260);

            Win32.API.GetModuleFileNameEx(process, IntPtr.Zero, modulePath, 260);

            string exeName = System.IO.Path.GetFileName(modulePath.ToString());

            return $"SBPipe_{exeName}"; 
        }

        /// <summary>
        /// パイプに命令を書き込む
        /// </summary>
        /// <param name="window">対象プロセスのウィンドウ</param>
        /// <param name="buffer">書き込む命令 'i': install, 'u': uninstall</param>
        /// <returns>true: 成功時</returns>
        private bool WritePipe(IntPtr window, char buffer)
        {
            try
            {
                using (var stream = new NamedPipeClientStream(GetPipeName(window)))
                {
                    stream.Connect(100);

                    using (var writer = new StreamWriter(stream))
                    {
                        writer.Write(buffer);

                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }

        /// <summary>
        /// correctionInfo を設定する
        /// </summary>
        /// <param name="window"></param>
        /// <param name="sandBurstWindow"></param>
        /// <param name="shadowWindow"></param>
        /// <param name="scale"></param>
        /// <param name="windowSize"></param>
        /// <param name="clientSize"></param>
        /// <param name="setting"></param>
        /// <param name="d3d9PresentRVA"></param>
        /// <param name="d3d11SetViewportRVA"></param>
        unsafe private void makeCorrectionInfo(IntPtr window,
            IntPtr sandBurstWindow,
            IntPtr shadowWindow,
            int scale,
            Win32.POINT windowSize,
            Win32.POINT clientSize,
            CorrectionSetting setting,
            uint d3d9PresentRVA,
            uint d3d11SetViewportRVA,
            Win32.RECT display)
        {
            correctionInfo->hWnd = window;
            correctionInfo->hSandBurst = sandBurstWindow;
            correctionInfo->hShadow = shadowWindow;
            correctionInfo->Scale = scale;
            correctionInfo->WindowSize = windowSize;
            correctionInfo->ClientSize = clientSize;
            correctionInfo->D3D9PresentRVA = d3d9PresentRVA;
            correctionInfo->D3D11SetViewportRVA = d3d11SetViewportRVA;
            correctionInfo->ModifiesWindowSize = System.Convert.ToInt32(setting.WindowSize);
            correctionInfo->ModifiesChildWindowSize = System.Convert.ToInt32(setting.ChildWindowSize);
            correctionInfo->HookType = System.Convert.ToUInt32(setting.HookType);
            correctionInfo->CentralizesWindow = System.Convert.ToInt32(setting.CentralizesWindow);
            correctionInfo->ExcludesTaskbar = System.Convert.ToInt32(setting.ExcludesTaskbar);
            correctionInfo->MessageHook = System.Convert.ToInt32(setting.MsgHook);
            correctionInfo->SetCursorPosHook = System.Convert.ToInt32(setting.SetCursorPos);
            correctionInfo->GetCursorPosHook = System.Convert.ToInt32(setting.GetCursorPos);
            correctionInfo->MoveWindowHook = System.Convert.ToInt32(setting.MoveWindow);
            correctionInfo->SetWindowPosHook = System.Convert.ToInt32(setting.SetWindowPos);
            correctionInfo->SetWindowPlacementHook = System.Convert.ToInt32(setting.SetWindowPlacement);
            correctionInfo->ClipCursolHook = System.Convert.ToInt32(setting.ClipCursor);
            correctionInfo->ScreenShotHook = System.Convert.ToInt32(setting.ScreenShot);
            correctionInfo->GetWindowRectHook = System.Convert.ToInt32(setting.GetWindowRect);
            correctionInfo->GetClientRectHook = System.Convert.ToInt32(setting.GetClientRect);
            correctionInfo->D3DHook = System.Convert.ToInt32(setting.D3D);
            correctionInfo->WM_SIZE_Hook = System.Convert.ToInt32(setting.WmSize);
            correctionInfo->WM_WINDOWPOS_Hook = System.Convert.ToInt32(setting.WmWindowPos);
            correctionInfo->Display = display;

            // 32bit Windowsの場合はWow64の設定を無視してAPIベースに上書きする
            if (!Environment.Is64BitOperatingSystem)
                correctionInfo->HookType = 0;
        }

        /// <summary>
        /// Coreライブラリをインストールする
        /// </summary>
        /// <param name="window"></param>
        /// <returns></returns>
        public bool InstallHook(
            IntPtr window,
            IntPtr sandBurstWindow,
            IntPtr shadowWindow,
            int scale, 
            Win32.POINT windowSize,
            Win32.POINT clientSize,
            CorrectionSetting setting,
            uint d3d9PresentRVA,
            uint d3d11SetViewportRVA,
            Win32.RECT display)
        {

            makeCorrectionInfo(
                window,
                sandBurstWindow,
                shadowWindow,
                scale,
                windowSize,
                clientSize,
                setting,
                d3d9PresentRVA,
                d3d11SetViewportRVA,
                display);

            if (!WritePipe(window, 'i'))
            {
                if (!InjectCoreDll(window))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Coreライブラリをアンインストールする
        /// </summary>
        /// <param name="window"></param>
        /// <returns></returns>
        public bool UninstallHook(IntPtr window)
        {
            return WritePipe(window, 'u');
        }

        /// <summary>
        /// 対象プロセスのコマンドラインを取得する
        /// </summary>
        /// <param name="window"></param>
        /// <returns>失敗したらnull</returns>
        unsafe public string GetCommandLine(IntPtr window)
        {
            uint processId;
            Win32.API.GetWindowThreadProcessId(window, out processId);

            StringBuilder commands = new StringBuilder(0x1000);

            if (!GetCommandLineEx(processId, commands, commands.Capacity))
                return null;

            string str = commands.ToString();

            return str;
        }

        [DllImport("PEFile.dll")]
        public static extern IntPtr PeInit();
        [DllImport("PEFile.dll")]
        public static extern void PeFree(IntPtr peFile);
        [DllImport("PEFile.dll", CharSet = CharSet.Ansi)]
        public static extern bool PeLoadFromFile(IntPtr peFile, string fileName);
        [DllImport("PEFile.dll")]
        public static extern bool PeReleaseFile(IntPtr peFile);
        [DllImport("PEFile.dll", CharSet = CharSet.Ansi)]
        public static extern uint PeGetProcRVA(IntPtr peFile, string procName);

        [DllImport("WinHelper.dll", CharSet = CharSet.Unicode)]
        public static extern bool GetCommandLineEx(uint processId, StringBuilder str, int size);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CORRECTION_INFORMATION
    {
        // 各種ウィンドウハンドル
        public IntPtr hWnd;
#if _WIN64
#else
        public uint Reserved1;
#endif
        public IntPtr hSandBurst;
#if _WIN64
#else
        public uint Reserved2;
#endif
        public IntPtr hShadow;
#if _WIN64
#else
        public uint Reserved3;
#endif

        // 拡大サイズ他
        public int Scale;
        public Win32.POINT WindowSize;
        public Win32.POINT ClientSize;

        // D3D
        public uint D3D9PresentRVA;
        public uint D3D11SetViewportRVA;
        // ウィンドウサイズ変更
        public int ModifiesWindowSize;
        public int ModifiesChildWindowSize;

        /// <summary>
        /// 0: WinAPI, 1: Wow64
        /// </summary>
        public uint HookType;

        // 中央寄せ、タスクバー除外
        public int CentralizesWindow;
        public int ExcludesTaskbar;

        // ディスプレイ
        public Win32.RECT Display;

        // 各種APIフック
        public int MessageHook;
        public int SetCursorPosHook;
        public int GetCursorPosHook;
        public int MoveWindowHook;
        public int SetWindowPosHook;
        public int SetWindowPlacementHook;
        public int ClipCursolHook;
        public int ScreenShotHook;
        public int GetWindowRectHook;
        public int GetClientRectHook;
        public int D3DHook;
        public int WM_SIZE_Hook;
        public int WM_WINDOWPOS_Hook;
    }
}
