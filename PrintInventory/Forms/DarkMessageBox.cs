namespace PrintInventory.Forms;

// A message box in the dark theme, used instead of the light Windows MessageBox.
//
//   DarkMessageBox.ShowWarning(this, "Ad zorunludur.", "Girdiyi kontrol edin");
//   if (DarkMessageBox.AskYesNo(this, "Silinsin mi?", "Baskıyı sil")) { ... }
public class DarkMessageBox : Form
{
    private enum MessageKind
    {
        Info,
        Warning,
        Error,
        Question
    }

    private const int TextWidth = 380;

    private readonly MessageKind kind;
    private Button? yesButton; // only for questions

    private DarkMessageBox(string text, string caption, MessageKind kind, bool isQuestion)
    {
        this.kind = kind;
        Text = caption;
        UiTheme.ApplyDialogStyle(this);
        BuildLayout(text, isQuestion);
    }

    public static void ShowInfo(IWin32Window? owner, string text, string caption)
    {
        ShowMessage(owner, text, caption, MessageKind.Info);
    }

    public static void ShowWarning(IWin32Window? owner, string text, string caption)
    {
        ShowMessage(owner, text, caption, MessageKind.Warning);
    }

    public static void ShowError(IWin32Window? owner, string text, string caption)
    {
        ShowMessage(owner, text, caption, MessageKind.Error);
    }

    // Returns true when the user clicked "Evet".
    public static bool AskYesNo(IWin32Window? owner, string text, string caption)
    {
        using var box = new DarkMessageBox(text, caption, MessageKind.Question, isQuestion: true);
        return box.ShowDialog(owner) == DialogResult.Yes;
    }

    private static void ShowMessage(IWin32Window? owner, string text, string caption, MessageKind kind)
    {
        using var box = new DarkMessageBox(text, caption, kind, isQuestion: false);
        box.ShowDialog(owner);
    }

    private void BuildLayout(string text, bool isQuestion)
    {
        var iconBox = new PictureBox
        {
            Size = new Size(40, 40),
            Margin = new Padding(0, 0, 16, 0),
            BackColor = DarkTheme.CardColor
        };
        iconBox.Paint += IconBox_Paint;

        var messageLabel = new Label
        {
            Text = text,
            AutoSize = true,
            MaximumSize = new Size(TextWidth, 0), // long texts wrap onto more lines
            ForeColor = DarkTheme.TextColor,
            Margin = new Padding(0, 9, 0, 0)
        };

        var messageRow = new TableLayoutPanel { AutoSize = true, ColumnCount = 2, RowCount = 1, Margin = new Padding(0) };
        messageRow.Controls.Add(iconBox, 0, 0);
        messageRow.Controls.Add(messageLabel, 1, 0);

        var buttonsPanel = new FlowLayoutPanel
        {
            AutoSize = true,
            FlowDirection = FlowDirection.RightToLeft,
            Anchor = AnchorStyles.Right,
            Margin = new Padding(0, 22, 0, 0)
        };

        if (isQuestion)
        {
            var noButton = new NeonButton("Hayır", isPrimary: false) { DialogResult = DialogResult.No, Margin = new Padding(0) };
            yesButton = new NeonButton("Evet", isPrimary: true) { DialogResult = DialogResult.Yes, Margin = new Padding(0, 0, 10, 0) };
            buttonsPanel.Controls.Add(noButton);
            buttonsPanel.Controls.Add(yesButton);
            AcceptButton = yesButton; // Enter = Evet
            CancelButton = noButton;  // Esc = Hayır
        }
        else
        {
            var okButton = new NeonButton("Tamam", isPrimary: true) { DialogResult = DialogResult.OK, Margin = new Padding(0) };
            buttonsPanel.Controls.Add(okButton);
            AcceptButton = okButton;
            CancelButton = okButton;
        }

        var layout = new TableLayoutPanel
        {
            AutoSize = true,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(24, 22, 24, 20)
        };
        layout.Controls.Add(messageRow, 0, 0);
        layout.Controls.Add(buttonsPanel, 0, 1);
        Controls.Add(layout);
    }

    // Enter clicks the button that has the focus. "Hayır" was added first, so it would get
    // the focus; we give it to "Evet" instead (like the Windows MessageBox). This must happen
    // after the window is visible, so we do it in OnShown.
    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        yesButton?.Focus();
    }

    // A colored, glowing circle with a symbol: i (info), ! (warning), ✕ (error), ? (question).
    private void IconBox_Paint(object? sender, PaintEventArgs e)
    {
        Graphics graphics = e.Graphics;
        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        Color color;
        string symbol;
        switch (kind)
        {
            case MessageKind.Warning:
                color = Color.FromArgb(255, 180, 60);
                symbol = "!";
                break;
            case MessageKind.Error:
                color = DarkTheme.WarningColor;
                symbol = "✕";
                break;
            case MessageKind.Question:
                color = DarkTheme.AccentColor;
                symbol = "?";
                break;
            default:
                color = DarkTheme.AccentColor;
                symbol = "i";
                break;
        }

        using (var glowBrush = new SolidBrush(Color.FromArgb(45, color)))
        {
            graphics.FillEllipse(glowBrush, 0, 0, 39, 39);
        }
        using (var ringPen = new Pen(color, 2))
        {
            graphics.DrawEllipse(ringPen, 5, 5, 29, 29);
        }
        DarkTheme.DrawText(graphics, symbol, DarkTheme.NormalBoldFont, new Rectangle(0, 0, 40, 40), color,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }
}
