using System.Globalization;
using Microsoft.EntityFrameworkCore;
using PrintInventory.Data;
using PrintInventory.Forms;

namespace PrintInventory;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // The app is in Turkish: numbers like "6,75", dates like "19.10.2026",
        // whatever language Windows itself uses.
        var turkish = new CultureInfo("tr-TR");
        CultureInfo.DefaultThreadCurrentCulture = turkish;
        CultureInfo.DefaultThreadCurrentUICulture = turkish;
        CultureInfo.CurrentCulture = turkish;
        CultureInfo.CurrentUICulture = turkish;

        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();

        // If an unexpected error happens in a button click or other UI event,
        // show a message and keep the app running, instead of crashing.
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += Application_ThreadException;

        // Create the database file if it does not exist, and apply any new migrations.
        try
        {
            using var db = new AppDbContext();
            db.Database.Migrate();
        }
        catch (Exception ex)
        {
            DarkMessageBox.ShowError(null,
                $"Veritabanı açılamadı:\n{AppPaths.DatabaseFile}\n\n{ex.Message}",
                "Baskı Envanteri");
            return; // without a database the app cannot work
        }

        Application.Run(new MainForm());
    }

    private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
    {
        DarkMessageBox.ShowError(null,
            $"Bir hata oluştu:\n\n{e.Exception.Message}",
            "Baskı Envanteri");
    }
}
