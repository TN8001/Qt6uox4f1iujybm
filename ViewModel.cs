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
    public ObservableCollection<Item> Items { get; } = new();

    [ObservableProperty] private bool isSubWindowShown;

    [ObservableProperty] private Item? selectedItem;


    private readonly string folder;

    public ViewModel(string folder)
    {
        this.folder = folder;
        Directory.CreateDirectory(folder);
        ImportFiles(Directory.EnumerateFiles(folder, "*.jpg", SearchOption.TopDirectoryOnly));
    }

    [RelayCommand]
    private void Delete(Item item)
    {
        Items.Remove(item);
        File.Delete(item.FilePath);
    }

    [RelayCommand]
    private void Apply(Item? item) => SelectedItem = item;



    public void ImportFiles(IEnumerable<string> paths)
    {
        foreach (var path in paths) Items.Add(new(path));
    }

    public void AddItem(string path, Func<bool> isOverwrite)
    {
        var fileName = Path.GetFileNameWithoutExtension(path);
        var outPath = Path.Combine(folder, $"{fileName}.jpg");

        if (File.Exists(outPath))
        {
            if (!isOverwrite()) return;
            Items.Remove(Items.FirstOrDefault(x => x.FilePath == outPath)!);
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

        Items.Add(new(outPath));
    }
}
