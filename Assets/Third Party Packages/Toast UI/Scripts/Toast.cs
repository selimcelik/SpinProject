using UnityEngine;
using EasyUI.Helpers;

namespace EasyUI.Toast
{
    public enum ToastColor
    {
        Black,
        Red,
        Purple,
        Magenta,
        Blue,
        Green,
        Yellow,
        Orange
    }

    public enum ToastPosition
    {
        TopLeft,
        TopCenter,
        TopRight,
        MiddleLeft,
        MiddleCenter,
        MiddleRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }

    public static class Toast
    {
        public static bool isLoaded = false;
        private static ToastUI toastUI;

        public static GameObject ToastUIGameObject;

        private static void LoadPanel()
        {
            if (!isLoaded)
            {
                ToastUIGameObject = MonoBehaviour.Instantiate(Resources.Load<GameObject>("ToastUI"));
                ToastUIGameObject.name = "[TOAST UI]";
                toastUI = ToastUIGameObject.GetComponent<ToastUI>();
                isLoaded = true;
            }
        }
        
        public static void DestroyPanel()
        {
            if (isLoaded)
            {
                MonoBehaviour.Destroy(ToastUIGameObject);
                isLoaded = false;
            }
        }
        
        public static void Show(string text)
        {
            LoadPanel();
            toastUI.Init(text, 2f, ToastColor.Black, ToastPosition.BottomCenter);
        }


        public static void Show(string text, float duration)
        {
            LoadPanel();
            toastUI.Init(text, duration, ToastColor.Black, ToastPosition.BottomCenter);
        }

        public static void Show(string text, float duration, ToastPosition position)
        {
            LoadPanel();
            toastUI.Init(text, duration, ToastColor.Black, position);
        }


        public static void Show(string text, ToastColor color)
        {
            LoadPanel();
            toastUI.Init(text, 2f, color, ToastPosition.BottomCenter);
        }

        public static void Show(string text, ToastColor color, ToastPosition position)
        {
            LoadPanel();
            toastUI.Init(text, 2f, color, position);
        }


        public static void Show(string text, Color color)
        {
            LoadPanel();
            toastUI.Init(text, 2f, color, ToastPosition.BottomCenter);
        }

        public static void Show(string text, Color color, ToastPosition position)
        {
            LoadPanel();
            toastUI.Init(text, 2f, color, position);
        }


        public static void Show(string text, float duration, ToastColor color)
        {
            LoadPanel();
            toastUI.Init(text, duration, color, ToastPosition.BottomCenter);
        }

        public static void Show(string text, float duration, ToastColor color, ToastPosition position)
        {
            LoadPanel();
            toastUI.Init(text, duration, color, position);
        }


        public static void Show(string text, float duration, Color color)
        {
            LoadPanel();
            toastUI.Init(text, duration, color, ToastPosition.BottomCenter);
        }

        public static void Show(string text, float duration, Color color, ToastPosition position)
        {
            LoadPanel();
            toastUI.Init(text, duration, color, position);
        }


        public static void Dismiss()
        {
            toastUI.Dismiss();
        }
    }
}