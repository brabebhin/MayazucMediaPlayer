using Microsoft.UI.Xaml;
using System;
using System.Threading.Tasks;

namespace MayazucMediaPlayer
{
    public static class UIControlExtensions
    {
        /// <summary>
        /// Waits until the given element resizes
        /// </summary>
        /// <param name="control"></param>
        /// <param name="operationToPerform">Must trigger change in the size of the control</param>
        /// <returns></returns>
        public static async Task WaitForElementSizeChangedAsync(this FrameworkElement control, Func<Task> operationToPerform)
        {
            TaskCompletionSource<bool> sizeChangedHandler = new TaskCompletionSource<bool>();
            SizeChangedEventHandler siceChangedEvent = (s, e) =>
            {
                sizeChangedHandler.TrySetResult(true);
            };
            control.SizeChanged += siceChangedEvent;
            await operationToPerform.Invoke();
            await sizeChangedHandler.Task;
            control.SizeChanged -= siceChangedEvent;
        }

        /// <summary>
        /// Waits until the given element resizes
        /// </summary>
        /// <param name="control"></param>
        /// <param name="operationToPerform">Must trigger change in the size of the control</param>
        /// <returns></returns>
        public static async Task WaitForElementSizeChangedAsync(this FrameworkElement control, Action operationToPerform)
        {
            TaskCompletionSource<bool> sizeChangedHandler = new TaskCompletionSource<bool>();
            SizeChangedEventHandler siceChangedEvent = (s, e) =>
            {
                sizeChangedHandler.TrySetResult(true);
            };
            control.SizeChanged += siceChangedEvent;
            operationToPerform.Invoke();
            await sizeChangedHandler.Task;
            control.SizeChanged -= siceChangedEvent;
        }
    }
}
