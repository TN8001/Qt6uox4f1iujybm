using System.Diagnostics;
using System.IO;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Qt6uox4f1iujybm;


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
            Debug.WriteLine("TrySave:false");
            using var fsout = new FileStream(FilePath, FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
            var encoder = new JpegBitmapEncoder();
            foreach (var cloneFrame in decoder.Frames) encoder.Frames.Add(BitmapFrame.Create(cloneFrame));
            ((BitmapMetadata)encoder.Frames[0].Metadata).Title = value;
            encoder.Save(fsout);
        }
    }
}
