using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UHFPS.Rendering;

public class BuildMainMenuBackground
{
    [MenuItem("Antigravity/Build Main Menu Background")]
    public static void Build()
    {
        string rootName = "= BACKGROUND";
        GameObject oldRoot = GameObject.Find(rootName);
        if (oldRoot != null)
        {
            Undo.DestroyObjectImmediate(oldRoot);
        }

        GameObject root = new GameObject(rootName);
        Undo.RegisterCreatedObjectUndo(root, "Create Main Menu Background");

        // 1. Terrain Creation
        string terrainDataPath = "Assets/MainMenuTerrainData.asset";
        TerrainData terrainData = AssetDatabase.LoadAssetAtPath<TerrainData>(terrainDataPath);
        if (terrainData == null)
        {
            terrainData = new TerrainData();
            terrainData.heightmapResolution = 513;
            terrainData.size = new Vector3(300, 30, 300);
            AssetDatabase.CreateAsset(terrainData, terrainDataPath);
        }
        else
        {
            terrainData.size = new Vector3(300, 30, 300);
        }

        // Configure Terrain Layers (Duplicated and set to high smoothness for wet reflection)
        TerrainLayer grassLayer = AssetDatabase.LoadAssetAtPath<TerrainLayer>("Assets/MainMenuForest.terrainlayer");
        if (grassLayer == null)
        {
            TerrainLayer origGrass = AssetDatabase.LoadAssetAtPath<TerrainLayer>("Assets/Scenes/Playground/Layers/Forest.terrainlayer");
            if (origGrass != null)
            {
                grassLayer = new TerrainLayer();
                EditorUtility.CopySerialized(origGrass, grassLayer);
                grassLayer.smoothness = 0.45f;
                AssetDatabase.CreateAsset(grassLayer, "Assets/MainMenuForest.terrainlayer");
            }
        }
        else
        {
            grassLayer.smoothness = 0.45f;
            EditorUtility.SetDirty(grassLayer);
        }

        TerrainLayer roadLayer = AssetDatabase.LoadAssetAtPath<TerrainLayer>("Assets/MainMenuGravel.terrainlayer");
        if (roadLayer == null)
        {
            TerrainLayer origRoad = AssetDatabase.LoadAssetAtPath<TerrainLayer>("Assets/Scenes/Playground/Layers/Gravel.terrainlayer");
            if (origRoad != null)
            {
                roadLayer = new TerrainLayer();
                EditorUtility.CopySerialized(origRoad, roadLayer);
                roadLayer.smoothness = 0.9f; // Highly reflective when wet!
                AssetDatabase.CreateAsset(roadLayer, "Assets/MainMenuGravel.terrainlayer");
            }
        }
        else
        {
            roadLayer.smoothness = 0.9f;
            EditorUtility.SetDirty(roadLayer);
        }

        if (grassLayer != null && roadLayer != null)
        {
            terrainData.terrainLayers = new TerrainLayer[] { grassLayer, roadLayer };
        }

        // Paint Road down the middle (X = 0 in world, which is middle of terrain width)
        int mapWidth = terrainData.alphamapWidth;
        int mapHeight = terrainData.alphamapHeight;
        float[,,] alphamaps = new float[mapWidth, mapHeight, 2];
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float normX = (float)x / (mapWidth - 1);
                float distanceToCenter = Mathf.Abs(normX - 0.5f);
                if (distanceToCenter < 0.025f)
                {
                    alphamaps[x, y, 0] = 0.0f; // Grass
                    alphamaps[x, y, 1] = 1.0f; // Road
                }
                else if (distanceToCenter < 0.045f)
                {
                    float t = (distanceToCenter - 0.025f) / 0.020f;
                    alphaps(x, y, alphamaps, t);
                }
                else
                {
                    alphamaps[x, y, 0] = 1.0f;
                    alphamaps[x, y, 1] = 0.0f;
                }
            }
        }
        terrainData.SetAlphamaps(0, 0, alphamaps);

        GameObject terrainGo = Terrain.CreateTerrainGameObject(terrainData);
        terrainGo.name = "Terrain";
        terrainGo.transform.parent = root.transform;
        // Center the terrain X=0 at world coordinate X=0
        terrainGo.transform.localPosition = new Vector3(-150, -5, -20);
        Terrain terrainComp = terrainGo.GetComponent<Terrain>();

        // 2. Matadero Building
        string mataderoPath = "Assets/Models/Matadero/MataderoViejo.fbx";
        GameObject mataderoPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(mataderoPath);
        if (mataderoPrefab != null)
        {
            GameObject matadero = (GameObject)PrefabUtility.InstantiatePrefab(mataderoPrefab);
            matadero.name = "MataderoViejo";
            matadero.transform.parent = root.transform;
            float mataderoZ = 42.0f;
            float mataderoY = GetTerrainHeight(0.6f, mataderoZ, terrainComp) + 17.2f;
            matadero.transform.localPosition = new Vector3(0.6f, mataderoY, mataderoZ);
            matadero.transform.localEulerAngles = new Vector3(270f, 180f, 0f);
            matadero.transform.localScale = new Vector3(1900f, 1900f, 1900f);
        }
        else
        {
            Debug.LogError("Could not find MataderoViejo prefab at: " + mataderoPath);
        }

        // 3. Wooden Sign "Welcome Epecuen" (with correct scale and rotation!)
        string signPath = "Assets/AssetsDescargados/Bosque/Cartel Epecuen/Welcome Epecuen.fbx";
        GameObject signPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(signPath);
        if (signPrefab != null)
        {
            GameObject sign = (GameObject)PrefabUtility.InstantiatePrefab(signPrefab);
            sign.name = "Cartel_WelcomeEpecuen";
            sign.transform.parent = root.transform;
            
            sign.transform.localPosition = new Vector3(-4.03f, -4.16f, -1.02f);
            sign.transform.localEulerAngles = new Vector3(273.8f, 44.7f, 95.6f);
            sign.transform.localScale = new Vector3(210f, 210f, 210f);
        }
        else
        {
            Debug.LogError("Could not find Cartel Epecuen prefab at: " + signPath);
        }

        // 4. Fences
        string fencePath = "Assets/AssetsDescargados/Pueblo/Flooded_Grounds/Prefabs/Buildings/Structures1/Struct_Fence2_Mid_A.prefab";
        GameObject fencePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(fencePath);
        if (fencePrefab != null)
        {
            float zStart = -15f;
            float zEnd = 28f;
            float step = 4.3f;
            float leftX = -4.0f;
            float rightX = 4.0f;

            for (float zVal = zStart; zVal <= zEnd; zVal += step)
            {
                // Skip left fence placement where the sign is (around Z = -1)
                if (zVal >= -3f && zVal <= 2f)
                {
                    continue;
                }

                // Left Fence
                GameObject leftFence = (GameObject)PrefabUtility.InstantiatePrefab(fencePrefab);
                leftFence.transform.parent = root.transform;
                float yRotL = 0f + Random.Range(-12f, 12f);
                float xRotL = 0f + Random.Range(-8f, 8f);
                float zRotL = 0f + Random.Range(-8f, 8f);
                float worldYL = GetTerrainHeight(leftX, zVal, terrainComp) - 0.15f;
                leftFence.transform.localPosition = new Vector3(leftX + Random.Range(-0.4f, 0.4f), worldYL + Random.Range(-0.1f, 0.05f), zVal + Random.Range(-0.4f, 0.4f));
                leftFence.transform.localEulerAngles = new Vector3(xRotL, yRotL, zRotL);

                // Right Fence
                GameObject rightFence = (GameObject)PrefabUtility.InstantiatePrefab(fencePrefab);
                rightFence.transform.parent = root.transform;
                float yRotR = 180f + Random.Range(-12f, 12f);
                float xRotR = 0f + Random.Range(-8f, 8f);
                float zRotR = 0f + Random.Range(-8f, 8f);
                float worldYR = GetTerrainHeight(rightX, zVal, terrainComp) - 0.15f;
                rightFence.transform.localPosition = new Vector3(rightX + Random.Range(-0.4f, 0.4f), worldYR + Random.Range(-0.1f, 0.05f), zVal + Random.Range(-0.4f, 0.4f));
                rightFence.transform.localEulerAngles = new Vector3(xRotR, yRotR, zRotR);
            }
        }
        else
        {
            Debug.LogError("Could not find Fence prefab at: " + fencePath);
        }

        // 5. Conifer Trees
        string[] treePaths = new string[] {
            "Assets/Forst/Conifers [BOTD]/Render Pipeline Support/URP/Prefabs/PF Conifer Bare BOTD URP.prefab",
            "Assets/Forst/Conifers [BOTD]/Render Pipeline Support/URP/Prefabs/PF Conifer Tall BOTD URP.prefab",
            "Assets/Forst/Conifers [BOTD]/Render Pipeline Support/URP/Prefabs/PF Conifer Medium BOTD URP.prefab"
        };
        GameObject[] treePrefabs = new GameObject[treePaths.Length];
        for (int i = 0; i < treePaths.Length; i++)
        {
            treePrefabs[i] = AssetDatabase.LoadAssetAtPath<GameObject>(treePaths[i]);
        }

        if (treePrefabs[0] != null && treePrefabs[1] != null && treePrefabs[2] != null)
        {
            Random.InitState(42);

            // Left Forest
            for (int i = 0; i < 45; i++)
            {
                float z = Random.Range(-20f, 95f);
                float x = Random.Range(-25f, -8.0f);
                GameObject treePrefab = treePrefabs[Random.Range(0, treePrefabs.Length)];
                GameObject tree = (GameObject)PrefabUtility.InstantiatePrefab(treePrefab);
                tree.transform.parent = root.transform;
                float worldY = GetTerrainHeight(x, z, terrainComp) - 0.2f;
                tree.transform.localPosition = new Vector3(x, worldY, z);
                tree.transform.localEulerAngles = new Vector3(Random.Range(-2f, 2f), Random.Range(0f, 360f), Random.Range(-2f, 2f));
                float scale = Random.Range(0.80f, 1.30f);
                tree.transform.localScale = new Vector3(scale, scale, scale);
            }

            // Right Forest
            for (int i = 0; i < 45; i++)
            {
                float z = Random.Range(-20f, 95f);
                float x = Random.Range(8.0f, 25f);
                GameObject treePrefab = treePrefabs[Random.Range(0, treePrefabs.Length)];
                GameObject tree = (GameObject)PrefabUtility.InstantiatePrefab(treePrefab);
                tree.transform.parent = root.transform;
                float worldY = GetTerrainHeight(x, z, terrainComp) - 0.2f;
                tree.transform.localPosition = new Vector3(x, worldY, z);
                tree.transform.localEulerAngles = new Vector3(Random.Range(-2f, 2f), Random.Range(0f, 360f), Random.Range(-2f, 2f));
                float scale = Random.Range(0.80f, 1.30f);
                tree.transform.localScale = new Vector3(scale, scale, scale);
            }

            // Cinematic Framing Trees (Repoussoir) close to the camera
            // 1. Dead Bare Tree on the Left foreground
            GameObject leftDeadTree = (GameObject)PrefabUtility.InstantiatePrefab(treePrefabs[0]);
            leftDeadTree.transform.parent = root.transform;
            float deadTreeYL = GetTerrainHeight(-4.2f, -9.5f, terrainComp) - 0.2f;
            leftDeadTree.transform.localPosition = new Vector3(-4.2f, deadTreeYL, -9.5f);
            leftDeadTree.transform.localEulerAngles = new Vector3(3f, 15f, -2f);
            leftDeadTree.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);

            // 2. Medium Tree on the Right foreground to frame the right side
            GameObject rightMediumTree = (GameObject)PrefabUtility.InstantiatePrefab(treePrefabs[2]);
            rightMediumTree.transform.parent = root.transform;
            float medTreeYR = GetTerrainHeight(4.5f, -8.0f, terrainComp) - 0.2f;
            rightMediumTree.transform.localPosition = new Vector3(4.5f, medTreeYR, -8.0f);
            rightMediumTree.transform.localEulerAngles = new Vector3(-2f, 45f, 1f);
            rightMediumTree.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);

            // 3. Tall Tree behind the Swing to cast branches over it
            GameObject swingBackTree = (GameObject)PrefabUtility.InstantiatePrefab(treePrefabs[1]);
            swingBackTree.transform.parent = root.transform;
            float tallTreeY = GetTerrainHeight(4.8f, -5.0f, terrainComp) - 0.2f;
            swingBackTree.transform.localPosition = new Vector3(4.8f, tallTreeY, -5.0f);
            swingBackTree.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
            swingBackTree.transform.localScale = new Vector3(1.4f, 1.4f, 1.4f);
        }
        else
        {
            Debug.LogError("Could not find Conifer tree prefabs.");
        }

        // 6. Moon Light
        GameObject moonLightGo = new GameObject("Moon Light");
        moonLightGo.transform.parent = root.transform;
        moonLightGo.transform.localEulerAngles = new Vector3(22f, 185f, 0f);
        Light moonLight = moonLightGo.AddComponent<Light>();
        moonLight.type = LightType.Directional;
        moonLight.color = new Color(0.08f, 0.12f, 0.20f); // Darker cool blue
        moonLight.intensity = 0.12f; // Much darker moon lighting for horror atmosphere
        moonLight.shadows = LightShadows.Soft;

        var lightData = moonLightGo.GetComponent<UniversalAdditionalLightData>();
        if (lightData == null)
        {
            lightData = moonLightGo.AddComponent<UniversalAdditionalLightData>();
        }

        // 7. Warm Window Glow
        GameObject warmGlow = new GameObject("Building Glow");
        warmGlow.transform.parent = root.transform;
        float buildGlowY = GetTerrainHeight(0f, 78f, terrainComp) + 3.0f;
        warmGlow.transform.localPosition = new Vector3(0f, buildGlowY, 78f);
        Light glowLight = warmGlow.AddComponent<Light>();
        glowLight.type = LightType.Point;
        glowLight.color = new Color(0.98f, 0.65f, 0.15f);
        glowLight.intensity = 3.5f;
        glowLight.range = 25f;
        glowLight.shadows = LightShadows.None;

        // Add subtle flicker to building glow
        var distFlicker = warmGlow.AddComponent<MainMenuLightFlicker>();
        distFlicker.flickerSpeed = 0.35f;
        distFlicker.minIntensityMultiplier = 0.85f;
        distFlicker.maxIntensityMultiplier = 1.15f;

        // 8. Player's Lantern Light
        GameObject lanternLightGo = new GameObject("Player Lantern Light");
        lanternLightGo.transform.parent = root.transform;
        float lanternY = GetTerrainHeight(1.2f, -10.0f, terrainComp) + 1.2f;
        lanternLightGo.transform.localPosition = new Vector3(1.2f, lanternY, -10.0f);
        Light lanternLight = lanternLightGo.AddComponent<Light>();
        lanternLight.type = LightType.Point;
        lanternLight.color = new Color(1.0f, 0.62f, 0.18f);
        lanternLight.intensity = 5.5f;
        lanternLight.range = 25f;
        lanternLight.shadows = LightShadows.Soft;

        var lanternLightData = lanternLightGo.GetComponent<UniversalAdditionalLightData>();
        if (lanternLightData == null)
        {
            lanternLightData = lanternLightGo.AddComponent<UniversalAdditionalLightData>();
        }

        // Add spooky flicker to the player lantern light
        var lanternFlicker = lanternLightGo.AddComponent<MainMenuLightFlicker>();
        lanternFlicker.flickerSpeed = 0.14f;
        lanternFlicker.minIntensityMultiplier = 0.70f;
        lanternFlicker.maxIntensityMultiplier = 1.30f;

        // 9. NEW: Global Post-Processing Volume for Cinematic Horror Look
        GameObject volumeGo = new GameObject("PostProcessing Volume");
        volumeGo.transform.parent = root.transform;
        Volume volumeComp = volumeGo.AddComponent<Volume>();
        volumeComp.isGlobal = true;
        volumeComp.weight = 1.0f;
        
        string volumeProfilePath = "Assets/Scenes/MainMenuProfile.asset";
        VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(volumeProfilePath);
        bool isNewProfile = false;
        if (profile == null)
        {
            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            isNewProfile = true;
        }

        // Configure Vignette
        Vignette vignette;
        if (!profile.TryGet(out vignette)) vignette = profile.Add<Vignette>(true);
        vignette.active = true;
        vignette.intensity.overrideState = true;
        vignette.intensity.value = 0.55f; // Stronger vignettes
        vignette.smoothness.overrideState = true;
        vignette.smoothness.value = 0.6f;
        vignette.color.overrideState = true;
        vignette.color.value = Color.black;

        // Configure Chromatic Aberration
        ChromaticAberration ca;
        if (!profile.TryGet(out ca)) ca = profile.Add<ChromaticAberration>(true);
        ca.active = true;
        ca.intensity.overrideState = true;
        ca.intensity.value = 0.15f;

        // Configure Lens Distortion
        LensDistortion ld;
        if (!profile.TryGet(out ld)) ld = profile.Add<LensDistortion>(true);
        ld.active = true;
        ld.intensity.overrideState = true;
        ld.intensity.value = -0.05f;

        // Configure Film Grain
        FilmGrain filmGrain;
        if (!profile.TryGet(out filmGrain)) filmGrain = profile.Add<FilmGrain>(true);
        filmGrain.active = true;
        filmGrain.type.overrideState = true;
        filmGrain.type.value = FilmGrainLookup.Medium3;
        filmGrain.intensity.overrideState = true;
        filmGrain.intensity.value = 0.24f; // Grittier film grain

        // Configure Bloom
        Bloom bloom;
        if (!profile.TryGet(out bloom)) bloom = profile.Add<Bloom>(true);
        bloom.active = true;
        bloom.intensity.overrideState = true;
        bloom.intensity.value = 1.2f;
        bloom.threshold.overrideState = true;
        bloom.threshold.value = 0.9f;

        // Configure Raindrop
        Raindrop raindrop;
        if (!profile.TryGet(out raindrop)) raindrop = profile.Add<Raindrop>(true);
        raindrop.active = true;
        raindrop.Raining.overrideState = true;
        raindrop.Raining.value = 1f;
        raindrop.Distortion.overrideState = true;
        raindrop.Distortion.value = 0.55f;
        raindrop.DropletsGravity.overrideState = true;
        raindrop.DropletsGravity.value = 0.75f;
        raindrop.DropletsSpeed.overrideState = true;
        raindrop.DropletsSpeed.value = 0.6f;
        raindrop.DropletsStrength.overrideState = true;
        raindrop.DropletsStrength.value = 0.65f;
        raindrop.Tiling.overrideState = true;
        raindrop.Tiling.value = new Vector2(1.5f, 1.5f);

        Texture2D maskTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/ThunderWire Studio/UHFPS/Content/Shaders/Raindrop/Materials/droplets_mask.tif");
        if (maskTex != null)
        {
            raindrop.DropletsMask.overrideState = true;
            raindrop.DropletsMask.value = maskTex;
        }

        if (isNewProfile)
        {
            AssetDatabase.CreateAsset(profile, volumeProfilePath);
        }
        else
        {
            EditorUtility.SetDirty(profile);
        }
        volumeComp.profile = profile;

        // Attach MainMenuGlitchEffect script to run voltage glitches
        var glitchEffect = volumeGo.AddComponent<MainMenuGlitchEffect>();
        glitchEffect.volume = volumeComp;


        // 10. Atmospheric Setup (Denser, darker Fog and Skybox)
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.008f, 0.012f, 0.02f); // Darker, desaturated night fog
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = 0.024f; // Denser fog for horror mystery

        Material skyboxMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/AssetsDescargados/AllSkyFree/Night MoonBurst/Night Moon Burst.mat");
        if (skyboxMat != null)
        {
            RenderSettings.skybox = skyboxMat;
        }

        // 11. Camera Setup
        GameObject mainCameraGo = GameObject.FindWithTag("MainCamera");
        GameObject flashGo = GameObject.Find("FlashlightSpotlight");
        if (mainCameraGo != null)
        {
            Undo.RecordObject(mainCameraGo.transform, "Configure Main Camera Position");
            float camY = GetTerrainHeight(0f, -12f, terrainComp) + 1.6f; // Lowered camera height slightly
            mainCameraGo.transform.position = new Vector3(0f, camY, -12f);
            mainCameraGo.transform.eulerAngles = new Vector3(3.0f, -2.0f, 1.2f); // Uneasy Dutch angle

            Camera cam = mainCameraGo.GetComponent<Camera>();
            if (cam != null)
            {
                Undo.RecordObject(cam, "Configure Camera FOV");
                cam.fieldOfView = 52f;
                cam.farClipPlane = 250f;
            }

            // Add handheld breathing sway effect to the camera!
            var breathing = mainCameraGo.GetComponent<MainMenuCameraBreathing>();
            if (breathing == null)
            {
                breathing = mainCameraGo.AddComponent<MainMenuCameraBreathing>();
            }
            breathing.translationSpeed = 0.45f;
            breathing.translationAmount = 0.05f;
            breathing.rotationSpeed = 0.35f;
            breathing.rotationAmount = 0.15f;

            // Create New Game Flashlight under Camera
            if (flashGo != null)
            {
                Undo.DestroyObjectImmediate(flashGo);
            }
            flashGo = new GameObject("FlashlightSpotlight");
            flashGo.transform.parent = mainCameraGo.transform;
            flashGo.transform.localPosition = new Vector3(0f, 0f, 0f);
            flashGo.transform.localEulerAngles = new Vector3(6f, 0f, 0f); // Point slightly down the road
            Light flashLight = flashGo.AddComponent<Light>();
            flashLight.type = LightType.Spot;
            flashLight.color = new Color(0.95f, 0.95f, 1.0f); // LED White
            flashLight.intensity = 9.5f; // Brighter core spotlight
            flashLight.range = 40f;
            flashLight.spotAngle = 32f; // Narrower spotlight beam
            flashLight.innerSpotAngle = 22f;
            flashLight.shadows = LightShadows.Soft;
            flashLight.enabled = false;
            flashGo.AddComponent<UniversalAdditionalLightData>();

            // Add battery flicker to the flashlight spotlight too
            var flashFlicker = flashGo.GetComponent<MainMenuLightFlicker>();
            if (flashFlicker == null)
            {
                flashFlicker = flashGo.AddComponent<MainMenuLightFlicker>();
            }
            flashFlicker.flickerSpeed = 0.08f;
            flashFlicker.minIntensityMultiplier = 0.92f;
            flashFlicker.maxIntensityMultiplier = 1.08f;

            // Add Camera Transitions Script
            var transition = mainCameraGo.GetComponent<MainMenuCameraTransition>();
            if (transition == null)
            {
                transition = mainCameraGo.AddComponent<MainMenuCameraTransition>();
            }
        }
        else
        {
            Debug.LogWarning("No MainCamera found with tag 'MainCamera' to reposition.");
        }

        // 12. Ambient Sound Setup
        GameObject ambientGo = GameObject.Find("AmbientSound");
        if (ambientGo != null)
        {
            Undo.DestroyObjectImmediate(ambientGo);
        }
        ambientGo = new GameObject("AmbientSound");
        ambientGo.transform.parent = root.transform;
        AudioSource ambientSource = ambientGo.AddComponent<AudioSource>();
        AudioClip windClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/AssetsDescargados/Pueblo/Flooded_Grounds/Content/Sounds/WindHowl.mp3");
        if (windClip != null)
        {
            ambientSource.clip = windClip;
            ambientSource.loop = true;
            ambientSource.volume = 0.25f;
            ambientSource.playOnAwake = true;
            ambientSource.spatialBlend = 0f; // 2D Sound
            ambientSource.Play();
        }

        // 13. UI Setup: Ensure Background is active but its Image is disabled so we see the 3D scene
        GameObject mainMenuGo = GameObject.Find("MAINMENU");
        var cameraTransComp = mainCameraGo != null ? mainCameraGo.GetComponent<MainMenuCameraTransition>() : null;

        if (mainMenuGo != null)
        {
            Transform canvasTrans = mainMenuGo.transform.Find("Canvas");
            if (canvasTrans != null)
            {
                Transform bgTrans = canvasTrans.Find("Background");
                if (bgTrans != null)
                {
                    GameObject bgGo = bgTrans.gameObject;
                    Undo.RecordObject(bgGo, "Configure UI Background active state");
                    bgGo.SetActive(true);
                    
                    var bgImage = bgGo.GetComponent<UnityEngine.UI.Image>();
                    if (bgImage != null)
                    {
                        Undo.RecordObject(bgImage, "Disable solid black background image");
                        bgImage.enabled = false;
                    }

                    // Find Blur child
                    Transform blurTrans = bgTrans.Find("Blur");
                    if (blurTrans != null)
                    {
                        // Configure camera transition waypoints/panels
                        if (cameraTransComp != null)
                        {
                            Transform mainPanelTrans = blurTrans.Find("MainMenu");
                            if (mainPanelTrans != null) cameraTransComp.mainMenuPanel = mainPanelTrans.gameObject;
                            
                            Transform panelsTrans = blurTrans.Find("MenuPanels");
                            if (panelsTrans != null)
                            {
                                Transform optionsTrans = panelsTrans.Find("Options");
                                if (optionsTrans != null) cameraTransComp.optionsPanel = optionsTrans.gameObject;
                                
                                Transform loadTrans = panelsTrans.Find("LoadGame");
                                if (loadTrans != null) cameraTransComp.loadGamePanel = loadTrans.gameObject;
                            }
                        }

                        // Attach Logo Effect
                        Transform logoTrans = blurTrans.Find("MainMenu/Logo");
                        if (logoTrans != null)
                        {
                            GameObject logoGo = logoTrans.gameObject;
                            var logoEffect = logoGo.GetComponent<MainMenuLogoEffect>();
                            if (logoEffect == null)
                            {
                                logoEffect = logoGo.AddComponent<MainMenuLogoEffect>();
                            }
                        }

                        // Load Font & Audio assets for buttons
                        TMPro.TMP_FontAsset horrorFont = AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>("Assets/ThunderWire Studio/UHFPS/Content/Fonts/Roboto Condensed/TMP/RobotoCondensed-Regular SDF.asset");
                        AudioClip hoverSFX = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/AssetsDescargados/AdvancedMobileHorror/Sounds/Audio_InventorySelect.wav");
                        AudioClip clickSFX = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/AssetsDescargados/AdvancedMobileHorror/Sounds/Audio_ButtonClick.wav");
                        AudioClip whisperSFX = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/Sintomas/whisper.mp3");
                        
                        Light playerLanternComp = GameObject.Find("Player Lantern Light")?.GetComponent<Light>();
                        Light flashlightSpotComp = flashGo != null ? flashGo.GetComponent<Light>() : null;

                        // Setup button layout group spacing for readability
                        var layoutGroup = blurTrans.Find("MainMenu/MenuButtons")?.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
                        if (layoutGroup != null)
                        {
                            Undo.RecordObject(layoutGroup, "Adjust Button Vertical Spacing");
                            layoutGroup.spacing = 24f; // More compact spacing for smaller font
                        }

                        // Setup buttons
                        string[] buttonNames = { "Continue", "NewGame", "LoadGame", "Options", "Quit" };
                        foreach (string bName in buttonNames)
                        {
                            Transform btnTrans = blurTrans.Find("MainMenu/MenuButtons/" + bName);
                            if (btnTrans != null)
                            {
                                GameObject btnGo = btnTrans.gameObject;
                                
                                // Resize the button RectTransform so larger text fits perfectly
                                RectTransform btnRect = btnGo.GetComponent<RectTransform>();
                                if (btnRect != null)
                                {
                                    Undo.RecordObject(btnRect, "Resize Button");
                                    btnRect.sizeDelta = new Vector2(750f, 80f); // Reduced height to 80f (was 110f)
                                }

                                var buttonEffects = btnGo.GetComponent<MainMenuButtonEffects>();
                                if (buttonEffects == null)
                                {
                                    buttonEffects = btnGo.AddComponent<MainMenuButtonEffects>();
                                }
                                
                                buttonEffects.hoverClip = hoverSFX;
                                buttonEffects.clickClip = clickSFX;
                                buttonEffects.whisperClip = null;
                                buttonEffects.playerLantern = playerLanternComp;
                                buttonEffects.flashlightSpot = flashlightSpotComp;

                                // Set font, size, and outlines for readability safely
                                var txtComp = btnGo.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                                if (txtComp != null && horrorFont != null)
                                {
                                    try
                                    {
                                        Undo.RecordObject(txtComp, "Apply Horror Font and Size");
                                        txtComp.font = horrorFont;
                                        txtComp.fontSharedMaterial = horrorFont.material;
                                        txtComp.enableAutoSizing = true;
                                        txtComp.fontSizeMin = 20f;
                                        txtComp.fontSizeMax = 45f;
                                        txtComp.outlineWidth = 0.22f;
                                        txtComp.outlineColor = new Color32(0, 0, 0, 240);
                                        txtComp.characterSpacing = 1.5f; // Extra spacing for a cleaner, cinematic look
                                    }
                                    catch (System.Exception ex)
                                    {
                                        Debug.LogWarning("Caught TMPro editor setup exception (safe to ignore): " + ex.Message);
                                    }
                                }
                            }
                        }

                        // Attach Custom Cursor Component to MAINMENU
                        var customCursor = mainMenuGo.GetComponent<MainMenuCustomCursor>();
                        if (customCursor == null)
                        {
                            customCursor = mainMenuGo.AddComponent<MainMenuCustomCursor>();
                        }
                        Texture2D cursorTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/ThunderWire Studio/UHFPS/Content/Art/Textures/UI/Reticles/Hand/hand_pointing_1.png");
                        if (cursorTex != null)
                        {
                            customCursor.cursorTexture = cursorTex;
                            customCursor.hotspot = new Vector2(10f, 6f);
                        }

                        // Attach Dynamic Thunder Shadows to the ThunderManager light
                        var thunderManager = Object.FindAnyObjectByType<AdvancedHorrorFPS.ThunderManager>();
                        if (thunderManager != null)
                        {
                            var shadowsComp = thunderManager.gameObject.GetComponent<MainMenuThunderShadows>();
                            if (shadowsComp == null)
                            {
                                shadowsComp = thunderManager.gameObject.AddComponent<MainMenuThunderShadows>();
                            }
                            shadowsComp.maxAngleDriftY = 10f;
                            shadowsComp.maxAngleDriftX = 5f;
                            shadowsComp.speedX = 28f;
                            shadowsComp.speedY = 38f;
                        }
                    }
                }
            }
        }
        // 14.1. La Dama Espectral (Horror Maiden)
        string maidenPrefabPath = "Assets/HorrorMaiden/Prefabs/WetDressLongHair.prefab";
        GameObject maidenPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(maidenPrefabPath);
        if (maidenPrefab != null)
        {
            GameObject oldMaiden = GameObject.Find("Creepy_Maiden");
            if (oldMaiden != null) Undo.DestroyObjectImmediate(oldMaiden);

            GameObject maiden = (GameObject)PrefabUtility.InstantiatePrefab(maidenPrefab);
            maiden.name = "Creepy_Maiden";
            maiden.transform.parent = root.transform;

            float maidenY = GetTerrainHeight(-2.6f, -7.5f, terrainComp) - 0.05f;
            maiden.transform.localPosition = new Vector3(-2.6f, maidenY, -7.5f);
            maiden.transform.localEulerAngles = new Vector3(0f, 135f, 0f);
            maiden.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

            var creepyIdle = maiden.GetComponent<MainMenuCreepyIdle>();
            if (creepyIdle == null) creepyIdle = maiden.AddComponent<MainMenuCreepyIdle>();
            
            creepyIdle.rockAmount = 0f; // Disable toy horse rocking
            creepyIdle.lookSpeed = 1.5f;
            creepyIdle.idleDelay = 3.0f;
            if (mainCameraGo != null) creepyIdle.lookTarget = mainCameraGo.transform;

            // Add Animator and assign horror idle controller to remove T-pose
            var animator = maiden.GetComponent<Animator>();
            if (animator == null) animator = maiden.AddComponent<Animator>();
            
            string maidenAnimPath = "Assets/HorrorMaiden/Art/AnimatorControllers/HorrorMaiden_Idle_Jittery01.controller";
            var maidenController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(maidenAnimPath);
            if (maidenController == null)
            {
                maidenAnimPath = "Assets/HorrorMaiden/Art/AnimatorControllers/HorrorMaiden_Idle.controller";
                maidenController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(maidenAnimPath);
            }
            if (maidenController != null)
            {
                animator.runtimeAnimatorController = maidenController;
            }
        }
        else
        {
            Debug.LogError("Could not find Horror Maiden prefab at: " + maidenPrefabPath);
        }

        // 14.2. El Columpio Oscilante (Creepy Swing)
        string swingPrefabPath = "Assets/Models/Columpio/Columpios.fbx";
        GameObject swingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(swingPrefabPath);
        if (swingPrefab != null)
        {
            GameObject oldSwing = GameObject.Find("Creepy_Swing");
            if (oldSwing != null) Undo.DestroyObjectImmediate(oldSwing);

            GameObject swing = (GameObject)PrefabUtility.InstantiatePrefab(swingPrefab);
            swing.name = "Creepy_Swing";
            swing.transform.parent = root.transform;

            float swingY = GetTerrainHeight(3.6f, -5.0f, terrainComp) - 0.05f;
            swing.transform.localPosition = new Vector3(3.6f, swingY, -5.0f);
            swing.transform.localEulerAngles = new Vector3(0f, 75f, 0f);
            swing.transform.localScale = new Vector3(2.2f, 2.2f, 2.2f); // Match forest scene scale

            // Find child "PivotL" so only the left seat/ropes swing, not the frame (SM_Swingset_1)!
            Transform seatChild = null;
            foreach (Transform child in swing.GetComponentsInChildren<Transform>())
            {
                if (child.name == "PivotL")
                {
                    seatChild = child;
                    break;
                }
            }

            // Fallback search case-insensitive
            if (seatChild == null)
            {
                foreach (Transform child in swing.GetComponentsInChildren<Transform>())
                {
                    if (child != swing.transform && child.name.ToLower().Contains("pivotl"))
                    {
                        seatChild = child;
                        break;
                    }
                }
            }

            GameObject targetSwingObj = seatChild != null ? seatChild.gameObject : swing;

            var swingInertia = targetSwingObj.GetComponent<ColumpioInercia>();
            if (swingInertia == null) swingInertia = targetSwingObj.AddComponent<ColumpioInercia>();

            // Set Y rotation axis to match the forest scene pendulum physics for Columpios.fbx!
            swingInertia.ejeDeRotacion = ColumpioInercia.EjeRotacion.Y;
            swingInertia.gravedad = 9.81f;
            swingInertia.longitudPendulo = 2.5f;
            swingInertia.masa = 5.0f;
            swingInertia.amortiguacion = 0.2f;
            swingInertia.modoImpulso = ColumpioInercia.ModoImpulso.Automatico;
            swingInertia.anguloMaximoAutomatico = 25f;
            swingInertia.efectoFantasma = true;
            swingInertia.fuerzaFantasma = 1.2f;
            swingInertia.frecuenciaFantasma = 0.5f;

            AudioSource swingAudio = targetSwingObj.GetComponent<AudioSource>();
            if (swingAudio == null) swingAudio = targetSwingObj.AddComponent<AudioSource>();
            swingAudio.spatialBlend = 1f;
            swingAudio.minDistance = 1f;
            swingAudio.maxDistance = 15f;
            swingAudio.playOnAwake = false;

            AudioClip swingClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/AssetsDescargados/AdvancedMobileHorror/Sounds/Swinging.wav");
            if (swingClip != null)
            {
                swingAudio.clip = swingClip;
                swingInertia.fuenteAudioCreak = swingAudio;
                swingInertia.modoAudio = ColumpioInercia.ModoAudio.LoopModuladoVelocidad;
                swingInertia.volumenMaximo = 0.4f;
            }
        }
        else
        {
            Debug.LogError("Could not find Swingset model at: " + swingPrefabPath);
        }

        // 14.3. El Santuario de San La Muerte
        string shrinePath = "Assets/Models/Santuarios/San_La_Muerte/Meshy_AI_San_La_Muerte_0405222843_texture.fbx";
        GameObject shrineModel = AssetDatabase.LoadAssetAtPath<GameObject>(shrinePath);
        if (shrineModel != null)
        {
            GameObject oldShrine = GameObject.Find("Creepy_Shrine");
            if (oldShrine != null) Undo.DestroyObjectImmediate(oldShrine);

            GameObject shrine = (GameObject)PrefabUtility.InstantiatePrefab(shrineModel);
            shrine.name = "Creepy_Shrine";
            shrine.transform.parent = root.transform;

            float shrineY = GetTerrainHeight(-3.8f, -1.8f, terrainComp) - 0.05f;
            shrine.transform.localPosition = new Vector3(-3.8f, shrineY, -1.8f);
            shrine.transform.localEulerAngles = new Vector3(0f, 135f, 0f);
            shrine.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);

            string candlePath = "Assets/AssetsDescargados/Bosque/Prefaps/Candle.prefab";
            GameObject candlePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(candlePath);
            if (candlePrefab != null)
            {
                GameObject candle = (GameObject)PrefabUtility.InstantiatePrefab(candlePrefab);
                candle.name = "Shrine_Candle";
                candle.transform.parent = shrine.transform;
                candle.transform.localPosition = new Vector3(0f, 0.65f, 0.2f);
                candle.transform.localEulerAngles = Vector3.zero;
                candle.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);

                Light candleLight = candle.GetComponentInChildren<Light>();
                if (candleLight == null)
                {
                    GameObject candleLightGo = new GameObject("Candle_Light");
                    candleLightGo.transform.parent = candle.transform;
                    candleLightGo.transform.localPosition = new Vector3(0f, 0.2f, 0f);
                    candleLight = candleLightGo.AddComponent<Light>();
                    candleLight.type = LightType.Point;
                    candleLightGo.AddComponent<UniversalAdditionalLightData>();
                }
                
                candleLight.color = new Color(0.98f, 0.52f, 0.12f); // Warm candle amber
                candleLight.intensity = 5.0f;
                candleLight.range = 8.0f;
                candleLight.shadows = LightShadows.Soft;

                var candleFlicker = candleLight.gameObject.GetComponent<MainMenuLightFlicker>();
                if (candleFlicker == null) candleFlicker = candleLight.gameObject.AddComponent<MainMenuLightFlicker>();
                candleFlicker.flickerSpeed = 0.25f;
                candleFlicker.minIntensityMultiplier = 0.60f;
                candleFlicker.maxIntensityMultiplier = 1.40f;
            }
            else
            {
                GameObject candleLightGo = new GameObject("Candle_Light");
                candleLightGo.transform.parent = shrine.transform;
                candleLightGo.transform.localPosition = new Vector3(0f, 0.8f, 0.2f);

                Light candleLight = candleLightGo.AddComponent<Light>();
                candleLight.type = LightType.Point;
                candleLight.color = new Color(0.98f, 0.52f, 0.12f);
                candleLight.intensity = 5.0f;
                candleLight.range = 8.0f;
                candleLight.shadows = LightShadows.Soft;

                candleLightGo.AddComponent<UniversalAdditionalLightData>();

                var candleFlicker = candleLightGo.AddComponent<MainMenuLightFlicker>();
                candleFlicker.flickerSpeed = 0.25f;
                candleFlicker.minIntensityMultiplier = 0.60f;
                candleFlicker.maxIntensityMultiplier = 1.40f;
            }
        }
        else
        {
            Debug.LogError("Could not find San La Muerte model at: " + shrinePath);
        }

        // 14.4. El Enemigo Común en las Sombras (NPC)
        string npcPath = "Assets/Models/NPC/Comun/Npc_Comun_Pose_T.fbx";
        GameObject npcModel = AssetDatabase.LoadAssetAtPath<GameObject>(npcPath);
        if (npcModel != null)
        {
            GameObject oldNPC = GameObject.Find("Creepy_NPC");
            if (oldNPC != null) Undo.DestroyObjectImmediate(oldNPC);

            GameObject npc = (GameObject)PrefabUtility.InstantiatePrefab(npcModel);
            npc.name = "Creepy_NPC";
            npc.transform.parent = root.transform;

            float npcY = GetTerrainHeight(0.5f, 28.0f, terrainComp) - 0.05f; // Placed far in the middle of the road
            npc.transform.localPosition = new Vector3(0.5f, npcY, 28.0f);
            npc.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
            npc.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

            // Add Animator and assign leader/generic NPC controller to remove T-pose
            var animator = npc.GetComponent<Animator>();
            if (animator == null) animator = npc.AddComponent<Animator>();
            
            string npcAnimPath = "Assets/Models/NPC/Lider/AC_NPC_Lider.controller";
            var npcController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(npcAnimPath);
            if (npcController != null)
            {
                animator.runtimeAnimatorController = npcController;
            }

            var creepyIdle = npc.GetComponent<MainMenuCreepyIdle>();
            if (creepyIdle == null) creepyIdle = npc.AddComponent<MainMenuCreepyIdle>();
            
            creepyIdle.rockAmount = 0f; // Disable toy horse rocking
            creepyIdle.lookSpeed = 1.0f;
            creepyIdle.idleDelay = 4.0f;
            if (mainCameraGo != null) creepyIdle.lookTarget = mainCameraGo.transform;
        }
        else
        {
            Debug.LogError("Could not find NPC model at: " + npcPath);
        }

        // 14.5. Blood Decal on Welcome Sign
        string bloodPath = "Assets/Blood decal pack/blood/prefab/blood1.prefab";
        GameObject bloodPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(bloodPath);
        if (bloodPrefab != null)
        {
            GameObject oldBlood = GameObject.Find("Sign_Blood_Decal");
            if (oldBlood != null) Undo.DestroyObjectImmediate(oldBlood);

            GameObject blood = (GameObject)PrefabUtility.InstantiatePrefab(bloodPrefab);
            blood.name = "Sign_Blood_Decal";
            
            GameObject signGo = GameObject.Find("Cartel_WelcomeEpecuen");
            if (signGo != null)
            {
                blood.transform.parent = root.transform; // parent to root so its scale is 1
                Renderer signRenderer = signGo.GetComponentInChildren<Renderer>();
                Vector3 centerPos = signGo.transform.position;
                if (signRenderer != null)
                {
                    centerPos = signRenderer.bounds.center;
                }
                Vector3 normal = signGo.transform.up; // Sign normal in world space (local Up)
                blood.transform.position = centerPos + normal * 0.08f;
                blood.transform.rotation = Quaternion.LookRotation(normal, Vector3.up);
                blood.transform.localScale = new Vector3(1.6f, 1.6f, 1.6f);
            }
            else
            {
                blood.transform.parent = root.transform;
                blood.transform.localPosition = new Vector3(-4.0f, -3.8f, -0.9f);
                blood.transform.localEulerAngles = new Vector3(0f, 45f, 0f);
                blood.transform.localScale = new Vector3(1.6f, 1.6f, 1.6f);
            }
        }

        // 14.6. AAA Environmental Details
        // 14.6.1. Creepy Light Pole
        string lightPolePath = "Assets/AssetsDescargados/Bosque/Prefaps/Poste_luz.prefab";
        GameObject lightPolePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(lightPolePath);
        if (lightPolePrefab != null)
        {
            GameObject oldPole = GameObject.Find("Creepy_LightPole");
            if (oldPole != null) Undo.DestroyObjectImmediate(oldPole);
            
            GameObject pole = (GameObject)PrefabUtility.InstantiatePrefab(lightPolePrefab);
            pole.name = "Creepy_LightPole";
            pole.transform.parent = root.transform;
            float poleX = 4.2f;
            float poleZ = 10.0f;
            float poleY = GetTerrainHeight(poleX, poleZ, terrainComp) - 0.1f;
            pole.transform.localPosition = new Vector3(poleX, poleY, poleZ);
            pole.transform.localEulerAngles = new Vector3(-3f, 210f, 4f); // Creepy tilt
            pole.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);

            Light poleLight = pole.GetComponentInChildren<Light>();
            if (poleLight == null)
            {
                GameObject poleLightGo = new GameObject("Street_Light");
                poleLightGo.transform.parent = pole.transform;
                poleLightGo.transform.localPosition = new Vector3(0f, 4.5f, -1.0f);
                poleLight = poleLightGo.AddComponent<Light>();
                poleLight.type = LightType.Spot;
                poleLightGo.AddComponent<UniversalAdditionalLightData>();
            }
            
            poleLight.color = new Color(0.95f, 0.85f, 0.4f); // Sickly yellow
            poleLight.intensity = 8f;
            poleLight.range = 15f;
            poleLight.spotAngle = 60f;
            poleLight.shadows = LightShadows.Soft;

            var poleFlicker = poleLight.gameObject.GetComponent<MainMenuLightFlicker>();
            if (poleFlicker == null) poleFlicker = poleLight.gameObject.AddComponent<MainMenuLightFlicker>();
            poleFlicker.flickerSpeed = 0.05f; // Nervous flicker
            poleFlicker.minIntensityMultiplier = 0.2f;
            poleFlicker.maxIntensityMultiplier = 1.3f;
        }

        // 14.6.2. Creepy Scarecrow
        string scarecrowPath = "Assets/AssetsDescargados/Bosque/Prefaps/Espantapajaros.prefab";
        GameObject scarecrowPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(scarecrowPath);
        if (scarecrowPrefab != null)
        {
            GameObject oldScarecrow = GameObject.Find("Creepy_Scarecrow");
            if (oldScarecrow != null) Undo.DestroyObjectImmediate(oldScarecrow);

            GameObject scarecrow = (GameObject)PrefabUtility.InstantiatePrefab(scarecrowPrefab);
            scarecrow.name = "Creepy_Scarecrow";
            scarecrow.transform.parent = root.transform;
            float scX = -4.5f;
            float scZ = 5.0f;
            float scY = GetTerrainHeight(scX, scZ, terrainComp) - 0.05f;
            scarecrow.transform.localPosition = new Vector3(scX, scY, scZ);
            scarecrow.transform.localEulerAngles = new Vector3(0f, 140f, 0f);
            scarecrow.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
        }

        // 14.6.3. Creepy Gallows (Poste de ahorcado)
        string gallowsPath = "Assets/AssetsDescargados/Bosque/Prefaps/Poste_ahorcado_3.prefab";
        GameObject gallowsPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(gallowsPath);
        if (gallowsPrefab != null)
        {
            GameObject oldGallows = GameObject.Find("Creepy_Gallows");
            if (oldGallows != null) Undo.DestroyObjectImmediate(oldGallows);

            GameObject gallows = (GameObject)PrefabUtility.InstantiatePrefab(gallowsPrefab);
            gallows.name = "Creepy_Gallows";
            gallows.transform.parent = root.transform;
            float galX = 5.5f;
            float galZ = 20.0f;
            float galY = GetTerrainHeight(galX, galZ, terrainComp) - 0.1f;
            gallows.transform.localPosition = new Vector3(galX, galY, galZ);
            gallows.transform.localEulerAngles = new Vector3(-2f, 250f, 0f);
            gallows.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
        }

        // 14.6.4. Creepy Crows
        string crowPath = "Assets/AssetsDescargados/Bosque/Prefaps/= CROW.prefab";
        GameObject crowPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(crowPath);
        if (crowPrefab != null)
        {
            GameObject oldCrows = GameObject.Find("Creepy_Crows");
            if (oldCrows != null) Undo.DestroyObjectImmediate(oldCrows);

            GameObject crowsGroup = new GameObject("Creepy_Crows");
            crowsGroup.transform.parent = root.transform;

            // Crow 1: on the Welcome Sign
            GameObject signGoForCrow = GameObject.Find("Cartel_WelcomeEpecuen");
            if (signGoForCrow != null)
            {
                GameObject crow1 = (GameObject)PrefabUtility.InstantiatePrefab(crowPrefab);
                crow1.name = "Crow_Sign";
                crow1.transform.parent = root.transform; // parent to root so its scale is 1
                Renderer signRenderer = signGoForCrow.GetComponentInChildren<Renderer>();
                Vector3 spawnPos = signGoForCrow.transform.position + signGoForCrow.transform.forward * 1.5f; // Fallback
                if (signRenderer != null)
                {
                    spawnPos = new Vector3(signRenderer.bounds.center.x, signRenderer.bounds.max.y + 0.02f, signRenderer.bounds.center.z);
                }
                crow1.transform.position = spawnPos;
                crow1.transform.localEulerAngles = new Vector3(0f, 45f, 0f); // Face the road
                crow1.transform.localScale = new Vector3(0.08f, 0.08f, 0.08f); // Normal crow scale
            }

            // Crow 2: on the dead tree
            GameObject crow2 = (GameObject)PrefabUtility.InstantiatePrefab(crowPrefab);
            crow2.name = "Crow_Tree";
            crow2.transform.parent = crowsGroup.transform;
            crow2.transform.localPosition = new Vector3(-3.8f, GetTerrainHeight(-3.8f, -9.0f, terrainComp) + 3.2f, -9.0f);
            crow2.transform.localEulerAngles = new Vector3(0f, 45f, 0f);
            crow2.transform.localScale = new Vector3(0.08f, 0.08f, 0.08f);

            // Crow 3: on a fence post on the right
            GameObject crow3 = (GameObject)PrefabUtility.InstantiatePrefab(crowPrefab);
            crow3.name = "Crow_Fence";
            crow3.transform.parent = crowsGroup.transform;
            crow3.transform.localPosition = new Vector3(3.9f, GetTerrainHeight(3.9f, 2.5f, terrainComp) + 1.1f, 2.5f);
            crow3.transform.localEulerAngles = new Vector3(0f, 290f, 0f);
            crow3.transform.localScale = new Vector3(0.08f, 0.08f, 0.08f);
        }

        // Force scene view update
        SceneView.RepaintAll();
        EditorUtility.SetDirty(root);
        Debug.Log("MainMenu Background Built Successfully!");
    }

    private static float GetTerrainHeight(float x, float z, Terrain terrain)
    {
        if (terrain == null || terrain.terrainData == null) return 0f;
        return terrain.SampleHeight(new Vector3(x, 0f, z)) + terrain.transform.position.y;
    }

    private static void alphaps(int x, int y, float[,,] alphamaps, float t)
    {
        alphamaps[x, y, 0] = t;
        alphamaps[x, y, 1] = 1.0f - t;
    }

    [MenuItem("Antigravity/Add Crow to Sign In-Place")]
    public static void AddCrowToSignInPlace()
    {
        string crowPath = "Assets/AssetsDescargados/Bosque/Prefaps/= CROW.prefab";
        GameObject crowPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(crowPath);
        if (crowPrefab == null)
        {
            Debug.LogError("Crow prefab not found");
            return;
        }

        GameObject signGo = GameObject.Find("Cartel_WelcomeEpecuen");
        if (signGo == null)
        {
            Debug.LogError("Welcome sign not found");
            return;
        }

        GameObject oldCrow = GameObject.Find("Crow_Sign");
        if (oldCrow != null) Undo.DestroyObjectImmediate(oldCrow);

        GameObject root = GameObject.Find("= BACKGROUND");
        Transform parentTrans = root != null ? root.transform : null;

        GameObject crow = (GameObject)PrefabUtility.InstantiatePrefab(crowPrefab);
        crow.name = "Crow_Sign";
        crow.transform.parent = parentTrans;

        Renderer signRenderer = signGo.GetComponentInChildren<Renderer>();
        Vector3 spawnPos = signGo.transform.position + signGo.transform.forward * 1.5f;
        if (signRenderer != null)
        {
            spawnPos = new Vector3(signRenderer.bounds.center.x, signRenderer.bounds.max.y + 0.02f, signRenderer.bounds.center.z);
        }

        crow.transform.position = spawnPos;
        crow.transform.localEulerAngles = new Vector3(0f, 45f, 0f);
        crow.transform.localScale = new Vector3(0.08f, 0.08f, 0.08f);

        Undo.RegisterCreatedObjectUndo(crow, "Add Crow to Sign");
        
        GameObject oldBlood = GameObject.Find("Sign_Blood_Decal");
        if (oldBlood == null)
        {
            string bloodPath = "Assets/Blood decal pack/blood/prefab/blood1.prefab";
            GameObject bloodPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(bloodPath);
            if (bloodPrefab != null)
            {
                GameObject blood = (GameObject)PrefabUtility.InstantiatePrefab(bloodPrefab);
                blood.name = "Sign_Blood_Decal";
                blood.transform.parent = parentTrans;

                Renderer signRenderer2 = signGo.GetComponentInChildren<Renderer>();
                Vector3 centerPos = signGo.transform.position;
                if (signRenderer2 != null)
                {
                    centerPos = signRenderer2.bounds.center;
                }
                Vector3 normal = signGo.transform.up;
                blood.transform.position = centerPos + normal * 0.08f;
                blood.transform.rotation = Quaternion.LookRotation(normal, Vector3.up);
                blood.transform.localScale = new Vector3(1.6f, 1.6f, 1.6f);
                Undo.RegisterCreatedObjectUndo(blood, "Add Blood to Sign");
            }
        }
        
        SceneView.RepaintAll();
        Debug.Log("Crow and Blood Decal added/refreshed on sign successfully in-place!");
    }
}
