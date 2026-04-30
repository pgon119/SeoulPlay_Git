using UnityEngine;

namespace SeoulPlay
{
    public sealed class SeoulPlayProjectile : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float speed = 45f;
        [SerializeField, Min(0f)] private float damage = 10f;
        [SerializeField, Min(0.1f)] private float lifetime = 2f;
        [SerializeField, Min(0f)] private float gravity;
        [SerializeField, Min(0f)] private float spinDegreesPerSecond;

        private Vector3 direction = Vector3.forward;
        private Vector3 velocity;
        private Transform ignoredRoot;
        private float age;

        public void Launch(
            Vector3 launchDirection,
            float launchSpeed,
            float projectileDamage,
            float projectileLifetime,
            Transform owner = null)
        {
            direction = launchDirection.sqrMagnitude > 0.001f ? launchDirection.normalized : transform.forward;
            speed = launchSpeed;
            damage = projectileDamage;
            lifetime = projectileLifetime;
            ignoredRoot = owner;
            velocity = direction * speed;
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }

        public void ConfigureMotion(float projectileGravity, float projectileSpinDegreesPerSecond = 0f)
        {
            gravity = Mathf.Max(0f, projectileGravity);
            spinDegreesPerSecond = Mathf.Max(0f, projectileSpinDegreesPerSecond);
        }

        private void Update()
        {
            if (gravity > 0f)
            {
                velocity += Physics.gravity.normalized * gravity * Time.deltaTime;
                if (velocity.sqrMagnitude > 0.001f)
                {
                    direction = velocity.normalized;
                }
            }

            transform.position += velocity * Time.deltaTime;

            if (spinDegreesPerSecond > 0f)
            {
                transform.Rotate(Vector3.right, spinDegreesPerSecond * Time.deltaTime, Space.Self);
            }
            else if (direction.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            }

            age += Time.deltaTime;
            if (age >= lifetime)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (ignoredRoot != null && other.transform.IsChildOf(ignoredRoot))
            {
                return;
            }

            var damageable = other.GetComponentInParent<SeoulPlayDamageable>();
            if (damageable != null && damageable.IsAlive)
            {
                damageable.TakeDamage(damage, direction, ignoredRoot);
            }

            if (other.attachedRigidbody != null)
            {
                other.attachedRigidbody.AddForce(direction * damage, ForceMode.Impulse);
            }

            Destroy(gameObject);
        }
    }
}
