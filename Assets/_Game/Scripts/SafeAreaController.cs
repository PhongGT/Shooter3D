using UnityEngine;

namespace Shooter3D
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaController : MonoBehaviour
    {
        private RectTransform panel;
        private Rect lastSafeArea;
        private Vector2Int lastScreenSize;

        private void Awake()
        {
            panel = GetComponent<RectTransform>();
            Apply();
        }

        private void Update()
        {
            if (lastSafeArea != Screen.safeArea || lastScreenSize.x != Screen.width || lastScreenSize.y != Screen.height)
            {
                Apply();
            }
        }

        private void Apply()
        {
            Rect safe = Screen.safeArea;
            lastSafeArea = safe;
            lastScreenSize = new Vector2Int(Screen.width, Screen.height);

            Vector2 min = safe.position;
            Vector2 max = safe.position + safe.size;
            min.x /= Screen.width;
            min.y /= Screen.height;
            max.x /= Screen.width;
            max.y /= Screen.height;
            panel.anchorMin = min;
            panel.anchorMax = max;
            panel.offsetMin = Vector2.zero;
            panel.offsetMax = Vector2.zero;
        }
    }
}
