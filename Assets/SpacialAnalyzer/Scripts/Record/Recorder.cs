using System;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.ARCore;
using UnityEngine.XR.ARFoundation;

namespace SpacialAnalyzer.Scripts.Record
{
    public sealed class Recorder : MonoBehaviour
    {
        private const string DateTimeFormat = "yyyyMMddHHmmss";

        private ARSession _arSession = null;

        private async void StartRecord()
        {
            if (_arSession == null)
            {
                _arSession = FindObjectOfType<ARSession>();
            }

            if (!(_arSession.subsystem is ARCoreSessionSubsystem subsystem))
            {
                return;
            }

            var path = Path.Combine(Application.persistentDataPath, "record", DateTimeOffset.Now.ToString(DateTimeFormat));
            path += ".mp4";
            var fileInfo = new FileInfo(path);
            if (fileInfo.Directory is { Exists: false })
            {
                fileInfo.Directory.Create();
            }

            var session = subsystem.session;

            using var config = new ArRecordingConfig(session);
            config.SetMp4DatasetFilePath(session, path);
            config.SetRecordingRotation(session, GetRotation());
            var status = subsystem.StartRecording(config);

            await UniTask.Delay(TimeSpan.FromSeconds(10));

            status = subsystem.StopRecording();
        }

        private static int GetRotation() => Screen.orientation switch
        {
            ScreenOrientation.Portrait => 0,
            ScreenOrientation.LandscapeLeft => 90,
            ScreenOrientation.PortraitUpsideDown => 180,
            ScreenOrientation.LandscapeRight => 270,
            _ => 0
        };

        private async UniTaskVoid Start()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(5));
            StartRecord();
        }
    }
}
