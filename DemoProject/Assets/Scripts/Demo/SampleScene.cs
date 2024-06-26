using TMPro;
using UnityEngine;

namespace Demo
{
    public class SampleScene : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _unityVersion;
        [SerializeField] private TextMeshProUGUI _productName;
        [SerializeField] private TextMeshProUGUI _productVersion;
        [SerializeField] private TextMeshProUGUI _bundleVersion;

        private void Awake()
        {
            _unityVersion.text = Application.unityVersion;
            _productName.text = Application.productName;
            _productVersion.text = Application.version;
            _bundleVersion.text = "?";
        }
    }
}
