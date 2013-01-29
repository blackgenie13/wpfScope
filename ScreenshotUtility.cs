using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;

namespace wpfScope
{
    internal class ScreenshotUtility
    {
        /// <summary>
        /// Shamelessly ripped from:
        /// http://10rem.net/blog/2011/02/08/capturing-screen-images-in-wpf-using-gdi-win32-and-a-little-wpf-interop-help.
        /// </summary>
        public static BitmapSource CaptureRegion(int x, int y, int width, int height)
        {
            BitmapSource bitmap = null;

            IntPtr sourceDC = IntPtr.Zero;
            IntPtr targetDC = IntPtr.Zero;
            IntPtr compatibleBitmapHandle = IntPtr.Zero;

            try
            {
                sourceDC = Win32API.GetDC(Win32API.GetDesktopWindow());
                targetDC = Win32API.CreateCompatibleDC(sourceDC);

                compatibleBitmapHandle = Win32API.CreateCompatibleBitmap(sourceDC, width, height);
                Win32API.SelectObject(targetDC, compatibleBitmapHandle);

                Win32API.BitBlt(targetDC, 0, 0, width, height, sourceDC, x, y, Win32API.GDI32SRCCOPY);
                bitmap = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(compatibleBitmapHandle, IntPtr.Zero,
                    Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                bitmap.Freeze();
            }
            catch (Exception ex)
            {
                // TODO.byip: Handle this later.
            }
            finally
            {
                Win32API.DeleteObject(compatibleBitmapHandle);

                Win32API.ReleaseDC(IntPtr.Zero, sourceDC);
                Win32API.ReleaseDC(IntPtr.Zero, targetDC);
            }

            return bitmap;
        }
    }

    internal class Win32API
    {
        #region gdi32

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool BitBlt(IntPtr hDestDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, Int32 dwRop);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll", ExactSpelling = true, PreserveSig = true, SetLastError = true)]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        public static readonly int GDI32SRCCOPY = 0xCC0020;

        #endregion

        #region user32

        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        public static extern int ReleaseDC(IntPtr hwnd, IntPtr dc);

        #endregion
    }
}
