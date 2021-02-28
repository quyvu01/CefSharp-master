// Copyright © 2014 The CefSharp Authors. All rights reserved.
//
// Use of this source code is governed by a BSD-style license that can be found in the LICENSE file.

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
        // chromium does not manage timeouts, so we'll implement one
        private ManualResetEvent manualResetEvent = new ManualResetEvent(false);
        private const string TestUrl = "http://3dviewer.anybim.vn/main?AUTHCODE=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIwOGQ4MzhmMi0xZDdhLTRmYTEtODI2My03ZTAyNGMxNTZjNmUiLCJqdGkiOiIwYjdlOGEwZS05MzUwLTRkYmEtOWZlYy1lMjFmZTJlOTU4YTIiLCJpYXQiOiIyLzI4LzIwMjEgMzowNTozMSBQTSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJxdXkudnUuMDEwMSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6Ik1lbWJlciIsIm5iZiI6MTYxNDUyNDczMSwiZXhwIjoxNjE1MTI5NTMyLCJpc3MiOiJodHRwOi8vM2R2aWV3ZXIuY29uc3RydXhpdi5jb20iLCJhdWQiOiJodHRwOi8vY29uc3RydXhpdi5jb20ifQ.C2XnlxZSVZHya5L4Qsc9Ni9GWEoq0FtuDRJHz9Bvsqk&VIEWID=59a3784c-cf03-4d90-912a-2a4e7cd868b2&URL=3dviewer.anybim.vn%2Fservice%2Fconversion";
        private static Browser Browser;
        public static int Main(string[] args)
        {
            //Console.WriteLine("This example application will load {0}, take a screenshot, and save it to your desktop.", TestUrl);
            //Console.WriteLine("You may see a lot of Chromium debugging output, please wait...");
            //Console.WriteLine();

            //Cef.EnableWaitForBrowsersToClose();

            //// You need to replace this with your own call to Cef.Initialize();
            //CefExample.Init(new CefSettings(), browserProcessHandler: new BrowserProcessHandler());

            //MainAsync("cache\\path1", 1.0);
            ////Demo showing Zoom Level of 3.0
            ////Using seperate request contexts allows the urls from the same domain to have independent zoom levels
            ////otherwise they would be the same - default behaviour of Chromium
            ////MainAsync("cache\\path2", 3.0);

            //// We have to wait for something, otherwise the process will exit too soon.
            //Console.ReadKey();

            ////Wait until the browser has finished closing (which by default happens on a different thread).
            ////Cef.EnableWaitForBrowsersToClose(); must be called before Cef.Initialize to enable this feature
            ////See https://github.com/cefsharp/CefSharp/issues/3047 for details
            //Cef.WaitForBrowsersToClose();

            //// Clean up Chromium objects.  You need to call this in your application otherwise
            //// you will get a crash when closing.
            //Cef.Shutdown();

            ////Success

            Browser = new Browser();
            Browser.OpenUrl(TestUrl);
            //string source = Browser.Page.GetSourceAsync().GetAwaiter().GetResult();
            //File.WriteAllText("test.txt", source);
            //Console.WriteLine(source);
            //Browser.Page.LoadError += (s, e) => Console.WriteLine($"Error text: {e.ErrorText}");
            //Thread.Sleep(30000);
            //var bm = Browser.Page.ScreenshotAsync().GetAwaiter().GetResult();
            //var screenshotPath = "test_img.png";
            //bm.Save(screenshotPath);
            //Process.Start(new ProcessStartInfo(screenshotPath)
            //{
            //    // UseShellExecute is false by default on .NET Core.
            //    UseShellExecute = true
            //});
            Console.ReadKey();

            return 0;

        }

        private static async void MainAsync(string cachePath, double zoomLevel)
        {
            var browserSettings = new BrowserSettings
            {
                //Reduce rendering speed to one frame per second so it's easier to take screen shots
                WindowlessFrameRate = 1
            };
            var requestContextSettings = new RequestContextSettings
            {
                CachePath = Path.GetFullPath(cachePath)
            };

            // RequestContext can be shared between browser instances and allows for custom settings
            // e.g. CachePath
            using var requestContext = new RequestContext(requestContextSettings);
            using var browser = new ChromiumWebBrowser(TestUrl, browserSettings, requestContext);
            if (zoomLevel > 1)
            {
                browser.FrameLoadStart += (s, argsi) =>
                {
                    var b = (ChromiumWebBrowser)s;
                    if (argsi.Frame.IsMain)
                    {
                        b.SetZoomLevel(zoomLevel);
                    }
                };
            }
            browser.LoadingStateChanged += Browser_LoadingStateChanged;
            await LoadPageAsync(browser);

            //Check preferences on the CEF UI Thread
            await Cef.UIThreadTaskFactory.StartNew(delegate
            {
                var preferences = requestContext.GetAllPreferences(true);

                //Check do not track status
                var doNotTrack = (bool)preferences["enable_do_not_track"];

                Debug.WriteLine("DoNotTrack: " + doNotTrack);
            });

            var onUi = Cef.CurrentlyOnThread(CefThreadIds.TID_UI);

            // For Google.com pre-pupulate the search text box
            await browser.EvaluateScriptAsync("document.querySelector('[name=q]').value = 'CefSharp Was Here!'");

            //while (K++ < 10)
            //{
            //    _ = browser.ScreenshotAsync(true).ContinueWith(DisplayBitmap);
            //    Console.WriteLine("Get bitmap");
            //    Thread.Sleep(1000);
            //}
        }

        private static void Browser_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            Console.WriteLine($"State changed: {e.Browser.HasDocument}");
        }

        public static Task LoadPageAsync(IWebBrowser browser, string address = null)
        {
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            void handler(object sender, LoadingStateChangedEventArgs args)
            {
                //Wait for while page to finish loading not just the first frame
                if (!args.IsLoading)
                {
                    browser.LoadingStateChanged -= handler;
                    //Important that the continuation runs async using TaskCreationOptions.RunContinuationsAsynchronously
                    tcs.TrySetResult(true);
                }
            }

            browser.LoadingStateChanged += handler;

            if (!string.IsNullOrEmpty(address))
            {
                browser.Load(address);
            }
            return tcs.Task;
        }

        private static void DisplayBitmap(Task<Bitmap> task)
        {
            var screenshotPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "TestCapture", "CefSharp screenshot" + DateTime.Now.Ticks + ".png");

            Console.WriteLine();
            Console.WriteLine("Screenshot ready. Saving to {0}", screenshotPath);

            var bitmap = task.Result;

            // Save the Bitmap to the path.
            // The image type is auto-detected via the ".png" extension.
            bitmap.Save(screenshotPath);

            // We no longer need the Bitmap.
            // Dispose it to avoid keeping the memory alive.  Especially important in 32-bit applications.
            bitmap.Dispose();

            Console.WriteLine("Screenshot saved. Launching your default image viewer...");

            // Tell Windows to launch the saved image.
            Process.Start(new ProcessStartInfo(screenshotPath)
            {
                // UseShellExecute is false by default on .NET Core.
                UseShellExecute = true
            });

        }
    }
}
