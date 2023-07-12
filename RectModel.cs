using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Qt6uox4f1iujybm;


public partial class RectModel : ObservableObject
{
    public ObservableCollection<RectItem> Items { get; } = new();
    [RelayCommand] private void Delete(RectItem item) => Items.Remove(item);

    public void AddItem(Rect rect)
    {
        if (10 < rect.Width && 10 < rect.Height)
            Items.Add(new(rect));
    }
}
