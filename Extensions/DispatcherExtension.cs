using System;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Antelcat.Wpf.Extensions;

public static class DispatcherExtension
{
	/// <summary>
	/// 在指定的DispatcherFrame上运行一个Task，等待其完成，避免阻塞UI线程
	/// </summary>
	/// <param name="task"></param>
	/// <returns></returns>
	/// <exception cref="AggregateException"></exception>
	public static void AwaitOnDispatcherFrame(this Task task)
	{
		var                 frame             = new DispatcherFrame();
		AggregateException? capturedException = null;
		
		task.ContinueWith(t =>
		{
			capturedException = t.Exception;
			frame.Continue    = false; // 结束消息循环
		}, TaskContinuationOptions.AttachedToParent);
		
		Dispatcher.PushFrame(frame);
		
		if (capturedException != null)
		{
			throw capturedException;
		}
	}	
	
	/// <summary>
	/// 在指定的DispatcherFrame上运行一个Task，等待其完成，避免阻塞UI线程
	/// </summary>
	/// <param name="task"></param>
	/// <typeparam name="TResult"></typeparam>
	/// <returns></returns>
	/// <exception cref="AggregateException"></exception>
	public static TResult? AwaitOnDispatcherFrame<TResult>(this Task<TResult> task)
	{
		var frame = new DispatcherFrame();
		
		TResult? result = default;
		
		AggregateException? capturedException = null;
		
		task.ContinueWith(t =>
		{
			capturedException = t.Exception;
			result            = t.Result;
			frame.Continue    = false; // 结束消息循环
		}, TaskContinuationOptions.AttachedToParent);
		
		Dispatcher.PushFrame(frame);
		
		if (capturedException != null)
		{
			throw capturedException;
		}
		
		return result;
	}
}