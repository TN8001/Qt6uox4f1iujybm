using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Xaml.Behaviors;

namespace Qt6uox4f1iujybm;


// ドラッグでラバーバンド（範囲選択するような四角の枠）を表示するビヘイビア
public class RubberBandBehavior : Behavior<Canvas>
{
    // ドラッグ終了時に選択範囲を通知するイベント
    public event NewRectEventHandler? NewRect;

    //  RoutedEventじゃないと→が出る IDE0051	プライベート メンバー 'MainWindow.RubberBandBehavior_NewRect' は使用されていません
    //public event EventHandler<Rect>? NewRect;

    private RubberBandAdorner adorner = null!;
    private bool isMouseDown;

    protected override void OnAttached()
    {
        AssociatedObject.Loaded += Loaded;
        AssociatedObject.MouseLeftButtonDown += MouseLeftButtonDown;
        AssociatedObject.MouseMove += MouseMove;
        AssociatedObject.MouseLeftButtonUp += MouseLeftButtonUp;
    }
    protected override void OnDetaching()
    {
        AssociatedObject.Loaded -= Loaded;
        AssociatedObject.MouseLeftButtonDown -= MouseLeftButtonDown;
        AssociatedObject.MouseMove -= MouseMove;
        AssociatedObject.MouseLeftButtonUp -= MouseLeftButtonUp;

        AdornerLayer.GetAdornerLayer(AssociatedObject)?.Remove(adorner);
    }

    private void Loaded(object sender, RoutedEventArgs e)
    {
        adorner = new RubberBandAdorner(AssociatedObject);
        AdornerLayer.GetAdornerLayer(AssociatedObject).Add(adorner);

        if (AssociatedObject.Background == null)
            AssociatedObject.Background = Brushes.Transparent;
    }

    private void MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        AssociatedObject.CaptureMouse();

        var p = e.GetPosition(AssociatedObject);
        adorner.StartPoint = adorner.EndPoint = p;

        isMouseDown = true;
    }
    private void MouseMove(object sender, MouseEventArgs e)
    {
        if (isMouseDown) adorner.EndPoint = e.GetPosition(AssociatedObject);
    }
    private void MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (isMouseDown)
        {
            AssociatedObject.ReleaseMouseCapture();
            isMouseDown = false;

            var args = new NewRectEventArgs(this)
            {
                Rect = new(adorner.StartPoint, adorner.EndPoint),
                RoutedEvent = RubberBandAdorner.NewRectEvent,
                Source = AssociatedObject,
            };
            NewRect?.Invoke(this, args);

            adorner.StartPoint = adorner.EndPoint = default;
        }
    }

    private class RubberBandAdorner : Adorner
    {
        public static readonly RoutedEvent NewRectEvent
            = EventManager.RegisterRoutedEvent(nameof(NewRect), RoutingStrategy.Bubble,
                typeof(NewRectEventHandler), typeof(RubberBandAdorner));


        // 始点（ドラッグ開始点）
        public Point StartPoint { get => (Point)GetValue(StartPointProperty); set => SetValue(StartPointProperty, value); }
        public static readonly DependencyProperty StartPointProperty
            = DependencyProperty.Register(nameof(StartPoint), typeof(Point), typeof(RubberBandAdorner),
                new FrameworkPropertyMetadata(default(Point), FrameworkPropertyMetadataOptions.AffectsRender));

        // 終点（ドラッグ現在点）
        public Point EndPoint { get => (Point)GetValue(EndPointProperty); set => SetValue(EndPointProperty, value); }
        public static readonly DependencyProperty EndPointProperty
            = DependencyProperty.Register(nameof(EndPoint), typeof(Point), typeof(RubberBandAdorner),
                new FrameworkPropertyMetadata(default(Point), FrameworkPropertyMetadataOptions.AffectsRender));

        public RubberBandAdorner(UIElement adornedElement) : base(adornedElement) { }

        private readonly Pen pen = new(new SolidColorBrush(Colors.Black) { Opacity = 0.3, }, 1);
        private readonly Brush brush = new SolidColorBrush(Colors.SkyBlue) { Opacity = 0.3, };

        // StartPointかEndPointが変更されると呼ばれる（FrameworkPropertyMetadataOptions.AffectsRender）
        protected override void OnRender(DrawingContext drawingContext)
            => drawingContext.DrawRectangle(brush, pen, new(StartPoint, EndPoint));
    }
}

public class NewRectEventArgs : RoutedEventArgs
{
    public Rect Rect { get; init; }
    public NewRectEventArgs(object source) : base(null, source) { }
}
public delegate void NewRectEventHandler(object sender, NewRectEventArgs e);
