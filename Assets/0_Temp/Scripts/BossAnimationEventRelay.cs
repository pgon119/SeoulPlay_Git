using UnityEngine;

namespace SeoulPlay
{
    [DisallowMultipleComponent]
    public sealed class BossAnimationEventRelay : MonoBehaviour
    {
        private BossAttackController attackController;

        public void Initialize(BossAttackController controller)
        {
            attackController = controller;
        }

        private BossAttackController ResolveController()
        {
            if (attackController == null)
            {
                attackController = GetComponentInParent<BossAttackController>();
            }

            return attackController;
        }

        public void CreateAttack1RockClone()
        {
            ResolveController()?.CreateAttack1RockClone();
        }

        public void FireAttack1Rock()
        {
            ResolveController()?.FireAttack1Rock();
        }

        public void FireAttack1BulletFan()
        {
            ResolveController()?.FireAttack1BulletFan();
        }

        public void Attack01_Hit()
        {
            ResolveController()?.Attack01_Hit();
        }

        public void Attack01_ThrowRock()
        {
            ResolveController()?.Attack01_ThrowRock();
        }

        public void Attack01_End()
        {
            ResolveController()?.Attack01_End();
        }

        public void FireAttack2EarthBlast()
        {
            ResolveController()?.FireAttack2EarthBlast();
        }

        public void Attack02_Hit()
        {
            ResolveController()?.Attack02_Hit();
        }

        public void StartAttack3JumpSlamMove()
        {
            ResolveController()?.StartAttack3JumpSlamMove();
        }

        public void FireAttack3JumpSlam()
        {
            ResolveController()?.FireAttack3JumpSlam();
        }

        public void FireAttack3ImpactVfx()
        {
            ResolveController()?.FireAttack3ImpactVfx();
        }

        public void DamageAttack3Impact()
        {
            ResolveController()?.DamageAttack3Impact();
        }

        public void Attack03_Jump()
        {
            ResolveController()?.Attack03_Jump();
        }

        public void Attack03_Hit()
        {
            ResolveController()?.Attack03_Hit();
        }

        public void Attack03_hit()
        {
            ResolveController()?.Attack03_hit();
        }

        public void Attack03_Effect()
        {
            ResolveController()?.Attack03_Effect();
        }

        public void Attack03_Damage()
        {
            ResolveController()?.Attack03_Damage();
        }

        public void AttackSignal()
        {
            ResolveController()?.AttackSignal();
        }

        public void Enrage_Start()
        {
            ResolveController()?.Enrage_Start();
        }

        public void FootL()
        {
        }

        public void FootR()
        {
        }
    }
}
