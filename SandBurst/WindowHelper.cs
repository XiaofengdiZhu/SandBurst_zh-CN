using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Win32;

namespace SandBurst
{
    class WindowHelper
    {
        /// <summary>
        /// メニューの高さを取得する
        /// </summary>
        /// <param name="window"></param>
        /// <returns></returns>
        public static int GetMenuHeight(IntPtr window)
        {
            if (Win32.API.GetMenu(window) == IntPtr.Zero)
                return 0;

            RECT rect, client;

            API.GetWindowRect(window, out rect);
            API.GetClientRect(window, out client);

            int height = (rect.bottom - rect.top) - (client.bottom - client.top);

            height -= API.GetSystemMetrics(Constants.SM_CYCAPTION);
            height -= API.GetSystemMetrics(Constants.SM_CYSIZEFRAME);

            if (height > 0)
            {
                height = API.GetSystemMetrics(Constants.SM_CYMENU);
            }

            return height;
        }

        /// <summary>
        /// 対象ウィンドウと指定した横幅から拡大率を%で取得する
        /// </summary>
        /// <param name="window"></param>
        /// <param name="width"></param>
        /// <returns>成功時 拡大率% : 失敗時 -1</returns>
        public static int GetScaleFromWidth(IntPtr window, int width)
        {
            RECT client;
            API.GetClientRect(window, out client);

            int cx = client.right - client.left;

            if (cx == 0)
                return -1;

            return width * 100 / cx;
        }

        /// <summary>
        /// 対象ウィンドウと指定した比率から拡大率を%で取得する
        /// </summary>
        /// <param name="window"></param>
        /// <param name="ratio"></param>
        /// <returns></returns>
        public static int GetScaleFromRatio(IntPtr window, int ratio, Screen scren)
        {
            RECT recWindow, recClient;
            API.GetClientRect(window, out recClient);
            API.GetWindowRect(window, out recWindow);
            int cy = recClient.bottom - recClient.top;
            int wy = recWindow.bottom - recWindow.top;
            int fy = wy - cy;

            int dispy = scren.Bounds.Height;
            float ratioY = cy / (float)dispy;

            int targetSize = dispy * ratio / 100;
            return (targetSize - fy) * 100 / cy;            
        }

        /// <summary>
        /// 拡大率をディスプレイの大きさ、タスクバーで制限する
        /// </summary>
        /// <param name="window"></param>
        /// <param name="scale"></param>
        /// <param name="limitsDisplay"></param>
        /// <param name="limitsTaskbar"></param>
        /// <returns>成功時 拡大率% : 失敗時 -1</returns>
        public static int ClipScale(IntPtr window, int scale, bool limitsDisplay, bool limitsTaskbar)
        {
            RECT rect, client;
            API.GetWindowRect(window, out rect);
            API.GetClientRect(window, out client);

            int cx = client.right - client.left;
            int cy = client.bottom - client.top;

            if ((cx == 0) || (cy == 0))
                return -1;

            int wx = rect.right - rect.left;
            int wy = rect.bottom - rect.top;
            int dispx = API.GetSystemMetrics(Constants.SM_CXSCREEN);
            int dispy = API.GetSystemMetrics(Constants.SM_CYSCREEN);

            int scaledX = scale * cx / 100 + (wx - cx);
            int scaledY = scale * cy / 100 + (wy - cy);

            if ((scaledY > dispy) && limitsDisplay)
            {
                scale = (dispy - (wy - cy)) * 100 / cy;
            }

            if (limitsTaskbar)
            {
                POINT desktopSize = GetDesktopSize();

                float rX = scaledX / (float)desktopSize.x;
                float rY = scaledY / (float)desktopSize.y;

                if ((rX > 1.0f) || (rY > 1.0f))
                {
                    if (rX < rY)
                        scale = (desktopSize.y - (wy - cy)) * 100 / cy;
                    else
                        scale = (desktopSize.x - (wx - cx)) * 100 / cx;
                }
                    
            }

            return scale;
        }

        /// <summary>
        /// 指定したウィンドウのウィンドウサイズ、クライアントサイズを取得
        /// </summary>
        /// <param name="window"></param>
        /// <param name="windowSize"></param>
        /// <param name="clientSize"></param>
        public static void GetWindowSize(IntPtr window, out POINT windowSize, out POINT clientSize)
        {
            RECT rect;
            Win32.API.GetWindowRect(window, out rect);

            windowSize.x = rect.right - rect.left;
            windowSize.y = rect.bottom - rect.top;

            Win32.API.GetClientRect(window, out rect);
            clientSize.x = rect.right - rect.left;
            clientSize.y = rect.bottom - rect.top;
        }

        /// <summary>
        /// 拡大した後のウィンドウサイズ、クライアントサイズを取得
        /// </summary>
        /// <param name="window"></param>
        /// <param name="scale"></param>
        /// <param name="windowRect"></param>
        /// <param name="clientRect"></param>
        public static void GetScaledWindowRect(IntPtr window, int scale, out RECT windowRect, out RECT clientRect)
        {
            RECT rect;
            Win32.API.GetWindowRect(window, out rect);

            int x = rect.right - rect.left;
            int y = rect.bottom - rect.top;

            Win32.API.GetClientRect(window, out rect);

            int cx = rect.right - rect.left;
            int cy = rect.bottom - rect.top;

            int frameX = x - cx;
            int frameY = y - cy;

            clientRect.left = clientRect.top = 0;
            clientRect.right = cx * scale / 100;
            clientRect.bottom = cy * scale / 100;

            windowRect.left = windowRect.top = 0;
            windowRect.right = clientRect.right + frameX;
            windowRect.bottom = clientRect.bottom + frameX;
        }

        /// <summary>
        /// 現在のディスプレイサイズを取得する
        /// </summary>
        /// <returns></returns>
        public static POINT GetDisplaySize()
        {
            POINT size;
            size.x = API.GetSystemMetrics(Constants.SM_CXSCREEN);
            size.y = API.GetSystemMetrics(Constants.SM_CYSCREEN);

            return size;
        }

        /// <summary>
        /// タスクバーの領域を取得する
        /// </summary>
        /// <returns></returns>
        private static RECT GetTaskbarRect()
        {
            APPBARDATA abd = new APPBARDATA();
            abd.cbSize = (uint)Marshal.SizeOf(typeof(APPBARDATA));

            API.SHAppBarMessage(Constants.ABM_GETTASKBARPOS, ref abd);

            return abd.rc;
        }

        /// <summary>
        /// タスクバーを除いたデスクトップサイズを取得する
        /// </summary>
        /// <returns></returns>
        private static POINT GetDesktopSize()
        {
            int dispx = API.GetSystemMetrics(Constants.SM_CXSCREEN);
            int dispy = API.GetSystemMetrics(Constants.SM_CYSCREEN);

            RECT taskbar = GetTaskbarRect();

            POINT result;
            result.x = dispx;
            result.y = dispy;

            if ((taskbar.left == 0) && (taskbar.top == 0))
            {
                if (taskbar.bottom == dispy)
                {
                    // 左タスクバー
                    result.x -= taskbar.right - taskbar.left;
                }
                else
                {
                    // 上タスクバー
                    result.y -= taskbar.bottom - taskbar.top;
                }
            }
            else
            {
                if (taskbar.left == 0)
                {
                    // 下タスクバー
                    result.y -= taskbar.bottom - taskbar.top;
                }
                else
                {
                    // 右タスクバー
                    result.x -= taskbar.right - taskbar.left;
                }
            }

            return result;
        }


    }
}
