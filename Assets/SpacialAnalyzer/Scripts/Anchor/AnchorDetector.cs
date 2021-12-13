using System.Collections.Generic;
using SpacialAnalyzer.Scripts.Utils;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace SpacialAnalyzer.Scripts.Anchor
{
    public sealed class AnchorDetector
    {
        private const TrackableType TrackableTypes =
            TrackableType.FeaturePoint |
            TrackableType.PlaneWithinPolygon;

        private ARAnchorManager _anchorManager;
        private ARRaycastManager _raycastManager;
        private ARPointCloudManager _pointCloudManager;
        private ARPlaneManager _planeManager;
        private readonly List<ARRaycastHit> _arRaycastHits = new List<ARRaycastHit>();

        public AnchorWithMemo _prefabAnchorWithMemo = null;

        public readonly List<ARAnchor> Anchors = new List<ARAnchor>();

        public void Init()
        {
            _anchorManager = ArComponentsSupplier.GetComponent<ARAnchorManager>();
            _raycastManager = ArComponentsSupplier.GetComponent<ARRaycastManager>();
            _pointCloudManager = ArComponentsSupplier.GetComponent<ARPointCloudManager>();
            _planeManager = ArComponentsSupplier.GetComponent<ARPlaneManager>();

            _anchorManager.anchorsChanged += OnAnchorsChanged;
        }

        public ARAnchor CastAnchor(Vector3 origin, Vector3 forward)
        {
            _arRaycastHits.Clear();
            if (!_raycastManager.Raycast(new Ray(origin, forward), _arRaycastHits, TrackableTypes))
            {
                return null;
            }
            var hit = _arRaycastHits[0];

            ARAnchor anchor;
            if (hit.trackable is ARPlane plane)
            {
                anchor = _anchorManager.AttachAnchor(plane, hit.pose);
            }
            else
            {
                anchor = _anchorManager.GetAnchor(hit.trackableId);
            }
            if (anchor == null)
            {
                return null;
            }

            var anchorTs = anchor.transform;
            var trackableId = anchor.trackableId.subId1.ToString("X16");
            var memo = _prefabAnchorWithMemo.Create(trackableId, anchorTs.position, anchorTs.rotation, anchorTs);

            return anchor;
        }

        private void OnAnchorsChanged(ARAnchorsChangedEventArgs arg)
        {
            Anchors.AddRange(arg.added);
            Anchors.RemoveAll(anchor => arg.removed.Contains(anchor));
        }
    }
}
