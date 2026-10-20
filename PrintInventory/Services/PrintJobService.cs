using PrintInventory.Data;

namespace PrintInventory.Services;

// Business rules about print jobs: cost, and how many grams a job may use.
public class PrintJobService
{
    private readonly SpoolService spoolService = new SpoolService();

    // Print cost = PricePaid / InitialWeightGrams * UsedGrams
    // Example: 450 for a 1000 g spool, 15 g used -> 450 / 1000 * 15 = 6.75
    public decimal CalculateCost(Spool spool, double usedGrams)
    {
        if (spool.InitialWeightGrams <= 0)
        {
            return 0;
        }
        decimal pricePerGram = spool.PricePaid / (decimal)spool.InitialWeightGrams;
        return pricePerGram * (decimal)usedGrams;
    }

    // How many grams a job can use from this spool.
    // When we edit an existing job, its own UsedGrams are already subtracted from the
    // remaining value. So we add them back, otherwise the job would be "counted twice".
    public double GetAvailableGrams(int spoolId, int? editingJobId)
    {
        double availableGrams = spoolService.GetRemainingGrams(spoolId);

        if (editingJobId != null)
        {
            using var db = new AppDbContext();
            PrintJob? editingJob = db.PrintJobs.Find(editingJobId.Value);
            if (editingJob != null && editingJob.SpoolId == spoolId)
            {
                availableGrams += editingJob.UsedGrams;
            }
        }
        return availableGrams;
    }

    // A print job needs a model, a printer and an active spool.
    // Returns what is missing (Turkish text for the user, e.g. "bir model"), or null if everything is there.
    public string? FindMissingDataForNewJob()
    {
        using var db = new AppDbContext();
        if (!db.PrintModels.Any())
        {
            return "bir model";
        }
        if (!db.Printers.Any())
        {
            return "bir yazıcı";
        }
        if (!db.Spools.Any(s => !s.IsArchived))
        {
            return "aktif bir makara";
        }
        return null;
    }

    // Rule: warn (but do not block) when a job uses more than the spool has left.
    public bool UsesMoreThanAvailable(double usedGrams, double availableGrams)
    {
        return usedGrams > availableGrams;
    }
}
