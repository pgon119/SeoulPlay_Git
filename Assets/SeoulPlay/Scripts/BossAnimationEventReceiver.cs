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
            Attack01_CreateRock();
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
            if (attackController != null)
            {
                attackController.FireAttack1Rock();
            }
        }

        public void ThrowRock()
        {
            Attack01_ThrowRock();
        }

        public void Attack01_HideRock()
        {
            if (attackController != null)
            {
                attackController.HideHeldRock();
            }
        }

        public void Attack02_Hit()
        {
        }

        public void Attack03_Hit()
        {
        }

        public void AttackSignal()
        {
        }

        public void Enrage_Start()
        {
        }
    }
}
