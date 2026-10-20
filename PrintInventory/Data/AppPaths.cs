namespace PrintInventory.Data;

// All file locations used by the app, in one place.
// Everything lives under %LocalAppData%\PrintInventory.
public static class AppPaths
{
    public static string DataFolder
    {
        get
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string folder = Path.Combine(localAppData, "PrintInventory");
            Directory.CreateDirectory(folder); // Does nothing if the folder already exists.
            return folder;
        }
    }

    public static string DatabaseFile => Path.Combine(DataFolder, "printinventory.db");

    public static string ImagesFolder
    {
        get
        {
            string folder = Path.Combine(DataFolder, "Images");
            Directory.CreateDirectory(folder);
            return folder;
        }
    }
}
