using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Antelcat.Wpf.Controls;

public class VisualAdorner(UIElement adornedElement, Visual visual) : Adorner(adornedElement)
{
    protected override int VisualChildrenCount => 1;

    protected override Visual GetVisualChild(int index) => visual;
}

public class UIElementAdorner(UIElement adornedElement, UIElement uiElement) : VisualAdorner(adornedElement, uiElement)
{
    protected override Size ArrangeOverride(Size finalSize)
    {
        uiElement.Arrange(new Rect(finalSize));
        return finalSize;
    }

    protected override Size MeasureOverride(Size constraint)
    {
        var desiredWidth = HorizontalAlignment switch
        {
            HorizontalAlignment.Stretch => constraint.Width,
            HorizontalAlignment.Left => uiElement.DesiredSize.Width,
            HorizontalAlignment.Right => uiElement.DesiredSize.Width,
            HorizontalAlignment.Center => uiElement.DesiredSize.Width,
            _ => 0
        };
        var desiredHeight = VerticalAlignment switch
        {
            VerticalAlignment.Stretch => constraint.Height,
            VerticalAlignment.Top => uiElement.DesiredSize.Height,
            VerticalAlignment.Bottom => uiElement.DesiredSize.Height,
            VerticalAlignment.Center => uiElement.DesiredSize.Height,
            _ => 0
        };
        uiElement.Measure(new Size(desiredWidth, desiredHeight));
        return new Size(desiredWidth, desiredHeight);
    }
}

public class ContentPresenterAdorner(UIElement adornedElement, object content)
    : UIElementAdorner(adornedElement, new ContentPresenter { Content = content });