using UnityEngine;
using UnityEngine.InputSystem;

namespace Shooter3D
{
    public sealed class CrosshairController : MonoBehaviour
    {
        [SerializeField] private RectTransform crosshair;
        [SerializeField] private Vector2 horizontalLimits = new Vector2(0.1f, 0.9f);
        [SerializeField] private Vector2 verticalLimits = new Vector2(0.1f, 0.85f);

        public Vector2 ScreenPosition => crosshair != null ? (Vector2)crosshair.position : new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

        public void Configure(RectTransform target)
        {
            crosshair = target;
        }

        private void Start()
        {
            MoveTo(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f));
        }

        private void Update()
        {
            Vector2 pointer = ScreenPosition;
            bool hasPointer = false;

            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            {
                pointer = Touchscreen.current.primaryTouch.position.ReadValue();
                hasPointer = true;
            }
            else if (Mouse.current != null)
            {
                pointer = Mouse.current.position.ReadValue();
                hasPointer = true;
            }

            if (hasPointer)
            {
                MoveTo(pointer);
            }
        }

        private void MoveTo(Vector2 screenPoint)
        {
            if (crosshair == null)
            {
                return;
            }

            screenPoint.x = Mathf.Clamp(screenPoint.x, Screen.width * horizontalLimits.x, Screen.width * horizontalLimits.y);
            screenPoint.y = Mathf.Clamp(screenPoint.y, Screen.height * verticalLimits.x, Screen.height * verticalLimits.y);
            crosshair.position = screenPoint;
        }
    }
}
