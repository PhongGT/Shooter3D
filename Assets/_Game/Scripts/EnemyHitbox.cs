using UnityEngine;

namespace Shooter3D
{
    public enum HitZone
    {
        Body,
        Head,
        Arm
    }

    public sealed class EnemyHitbox : MonoBehaviour
    {
        [SerializeField] private EnemyController owner;
        [SerializeField] private HitZone zone;

        public void Configure(EnemyController target, HitZone hitZone)
        {
            owner = target;
            zone = hitZone;
        }

        public void ReceiveHit(float damage)
        {
            if (owner != null)
            {
                owner.ReceiveHit(damage, zone);
            }
        }
    }
}
