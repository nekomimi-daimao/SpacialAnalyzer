using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using SpacialAnalyzer.Scripts.Vision.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace SpacialAnalyzer.Scripts.Vision
{
    public static class RequestSender
    {
        private const string API = "https://vision.googleapis.com/v1/images:annotate?key=";

        public static async UniTask<ResponseData> SendRequest(RequestData requestData, CancellationToken token)
        {
            try
            {
                var postBuffer = System.Text.Encoding.UTF8.GetBytes(requestData.ToJson());
                var uploadHandler = new UploadHandlerRaw(postBuffer);
                var downloadHandler = new DownloadHandlerBuffer();

                var request = new UnityWebRequest(API + VisionConst.ApiKey, UnityWebRequest.kHttpVerbPOST)
                    { uploadHandler = uploadHandler, downloadHandler = downloadHandler, };
                request.SetRequestHeader("Content-Type", "application/json; charset=utf-8");

                await request.SendWebRequest().WithCancellation(token);
                if (request.result != UnityWebRequest.Result.Success)
                {
                    return null;
                }
                var responseRaw = downloadHandler.text;
                return ResponseData.FromJson(responseRaw);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return null;
            }
        }
    }
}
