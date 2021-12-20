using System;
using ARFoundationRemote.Runtime;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace SpacialAnalyzer.Scripts.Analyzer
{
    [Serializable]
    public sealed class AnalyzeSnapShot
    {
        public string textureBase64;
        public int textureWidth;
        public int textureHeight;
        public int screenWidth;
        public int screenHeight;
        public TrackableIdSerializable trackableIdSerializable;
        public TrackableId trackableId => trackableIdSerializable.Value;
        public Vector3 diffPos;
        public Quaternion diffRot;

        public static AnalyzeSnapShot Create(
            Texture2D texture,
            int screenWidth, int screenHeight,
            ARAnchor anchor, Transform cameraTs)
        {
            var anchorTs = anchor.transform;
            return new AnalyzeSnapShot
            {
                textureBase64 = Convert.ToBase64String(texture.EncodeToJPG()),
                textureWidth = texture.width,
                textureHeight = texture.height,
                screenWidth = screenWidth,
                screenHeight = screenHeight,
                trackableIdSerializable = TrackableIdSerializable.Create(anchor.trackableId),
                diffPos = cameraTs.position - anchorTs.position,
                diffRot = cameraTs.rotation * Quaternion.Inverse(anchorTs.rotation),
            };
        }

        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }

        public static AnalyzeSnapShot FromJson(string json)
        {
            try
            {
                return JsonUtility.FromJson<AnalyzeSnapShot>(json);
            }
            catch
            {
                return null;
            }
        }
    }
}
