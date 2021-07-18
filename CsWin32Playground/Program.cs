[assembly:System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]

namespace CsWin32Playground
{
    // https://docs.microsoft.com/en-us/windows/win32/learnwin32/creating-a-window
    // https://docs.microsoft.com/en-us/windows/win32/learnwin32/window-messages
    // https://www.gitmemory.com/issue/microsoft/CsWin32/244/822066737
    class Program
    {
        static unsafe void Main(string[] args)
        {
            HwndHost host = new HwndHost();
            host.Title = "CsWin32 Playground";
            host.Show();
        }
    } 
}
