using System.Drawing.Drawing2D;

namespace PrintInventory.Forms;

// Loads pictures for display.
// Image.FromFile keeps the file locked until the image is disposed, so we could not
// replace or delete it. Here we read the file, make a copy in memory and close the file.
public static class ImageLoader
{
    // Returns null if there is no path, the file is missing, or it is not a valid image.
    public static Image? LoadImage(string? path)
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            return null;
        }

        try
        {
            using FileStream stream = File.OpenRead(path);
            using Image imageFromFile = Image.FromStream(stream);
            return new Bitmap(imageFromFile); // a copy that does not depend on the file
        }
        catch (Exception)
        {
            // Not an image, or the file could not be read: show no picture instead of crashing.
            return null;
        }
    }

    // A small version of the image that fits in a maxSize x maxSize square.
    // The aspect ratio is kept, so the picture is not stretched.
    public static Image? LoadThumbnail(string? path, int maxSize)
    {
        using Image? image = LoadImage(path);
        if (image == null)
        {
            return null;
        }

        double scale = Math.Min((double)maxSize / image.Width, (double)maxSize / image.Height);
        int width = Math.Max(1, (int)(image.Width * scale));
        int height = Math.Max(1, (int)(image.Height * scale));

        var thumbnail = new Bitmap(width, height);
        using (Graphics graphics = Graphics.FromImage(thumbnail))
        {
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic; // smooth resizing
            graphics.DrawImage(image, 0, 0, width, height);
        }
        return thumbnail;
    }
}
