using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Win32
{
    static class API
    {
        [DllImport("user32.dll", EntryPoint = "GetDesktopWindow")]
        public extern static IntPtr GetDesktopWindow();

        [DllImport("user32.dll", EntryPoint = "GetWindow")]
        public extern static IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        [DllImport("user32.dll", EntryPoint = "IsWindow")]
        public extern static bool IsWindow(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "IsWindowVisible")]
        public extern static bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        public extern static int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowText")]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", EntryPoint = "GetWindowRect")]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("kernel32.dll", EntryPoint = "CreateFileMappingA", CharSet = CharSet.Ansi)]
        public static extern IntPtr CreateFileMapping(
            IntPtr hFile,
            IntPtr lpAttributes,
            uint flProtect,
            uint dwMaximumSizeHigh,
            uint dwMaximumSizeLow,
            string lpName
        );

        [DllImport("kernel32.dll", EntryPoint = "MapViewOfFile")]
        public static extern IntPtr MapViewOfFile(
            IntPtr hFileMappingObject,
            uint dwDesiredAccess,
            uint dwFileOffsetHigh,
            uint dwFileOffsetLow,
            uint dwNumberOfBytesToMap
        );

        [DllImport("kernel32.dll", EntryPoint = "UnmapViewOfFile")]
        public static extern bool UnmapViewOfFile(IntPtr lpBaseAddress);

        [DllImport("kernel32.dll", EntryPoint = "CloseHandle")]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("user32.dll", EntryPoint = "GetWindowThreadProcessId")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("kernel32.dll", EntryPoint = "OpenProcess")]
        public static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

        [DllImport("psapi.dll", EntryPoint = "EnumProcessModulesEx")]
        public static extern bool EnumProcessModulesEx(IntPtr hProcess, IntPtr[] lphModule, uint cb, out uint lpcbNeeded, uint dwFilterFlag);

        [DllImport("psapi.dll", EntryPoint = "GetModuleFileNameEx")]
        public static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, StringBuilder lpFilename, uint nSize);

        [DllImport("kernel32.dll", EntryPoint = "GetModuleHandle")]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", EntryPoint = "GetProcAddress")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport("kernel32.dll", EntryPoint = "CreateRemoteThread")]
        public static extern IntPtr CreateRemoteThread(
            IntPtr hProcess,
            IntPtr lpThreadAttributes,
            uint dwStackSize,
            IntPtr lpStartAddress,
            IntPtr lpParameter,
            uint dwCreationFlags,
            ref ulong lpThreadId
        );

        [DllImport("kernel32.dll", EntryPoint = "WaitForSingleObject")]
        public static extern IntPtr WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

        [DllImport("kernel32.dll", EntryPoint = "VirtualAllocEx")]
        public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", EntryPoint = "VirtualFreeEx")]
        public static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint dwFreeType);

        [DllImport("kernel32.dll", EntryPoint = "WriteProcessMemory")]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, IntPtr lpBuffer, uint nSize, out uint lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", EntryPoint = "ReadProcessMemory")]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, IntPtr lpBuffer, uint nSize, out uint lpNumberOfBytesRead);

        [DllImport("kernel32.dll", EntryPoint = "GetModuleFileName")]
        public static extern uint GetModuleFileName(IntPtr hModule, StringBuilder lpFilename, uint nSize);

        [DllImport("user32.dll", EntryPoint = "GetClientRect")]
        public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll", EntryPoint = "ClientToScreen")]
        public static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll", EntryPoint = "SetForegroundWindow")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern void PostQuitMessage(int ExitCode);

        [DllImport("user32.dll")]
        public static extern int DefWindowProc(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "RegisterClassExW", CharSet = CharSet.Unicode)]
        public static extern ushort RegisterClassEx(ref WNDCLASSEX lpwcx);

        [DllImport("user32.dll", EntryPoint = "CreateWindowExW", CharSet = CharSet.Unicode)]
        public static extern IntPtr CreateWindowEx(
            uint dwExStyle,
            IntPtr lpClassName,
            IntPtr lpWindowName,
            uint dwStyle,
            int x,
            int y,
            int nWidth,
            int nHeight,
            IntPtr hWndParent,
            IntPtr hMenu,
            IntPtr hInstance,
            IntPtr lpParam
        );

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrA")]
        public static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int uIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrA")]
        public static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int uIndex);

        [DllImport("kernel32.dll", EntryPoint = "GetPrivateProfileStringA")]
        public static extern uint GetPrivateProfileString(
            string lpAppName,        // セクション名
            string lpKeyName,        // キー名
            string lpDefault,        // 既定の文字列
            StringBuilder lpReturnedString,  // 情報が格納されるバッファ
            uint nSize,              // 情報バッファのサイズ
            string lpFileName        // .ini ファイルの名前
        );

        [DllImport("kernel32.dll", EntryPoint = "GetPrivateProfileSectionNamesA")]
        public static extern uint GetPrivateProfileSectionNames(
            byte[] lpszReturnBuffer,     // 情報が格納されるバッファ
            uint nSize,                             // 情報バッファのサイズ
            string lpFileName                       // .ini ファイルの名前
        );

        [DllImport("kernel32.dll", EntryPoint = "WritePrivateProfileSectionA")]
        public static extern uint WritePrivateProfileSection(
            string lpAppName,  // セクション名
            string lpString,   // 書き込むべきデータ
            string lpFileName  // ファイル名
        );

        [DllImport("kernel32.dll", EntryPoint = "WritePrivateProfileStringA")]
        public static extern uint WritePrivateProfileString(
            string lpAppName,  // セクション名
            string lpKeyName,  // キー名
            string lpString,   // 書き込むべきデータ
            string lpFileName  // ファイル名
        );

        [DllImport("user32.dll", EntryPoint = "GetMenu")]
        public static extern IntPtr GetMenu(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "GetSystemMetrics")]
        public static extern int GetSystemMetrics(int nIndex);

        [DllImport("shell32.dll", EntryPoint = "SHAppBarMessage")]
        public static extern int SHAppBarMessage(uint dwMessage, ref APPBARDATA pData);

        [DllImport("gdi32.dll", EntryPoint = "BitBlt")]
        public static extern int BitBlt(
            IntPtr hdcDest, // コピー先デバイスコンテキストのハンドル
            int nXDest,  // コピー先長方形の左上隅の x 座標
            int nYDest,  // コピー先長方形の左上隅の y 座標
            int nWidth,  // コピー先長方形の幅
            int nHeight, // コピー先長方形の高さ
            IntPtr hdcSrc,  // コピー元デバイスコンテキストのハンドル
            int nXSrc,   // コピー元長方形の左上隅の x 座標
            int nYSrc,   // コピー元長方形の左上隅の y 座標
            uint dwRop  // ラスタオペレーションコード
        );

        [DllImport("user32.dll", EntryPoint = "GetDC")]
        public static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "ReleaseDC")]
        public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("user32.dll", EntryPoint = "SetLayeredWindowAttributes")]
        public static extern int SetLayeredWindowAttributes(IntPtr hWnd, IntPtr crKey, byte bAlpha, uint dwFlags);

        [DllImport("user32.dll", EntryPoint = "FindWindowA")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", EntryPoint = "GetClassNameA")]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("ntdll.dll", EntryPoint = "RtlGetVersion")]
        public static extern uint RtlGetVersion(ref OSVERSIONINFOEX lpVersionInformation);

        [DllImport("advapi32.dll", EntryPoint = "OpenProcessToken")]
        public static extern uint OpenProcessToken(IntPtr ProcessHandle, uint DesireAccess, out IntPtr TokenHandle);

        [DllImport("advapi32.dll", EntryPoint = "LookupPrivilegeValueA")]
        public static extern uint LookupPrivilegeValue(string lpSystemName, string lpName, out LUID lpLuid);

        [DllImport("advapi32.dll", EntryPoint = "AdjustTokenPrivileges")]
        public static extern uint AdjustTokenPrivileges(IntPtr TokenHandle, uint DisableAllPrivileges, ref TOKEN_PRIVILEGES NewState, uint BufferLength, out TOKEN_PRIVILEGES PreviousState, out uint ReturnLength);

        [DllImport("user32.dll", EntryPoint = "keybd_event")]
        public static extern void keybd_event(
          byte bVk,               // 仮想キーコード
          byte bScan,             // ハードウェアスキャンコード
          uint dwFlags,          // 関数のオプション
          UIntPtr dwExtraInfo   // 追加のキーストロークデータ
        );

        [DllImport("user32.dll", EntryPoint = "MapVirtualKey")]
        public static extern uint MapVirtualKey(
          uint uCode,     // 仮想キーコードまたはスキャンコード
          uint uMapType   // 実行したい変換の種類
        );
    }

    static class DwmAPI
    {
        [DllImport("dwmapi.dll")]
        public static extern int DwmRegisterThumbnail(IntPtr hwndDestination, IntPtr hwndSource, ref IntPtr phThumbnailId);
        [DllImport("dwmapi.dll")]
        public static extern int DwmUnregisterThumbnail(IntPtr hThumbnailId);
        [DllImport("dwmapi.dll")]
        public static extern int DwmQueryThumbnailSourceSize(IntPtr hThumbnailId, ref POINT pSize);
        [DllImport("dwmapi.dll")]
        public static extern int DwmUpdateThumbnailProperties(IntPtr hThumbnailId, ref DWM_THUMBNAIL_PROPERTIES ptnProperties);
        [DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(IntPtr hWnd, uint dwAttribute, IntPtr pvAttribute, uint cbSize);
        [DllImport("dwmapi.dll")]
        public static extern int DwmIsCompositionEnabled(out int pfEnabled);
    }

    public static class Constants
    {
        public const uint GW_CHILD = 5;
        public const int GWL_STYLE = -16;
        public const int GW_HWNDNEXT = 2;

        public const int PAGE_READWRITE = 4;

        public const int FILE_MAP_ALL_ACCESS = 7;   // 厳密には7ではない

        public const int PROCESS_ALL_ACCESS = 0x1FFFFF;
        public const int PROCESS_VM_READ = 0x10;
        public const int PROCESS_QUERY_INFORMATION = 0x400;

        public const int MEM_COMMIT = 0x1000;
        public const int MEM_DECOMMIT = 0x4000;

        public const int S_OK = 0;

        public const uint WS_CAPTION = 0x00C00000;
        public const int WS_MINIMIZE = 0x20000000;
        public const uint WS_POPUP = 0x80000000;
        public const int WS_EX_TOPMOST = 8;
        public const int WS_EX_LAYERED = 0x00080000;
        public const int WS_EX_TRANSPARENT = 0x00000020;

        public const int SW_SHOWNORMAL = 1;
        public const int SW_HIDE = 0;

        public const int SWP_NOSIZE = 1;
        public const int SWP_NOMOVE = 2;
        public const int SWP_NOZORDER = 4;
        public const int SWP_NOACTIVATE = 0x10;

        public const int LIST_MODULES_ALL = 3;

        public const int SM_CXSCREEN = 0;
        public const int SM_CYSCREEN = 1;
        public const int SM_CYCAPTION = 4;
        public const int SM_CYMENU = 15;
        public const int SM_CYSIZEFRAME = 33;

        public const int ABM_GETTASKBARPOS = 5;

        public const int WM_USER = 0x400;
        public const int WM_USER_EXIT = 0x401;

        public const int WA_INACTIVE = 0;
        public const int WA_ACTIVE = 1;
        public const int WA_CLICKACTIVE = 2;

        public const uint SRCCOPY = 0x00CC0020;

        public const uint LWA_ALPHA = 2;

        public const uint TOKEN_QUERY = 8;
        public const uint TOKEN_ADJUST_PRIVILEGES = 0x20;

        public const uint SE_PRIVILEGE_ENABLED = 2;
    }

    public delegate int WNDPROC(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int x;
        public int y;
    }



    [StructLayout(LayoutKind.Sequential)]
    public struct DWM_THUMBNAIL_PROPERTIES
    {
        public uint dwFlags;
        public RECT rcDestination;
        public RECT rcSource;
        public byte opacity;
        public int fVisible;
        public int fSourceClientAreaOnly;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WNDCLASSEX
    {
        public uint cbSize;
        public uint style;
        public WNDPROC lpWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public IntPtr hInstance;
        public IntPtr hIcon;
        public IntPtr hCursor;
        public IntPtr hbrBackground;
        public IntPtr lpszMenuName;
        public IntPtr lpszClassName;
        public IntPtr hIconSm;
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct APPBARDATA
    {
        public uint cbSize;
        public IntPtr hWnd;
        public uint uCallbackMessage;
        public uint uEdge;
        public RECT rc;
        public IntPtr lParam;
    }

    [StructLayout(LayoutKind.Sequential)]
    unsafe public struct OSVERSIONINFOEX
    {
        public uint dwOSVersionInfoSize;
        public uint dwMajorVersion;
        public uint dwMinorVersion;
        public uint dwBuildNumber;
        public uint dwPlatformId;
        public fixed char szCSDVersion[128];
        public ushort wServicePackMajor;
        public ushort wServicePackMinor;
        public ushort wSuiteMask;
        public byte wProductType;
        public byte wReserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TOKEN_PRIVILEGES
    {
        public UInt32 PrivilegeCount;
        public LUID_AND_ATTRIBUTES Privileges;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LUID_AND_ATTRIBUTES
    {
        public LUID Luid;
        public uint Attributes;
        
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LUID
    {
        public uint LowPart;
        public uint HighPart;
    }
}
