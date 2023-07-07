using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Qt6uox4f1iujybm;


public partial class ViewModel : ObservableObject
{
    public ObservableCollection<PictItem> PictItems { get; } = new();

    [ObservableProperty] private PictItem? selectedPictItem;


    private readonly string folder;

    public ViewModel(string folder)
    {
        this.folder = folder;
        Directory.CreateDirectory(folder);
        ImportFiles(Directory.EnumerateFiles(folder, "*.jpg", SearchOption.TopDirectoryOnly));
    }

    [RelayCommand]
    private void Delete(PictItem item)
    {
        PictItems.Remove(item);
        File.Delete(item.FilePath);
    }

    [RelayCommand] private void Apply(PictItem? item) => SelectedPictItem = item;

    private void ImportFiles(IEnumerable<string> paths)
    {
        foreach (var path in paths) PictItems.Add(new(path));
    }

    public void AddPictItem(string path, Func<bool> isOverwrite)
    {
        var fileName = Path.GetFileNameWithoutExtension(path);
        var outPath = Path.Combine(folder, $"{fileName}.jpg");

        if (File.Exists(outPath))
        {
            if (!isOverwrite()) return;
            PictItems.Remove(PictItems.FirstOrDefault(x => x.FilePath == outPath)!);
        }

        using (var fsin = new FileStream(path, FileMode.Open, FileAccess.ReadWrite))
        using (var fsout = new FileStream(outPath, FileMode.Create, FileAccess.Write))
        {
            var f = BitmapFrame.Create(fsin, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnDemand);
            var meta = f.Metadata.Clone() as BitmapMetadata ?? new BitmapMetadata("jpg");
            var enc = new JpegBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(f, f.Thumbnail, meta, f.ColorContexts));
            enc.Save(fsout);
        }

        PictItems.Add(new(outPath));
    }
}
