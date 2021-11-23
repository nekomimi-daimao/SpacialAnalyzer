using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;

namespace SpacialAnalyzer.Scripts.Utils
{
    public sealed class ShowSessionState : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static async void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            await UniTask.Yield();
            var session = FindObjectOfType<ARSession>();
            if (session == null)
            {
                return;
            }
            session.gameObject.AddComponent<ShowSessionState>();
            style = new GUIStyle();
            style.fontSize = 40;
            style.normal.textColor = Color.green;
            Rect = new Rect(20, 20, Screen.width, Screen.height);
        }

        private static GUIStyle style = new GUIStyle();
        private static Rect Rect = new Rect();

        private void OnGUI()
        {
            GUI.Label(Rect, ARSession.state.ToString(), style);
        }
    }
}
