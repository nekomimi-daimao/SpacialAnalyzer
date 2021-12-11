using System;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.ARCore;
using UnityEngine.XR.ARFoundation;

namespace SpacialAnalyzer.Scripts.Record
{
    public sealed class RecordAndPlayBack
    {
        #region Components

        private ARSession _arSession = null;
        private ARCoreSessionSubsystem _subsystemArCore;

        private bool InitComponents()
        {
            if (_arSession == null)
            {
                _arSession = UnityEngine.Object.FindObjectOfType<ARSession>();
            }
            if (_arSession == null)
            {
                return false;
            }

            _subsystemArCore ??= _arSession.subsystem as ARCoreSessionSubsystem;

            return _subsystemArCore != null;
        }

        #endregion

        #region Record

        private const string DateTimeFormat = "yyyyMMddHHmmss";

        public async UniTask<ArStatus> StartRecord()
        {
            await UniTask.Yield();

            var initResult = InitComponents();
            if (!initResult)
            {
                return ArStatus.ErrorFatal;
            }
            if (_subsystemArCore.recordingStatus.Recording() || _subsystemArCore.playbackStatus.Playing())
            {
                return ArStatus.ErrorIllegalState;
            }

            var path = Path.Combine(Application.persistentDataPath, "record", DateTimeOffset.Now.ToString(DateTimeFormat));
            path += ".mp4";
            var fileInfo = new FileInfo(path);
            if (fileInfo.Directory is { Exists: false })
            {
                fileInfo.Directory.Create();
            }

            var session = _subsystemArCore.session;
            using var config = new ArRecordingConfig(session);
            config.SetMp4DatasetFilePath(session, path);
            config.SetRecordingRotation(session, GetRotation());
            return _subsystemArCore.StartRecording(config);
        }

        public async UniTask<ArStatus> StopRecord()
        {
            await UniTask.Yield();

            var initResult = InitComponents();
            if (!initResult)
            {
                return ArStatus.ErrorFatal;
            }
            if (!_subsystemArCore.recordingStatus.Recording())
            {
                return ArStatus.ErrorIllegalState;
            }

            return _subsystemArCore.StopRecording();
        }

        private static int GetRotation() => Screen.orientation switch
        {
            ScreenOrientation.Portrait => 0,
            ScreenOrientation.LandscapeLeft => 90,
            ScreenOrientation.PortraitUpsideDown => 180,
            ScreenOrientation.LandscapeRight => 270,
            _ => 0
        };

        #endregion

        #region PlayBack

        public async UniTask<ArStatus> StartPlayback(string path)
        {
            await UniTask.Yield();

            var initResult = InitComponents();
            if (!initResult)
            {
                return ArStatus.ErrorFatal;
            }
            if (_subsystemArCore.recordingStatus.Recording() || _subsystemArCore.playbackStatus.Playing())
            {
                return ArStatus.ErrorIllegalState;
            }

            var fileInfo = new FileInfo(path);
            if (!fileInfo.Exists)
            {
                return ArStatus.ErrorInvalidArgument;
            }

            return _subsystemArCore.StartPlayback(path);
        }

        public async UniTask<ArStatus> StopPlayBack()
        {
            await UniTask.Yield();

            var initResult = InitComponents();
            if (!initResult)
            {
                return ArStatus.ErrorFatal;
            }
            if (!_subsystemArCore.playbackStatus.Playing())
            {
                return ArStatus.ErrorIllegalState;
            }

            return _subsystemArCore.StopPlayback();
        }

        #endregion
    }
}
