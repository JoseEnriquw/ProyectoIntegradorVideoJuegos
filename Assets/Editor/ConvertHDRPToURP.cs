using UnityEngine;
using UnityEditor;

public class ConvertHDRPToURP
{
    [MenuItem("Tools/Convert Materials to URP")]
    public static void ConvertAllMaterials()
    {
        string[] guids = AssetDatabase.FindAssets("t:Material");
        int totalFound = 0;
        int convertedCount = 0;

        Shader urpLitShader = Shader.Find("Universal Render Pipeline/Lit");
        Shader urpDecalShader = Shader.Find("Universal Render Pipeline/Decal");
        Shader urpUnlitShader = Shader.Find("Universal Render Pipeline/Unlit");

        if (urpLitShader == null)
        {
            Debug.LogError("URP Lit Shader not found! Make sure URP is installed and configured in your project.");
            return;
        }

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (mat == null)
                continue;

            string shaderName = mat.shader != null ? mat.shader.name : "Missing Shader";
            
            // Determine if the shader is compatible with URP
            bool isCompatible = shaderName.StartsWith("Universal Render Pipeline/")
                                || shaderName.StartsWith("Shader Graphs/")
                                || shaderName.StartsWith("UI/")
                                || shaderName.StartsWith("TextMeshPro/")
                                || (shaderName.StartsWith("Hidden/") && shaderName != "Hidden/InternalErrorShader")
                                || shaderName.StartsWith("Sprite")
                                || shaderName.StartsWith("Skybox/");

            bool isMissingOrError = mat.shader == null || shaderName == "Hidden/InternalErrorShader";
            bool shouldConvert = !isCompatible || isMissingOrError;

            if (shouldConvert)
            {
                totalFound++;
                Debug.Log($"Converting material: {path} (Original Shader: {shaderName})");

                // Cache all possible properties (Standard, HDRP, and common names)
                Texture baseColorMap = null;
                Color baseColor = Color.white;
                Texture normalMap = null;
                float normalScale = 1.0f;
                Texture metallicMap = null;
                float metallic = 0.0f;
                float smoothness = 0.5f;
                Texture emissionMap = null;
                Color emissionColor = Color.black;
                Texture occlusionMap = null;
                float occlusionStrength = 1.0f;

                // 1. Base Map / Albedo
                if (mat.HasProperty("_BaseColorMap") && mat.GetTexture("_BaseColorMap") != null)
                    baseColorMap = mat.GetTexture("_BaseColorMap");
                else if (mat.HasProperty("_MainTex") && mat.GetTexture("_MainTex") != null)
                    baseColorMap = mat.GetTexture("_MainTex");
                else if (mat.HasProperty("_UnlitColorMap") && mat.GetTexture("_UnlitColorMap") != null)
                    baseColorMap = mat.GetTexture("_UnlitColorMap");

                if (mat.HasProperty("_BaseColor"))
                    baseColor = mat.GetColor("_BaseColor");
                else if (mat.HasProperty("_Color"))
                    baseColor = mat.GetColor("_Color");
                else if (mat.HasProperty("_UnlitColor"))
                    baseColor = mat.GetColor("_UnlitColor");

                // 2. Normal Map
                if (mat.HasProperty("_NormalMap") && mat.GetTexture("_NormalMap") != null)
                    normalMap = mat.GetTexture("_NormalMap");
                else if (mat.HasProperty("_BumpMap") && mat.GetTexture("_BumpMap") != null)
                    normalMap = mat.GetTexture("_BumpMap");

                if (mat.HasProperty("_NormalScale"))
                    normalScale = mat.GetFloat("_NormalScale");
                else if (mat.HasProperty("_BumpScale"))
                    normalScale = mat.GetFloat("_BumpScale");

                // 3. Metallic / Smoothness
                if (mat.HasProperty("_MaskMap") && mat.GetTexture("_MaskMap") != null)
                    metallicMap = mat.GetTexture("_MaskMap");
                else if (mat.HasProperty("_MetallicGlossMap") && mat.GetTexture("_MetallicGlossMap") != null)
                    metallicMap = mat.GetTexture("_MetallicGlossMap");

                if (mat.HasProperty("_Metallic"))
                    metallic = mat.GetFloat("_Metallic");

                if (mat.HasProperty("_Smoothness"))
                    smoothness = mat.GetFloat("_Smoothness");
                else if (mat.HasProperty("_Glossiness"))
                    smoothness = mat.GetFloat("_Glossiness");

                // 4. Occlusion
                if (mat.HasProperty("_OcclusionMap") && mat.GetTexture("_OcclusionMap") != null)
                    occlusionMap = mat.GetTexture("_OcclusionMap");
                
                if (mat.HasProperty("_OcclusionStrength"))
                    occlusionStrength = mat.GetFloat("_OcclusionStrength");

                // 5. Emission
                if (mat.HasProperty("_EmissionMap") && mat.GetTexture("_EmissionMap") != null)
                    emissionMap = mat.GetTexture("_EmissionMap");
                else if (mat.HasProperty("_EmissiveColorMap") && mat.GetTexture("_EmissiveColorMap") != null)
                    emissionMap = mat.GetTexture("_EmissiveColorMap");

                if (mat.HasProperty("_EmissionColor"))
                    emissionColor = mat.GetColor("_EmissionColor");
                else if (mat.HasProperty("_EmissiveColor"))
                    emissionColor = mat.GetColor("_EmissiveColor");

                // Determine target URP shader
                Shader targetShader = urpLitShader;
                bool isDecal = shaderName.Contains("Decal") || mat.HasProperty("_DecalColorMap") || mat.HasProperty("_DecalColor");
                bool isUnlit = shaderName.Contains("Unlit") && urpUnlitShader != null;

                if (isDecal && urpDecalShader != null)
                {
                    targetShader = urpDecalShader;
                    mat.shader = targetShader;

                    if (baseColorMap != null) mat.SetTexture("_DecalColorMap", baseColorMap);
                    mat.SetColor("_DecalColor", baseColor);
                    if (normalMap != null) mat.SetTexture("_NormalMap", normalMap);
                    if (metallicMap != null) mat.SetTexture("_MaskMap", metallicMap);
                }
                else if (isUnlit)
                {
                    targetShader = urpUnlitShader;
                    mat.shader = targetShader;

                    if (baseColorMap != null) mat.SetTexture("_BaseMap", baseColorMap);
                    mat.SetColor("_BaseColor", baseColor);
                }
                else
                {
                    mat.shader = urpLitShader;

                    // Remap properties
                    if (baseColorMap != null) mat.SetTexture("_BaseMap", baseColorMap);
                    mat.SetColor("_BaseColor", baseColor);

                    if (normalMap != null)
                    {
                        mat.SetTexture("_BumpMap", normalMap);
                        mat.SetFloat("_BumpScale", normalScale);
                        mat.EnableKeyword("_NORMALMAP");
                    }

                    if (metallicMap != null)
                    {
                        mat.SetTexture("_MetallicGlossMap", metallicMap);
                        mat.EnableKeyword("_METALLICSPECGLOSSMAP");
                    }
                    mat.SetFloat("_Metallic", metallic);
                    mat.SetFloat("_Smoothness", smoothness);

                    if (occlusionMap != null)
                    {
                        mat.SetTexture("_OcclusionMap", occlusionMap);
                        mat.SetFloat("_OcclusionStrength", occlusionStrength);
                    }

                    if (emissionMap != null || emissionColor != Color.black)
                    {
                        mat.SetTexture("_EmissionMap", emissionMap);
                        mat.SetColor("_EmissionColor", emissionColor);
                        mat.EnableKeyword("_EMISSION");
                    }
                }

                EditorUtility.SetDirty(mat);
                convertedCount++;
            }
        }

        if (convertedCount > 0)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Conversion Complete", $"Successfully converted {convertedCount} materials to URP.", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Conversion Complete", "No incompatible materials or missing shaders were found to convert.", "OK");
        }
    }

    [MenuItem("Tools/Revert Materials Not In Active Scene")]
    public static void RevertMaterialsNotInActiveScene()
    {
        // Get active scene
        UnityEngine.SceneManagement.Scene activeScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
        if (string.IsNullOrEmpty(activeScene.path))
        {
            EditorUtility.DisplayDialog("Error", "No active scene found. Please open a scene first.", "OK");
            return;
        }

        // Get dependencies of the active scene
        string[] dependencies = AssetDatabase.GetDependencies(activeScene.path, true);
        System.Collections.Generic.HashSet<string> sceneMaterials = new System.Collections.Generic.HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
        foreach (string dep in dependencies)
        {
            if (dep.EndsWith(".mat", System.StringComparison.OrdinalIgnoreCase))
            {
                sceneMaterials.Add(dep.Replace("\\", "/"));
            }
        }

        // Get modified files from git
        System.Diagnostics.Process process = new System.Diagnostics.Process();
        process.StartInfo.FileName = "git";
        process.StartInfo.Arguments = "status --porcelain";
        process.StartInfo.WorkingDirectory = Application.dataPath + "/..";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.CreateNoWindow = true;
        process.Start();

        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        string[] lines = output.Split('\n');
        System.Collections.Generic.List<string> materialsToRevert = new System.Collections.Generic.List<string>();

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            // git status --porcelain output lines start with 2 characters status, then a space, then the path.
            if (line.Length < 4) continue;
            string file = line.Substring(3).Trim().Trim('"');

            if (file.EndsWith(".mat", System.StringComparison.OrdinalIgnoreCase))
            {
                string standardPath = file.Replace("\\", "/");
                if (!sceneMaterials.Contains(standardPath))
                {
                    materialsToRevert.Add(standardPath);
                }
            }
        }

        if (materialsToRevert.Count == 0)
        {
            EditorUtility.DisplayDialog("Revert Materials", "No modified materials found outside the active scene.", "OK");
            return;
        }

        string msg = $"Found {materialsToRevert.Count} modified materials that are not used in the active scene: '{activeScene.name}'.\n\nDo you want to revert them using git?";
        if (EditorUtility.DisplayDialog("Revert Materials?", msg, "Yes, Revert Them", "Cancel"))
        {
            int revertedCount = 0;
            foreach (string matPath in materialsToRevert)
            {
                System.Diagnostics.Process revertProc = new System.Diagnostics.Process();
                revertProc.StartInfo.FileName = "git";
                revertProc.StartInfo.Arguments = $"checkout HEAD -- \"{matPath}\"";
                revertProc.StartInfo.WorkingDirectory = Application.dataPath + "/..";
                revertProc.StartInfo.UseShellExecute = false;
                revertProc.StartInfo.CreateNoWindow = true;
                revertProc.Start();
                revertProc.WaitForExit();
                if (revertProc.ExitCode == 0)
                {
                    revertedCount++;
                }
                else
                {
                    Debug.LogError($"Failed to revert: {matPath}");
                }
            }

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Success", $"Successfully reverted {revertedCount} materials.", "OK");
        }
    }
}

