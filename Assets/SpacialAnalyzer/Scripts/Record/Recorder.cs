using System;
using System.IO;
using Cysharp.Threading.Tasks;
using Google.XR.ARCoreExtensions;
using UnityEngine;

namespace SpacialAnalyzer.Scripts.Record
{
    [RequireComponent(typeof(ARRecordingManager))]
    public sealed class Recorder : MonoBehaviour
    {
        private ARRecordingManager _recordingManager = null;
        private const string DateTimeFormat = "yyyyMMddHHmmss";

        private void StartRecord()
        {
            if (_recordingManager == null)
            {
                _recordingManager = GetComponent<ARRecordingManager>();
            }

            var path = Path.Combine(Application.persistentDataPath, "record", DateTimeOffset.Now.ToString(DateTimeFormat));
            path += ".mp4";
            var fileInfo = new FileInfo(path);
            if (fileInfo.Directory is { Exists: false })
            {
                fileInfo.Directory.Create();
            }

            Debug.Log(fileInfo.FullName);

            var recordingConfig = ScriptableObject.CreateInstance<ARCoreRecordingConfig>();
            recordingConfig.Mp4DatasetUri = new Uri(fileInfo.FullName);
            recordingConfig.AutoStopOnPause = true;
            var recordResult = _recordingManager.StartRecording(recordingConfig);
            Debug.Log(recordResult);
        }


        private async UniTaskVoid Start()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(5));
            StartRecord();
        }

        private void OnDestroy()
        {
            _recordingManager.StopRecording();
        }
    }
}
