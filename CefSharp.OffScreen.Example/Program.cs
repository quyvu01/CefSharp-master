using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CefSharp.Example;
using CefSharp.Example.Handlers;

namespace CefSharp.OffScreen.Example
{
    public class Program
    {
        private const string TestUrl = "http://localhost:4200/main?AUTHCODE=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIwOGQ4MzhmMi0xZDdhLTRmYTEtODI2My03ZTAyNGMxNTZjNmUiLCJqdGkiOiIyZmEwNzBkZC0yMjJmLTQ3ZTYtYmE3Ni1iYWY3NTZmOWMxNDMiLCJpYXQiOiIyLzI0LzIwMjEgMjoyNDowOSBBTSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJxdXkudnUuMDEwMSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6Ik1lbWJlciIsIm5iZiI6MTYxNDEzMzQ0OSwiZXhwIjoxNjE0NzM4MjUwLCJpc3MiOiJodHRwOi8vM2R2aWV3ZXIuY29uc3RydXhpdi5jb20iLCJhdWQiOiJodHRwOi8vY29uc3RydXhpdi5jb20ifQ.yyaH_-BP7a29jMX-Jw_kpB2kmDSm7b3Bje58i9ZeUNo&VIEWID=062417cb-0e2c-4a9c-a73d-0b8a9bfc39ac&URL=3dviewer.anybim.vn%2Fservice%2Fconversion";
        private static Browser Browser;
        public static int Main(string[] args)
        {
            Browser = new Browser();
            Browser.Page.ConsoleMessage += (s, e) => Console.WriteLine($"Console message is: {e.Message}");
            Browser.OpenUrl(TestUrl);
            Task.Run(async () =>
            {
                Thread.Sleep(40000);
                var bm = await Browser.Page.ScreenshotAsync();
                bm.Save("test_img.png");
                Console.WriteLine("Saved image");
            });
            Console.ReadKey();

            return 0;

        }
    }
}
