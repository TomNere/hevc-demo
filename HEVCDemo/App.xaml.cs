using HEVCDemo.Views;
using Prism.Ioc;
using Prism.Modularity;
using Rasyidf.Localization;
using System.Windows;

namespace HEVCDemo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {

        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            base.ConfigureModuleCatalog(moduleCatalog);
            moduleCatalog.AddModule<MainModule>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            // Set the language packs folder and default language
            LocalizationService.Current.Initialize();
            base.OnStartup(e);
        }
    }
}
