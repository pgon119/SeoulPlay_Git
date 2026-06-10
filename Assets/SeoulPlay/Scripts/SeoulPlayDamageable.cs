using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace SeoulPlay
{
    public sealed class SeoulPlayDamageable : MonoBehaviour
    {
        private const float HeavyHitDamageThreshold = 20f;
        private const float HeavyHitFallbackDuration = 0.55f;
        private const string HeavyHitTrigger = "HeavyHit";
        private const int BaseLayerIndex = 0;

        [SerializeField, Min(1f)] private float maxHealth = 300f;
        [SerializeField, Min(0f)] private float currentHealth = 300f;
        [SerializeField] private bool fillHealthOnAwake = true;
        [SerializeField] private bool disableCollidersOnDeath = true;
        [SerializeField] private bool destroyOnDeath;
        [SerializeField, Min(0f)] private float destroyDelay = 3f;
        [SerializeField] private Animator animator;
        [SerializeField] private bool playHitReaction;
        [SerializeField] private string hitTrigger = "Hit";
        [SerializeField] private string deathTrigger = "Die";
        [SerializeField] private bool blockFireOnHit = true;
        [SerializeField, Min(0f)] private float hitFireLockoutDuration = 0.35f;
        [SerializeField, Min(0f)] private float heavyHitKnockbackDistance = 1.6f;
        [SerializeField, Min(0f)] private float postKnockbackInvincibleDuration = 0.5f;
        [SerializeField] private UnityEvent<float> onDamaged = new UnityEvent<float>();
        [SerializeField] private UnityEvent onDeath = new UnityEvent();

        private bool dead;
        private Coroutine heavyHitRoutine;
        private float invincibleTimer;

        public float MaxHealth => maxHealth;
        public float CurrentHealth => currentHealth;
        public bool IsAlive => !dead && currentHealth > 0f;

        private void Reset()
        {
            animator = GetComponentInChildren<Animator>();
            currentHealth = maxHealth;
        }

        private void Awake()
        {
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            if (fillHealthOnAwake)
            {
                currentHealth = maxHealth;
            }

            dead = currentHealth <= 0f;
        }

        private void Update()
        {
            invincibleTimer = Mathf.Max(0f, invincibleTimer - Time.deltaTime);
        }

        private void OnValidate()
        {
            maxHealth = Mathf.Max(1f, maxHealth);
            currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        }

        public void TakeDamage(float damage)
        {
            TakeDamage(damage, Vector3.zero, null);
        }

        public void ResetHealth()
        {
            if (heavyHitRoutine != null)
            {
                StopCoroutine(heavyHitRoutine);
                heavyHitRoutine = null;
            }

            currentHealth = maxHealth;
            dead = false;
            invincibleTimer = 0f;
            SetDeathFireBlocked(false);

            foreach (var targetCollider in GetComponentsInChildren<Collider>())
            {
                targetCollider.enabled = true;
            }
        }

        public void TakeDamage(float damage, Vector3 hitDirection, Transform attacker)
        {
            if (dead || damage <= 0f || invincibleTimer > 0f)
            {
                return;
            }

            currentHealth = Mathf.Max(0f, currentHealth - damage);
            onDamaged.Invoke(damage);

            if (currentHealth <= 0f)
            {
                Die();
                return;
            }

            if (IsHeavyHit(damage))
            {
                PlayHeavyHitReaction(hitDirection, attacker);
                return;
            }

            if (playHitReaction)
            {
                SetAnimatorTrigger(hitTrigger);
            }

            if (blockFireOnHit)
            {
                ApplyFireLockout(hitFireLockoutDuration);
            }
        }

        private bool IsHeavyHit(float damage)
        {
            return heavyHitKnockbackDistance > 0f && damage >= HeavyHitDamageThreshold;
        }

        private void PlayHeavyHitReaction(Vector3 hitDirection, Transform attacker)
        {
            if (playHitReaction)
            {
                SetAnimatorTrigger(HeavyHitTrigger);
            }

            if (blockFireOnHit)
            {
                ApplyFireLockout(HeavyHitFallbackDuration);
            }

            var knockbackDirection = ResolveKnockbackDirection(hitDirection, attacker);
            if (heavyHitRoutine != null)
            {
                StopCoroutine(heavyHitRoutine);
            }

            heavyHitRoutine = StartCoroutine(ApplyHeavyHitKnockbackAfterAnimatorUpdate(knockbackDirection));
        }

        private IEnumerator ApplyHeavyHitKnockbackAfterAnimatorUpdate(Vector3 knockbackDirection)
        {
            yield return null;
            if (dead)
            {
                heavyHitRoutine = null;
                yield break;
            }

            var duration = ResolveHeavyHitAnimationDuration();
            if (blockFireOnHit)
            {
                ApplyFireLockout(duration);
            }

            var heroMover = ResolveComponent<SimpleHeroMover>();
            if (heroMover != null)
            {
                heroMover.ApplyKnockback(knockbackDirection, heavyHitKnockbackDistance, duration);
            }

            if (postKnockbackInvincibleDuration > 0f)
            {
                yield return new WaitForSeconds(duration);
                invincibleTimer = Mathf.Max(invincibleTimer, postKnockbackInvincibleDuration);
            }

            heavyHitRoutine = null;
        }

        private float ResolveHeavyHitAnimationDuration()
        {
            if (animator == null || animator.runtimeAnimatorController == null)
            {
                return HeavyHitFallbackDuration;
            }

            if (BaseLayerIndex >= animator.layerCount)
            {
                return HeavyHitFallbackDuration;
            }

            var stateInfo = animator.GetCurrentAnimatorStateInfo(BaseLayerIndex);
            if (stateInfo.length > 0.01f && !stateInfo.loop)
            {
                return stateInfo.length;
            }

            var clips = animator.GetCurrentAnimatorClipInfo(BaseLayerIndex);
            var duration = 0f;
            for (var i = 0; i < clips.Length; i++)
            {
                if (clips[i].clip != null)
                {
                    duration = Mathf.Max(duration, clips[i].clip.length);
                }
            }

            return duration > 0.01f ? duration : HeavyHitFallbackDuration;
        }

        private Vector3 ResolveKnockbackDirection(Vector3 hitDirection, Transform attacker)
        {
            hitDirection.y = 0f;
            if (hitDirection.sqrMagnitude > 0.001f)
            {
                return hitDirection.normalized;
            }

            if (attacker != null)
            {
                var attackerDirection = transform.position - attacker.position;
                attackerDirection.y = 0f;
                if (attackerDirection.sqrMagnitude > 0.001f)
                {
                    return attackerDirection.normalized;
                }
            }

            return -transform.forward;
        }

        private void ApplyFireLockout(float duration)
        {
            if (duration <= 0f)
            {
                return;
            }

            var shooter = ResolveComponent<SeoulPlayShooter>();
            if (shooter != null)
            {
                shooter.ApplyHitFireLockout(duration);
            }

            var heroMover = ResolveComponent<SimpleHeroMover>();
            if (heroMover != null)
            {
                heroMover.ApplyHitFireLockout(duration);
            }
        }

        private void Die()
        {
            if (dead)
            {
                return;
            }

            dead = true;
            if (heavyHitRoutine != null)
            {
                StopCoroutine(heavyHitRoutine);
                heavyHitRoutine = null;
            }

            invincibleTimer = 0f;
            SetDeathFireBlocked(true);
            SetAnimatorTrigger(deathTrigger);
            onDeath.Invoke();

            if (disableCollidersOnDeath)
            {
                foreach (var targetCollider in GetComponentsInChildren<Collider>())
                {
                    targetCollider.enabled = false;
                }
            }

            if (destroyOnDeath)
            {
                Destroy(gameObject, destroyDelay);
            }
        }

        private void SetDeathFireBlocked(bool blocked)
        {
            var shooter = ResolveComponent<SeoulPlayShooter>();
            if (shooter != null)
            {
                shooter.SetDeathFireBlocked(blocked);
            }

            var heroMover = ResolveComponent<SimpleHeroMover>();
            if (heroMover != null)
            {
                heroMover.SetDeathFireBlocked(blocked);
            }
        }

        private void SetAnimatorTrigger(string triggerName)
        {
            if (animator != null && HasAnimatorTrigger(triggerName))
            {
                animator.SetTrigger(triggerName);
            }
        }

        private bool HasAnimatorTrigger(string triggerName)
        {
            if (string.IsNullOrEmpty(triggerName) || animator.runtimeAnimatorController == null)
            {
                return false;
            }

            foreach (var parameter in animator.parameters)
            {
                if (parameter.type == AnimatorControllerParameterType.Trigger && parameter.name == triggerName)
                {
                    return true;
                }
            }

            return false;
        }

        private T ResolveComponent<T>() where T : Component
        {
            var target = GetComponentInParent<T>();
            if (target != null)
            {
                return target;
            }

            target = GetComponentInChildren<T>();
            if (target != null)
            {
                return target;
            }

            return transform.root != null ? transform.root.GetComponentInChildren<T>() : null;
        }
    }
}
