using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using SpacialAnalyzer.Scripts.Anchor;
using SpacialAnalyzer.Scripts.Capture;
using SpacialAnalyzer.Scripts.UI;
using SpacialAnalyzer.Scripts.Utils;
using SpacialAnalyzer.Scripts.Vision;
using SpacialAnalyzer.Scripts.Vision.Json;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using VContainer;

namespace SpacialAnalyzer.Scripts.Analyzer
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public sealed class AnalyzeSpatial
    {
        private readonly AnchorDetector _anchorDetector;
        private readonly CaptureTexture _captureTexture;
        private readonly Camera _camera;
        private readonly Transform _cameraTs;
        private readonly UICanvas _uiCanvas;
        private readonly AnchorWithMemo _prefabAnchorWithMemo;
        private Transform _transformHelper;

        [Inject]
        public AnalyzeSpatial(
            AnchorDetector anchorDetector,
            CaptureTexture captureTexture,
            ARCameraManager cameraManager,
            UICanvas uiCanvas,
            AnchorWithMemo prefabAnchorWithMemo)
        {
            _anchorDetector = anchorDetector;
            _captureTexture = captureTexture;

            _camera = cameraManager.gameObject.GetComponent<Camera>();
            _cameraTs = _camera.transform;
            _uiCanvas = uiCanvas;
            _prefabAnchorWithMemo = prefabAnchorWithMemo;
        }

        public void Init()
        {
            _transformHelper = new GameObject(nameof(AnalyzeSpatial)).transform;

            _uiCanvas.ButtonScan
                .OnClickAsAsyncEnumerable()
                .ForEachAwaitWithCancellationAsync(OnClickAnalyze, _uiCanvas.GetCancellationTokenOnDestroy());
        }

        private async UniTask OnClickAnalyze(AsyncUnit _, CancellationToken token)
        {
            try
            {
                await Analyze(token);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public async UniTask Analyze(CancellationToken token)
        {
            Texture2D texture = null;
            try
            {
                var anchor = _anchorDetector.CastAnchor(_cameraTs.position, _cameraTs.forward);
                if (anchor == null)
                {
                    return;
                }
                texture = await _captureTexture.CaptureAsync();
                if (texture == null)
                {
                    return;
                }

                var anchorTs = anchor.transform;
                var trackableId = anchor.trackableId.subId1.ToString("X16");
                var memo = _prefabAnchorWithMemo.Create(trackableId, anchorTs.position, anchorTs.rotation, anchorTs);

                var snapShot = AnalyzeSnapShot.Create(texture, Screen.width, Screen.height, anchor, _cameraTs);
                var requestData = RequestData.Create(snapShot.textureBase64);
                var responseData = await RequestSender.SendRequest(requestData, token);
                if (responseData == null)
                {
                    return;
                }

                Cast(snapShot, responseData, anchor);
            }
            finally
            {
                if (texture != null)
                {
                    UnityEngine.Object.Destroy(texture);
                }
            }
        }

        private void Cast(AnalyzeSnapShot snapShot, ResponseData response, ARAnchor anchor)
        {
            var anchorTs = anchor.transform;
            var recentPos = anchorTs.TransformPoint(snapShot.diffPos);
            var recentRot = snapShot.diffRot * anchorTs.rotation;
            _transformHelper.SetPositionAndRotation(recentPos, recentRot);

            var width = snapShot.screenWidth;
            var height = (snapShot.screenWidth / snapShot.textureWidth) * snapShot.textureHeight;

            var localizedObjectAnnotations =
                response.responses.SelectMany(r => r.localizedObjectAnnotations);

            foreach (var objectAnnotation in localizedObjectAnnotations)
            {
                var x = 0d;
                var y = 0d;
                var normalizedVertices = objectAnnotation.boundingPoly.normalizedVertices;
                foreach (var vertex in normalizedVertices)
                {
                    x += vertex.x;
                    y += vertex.y;
                }
                x /= normalizedVertices.Count;
                y /= normalizedVertices.Count;

                var ray = _camera.ScreenPointToRay(new Vector3((float)(width * x), (float)(height * y), 0));
                var rayLocalPos = anchorTs.InverseTransformPoint(ray.origin);
                var rayLocalDirection = anchorTs.InverseTransformDirection(ray.direction);

                var rayPos = _transformHelper.TransformPoint(rayLocalPos);
                var rayDirection = _transformHelper.TransformDirection(rayLocalDirection);

                var castAnchor = _anchorDetector.CastAnchor(rayPos, rayDirection);
                if (castAnchor == null)
                {
                    continue;
                }

                var castAnchorTs = castAnchor.transform;
                var memo = _prefabAnchorWithMemo.Create(
                    objectAnnotation.name,
                    castAnchorTs.position, castAnchorTs.rotation,
                    castAnchorTs);
            }
        }
    }
}
