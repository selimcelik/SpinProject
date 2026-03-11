using Popup;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace Core
{
    [CreateAssetMenu(fileName = "Repository Installer", menuName = "Core/System/Installer/Repository Installer")]
    public class RepositoryInstaller : ScriptableObjectInstaller<RepositoryInstaller>
    {
        [SerializeField] private PopupRepository _popupRepository;
        [SerializeField] private ItemRepository _itemRepository;
        [SerializeField] private SpinRepository _spinRepository;
        [SerializeField] private SpinWaveData _spinWaveData;

        public override void InstallBindings()
        {
            // Configure bindings
            Container.Bind<PopupRepository>().FromScriptableObject(_popupRepository).AsSingle();
            Container.Bind<ItemRepository>().FromScriptableObject(_itemRepository).AsSingle();
            Container.Bind<SpinRepository>().FromScriptableObject(_spinRepository).AsSingle();
            Container.Bind<SpinWaveData>().FromScriptableObject(_spinWaveData).AsSingle();
        }
    }
}
