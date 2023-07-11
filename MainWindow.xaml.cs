using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Qt6uox4f1iujybm;


public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var imageFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "image");
        DataContext = new ViewModel { Pict = new(imageFolder), };
    }

    private void OpenSelectPictDialog_Click(object sender, RoutedEventArgs e)
        => new SelectPictDialog { DataContext = DataContext, Owner = this, }.ShowDialog();
}
