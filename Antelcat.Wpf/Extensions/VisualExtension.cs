using System;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace Antelcat.Wpf.Extensions;

public static class VisualExtension
{
    public static VisualStateGroup? TryGetVisualStateGroup(DependencyObject d, string groupName)
    {
        var root = GetImplementationRoot(d);
        if (root == null) return null;

        return VisualStateManager
            .GetVisualStateGroups(root)?
            .OfType<VisualStateGroup>()
            .FirstOrDefault(group => string.CompareOrdinal(groupName, group.Name) == 0);
    }

    public static FrameworkElement? GetImplementationRoot(DependencyObject d) =>
        1 == VisualTreeHelper.GetChildrenCount(d)
            ? VisualTreeHelper.GetChild(d, 0) as FrameworkElement
            : null;

    public static IntPtr GetHandle(this Visual visual) => (PresentationSource.FromVisual(visual) as HwndSource)?.Handle ?? IntPtr.Zero;

    public static IntPtr GetHandle(this Window window) => new WindowInteropHelper(window).EnsureHandle();

    internal static void HitTestVisibleElements(Visual visual, HitTestResultCallback resultCallback, HitTestParameters parameters) =>
        VisualTreeHelper.HitTest(visual, ExcludeNonVisualElements, resultCallback, parameters);

    private static HitTestFilterBehavior ExcludeNonVisualElements(DependencyObject potentialHitTestTarget)
    {
        if (potentialHitTestTarget is not Visual) return HitTestFilterBehavior.ContinueSkipSelfAndChildren;

        if (potentialHitTestTarget is not UIElement uIElement || uIElement is { IsVisible: true, IsEnabled: true })
            return HitTestFilterBehavior.Continue;

        return HitTestFilterBehavior.ContinueSkipSelfAndChildren;
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

    public static TTarget? FindParent<TTarget>(this DependencyObject? child, string? targetName = null) where TTarget : DependencyObject
    {
        while (child is not null and not Window)
        {
            switch (child)
            {
                case TTarget t when targetName is null || t is FrameworkElement fe && fe.Name == targetName:
                {
                    return t;
                }
                case FrameworkContentElement fce:
                {
                    child = fce.Parent;
                    break;
                }
                default:
                {
                    child = VisualTreeHelper.GetParent(child);
                    break;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// 寻找类型为TTarget的UI元素，如果遇到TStop类型的元素则停止寻找，返回null
    /// </summary>
    /// <typeparam name="TTarget"></typeparam>
    /// <typeparam name="TStopAt"></typeparam>
    /// <param name="child"></param>
    /// <param name="targetName"></param>
    /// <returns></returns>
    // ReSharper disable once InconsistentNaming
    public static TTarget? FindParent<TTarget, TStopAt>(this DependencyObject? child, string? targetName = null)
        where TTarget : DependencyObject where TStopAt : DependencyObject
    {
        while (child is not null and not Window)
        {
            switch (child)
            {
                case TStopAt:
                {
                    return null;
                }
                case TTarget t when targetName is null || t is FrameworkElement fe && fe.Name == targetName:
                {
                    return t;
                }
                case FrameworkContentElement fce:
                {
                    child = fce.Parent;
                    break;
                }
                default:
                {
                    child = VisualTreeHelper.GetParent(child);
                    break;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// 递归寻找子元素，深度优先
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="targetName"></param>
    /// <typeparam name="TTarget"></typeparam>
    /// <returns></returns>
    public static TTarget? FindChild<TTarget>(this DependencyObject? parent, string? targetName = null) where TTarget : DependencyObject
    {
        switch (parent)
        {
            case null:
            {
                return null;
            }
            case TTarget t when targetName is null || t is FrameworkElement fe && fe.Name == targetName:
            {
                return t;
            }
            default:
            {
                for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
                {
                    var child = VisualTreeHelper.GetChild(parent, i);
                    var result = FindChild<TTarget>(child, targetName);
                    if (result != null)
                    {
                        return result;
                    }
                }
                return null;
            }
        }
    }
}