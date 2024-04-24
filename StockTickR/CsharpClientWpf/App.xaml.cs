using Prism.Ioc;
using Prism.Unity;
using System.Windows;

namespace CsharpClientWpfFramework
{
    public partial class App : PrismApplication
    {
        protected override Window CreateShell()
        {
            return new MainWindow();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<ISignalRClientService, SignalRClientService>();
            // creates the instance right away
            // the alternative is to use RegisterInstance(), but this requires all the dependencies to be set manually
            Container.Resolve<ISignalRClientService>();
        }
    }
}
