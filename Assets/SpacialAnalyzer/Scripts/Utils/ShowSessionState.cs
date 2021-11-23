using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARCore;
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
            var showSessionState = session.gameObject.AddComponent<ShowSessionState>();
            if (!(session.subsystem is ARCoreSessionSubsystem subsystem))
            {
                return;
            }

            showSessionState._sessionSubsystem = subsystem;

            style = new GUIStyle();
            style.fontSize = 40;
            style.normal.textColor = Color.green;
            Rect = new Rect(20, 20, Screen.width, Screen.height);
        }

        private static GUIStyle style = new GUIStyle();
        private static Rect Rect = new Rect();

        private void OnGUI()
        {
            GUI.Label(Rect, ToLog(), style);
        }

        private readonly StringBuilder _stringBuilder = new StringBuilder();
        private ARCoreSessionSubsystem _sessionSubsystem;

        private string ToLog()
        {
            _stringBuilder.Clear();

            _stringBuilder.AppendLine(ARSession.state.ToString());
            _stringBuilder.AppendLine(_sessionSubsystem.recordingStatus.ToString());
            _stringBuilder.AppendLine(_sessionSubsystem.playbackStatus.ToString());

            return _stringBuilder.ToString();
        }
    }
}
