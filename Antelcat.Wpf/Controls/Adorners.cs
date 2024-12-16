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
        uiElement.Measure(constraint);
        return uiElement.DesiredSize;
    }
}

public class ContentPresenterAdorner(UIElement adornedElement, object content)
    : UIElementAdorner(adornedElement, new ContentPresenter { Content = content });