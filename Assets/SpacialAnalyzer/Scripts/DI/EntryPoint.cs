using SpacialAnalyzer.Scripts.Analyzer;
using UnityEngine;
using VContainer;

namespace SpacialAnalyzer.Scripts.DI
{
    public sealed class EntryPoint : MonoBehaviour
    {
        [Inject]
        public void Initialize(AnalyzeSpatial spatial)
        {
            spatial.Init();
        }
    }
}
