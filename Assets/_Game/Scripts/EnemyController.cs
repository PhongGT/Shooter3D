using UnityEngine;

namespace Shooter3D
{
    public sealed class EnemyController : MonoBehaviour
    {
        [SerializeField] private float activationTime;
        [SerializeField] private float visibleDuration = 6f;
        [SerializeField] private float threatDelay = 3f;
        [SerializeField] private int scoreValue = 100;
        [SerializeField] private bool civilian;
        [SerializeField] private Transform dangerFill;

        private Renderer[] cachedRenderers;
        private Collider[] cachedColliders;
        private Vector3 standingPosition;
        private Vector3 hiddenPosition;
        private float health;
        private float activeTime;
        private float threatTime;
        private bool active;

        public float ActivationTime => activationTime;
        public bool Triggered { get; private set; }

        public void Configure(float startTime, float duration, float dangerDelay, int points, bool isCivilian, Transform fill)
        {
            activationTime = startTime;
            visibleDuration = duration;
            threatDelay = dangerDelay;
            scoreValue = points;
            civilian = isCivilian;
            dangerFill = fill;
        }

        public void Prepare()
        {
            cachedRenderers = GetComponentsInChildren<Renderer>(true);
            cachedColliders = GetComponentsInChildren<Collider>(true);
            standingPosition = transform.position;
            hiddenPosition = standingPosition + Vector3.down * 1.1f;
            health = 100f;
            active = false;
            Triggered = false;
            SetVisible(false);
        }

        public void ActivateTarget()
        {
            if (Triggered)
            {
                return;
            }

            Triggered = true;
            active = true;
            activeTime = 0f;
            threatTime = 0f;
            transform.position = hiddenPosition;
            SetVisible(true);
            SetDanger(0f);
        }

        private void Update()
        {
            if (!active)
            {
                return;
            }

            activeTime += Time.deltaTime;
            float reveal = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(activeTime / 0.45f));
            transform.position = Vector3.Lerp(hiddenPosition, standingPosition, reveal);

            if (!civilian)
            {
                threatTime += Time.deltaTime;
                SetDanger(Mathf.Clamp01(threatTime / threatDelay));
                if (threatTime >= threatDelay)
                {
                    GameManager.Instance?.DamagePlayer(12);
                    threatTime = 0f;
                }
            }

            if (activeTime >= visibleDuration)
            {
                DeactivateTarget();
            }
        }

        public void ReceiveHit(float baseDamage, HitZone zone)
        {
            if (!active)
            {
                return;
            }

            if (civilian)
            {
                GameManager.Instance?.RegisterCivilianHit();
                DeactivateTarget();
                return;
            }

            float multiplier = zone == HitZone.Head ? 2f : zone == HitZone.Arm ? 1.25f : 1f;
            health -= baseDamage * multiplier;
            if (health <= 0f)
            {
                int awarded = Mathf.RoundToInt(scoreValue * multiplier);
                GameManager.Instance?.RegisterEnemyDown(awarded, zone);
                DeactivateTarget();
            }
        }

        private void DeactivateTarget()
        {
            active = false;
            SetVisible(false);
        }

        private void SetVisible(bool value)
        {
            if (cachedRenderers == null)
            {
                cachedRenderers = GetComponentsInChildren<Renderer>(true);
            }
            if (cachedColliders == null)
            {
                cachedColliders = GetComponentsInChildren<Collider>(true);
            }

            foreach (Renderer item in cachedRenderers)
            {
                item.enabled = value;
            }
            foreach (Collider item in cachedColliders)
            {
                item.enabled = value;
            }
        }

        private void SetDanger(float normalized)
        {
            if (dangerFill == null)
            {
                return;
            }

            Vector3 scale = dangerFill.localScale;
            scale.x = Mathf.Max(0.001f, normalized);
            dangerFill.localScale = scale;
            dangerFill.localPosition = new Vector3((normalized - 1f) * 0.45f, dangerFill.localPosition.y, dangerFill.localPosition.z);
        }
    }
}
