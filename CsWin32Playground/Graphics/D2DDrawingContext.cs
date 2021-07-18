using Windows.Win32.Graphics.Direct2D;

namespace CsWin32Playground.Graphics
{
    internal class D2DDrawingContext
    {
        public D2DDrawingContext(ID2D1Factory factory, ID2D1RenderTarget renderTarget)
        {
            Factory = factory;
            RenderTarget = renderTarget;
        }

        public ID2D1Factory Factory { get; }
        public ID2D1RenderTarget RenderTarget { get; }
    }
}
