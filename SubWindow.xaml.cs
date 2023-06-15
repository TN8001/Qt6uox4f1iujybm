using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace Qt6uox4f1iujybm;


public partial class SubWindow : Window
{
    private static readonly string[] SupportedFormats = { ".jpg", ".jpeg", ".bmp", ".png" };

    private readonly ViewModel viewModel;

    public SubWindow(ViewModel viewModel)
    {
        InitializeComponent();
        DataContext = this.viewModel = viewModel;
    }

    private void File_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop, true) ? DragDropEffects.Copy
                                                                      : DragDropEffects.None;
        e.Handled = true;
    }

    private void File_Drop(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

        var files = (string[])e.Data.GetData(DataFormats.FileDrop);

        var ok = files.Where(x => SupportedFormats.Contains(Path.GetExtension(x).ToLower()));
        foreach (var path in ok)
        {
            viewModel.AddItem(path, IsOverwrite);

            // ViewModel内でMessageBox.Showしたくないので、確認画面をローカル関数で雑に差し込むｗ
            //[ローカル関数 -C# プログラミング ガイド | Microsoft Learn](https://learn.microsoft.com/ja-jp/dotnet/csharp/programming-guide/classes-and-structs/local-functions)
            bool IsOverwrite()
            {
                var msg = $"同名のファイルが既にあります。\n上書きしますか？\n\n{path}";
                var r = MessageBox.Show(this, msg, "", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                return r == MessageBoxResult.Yes;
            }
        }

        var ng = files.Where(x => !SupportedFormats.Contains(Path.GetExtension(x).ToLower()));
        if (ng.Any()) MessageBox.Show(this, $"{string.Join("\n", ng)}\nのファイルは読めませんでした");
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
        // 閉じる動作をキャンセルし非表示にするだけ
        e.Cancel = true;
        Hide();
    }
}
