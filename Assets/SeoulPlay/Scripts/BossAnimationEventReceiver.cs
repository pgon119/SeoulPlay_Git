using UnityEngine;

namespace SeoulPlay
{
    public sealed class BossAnimationEventReceiver : MonoBehaviour
    {
        [SerializeField] private BossAttackController attackController;

        private void Awake()
        {
            if (attackController == null)
            {
                attackController = GetComponentInParent<BossAttackController>();
            }
        }

        public void Attack01_Hit()
        {
            Attack01_FireBulletFan();
        }

        public void Attack01_ShowRock()
        {
            Attack01_CreateRock();
        }

        public void CreateRock()
        {
            Attack01_CreateRock();
        }

        public void CreateRockClone()
        {
            Attack01_CreateRock();
        }

        public void CreateAttack1RockClone()
        {
            if (attackController != null)
            {
                attackController.CreateAttack1RockClone();
            }
        }

        public void SpawnRock()
        {
            Attack01_CreateRock();
        }

        public void SpawnRockClone()
        {
            Attack01_CreateRock();
        }

        public void ShowRock()
        {
            Attack01_CreateRock();
        }

        public void Attack01_CreateRock()
        {
            if (attackController != null)
            {
                attackController.CreateAttack1RockClone();
            }
        }

        public void Attack01_ThrowRock()
        {
            Attack01_FireBulletFan();
        }

        public void ThrowRock()
        {
            Attack01_ThrowRock();
        }

        public void Attack01_FireBulletFan()
        {
            if (attackController != null)
            {
                attackController.FireAttack1BulletFan();
            }
        }

        public void FireBulletFan()
        {
            Attack01_FireBulletFan();
        }

        public void Attack01_HideRock()
        {
            if (attackController != null)
            {
                attackController.HideHeldRock();
            }
        }

        public void Attack01_End()
        {
            if (attackController != null)
            {
                attackController.FinishAttack();
            }
        }

        public void FinishAttack()
        {
            Attack01_End();
        }

        public void Attack02_Hit()
        {
            if (attackController != null)
            {
                attackController.FireAttack2EarthBlast();
            }
        }

        public void Attack03_Hit()
        {
            if (attackController != null)
            {
                attackController.FireAttack3JumpSlam();
            }
        }

        public void Attack03_Effect()
        {
            if (attackController != null)
            {
                attackController.FireAttack3ImpactVfx();
            }
        }

        public void Attack03_Vfx()
        {
            Attack03_Effect();
        }

        public void Attack03_Damage()
        {
            if (attackController != null)
            {
                attackController.DamageAttack3Impact();
            }
        }

        public void Attack03_Slam()
        {
            Attack03_Hit();
        }

        public void AttackSignal()
        {
        }

        public void Enrage_Start()
        {
        }
    }
}
