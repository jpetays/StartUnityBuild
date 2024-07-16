using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Demo
{
    public class LoaderScene : MonoBehaviour
    {
        [SerializeField] private RectTransform _rootPanel;
        [SerializeField] private Button _continue;

        private void Awake()
        {
#if UNITY_WEBGL
            WebGlPreload();
#else
            NormalLoad();
#endif
        }

        private void WebGlPreload()
        {
            // This is required to confirm that User controls the app and we can play music etc. after confirmation.
            _rootPanel.gameObject.SetActive(true);
            _continue.onClick.AddListener(NormalLoad);
        }

        private void NormalLoad()
        {
            SceneManager.LoadScene(1);
        }
    }
}
