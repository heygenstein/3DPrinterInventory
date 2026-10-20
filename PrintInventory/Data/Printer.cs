namespace PrintInventory.Data;

public class Printer
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Brand { get; set; } = "";
    public string Model { get; set; } = "";
    public double NozzleDiameterMm { get; set; } = 0.4;
    public int BuildVolumeXMm { get; set; }
    public int BuildVolumeYMm { get; set; }
    public int BuildVolumeZMm { get; set; }
    public string? Notes { get; set; }

    // Navigation property: all print jobs printed on this printer.
    public List<PrintJob> PrintJobs { get; set; } = new();
}
