using System;
using System.IO;
using System.Windows;

namespace Qt6uox4f1iujybm;


public partial class MainWindow : Window
{
    private readonly SubWindow subWindow; // GCよけ

    public MainWindow()
    {
        InitializeComponent();

        var imageFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "image");
        DataContext = new ViewModel(imageFolder);

        subWindow = new((ViewModel)DataContext);
    }

    private void Button_Click(object sender, RoutedEventArgs e)
        => new SelectDialog((ViewModel)DataContext).ShowDialog();
}
