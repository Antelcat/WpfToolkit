using System;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Interop;
using System.Windows.Media;

namespace Antelcat.Wpf.Extensions;

public static class VisualExtension
{
    internal static VisualStateGroup? TryGetVisualStateGroup(DependencyObject d, string groupName)
    {
        var root = GetImplementationRoot(d);
        if (root == null) return null;

        return VisualStateManager
            .GetVisualStateGroups(root)?
            .OfType<VisualStateGroup>()
            .FirstOrDefault(group => string.CompareOrdinal(groupName, group.Name) == 0);
    }

    internal static FrameworkElement? GetImplementationRoot(DependencyObject d) =>
        1 == VisualTreeHelper.GetChildrenCount(d)
            ? VisualTreeHelper.GetChild(d, 0) as FrameworkElement
            : null;

    public static T? GetChild<T>(this DependencyObject? d) where T : DependencyObject
    {
        switch (d)
        {
            case null:
                return default;
            case T t:
                return t;
        }

        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(d); i++)
        {
            var child = VisualTreeHelper.GetChild(d, i);

            var result = GetChild<T>(child);
            if (result != null) return result;
        }

        return default;
    }

    public static T? GetParent<T>(this DependencyObject? d) where T : DependencyObject =>
        d switch
        {
            null => default,
            T t => t,
            Window => null,
            _ => GetParent<T>(VisualTreeHelper.GetParent(d))
        };

    public static IntPtr GetHandle(this Visual visual) => (PresentationSource.FromVisual(visual) as HwndSource)?.Handle ?? IntPtr.Zero;

    internal static void HitTestVisibleElements(Visual visual, HitTestResultCallback resultCallback, HitTestParameters parameters) =>
        VisualTreeHelper.HitTest(visual, ExcludeNonVisualElements, resultCallback, parameters);

    private static HitTestFilterBehavior ExcludeNonVisualElements(DependencyObject potentialHitTestTarget)
    {
        if (potentialHitTestTarget is not Visual) return HitTestFilterBehavior.ContinueSkipSelfAndChildren;

        if (potentialHitTestTarget is not UIElement uIElement || uIElement is { IsVisible: true, IsEnabled: true })
            return HitTestFilterBehavior.Continue;

        return HitTestFilterBehavior.ContinueSkipSelfAndChildren;
    }

    public static bool IsChildOf(this object child, UIElement parent, UIElement? stopAt = null)
    {
        if (child is DependencyObject ui)
        {
            return ui.IsChildOf(parent, stopAt);
        }

        return false;
    }

    public static bool IsChildOf(this DependencyObject? child, UIElement parent, UIElement? stopAt = null)
    {
        while (child is not null and not Window)
        {
            if (child == stopAt)
            {
                return false;
            }
            if (child == parent)
            {
                return true;
            }

            child = VisualTreeHelper.GetParent(child);
        }
        return false;
    }

    public static TTarget? FindParent<TTarget>(this object child) where TTarget : class
    {
        if (child is DependencyObject ui)
        {
            return ui.FindParent<TTarget>();
        }
        return null;
    }

    public static TTarget? FindParent<TTarget>(this DependencyObject? child) where TTarget : class
    {
        while (child is not null and not Window)
        {
            switch (child)
            {
                case TTarget t:
                    return t;
                case Run run:
                    child = run.Parent;
                    break;
                default:
                    child = VisualTreeHelper.GetParent(child);
                    break;
            }
        }

        return null;
    }

    /// <summary>
    /// 寻找类型为T的UI元素，如果遇到S类型的元素则停止寻找，返回null
    /// </summary>
    /// <typeparam name="TTarget"></typeparam>
    /// <typeparam name="TStop"></typeparam>
    /// <param name="child"></param>
    /// <returns></returns>
    // ReSharper disable once InconsistentNaming
    public static TTarget? FindParent<TTarget, TStop>(this object child) where TTarget : class where TStop : class
    {
        if (child is DependencyObject ui)
        {
            return ui.FindParent<TTarget, TStop>();
        }

        return null;
    }

    /// <summary>
    /// 寻找类型为T的UI元素，如果遇到S类型的元素则停止寻找，返回null
    /// </summary>
    /// <typeparam name="TTarget"></typeparam>
    /// <typeparam name="TStop"></typeparam>
    /// <param name="child"></param>
    /// <returns></returns>
    // ReSharper disable once InconsistentNaming
    public static TTarget? FindParent<TTarget, TStop>(this DependencyObject? child) where TTarget : class where TStop : class
    {
        while (child is not null and not Window)
        {
            switch (child)
            {
                case TStop:
                    return null;
                case TTarget t:
                    return t;
                case Run run:
                    child = run.Parent;
                    break;
                default:
                    child = VisualTreeHelper.GetParent(child);
                    break;
            }
        }

        return null;
    }

    public static TTarget? FindChild<TTarget>(this object parent) where TTarget : class
    {
        if (parent is DependencyObject ui)
        {
            return ui.FindChild<TTarget>();
        }
        return null;
    }

    public static TTarget? FindChild<TTarget>(this DependencyObject? parent) where TTarget : class
    {
        switch (parent)
        {
            case null:
                return null;
            case TTarget t:
                return t;
            default:
                for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
                {
                    var child = VisualTreeHelper.GetChild(parent, i);
                    var result = FindChild<TTarget>(child);
                    if (result != null)
                    {
                        return result;
                    }
                }
                return null;
        }
    }
}