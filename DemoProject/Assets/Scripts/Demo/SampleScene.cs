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
            var white = Color.white;
            var gray = Color.gray;

#if UNITY_EDITOR
            var buildVersion = PrgBuild.Info.Version;
#else
            var buildVersion = "---";
#endif
            var lines = new[]
            {
                new Tuple<string, string, Color>(" UnityVersion", Application.unityVersion, white),
                new Tuple<string, string, Color>(" ProductName", Application.productName, white),
                new Tuple<string, string, Color>(" Version", Application.version, white),
                new Tuple<string, string, Color>(" BundleVersion", BuildProperties.BundleVersionCode.ToString(), white),
                new Tuple<string, string, Color>(" CompiledOnDate", BuildProperties.CompiledOnDate, white),
                new Tuple<string, string, Color>(" PrgFrame", PrgFrame.Info.Version, white),
                new Tuple<string, string, Color>(" PrgBuild", buildVersion, gray),
            };
            for (int i = 0; i < lines.Length; ++i)
            {
                var tuple = lines[i];
                SetContent(i, tuple.Item1, tuple.Item2, tuple.Item3);
            }
            return;

            void SetContent(int index, string label, string content, Color color)
            {
                var parent = _labels[index];
                var textLabel = parent.GetChild(0).GetComponent<TextMeshProUGUI>();
                var textContent = parent.GetChild(1).GetComponent<TextMeshProUGUI>();
                textLabel.text = label;
                textLabel.color = color;
                textContent.text = content;
                textContent.color = color;
            }
        }
    }
}
