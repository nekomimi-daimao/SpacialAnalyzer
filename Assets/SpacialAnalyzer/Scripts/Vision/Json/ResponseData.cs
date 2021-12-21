using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpacialAnalyzer.Scripts.Vision.Json
{
    [Serializable]
    public class ResponseData
    {
        public List<Responses> responses;

        public string ToJson()
        {
            return JsonUtility.ToJson(this, true);
        }

        public static ResponseData FromJson(string json)
        {
            return JsonUtility.FromJson<ResponseData>(json);
        }

        [Serializable]
        public class Responses
        {
            public List<LocalizedObjectAnnotation> localizedObjectAnnotations;
        }

        [Serializable]
        public class LocalizedObjectAnnotation
        {
            public string mid;
            public string name;
            public double score;
            public BoundingPoly boundingPoly;
        }

        [Serializable]
        public class BoundingPoly
        {
            public List<NormalizedVertices> normalizedVertices;
        }

        [Serializable]
        public class NormalizedVertices
        {
            public double x;
            public double y;
        }
    }
}
