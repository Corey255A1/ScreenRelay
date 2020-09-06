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
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct DISPLAY_DEVICE
    {
        public uint cb;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string DeviceName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceString;
        public uint StateFlags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceKey;
    }



    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Ansi)]
    public struct DEVMODE
    {
        public const int CCHDEVICENAME = 32;
        public const int CCHFORMNAME = 32;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
        [System.Runtime.InteropServices.FieldOffset(0)]
        public string dmDeviceName;
        [System.Runtime.InteropServices.FieldOffset(32)]
        public Int16 dmSpecVersion;
        [System.Runtime.InteropServices.FieldOffset(34)]
        public Int16 dmDriverVersion;
        [System.Runtime.InteropServices.FieldOffset(36)]
        public Int16 dmSize;
        [System.Runtime.InteropServices.FieldOffset(38)]
        public Int16 dmDriverExtra;
        [System.Runtime.InteropServices.FieldOffset(40)]
        public UInt32 dmFields;

        [System.Runtime.InteropServices.FieldOffset(44)]
        Int16 dmOrientation;
        [System.Runtime.InteropServices.FieldOffset(46)]
        Int16 dmPaperSize;
        [System.Runtime.InteropServices.FieldOffset(48)]
        Int16 dmPaperLength;
        [System.Runtime.InteropServices.FieldOffset(50)]
        Int16 dmPaperWidth;
        [System.Runtime.InteropServices.FieldOffset(52)]
        Int16 dmScale;
        [System.Runtime.InteropServices.FieldOffset(54)]
        Int16 dmCopies;
        [System.Runtime.InteropServices.FieldOffset(56)]
        Int16 dmDefaultSource;
        [System.Runtime.InteropServices.FieldOffset(58)]
        Int16 dmPrintQuality;

        [System.Runtime.InteropServices.FieldOffset(44)]
        public POINT dmPosition;
        [System.Runtime.InteropServices.FieldOffset(52)]
        public Int32 dmDisplayOrientation;
        [System.Runtime.InteropServices.FieldOffset(56)]
        public Int32 dmDisplayFixedOutput;

        [System.Runtime.InteropServices.FieldOffset(60)]
        public short dmColor; // See note below!
        [System.Runtime.InteropServices.FieldOffset(62)]
        public short dmDuplex; // See note below!
        [System.Runtime.InteropServices.FieldOffset(64)]
        public short dmYResolution;
        [System.Runtime.InteropServices.FieldOffset(66)]
        public short dmTTOption;
        [System.Runtime.InteropServices.FieldOffset(68)]
        public short dmCollate; // See note below!
        [System.Runtime.InteropServices.FieldOffset(72)]
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHFORMNAME)]
        public string dmFormName;
        [System.Runtime.InteropServices.FieldOffset(102)]
        public Int16 dmLogPixels;
        [System.Runtime.InteropServices.FieldOffset(104)]
        public Int32 dmBitsPerPel;
        [System.Runtime.InteropServices.FieldOffset(108)]
        public Int32 dmPelsWidth;
        [System.Runtime.InteropServices.FieldOffset(112)]
        public Int32 dmPelsHeight;
        [System.Runtime.InteropServices.FieldOffset(116)]
        public Int32 dmDisplayFlags;
        [System.Runtime.InteropServices.FieldOffset(116)]
        public Int32 dmNup;
        [System.Runtime.InteropServices.FieldOffset(120)]
        public Int32 dmDisplayFrequency;
    }






    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct MONITORINFO
    {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public int dwFlags;
        public void init() { cbSize = 40; rcMonitor = new RECT(); rcWork = new RECT(); dwFlags = 0; }
    };


    public delegate bool MONITORENUMPROC(IntPtr hMonitor, IntPtr hdc, ref RECT rect, int lParam);
    
    public class ScreenFinder
    {

        public const int ENUM_CURRENT_SETTINGS = -1;

        [DllImport("user32.dll")]
        public static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MONITORENUMPROC lpfnEnum, int dwData);

        [DllImport("user32.dll")]
        public static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);

        [DllImport("user32.dll")]
        public static extern bool EnumDisplayDevices(string deviceName, int iDevNum, ref DISPLAY_DEVICE dispDevice, uint dwData);

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(ref POINT pt);

        [DllImport("user32.dll")]
        public static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        [DllImport("Shcore.dll")]
        public static extern uint GetDpiForMonitor(IntPtr hMonitor, uint dpiType, ref uint dpiX, ref uint dpiY);




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
            int i = 0;
            DEVMODE dm = new DEVMODE();
            dm.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));
            dm.dmDriverExtra = 0;
            DISPLAY_DEVICE dd = new DISPLAY_DEVICE();
            dd.cb = (uint)Marshal.SizeOf(typeof(DISPLAY_DEVICE));
            try
            {
                //Enumerate the Displays to get the actual Resolution and offset
                while(EnumDisplayDevices(null, i,  ref dd, 0))
                {
                    if (EnumDisplaySettings(dd.DeviceName, ENUM_CURRENT_SETTINGS, ref dm))
                    {
                        Console.WriteLine($"{dd.DeviceName} {dm.dmPelsWidth} {dm.dmPelsHeight}");
                        screens.Add(new Screen(dm.dmPosition.X, dm.dmPosition.Y, dm.dmPelsWidth, dm.dmPelsHeight));
                    }
                    i++;
                }

                //Enumerate the Monitor Rects to find differences in DPI
                //And set the scale
                EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero,
                    (IntPtr hMonitor, IntPtr hdc, ref RECT rect, int lParam) =>
                    {
                        Console.WriteLine($"{rect.left} {rect.top} {rect.right} {rect.bottom}");
                        int l = rect.left;
                        int t = rect.top;
                        var screen = screens.Find(s => s.Left == l && s.Top == t);
                        if (screen != null)
                        {
                            screen.SetScale(rect.right - rect.left, rect.bottom - rect.top);
                        }
                        return true;
                    }
                    , 0);


            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return screens;
        }

        public static MemoryStream GetScreen(Screen s)
        {
            return GetScreen(s.Left, s.Top, s.Right, s.Bottom, s.ScaleX, s.ScaleY);
        }

        public static MemoryStream GetScreen(int left, int top, int right, int bottom, double scaleX = 1.0, double scaleY = 1.0)
        {
            int width = right - left;
            int height = bottom - top;
            var bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            using (var graphics_mem = Graphics.FromImage(bitmap))
            {
                graphics_mem.CopyFromScreen(left, top, 0, 0, new Size(width, height));
                var mouse = GetMousePosition();
                graphics_mem.DrawIcon(new Icon("Cursor.ico"), (int)((mouse.X - left) * scaleX), (int)((mouse.Y - top) * scaleY));
            }
            
            var mem = new MemoryStream();
            bitmap.Save(mem, System.Drawing.Imaging.ImageFormat.Bmp);
            mem.Position = 0;
            return mem;           

        }
    }
}
