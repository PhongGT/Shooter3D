using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Shooter3D
{
    public sealed class GunController : MonoBehaviour
    {
        [SerializeField] private Camera gameplayCamera;
        [SerializeField] private CrosshairController crosshair;
        [SerializeField] private Text ammoText;
        [SerializeField] private Text reloadText;
        [SerializeField] private int magazineSize = 6;
        [SerializeField] private float damage = 55f;
        [SerializeField] private float shotCooldown = 0.18f;
        [SerializeField] private float reloadDuration = 1.15f;

        private int ammo;
        private float nextShotTime;
        private bool reloading;

        public void Configure(Camera targetCamera, CrosshairController targetCrosshair, Text ammoLabel, Text reloadLabel)
        {
            gameplayCamera = targetCamera;
            crosshair = targetCrosshair;
            ammoText = ammoLabel;
            reloadText = reloadLabel;
        }

        private void Start()
        {
            ammo = magazineSize;
            RefreshUI();
        }

        private void Update()
        {
            bool shootPressed = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
            shootPressed |= Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame;

            bool reloadPressed = Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame;
            if (reloadPressed && ammo < magazineSize && !reloading)
            {
                StartCoroutine(Reload());
            }

            if (shootPressed)
            {
                TryShoot();
            }
        }

        private void TryShoot()
        {
            if (reloading || Time.time < nextShotTime)
            {
                return;
            }

            if (ammo <= 0)
            {
                StartCoroutine(Reload());
                return;
            }

            ammo--;
            nextShotTime = Time.time + shotCooldown;
            RefreshUI();

            Ray ray = gameplayCamera.ScreenPointToRay(crosshair.ScreenPosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 200f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                EnemyHitbox hitbox = hit.collider.GetComponent<EnemyHitbox>();
                if (hitbox != null)
                {
                    hitbox.ReceiveHit(damage);
                }
            }

            if (ammo <= 0)
            {
                StartCoroutine(Reload());
            }
        }

        private IEnumerator Reload()
        {
            reloading = true;
            if (reloadText != null)
            {
                reloadText.gameObject.SetActive(true);
                reloadText.text = "RELOADING";
            }

            yield return new WaitForSeconds(reloadDuration);
            ammo = magazineSize;
            reloading = false;
            if (reloadText != null)
            {
                reloadText.gameObject.SetActive(false);
            }
            RefreshUI();
        }

        private void RefreshUI()
        {
            if (ammoText != null)
            {
                ammoText.text = ammo + " / " + magazineSize;
            }
        }
    }
}
