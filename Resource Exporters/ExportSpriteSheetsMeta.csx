/**
    Original script taken from ExportSpritesAsGIF.csx, changed by sam-k0.

    This script allows to apply a filter by sprite dimensions (width,height) and also generates the corresponding .meta file.

    .meta files are used by my "AssetLoader" mod for GameMaker games.
    The mod can be found on my github: https://github.com/sam-k0/AssetLoader
*/

using System.Text;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UndertaleModLib.Util;

using System.Windows.Forms;
using System.Drawing;


EnsureDataLoaded();

string texFolder = PromptChooseDirectory();
if (texFolder is null)
{
    return;
}

// Function to show the options dialog and get results
(string ComparisonOperator, int Width, int Height) ShowSpriteOptionsDialog()
{
    Form form = new Form();
    form.Text = "Sprite Export Filter Options";
    form.FormBorderStyle = FormBorderStyle.FixedDialog;
    form.StartPosition = FormStartPosition.CenterParent;
    form.ClientSize = new Size(600, 300);
    form.Font = new Font("Segoe UI", 9F);

    // Comparison operator dropdown
    Label labelDesc = new Label() {
        Text = "HOW TO USE\nThis script only exports sprites that meet the comparison requirement.",
        AutoSize = true, Location = new Point(10, 180) };

    Label labelDefault = new Label()
    {
        Text = "Default: Sprites with Width and Height >(greater) 32(w), 32(h) will be exported.",
        AutoSize = true, Location = new Point(10, 250)
    };



    Label labelOp = new Label() { Text = "Comparison [comp]:", AutoSize = true, Location = new Point(10, 10) };
    ComboBox comboBox = new ComboBox()
    {
        Location = new Point(130, 8),
        Size = new Size(160, 24),
        DropDownStyle = ComboBoxStyle.DropDownList
    };
    string[] operators = { ">", "<", "=", ">=", "<=" };
    comboBox.Items.AddRange(operators);
    comboBox.SelectedIndex = 0;

    // Width input
    Label labelWidth = new Label() { Text = "Spr. Width [comp]", AutoSize = true, Location = new Point(10, 48) };
    TextBox textBoxWidth = new TextBox() { Location = new Point(130, 44), Size = new Size(160, 24), Text = "32" };

    // Height input
    Label labelHeight = new Label() { Text = "Spr. Height [comp]", AutoSize = true, Location = new Point(10, 86) };
    TextBox textBoxHeight = new TextBox() { Location = new Point(130, 82), Size = new Size(160, 24), Text = "32" };

    // OK/Cancel buttons
    Button buttonOk = new Button() { Text = "OK", DialogResult = DialogResult.OK, Location = new Point(130, 120), Size = new Size(75, 28) };
    Button buttonCancel = new Button() { Text = "Cancel", DialogResult = DialogResult.Cancel, Location = new Point(215, 120), Size = new Size(75, 28) };

    form.Controls.AddRange(new Control[] { labelOp, comboBox, labelWidth, textBoxWidth, labelHeight, textBoxHeight, buttonOk, buttonCancel, labelDesc , labelDefault});

    form.AcceptButton = buttonOk;
    form.CancelButton = buttonCancel;

    // Show dialog
    if (form.ShowDialog() == DialogResult.OK)
    {
        string op = comboBox.SelectedItem.ToString();
        int width, height;
        if (!int.TryParse(textBoxWidth.Text, out width))
        {
            MessageBox.Show("Please enter a valid integer for width.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return ShowSpriteOptionsDialog(); // Recursively re-show dialog
        }
        if (!int.TryParse(textBoxHeight.Text, out height))
        {
            MessageBox.Show("Please enter a valid integer for height.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return ShowSpriteOptionsDialog();
        }
        return (op, width, height);
    }
    else
    {
       // cancel
        return (null, -1, -1);
    }
}


// Prompt for export settings.
bool padded = ScriptQuestion("Export sprites with padding?");
bool useSubDirectories = ScriptQuestion("Export sprites into subdirectories?");

var (comp, w, h) = ShowSpriteOptionsDialog();
if (comp == null)
{
    return;
}


SetProgressBar(null, "Sprites", 0, Data.Sprites.Count);
StartProgressBarUpdater();

TextureWorker worker = null;
using (worker = new())
{
    await DumpSprites();
}

await StopProgressBarUpdater();
HideProgressBar();

async Task DumpSprites()
{
    await Task.Run(() => Parallel.ForEach(Data.Sprites, DumpSprite));
}

void DumpSprite(UndertaleSprite sprite)
{
    if (sprite is not null)
    {
        var c_res = false;
        switch (comp) // check comp
        {
            case ">":
                c_res = sprite.Width > w && sprite.Height > h;
                break;

            case "<":
                c_res = sprite.Width < w&& sprite.Height < h;
                break;

            case "=":
                c_res = sprite.Width == w&& sprite.Height == h;
                break;

            case ">=":
                c_res = sprite.Width >= w&& sprite.Height >= h;
                break;
                
            case "<=":
                c_res = sprite.Width <= w&& sprite.Height <= h;
                break;
        }
        if (c_res == true)
        {
            string outputFolder = texFolder;
            if (useSubDirectories)
            {
                outputFolder = Path.Combine(outputFolder, sprite.Name.Content);
                if (sprite.Textures.Count > 0)
                {
                    Directory.CreateDirectory(outputFolder);
                }
            }

            //TODO: Combine these into one png file
            for (int i = 0; i < sprite.Textures.Count; i++)
            {
                if (sprite.Textures[i]?.Texture is not null)
                {
                    worker.ExportAsPNG(sprite.Textures[i].Texture, Path.Combine(outputFolder, $"{sprite.Name.Content}_{i}.png"), null, padded);
                }
            }

            // Also create a meta file with:
            var origins = (sprite.OriginX, sprite.OriginY);
            int imgcount = sprite.Textures.Count;
            string metaContent = $"SPRITESHEET={sprite.Name.Content}.png\nIMAGECOUNT={imgcount}\nXORIGIN={origins.Item1}\nYORIGIN={origins.Item2}\nREMOVEBG=1\nSMOOTH=0";
            File.WriteAllText(Path.Combine(outputFolder, $"{sprite.Name.Content}.meta"), metaContent);
        }
    }
    IncrementProgressParallel();
}
