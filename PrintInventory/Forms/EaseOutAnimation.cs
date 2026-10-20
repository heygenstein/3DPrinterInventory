namespace PrintInventory.Forms;

// Moves Progress from 0 to 1 in about half a second: fast at first, slower at the end
// ("ease out"). A control multiplies its value by Progress and draws itself again on
// every ProgressChanged, so a ring "fills up" instead of appearing at once.
//
// IDisposable is the standard .NET way to say "call Dispose() when you are done";
// here it frees the timer.
public class EaseOutAnimation : IDisposable
{
    private readonly System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer { Interval = 15 };

    public double Progress { get; private set; } = 1; // 1 = finished (nothing to animate)

    public event EventHandler? ProgressChanged;

    public EaseOutAnimation()
    {
        timer.Tick += Timer_Tick;
    }

    public void Start()
    {
        Progress = 0;
        timer.Start();
    }

    // Each tick covers 12 % of the remaining way (plus a tiny fixed step, so it really ends).
    private void Timer_Tick(object? sender, EventArgs e)
    {
        Progress += (1 - Progress) * 0.12 + 0.004;
        if (Progress >= 0.999)
        {
            Progress = 1;
            timer.Stop();
        }
        ProgressChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        timer.Dispose();
    }
}
