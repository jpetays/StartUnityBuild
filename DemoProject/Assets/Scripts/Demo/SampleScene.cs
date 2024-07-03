using System;
using TMPro;
using UnityEngine;

namespace Demo
{
    public class SampleScene : MonoBehaviour
    {
        [SerializeField] private Transform[] _labels;

        private void Awake()
        {
            var lines = new[]
            {
                new Tuple<string, string>("productName", Application.productName),
                new Tuple<string, string>("version", Application.version),
                new Tuple<string, string>("bundleVersion", "?"),
                new Tuple<string, string>("PrgFrame", PrgFrame.Info.Version),
#if UNITY_EDITOR
                new Tuple<string, string>("PrgBuild", PrgBuild.Info.Version),
#else
                new Tuple<string, string>("PrgBuild", ""),
#endif
                new Tuple<string, string>("unityVersion", Application.unityVersion),
            };
            for (int i = 0; i < lines.Length; ++i)
            {
                SetContent(i, lines[i].Item1, lines[i].Item2);
            }
            return;

            void SetContent(int index, string label, string content)
            {
                var parent = _labels[index];
                parent.GetChild(0).GetComponent<TextMeshProUGUI>().text = label;
                parent.GetChild(1).GetComponent<TextMeshProUGUI>().text = content;
            }
        }
    }
}
