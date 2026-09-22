using UnityEngine;

namespace Shooter3D
{
    public sealed class RailCameraController : MonoBehaviour
    {
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private float[] segmentDurations;

        private int segmentIndex;
        private float segmentTime;

        public void Configure(Transform[] points, float[] durations)
        {
            waypoints = points;
            segmentDurations = durations;
        }

        private void Start()
        {
            if (waypoints != null && waypoints.Length > 0)
            {
                transform.SetPositionAndRotation(waypoints[0].position, waypoints[0].rotation);
            }
        }

        private void Update()
        {
            if (waypoints == null || waypoints.Length < 2 || segmentIndex >= waypoints.Length - 1)
            {
                return;
            }

            float duration = Mathf.Max(0.01f, segmentDurations[Mathf.Min(segmentIndex, segmentDurations.Length - 1)]);
            segmentTime += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(segmentTime / duration));

            Transform from = waypoints[segmentIndex];
            Transform to = waypoints[segmentIndex + 1];
            transform.position = Vector3.Lerp(from.position, to.position, t);
            transform.rotation = Quaternion.Slerp(from.rotation, to.rotation, t);

            if (segmentTime >= duration)
            {
                segmentTime -= duration;
                segmentIndex++;
            }
        }
    }
}
