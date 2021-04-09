using System;
using SlimDX.Direct3D9;
using System.Drawing;
using System.Runtime.InteropServices;

namespace SandBurst
{
    public enum D3DFilter
    {
        None = 0,
        Linear = 1,
        Cubic = 2
    };

    class D3DManager
    {
        [DllImport("WinHelper.dll", EntryPoint = "CreateEffect", CharSet = CharSet.Unicode )]
        public extern static IntPtr CreateEffect(IntPtr device, string fileName);

        [DllImport("WinHelper.dll", EntryPoint = "ReleaseEffect")]
        public extern static IntPtr ReleaseEffect(IntPtr effect);

        [DllImport("WinHelper.dll", EntryPoint = "CreateVertexBuffer")]
        public extern static IntPtr CreateVertexBuffer(IntPtr effect, float scale, float texSize, float clientWidth, float clientHeight, float menuHeight);

        [DllImport("WinHelper.dll", EntryPoint = "DrawEffectVertex")]
        public extern static IntPtr DrawEffectVertex(IntPtr effect, IntPtr texture, float scale, float texSize);

        private const int S_OK = Win32.Constants.S_OK;

        // D3Dオブジェクト
        private Direct3D d3d;
        private Device device;
        private Texture texture;
        private Surface surface;
        private Sprite sprite;
        private IntPtr effect = IntPtr.Zero;

        // ウィンドウサイズ
        private int sourceWidth;
        private int sourceHeight;
        private int destWidth;
        private int destHeight;
        private float scale;
        private int texSize;
        private D3DFilter filter;
        private float menuHeight;

        // 拡大元ウィンドウのHDC
        private IntPtr sourceDC;

        private IntPtr sourceWindow;

        ~D3DManager()
        {
            Release();
        }
        
        public bool Init(IntPtr sourceWindow, int sourceWidth, int sourceHeight, IntPtr destWindow, int destWidth, int destHeight, float scale, D3DFilter filter, float menuHeight)
        {
            PresentParameters param = new PresentParameters()
            {
                Windowed = true,
                SwapEffect = SwapEffect.Discard,
                BackBufferFormat = Format.Unknown,
                BackBufferWidth = destWidth,
                BackBufferHeight = destHeight,
                EnableAutoDepthStencil = false,
                DeviceWindowHandle = destWindow,
                PresentationInterval = PresentInterval.One
            };
            
            try
            {
                d3d = new Direct3D();
                device = new Device(d3d, 0, DeviceType.Hardware, destWindow, CreateFlags.HardwareVertexProcessing, param);

                texSize = GetTextureSize(sourceWidth, sourceHeight);
                texture = new Texture(device, texSize, texSize, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default);

                surface = Surface.CreateOffscreenPlain(device, sourceWidth, sourceHeight, Format.A8R8G8B8, Pool.SystemMemory);
                
                sprite = new Sprite(device);

                if (filter == D3DFilter.Cubic)
                {
                    effect = CreateEffect(device.ComPointer, "Bicubic.fx");
                    CreateVertexBuffer(effect, scale, texSize, sourceWidth, sourceHeight, menuHeight);
                }

                this.sourceWidth = sourceWidth;
                this.sourceHeight = sourceHeight;
                this.destWidth = destWidth;
                this.destHeight = destHeight;
                this.sourceDC = Win32.API.GetDC(sourceWindow);
                this.sourceWindow = sourceWindow;
                this.scale = scale;
                this.filter = filter;
                this.menuHeight = menuHeight;

                return true;
            }
            catch(Exception e)
            {
                Release();
                return false;
            }
        }

        public void Release()
        {
            if (effect != IntPtr.Zero) ReleaseEffect(effect);
            if (sprite != null) sprite.Dispose();
            if (texture != null) texture.Dispose();
            if (surface != null) surface.Dispose();
            if (device != null) device.Dispose();
            if (d3d != null) d3d.Dispose();
            if (sourceDC != IntPtr.Zero) Win32.API.ReleaseDC(sourceWindow, sourceDC);

            effect = IntPtr.Zero;
            sourceDC = IntPtr.Zero;
            sprite = null;
            texture = null;
            surface = null;
            device = null;
            d3d = null;
        }

        public void Draw()
        {
            if ((filter == D3DFilter.None) || (filter == D3DFilter.Linear))
            {
                DrawBasic();
            }
            else
            {
                DrawCubic();
            }
            
        }

        private void DrawBasic()
        {
            if (device.Clear(ClearFlags.Target, Color.Blue, 1.0f, 0).Code != S_OK)
                return;

            if (device.BeginScene().Code != S_OK)
                return;

            CaptureWindowImage();
            
            sprite.Begin(SpriteFlags.DoNotAddRefTexture);
           
            if (filter == D3DFilter.None)
            {
                device.SetSamplerState(0, SamplerState.MinFilter, TextureFilter.Point);
                device.SetSamplerState(0, SamplerState.MagFilter, TextureFilter.Point);
            }

            DrawRect(0, 0, sourceWidth, sourceHeight, scale);

            sprite.End();

            device.EndScene();
            device.Present();
        }

        private void DrawCubic()
        {
            if (device.Clear(ClearFlags.Target, Color.Blue, 1.0f, 0).Code != S_OK)
                return;

            if (device.BeginScene().Code != S_OK)
                return;

            CaptureWindowImage();
            
            sprite.Begin(SpriteFlags.DoNotAddRefTexture);

            const float size = 16.0f;

            DrawRect(0, 0, sourceWidth, size, scale);
            DrawRect(0, 0, size, sourceHeight, scale);
            DrawRect(0, sourceHeight - size - menuHeight, sourceWidth, size, scale);
            DrawRect(sourceWidth - size, 0, size, sourceHeight, scale);
            sprite.End();
            
            DrawEffectVertex(effect, texture.ComPointer, scale, texSize);
            device.EndScene();
            device.Present();
        }

        
        private void DrawRect(float x, float y, float w, float h, float scale)
        {
            SlimDX.Matrix transform = SlimDX.Matrix.Scaling(scale, scale, 1);
            sprite.Transform = transform;

            Rectangle rectangle = new Rectangle
            {
                X = (int)x,
                Y = (int)y,
                Width = (int)w,
                Height = (int)h
            };

            SlimDX.Vector3 pos;
            pos.X = x;
            pos.Y = y;
            pos.Z = 0;

            SlimDX.Vector3 center;
            center.X = 0;
            center.Y = 0;
            center.Z = 0;

            sprite.Draw(texture, rectangle, center, pos, Color.White);
        }

        private void CaptureWindowImage()
        {
            // ますSurfaceにBitbltする

            IntPtr hdc = surface.GetDC();

            Win32.API.BitBlt(hdc, 0, 0, sourceWidth, sourceWidth, sourceDC, 0, 0, Win32.Constants.SRCCOPY);

            surface.ReleaseDC(hdc);


            // SurfaceからTextureに書き込む

            Surface dest = texture.GetSurfaceLevel(0);

            device.UpdateSurface(surface, dest);

            dest.Dispose();
        }

        private int GetTextureSize(int width, int height)
        {
            int max = width > height ? width : height;

            for (int i = 1; i <= 4096; i *= 2 )
            {
                if (i >= max)
                    return i;
            }

            return 0;
        }
    }
}
