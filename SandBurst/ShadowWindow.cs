using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Timers;
using Win32;

namespace SandBurst
{
    public delegate void ErrorDelegate();

    public class ShadowWindow
    {
        private bool visible;
        private Point position;
        private Point size;
        private Timer timer;
        private IntPtr targetWindow;
        private WNDPROC proc;
        private ErrorDelegate errorDelegate;

        WNDCLASSEX wc;

        /// <summary>
        /// ShadowWindowのウィンドウハンドル
        /// </summary>
        public IntPtr Window { get; set; }        

        public bool Visible
        {
            set
            {
                if (value)
                    Win32.API.ShowWindow(Window, Win32.Constants.SW_SHOWNORMAL);
                else
                    Win32.API.ShowWindow(Window, Win32.Constants.SW_HIDE);

                visible = value;
            }
            get
            {

                return visible;
            }
        }

        public bool Topmost
        {
            set
            {
                uint flag = Win32.Constants.SWP_NOSIZE | Win32.Constants.SWP_NOMOVE;
                if (value)
                {
                    Win32.API.SetWindowPos(Window, (IntPtr)(-1), 0, 0, 0, 0, flag);
                    Win32.API.SetWindowPos(Window, (IntPtr)(-1), 0, 0, 0, 0, flag);
                }
                else
                {
                    IntPtr activeWindow = Win32.API.GetForegroundWindow();
                    Win32.API.SetWindowPos(activeWindow, (IntPtr)(-2), 0, 0, 0, 0, flag);
                    Win32.API.SetWindowPos(Window, (IntPtr)(-2), 0, 0, 0, 0, flag);

                    StringBuilder title = new StringBuilder(256);
                    Win32.API.GetWindowText(activeWindow, title, 256);

                    if (title.ToString() == "Program Manager")
                        Win32.API.SetWindowPos(targetWindow, (IntPtr)(-2), 0, 0, 0, 0, flag);
                    else
                        Win32.API.SetWindowPos(Window, activeWindow, 0, 0, 0, 0, flag);
                }
            }
        }

        public ShadowWindow()
        {
            Window = CreateWindow();
            proc = WindProc;
            timer = new Timer(50);
            timer.Elapsed += this.OnTimedEvent;
        }

        /// <summary>
        /// ShadowWindowをtargetWindowに追従させる
        /// </summary>
        /// <param name="targetWindow"></param>
        /// <param name="errorDelegate"></param>
        public void StartFollow(IntPtr targetWindow, ErrorDelegate errorDelegate)
        {
            this.targetWindow = targetWindow;
            this.errorDelegate = errorDelegate;
            timer.Start();
        }

        /// <summary>
        /// ShadowWindowの追従を停止する
        /// </summary>
        public void StopFollow()
        {
            timer.Stop();
        }

        /// <summary>
        /// ShadowWindowの位置を変更する
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void SetPos(int x, int y)
        {
            position.X = x;
            position.Y = y;

            Win32.API.SetWindowPos(Window, IntPtr.Zero, x, y, 0, 0, Constants.SWP_NOSIZE | Constants.SWP_NOZORDER);
        }

        /// <summary>
        /// ShadowWindowの大きさを変更する
        /// </summary>
        /// <param name="sx"></param>
        /// <param name="sy"></param>
        public void SetSize(int sx, int sy)
        {
            size.X = sx;
            size.Y = sy;

            Win32.API.SetWindowPos(Window, IntPtr.Zero, 0, 0, sx, sy, Constants.SWP_NOMOVE);
        }

        /// <summary>
        /// ShadowWindowを作成する
        /// </summary>
        /// <returns></returns>
        private IntPtr CreateWindow()
        {
            char[] c = ("SundBurstShadow\0\0").ToCharArray();
            IntPtr p = Marshal.AllocHGlobal(c.Length * Marshal.SystemDefaultCharSize);
            Marshal.Copy(c, 0, p, c.Length);

            wc.cbSize = (uint)(16 + 8 * Marshal.SizeOf(p));
            wc.style = 0;
            wc.lpWndProc = WindProc;
            wc.cbClsExtra = 0;
            wc.cbWndExtra = 0;
            wc.hInstance = Win32.API.GetModuleHandle(null);
            wc.hIcon = IntPtr.Zero;
            wc.hCursor = IntPtr.Zero;
            wc.hbrBackground = IntPtr.Zero;
            wc.lpszMenuName = IntPtr.Zero;
            wc.lpszClassName = p;
            wc.hIconSm = IntPtr.Zero;

            size.X = 1;
            size.Y = 1;


            Win32.API.RegisterClassEx(ref wc);

            uint exStyle = Win32.Constants.WS_EX_TOPMOST | Win32.Constants.WS_EX_LAYERED | Win32.Constants.WS_EX_TRANSPARENT;
            IntPtr result = Win32.API.CreateWindowEx(exStyle, p, p, Win32.Constants.WS_POPUP, 0, 0, 1, 1, IntPtr.Zero, IntPtr.Zero, Win32.API.GetModuleHandle(null), IntPtr.Zero);

            Win32.API.SetLayeredWindowAttributes(result, IntPtr.Zero, 255, Win32.Constants.LWA_ALPHA);
            Marshal.FreeHGlobal(p);

            return result;
        }

        /// <summary>
        /// 追従ようタイマーイベント
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            Win32.RECT rect;
            if (!Win32.API.GetClientRect(targetWindow, out rect))
                errorDelegate();

            Win32.POINT pos;
            pos.x = rect.left;
            pos.y = rect.top;
            Win32.API.ClientToScreen(targetWindow, ref pos);

            SetPos(pos.x, pos.y);
        }

        private int WindProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam)
        {
            switch (uMsg)
            {
                // WM_DESTROY
                case 2:
                    Win32.API.PostQuitMessage(0);
                    break;
            }

            return Win32.API.DefWindowProc(hWnd, uMsg, wParam, lParam);
        }

    }
}