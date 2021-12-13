using SpacialAnalyzer.Scripts.Analyzer;
using SpacialAnalyzer.Scripts.Anchor;
using SpacialAnalyzer.Scripts.Capture;
using SpacialAnalyzer.Scripts.UI;
using SpacialAnalyzer.Scripts.Utils;
using UnityEngine.XR.ARFoundation;
using VContainer;
using VContainer.Unity;

namespace SpacialAnalyzer.Scripts.DI
{
    public sealed class LifetimeScopeAnalyzer : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<ARSession>();
            builder.RegisterComponentInHierarchy<ARCameraManager>();
            builder.RegisterComponentInHierarchy<AnchorWithMemo>();
            builder.RegisterComponentInHierarchy<UICanvas>();

            var origin = FindObjectOfType<ARSessionOrigin>();
            builder.RegisterComponent(origin);

            var originGo = origin.gameObject;
            builder.RegisterComponent(originGo.GetOrAddComponent<ARAnchorManager>());
            builder.RegisterComponent(originGo.GetOrAddComponent<ARRaycastManager>());
            builder.RegisterComponent(originGo.GetOrAddComponent<ARPointCloudManager>());
            builder.RegisterComponent(originGo.GetOrAddComponent<ARPlaneManager>());

            builder.Register<AnchorDetector>(Lifetime.Scoped);
            builder.Register<CaptureTexture>(Lifetime.Scoped);

            builder.RegisterEntryPoint<AnalyzeSpatial>();
        }
    }
}
