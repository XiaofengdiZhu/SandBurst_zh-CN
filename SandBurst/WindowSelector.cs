using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Win32;
using System.Runtime.InteropServices;
using System.Text;

namespace SandBurst
{
    /// <summary>
    /// トップレベルウィンドウの取得クラス
    /// ignores.txtによって不要なWindowを除外する
    /// </summary>
    public class WindowSelector
    {
        private List<string> ignoreList;
        private List<IntPtr> ignoreHandles;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ignoreFilePath">除外リストのファイルパス</param>
        /// <param name="ignoreHandles">除外するウィンドウハンドル</param>
        public WindowSelector(string ignoreFilePath, IntPtr[] ignoreHandles)
        {

            this.ignoreHandles = ignoreHandles.ToList();

            if (ignoreFilePath == null)
                return;

            ignoreList = File.ReadLines(ignoreFilePath).ToList<string>();
        }

        /// <summary>
        /// Windowリストを取得する
        /// </summary>
        /// <returns></returns>
        public List<WindowInformation> GetWindows()
        {
            List<WindowInformation> windowList = new List<WindowInformation>();
            List<IntPtr> handleList = GetDesktopWindows();
            System.Text.StringBuilder title = new System.Text.StringBuilder(256);

            for (int i = 0; i < handleList.Count; i++)
            {
                IntPtr handle = handleList[i];

                RECT rect;
                Win32.API.GetWindowRect(handle, out rect);

                int cx = rect.right - rect.left;
                int cy = rect.bottom - rect.top;
                

                if ((cx > 0) && (cy > 0))
                {
                    Win32.API.GetWindowText(handle, title, 256);

                    string exePath = GetExeFileNameFromHandle(handle);
                    
                    if ((exePath != null) && (!ContainIgnores(exePath)) && (!IgnoresWindow(handle)))
                    {
                        POINT windowSize, clientSize;
                        WindowHelper.GetWindowSize(handle, out windowSize, out clientSize);
                        WindowInformation info = new WindowInformation(handle, title.ToString(), exePath, windowSize, clientSize);
                        windowList.Add(info);
                    }
                }
            }


            return windowList;
        }

        /// <summary>
        /// 該当するWindowを取得する
        /// </summary>
        /// <param name="path"></param>
        /// <param name="title"></param>
        /// <param name="windowSize"></param>
        /// <returns></returns>
        public WindowInformation Find(string path, string title)
        {
            List<WindowInformation> list = GetWindows();

            foreach (WindowInformation info in list)
            {
                if ((info.Path == path) && (info.Title == title))
                {
                        return info;
                }
                    
            }

            return null;
        }

        /// <summary>
        /// ignoreListに該当するか判定する
        /// </summary>
        /// <param name="exePath"></param>
        /// <returns></returns>
        private bool ContainIgnores(string exePath)
        {
            if (ignoreList == null)
                return false;

            for (int i = 0; i < ignoreList.Count; i++)
            {
                if (exePath.ToLower().Contains(ignoreList[i].ToLower()))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Windowハンドルから実行ファイルのフルパスを取得する
        /// </summary>
        /// <param name="wnd"></param>
        /// <returns></returns>
        private string GetExeFileNameFromHandle(IntPtr wnd)
        {
            uint id;
            Win32.API.GetWindowThreadProcessId(wnd, out id);

            uint access = Win32.Constants.PROCESS_QUERY_INFORMATION | Win32.Constants.PROCESS_VM_READ;
            IntPtr process = IntPtr.Zero;


            try
            {
                process = Win32.API.OpenProcess(access, false, id);

                if (process == IntPtr.Zero)
                    return null;

                IntPtr[] modules = new IntPtr[1];
                uint needed;

                if (!Win32.API.EnumProcessModulesEx(process, modules, (uint)Marshal.SizeOf(modules[0]), out needed, 0))
                    return null;

                System.Text.StringBuilder fileName = new System.Text.StringBuilder(256);

                if (Win32.API.GetModuleFileNameEx(process, modules[0], fileName, 256) == 0)
                    return null;

                return fileName.ToString();
            }
            finally
            {
                if (process != IntPtr.Zero)
                    Win32.API.CloseHandle(process);
            }
        }

        /// <summary>
        /// トップレベルウィンドウを全て取得する
        /// </summary>
        /// <returns></returns>
        private List<IntPtr> GetDesktopWindows()
        {
            IntPtr hWnd;
            int style;
            List<IntPtr> list = new List<IntPtr>();

            System.Text.StringBuilder tmp = new System.Text.StringBuilder(256);


            hWnd = Win32.API.GetWindow(Win32.API.GetDesktopWindow(), Win32.Constants.GW_CHILD);

            while (hWnd != IntPtr.Zero)
            {
                if (Win32.API.IsWindow(hWnd))
                {
                    if (Win32.API.IsWindowVisible(hWnd))
                    {
                        // style = (long)Win32.API.GetWindowLongPtr(hWnd, Win32.Constants.GWL_STYLE);
                        style = Win32.API.GetWindowLong(hWnd, Win32.Constants.GWL_STYLE);

                        if ((style & Win32.Constants.WS_CAPTION) != 0)
                        {

                            list.Add(hWnd);
                        }
                        else
                        {
                            if (Win32.API.GetWindowText(hWnd, tmp, 255) > 0)
                            {
                                if ((tmp.ToString() != "スタート") && (tmp.ToString() != "Program Manager"))
                                {
                                    list.Add(hWnd);
                                }

                            }
                        }
                    }
                }

                hWnd = Win32.API.GetWindow(hWnd, Win32.Constants.GW_HWNDNEXT);
            }

            return list;
        }

        /// <summary>
        /// windowを除外するか判定する
        /// </summary>
        /// <param name="window"></param>
        /// <returns></returns>
        private bool IgnoresWindow(IntPtr window)
        {
            if (ignoreHandles == null)
                return false;

            foreach (IntPtr ignoreHandle in ignoreHandles)
            {
                if (window == ignoreHandle)
                    return true;
            }

            return false;
        }
        
    }
    
    public class WindowInformation
    {
        public WindowInformation(IntPtr window, string title, string path, POINT windowSize, POINT clientSize)
        {
            Window = window;
            Title = title;
            Path = path;
            WindowSize = windowSize;
            ClientSize = clientSize;
        }

        public IntPtr Window;
        public string Title;
        public string Path;
        public POINT WindowSize;
        public POINT ClientSize;
    }
}