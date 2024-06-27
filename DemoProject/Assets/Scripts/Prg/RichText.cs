using System.Diagnostics.CodeAnalysis;

namespace Prg
{
    /// <summary>
    /// UNITY Rich Text formatting wrapper.<br />
    /// https://docs.unity3d.com/Packages/com.unity.ugui@1.0/manual/StyledText.html#supported-colors
    /// </summary>
    public static class RichText
    {
        public static string Bold(object text)
        {
#if UNITY_EDITOR
            return $"<b>{text}</b>";
#else
            return text?.ToString();
#endif
        }

        public static string White(object text)
        {
#if UNITY_EDITOR
            return $"<color=white>{text}</color>";
#else
                return text?.ToString();
#endif
        }

        public static string Red(object text)
        {
#if UNITY_EDITOR
            return $"<color=red>{text}</color>";
#else
            return text?.ToString();
#endif
        }

        public static string Blue(object text)
        {
#if UNITY_EDITOR
            return $"<color=blue>{text}</color>";
#else
            return text?.ToString();
#endif
        }

        public static string LightBlue(object text)
        {
#if UNITY_EDITOR
            return $"<color=#add8e6ff>{text}</color>";
#else
            return text?.ToString();
#endif
        }

        public static string Green(object text)
        {
#if UNITY_EDITOR
            return $"<color=green>{text}</color>";
#else
            return text?.ToString();
#endif
        }

        public static string Magenta(object text)
        {
#if UNITY_EDITOR
            return $"<color=#ff00ffff>{text}</color>";
#else
            return text?.ToString();
#endif
        }

        public static string Yellow(object text)
        {
#if UNITY_EDITOR
            return $"<color=yellow>{text}</color>";
#else
            return text?.ToString();
#endif
        }

        public static string Orange(object text)
        {
#if UNITY_EDITOR
            return $"<color=#ffa500ff>{text}</color>";
#else
            return text?.ToString();
#endif
        }

        public static string Brown(object text)
        {
#if UNITY_EDITOR
            return $"<color=#a52a2aff>{text}</color>";
#else
            return text?.ToString();
#endif
        }

        public static string Grey(object text)
        {
#if UNITY_EDITOR
            return $"<color=#808080ff>{text}</color>";
#else
            return text?.ToString();
#endif
        }
    }
}
