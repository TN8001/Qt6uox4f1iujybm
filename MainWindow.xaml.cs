using System;
using System.IO;
using System.Windows;

namespace Qt6uox4f1iujybm;


public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var imageFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "image");
        DataContext = new ViewModel(imageFolder);
    }

    private void OpenSelectDialogButton_Click(object sender, RoutedEventArgs e)
        => new SelectDialog((ViewModel)DataContext) { Owner = this, }.ShowDialog();
}
