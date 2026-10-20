using Microsoft.EntityFrameworkCore;
using PrintInventory.Data;

namespace PrintInventory.Services;

// One active spool together with its calculated values.
public class SpoolStatus
{
    public Spool Spool { get; set; } = null!;
    public double RemainingGrams { get; set; }
    public double RemainingPercent { get; set; }
    public bool IsLowStock { get; set; }
}

// How many grams of one material are left in stock (for the donut chart).
public class MaterialShare
{
    public FilamentMaterial Material { get; set; }
    public double RemainingGrams { get; set; }
}

// The numbers shown on the main screen. The remaining-grams rules stay in SpoolService;
// this class only collects and adds them up.
public class DashboardService
{
    private readonly SpoolService spoolService = new SpoolService();

    public List<SpoolStatus> GetActiveSpools()
    {
        List<Spool> spools;
        using (var db = new AppDbContext())
        {
            // Stable order (not by remaining %), so a card does not jump around after every print.
            spools = db.Spools
                .Include(s => s.Filament)
                .Where(s => !s.IsArchived)
                .OrderBy(s => s.Filament.Brand.ToLower())
                .ThenBy(s => s.Filament.ColorName.ToLower())
                .ThenBy(s => s.Id)
                .ToList();
        }

        var statuses = new List<SpoolStatus>();
        foreach (Spool spool in spools)
        {
            double usedGrams = spoolService.GetUsedGrams(spool.Id);
            double remainingGrams = spoolService.CalculateRemainingGrams(spool, usedGrams);
            statuses.Add(new SpoolStatus
            {
                Spool = spool,
                RemainingGrams = remainingGrams,
                RemainingPercent = spoolService.CalculateRemainingPercent(spool, remainingGrams),
                IsLowStock = spoolService.IsLowStock(spool, remainingGrams)
            });
        }
        return statuses;
    }

    // Negative remaining (more used than the spool had) counts as 0 in the totals.
    public double GetTotalRemainingGrams(List<SpoolStatus> spools)
    {
        return spools.Sum(s => Math.Max(0, s.RemainingGrams));
    }

    // Groups the spools by material and adds up the grams, biggest first.
    // GroupBy = "put the items with the same Material into one group".
    public List<MaterialShare> GetRemainingByMaterial(List<SpoolStatus> spools)
    {
        return spools
            .GroupBy(s => s.Spool.Filament.Material)
            .Select(group => new MaterialShare
            {
                Material = group.Key,
                RemainingGrams = group.Sum(s => Math.Max(0, s.RemainingGrams))
            })
            .Where(share => share.RemainingGrams > 0)
            .OrderByDescending(share => share.RemainingGrams)
            .ToList();
    }

    public int GetArchivedSpoolCount()
    {
        using var db = new AppDbContext();
        return db.Spools.Count(s => s.IsArchived);
    }

    public int GetPrintJobCount()
    {
        using var db = new AppDbContext();
        return db.PrintJobs.Count();
    }

    public double GetTotalPrintedGrams()
    {
        using var db = new AppDbContext();
        return db.PrintJobs.Sum(j => j.UsedGrams);
    }

    // Success rate = successful prints / finished prints (successful + failed).
    // Cancelled prints are not counted: they did not succeed, but they did not fail either.
    // Returns null when there are no finished prints yet.
    public double? GetSuccessRatePercent()
    {
        using var db = new AppDbContext();
        int successCount = db.PrintJobs.Count(j => j.Result == PrintResult.Success);
        int failedCount = db.PrintJobs.Count(j => j.Result == PrintResult.Failed);
        int finishedCount = successCount + failedCount;
        if (finishedCount == 0)
        {
            return null;
        }
        return 100.0 * successCount / finishedCount;
    }
}
