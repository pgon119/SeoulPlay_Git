using UnityEngine;
using UnityEngine.Events;

namespace SeoulPlay
{
    public sealed class SeoulPlayDamageable : MonoBehaviour
    {
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
        [SerializeField] private UnityEvent<float> onDamaged = new UnityEvent<float>();
        [SerializeField] private UnityEvent onDeath = new UnityEvent();

        private bool dead;

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

        private void OnValidate()
        {
            maxHealth = Mathf.Max(1f, maxHealth);
            currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        }

        public void TakeDamage(float damage)
        {
            TakeDamage(damage, Vector3.zero, null);
        }

        public void TakeDamage(float damage, Vector3 hitDirection, Transform attacker)
        {
            if (dead || damage <= 0f)
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

            if (playHitReaction)
            {
                SetAnimatorTrigger(hitTrigger);
            }
        }

        private void Die()
        {
            if (dead)
            {
                return;
            }

            dead = true;
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
    }
}
