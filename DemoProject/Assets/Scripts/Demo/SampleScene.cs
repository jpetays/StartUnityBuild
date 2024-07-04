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
                new Tuple<string, string>(" ProductName", Application.productName),
                new Tuple<string, string>(" Version", Application.version),
                new Tuple<string, string>(" BundleVersion", BuildProperties.BundleVersionCode.ToString()),
                new Tuple<string, string>(" CompiledOnDate", BuildProperties.CompiledOnDate),
                new Tuple<string, string>(" PrgFrame", PrgFrame.Info.Version),
#if UNITY_EDITOR
                new Tuple<string, string>(" PrgBuild", PrgBuild.Info.Version),
#else
                new Tuple<string, string>(" PrgBuild", "---"),
#endif
                new Tuple<string, string>(" UnityVersion", Application.unityVersion),
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
