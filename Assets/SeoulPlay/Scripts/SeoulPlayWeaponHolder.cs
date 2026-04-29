using UnityEngine;

namespace SeoulPlay
{
    [DisallowMultipleComponent]
    public sealed class SeoulPlayWeaponHolder : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Animator animator;
        [SerializeField] private GameObject weaponPrefab;
        [SerializeField] private Transform weaponMount;

        [Header("Equip Point")]
        [SerializeField] private HumanBodyBones mountBone = HumanBodyBones.RightHand;
        [SerializeField] private string mountName = "RightWeaponMount";
        [SerializeField] private Vector3 mountLocalPosition = new(0.05f, 0.02f, 0.08f);
        [SerializeField] private Vector3 mountLocalEulerAngles = new(0f, 90f, 0f);

        [Header("Equipped Weapon")]
        [SerializeField] private bool equipOnAwake = true;
        [SerializeField, Min(0f)] private float defaultWeaponDamage = 1f;
        [SerializeField] private Vector3 weaponLocalPosition = Vector3.zero;
        [SerializeField] private Vector3 weaponLocalEulerAngles = Vector3.zero;
        [SerializeField] private Vector3 weaponLocalScale = Vector3.one;

        [Header("Prototype")]
        [SerializeField] private bool createPrototypeCubeIfMissing = true;
        [SerializeField] private Vector3 prototypeLocalPosition = new(0f, 0f, 0.18f);
        [SerializeField] private Vector3 prototypeLocalEulerAngles = Vector3.zero;
        [SerializeField] private Vector3 prototypeLocalScale = new(0.12f, 0.16f, 0.55f);

        private SeoulPlayWeapon equippedWeapon;
        public SeoulPlayWeapon EquippedWeapon => equippedWeapon;
        public Transform WeaponMount => weaponMount;

        private void Awake()
        {
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            EnsureWeaponMount();

            if (equipOnAwake)
            {
                EquipDefaultWeapon();
            }
        }

        private void Start()
        {
            if (equipOnAwake && equippedWeapon == null)
            {
                EquipDefaultWeapon();
            }
        }

        public void EquipDefaultWeapon()
        {
            if (weaponPrefab != null)
            {
                Equip(weaponPrefab);
                return;
            }

            if (createPrototypeCubeIfMissing)
            {
                EquipPrototypeWeapon();
            }
        }

        public void Equip(GameObject prefab)
        {
            if (prefab == null)
            {
                return;
            }

            EnsureWeaponMount();
            Unequip();

            var weaponObject = Instantiate(prefab, weaponMount);
            weaponObject.name = prefab.name;
            weaponObject.transform.localPosition = weaponLocalPosition;
            weaponObject.transform.localEulerAngles = weaponLocalEulerAngles;
            weaponObject.transform.localScale = weaponLocalScale;

            equippedWeapon = weaponObject.GetComponent<SeoulPlayWeapon>();
            if (equippedWeapon == null)
            {
                equippedWeapon = weaponObject.AddComponent<SeoulPlayWeapon>();
            }

            equippedWeapon.SetDamage(defaultWeaponDamage);
        }

        public void EquipPrototypeWeapon()
        {
            EnsureWeaponMount();
            Unequip();

            var weaponObject = CreatePrototypeWeapon();
            weaponObject.transform.SetParent(weaponMount, false);
            weaponObject.transform.localPosition = prototypeLocalPosition;
            weaponObject.transform.localEulerAngles = prototypeLocalEulerAngles;
            weaponObject.transform.localScale = prototypeLocalScale;
            equippedWeapon = weaponObject.GetComponent<SeoulPlayWeapon>();
            equippedWeapon.SetDamage(defaultWeaponDamage);
        }

        public void Unequip()
        {
            if (equippedWeapon == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(equippedWeapon.gameObject);
            }
            else
            {
                DestroyImmediate(equippedWeapon.gameObject);
            }

            equippedWeapon = null;
        }

        private void EnsureWeaponMount()
        {
            if (weaponMount != null)
            {
                return;
            }

            var parent = GetMountParent();
            if (parent == null)
            {
                parent = transform;
            }

            var existing = parent.Find(mountName);
            weaponMount = existing != null ? existing : new GameObject(mountName).transform;
            weaponMount.SetParent(parent, false);
            weaponMount.localPosition = mountLocalPosition;
            weaponMount.localEulerAngles = mountLocalEulerAngles;
            weaponMount.localScale = Vector3.one;
        }

        private Transform GetMountParent()
        {
            if (animator == null)
            {
                return null;
            }

            if (!animator.isHuman || animator.avatar == null || !animator.avatar.isValid)
            {
                return null;
            }

            return animator.GetBoneTransform(mountBone);
        }

        private GameObject CreatePrototypeWeapon()
        {
            var weaponObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            weaponObject.name = "Prototype Cube Rifle";

            var collider = weaponObject.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            var muzzle = new GameObject("Muzzle").transform;
            muzzle.SetParent(weaponObject.transform, false);
            muzzle.localPosition = new Vector3(0f, 0f, 0.6f);
            muzzle.localRotation = Quaternion.identity;

            var weapon = weaponObject.AddComponent<SeoulPlayWeapon>();
            weapon.SetMuzzle(muzzle);
            return weaponObject;
        }
    }
}
