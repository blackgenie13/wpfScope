using System.Drawing;
using System.Drawing.Imaging;

namespace wpfScope
{
    internal class ScreenshotUtility
    {
        /// <summary>
        /// http://pastie.org/pastes/1546855/text
        /// </summary>
        public static Bitmap ScreenshotRegion(int x, int y, int width, int height)
        {
            Bitmap bmp = null;

            if (width > 0 && height > 0)
            {
                bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
                using (Graphics gfx = Graphics.FromImage(bmp))
                {
                    gfx.CopyFromScreen(x, y, 0, 0, new Size(width, height), CopyPixelOperation.SourceCopy);
                }
            }

            return bmp;
        }
    }
}
