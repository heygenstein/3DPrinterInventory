using PrintInventory.Data;

namespace PrintInventory.Services;

// Keeps preview images inside the app's own Images folder.
// The database stores only the file path, never the image bytes.
public class ImageStorageService
{
    // Copies a picture into the Images folder under a new unique name and returns the new path.
    // A unique name (Guid) means two pictures called "photo.jpg" never overwrite each other.
    public string CopyToImagesFolder(string sourcePath)
    {
        string extension = Path.GetExtension(sourcePath).ToLowerInvariant();
        string fileName = $"{Guid.NewGuid()}{extension}";
        string destinationPath = Path.Combine(AppPaths.ImagesFolder, fileName);

        File.Copy(sourcePath, destinationPath);
        return destinationPath;
    }

    // Deletes an image, but only if it is inside our Images folder.
    // This way we can never delete one of the user's own files by mistake.
    public void DeleteIfInImagesFolder(string? path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        string fullPath = Path.GetFullPath(path);
        string imagesFolder = Path.GetFullPath(AppPaths.ImagesFolder) + Path.DirectorySeparatorChar;
        bool isInImagesFolder = fullPath.StartsWith(imagesFolder, StringComparison.OrdinalIgnoreCase);

        if (isInImagesFolder && File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }
}
