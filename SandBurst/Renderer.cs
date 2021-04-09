using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SandBurst
{
    class Renderer
    {
        private IntPtr destWindow;
        private IntPtr sourceWindow;
        private Win32.RECT destRect;
        private Win32.RECT sourceRect;
        private D3DManager d3dManager;
        private IntPtr thumbnail;
        private RenderingMode mode;
        private Thread renderThread;
        private ManualResetEvent renderEvent;
        private bool running;

        public Renderer()
        {
            d3dManager = new D3DManager();
            renderEvent = new ManualResetEvent(false);
        }

        public bool Start(RenderingMode mode, IntPtr sourceWindow, IntPtr destWindow, Win32.RECT sourceRect, Win32.RECT destRect, D3DFilter filter, float menuHeight)
        {
            this.mode = mode;

            switch (mode)
            {
                case RenderingMode.Dwm:
                    return InitDwm(sourceWindow, destWindow, sourceRect, destRect);
                case RenderingMode.D3D:
                    return InitD3D(sourceWindow, destWindow, sourceRect, destRect, filter, menuHeight);
            }            

            return false;
        }

        public void Stop()
        {
            running = false;

            switch (mode)
            {
                case RenderingMode.Dwm:
                    Win32.DwmAPI.DwmUnregisterThumbnail(thumbnail);
                    thumbnail = IntPtr.Zero;
                    break;
                case RenderingMode.D3D:
                    renderEvent.WaitOne();
                    d3dManager.Release();
                    break;
            }

            
        }

        public enum RenderingMode
        {
            Dwm, D3D
        }

        private bool InitDwm(IntPtr sourceWindow, IntPtr destWindow, Win32.RECT sourceRect, Win32.RECT destRect)
        {
            Win32.DWM_THUMBNAIL_PROPERTIES props;

            props.dwFlags = 0x1F;
            props.rcDestination = destRect;
            props.rcSource = sourceRect;
            props.opacity = 255;
            props.fVisible = 1;
            props.fSourceClientAreaOnly = 1;

            if (thumbnail != IntPtr.Zero)
                Win32.DwmAPI.DwmUnregisterThumbnail(thumbnail);

            if (Win32.DwmAPI.DwmRegisterThumbnail(destWindow, sourceWindow, ref thumbnail) != Win32.Constants.S_OK)
            {
                ErrorHelper.ShowErrorMessage("サムネイルの取得に失敗しました");

                return false;
            }

            if (Win32.DwmAPI.DwmUpdateThumbnailProperties(thumbnail, ref props) != Win32.Constants.S_OK)
            {
                ErrorHelper.ShowErrorMessage("サムネイルの更新に失敗しました");
                Stop();

                return false;
            }

            this.sourceRect = sourceRect;
            this.destRect = destRect;
            this.sourceWindow = sourceWindow;
            this.destWindow = destWindow;

            running = true;

            return true;
        }

        private bool InitD3D(IntPtr sourceWindow, IntPtr destWindow, Win32.RECT sourceRect, Win32.RECT destRect, D3DFilter filter, float menuHeight)
        {
            float scale = destRect.right / (float)sourceRect.right;

            if (!d3dManager.Init(sourceWindow, sourceRect.right, sourceRect.bottom, destWindow, destRect.right, destRect.bottom, scale, filter, menuHeight))
            {
                ErrorHelper.ShowErrorMessage("Direct3Dの初期化に失敗しました");
                return false;
            }
            

            this.sourceRect = sourceRect;
            this.destRect = destRect;
            this.sourceWindow = sourceWindow;
            this.destWindow = destWindow;

            renderEvent = new ManualResetEvent(false);

            renderThread = new Thread(RenderProc);
            renderThread.Start();
            
            running = true;

            return true;
        }

        private void RenderProc()
        {
            while (running)
            {
                d3dManager.Draw();
            }

            renderEvent.Set();
        }
    }
}
