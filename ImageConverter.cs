using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Qt6uox4f1iujybm;



// [WPFで画像表示時にファイルをロックしないようにしたい - かずきのBlog@hatena](https://blog.okazuki.jp/entry/2015/06/20/122427)
public class ImageConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        try
        {
            using var fs = new FileStream((string)value, FileMode.Open);
            var decoder = BitmapDecoder.Create(fs, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            var bmp = new WriteableBitmap(decoder.Frames[0]);
            bmp.Freeze();
            return bmp;
        }
        catch { }
        return DependencyProperty.UnsetValue;
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}