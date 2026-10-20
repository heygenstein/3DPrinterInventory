namespace PrintInventory.Data;

// A physical roll of a Filament.
// Remaining grams is NOT stored here. It is calculated in SpoolService.
public class Spool
{
    public int Id { get; set; }

    public int FilamentId { get; set; }
    public Filament Filament { get; set; } = null!;

    public double InitialWeightGrams { get; set; } = 1000;
    public decimal PricePaid { get; set; }
    public DateTime PurchaseDate { get; set; } = DateTime.Today;
    public double LowStockThresholdGrams { get; set; } = 150;
    public double AdjustmentGrams { get; set; } = 0;
    public bool IsArchived { get; set; }
    public string? Notes { get; set; }

    // Navigation property: all print jobs that used this spool.
    public List<PrintJob> PrintJobs { get; set; } = new();
}
