using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpacialAnalyzer.Scripts.Vision.Json
{
    [Serializable]
    public class RequestData
    {
        public List<Request> requests;

        public string ToJson()
        {
            return JsonUtility.ToJson(this, true);
        }

        public static RequestData Create(string content)
        {
            return new RequestData
            {
                requests = new List<Request>
                {
                    new Request
                    {
                        image = new Image
                        {
                            content = content,
                        },
                        features = new List<Feature>
                        {
                            new Feature
                            {
                                maxResults = 10,
                                type = "OBJECT_LOCALIZATION",
                            }
                        }
                    }
                }
            };
        }

        [Serializable]
        public class Request
        {
            public Image image;
            public List<Feature> features;
        }

        [Serializable]
        public class Image
        {
            public string content;
        }

        [Serializable]
        public class Feature
        {
            public int maxResults;
            public string type;
        }
    }
}
