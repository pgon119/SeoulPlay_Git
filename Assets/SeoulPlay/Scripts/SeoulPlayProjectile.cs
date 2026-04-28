using UnityEngine;

namespace SeoulPlay
{
    [RequireComponent(typeof(Collider))]
    public sealed class SeoulPlayProjectile : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float speed = 45f;
        [SerializeField, Min(0f)] private float damage = 10f;
        [SerializeField, Min(0.1f)] private float lifetime = 2f;

        private Vector3 direction = Vector3.forward;
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
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }

        private void Update()
        {
            var distance = speed * Time.deltaTime;
            transform.position += direction * distance;

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

            if (other.attachedRigidbody != null)
            {
                other.attachedRigidbody.AddForce(direction * damage, ForceMode.Impulse);
            }

            Destroy(gameObject);
        }
    }
}
