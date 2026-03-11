using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Popup
{
    public abstract class IPopup : MonoBehaviour
    {
        public PopupType Type => GetPopupType();
        public bool IsOpen { get; private set; }

        [Header("VALUES")] 
        [SerializeField] private PopupType _type;

        [Header("COMPONENTS")] 
        [SerializeField] [Required] private Transform _popupRootContainer;
        [SerializeField] [Required] private Transform _popupContainer;
        [SerializeField] protected CanvasGroup _canvasGroup;
        [SerializeField] private Button _closeButton;
        [SerializeField] private RectTransform _content;
        [SerializeField] private ContentSizeFitter _contentSizeFitter;

        [Header("READONLY")] 
        [ShowInInspector] [ReadOnly] private PopupData _data;
        [ShowInInspector] [ReadOnly] private PopupAnimationData _animationData;

        private bool _isInit;
        private Sequence _sequence;

        protected PopupManager _manager;
        protected SignalBus _signalBus;

        [Inject]
        protected virtual void Construct(PopupManager manager,SignalBus signalBus)
        {
            _manager = manager;
            _signalBus = signalBus;
            _manager.Add(this);
            _data = _manager.GetData(Type);
            _animationData = _data.animationData;
            if (_closeButton != null)
            {
                _closeButton.onClick.AddListener(OnCloseStart);
            }

            OnConstruct();
        }

        private void Start()
        {
            if (_contentSizeFitter != null) _contentSizeFitter.enabled = false;
        }
        

        private void Init()
        {
            if (_isInit)
            {
                return;
            }

            if (!_popupRootContainer)
            {
                Debug.LogError($"PopupContainer is not assigned for {name}. Assign it in the inspector.");
            }

            OnInit();
            _isInit = true;
        }

        public virtual void Awake()
        {
            Init();
        }

        [Button]
        public void Open(Action onComplete = null)
        {
            Init();
            gameObject.SetActive(true);
            BringToFront();
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;

            if (_sequence.IsActive())
            {
                _sequence.Kill(true);
            }

            _popupContainer.transform.localScale = _animationData.startScale;
            _sequence = DOTween.Sequence();
            _sequence.Append(_canvasGroup.DOFade(1f, 0.25f).SetEase(Ease.Linear))
                .Append(
                    _popupContainer.transform.DOScale(_animationData.endScale, _animationData.openDuration).SetEase(_animationData.openEase).OnComplete(
                        () =>
                        {
                            if (_contentSizeFitter != null)
                            {
                                _contentSizeFitter.enabled = true;
                            }
                            OnShow();
                            onComplete?.Invoke();
                        }));
            IsOpen = true;
            _manager.PopupOpened(Type);
        }

        public void Open(Vector2 screenPos, Action onComplete = null)
        {
            Init();
            transform.position = screenPos;
            gameObject.SetActive(true);
            BringToFront();
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;

            if (_sequence.IsActive())
            {
                _sequence.Kill(true);
            }

            _popupContainer.transform.localScale = _animationData.startScale;
            _sequence = DOTween.Sequence();
            _sequence.Append(_canvasGroup.DOFade(1f, 0.25f).SetEase(Ease.Linear))
                .Append(
                    _popupContainer.transform.DOScale(_animationData.endScale, _animationData.openDuration).SetEase(_animationData.openEase).OnComplete(
                        () =>
                        {
                            if (_contentSizeFitter != null)
                            {
                                _contentSizeFitter.enabled = true;
                            }
                            OnShow();
                            onComplete?.Invoke();
                        }));
            IsOpen = true;
            _manager.PopupOpened(Type);
        }

        [Button]
        public void Close(Action onComplete = null)
        {
            Init();

            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            if (_sequence.IsActive())
            {
                _sequence.Kill(true);
            }

            _sequence = DOTween.Sequence();
            _sequence.Append(_popupContainer.transform.DOScale(_animationData.startScale, _animationData.closeDuration)
                .SetEase(_animationData.closeEase).OnComplete(
                    () =>
                    {
                        _canvasGroup.alpha = 0f;
                        gameObject.SetActive(false);
                        
                        if (_contentSizeFitter != null)
                        {
                            _contentSizeFitter.enabled = false;
                        }

                        if (_content != null)
                        {
                            Vector2 size = _content.sizeDelta;
                            size.y = 0;
                            _content.sizeDelta = size;
                        }
                        
                        OnHide();
                        _manager.PopupClosed(Type);
                        onComplete?.Invoke();
                    }));
            IsOpen = false;
        }

        private void BringToFront()
        {
            if (_popupRootContainer == null)
            {
                Debug.LogWarning("Popup container is null. Cannot reorder popup.");
                return;
            }

            transform.SetAsLastSibling();
        }

        public bool IsHideUI()
        {
            return _data.isHideUI;
        }
        
        protected virtual void OnInit() { }
        protected virtual void OnShow() { }
        protected virtual void OnHide() { }
        protected virtual PopupType GetPopupType() { return _type; }

        protected virtual void OnConstruct(){}

        protected virtual void OnCloseStart()
        {
            _manager.Close(this);
        }
    }
}
