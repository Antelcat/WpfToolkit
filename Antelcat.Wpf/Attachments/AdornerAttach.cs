using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Antelcat.Wpf.Attachments;

/// <summary>
/// 允许直接在xaml中附加装饰器
/// </summary>
public class AdornerAttach
{
    public static object? GetContent(DependencyObject d) => d.GetValue(ContentProperty);

    public static void SetContent(DependencyObject d, object? value) => d.SetValue(ContentProperty, value);

    // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ContentProperty =
        DependencyProperty.RegisterAttached("Content",
            typeof(object),
            typeof(AdornerAttach),
            new PropertyMetadata(null, ContentProperty_OnChanged));

    private static void ContentProperty_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UIElement uiElement) return;
        var adornerLayer = AdornerLayer.GetAdornerLayer(uiElement);
        if (adornerLayer == null) return;
        if (adornerLayer.GetAdorners(uiElement) is { } adorners)
        {
            foreach (var adorner in adorners)
            {
                if (adorner is SimpleAdorner)
                {
                    adornerLayer.Remove(adorner);
                }
            }
        }

        if (e.NewValue != null)
        {
            var adorner = new SimpleAdorner(uiElement, e.NewValue);
            adornerLayer.Add(adorner);
        }
    }

    private class SimpleAdorner : Adorner
    {
        private readonly ContentPresenter contentPresenter = new();
        
        public SimpleAdorner(UIElement adornedElement, object content) : base(adornedElement)
        {
            contentPresenter.Content = content;
            AddVisualChild(contentPresenter);
        }

        protected override Visual GetVisualChild(int index)
        {
            return contentPresenter;
        }

        protected override int VisualChildrenCount => 1;

        protected override Size ArrangeOverride(Size finalSize)
        {
            contentPresenter.Arrange(new Rect(new Point(0, 0), finalSize));
            return finalSize;
        }
    }
}