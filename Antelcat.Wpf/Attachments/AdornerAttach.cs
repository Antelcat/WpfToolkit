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

    public static HorizontalAlignment GetHorizontalAlignment(DependencyObject d) => (HorizontalAlignment)d.GetValue(HorizontalAlignmentProperty);

    public static void SetHorizontalAlignment(DependencyObject d, HorizontalAlignment value) => d.SetValue(HorizontalAlignmentProperty, value);

    public static readonly DependencyProperty HorizontalAlignmentProperty =
        DependencyProperty.RegisterAttached("HorizontalAlignment",
            typeof(HorizontalAlignment),
            typeof(AdornerAttach),
            new PropertyMetadata(HorizontalAlignment.Stretch, Adorner_OnPropertyChanged));

    public static VerticalAlignment GetVerticalAlignment(DependencyObject d) => (VerticalAlignment)d.GetValue(VerticalAlignmentProperty);

    public static void SetVerticalAlignment(DependencyObject d, VerticalAlignment value) => d.SetValue(VerticalAlignmentProperty, value);

    public static readonly DependencyProperty VerticalAlignmentProperty =
        DependencyProperty.RegisterAttached("VerticalAlignment",
            typeof(VerticalAlignment),
            typeof(AdornerAttach),
            new PropertyMetadata(VerticalAlignment.Stretch, Adorner_OnPropertyChanged));

    private readonly static Dictionary<UIElement, Adorner> AttachedAdorners = [];

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
                UIElement element => new UIElementAdorner(uiElement, element)
                {
                    HorizontalAlignment = GetHorizontalAlignment(uiElement),
                    VerticalAlignment = GetVerticalAlignment(uiElement)
                },
                _ => new ContentPresenterAdorner(uiElement, e.NewValue)
                {
                    HorizontalAlignment = GetHorizontalAlignment(uiElement),
                    VerticalAlignment = GetVerticalAlignment(uiElement)
                }
            };
            adornerLayer.Add(adorner);
            AttachedAdorners.Add(uiElement, adorner);
        }
    }

    private static void Adorner_OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UIElement uiElement) return;
        if (AttachedAdorners.TryGetValue(uiElement, out var adorner))
        {
            if (e.Property == HorizontalAlignmentProperty)
            {
                adorner.HorizontalAlignment = (HorizontalAlignment)e.NewValue;
            } 
            else if (e.Property == VerticalAlignmentProperty)
            {
                adorner.VerticalAlignment = (VerticalAlignment)e.NewValue;
            }
        }
    }
}