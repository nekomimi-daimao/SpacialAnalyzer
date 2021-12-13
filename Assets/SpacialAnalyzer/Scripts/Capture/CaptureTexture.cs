using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using VContainer;

namespace SpacialAnalyzer.Scripts.Capture
{
    public sealed class CaptureTexture
    {
        [Inject]
        private ARCameraManager _cameraManager;

        public async UniTask<Texture2D> CaptureAsync()
        {
            using (var disposable = new XRCpuImageDisposable())
            {
                if (!_cameraManager.TryAcquireLatestCpuImage(out var xrCpuImage))
                {
                    return null;
                }
                if (!xrCpuImage.valid)
                {
                    return null;
                }
                disposable._cpuImage = xrCpuImage;

                var conversionParams = new XRCpuImage.ConversionParams
                {
                    inputRect = new RectInt(0, 0, xrCpuImage.width, xrCpuImage.height),
                    outputDimensions = new Vector2Int(xrCpuImage.width, xrCpuImage.height),
                    outputFormat = TextureFormat.RGBA32,
                    transformation = XRCpuImage.Transformation.MirrorX,
                };
                Texture2D texture = null;
                using (var asyncConversion = xrCpuImage.ConvertAsync(conversionParams))
                {
                    await UniTask.WaitUntil(() => asyncConversion.status.IsDone());
                    if (asyncConversion.status != XRCpuImage.AsyncConversionStatus.Ready)
                    {
                        return null;
                    }
                    texture = new Texture2D(xrCpuImage.width, xrCpuImage.height, TextureFormat.RGBA32, false);
                    texture.LoadRawTextureData(asyncConversion.GetData<byte>());
                    texture.Apply();
                }
                return texture;
            }
        }

        private class XRCpuImageDisposable : IDisposable
        {
            public XRCpuImage? _cpuImage;

            public void Dispose()
            {
                _cpuImage?.Dispose();
            }
        }
    }
}
