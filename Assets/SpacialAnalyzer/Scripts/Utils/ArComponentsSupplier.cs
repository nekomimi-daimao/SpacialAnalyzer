using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;

namespace SpacialAnalyzer.Scripts.Utils
{
    public sealed class ArComponentsSupplier : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mode == LoadSceneMode.Additive)
            {
                return;
            }

            if (FindObjectOfType<ArComponentsSupplier>() != null)
            {
                return;
            }

            var go = new GameObject(nameof(ArComponentsSupplier));
            var supplier = go.AddComponent<ArComponentsSupplier>();
            supplier.Init();
        }

        private static ArComponentsSupplier Instance;
        private ARSession _arSession;
        private ARSessionOrigin _sessionOrigin;

        private void Init()
        {
            _arSession = FindObjectOfType<ARSession>();
            _sessionOrigin = FindObjectOfType<ARSessionOrigin>();
            Instance = this;
        }

        private void OnDestroy()
        {
            _arSession = null;
            _sessionOrigin = null;
            Instance = null;
        }

        public static T GetComponent<T>() where T : MonoBehaviour
        {
            var c = Instance._sessionOrigin.gameObject.GetComponent<T>();
            if (c != null)
            {
                return c;
            }
            return Instance._sessionOrigin.gameObject.AddComponent<T>();
        }
    }
}
