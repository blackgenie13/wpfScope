using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace wpfScope
{
    /// <summary>
    /// Creates screenshots on the desktop.
    /// </summary>
    public class ScreenshotUtility
    {
        /// <summary>
        /// Creates a screenshot of the desktop given an origin (x, y) and size width x height.
        /// 
        /// Ripped from: http://pastie.org/pastes/1546855/text
        /// </summary>
        /// <param name="x">The x of the origin.</param>
        /// <param name="y">The y of the origin.</param>
        /// <param name="width">The width of the screenshot.</param>
        /// <param name="height">The height of the screenshot.</param>
        /// <returns></returns>
        public static Bitmap ScreenshotRegion(int x, int y, int width, int height)
        {
            Bitmap bmp = null;

            try
            {
                if (width > 0 && height > 0)
                {
                    bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
                    using (Graphics gfx = Graphics.FromImage(bmp))
                    {
                        gfx.CopyFromScreen(x, y, 0, 0, new Size(width, height), CopyPixelOperation.SourceCopy);
                    }
                }
            }
            catch (Exception e)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine(e);
#endif
            }

            return bmp;
        }
    }
}
