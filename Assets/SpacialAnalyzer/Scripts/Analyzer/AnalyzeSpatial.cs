using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using SpacialAnalyzer.Scripts.Anchor;
using SpacialAnalyzer.Scripts.Capture;
using SpacialAnalyzer.Scripts.UI;
using SpacialAnalyzer.Scripts.Vision;
using SpacialAnalyzer.Scripts.Vision.Json;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using VContainer;
using VContainer.Unity;

namespace SpacialAnalyzer.Scripts.Analyzer
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public sealed class AnalyzeSpatial : IPostStartable
    {
        private readonly AnchorDetector _anchorDetector;
        private readonly CaptureTexture _captureTexture;
        private readonly Transform _cameraTs;
        private readonly UICanvas _uiCanvas;

        [Inject]
        public AnalyzeSpatial(
            AnchorDetector anchorDetector,
            CaptureTexture captureTexture,
            ARCameraManager cameraManager,
            UICanvas uiCanvas)
        {
            _anchorDetector = anchorDetector;
            _captureTexture = captureTexture;

            _cameraTs = cameraManager.transform;
            _uiCanvas = uiCanvas;
        }

        public void PostStart()
        {
            _uiCanvas.ButtonScan
                .OnClickAsAsyncEnumerable()
                .ForEachAwaitWithCancellationAsync(OnClickAnalyze, _uiCanvas.GetCancellationTokenOnDestroy());
        }

        private UniTask OnClickAnalyze(AsyncUnit _, CancellationToken token)
        {
            try
            {
                return Analyze(token);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return UniTask.CompletedTask;
            }
        }

        public async UniTask Analyze(CancellationToken token)
        {
            var anchor = _anchorDetector.CastAnchor(_cameraTs.position, _cameraTs.forward);
            if (anchor == null)
            {
                return;
            }
            var texture = await _captureTexture.CaptureAsync();
            if (texture == null)
            {
                return;
            }

            var base64String = System.Convert.ToBase64String(texture.EncodeToJPG());
            var requestData = RequestData.Create(base64String);
            var request = await RequestSender.SendRequest(requestData, token);
        }
    }
}
