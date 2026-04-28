using UnityEngine;

namespace SeoulPlay
{
    public sealed class SeoulPlayWeapon : MonoBehaviour
    {
        [SerializeField] private string weaponName = "Prototype Rifle";
        [SerializeField] private Transform muzzle;
        [SerializeField, Min(0f)] private float damage = 10f;
        [SerializeField, Min(0.01f)] private float fireRate = 8f;
        [SerializeField, Min(0f)] private float range = 60f;

        public string WeaponName => weaponName;
        public Transform Muzzle => muzzle != null ? muzzle : transform;
        public float Damage => damage;
        public float FireRate => fireRate;
        public float Range => range;

        public void SetMuzzle(Transform value)
        {
            muzzle = value;
        }

        private void OnDrawGizmosSelected()
        {
            var origin = Muzzle;
            if (origin == null)
            {
                return;
            }

            Gizmos.color = Color.red;
            Gizmos.DrawRay(origin.position, origin.forward * 0.5f);
        }
    }
}
