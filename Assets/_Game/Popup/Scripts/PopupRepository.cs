using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Popup
{
    [CreateAssetMenu(menuName = "Game/Popup/Popup Repository")]

    public class PopupRepository : ScriptableObject
    {
        [SerializeField] private List<PopupData> _datas;

        public PopupData GetData(PopupType type)
        {
            return _datas.FirstOrDefault(data => data.type == type);
        }
    }

    [Serializable]
    public class PopupData
    {
        public PopupType type;
        public PopupAnimationData animationData;
        public bool isHideUI;
    }

    [Serializable]
    public class PopupAnimationData
    {
        public Ease openEase = Ease.OutBack;
        public float openDuration = 0.5f;
        public Ease closeEase = Ease.InBack;
        public float closeDuration = 0.5f;
        public Vector3 startScale = Vector3.zero;
        public Vector3 endScale = Vector3.one;
    }

    public enum PopupType
    {
        Undefined = 0,
        CardPopUp = 1,
    }
}