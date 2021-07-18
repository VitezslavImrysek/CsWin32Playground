using CsWin32Playground.Graphics;
using CsWin32Playground.Helpers;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Direct2D;
using Windows.Win32.UI.WindowsAndMessaging;

namespace CsWin32Playground
{
    public class HwndHost
    {
        private D2DRenderer _renderer;

        public string Title { get; set; }

        public void Show()
        {
            var hwnd = CreateHwnd();

            _renderer = new D2DRenderer(hwnd);
            _renderer.Paint += OnDirect2DPaint;

            PInvoke.ShowWindow(hwnd, SHOW_WINDOW_CMD.SW_NORMAL);

            MSG msg;
            while (PInvoke.GetMessage(out msg, new HWND(), 0, 0))
            {
                PInvoke.TranslateMessage(msg);
                PInvoke.DispatchMessage(msg);
            }

            _renderer.Dispose();
            _renderer = null;
        }

        private LRESULT WndProc(HWND hwnd, uint msg, WPARAM wParam, LPARAM lParam)
        {
            switch (msg)
            {
                case Constants.WM_PAINT:
                    OnPaint(hwnd);
                    PInvoke.ValidateRect(hwnd, (RECT?)null);
                    return new LRESULT(0);
                case Constants.WM_CREATE:
                    break;
                case Constants.WM_SIZE:
                    {
                        int width = GetLowWord(lParam.Value);   // Get the low-order word.
                        int height = GetHighWord(lParam.Value); // Get the high-order word.

                        // Respond to the message:
                        OnSize(hwnd, wParam, width, height);
                    }
                    break;
                // Closing Window: https://docs.microsoft.com/en-us/windows/win32/learnwin32/closing-the-window
                case Constants.WM_CLOSE:
                    PInvoke.DestroyWindow(hwnd);
                    return new LRESULT(0);
                case Constants.WM_DESTROY:
                    PInvoke.PostQuitMessage(0);
                    return new LRESULT(0);
                default:
                    break;
            }

            return PInvoke.DefWindowProc(hwnd, msg, wParam, lParam);
        }

        private void OnSize(HWND hwnd, WPARAM wParam, int width, int height)
        {

        }

        private void OnPaint(HWND hwnd)
        {
            _renderer.Render();
        }

        private void OnDirect2DPaint(object sender, D2DDrawingContext drawingContext)
        {
            var renderTarget = drawingContext.RenderTarget;

            ID2D1SolidColorBrush brush;
            renderTarget.CreateSolidColorBrush(Color.Red.ToD2Color(), null, out brush);

            renderTarget.FillRectangle(new D2D_RECT_F() { left = 100, top = 100, bottom = 200, right = 200 }, brush);

            // Optionally let runtime know that this object can be cleared.
            Marshal.ReleaseComObject(brush);
        }

        private unsafe HWND CreateHwnd()
        {
            string className = "Sample Window Class";
            IntPtr hInstance = Process.GetCurrentProcess().Handle;

            ushort classId;

            fixed (char* classNamePtr = className)
            {
                WNDCLASSW wc = new WNDCLASSW();
                wc.lpfnWndProc = WndProc;
                wc.lpszClassName = classNamePtr;
                wc.hInstance = (HINSTANCE)hInstance;

                classId = PInvoke.RegisterClass(wc);

                if (classId == 0)
                {
                    throw new Exception("class not registered");
                }
            }

            HWND hwnd = PInvoke.CreateWindowEx(
                0,
                className,
                Title,
                WINDOW_STYLE.WS_OVERLAPPEDWINDOW,
                Constants.CW_USEDEFAULT, Constants.CW_USEDEFAULT, Constants.CW_USEDEFAULT, Constants.CW_USEDEFAULT,
                new HWND(),
                null,
                new CreateWindowHandle(hInstance, false),
                null);

            if (hwnd.Value == 0)
            {
                throw new Exception("hwnd not created");
            }

            return hwnd;
        }

        private static int GetLowWord(nint value)
        {
            uint xy = (uint)value;
            int x = unchecked((short)xy);
            int y = unchecked((short)(xy >> 16));

            return x;
        }

        private static int GetHighWord(nint value)
        {
            uint xy = (uint)value;
            int x = unchecked((short)xy);
            int y = unchecked((short)(xy >> 16));

            return y;
        }

        public class CreateWindowHandle : SafeHandle
        {
            public CreateWindowHandle(IntPtr invalidHandleValue, bool ownsHandle)
                : base(invalidHandleValue, ownsHandle)
            {
            }

            public override bool IsInvalid => false;

            protected override bool ReleaseHandle()
            {
                return true;
            }
        }
    }
}
