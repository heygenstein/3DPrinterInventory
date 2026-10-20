namespace PrintInventory.Data;

// A filament product type, e.g. "eSun PLA+ Black".
public class Filament
{
    public int Id { get; set; }
    public string Brand { get; set; } = "";
    public FilamentMaterial Material { get; set; } = FilamentMaterial.PLA;
    public string ColorName { get; set; } = "";
    public string ColorHex { get; set; } = "#000000";
    public double DiameterMm { get; set; } = 1.75;
    public double DensityGPerCm3 { get; set; } = 1.24;
    public int? RecommendedNozzleTempC { get; set; }
    public int? RecommendedBedTempC { get; set; }
    public string? Notes { get; set; }

    // Navigation property: all physical spools of this filament.
    public List<Spool> Spools { get; set; } = new();
}
