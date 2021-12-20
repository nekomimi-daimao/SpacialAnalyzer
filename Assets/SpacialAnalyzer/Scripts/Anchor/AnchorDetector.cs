using System;
using System.Collections.Generic;
using SpacialAnalyzer.Scripts.Utils;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using VContainer;

namespace SpacialAnalyzer.Scripts.Anchor
{
    public sealed class AnchorDetector : IDisposable
    {
        private const TrackableType TrackableTypes =
            TrackableType.FeaturePoint |
            TrackableType.PlaneWithinPolygon;

        private readonly ARAnchorManager _anchorManager;

        private readonly ARRaycastManager _raycastManager;

        private readonly ARPointCloudManager _pointCloudManager;

        private readonly ARPlaneManager _planeManager;

        private readonly List<ARRaycastHit> _arRaycastHits = new List<ARRaycastHit>();

        public readonly List<ARAnchor> Anchors = new List<ARAnchor>();

        [Inject]
        public AnchorDetector(
            ARAnchorManager anchorManager,
            ARRaycastManager raycastManager,
            ARPointCloudManager pointCloudManager,
            ARPlaneManager planeManager)
        {
            _anchorManager = anchorManager;
            _raycastManager = raycastManager;
            _pointCloudManager = pointCloudManager;
            _planeManager = planeManager;

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
            return anchor;
        }

        private void OnAnchorsChanged(ARAnchorsChangedEventArgs arg)
        {
            Anchors.AddRange(arg.added);
            Anchors.RemoveAll(anchor => arg.removed.Contains(anchor));
        }

        public void Dispose()
        {
            _anchorManager.anchorsChanged -= OnAnchorsChanged;
        }
    }
}
