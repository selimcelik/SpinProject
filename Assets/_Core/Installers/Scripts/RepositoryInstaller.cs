//using Popup;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace Core
{
    [CreateAssetMenu(fileName = "Repository Installer", menuName = "Core/System/Installer/Repository Installer")]
    public class RepositoryInstaller : ScriptableObjectInstaller<RepositoryInstaller>
    {
        //[SerializeField] private PopupRepository _popupRepository;

        public override void InstallBindings()
        {
            // Configure bindings
            //Container.Bind<PopupRepository>().FromScriptableObject(_popupRepository).AsSingle();
        }
    }
}