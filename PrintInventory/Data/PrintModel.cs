namespace PrintInventory.Data;

// A 3D model that I print (usually an .stl file).
public class PrintModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? FilePath { get; set; }
    public string? PreviewImagePath { get; set; }
    public double EstimatedGrams { get; set; }
    public int EstimatedPrintTimeMinutes { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;

    // Navigation property: all print attempts of this model.
    public List<PrintJob> PrintJobs { get; set; } = new();
}
