using UnityEngine;
using VContainer;

namespace SpacialAnalyzer.Scripts.Analyzer
{
    public sealed class EntryPoint : MonoBehaviour
    {
        [Inject]
        public void Initialize(AnalyzeSpatial spatial)
        {
            // spatial.Init();
        }
    }
}
