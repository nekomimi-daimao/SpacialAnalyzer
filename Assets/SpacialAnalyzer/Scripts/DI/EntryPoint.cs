using Cysharp.Threading.Tasks;
using SpacialAnalyzer.Scripts.Analyzer;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using VContainer;

namespace SpacialAnalyzer.Scripts.DI
{
    public sealed class EntryPoint : MonoBehaviour
    {
        [Inject]
        public void Initialize(AnalyzeSpatial spatial)
        {
            Init(spatial).Forget();
        }

        private async UniTaskVoid Init(AnalyzeSpatial spatial)
        {
            await UniTask.Yield();
            var token = this.GetCancellationTokenOnDestroy();
            await UniTask.WaitUntil(() => ARSession.state == ARSessionState.SessionTracking, cancellationToken: token);
            spatial.Init();
        }
    }
}
