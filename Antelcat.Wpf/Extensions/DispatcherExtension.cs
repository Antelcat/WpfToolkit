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
	public static void WaitOnDispatcherFrame(this Task task)
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
	public static TResult WaitOnDispatcherFrame<TResult>(this Task<TResult> task)
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
		
		return result ?? throw new InvalidOperationException("Task result is null");
	}

	/// <summary>
	/// 检查当前是否在UI线程，如果是，直接调用；否则用Dispatcher异步调用
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="dispatcher"></param>
	/// <param name="callback"></param>
	/// <returns></returns>
	public static async ValueTask<T> CheckAndDispatchAsync<T>(this Dispatcher dispatcher, Func<T> callback) {
		if (dispatcher.CheckAccess()) {
			return callback();
		}

		return await dispatcher.InvokeAsync(callback);
	}

	/// <summary>
	/// 检查当前是否在UI线程，如果是，直接调用；否则用Dispatcher异步调用
	/// </summary>
	/// <param name="dispatcher"></param>
	/// <param name="callback"></param>
	/// <returns></returns>
	public static async ValueTask CheckAndDispatchAsync(this Dispatcher dispatcher, Action callback) {
		if (dispatcher.CheckAccess()) {
			callback();
			return;
		}

		await dispatcher.InvokeAsync(callback);
	}

	/// <summary>
	/// 检查当前是否在UI线程，如果是，直接调用；否则用Dispatcher调用
	/// </summary>
	/// <param name="dispatcher"></param>
	/// <param name="callback"></param>
	/// <returns></returns>
	public static void CheckAndDispatch(this Dispatcher dispatcher, Action callback) {
		if (dispatcher.CheckAccess()) {
			callback();
			return;
		}

		dispatcher.Invoke(callback);
	}
}