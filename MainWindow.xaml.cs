using System;
using System.IO;
using System.Windows;

namespace Qt6uox4f1iujybm;


public partial class MainWindow : Window
{
    private ViewModel Vm => (ViewModel)DataContext;

    public MainWindow()
    {
        InitializeComponent();

        var imageFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "image");
        DataContext = new ViewModel
        {
            Pict = new(imageFolder),
            Rect = new(),
        };
    }

    private void OpenRectListWindow_Click(object sender, RoutedEventArgs e)
        => new RectListWindow { DataContext = Vm.Rect, Owner = this, }.Show();
    // Owner付きだとMainWindowの下にならない
    //=> new RectListWindow { DataContext = Vm.Rect, }.Show();

    private void OpenSelectPictDialog_Click(object sender, RoutedEventArgs e)
        => new SelectPictDialog { DataContext = Vm.Pict, Owner = this, }.ShowDialog();

    private void RubberBandBehavior_NewRect(object sender, NewRectEventArgs e)
        => Vm.Rect?.AddItem(e.Rect);
}
