using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Qt6uox4f1iujybm;

public partial class RectItem : ObservableObject
{
    [ObservableProperty] private double x;
    [ObservableProperty] private double y;
    [ObservableProperty] private double width;
    [ObservableProperty] private double height;

    [ObservableProperty] private double strokeThickness = 3;
    [ObservableProperty] private Brush stroke = Brushes.Red;
    [ObservableProperty] private Brush fill = Brushes.Transparent;

    public RectItem() { }
    public RectItem(Rect rect)
        => (X, Y, Width, Height) = (rect.X, rect.Y, rect.Width, rect.Height);
}
