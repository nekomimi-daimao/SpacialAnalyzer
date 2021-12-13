using UnityEngine;

namespace SpacialAnalyzer.Scripts.Utils
{
    public static class ExtensionGameObject
    {
        public static T GetOrAddComponent<T>(this GameObject obj) where T : Component
        {
            var c = obj.GetComponent<T>();
            return c != null ? c : obj.AddComponent<T>();
        }
    }
}
