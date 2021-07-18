using CsWin32Playground.Helpers;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Direct2D;

namespace CsWin32Playground.Graphics
{
    internal class D2DRenderer : IDisposable
    {
        // https://docs.microsoft.com/en-us/windows/win32/direct2d/direct2d-quickstart

        private readonly HWND _hwnd;

        private ID2D1Factory _d2D1Factory;
        private ID2D1HwndRenderTarget _renderTarget;
        private D2DDrawingContext _drawingContext;
        private RECT? _renderTargetSize;

        public D2DRenderer(HWND hwnd)
        {
            _hwnd = hwnd;
        }

        public event EventHandler<D2DDrawingContext> Initialized;
        public event EventHandler<D2DDrawingContext> Paint;

        public void Dispose()
        {
            Marshal.ReleaseComObject(_renderTarget);
            Marshal.ReleaseComObject(_d2D1Factory);
        }

        public void Render()
        {
            CreateResources();
            RenderImpl();
        }

        private unsafe void CreateResources()
        {
            var hwnd = _hwnd;

            RECT clientRect;
            PInvoke.GetClientRect(hwnd, out clientRect);

            if (_d2D1Factory == null)
            {
                _d2D1Factory = CreateD2D1Factory();
                _renderTarget = CreateHwndRenderTarget(_d2D1Factory, hwnd, clientRect);
                _renderTargetSize = clientRect;

                _drawingContext = new D2DDrawingContext(_d2D1Factory, _renderTarget);

                Initialized?.Invoke(this, _drawingContext);
            }
            else
            {
                ID2D1HwndRenderTarget renderTarget = _renderTarget;

                // TODO: Why does this throw an exception?
                //var size = renderTarget.GetSize();
                var clientWidth = clientRect.right - clientRect.left;
                var clientHeight = clientRect.bottom - clientRect.top;
                var renderTargetWidth = _renderTargetSize.Value.right - _renderTargetSize.Value.left;
                var renderTargetHeight = _renderTargetSize.Value.bottom - _renderTargetSize.Value.top;
                if (clientWidth != renderTargetWidth || clientHeight != renderTargetHeight)
                {
                    renderTarget.Resize(new D2D_SIZE_U() { width = (uint)clientWidth, height = (uint)clientHeight });

                    _renderTargetSize = clientRect;
                }
            }
        }

        private unsafe void RenderImpl()
        {
            D2D1_COLOR_F clearColor = Color.Blue.ToD2Color();
            
            D2D_MATRIX_3X2_F matrix = new D2D_MATRIX_3X2_F();
            matrix.Anonymous.m._0 = 1;
            matrix.Anonymous.m._3 = 1;

            var renderTarget = _renderTarget;

            renderTarget.BeginDraw();
            renderTarget.Clear(clearColor);
            renderTarget.SetTransform(matrix);

            Paint?.Invoke(this, _drawingContext);

            renderTarget.EndDraw();
        }

        private unsafe ID2D1Factory CreateD2D1Factory()
        {
            void* ptr;

            HRESULT result = PInvoke.D2D1CreateFactory(D2D1_FACTORY_TYPE.D2D1_FACTORY_TYPE_SINGLE_THREADED, Guid.Parse("06152247-6F50-465A-9245-118BFD3B6007"), null, out ptr);
            if (result.Failed)
            {
                throw new Exception(nameof(CreateD2D1Factory));
            }

            return Marshal.GetObjectForIUnknown((IntPtr)ptr) as ID2D1Factory;
        }

        private unsafe ID2D1HwndRenderTarget CreateHwndRenderTarget(ID2D1Factory d2d1Factory, HWND hwnd, RECT clientRect)
        {
            D2D_SIZE_U size = new D2D_SIZE_U()
            {
                width = (uint)(clientRect.right - clientRect.left),
                height = (uint)(clientRect.bottom - clientRect.top)
            };

            ID2D1HwndRenderTarget renderTarget;
            d2d1Factory.CreateHwndRenderTarget(
                new D2D1_RENDER_TARGET_PROPERTIES(),
                new D2D1_HWND_RENDER_TARGET_PROPERTIES() { hwnd = hwnd, pixelSize = size },
                out renderTarget);

            return renderTarget;
        }
    }
}
