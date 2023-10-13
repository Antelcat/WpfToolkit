using System.Windows.Controls;
using System.Windows.Threading;

namespace Antelcat.Wpf.Extensions;

public static class AsyncExtension {
    /// <summary>
    /// 等待按钮点击
    /// </summary>
    /// <param name="button"></param>
    /// <returns></returns>
    public static void WaitForClick(this Button button) {
        var frame = new DispatcherFrame();

        void ContinueFrame(object? sender, object? args) {
            button.Unloaded -= ContinueFrame;
            button.Click -= ContinueFrame;
            frame.Continue = false;
        }

        button.Unloaded += ContinueFrame;
        button.Click += ContinueFrame;
        Dispatcher.PushFrame(frame);
    }
}