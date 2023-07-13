using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Xaml.Behaviors;

namespace Qt6uox4f1iujybm;


// 動かしたい本体（例ではRectangle）にアタッチ
// ItemsPanelがCanvasのListBoxに置かれるという前提
public class ResizeBehavior : Behavior<FrameworkElement>
{
    // Thumbが置いてあるControlTemplate
    public ControlTemplate? AdornerTemplate { get; set; }

    // ↑を実体化したControlを持つAdorner
    private ResizeAdorner adorner = null!;
    // アタッチ元（AssociatedObject）を包含するListBoxItem
    private ListBoxItem listBoxItem = null!;
    // アタッチ元（AssociatedObject）が置かれているCanvas
    private Canvas canvas = null!;

    private Point topLeft;     // ドラッグ開始時の左上座標（Canvas基準）
    private Point bottomRight; // ドラッグ開始時の右下座標（Canvas基準）
    private Point offset;      // ドラッグ開始時のクリックした座標（ListBoxItem基準）

    protected override void OnAttached() => AssociatedObject.Loaded += Loaded;
    protected override void OnDetaching()
    {
        AssociatedObject.Loaded -= Loaded;

        listBoxItem.Selected -= ListBoxItem_Selected;
        listBoxItem.Unselected -= ListBoxItem_Unselected;

        adorner.RemoveHandler(Thumb.DragStartedEvent, (DragStartedEventHandler)Thumb_DragStarted);
        adorner.RemoveHandler(Thumb.DragDeltaEvent, (DragDeltaEventHandler)Thumb_DragDelta);
    }

    private void Loaded(object sender, RoutedEventArgs e)
    {
        var container = AssociatedObject;
        while (true)
        {
            var parent = VisualTreeHelper.GetParent(container) as FrameworkElement ?? throw new InvalidOperationException("Canvas not found.");
            if (parent is Canvas c)
            {
                canvas = c;
                break;
            }
            container = parent;
        }

        listBoxItem = container as ListBoxItem ?? throw new InvalidOperationException("ListBoxItem not found.");
        listBoxItem.Selected += ListBoxItem_Selected;
        listBoxItem.Unselected += ListBoxItem_Unselected;

        adorner = new ResizeAdorner(AssociatedObject, AdornerTemplate);
        // Thumbのイベントは親でまとめてハンドルする
        adorner.AddHandler(Thumb.DragStartedEvent, (DragStartedEventHandler)Thumb_DragStarted);
        adorner.AddHandler(Thumb.DragDeltaEvent, (DragDeltaEventHandler)Thumb_DragDelta);

        if (listBoxItem.IsSelected)
            AdornerLayer.GetAdornerLayer(AssociatedObject).Add(adorner);
    }


    // 選択時にAdorner追加（= 移動・リサイズ可能）
    private void ListBoxItem_Selected(object sender, RoutedEventArgs e)
        => AdornerLayer.GetAdornerLayer(AssociatedObject).Add(adorner);
    // 選択解除時にAdorner削除
    private void ListBoxItem_Unselected(object sender, RoutedEventArgs e)
        => AdornerLayer.GetAdornerLayer(AssociatedObject)?.Remove(adorner);

    private void Thumb_DragStarted(object sender, DragStartedEventArgs e)
    {
        // ドラッグ開始時の各点保存
        topLeft = new Point(Canvas.GetLeft(listBoxItem), Canvas.GetTop(listBoxItem));
        bottomRight = topLeft + new Vector(AssociatedObject.Width, AssociatedObject.Height);
        offset = Mouse.GetPosition(listBoxItem);
    }
    private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
    {
        if (e.OriginalSource is Thumb thumb)
        {
            var p = Mouse.GetPosition(canvas);

            // 移動
            if (thumb.HorizontalAlignment == HorizontalAlignment.Stretch && thumb.VerticalAlignment == VerticalAlignment.Stretch)
            {
                // offset分戻して左上座標設定
                var pp = p - offset;
                Canvas.SetLeft(listBoxItem, pp.X);
                Canvas.SetTop(listBoxItem, pp.Y);
                return;
            }

            // リサイズ
            // X（左右）について
            // 例えばAlignment.LeftならbottomRight.Xは動かさずもう一方だけ動かせばいい
            // 仮にbottomRight.Xより大きくなって（左辺を持って右辺より右に行く）
            // Widthがマイナスになるとしても後で正規化するので問題ない
            var (x1, x2) = thumb.HorizontalAlignment switch
            {
                HorizontalAlignment.Left => (p.X, bottomRight.X),
                HorizontalAlignment.Right => (topLeft.X, p.X),
                _ => (topLeft.X, bottomRight.X),
            };
            // Y（上下）について 同上
            var (y1, y2) = thumb.VerticalAlignment switch
            {
                VerticalAlignment.Top => (p.Y, bottomRight.Y),
                VerticalAlignment.Bottom => (topLeft.Y, p.Y),
                _ => (topLeft.Y, bottomRight.Y),
            };

            // 正規化はRectがやってくれる
            var rect = new Rect(new Point(x1, y1), new Point(x2, y2));
            Canvas.SetLeft(listBoxItem, rect.X);
            Canvas.SetTop(listBoxItem, rect.Y);
            AssociatedObject.Width = rect.Width;
            AssociatedObject.Height = rect.Height;
        }
    }

    // [リサイズハンドルをAdornerで実装する - CoMoの日記](https://como-2.hatenadiary.org/entry/20110428/1303996288)
    private class ResizeAdorner : Adorner
    {
        private readonly Control content;

        public ResizeAdorner(UIElement adornedElement, ControlTemplate? template) : base(adornedElement)
        {
            content = new() { Template = template, };
            AddVisualChild(content);
            AddLogicalChild(content);
        }

        protected override Size MeasureOverride(Size constraint) => AdornedElement.DesiredSize;
        protected override Size ArrangeOverride(Size finalSize)
        {
            content.Arrange(new Rect(AdornedElement.DesiredSize));
            return AdornedElement.DesiredSize;
        }
        protected override int VisualChildrenCount => 1;
        protected override Visual GetVisualChild(int index) => content;
    }
}
