using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Antelcat.Wpf.Utils;

public static class BetterScrollViewer
{
	/// <summary>
	/// 使所有ScrollViewer在触底或触顶的时候滚动，会向上冒泡（不阻止父控件滚动）
	/// </summary>
	public static void MakeScrollEventBubbleWhenReachLimit()
	{
		EventManager.RegisterClassHandler(typeof(ScrollViewer),
			UIElement.MouseWheelEvent,
			new MouseWheelEventHandler(ScrollViewer_OnMouseWheel));
	}

	private static void ScrollViewer_OnMouseWheel(object sender, MouseWheelEventArgs e)
	{
		if (e.Handled)
		{
			return;
		}

		var scrollViewer = (ScrollViewer)sender;
		if (scrollViewer.ViewportHeight < scrollViewer.ExtentHeight)
		{
			// 如果内容不足以滚动，那么一定是触顶或者触底，冒泡
			if (e.Delta > 0)
			{
				if (scrollViewer.VerticalOffset > 0)
				{
					return;
				}
			}
			else
			{
				if (scrollViewer.VerticalOffset + scrollViewer.ViewportHeight < scrollViewer.ExtentHeight)
				{
					return;
				}
			}
		}
		
		// 如果触底或触顶，冒泡
		e.Handled = true;

		if (scrollViewer.Parent is UIElement parent)
		{
			var e2 = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
				{ RoutedEvent = UIElement.MouseWheelEvent };
			parent.RaiseEvent(e2);
		}
	}
}