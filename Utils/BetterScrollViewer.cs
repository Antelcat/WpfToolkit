using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;

namespace Antelcat.Wpf.Utils; 

public static class BetterScrollViewer {
	/// <summary>
	/// 使所有ScrollViewer在触底或触顶的时候滚动，会向上冒泡（不阻止父控件滚动）
	/// </summary>
	public static void MakeScrollEventBubbleWhenReachLimit() {
		EventManager.RegisterClassHandler(typeof(ScrollViewer), UIElement.PreviewMouseWheelEvent, new MouseWheelEventHandler(ScrollViewer_OnPreviewMouseWheel));
	}

	private static void ScrollViewer_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e) {
		if (e.Handled) {
			return;
		}

		var scrollViewer = (ScrollViewer)sender;
		if (scrollViewer.ViewportHeight < scrollViewer.ScrollableHeight) {
			if (e.Delta > 0) {
				if (scrollViewer.VerticalOffset > 0) {
					return;
				}
			} else {
				if (scrollViewer.VerticalOffset + scrollViewer.ViewportHeight < scrollViewer.ScrollableHeight) {
					return;
				}
			}
		}

		// 如果触底或触顶，冒泡
		e.Handled = true;
		var e2 = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta) { RoutedEvent = UIElement.MouseWheelEvent };
		scrollViewer.RaiseEvent(e2);
	}
}