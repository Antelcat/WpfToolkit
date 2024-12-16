using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using Antelcat.Wpf.Controls;

namespace Antelcat.Wpf.Attachments;

/// <summary>
/// 允许直接在xaml中附加装饰器
/// </summary>
public class AdornerAttach
{
    public static object? GetContent(DependencyObject d) => d.GetValue(ContentProperty);

    public static void SetContent(DependencyObject d, object? value) => d.SetValue(ContentProperty, value);

    public static readonly DependencyProperty ContentProperty =
        DependencyProperty.RegisterAttached("Content",
            typeof(object),
            typeof(AdornerAttach),
            new PropertyMetadata(null, ContentProperty_OnChanged));
    
    private readonly static Dictionary<UIElement, Adorner> AttachedAdorners = new();

    private static void ContentProperty_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UIElement uiElement) return;
        var adornerLayer = AdornerLayer.GetAdornerLayer(uiElement);
        if (adornerLayer == null) return;
        
        if (e.OldValue != null && AttachedAdorners.TryGetValue(uiElement, out var adorner))
        {
            adornerLayer.Remove(adorner);
            AttachedAdorners.Remove(uiElement);
        }

        if (e.NewValue != null)
        {
            adorner = e.NewValue switch
            {
                UIElement element => new UIElementAdorner(uiElement, element),
                _ => new ContentPresenterAdorner(uiElement, e.NewValue)
            };
            adornerLayer.Add(adorner);
            AttachedAdorners.Add(uiElement, adorner);
        }
    }
}