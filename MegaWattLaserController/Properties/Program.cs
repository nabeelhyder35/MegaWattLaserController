using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using System;
using System.Threading;

namespace LaserControllerApp
{
    public static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            WinRT.ComWrappersSupport.InitializeComWrappers();
            bool isSingleInstanced = false;

            if (isSingleInstanced)
            {
                var activationArgs = AppInstance.GetCurrent().GetActivatedEventArgs();
                var keyInstance = AppInstance.FindOrRegisterForKey("LaserControllerApp");

                if (!keyInstance.IsCurrent)
                {
                    keyInstance.RedirectActivationToAsync(activationArgs).GetAwaiter().GetResult();
                    return;
                }
            }

            Application.Start((p) =>
            {
                var context = new DispatcherQueueSynchronizationContext(
                    DispatcherQueue.GetForCurrentThread());
                SynchronizationContext.SetSynchronizationContext(context);
                new App();
            });
        }
    }
}