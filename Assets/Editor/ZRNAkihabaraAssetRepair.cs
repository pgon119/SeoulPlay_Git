using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class ZRNAkihabaraAssetRepair
{
    private const string AssetRoot = "Assets/ZRNAssets/005339_08932_25_14";
    private const string SampleScene = AssetRoot + "/Scenes/Sample_005339_08932_25_14.unity";

    [MenuItem("Tools/ZRN Assets/Repair Akihabara Sample Scene")]
    public static void RepairAkihabaraSampleScene()
    {
        ConvertMaterialsToUrp();
        ResaveSampleScene();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Repaired Akihabara sample scene materials for URP and resaved the sample scene.");
    }

    private static void ConvertMaterialsToUrp()
    {
        var litShader = Shader.Find("Universal Render Pipeline/Lit");
        if (litShader == null)
        {
            Debug.LogError("Could not find Universal Render Pipeline/Lit shader. Make sure URP is installed.");
            return;
        }

        var materialGuids = AssetDatabase.FindAssets("t:Material", new[] { AssetRoot });
        foreach (var guid in materialGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                continue;
            }

            var mainTexture = GetFirstTexture(material, "_BaseMap", "_MainTex", "_MainTexture", "_Texture");
            var color = GetFirstColor(material, "_BaseColor", "_Color", "_TexColor");
            var shouldAlphaClip = IsCutoutMaterial(material, path);

            material.shader = litShader;

            if (mainTexture != null)
            {
                material.SetTexture("_BaseMap", mainTexture);
                material.SetTexture("_MainTex", mainTexture);
            }

            material.SetColor("_BaseColor", color);
            material.SetColor("_Color", color);
            material.SetFloat("_Smoothness", 0.2f);
            material.SetFloat("_Metallic", 0f);
            material.SetFloat("_Surface", 0f);
            material.SetFloat("_Cull", 2f);

            if (shouldAlphaClip)
            {
                material.EnableKeyword("_ALPHATEST_ON");
                material.SetFloat("_AlphaClip", 1f);
                material.SetFloat("_Cutoff", Mathf.Max(0.3f, material.HasProperty("_Cutoff") ? material.GetFloat("_Cutoff") : 0.5f));
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
                material.SetOverrideTag("RenderType", "TransparentCutout");
            }
            else
            {
                material.DisableKeyword("_ALPHATEST_ON");
                material.SetFloat("_AlphaClip", 0f);
                material.renderQueue = -1;
                material.SetOverrideTag("RenderType", "Opaque");
            }

            EditorUtility.SetDirty(material);
        }
    }

    private static Texture GetFirstTexture(Material material, params string[] names)
    {
        foreach (var name in names)
        {
            if (material.HasProperty(name))
            {
                var texture = material.GetTexture(name);
                if (texture != null)
                {
                    return texture;
                }
            }
        }

        return null;
    }

    private static Color GetFirstColor(Material material, params string[] names)
    {
        foreach (var name in names)
        {
            if (material.HasProperty(name))
            {
                return material.GetColor(name);
            }
        }

        return Color.white;
    }

    private static bool IsCutoutMaterial(Material material, string path)
    {
        var filename = Path.GetFileNameWithoutExtension(path);
        var shaderName = material.shader != null ? material.shader.name : string.Empty;

        return filename.Contains("TransP")
            || filename.IndexOf("trans", System.StringComparison.OrdinalIgnoreCase) >= 0
            || shaderName.Contains("Clip")
            || material.IsKeywordEnabled("_ALPHATEST_ON")
            || material.renderQueue == (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
    }

    private static void ResaveSampleScene()
    {
        if (!File.Exists(SampleScene))
        {
            Debug.LogWarning("Sample scene was not found: " + SampleScene);
            return;
        }

        var scene = EditorSceneManager.OpenScene(SampleScene, OpenSceneMode.Single);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }
}
