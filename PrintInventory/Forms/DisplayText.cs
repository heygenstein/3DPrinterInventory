using PrintInventory.Data;

namespace PrintInventory.Forms;

// Turkish texts for values that are stored in English in the database (enums),
// and small formatting helpers used on many screens.
public static class DisplayText
{
    public static string ForResult(PrintResult result)
    {
        switch (result)
        {
            case PrintResult.Success: return "Başarılı";
            case PrintResult.Failed: return "Başarısız";
            case PrintResult.Cancelled: return "İptal edildi";
            default: return result.ToString();
        }
    }

    // Material names (PLA, PETG, ...) are the same in Turkish, except "Other".
    public static string ForMaterial(FilamentMaterial material)
    {
        if (material == FilamentMaterial.Other)
        {
            return "Diğer";
        }
        return material.ToString();
    }

    // "eSun PLA Black", with the Turkish material name ("Diğer" instead of "Other").
    public static string ForFilament(Filament filament)
    {
        return $"{filament.Brand} {ForMaterial(filament.Material)} {filament.ColorName}";
    }

    // 135 -> "2 sa 15 dk", 25 -> "25 dk"
    public static string FormatDuration(int totalMinutes)
    {
        int hours = totalMinutes / 60;
        int minutes = totalMinutes % 60;
        if (hours == 0)
        {
            return $"{minutes} dk";
        }
        return $"{hours} sa {minutes} dk";
    }

    // In Turkish the percent sign comes first: "%67".
    public static string Percent(double percent)
    {
        return $"%{percent:0}";
    }

    // "az önce", "25 dk önce", "3 sa önce", "2 gün önce", or the date for older prints.
    public static string FormatAge(DateTime date)
    {
        TimeSpan age = DateTime.Now - date;
        if (age.TotalMinutes < 1)
        {
            return "az önce";
        }
        if (age.TotalHours < 1)
        {
            return $"{(int)age.TotalMinutes} dk önce";
        }
        if (age.TotalDays < 1)
        {
            return $"{(int)age.TotalHours} sa önce";
        }
        if (age.TotalDays < 7)
        {
            return $"{(int)age.TotalDays} gün önce";
        }
        return date.ToShortDateString();
    }

    // Fills a ComboBox with every value of an enum, shown with its Turkish text.
    // The enum value is stored as the ListItem Id (an enum is a number underneath).
    public static void FillResultComboBox(ComboBox comboBox)
    {
        foreach (PrintResult result in Enum.GetValues<PrintResult>())
        {
            comboBox.Items.Add(new ListItem((int)result, ForResult(result)));
        }
    }

    public static void FillMaterialComboBox(ComboBox comboBox)
    {
        foreach (FilamentMaterial material in Enum.GetValues<FilamentMaterial>())
        {
            comboBox.Items.Add(new ListItem((int)material, ForMaterial(material)));
        }
    }
}
