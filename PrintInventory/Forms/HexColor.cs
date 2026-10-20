namespace PrintInventory.Forms;

// Converts between a WinForms Color and the "#RRGGBB" text we store in the database.
public static class HexColor
{
    public static string FromColor(Color color)
    {
        // X2 = two-digit hexadecimal, e.g. 255 -> "FF".
        return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    }

    public static Color ToColor(string hex)
    {
        try
        {
            return ColorTranslator.FromHtml(hex);
        }
        catch (Exception)
        {
            // Invalid text in the database should not crash the app. Show black instead.
            return Color.Black;
        }
    }
}
