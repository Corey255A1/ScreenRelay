//WunderVision 2020
//https://www.wundervisionenvisionthefuture.com/
//Functions to wrap around the user32 functions that discover the connected Monitors
//Also functions to the screen image into Memory for conversion to a Bitmap


using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Drawing;
using System.IO;

namespace ScreenRelay
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct POINT
    {
        public int X;
        public int Y;
    };


    public delegate bool MONITORENUMPROC(IntPtr hMonitor, IntPtr hdc, ref RECT rect, int lParam);
    
    public class ScreenFinder
    {
        [DllImport("user32.dll")]
        public static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MONITORENUMPROC lpfnEnum, int dwData);

        [DllImport("user32.dll")]
        internal static extern bool GetCursorPos(ref POINT pt);
        public static Point GetMousePosition()
        {
            POINT mousPt = new POINT();
            GetCursorPos(ref mousPt);
            return new Point(mousPt.X, mousPt.Y);
        }
        public static void LogMonitors()
        {
            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero,
                (IntPtr hMonitor, IntPtr hdc, ref RECT rect, int lParam) =>
                {
                    Console.WriteLine($"{rect.left} {rect.top} {rect.right} {rect.bottom}");
                    return true;
                }
                , 0);
        }

        public static List<Screen> GetMonitors()
        {
            var screens = new List<Screen>();
            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero,
                (IntPtr hMonitor, IntPtr hdc, ref RECT rect, int lParam) =>
                {

                    screens.Add(new Screen(rect));
                    Console.WriteLine($"{rect.left} {rect.top} {rect.right} {rect.bottom}");
                    return true;
                }
                , 0);

            return screens;
        }

        public static MemoryStream GetScreen(Screen s)
        {
            return GetScreen(s.Left, s.Top, s.Right, s.Bottom);
        }

        public static MemoryStream GetScreen(int left, int top, int right, int bottom)
        {
            int width = right - left;
            int height = bottom - top;
            var bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            using (var graphics_mem = Graphics.FromImage(bitmap))
            {
                graphics_mem.CopyFromScreen(left, top, 0, 0, new Size(width, height));
                var mouse = GetMousePosition();                
                graphics_mem.DrawIcon(new Icon("Cursor.ico"), mouse.X-left, mouse.Y-top);
            }
            
            var mem = new MemoryStream();
            bitmap.Save(mem, System.Drawing.Imaging.ImageFormat.Bmp);
            mem.Position = 0;
            return mem;           

        }
    }
}
