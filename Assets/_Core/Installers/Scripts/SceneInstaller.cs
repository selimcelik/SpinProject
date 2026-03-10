//using Popup;
using Zenject;

namespace Core
{
    public class SceneInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            InstallSignalBus();
            
            // Configure bindings
            #if SEMRUK_SR_DEBUGGER_ENABLED
            Container.QueueForInject(SRDependencyManager.Instance);
            #endif
            
            //Container.BindInterfacesAndSelfTo<PopupManager>().AsSingle().NonLazy();
        }

        private void InstallSignalBus()
        {
            // Declares signals
        }
    }
}
