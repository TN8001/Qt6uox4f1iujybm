using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Qt6uox4f1iujybm;


public partial class PictModel : ObservableObject
{
    public ObservableCollection<PictItem> Items { get; } = new();
    [ObservableProperty] private PictItem? selectedItem;

    private readonly string folder;

    public PictModel(string folder)
    {
        this.folder = folder;
        Directory.CreateDirectory(folder);
        foreach (var path in Directory.EnumerateFiles(folder, "*.jpg", SearchOption.TopDirectoryOnly))
            Items.Add(new(path));
    }

    [RelayCommand] private void Apply(PictItem? item) => SelectedItem = item;
    [RelayCommand]
    private void Delete(PictItem item)
    {
        Items.Remove(item);
        File.Delete(item.FilePath);
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

public partial class PictItem : ObservableObject
{
    [ObservableProperty] private string? fileTitle;
    public string FilePath { get; }
    public string FileName => Path.GetFileNameWithoutExtension(FilePath);

    public PictItem(string filePath)
    {
        FilePath = filePath;
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        var f = BitmapFrame.Create(fs, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnDemand);
        var meta = f.Metadata as BitmapMetadata;
        fileTitle = meta?.Title;
    }

    partial void OnFileTitleChanged(string? value)
    {
        // なぜかタイトルと件名両方入っちゃう？？
        // 事前にパディング取っておくこともできると思うが、よくわからんので作り直しｗ
        // [Setting PNG image metadata doesn't work · Issue #7121 · dotnet/wpf](https://github.com/dotnet/wpf/issues/7121)
        using var fsin = new FileStream(FilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
        var decoder = new JpegBitmapDecoder(fsin, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
        var frame = decoder.Frames[0];
        var meta = frame.CreateInPlaceBitmapMetadataWriter();
        meta.Title = value;

        if (!meta.TrySave())
        {
            using var fsout = new FileStream(FilePath, FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
            var encoder = new JpegBitmapEncoder();
            foreach (var cloneFrame in decoder.Frames) encoder.Frames.Add(BitmapFrame.Create(cloneFrame));
            ((BitmapMetadata)encoder.Frames[0].Metadata).Title = value;
            encoder.Save(fsout);
        }
    }
}
