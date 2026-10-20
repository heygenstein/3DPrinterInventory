using PrintInventory.Data;

namespace PrintInventory.Services;

// Business rules about spools: how many grams are left, and correcting that value.
//
// Remaining grams is never stored in the database. It is always calculated:
//   Remaining = InitialWeightGrams - (sum of UsedGrams of this spool's print jobs) + AdjustmentGrams
public class SpoolService
{
    // The formula itself, in one place. Every other method uses this.
    public double CalculateRemainingGrams(Spool spool, double usedGrams)
    {
        return spool.InitialWeightGrams - usedGrams + spool.AdjustmentGrams;
    }

    // Total grams used by all print jobs of one spool (0 if there are no jobs).
    public double GetUsedGrams(int spoolId)
    {
        using var db = new AppDbContext();
        return db.PrintJobs
            .Where(job => job.SpoolId == spoolId)
            .Sum(job => job.UsedGrams);
    }

    public double GetRemainingGrams(int spoolId)
    {
        using var db = new AppDbContext();
        Spool spool = FindSpool(db, spoolId);
        double usedGrams = GetUsedGrams(spoolId);
        return CalculateRemainingGrams(spool, usedGrams);
    }

    // The user weighed the spool and tells us the real remaining grams.
    // We do not change InitialWeightGrams or the print jobs. Instead we compute the
    // AdjustmentGrams that makes the formula give exactly the real value:
    //   actual = Initial - Used + Adjustment
    //   =>  Adjustment = actual - Initial + Used
    public void AdjustRemaining(int spoolId, double actualRemainingGrams)
    {
        if (actualRemainingGrams < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(actualRemainingGrams), "Remaining grams cannot be negative.");
        }

        using var db = new AppDbContext();
        Spool spool = FindSpool(db, spoolId);
        double usedGrams = GetUsedGrams(spoolId);

        spool.AdjustmentGrams = actualRemainingGrams - spool.InitialWeightGrams + usedGrams;
        db.SaveChanges();
    }

    public bool IsLowStock(Spool spool, double remainingGrams)
    {
        return remainingGrams <= spool.LowStockThresholdGrams;
    }

    // Remaining as a percentage of the initial weight, kept between 0 and 100.
    public double CalculateRemainingPercent(Spool spool, double remainingGrams)
    {
        if (spool.InitialWeightGrams <= 0)
        {
            return 0;
        }
        double percent = remainingGrams / spool.InitialWeightGrams * 100;
        return Math.Clamp(percent, 0, 100);
    }

    // Spools are never deleted, only archived. This keeps the print history complete.
    public void SetArchived(int spoolId, bool isArchived)
    {
        using var db = new AppDbContext();
        Spool spool = FindSpool(db, spoolId);
        spool.IsArchived = isArchived;
        db.SaveChanges();
    }

    private static Spool FindSpool(AppDbContext db, int spoolId)
    {
        Spool? spool = db.Spools.Find(spoolId);
        if (spool == null)
        {
            throw new InvalidOperationException($"Spool with Id {spoolId} was not found.");
        }
        return spool;
    }
}
