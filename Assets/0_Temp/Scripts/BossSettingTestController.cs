using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SeoulPlay
{
    public sealed class BossSettingTestController : MonoBehaviour
    {
        private const string TestSceneName = "BossSettingTest";
        private const string BossPrefabPath = "Assets/SeoulPlay/Prefab/Boss_1/Monster_Boss_1.prefab";

        [SerializeField] private GameObject bossPrefab;
        [SerializeField] private Vector3 bossPosition = new Vector3(0f, 0f, 11f);
        [SerializeField] private Vector3 bossEulerAngles = new Vector3(0f, 180f, 0f);
        [SerializeField] private Vector3 dummyPosition = Vector3.zero;
        [SerializeField] private bool hideLooseSceneBossObjects = true;
        [SerializeField] private bool restartSkillButtons = true;

        private BossAttackController boss;
        private Transform dummyTarget;
        private Vector3 initialBossPosition;
        private Quaternion initialBossRotation;
        private Vector3 initialDummyPosition;
        private Quaternion initialDummyRotation;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (SceneManager.GetActiveScene().name != TestSceneName)
            {
                return;
            }

            if (FindObjectOfType<BossSettingTestController>() != null)
            {
                return;
            }

            new GameObject("BossSettingTestController").AddComponent<BossSettingTestController>();
        }

        private void Awake()
        {
            if (SceneManager.GetActiveScene().name != TestSceneName)
            {
                enabled = false;
                return;
            }

            Setup();
        }

        private void Setup()
        {
            dummyTarget = FindOrCreateDummyTarget();
            boss = FindObjectOfType<BossAttackController>();

            if (boss == null)
            {
                if (hideLooseSceneBossObjects)
                {
                    HideLooseBossObjects();
                }

                bossPrefab = bossPrefab != null ? bossPrefab : LoadBossPrefab();
                if (bossPrefab != null)
                {
                    var bossObject = Instantiate(
                        bossPrefab,
                        bossPosition,
                        Quaternion.Euler(bossEulerAngles));
                    bossObject.name = "Monster_Boss_1_TestRuntime";
                    boss = bossObject.GetComponentInChildren<BossAttackController>();
                }
            }

            if (boss == null)
            {
                Debug.LogWarning("BossSettingTestController could not find or load Monster_Boss_1.", this);
                return;
            }

            boss.transform.SetPositionAndRotation(bossPosition, Quaternion.Euler(bossEulerAngles));
            boss.SetTarget(dummyTarget);
            boss.SetAutoAttack(false);
            boss.ResetCooldowns();

            initialBossPosition = boss.transform.position;
            initialBossRotation = boss.transform.rotation;
            initialDummyPosition = dummyTarget.position;
            initialDummyRotation = dummyTarget.rotation;
        }

        private GameObject LoadBossPrefab()
        {
#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<GameObject>(BossPrefabPath);
#else
            return null;
#endif
        }

        private Transform FindOrCreateDummyTarget()
        {
            var playerObject = FindObjectWithTagSafe("Player");
            if (playerObject != null)
            {
                return playerObject.transform;
            }

            var dummyObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            dummyObject.name = "BossSkillTest_DummyTarget";
            dummyObject.transform.position = dummyPosition;
            dummyObject.transform.rotation = Quaternion.identity;
            dummyObject.transform.localScale = new Vector3(1f, 1.8f, 1f);

            TrySetTag(dummyObject, "Player");

            if (dummyObject.GetComponent<Rigidbody>() == null)
            {
                var body = dummyObject.AddComponent<Rigidbody>();
                body.isKinematic = true;
                body.useGravity = false;
            }

            if (dummyObject.GetComponent<SeoulPlayDamageable>() == null)
            {
                dummyObject.AddComponent<SeoulPlayDamageable>();
            }

            return dummyObject.transform;
        }

        private static GameObject FindObjectWithTagSafe(string tagName)
        {
            try
            {
                return GameObject.FindGameObjectWithTag(tagName);
            }
            catch (UnityException)
            {
                return null;
            }
        }

        private static void TrySetTag(GameObject targetObject, string tagName)
        {
            try
            {
                targetObject.tag = tagName;
            }
            catch (UnityException)
            {
                Debug.LogWarning($"Tag '{tagName}' does not exist. Boss test target will still be assigned directly.");
            }
        }

        private static void HideLooseBossObjects()
        {
            var allObjects = FindObjectsOfType<GameObject>();
            foreach (var sceneObject in allObjects)
            {
                if (!sceneObject.activeInHierarchy ||
                    sceneObject.GetComponentInParent<BossAttackController>() != null)
                {
                    continue;
                }

                if (sceneObject.name == "Monster_Boss_1" ||
                    sceneObject.name == "Monster_Boss_1_V2" ||
                    sceneObject.name == "Monster_Boss_1_Guide")
                {
                    sceneObject.SetActive(false);
                }
            }
        }

        private void OnGUI()
        {
            if (SceneManager.GetActiveScene().name != TestSceneName)
            {
                return;
            }

            GUILayout.BeginArea(new Rect(16f, 16f, 260f, 280f), GUI.skin.box);
            GUILayout.Label("Boss Skill Test");
            GUILayout.Label(boss != null ? $"State: {boss.CurrentState}" : "State: Missing Boss");

            GUI.enabled = boss != null;
            if (GUILayout.Button("Skill 1 - Rock Throw", GUILayout.Height(34f)))
            {
                StartSkill(1);
            }

            if (GUILayout.Button("Skill 2 - Earth Blast", GUILayout.Height(34f)))
            {
                StartSkill(2);
            }

            if (GUILayout.Button("Skill 3 - Jump Slam", GUILayout.Height(34f)))
            {
                StartSkill(3);
            }

            GUILayout.Space(8f);
            if (GUILayout.Button("Reset Cooldowns"))
            {
                boss.ResetCooldowns();
            }

            if (GUILayout.Button("Stop Current Skill"))
            {
                boss.ForceFinishAttack();
            }

            if (GUILayout.Button("Reset Positions"))
            {
                ResetPositions();
            }

            GUI.enabled = true;
            GUILayout.EndArea();
        }

        private void StartSkill(int skillIndex)
        {
            boss.SetTarget(dummyTarget);
            boss.SetAutoAttack(false);
            if (restartSkillButtons)
            {
                boss.ForceFinishAttack();
            }

            boss.ResetCooldowns();

            switch (skillIndex)
            {
                case 1:
                    boss.StartAttack1();
                    break;
                case 2:
                    boss.StartAttack2();
                    break;
                case 3:
                    boss.StartAttack3();
                    break;
            }
        }

        private void ResetPositions()
        {
            boss.ForceFinishAttack();
            boss.transform.SetPositionAndRotation(initialBossPosition, initialBossRotation);

            if (dummyTarget != null)
            {
                dummyTarget.SetPositionAndRotation(initialDummyPosition, initialDummyRotation);
                var damageable = dummyTarget.GetComponent<SeoulPlayDamageable>();
                if (damageable != null)
                {
                    damageable.ResetHealth();
                }
            }

            boss.SetTarget(dummyTarget);
        }
    }
}
