namespace PrintInventory.Data;

// One print attempt.
public class PrintJob
{
    public int Id { get; set; }

    public int PrintModelId { get; set; }
    public PrintModel PrintModel { get; set; } = null!;

    public int PrinterId { get; set; }
    public Printer Printer { get; set; } = null!;

    public int SpoolId { get; set; }
    public Spool Spool { get; set; } = null!;

    public DateTime PrintDate { get; set; } = DateTime.Now;
    public double UsedGrams { get; set; }
    public PrintResult Result { get; set; } = PrintResult.Success;
    public string? Notes { get; set; }
}
