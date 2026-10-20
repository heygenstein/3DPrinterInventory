using System.ComponentModel;

namespace PrintInventory.Forms;

// The top line of a drawer: a title on the left and a ✕ close button on the right.
public class DrawerHeader : TableLayoutPanel
{
    private readonly Label titleLabel = new Label
    {
        AutoSize = true,
        Font = DarkTheme.TitleFont,
        ForeColor = DarkTheme.TextColor,
        Anchor = AnchorStyles.Left
    };

    public event EventHandler? CloseClicked;

    public DrawerHeader()
    {
        Dock = DockStyle.Fill;
        AutoSize = true;
        ColumnCount = 2;
        RowCount = 1;
        Margin = new Padding(0);
        BackColor = DarkTheme.CardColor;
        ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        var closeButton = new NeonButton("✕", isPrimary: false) { Width = 38, Margin = new Padding(0) };
        closeButton.Click += CloseButton_Click;

        Controls.Add(titleLabel, 0, 0);
        Controls.Add(closeButton, 1, 0);
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] // not saved by the Designer
    public string Title
    {
        get => titleLabel.Text;
        set => titleLabel.Text = value;
    }

    private void CloseButton_Click(object? sender, EventArgs e)
    {
        CloseClicked?.Invoke(this, EventArgs.Empty);
    }
}
