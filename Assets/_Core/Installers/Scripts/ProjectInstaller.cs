using System.Linq;
using Zenject;

namespace Core
{
    public class ProjectInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            InstallSignalBus();
            
            // Configure bindings
            //Container.BindInterfacesAndSelfTo<CurrencyManager>().AsSingle().NonLazy();
            
        }

        public void InstallSignalBus()
        {
            SignalBusInstaller.Install(Container);

            // Declares signals
            

            //[GAME]
            //Container.DeclareSignal<CurrencyChangedSignal>().OptionalSubscriber();
        }
    }
}