using System.Linq;
using Zenject;

namespace Core
{
    public class ProjectInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            InstallSignalBus();
            
        }

        public void InstallSignalBus()
        {
            SignalBusInstaller.Install(Container);

            // Declares signals
            

            //[GAME]
            Container.DeclareSignal<CurrencyParticleSignal>().OptionalSubscriber();
            Container.DeclareSignal<DeathPopupDecisionSignal>().OptionalSubscriber();
            Container.DeclareSignal<SpinProgressResetSignal>().OptionalSubscriber();
        }
    }
}
