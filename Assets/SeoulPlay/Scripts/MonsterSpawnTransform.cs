using UnityEngine;

namespace SeoulPlay
{
    [DisallowMultipleComponent]
    public sealed class MonsterSpawnTransform : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform bossTransform;
        [SerializeField] private bool useLocalSpace = true;

        [Header("Monster_Boss_1 Start Transform")]
        [SerializeField] private Vector3 bossPosition;
        [SerializeField] private Vector3 bossEulerAngles;

        private void Awake()
        {
            ApplyBossTransform();
        }

        private void Reset()
        {
            bossTransform = FindBossTransform();
        }

        private void OnValidate()
        {
            if (bossTransform == null)
            {
                bossTransform = FindBossTransform();
            }
        }

        public void ApplyBossTransform()
        {
            if (bossTransform == null)
            {
                bossTransform = FindBossTransform();
            }

            if (bossTransform == null)
            {
                return;
            }

            var rotation = Quaternion.Euler(bossEulerAngles);

            if (useLocalSpace)
            {
                bossTransform.localPosition = bossPosition;
                bossTransform.localRotation = rotation;
                return;
            }

            bossTransform.SetPositionAndRotation(bossPosition, rotation);
        }

        private Transform FindBossTransform()
        {
            foreach (Transform child in transform)
            {
                if (child.name == "Monster_Boss_1")
                {
                    return child;
                }
            }

            return transform.Find("Monster_Boss_1");
        }
    }
}
