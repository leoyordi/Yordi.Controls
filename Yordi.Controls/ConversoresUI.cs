namespace Yordi.Controls
{
    public static class ConversoresUI
    {
        public static byte[]? ToByteArray(Image? imageIn)
        {
            //if (imageIn == null)
            //    return null;
            return (byte[]?)new ImageConverter().ConvertTo(imageIn, typeof(byte[]));
        }

        public static Image? ToImage(byte[] byteArrayIn)
        {
            if (byteArrayIn == null) return null;
            Image? returnImage = null;
            try
            {
                MemoryStream ms = new MemoryStream(byteArrayIn);
                returnImage = Image.FromStream(ms);
            }
            catch { }
            return returnImage;
        }
        public static Rectangle DeflateRect(Rectangle rect, Padding padding)
        {
            rect.X += padding.Left;
            rect.Y += padding.Top;
            rect.Width -= padding.Horizontal;
            rect.Height -= padding.Vertical;
            return rect;
        }

    }
}
