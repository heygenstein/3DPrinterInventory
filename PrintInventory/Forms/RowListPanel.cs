namespace PrintInventory.Forms;

// A scrollable vertical list of rows. Every row is made as wide as the list.
public class RowListPanel : FlowLayoutPanel
{
    public RowListPanel()
    {
        Dock = DockStyle.Fill;
        FlowDirection = FlowDirection.TopDown;
        WrapContents = false; // one column
        AutoScroll = true;
        BackColor = DarkTheme.CardColor;
        Margin = new Padding(0);
        DoubleBuffered = true; // less flicker while scrolling
        DarkTheme.ApplyDarkScrollBars(this);
    }

    protected override void OnControlAdded(ControlEventArgs e)
    {
        base.OnControlAdded(e);
        if (e.Control != null)
        {
            FitWidth(e.Control);
        }
    }

    protected override void OnClientSizeChanged(EventArgs e)
    {
        base.OnClientSizeChanged(e);
        foreach (Control row in Controls)
        {
            FitWidth(row);
        }
    }

    // We always keep room for the scroll bar. Otherwise the rows would get narrower
    // (and jump) at the moment the scroll bar appears.
    private void FitWidth(Control row)
    {
        int width = ClientSize.Width - SystemInformation.VerticalScrollBarWidth - row.Margin.Horizontal;
        row.Width = Math.Max(100, width);
    }

    public void ShowEmptyText(string text)
    {
        Controls.Add(new Label
        {
            Text = text,
            AutoSize = true,
            ForeColor = DarkTheme.MutedTextColor,
            Font = DarkTheme.NormalFont,
            Margin = new Padding(2, 8, 0, 0)
        });
    }
}
