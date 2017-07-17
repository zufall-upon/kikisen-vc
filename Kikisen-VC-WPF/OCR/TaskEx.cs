using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Kikisen_VC_WPF.OCR
{
	/// <summary>
	/// WinRT を await/async するために
	/// refer to http://blog.xin9le.net/entry/2012/11/12/123231
	/// </summary>
	public static class TaskEx
	{
		public static Task<T> AsTask<T>(this IAsyncOperation<T> operation) {
			var tcs = new TaskCompletionSource<T>();
			operation.Completed = delegate  //--- コールバックを設定
			{
				switch (operation.Status)   //--- 状態に合わせて完了通知
			{
					case AsyncStatus.Completed: tcs.SetResult(operation.GetResults()); break;
					case AsyncStatus.Error: tcs.SetException(operation.ErrorCode); break;
					case AsyncStatus.Canceled: tcs.SetCanceled(); break;
				}
			};
			return tcs.Task;  //--- 完了が通知されるTaskを返す
		}
		public static TaskAwaiter<T> GetAwaiter<T>(this IAsyncOperation<T> operation) {
			return operation.AsTask().GetAwaiter();
		}
	}
}
