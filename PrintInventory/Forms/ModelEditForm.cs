using PrintInventory.Data;
using PrintInventory.Services;

namespace PrintInventory.Forms;

// Dialog to add a new 3D model (modelId = null) or edit an existing one.
//
// About the preview image:
// - savedImagePath   = the picture stored in the database when the dialog opened.
// - previewImagePath = the picture shown in the dialog right now (the user may choose another one).
// The new picture is copied into the Images folder only when the user clicks Save,
// so Cancel leaves no extra files behind.
public class ModelEditForm : Form
{
    private readonly int? modelId;
    private readonly ImageStorageService imageStorage = new ImageStorageService();

    private string? savedImagePath;
    private string? previewImagePath;

    private readonly TextBox nameTextBox = new TextBox();
    private readonly TextBox filePathTextBox = new TextBox { Width = UiTheme.InputWidth };
    private readonly PictureBox previewPictureBox = new PictureBox
    {
        Size = new Size(200, 200),
        SizeMode = PictureBoxSizeMode.Zoom, // fit the picture, keep its aspect ratio
        BorderStyle = BorderStyle.FixedSingle,
        BackColor = DarkTheme.InputColor
    };
    private readonly NumericUpDown estimatedGramsInput = new DarkNumericUpDown
    {
        DecimalPlaces = 1,
        Minimum = 0,
        Maximum = 10000
    };
    private readonly NumericUpDown estimatedHoursInput = new DarkNumericUpDown
    {
        Minimum = 0,
        Maximum = 500,
        Width = 70
    };
    private readonly NumericUpDown estimatedMinutesInput = new DarkNumericUpDown
    {
        Minimum = 0,
        Maximum = 59,
        Width = 70
    };
    private readonly TextBox notesTextBox = UiTheme.CreateNotesTextBox();

    public ModelEditForm(int? modelId)
    {
        this.modelId = modelId;
        Text = modelId == null ? "Model Ekle" : "Modeli Düzenle";
        UiTheme.ApplyDialogStyle(this);
        BuildLayout();

        if (modelId != null)
        {
            LoadModel(modelId.Value);
        }
    }

    private void BuildLayout()
    {
        // File row: [text box] [Browse...]
        Button browseFileButton = new NeonButton("Gözat...", isPrimary: false);
        browseFileButton.Click += BrowseFileButton_Click;
        filePathTextBox.Margin = new Padding(0, 4, 8, 0);
        var fileRow = new FlowLayoutPanel { AutoSize = true, WrapContents = false };
        fileRow.Controls.Add(filePathTextBox);
        fileRow.Controls.Add(browseFileButton);

        // Image row: picture with [Choose...] [Clear] below it
        Button chooseImageButton = new NeonButton("Seç...", isPrimary: false);
        chooseImageButton.Click += ChooseImageButton_Click;
        Button clearImageButton = new NeonButton("Kaldır", isPrimary: false);
        clearImageButton.Click += ClearImageButton_Click;
        var imageButtons = new FlowLayoutPanel { AutoSize = true, Margin = new Padding(0, 8, 0, 0) };
        imageButtons.Controls.Add(chooseImageButton);
        imageButtons.Controls.Add(clearImageButton);
        var imageRow = new FlowLayoutPanel
        {
            AutoSize = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false
        };
        imageRow.Controls.Add(previewPictureBox);
        imageRow.Controls.Add(imageButtons);

        // Time row: [hours] h [minutes] min
        var timeRow = new FlowLayoutPanel { AutoSize = true, WrapContents = false };
        timeRow.Controls.Add(estimatedHoursInput);
        timeRow.Controls.Add(CreateUnitLabel("sa"));
        timeRow.Controls.Add(estimatedMinutesInput);
        timeRow.Controls.Add(CreateUnitLabel("dk"));

        TableLayoutPanel fieldsTable = UiTheme.CreateFieldsTable();
        UiTheme.AddFormRow(fieldsTable, "Ad", nameTextBox);
        UiTheme.AddFormRow(fieldsTable, "Model dosyası", fileRow);
        UiTheme.AddFormRow(fieldsTable, "Önizleme resmi", imageRow, labelAtTop: true);
        UiTheme.AddFormRow(fieldsTable, "Tahmini gram", estimatedGramsInput);
        UiTheme.AddFormRow(fieldsTable, "Tahmini süre", timeRow);
        UiTheme.AddFormRow(fieldsTable, "Notlar", notesTextBox);

        Button saveButton = UiTheme.CreateSaveButton();
        saveButton.Click += SaveButton_Click;
        Button cancelButton = UiTheme.CreateCancelButton();

        UiTheme.BuildDialogLayout(this, fieldsTable, saveButton, cancelButton);
    }

    private static Label CreateUnitLabel(string text)
    {
        return new Label
        {
            Text = text,
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(4, 0, 12, 0)
        };
    }

    private void LoadModel(int id)
    {
        using var db = new AppDbContext();
        PrintModel? model = db.PrintModels.Find(id);
        if (model == null)
        {
            throw new InvalidOperationException($"Model with Id {id} was not found.");
        }

        nameTextBox.Text = model.Name;
        filePathTextBox.Text = model.FilePath;
        estimatedGramsInput.Value = (decimal)model.EstimatedGrams;
        estimatedHoursInput.Value = Math.Min(model.EstimatedPrintTimeMinutes / 60, (int)estimatedHoursInput.Maximum);
        estimatedMinutesInput.Value = model.EstimatedPrintTimeMinutes % 60;
        notesTextBox.Text = model.Notes;

        savedImagePath = model.PreviewImagePath;
        ShowPreview(model.PreviewImagePath);
    }

    private void ShowPreview(string? imagePath)
    {
        previewImagePath = imagePath;

        // Free the old picture before showing the new one.
        Image? oldImage = previewPictureBox.Image;
        previewPictureBox.Image = ImageLoader.LoadImage(imagePath);
        oldImage?.Dispose();
    }

    private void BrowseFileButton_Click(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Model dosyasını seçin",
            Filter = "3D model dosyaları (*.stl;*.3mf;*.obj)|*.stl;*.3mf;*.obj|Tüm dosyalar (*.*)|*.*"
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            filePathTextBox.Text = dialog.FileName;

            // Convenience: if no name was typed yet, use the file name ("benchy.stl" -> "benchy").
            if (string.IsNullOrWhiteSpace(nameTextBox.Text))
            {
                nameTextBox.Text = Path.GetFileNameWithoutExtension(dialog.FileName);
            }
        }
    }

    private void ChooseImageButton_Click(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Önizleme resmini seçin",
            Filter = "Resimler (*.png;*.jpg;*.jpeg;*.bmp;*.gif)|*.png;*.jpg;*.jpeg;*.bmp;*.gif"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        Image? testImage = ImageLoader.LoadImage(dialog.FileName);
        if (testImage == null)
        {
            DarkMessageBox.ShowWarning(this, "Bu dosya resim olarak açılamadı.", "Önizleme resmi");
            return;
        }
        testImage.Dispose();

        ShowPreview(dialog.FileName);
    }

    private void ClearImageButton_Click(object? sender, EventArgs e)
    {
        ShowPreview(null);
    }

    private void SaveButton_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(nameTextBox.Text))
        {
            UiTheme.ShowValidationError(nameTextBox, "Ad zorunludur.");
            return;
        }

        // A typed path may have a typo. Warn, but allow it (the file may be on a USB drive that is not connected).
        string filePath = filePathTextBox.Text.Trim();
        if (filePath != "" && !File.Exists(filePath))
        {
            bool saveAnyway = DarkMessageBox.AskYesNo(this,
                $"Model dosyası bulunamadı:\n{filePath}\n\nYine de kaydedilsin mi?",
                "Model dosyası");
            if (!saveAnyway)
            {
                filePathTextBox.Focus();
                return;
            }
        }

        // If the user chose a different picture, copy it into the Images folder now.
        bool imageChanged = previewImagePath != savedImagePath;
        string? finalImagePath = savedImagePath;
        if (imageChanged)
        {
            finalImagePath = null;
            if (previewImagePath != null)
            {
                try
                {
                    finalImagePath = imageStorage.CopyToImagesFolder(previewImagePath);
                }
                catch (IOException ex)
                {
                    DarkMessageBox.ShowError(this, $"Resim kopyalanamadı:\n{ex.Message}", "Önizleme resmi");
                    return;
                }
            }
        }

        using (var db = new AppDbContext())
        {
            PrintModel model;
            if (modelId == null)
            {
                model = new PrintModel { CreatedDate = DateTime.Now };
                db.PrintModels.Add(model);
            }
            else
            {
                model = db.PrintModels.Find(modelId.Value)!;
            }

            model.Name = nameTextBox.Text.Trim();
            model.FilePath = filePath;
            model.PreviewImagePath = finalImagePath;
            model.EstimatedGrams = (double)estimatedGramsInput.Value;
            model.EstimatedPrintTimeMinutes = (int)estimatedHoursInput.Value * 60 + (int)estimatedMinutesInput.Value;
            model.Notes = notesTextBox.Text.Trim();

            db.SaveChanges();
        }

        // Only after the database is saved: remove the old copied picture, it is not used anymore.
        if (imageChanged)
        {
            imageStorage.DeleteIfInImagesFolder(savedImagePath);
        }

        DialogResult = DialogResult.OK;
    }

    // Free the picture when the dialog closes.
    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        previewPictureBox.Image?.Dispose();
        base.OnFormClosed(e);
    }
}
