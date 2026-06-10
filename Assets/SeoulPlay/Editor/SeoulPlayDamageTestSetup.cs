using SeoulPlay;
using UnityEditor;
using UnityEngine;

public static class SeoulPlayDamageTestSetup
{
    [MenuItem("SeoulPlay/Test/Setup Cube Sphere Damage Test")]
    private static void SetupCubeSphereDamageTest()
    {
        var cube = GameObject.Find("Cube");
        if (cube == null)
        {
            Debug.LogWarning("Could not find a GameObject named Cube in the open scene.");
            return;
        }

        var shooter = cube.GetComponent<SeoulPlaySphereDamageShooter>();
        if (shooter == null)
        {
            shooter = Undo.AddComponent<SeoulPlaySphereDamageShooter>(cube);
        }

        var damageable = FindDamageableTarget(cube);
        if (damageable != null)
        {
            Undo.RecordObject(shooter, "Configure Sphere Damage Shooter");
            shooter.SetTarget(damageable.transform);
            EditorUtility.SetDirty(shooter);

            var serializedDamageable = new SerializedObject(damageable);
            serializedDamageable.FindProperty("playHitReaction").boolValue = true;
            serializedDamageable.FindProperty("hitTrigger").stringValue = "Hit";
            serializedDamageable.FindProperty("blockFireOnHit").boolValue = true;
            serializedDamageable.FindProperty("hitFireLockoutDuration").floatValue = 0.35f;
            serializedDamageable.ApplyModifiedProperties();
        }

        Selection.activeGameObject = cube;
        EditorUtility.SetDirty(cube);
        Debug.Log("Cube sphere damage test is ready. Enter Play Mode to fire a damage sphere every second.", cube);
    }

    private static SeoulPlayDamageable FindDamageableTarget(GameObject shooterObject)
    {
        var heroMover = Object.FindObjectOfType<SimpleHeroMover>();
        if (heroMover != null)
        {
            var heroDamageable = heroMover.GetComponentInChildren<SeoulPlayDamageable>();
            if (heroDamageable != null)
            {
                return heroDamageable;
            }

            heroDamageable = heroMover.GetComponentInParent<SeoulPlayDamageable>();
            if (heroDamageable != null)
            {
                return heroDamageable;
            }
        }

        foreach (var damageable in Object.FindObjectsOfType<SeoulPlayDamageable>())
        {
            if (damageable != null && damageable.gameObject != shooterObject)
            {
                return damageable;
            }
        }

        return null;
    }
}
