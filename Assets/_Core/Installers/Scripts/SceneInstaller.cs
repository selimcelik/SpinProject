using Popup;
using Zenject;

namespace Core
{
    public class SceneInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            InstallSignalBus();
            
            // Configure bindings
            
            Container.BindInterfacesAndSelfTo<PopupManager>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<ItemManager>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<SpinManager>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<CurrencyManager>().AsSingle().NonLazy();
        }

        private void InstallSignalBus()
        {
            // Declares signals
        }
    }
}
