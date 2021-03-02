using HEVCDemo.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace HEVCDemo
{
    public class MainModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var region = containerProvider.Resolve<IRegionManager>();
            region.RegisterViewWithRegion("ContentRegion", typeof(ImagesViewer));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
        }
    }
}
