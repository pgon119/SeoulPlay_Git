using UnityEngine;

namespace SeoulPlay
{
    [DisallowMultipleComponent]
    public sealed class SeoulPlayWeaponHolder : MonoBehaviour
    {
        private static readonly string[] RightHandFallbackNames =
        {
            "hand_r",
            "RightHand",
            "mixamorig:RightHand",
            "Bip001 R Hand"
        };

        [Header("References")]
        [SerializeField] private Animator animator;
        [SerializeField] private SimpleHeroMover heroMover;
        [SerializeField] private GameObject weaponPrefab;
        [SerializeField] private Transform weaponMount;

        [Header("Equip Point")]
        [SerializeField] private HumanBodyBones mountBone = HumanBodyBones.RightHand;
        [SerializeField] private string mountName = "RightWeaponMount";
        [SerializeField] private Vector3 mountLocalPosition = new(0.05f, 0.02f, 0.08f);
        [SerializeField] private Vector3 mountLocalEulerAngles = new(0f, 90f, 0f);

        [Header("Fire Equip Point")]
        [SerializeField] private Vector3 fireMountLocalPosition = new(0.05f, 0.02f, 0.08f);
        [SerializeField] private Vector3 fireMountLocalEulerAngles = new(0f, 90f, 0f);
        [SerializeField, Min(0f)] private float mountPoseBlendSpeed = 18f;

        [Header("Equipped Weapon")]
        [SerializeField] private bool equipOnAwake = true;
        [SerializeField, Min(0f)] private float defaultWeaponDamage = 1f;
        [SerializeField] private string preferredMuzzleName = "Muzzle";
        [SerializeField] private Vector3 weaponLocalPosition = Vector3.zero;
        [SerializeField] private Vector3 weaponLocalEulerAngles = Vector3.zero;
        [SerializeField] private Vector3 weaponLocalScale = Vector3.one;

        [Header("Prototype")]
        [SerializeField] private bool createPrototypeCubeIfMissing = true;
        [SerializeField] private Vector3 prototypeLocalPosition = new(0f, 0f, 0.18f);
        [SerializeField] private Vector3 prototypeLocalEulerAngles = Vector3.zero;
        [SerializeField] private Vector3 prototypeLocalScale = new(0.12f, 0.16f, 0.55f);

        private SeoulPlayWeapon equippedWeapon;
        private float forcedFirePoseTimer;
        public SeoulPlayWeapon EquippedWeapon => equippedWeapon;
        public Transform WeaponMount => weaponMount;

        private void Awake()
        {
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            if (heroMover == null)
            {
                heroMover = GetComponent<SimpleHeroMover>();
            }

            EnsureWeaponMount();
            ApplyMountPose(false);

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

        private void LateUpdate()
        {
            ApplyMountPose(true);
            forcedFirePoseTimer = Mathf.Max(0f, forcedFirePoseTimer - Time.deltaTime);
        }

        public void SnapMountToFirePose(float duration = 0.08f)
        {
            forcedFirePoseTimer = Mathf.Max(forcedFirePoseTimer, duration);
            ApplyMountPose(false);
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

            EnsureMuzzle(weaponObject.transform);
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
            EnsureMuzzle(weaponObject.transform);
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
            var parent = GetMountParent();
            if (parent == null)
            {
                parent = transform;
            }

            if (weaponMount != null)
            {
                if (weaponMount.parent != parent)
                {
                    weaponMount.SetParent(parent, false);
                    ApplyMountPose(false);
                }

                return;
            }

            var existing = parent.Find(mountName);
            weaponMount = existing != null ? existing : new GameObject(mountName).transform;
            weaponMount.SetParent(parent, false);
            weaponMount.localPosition = mountLocalPosition;
            weaponMount.localEulerAngles = mountLocalEulerAngles;
            weaponMount.localScale = Vector3.one;
        }

        private void ApplyMountPose(bool interpolate)
        {
            if (weaponMount == null)
            {
                return;
            }

            var useFirePose = forcedFirePoseTimer > 0f || (heroMover != null && heroMover.IsWeaponFirePoseActive);
            var targetPosition = useFirePose ? fireMountLocalPosition : mountLocalPosition;
            var targetRotation = Quaternion.Euler(useFirePose ? fireMountLocalEulerAngles : mountLocalEulerAngles);

            if (!interpolate || mountPoseBlendSpeed <= 0f)
            {
                weaponMount.localPosition = targetPosition;
                weaponMount.localRotation = targetRotation;
                weaponMount.localScale = Vector3.one;
                return;
            }

            var blend = 1f - Mathf.Exp(-mountPoseBlendSpeed * Time.deltaTime);
            weaponMount.localPosition = Vector3.Lerp(weaponMount.localPosition, targetPosition, blend);
            weaponMount.localRotation = Quaternion.Slerp(weaponMount.localRotation, targetRotation, blend);
            weaponMount.localScale = Vector3.one;
        }

        private Transform GetMountParent()
        {
            Transform humanoidBone = null;
            if (animator != null && animator.isHuman && animator.avatar != null && animator.avatar.isValid)
            {
                humanoidBone = animator.GetBoneTransform(mountBone);
            }

            if (humanoidBone != null)
            {
                return humanoidBone;
            }

            var searchRoot = animator != null ? animator.transform : transform;
            foreach (var fallbackName in RightHandFallbackNames)
            {
                var fallback = FindChildRecursive(searchRoot, fallbackName);
                if (fallback != null)
                {
                    return fallback;
                }
            }

            return null;
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

        private void EnsureMuzzle(Transform weaponRoot)
        {
            if (equippedWeapon == null || weaponRoot == null || equippedWeapon.Muzzle != weaponRoot)
            {
                return;
            }

            var muzzle = FindChildRecursive(weaponRoot, preferredMuzzleName);
            if (muzzle == null)
            {
                muzzle = FindChildRecursive(weaponRoot, "Muzzle");
            }

            if (muzzle == null)
            {
                muzzle = FindChildRecursive(weaponRoot, "FireBullet_FireMuzzle_FxPosition");
            }

            if (muzzle == null)
            {
                muzzle = new GameObject("Muzzle").transform;
                muzzle.SetParent(weaponRoot, false);
                muzzle.localPosition = new Vector3(0f, 0f, 0.6f);
                muzzle.localRotation = Quaternion.identity;
            }

            equippedWeapon.SetMuzzle(muzzle);
        }

        private static Transform FindChildRecursive(Transform root, string childName)
        {
            if (root == null || string.IsNullOrWhiteSpace(childName))
            {
                return null;
            }

            for (var i = 0; i < root.childCount; i++)
            {
                var child = root.GetChild(i);
                if (child.name == childName)
                {
                    return child;
                }

                var nested = FindChildRecursive(child, childName);
                if (nested != null)
                {
                    return nested;
                }
            }

            return null;
        }
    }
}
