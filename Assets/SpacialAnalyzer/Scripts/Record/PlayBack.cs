using System;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.ARCore;
using UnityEngine.XR.ARFoundation;

namespace SpacialAnalyzer.Scripts.Record
{
    public sealed class PlayBack : MonoBehaviour
    {
        private ARSession _arSession = null;

        [ContextMenu(nameof(StartPlayBack))]
        private async UniTask StartPlayBack()
        {
            if (_arSession == null)
            {
                _arSession = FindObjectOfType<ARSession>();
            }

            if (!(_arSession.subsystem is ARCoreSessionSubsystem subsystem))
            {
                return;
            }

            var f = "20211124005211.mp4";
            var path = Path.Combine(Application.persistentDataPath, "record", f);

            subsystem.StartPlayback(path);
        }

        private async UniTaskVoid Start()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(5));
            StartPlayBack();
        }
    }
}
