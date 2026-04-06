using System.Text;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UndertaleModLib.Util;

EnsureDataLoaded();

string outputFolder = PromptChooseDirectory();
if (outputFolder is null)
{
    return;
}

SetProgressBar(null, "Collecting sprite names", 0, Data.Sprites.Count);
StartProgressBarUpdater();

List<string> spriteNames = new();

foreach (var sprite in Data.Sprites)
{
    if (sprite is not null)
    {
        spriteNames.Add(sprite.Name.Content);
    }
    IncrementProgressParallel();
}

await StopProgressBarUpdater();

// Write sprite names to file
string outputPath = Path.Combine(outputFolder, "sprite_names.txt");
File.WriteAllLines(outputPath, spriteNames);

HideProgressBar();
