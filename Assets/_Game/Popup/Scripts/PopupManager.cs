using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace Popup
{
    public class PopupManager : IInitializable, IDisposable
    {
        public event Action<bool, PopupType> OnPopupStateChanged;

        public bool IsPopUpActive;

        private List<IPopup> _popups = new List<IPopup>();
        
        private readonly PopupRepository _repository;
        private readonly DiContainer _container;

        public PopupManager(PopupRepository repository, DiContainer container)
        {
            _repository = repository;
            _container = container;
        }
        
        public void Initialize()
        {
        }

        public void Dispose()
        {
        }

        public void Add(IPopup popup)
        {
            if (_popups.Contains(popup))
            {
                return;
            }
            
            _popups.Add(popup);
        }

        public void Remove(IPopup popup)
        {
            _popups.Remove(popup);
        }
        
        public void Open(IPopup popup, Action onComplete = null)
        {
            if (popup.IsOpen)
            {
                Debug.LogWarning($"Popup {popup.name} is already open.");
                return;
            }

            popup.Open(() =>
            {
                IsPopUpActive = true;
                onComplete?.Invoke();
            });
        }
        
        public void Open(PopupType type,Vector2 screenPos, Action onComplete = null)
        {
            var popup = _popups.FirstOrDefault(p => p.Type == type);
            if (popup == null)
            {
                Debug.LogWarning($"Popup {type} not found.");
                return;
            }
            
            if (popup.IsOpen)
            {
                Debug.LogWarning($"Popup {popup.name} is already open.");
                return;
            }
            
            popup.Open(screenPos,() =>
            {
                IsPopUpActive = true;
                onComplete?.Invoke();
            });
        }
        
        public void Open(PopupType type, Action onComplete = null)
        {
            var popup = _popups.FirstOrDefault(p => p.Type == type);
            if (popup == null)
            {
                Debug.LogWarning($"Popup {type} not found.");
                return;
            }

            Open(popup, onComplete);
        }

        public void Close(IPopup popup, Action onComplete = null)
        {
            popup.Close(() =>
            {
                IsPopUpActive = false;
                onComplete?.Invoke();
            });
        }
        
        public void Close(PopupType type, Action onComplete = null)
        {
            var popup = _popups.FirstOrDefault(p => p.Type == type);
            if (popup == null)
            {
                Debug.LogWarning($"Popup {type} bulunamadı.");
                return;
            }

            Close(popup, onComplete);
        }
        
        public PopupData GetData(PopupType type)
        {
            return _repository.GetData(type);
        }

        public T GetPopUp<T>(PopupType type) where T : IPopup
        {
            T popup = _popups.OfType<T>().FirstOrDefault(existingPopup => existingPopup.Type == type);

            if (popup != null)
            {
                return popup;
            }

            return null;
        }

        public void PopupOpened(PopupType popup)
        {
            OnPopupStateChanged?.Invoke(true, popup);
        }
        public void PopupClosed(PopupType popup)
        {
            OnPopupStateChanged?.Invoke(false, popup);
        }
    }
}
