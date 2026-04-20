using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace SeoulPlay.Editor
{
    public static class UrpMaterialConverter
    {
        private const string InvectorRoot = "Assets/Invector-3rdPersonController";
        private const string HighFidelityPipelinePath = "Assets/Settings/URP-HighFidelity.asset";

        [MenuItem("SeoulPlay/URP/Fix Invector Materials")]
        public static void FixInvectorMaterials()
        {
            EnsureUrpPipeline();

            var urpLit = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLit == null)
            {
                Debug.LogError("URP Lit shader was not found. Check that Universal RP is installed.");
                return;
            }

            var converted = 0;
            var skipped = 0;
            var materialGuids = AssetDatabase.FindAssets("t:Material", new[] { InvectorRoot });

            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (var guid in materialGuids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                    if (material == null)
                    {
                        skipped++;
                        continue;
                    }

                    if (!ShouldConvert(material, path))
                    {
                        skipped++;
                        continue;
                    }

                    ConvertToUrpLit(material, urpLit);
                    EditorUtility.SetDirty(material);
                    converted++;
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"URP material conversion finished. Converted: {converted}, skipped: {skipped}");
        }

        private static void EnsureUrpPipeline()
        {
            var pipeline = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(HighFidelityPipelinePath);
            if (pipeline == null)
            {
                Debug.LogWarning($"URP pipeline asset was not found: {HighFidelityPipelinePath}");
                return;
            }

            if (GraphicsSettings.defaultRenderPipeline != pipeline)
            {
                GraphicsSettings.defaultRenderPipeline = pipeline;
            }

            if (QualitySettings.renderPipeline != pipeline)
            {
                QualitySettings.renderPipeline = pipeline;
            }
        }

        private static bool ShouldConvert(Material material, string path)
        {
            if (path.ToLowerInvariant().Contains("skybox"))
            {
                return false;
            }

            if (material.shader == null)
            {
                return true;
            }

            var shaderName = material.shader.name;
            return shaderName == "Standard"
                || shaderName == "Hidden/InternalErrorShader"
                || shaderName.StartsWith("Legacy Shaders/")
                || shaderName.StartsWith("Mobile/")
                || shaderName.StartsWith("Particles/");
        }

        private static void ConvertToUrpLit(Material material, Shader urpLit)
        {
            var color = GetColor(material, "_Color", Color.white);
            var mainTexture = GetTexture(material, "_MainTex");
            var mainScale = GetTextureScale(material, "_MainTex", Vector2.one);
            var mainOffset = GetTextureOffset(material, "_MainTex", Vector2.zero);
            var normalMap = GetTexture(material, "_BumpMap");
            var normalScale = GetFloat(material, "_BumpScale", 1f);
            var metallicMap = GetTexture(material, "_MetallicGlossMap");
            var metallic = GetFloat(material, "_Metallic", 0f);
            var smoothness = GetFloat(material, "_Glossiness", 0.5f);
            var cutoff = GetFloat(material, "_Cutoff", 0.5f);
            var emissionMap = GetTexture(material, "_EmissionMap");
            var emissionColor = GetColor(material, "_EmissionColor", Color.black);
            var wasTransparent = IsTransparent(material);

            material.shader = urpLit;

            SetColor(material, "_BaseColor", color);
            SetTexture(material, "_BaseMap", mainTexture);
            SetTextureScale(material, "_BaseMap", mainScale);
            SetTextureOffset(material, "_BaseMap", mainOffset);
            SetTexture(material, "_BumpMap", normalMap);
            SetFloat(material, "_BumpScale", normalScale);
            SetTexture(material, "_MetallicGlossMap", metallicMap);
            SetFloat(material, "_Metallic", metallic);
            SetFloat(material, "_Smoothness", smoothness);
            SetFloat(material, "_Cutoff", cutoff);
            SetTexture(material, "_EmissionMap", emissionMap);
            SetColor(material, "_EmissionColor", emissionColor);

            ConfigureKeywords(material, normalMap, metallicMap, emissionMap, emissionColor, wasTransparent);
        }

        private static bool IsTransparent(Material material)
        {
            if (material.renderQueue >= 3000)
            {
                return true;
            }

            return material.HasProperty("_Mode") && material.GetFloat("_Mode") > 0f;
        }

        private static void ConfigureKeywords(
            Material material,
            Texture normalMap,
            Texture metallicMap,
            Texture emissionMap,
            Color emissionColor,
            bool transparent)
        {
            SetFloat(material, "_Surface", transparent ? 1f : 0f);
            SetFloat(material, "_Blend", 0f);
            SetFloat(material, "_AlphaClip", 0f);
            SetFloat(material, "_SrcBlend", transparent ? (float)UnityEngine.Rendering.BlendMode.SrcAlpha : 1f);
            SetFloat(material, "_DstBlend", transparent ? (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha : 0f);
            SetFloat(material, "_ZWrite", transparent ? 0f : 1f);
            material.renderQueue = transparent ? (int)UnityEngine.Rendering.RenderQueue.Transparent : -1;

            SetKeyword(material, "_NORMALMAP", normalMap != null);
            SetKeyword(material, "_METALLICSPECGLOSSMAP", metallicMap != null);
            SetKeyword(material, "_EMISSION", emissionMap != null || emissionColor.maxColorComponent > 0.001f);
            SetKeyword(material, "_SURFACE_TYPE_TRANSPARENT", transparent);
        }

        private static Color GetColor(Material material, string propertyName, Color fallback)
        {
            return material.HasProperty(propertyName) ? material.GetColor(propertyName) : fallback;
        }

        private static float GetFloat(Material material, string propertyName, float fallback)
        {
            return material.HasProperty(propertyName) ? material.GetFloat(propertyName) : fallback;
        }

        private static Texture GetTexture(Material material, string propertyName)
        {
            return material.HasProperty(propertyName) ? material.GetTexture(propertyName) : null;
        }

        private static Vector2 GetTextureScale(Material material, string propertyName, Vector2 fallback)
        {
            return material.HasProperty(propertyName) ? material.GetTextureScale(propertyName) : fallback;
        }

        private static Vector2 GetTextureOffset(Material material, string propertyName, Vector2 fallback)
        {
            return material.HasProperty(propertyName) ? material.GetTextureOffset(propertyName) : fallback;
        }

        private static void SetColor(Material material, string propertyName, Color value)
        {
            if (material.HasProperty(propertyName))
            {
                material.SetColor(propertyName, value);
            }
        }

        private static void SetFloat(Material material, string propertyName, float value)
        {
            if (material.HasProperty(propertyName))
            {
                material.SetFloat(propertyName, value);
            }
        }

        private static void SetTexture(Material material, string propertyName, Texture value)
        {
            if (material.HasProperty(propertyName))
            {
                material.SetTexture(propertyName, value);
            }
        }

        private static void SetTextureScale(Material material, string propertyName, Vector2 value)
        {
            if (material.HasProperty(propertyName))
            {
                material.SetTextureScale(propertyName, value);
            }
        }

        private static void SetTextureOffset(Material material, string propertyName, Vector2 value)
        {
            if (material.HasProperty(propertyName))
            {
                material.SetTextureOffset(propertyName, value);
            }
        }

        private static void SetKeyword(Material material, string keyword, bool enabled)
        {
            if (enabled)
            {
                material.EnableKeyword(keyword);
            }
            else
            {
                material.DisableKeyword(keyword);
            }
        }
    }
}
