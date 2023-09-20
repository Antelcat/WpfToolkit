using System;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Antelcat.Wpf.Extensions;

public static class DispatcherExtension
{
	/// <summary>
	/// 在指定的Dispatcher上运行一个Task，等待其完成，避免阻塞UI线程
	/// </summary>
	/// <param name="dispatcher"></param>
	/// <param name="task"></param>
	/// <typeparam name="TResult"></typeparam>
	/// <returns></returns>
	/// <exception cref="AggregateException"></exception>
	public static TResult? RunTask<TResult>(this Dispatcher dispatcher, Task<TResult> task)
	{
		var frame = new DispatcherFrame();
		TResult? result = default;
		Exception? capturedException = null;
		
		dispatcher.BeginInvoke(async () =>
		{
			try
			{
				result = await task;
			}
			catch (Exception e)
			{
				capturedException = e;
			}
			finally
			{
				frame.Continue = false; // 结束消息循环
			}
		});
		
		Dispatcher.PushFrame(frame);
		
		if (capturedException is not null)
		{
			throw new AggregateException(capturedException);
		}
		
		return result;
	}
}