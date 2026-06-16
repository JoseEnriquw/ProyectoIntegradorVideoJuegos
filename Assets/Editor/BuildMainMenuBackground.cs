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

        // 6.1. Road Silhouette Rim Light (Backlight down the road)
        GameObject backlightGo = new GameObject("Road Silhouette Light");
        backlightGo.transform.parent = root.transform;
        float backlightY = GetTerrainHeight(0f, 38f, terrainComp) + 5.0f;
        backlightGo.transform.localPosition = new Vector3(0f, backlightY, 38f);
        backlightGo.transform.localEulerAngles = new Vector3(15f, 180f, 0f); // Point down the road towards the player/camera
        Light backlight = backlightGo.AddComponent<Light>();
        backlight.type = LightType.Spot;
        backlight.color = new Color(0.18f, 0.28f, 0.42f); // Cinematic cool steel blue
        backlight.intensity = 8.5f; // Stronger to cut through fog and backlight the NPC
        backlight.range = 45f;
        backlight.spotAngle = 65f;
        backlight.innerSpotAngle = 25f;
        backlight.shadows = LightShadows.None;
        backlightGo.AddComponent<UniversalAdditionalLightData>();

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
            
            string npcAnimPath = "Assets/ThunderWire Studio/UHFPS/Content/Animation/Zombie/Zombie.controller";
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

        // 14.7. AAA Forest Environmental Details
        // 14.7.1. Fallen Logs
        string logPrefabPath = "Assets/ThunderWire Studio/UHFPS/_Demo/Environment/Nature/Log/Log.prefab";
        GameObject logPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(logPrefabPath);
        if (logPrefab != null)
        {
            // Log 1: Left side, near the scarecrow
            GameObject log1 = (GameObject)PrefabUtility.InstantiatePrefab(logPrefab);
            log1.name = "Forest_Log_1";
            log1.transform.parent = root.transform;
            float l1X = -4.8f;
            float l1Z = 3.5f;
            float l1Y = GetTerrainHeight(l1X, l1Z, terrainComp) - 0.15f;
            log1.transform.localPosition = new Vector3(l1X, l1Y, l1Z);
            log1.transform.localEulerAngles = new Vector3(8f, 112f, -5f);
            log1.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);

            // Log 2: Right side, near the swing
            GameObject log2 = (GameObject)PrefabUtility.InstantiatePrefab(logPrefab);
            log2.name = "Forest_Log_2";
            log2.transform.parent = root.transform;
            float l2X = 4.2f;
            float l2Z = -3.2f;
            float l2Y = GetTerrainHeight(l2X, l2Z, terrainComp) - 0.2f;
            log2.transform.localPosition = new Vector3(l2X, l2Y, l2Z);
            log2.transform.localEulerAngles = new Vector3(-5f, 25f, 12f);
            log2.transform.localScale = new Vector3(1.2f, 1.2f, 1.5f);
        }

        // 14.7.2. Mossy Rocks
        string rockPrefabPath = "Assets/ThunderWire Studio/UHFPS/_Demo/Environment/Nature/Rock/RockB.prefab";
        GameObject rockPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(rockPrefabPath);
        if (rockPrefab != null)
        {
            // Rock 1: Right foreground
            GameObject rock1 = (GameObject)PrefabUtility.InstantiatePrefab(rockPrefab);
            rock1.name = "Forest_Rock_1";
            rock1.transform.parent = root.transform;
            float r1X = 3.8f;
            float r1Z = -8.5f;
            float r1Y = GetTerrainHeight(r1X, r1Z, terrainComp) - 0.3f;
            rock1.transform.localPosition = new Vector3(r1X, r1Y, r1Z);
            rock1.transform.localEulerAngles = new Vector3(12f, 45f, -8f);
            rock1.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);

            // Rock 2: Left midground, behind Epecuen sign
            GameObject rock2 = (GameObject)PrefabUtility.InstantiatePrefab(rockPrefab);
            rock2.name = "Forest_Rock_2";
            rock2.transform.parent = root.transform;
            float r2X = -5.2f;
            float r2Z = -3.5f;
            float r2Y = GetTerrainHeight(r2X, r2Z, terrainComp) - 0.2f;
            rock2.transform.localPosition = new Vector3(r2X, r2Y, r2Z);
            rock2.transform.localEulerAngles = new Vector3(-15f, 190f, 10f);
            rock2.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
        }

        // 14.7.3. Conifer Saplings (Small Trees for density)
        string saplingPath = "Assets/Forst/Conifers [BOTD]/Render Pipeline Support/URP/Prefabs/PF Conifer Small BOTD URP.prefab";
        GameObject saplingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(saplingPath);
        if (saplingPrefab != null)
        {
            // Spawn 16 saplings randomly in the background forest
            Random.InitState(101); // fixed seed for consistency
            for (int i = 0; i < 16; i++)
            {
                float z = Random.Range(-15f, 45f);
                float x = Random.Range(0, 2) == 0 ? Random.Range(-18f, -7f) : Random.Range(7f, 18f);
                GameObject sapling = (GameObject)PrefabUtility.InstantiatePrefab(saplingPrefab);
                sapling.name = "Forest_Sapling_" + i;
                sapling.transform.parent = root.transform;
                float worldY = GetTerrainHeight(x, z, terrainComp) - 0.15f;
                sapling.transform.localPosition = new Vector3(x, worldY, z);
                sapling.transform.localEulerAngles = new Vector3(Random.Range(-3f, 3f), Random.Range(0f, 360f), Random.Range(-3f, 3f));
                float scale = Random.Range(0.4f, 0.8f);
                sapling.transform.localScale = new Vector3(scale, scale, scale);
            }
        }

        // 14.7.4. Ground Foliage (Ferns and Dry Bushes)
        string fernAPath = "Assets/Samples/Shader Graph/17.3.0/Production Ready Shaders/Environment/Details/Ferns/Fern_A.prefab";
        string fernBPath = "Assets/Samples/Shader Graph/17.3.0/Production Ready Shaders/Environment/Details/Ferns/Fern_B.prefab";
        GameObject fernAPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(fernAPath);
        GameObject fernBPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(fernBPath);
        
        string bushPath = "Assets/TerrainSampleAssets/Prefabs/BushDry_A.prefab";
        GameObject bushPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(bushPath);

        // Spawn ferns near logs and rocks
        GameObject foliageGroup = new GameObject("Ground_Foliage");
        foliageGroup.transform.parent = root.transform;

        if (fernAPrefab != null && fernBPrefab != null)
        {
            // Fern clusters around Log 1 (Left)
            for (int i = 0; i < 4; i++)
            {
                GameObject fern = (GameObject)PrefabUtility.InstantiatePrefab(i % 2 == 0 ? fernAPrefab : fernBPrefab);
                fern.name = "Fern_Log1_" + i;
                fern.transform.parent = foliageGroup.transform;
                float fx = -4.8f + Random.Range(-0.8f, 0.8f);
                float fz = 3.5f + Random.Range(-1.2f, 1.2f);
                float fy = GetTerrainHeight(fx, fz, terrainComp) - 0.05f;
                fern.transform.localPosition = new Vector3(fx, fy, fz);
                fern.transform.localEulerAngles = new Vector3(0f, Random.Range(0f, 360f), 0f);
                float scale = Random.Range(0.8f, 1.3f);
                fern.transform.localScale = new Vector3(scale, scale, scale);
            }

            // Fern clusters around Log 2 (Right)
            for (int i = 0; i < 4; i++)
            {
                GameObject fern = (GameObject)PrefabUtility.InstantiatePrefab(i % 2 == 0 ? fernAPrefab : fernBPrefab);
                fern.name = "Fern_Log2_" + i;
                fern.transform.parent = foliageGroup.transform;
                float fx = 4.2f + Random.Range(-0.8f, 0.8f);
                float fz = -3.2f + Random.Range(-1.2f, 1.2f);
                float fy = GetTerrainHeight(fx, fz, terrainComp) - 0.05f;
                fern.transform.localPosition = new Vector3(fx, fy, fz);
                fern.transform.localEulerAngles = new Vector3(0f, Random.Range(0f, 360f), 0f);
                float scale = Random.Range(0.8f, 1.3f);
                fern.transform.localScale = new Vector3(scale, scale, scale);
            }

            // Fern clusters around Rock 1 (Right foreground)
            for (int i = 0; i < 3; i++)
            {
                GameObject fern = (GameObject)PrefabUtility.InstantiatePrefab(fernAPrefab);
                fern.name = "Fern_Rock1_" + i;
                fern.transform.parent = foliageGroup.transform;
                float fx = 3.8f + Random.Range(-0.6f, 0.6f);
                float fz = -8.5f + Random.Range(-0.6f, 0.6f);
                float fy = GetTerrainHeight(fx, fz, terrainComp) - 0.05f;
                fern.transform.localPosition = new Vector3(fx, fy, fz);
                fern.transform.localEulerAngles = new Vector3(0f, Random.Range(0f, 360f), 0f);
                float scale = Random.Range(0.9f, 1.2f);
                fern.transform.localScale = new Vector3(scale, scale, scale);
            }
        }

        if (bushPrefab != null)
        {
            // Spawn dry bushes along the tree line to hide bases
            for (int i = 0; i < 8; i++)
            {
                GameObject bush = (GameObject)PrefabUtility.InstantiatePrefab(bushPrefab);
                bush.name = "Dry_Bush_" + i;
                bush.transform.parent = foliageGroup.transform;
                float bx = i % 2 == 0 ? Random.Range(-14f, -6f) : Random.Range(6f, 14f);
                float bz = Random.Range(-10f, 30f);
                float by = GetTerrainHeight(bx, bz, terrainComp) - 0.1f;
                bush.transform.localPosition = new Vector3(bx, by, bz);
                bush.transform.localEulerAngles = new Vector3(0f, Random.Range(0f, 360f), 0f);
                float scale = Random.Range(1.2f, 1.8f);
                bush.transform.localScale = new Vector3(scale, scale, scale);
            }
        }

        // 14.7.5. Grass Shoulders along Fences (Vallas)
        string grassPath = "Assets/ThunderWire Studio/UHFPS/_Demo/Environment/Nature/GrassB/GrassB.prefab";
        GameObject grassPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(grassPath);
        if (grassPrefab != null)
        {
            // Place grass clumps at each fence post interval
            float zStart = -15f;
            float zEnd = 28f;
            float step = 4.3f;
            float leftX = -4.0f;
            float rightX = 4.0f;

            for (float zVal = zStart; zVal <= zEnd; zVal += step)
            {
                // Left Grass
                if (zVal < -3f || zVal > 2f) // Skip welcome sign area
                {
                    for (int j = 0; j < 2; j++)
                    {
                        GameObject grass = (GameObject)PrefabUtility.InstantiatePrefab(grassPrefab);
                        grass.name = "Grass_L_" + zVal + "_" + j;
                        grass.transform.parent = foliageGroup.transform;
                        float gx = leftX + Random.Range(-0.3f, 0.3f);
                        float gz = zVal + Random.Range(-0.5f, 0.5f);
                        float gy = GetTerrainHeight(gx, gz, terrainComp) - 0.05f;
                        grass.transform.localPosition = new Vector3(gx, gy, gz);
                        grass.transform.localEulerAngles = new Vector3(0f, Random.Range(0f, 360f), 0f);
                        float scale = Random.Range(1.3f, 1.7f);
                        grass.transform.localScale = new Vector3(scale, scale, scale);
                    }
                }

                // Right Grass
                for (int j = 0; j < 2; j++)
                {
                    GameObject grass = (GameObject)PrefabUtility.InstantiatePrefab(grassPrefab);
                    grass.name = "Grass_R_" + zVal + "_" + j;
                    grass.transform.parent = foliageGroup.transform;
                    float gx = rightX + Random.Range(-0.3f, 0.3f);
                    float gz = zVal + Random.Range(-0.5f, 0.5f);
                    float gy = GetTerrainHeight(gx, gz, terrainComp) - 0.05f;
                    grass.transform.localPosition = new Vector3(gx, gy, gz);
                    grass.transform.localEulerAngles = new Vector3(0f, Random.Range(0f, 360f), 0f);
                    float scale = Random.Range(1.3f, 1.7f);
                    grass.transform.localScale = new Vector3(scale, scale, scale);
                }
            }
        }

        // 14.7.6. Ground Ritual Candles (Flickering candlelight on the soil)
        string candlePrefabPath = "Assets/AssetsDescargados/Bosque/Prefaps/Candle.prefab";
        GameObject groundCandlePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(candlePrefabPath);
        if (groundCandlePrefab != null)
        {
            // Ground Candle 1: Base of the Gallows (Ahorcado)
            GameObject gc1 = (GameObject)PrefabUtility.InstantiatePrefab(groundCandlePrefab);
            gc1.name = "Ground_Candle_Gallows";
            gc1.transform.parent = root.transform;
            float gc1X = 4.8f;
            float gc1Z = 19.5f;
            float gc1Y = GetTerrainHeight(gc1X, gc1Z, terrainComp) - 0.02f;
            gc1.transform.localPosition = new Vector3(gc1X, gc1Y, gc1Z);
            gc1.transform.localEulerAngles = Vector3.zero;
            gc1.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);

            Light gc1Light = gc1.GetComponentInChildren<Light>();
            if (gc1Light == null)
            {
                GameObject gc1LightGo = new GameObject("Candle_Light");
                gc1LightGo.transform.parent = gc1.transform;
                gc1LightGo.transform.localPosition = new Vector3(0f, 0.2f, 0f);
                gc1Light = gc1LightGo.AddComponent<Light>();
                gc1Light.type = LightType.Point;
                gc1LightGo.AddComponent<UniversalAdditionalLightData>();
            }
            gc1Light.color = new Color(0.98f, 0.45f, 0.08f);
            gc1Light.intensity = 3.5f;
            gc1Light.range = 5.0f;
            gc1Light.shadows = LightShadows.Soft;

            var gc1Flicker = gc1Light.gameObject.GetComponent<MainMenuLightFlicker>();
            if (gc1Flicker == null) gc1Flicker = gc1Light.gameObject.AddComponent<MainMenuLightFlicker>();
            gc1Flicker.flickerSpeed = 0.22f;
            gc1Flicker.minIntensityMultiplier = 0.65f;
            gc1Flicker.maxIntensityMultiplier = 1.35f;

            // Ground Candle 2: Base of the Shrine (Santuario)
            GameObject gc2 = (GameObject)PrefabUtility.InstantiatePrefab(groundCandlePrefab);
            gc2.name = "Ground_Candle_Shrine";
            gc2.transform.parent = root.transform;
            float gc2X = -3.4f;
            float gc2Z = -2.2f;
            float gc2Y = GetTerrainHeight(gc2X, gc2Z, terrainComp) - 0.02f;
            gc2.transform.localPosition = new Vector3(gc2X, gc2Y, gc2Z);
            gc2.transform.localEulerAngles = Vector3.zero;
            gc2.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);

            Light gc2Light = gc2.GetComponentInChildren<Light>();
            if (gc2Light == null)
            {
                GameObject gc2LightGo = new GameObject("Candle_Light");
                gc2LightGo.transform.parent = gc2.transform;
                gc2LightGo.transform.localPosition = new Vector3(0f, 0.2f, 0f);
                gc2Light = gc2LightGo.AddComponent<Light>();
                gc2Light.type = LightType.Point;
                gc2LightGo.AddComponent<UniversalAdditionalLightData>();
            }
            gc2Light.color = new Color(0.98f, 0.45f, 0.08f);
            gc2Light.intensity = 3.5f;
            gc2Light.range = 5.0f;
            gc2Light.shadows = LightShadows.Soft;

            var gc2Flicker = gc2Light.gameObject.GetComponent<MainMenuLightFlicker>();
            if (gc2Flicker == null) gc2Flicker = gc2Light.gameObject.AddComponent<MainMenuLightFlicker>();
            gc2Flicker.flickerSpeed = 0.22f;
            gc2Flicker.minIntensityMultiplier = 0.65f;
            gc2Flicker.maxIntensityMultiplier = 1.35f;
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

    [MenuItem("Antigravity/Fix Black Screen UI")]
    public static void FixBlackScreenUI()
    {
        GameObject mainMenuGo = GameObject.Find("MAINMENU");
        if (mainMenuGo != null)
        {
            Transform canvasTrans = mainMenuGo.transform.Find("Canvas");
            if (canvasTrans != null)
            {
                Transform bgTrans = canvasTrans.Find("Background");
                if (bgTrans != null)
                {
                    var bgImage = bgTrans.GetComponent<UnityEngine.UI.Image>();
                    if (bgImage != null)
                    {
                        Undo.RecordObject(bgImage, "Disable solid black background image");
                        bgImage.enabled = false;
                        Debug.Log("UI Background Image disabled successfully!");
                    }
                }
            }
        }
        SceneView.RepaintAll();
    }

    [MenuItem("Antigravity/Add AAA Detailing In-Place")]
    public static void AddAAADetailingInPlace()
    {
        GameObject root = GameObject.Find("= BACKGROUND");
        if (root == null)
        {
            Debug.LogError("= BACKGROUND root not found in scene!");
            return;
        }

        Terrain terrainComp = root.GetComponentInChildren<Terrain>();
        if (terrainComp == null)
        {
            Debug.LogError("Terrain component not found in background!");
            return;
        }

        // 1. Fallen Logs
        string logPrefabPath = "Assets/ThunderWire Studio/UHFPS/_Demo/Environment/Nature/Log/Log.prefab";
        GameObject logPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(logPrefabPath);
        if (logPrefab != null)
        {
            if (GameObject.Find("Forest_Log_1") == null)
            {
                GameObject log1 = (GameObject)PrefabUtility.InstantiatePrefab(logPrefab);
                log1.name = "Forest_Log_1";
                log1.transform.parent = root.transform;
                float l1X = -4.8f;
                float l1Z = 3.5f;
                float l1Y = GetTerrainHeight(l1X, l1Z, terrainComp) - 0.15f;
                log1.transform.localPosition = new Vector3(l1X, l1Y, l1Z);
                log1.transform.localEulerAngles = new Vector3(8f, 112f, -5f);
                log1.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
                Undo.RegisterCreatedObjectUndo(log1, "Add Log 1");
            }

            if (GameObject.Find("Forest_Log_2") == null)
            {
                GameObject log2 = (GameObject)PrefabUtility.InstantiatePrefab(logPrefab);
                log2.name = "Forest_Log_2";
                log2.transform.parent = root.transform;
                float l2X = 4.2f;
                float l2Z = -3.2f;
                float l2Y = GetTerrainHeight(l2X, l2Z, terrainComp) - 0.2f;
                log2.transform.localPosition = new Vector3(l2X, l2Y, l2Z);
                log2.transform.localEulerAngles = new Vector3(-5f, 25f, 12f);
                log2.transform.localScale = new Vector3(1.2f, 1.2f, 1.5f);
                Undo.RegisterCreatedObjectUndo(log2, "Add Log 2");
            }
        }

        // 2. Mossy Rocks
        string rockPrefabPath = "Assets/ThunderWire Studio/UHFPS/_Demo/Environment/Nature/Rock/RockB.prefab";
        GameObject rockPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(rockPrefabPath);
        if (rockPrefab != null)
        {
            if (GameObject.Find("Forest_Rock_1") == null)
            {
                GameObject rock1 = (GameObject)PrefabUtility.InstantiatePrefab(rockPrefab);
                rock1.name = "Forest_Rock_1";
                rock1.transform.parent = root.transform;
                float r1X = 3.8f;
                float r1Z = -8.5f;
                float r1Y = GetTerrainHeight(r1X, r1Z, terrainComp) - 0.3f;
                rock1.transform.localPosition = new Vector3(r1X, r1Y, r1Z);
                rock1.transform.localEulerAngles = new Vector3(12f, 45f, -8f);
                rock1.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
                Undo.RegisterCreatedObjectUndo(rock1, "Add Rock 1");
            }

            if (GameObject.Find("Forest_Rock_2") == null)
            {
                GameObject rock2 = (GameObject)PrefabUtility.InstantiatePrefab(rockPrefab);
                rock2.name = "Forest_Rock_2";
                rock2.transform.parent = root.transform;
                float r2X = -5.2f;
                float r2Z = -3.5f;
                float r2Y = GetTerrainHeight(r2X, r2Z, terrainComp) - 0.2f;
                rock2.transform.localPosition = new Vector3(r2X, r2Y, r2Z);
                rock2.transform.localEulerAngles = new Vector3(-15f, 190f, 10f);
                rock2.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
                Undo.RegisterCreatedObjectUndo(rock2, "Add Rock 2");
            }
        }

        // 3. Conifer Saplings
        string saplingPath = "Assets/Forst/Conifers [BOTD]/Render Pipeline Support/URP/Prefabs/PF Conifer Small BOTD URP.prefab";
        GameObject saplingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(saplingPath);
        if (saplingPrefab != null)
        {
            if (GameObject.Find("Forest_Sapling_0") == null)
            {
                Random.InitState(101);
                for (int i = 0; i < 16; i++)
                {
                    float z = Random.Range(-15f, 45f);
                    float x = Random.Range(0, 2) == 0 ? Random.Range(-18f, -7f) : Random.Range(7f, 18f);
                    GameObject sapling = (GameObject)PrefabUtility.InstantiatePrefab(saplingPrefab);
                    sapling.name = "Forest_Sapling_" + i;
                    sapling.transform.parent = root.transform;
                    float worldY = GetTerrainHeight(x, z, terrainComp) - 0.15f;
                    sapling.transform.localPosition = new Vector3(x, worldY, z);
                    sapling.transform.localEulerAngles = new Vector3(Random.Range(-3f, 3f), Random.Range(0f, 360f), Random.Range(-3f, 3f));
                    float scale = Random.Range(0.4f, 0.8f);
                    sapling.transform.localScale = new Vector3(scale, scale, scale);
                    Undo.RegisterCreatedObjectUndo(sapling, "Add Sapling " + i);
                }
            }
        }

        // 4. Ground Foliage
        GameObject foliageGroup = GameObject.Find("Ground_Foliage");
        if (foliageGroup == null)
        {
            foliageGroup = new GameObject("Ground_Foliage");
            foliageGroup.transform.parent = root.transform;
            Undo.RegisterCreatedObjectUndo(foliageGroup, "Create Ground Foliage Group");

            string fernAPath = "Assets/Samples/Shader Graph/17.3.0/Production Ready Shaders/Environment/Details/Ferns/Fern_A.prefab";
            string fernBPath = "Assets/Samples/Shader Graph/17.3.0/Production Ready Shaders/Environment/Details/Ferns/Fern_B.prefab";
            GameObject fernAPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(fernAPath);
            GameObject fernBPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(fernBPath);
            string bushPath = "Assets/TerrainSampleAssets/Prefabs/BushDry_A.prefab";
            GameObject bushPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(bushPath);

            if (fernAPrefab != null && fernBPrefab != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    GameObject fern = (GameObject)PrefabUtility.InstantiatePrefab(i % 2 == 0 ? fernAPrefab : fernBPrefab);
                    fern.name = "Fern_Log1_" + i;
                    fern.transform.parent = foliageGroup.transform;
                    float fx = -4.8f + Random.Range(-0.8f, 0.8f);
                    float fz = 3.5f + Random.Range(-1.2f, 1.2f);
                    float fy = GetTerrainHeight(fx, fz, terrainComp) - 0.05f;
                    fern.transform.localPosition = new Vector3(fx, fy, fz);
                    fern.transform.localEulerAngles = new Vector3(0f, Random.Range(0f, 360f), 0f);
                    float scale = Random.Range(0.8f, 1.3f);
                    fern.transform.localScale = new Vector3(scale, scale, scale);
                }

                for (int i = 0; i < 4; i++)
                {
                    GameObject fern = (GameObject)PrefabUtility.InstantiatePrefab(i % 2 == 0 ? fernAPrefab : fernBPrefab);
                    fern.name = "Fern_Log2_" + i;
                    fern.transform.parent = foliageGroup.transform;
                    float fx = 4.2f + Random.Range(-0.8f, 0.8f);
                    float fz = -3.2f + Random.Range(-1.2f, 1.2f);
                    float fy = GetTerrainHeight(fx, fz, terrainComp) - 0.05f;
                    fern.transform.localPosition = new Vector3(fx, fy, fz);
                    fern.transform.localEulerAngles = new Vector3(0f, Random.Range(0f, 360f), 0f);
                    float scale = Random.Range(0.8f, 1.3f);
                    fern.transform.localScale = new Vector3(scale, scale, scale);
                }

                for (int i = 0; i < 3; i++)
                {
                    GameObject fern = (GameObject)PrefabUtility.InstantiatePrefab(fernAPrefab);
                    fern.name = "Fern_Rock1_" + i;
                    fern.transform.parent = foliageGroup.transform;
                    float fx = 3.8f + Random.Range(-0.6f, 0.6f);
                    float fz = -8.5f + Random.Range(-0.6f, 0.6f);
                    float fy = GetTerrainHeight(fx, fz, terrainComp) - 0.05f;
                    fern.transform.localPosition = new Vector3(fx, fy, fz);
                    fern.transform.localEulerAngles = new Vector3(0f, Random.Range(0f, 360f), 0f);
                    float scale = Random.Range(0.9f, 1.2f);
                    fern.transform.localScale = new Vector3(scale, scale, scale);
                }
            }

            if (bushPrefab != null)
            {
                for (int i = 0; i < 8; i++)
                {
                    GameObject bush = (GameObject)PrefabUtility.InstantiatePrefab(bushPrefab);
                    bush.name = "Dry_Bush_" + i;
                    bush.transform.parent = foliageGroup.transform;
                    float bx = i % 2 == 0 ? Random.Range(-14f, -6f) : Random.Range(6f, 14f);
                    float bz = Random.Range(-10f, 30f);
                    float by = GetTerrainHeight(bx, bz, terrainComp) - 0.1f;
                    bush.transform.localPosition = new Vector3(bx, by, bz);
                    bush.transform.localEulerAngles = new Vector3(0f, Random.Range(0f, 360f), 0f);
                    float scale = Random.Range(1.2f, 1.8f);
                    bush.transform.localScale = new Vector3(scale, scale, scale);
                }
            }

            // 5. Grass
            string grassPath = "Assets/ThunderWire Studio/UHFPS/_Demo/Environment/Nature/GrassB/GrassB.prefab";
            GameObject grassPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(grassPath);
            if (grassPrefab != null)
            {
                float zStart = -15f;
                float zEnd = 28f;
                float step = 4.3f;
                float leftX = -4.0f;
                float rightX = 4.0f;

                for (float zVal = zStart; zVal <= zEnd; zVal += step)
                {
                    if (zVal < -3f || zVal > 2f)
                    {
                        for (int j = 0; j < 2; j++)
                        {
                            GameObject grass = (GameObject)PrefabUtility.InstantiatePrefab(grassPrefab);
                            grass.name = "Grass_L_" + zVal + "_" + j;
                            grass.transform.parent = foliageGroup.transform;
                            float gx = leftX + Random.Range(-0.3f, 0.3f);
                            float gz = zVal + Random.Range(-0.5f, 0.5f);
                            float gy = GetTerrainHeight(gx, gz, terrainComp) - 0.05f;
                            grass.transform.localPosition = new Vector3(gx, gy, gz);
                            grass.transform.localEulerAngles = new Vector3(0f, Random.Range(0f, 360f), 0f);
                            float scale = Random.Range(1.3f, 1.7f);
                            grass.transform.localScale = new Vector3(scale, scale, scale);
                        }
                    }

                    for (int j = 0; j < 2; j++)
                    {
                        GameObject grass = (GameObject)PrefabUtility.InstantiatePrefab(grassPrefab);
                        grass.name = "Grass_R_" + zVal + "_" + j;
                        grass.transform.parent = foliageGroup.transform;
                        float gx = rightX + Random.Range(-0.3f, 0.3f);
                        float gz = zVal + Random.Range(-0.5f, 0.5f);
                        float gy = GetTerrainHeight(gx, gz, terrainComp) - 0.05f;
                        grass.transform.localPosition = new Vector3(gx, gy, gz);
                        grass.transform.localEulerAngles = new Vector3(0f, Random.Range(0f, 360f), 0f);
                        float scale = Random.Range(1.3f, 1.7f);
                        grass.transform.localScale = new Vector3(scale, scale, scale);
                    }
                }
            }
        }

        // 6. Ground Ritual Candles
        string candlePrefabPath = "Assets/AssetsDescargados/Bosque/Prefaps/Candle.prefab";
        GameObject groundCandlePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(candlePrefabPath);
        if (groundCandlePrefab != null)
        {
            if (GameObject.Find("Ground_Candle_Gallows") == null)
            {
                GameObject gc1 = (GameObject)PrefabUtility.InstantiatePrefab(groundCandlePrefab);
                gc1.name = "Ground_Candle_Gallows";
                gc1.transform.parent = root.transform;
                float gc1X = 4.8f;
                float gc1Z = 19.5f;
                float gc1Y = GetTerrainHeight(gc1X, gc1Z, terrainComp) - 0.02f;
                gc1.transform.localPosition = new Vector3(gc1X, gc1Y, gc1Z);
                gc1.transform.localEulerAngles = Vector3.zero;
                gc1.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);

                Light gc1Light = gc1.GetComponentInChildren<Light>();
                if (gc1Light == null)
                {
                    GameObject gc1LightGo = new GameObject("Candle_Light");
                    gc1LightGo.transform.parent = gc1.transform;
                    gc1LightGo.transform.localPosition = new Vector3(0f, 0.2f, 0f);
                    gc1Light = gc1LightGo.AddComponent<Light>();
                    gc1Light.type = LightType.Point;
                    gc1LightGo.AddComponent<UniversalAdditionalLightData>();
                }
                gc1Light.color = new Color(0.98f, 0.45f, 0.08f);
                gc1Light.intensity = 3.5f;
                gc1Light.range = 5.0f;
                gc1Light.shadows = LightShadows.Soft;

                var gc1Flicker = gc1Light.gameObject.GetComponent<MainMenuLightFlicker>();
                if (gc1Flicker == null) gc1Flicker = gc1Light.gameObject.AddComponent<MainMenuLightFlicker>();
                gc1Flicker.flickerSpeed = 0.22f;
                gc1Flicker.minIntensityMultiplier = 0.65f;
                gc1Flicker.maxIntensityMultiplier = 1.35f;
                Undo.RegisterCreatedObjectUndo(gc1, "Add Ground Candle 1");
            }

            if (GameObject.Find("Ground_Candle_Shrine") == null)
            {
                GameObject gc2 = (GameObject)PrefabUtility.InstantiatePrefab(groundCandlePrefab);
                gc2.name = "Ground_Candle_Shrine";
                gc2.transform.parent = root.transform;
                float gc2X = -3.4f;
                float gc2Z = -2.2f;
                float gc2Y = GetTerrainHeight(gc2X, gc2Z, terrainComp) - 0.02f;
                gc2.transform.localPosition = new Vector3(gc2X, gc2Y, gc2Z);
                gc2.transform.localEulerAngles = Vector3.zero;
                gc2.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);

                Light gc2Light = gc2.GetComponentInChildren<Light>();
                if (gc2Light == null)
                {
                    GameObject gc2LightGo = new GameObject("Candle_Light");
                    gc2LightGo.transform.parent = gc2.transform;
                    gc2LightGo.transform.localPosition = new Vector3(0f, 0.2f, 0f);
                    gc2Light = gc2LightGo.AddComponent<Light>();
                    gc2Light.type = LightType.Point;
                    gc2LightGo.AddComponent<UniversalAdditionalLightData>();
                }
                gc2Light.color = new Color(0.98f, 0.45f, 0.08f);
                gc2Light.intensity = 3.5f;
                gc2Light.range = 5.0f;
                gc2Light.shadows = LightShadows.Soft;

                var gc2Flicker = gc2Light.gameObject.GetComponent<MainMenuLightFlicker>();
                if (gc2Flicker == null) gc2Flicker = gc2Light.gameObject.AddComponent<MainMenuLightFlicker>();
                gc2Flicker.flickerSpeed = 0.22f;
                gc2Flicker.minIntensityMultiplier = 0.65f;
                gc2Flicker.maxIntensityMultiplier = 1.35f;
                Undo.RegisterCreatedObjectUndo(gc2, "Add Ground Candle 2");
            }
        }

        SceneView.RepaintAll();
        Debug.Log("AAA Detailing added successfully in-place without overwriting existing assets!");
    }

    [MenuItem("Antigravity/Add Weather In-Place")]
    public static void AddWeatherInPlace()
    {
        string thunderManagerPath = "Assets/AssetsDescargados/AdvancedMobileHorror/Prefabs/Managers/ThunderManager.prefab";
        GameObject thunderPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(thunderManagerPath);
        if (thunderPrefab == null)
        {
            Debug.LogError("ThunderManager prefab not found at: " + thunderManagerPath);
            return;
        }

        // Find existing ThunderManager in the scene
        var existingManager = Object.FindAnyObjectByType<AdvancedHorrorFPS.ThunderManager>();
        if (existingManager != null)
        {
            Debug.Log("ThunderManager already exists in the scene.");
            // Make sure MainMenuThunderShadows is attached
            var shadowsComp = existingManager.gameObject.GetComponent<MainMenuThunderShadows>();
            if (shadowsComp == null)
            {
                shadowsComp = Undo.AddComponent<MainMenuThunderShadows>(existingManager.gameObject);
                shadowsComp.maxAngleDriftY = 10f;
                shadowsComp.maxAngleDriftX = 5f;
                shadowsComp.speedX = 28f;
                shadowsComp.speedY = 38f;
                Debug.Log("Attached MainMenuThunderShadows to existing ThunderManager.");
            }
            return;
        }

        GameObject root = GameObject.Find("= BACKGROUND");
        Transform parentTrans = root != null ? root.transform : null;

        GameObject thunderGo = (GameObject)PrefabUtility.InstantiatePrefab(thunderPrefab);
        thunderGo.name = "ThunderManager";
        thunderGo.transform.parent = parentTrans;
        thunderGo.transform.localPosition = Vector3.zero;
        thunderGo.transform.localRotation = Quaternion.identity;

        // Attach MainMenuThunderShadows
        var shadows = thunderGo.GetComponent<MainMenuThunderShadows>();
        if (shadows == null)
        {
            shadows = Undo.AddComponent<MainMenuThunderShadows>(thunderGo);
        }
        shadows.maxAngleDriftY = 10f;
        shadows.maxAngleDriftX = 5f;
        shadows.speedX = 28f;
        shadows.speedY = 38f;

        Undo.RegisterCreatedObjectUndo(thunderGo, "Add ThunderManager");

        SceneView.RepaintAll();
        Debug.Log("ThunderManager (with rain and thunder) added successfully in-place!");
    }

    [MenuItem("Antigravity/Add Spooky Details In-Place")]
    public static void AddSpookyDetailsInPlace()
    {
        GameObject root = GameObject.Find("= BACKGROUND");
        if (root == null)
        {
            Debug.LogError("= BACKGROUND root not found in scene!");
            return;
        }

        // 1. Streetlight Moths Particle System
        GameObject pole = GameObject.Find("Creepy_LightPole");
        if (pole != null)
        {
            Transform existingMoths = pole.transform.Find("StreetLight_Moths");
            if (existingMoths == null)
            {
                GameObject mothsGo = new GameObject("StreetLight_Moths");
                mothsGo.transform.parent = pole.transform;
                // Position it at the bulb
                mothsGo.transform.localPosition = new Vector3(0f, 4.5f, -1.0f);
                mothsGo.transform.localRotation = Quaternion.identity;

                ParticleSystem ps = mothsGo.AddComponent<ParticleSystem>();
                
                // Configure Main module
                var main = ps.main;
                main.startLifetime = 1.2f;
                main.startSize = 0.04f;
                main.startSpeed = 0.3f;
                main.maxParticles = 15;
                main.simulationSpace = ParticleSystemSimulationSpace.Local;
                main.loop = true;
                main.playOnAwake = true;

                // Configure Emission module
                var emission = ps.emission;
                emission.rateOverTime = 6f;

                // Configure Shape module
                var shape = ps.shape;
                shape.shapeType = ParticleSystemShapeType.Sphere;
                shape.radius = 0.4f;

                // Configure Noise module for fluttering motion
                var noise = ps.noise;
                noise.enabled = true;
                noise.strength = 0.8f;
                noise.frequency = 2.0f;
                noise.scrollSpeed = 1.0f;
                noise.damping = true;

                // Configure Color over Lifetime
                var colorOverLifetime = ps.colorOverLifetime;
                colorOverLifetime.enabled = true;
                Gradient grad = new Gradient();
                grad.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(new Color(0.8f, 0.7f, 0.4f), 0.0f), new GradientColorKey(new Color(0.8f, 0.7f, 0.4f), 1.0f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(0.0f, 0.0f), new GradientAlphaKey(0.8f, 0.2f), new GradientAlphaKey(0.8f, 0.8f), new GradientAlphaKey(0.0f, 1.0f) }
                );
                colorOverLifetime.color = new ParticleSystem.MinMaxGradient(grad);

                // Apply material
                var psRenderer = mothsGo.GetComponent<ParticleSystemRenderer>();
                if (psRenderer != null)
                {
                    Material defaultMat = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Particle.mat");
                    if (defaultMat != null)
                    {
                        psRenderer.material = defaultMat;
                    }
                }

                Undo.RegisterCreatedObjectUndo(mothsGo, "Add Streetlight Moths");
                Debug.Log("Streetlight Moths Particle System added successfully.");
            }
            else
            {
                Debug.Log("StreetLight_Moths already exists.");
            }
        }
        else
        {
            Debug.LogWarning("Creepy_LightPole not found in scene. Cannot add moths.");
        }

        // 2. Matadero Activity Light
        GameObject matadero = GameObject.Find("MataderoViejo");
        if (matadero != null)
        {
            GameObject activityLightGo = GameObject.Find("Matadero_Activity_Light");
            if (activityLightGo == null)
            {
                activityLightGo = new GameObject("Matadero_Activity_Light");
                activityLightGo.transform.parent = root.transform;
                activityLightGo.transform.position = matadero.transform.position + new Vector3(0f, 3.0f, 1.5f);
                activityLightGo.transform.localRotation = Quaternion.identity;

                Light actLight = activityLightGo.AddComponent<Light>();
                actLight.type = LightType.Point;
                actLight.color = new Color(0.3f, 0.65f, 0.95f); // Cyan/cool blue
                actLight.intensity = 4.0f;
                actLight.range = 15f;
                actLight.shadows = LightShadows.Soft;

                activityLightGo.AddComponent<UniversalAdditionalLightData>();

                var activityScript = activityLightGo.AddComponent<MainMenuMataderoActivity>();
                activityScript.baseIntensity = 4.0f;
                activityScript.moveSpeed = 0.4f;
                activityScript.moveRange = new Vector3(8f, 1f, 3f);

                Undo.RegisterCreatedObjectUndo(activityLightGo, "Add Matadero Activity Light");
                Debug.Log("Matadero Activity Light added successfully.");
            }
            else
            {
                Debug.Log("Matadero_Activity_Light already exists.");
            }
        }
        else
        {
            Debug.LogWarning("MataderoViejo not found in scene. Cannot add activity light.");
        }

        SceneView.RepaintAll();
    }

    [MenuItem("Antigravity/Add Interactive Horror In-Place")]
    public static void AddInteractiveHorrorInPlace()
    {
        GameObject root = GameObject.Find("= BACKGROUND");
        if (root == null)
        {
            Debug.LogError("= BACKGROUND root not found in scene!");
            return;
        }

        GameObject managerGo = GameObject.Find("Horror_Events_Manager");
        if (managerGo == null)
        {
            managerGo = new GameObject("Horror_Events_Manager");
            managerGo.transform.parent = root.transform;
            managerGo.transform.localPosition = Vector3.zero;
            managerGo.transform.localRotation = Quaternion.identity;

            MainMenuHorrorEvents ev = managerGo.AddComponent<MainMenuHorrorEvents>();

            // Load resources
            AudioClip flickerSFX = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/AssetsDescargados/AdvancedMobileHorror/Sounds/Audio_ButtonClick.wav");
            AudioClip whisperSFX = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/Sintomas/whisper.mp3");

            ev.flickerSound = flickerSFX;
            ev.whisperSound = whisperSFX;

            Undo.RegisterCreatedObjectUndo(managerGo, "Add Horror Events Manager");
            Debug.Log("Horror Events Manager added successfully.");
        }
        else
        {
            Debug.Log("Horror_Events_Manager already exists.");
        }

        SceneView.RepaintAll();
    }

    [MenuItem("Antigravity/Add Cinematic Post-Processing In-Place")]
    public static void AddCinematicPostProcessingInPlace()
    {
        string volumeProfilePath = "Assets/Scenes/MainMenuProfile.asset";
        VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(volumeProfilePath);
        if (profile == null)
        {
            Debug.LogError("MainMenuProfile profile asset not found at path: " + volumeProfilePath);
            return;
        }

        Undo.RecordObject(profile, "Configure Cinematic PS5 Post-Processing");

        // 1. ACES Tonemapping
        Tonemapping tonemapping;
        if (!profile.TryGet(out tonemapping)) tonemapping = profile.Add<Tonemapping>(true);
        tonemapping.active = true;
        tonemapping.mode.overrideState = true;
        tonemapping.mode.value = TonemappingMode.ACES;

        // 2. Gaussian Depth of Field (Very optimized cinematic blur)
        DepthOfField dof;
        if (!profile.TryGet(out dof)) dof = profile.Add<DepthOfField>(true);
        dof.active = true;
        dof.mode.overrideState = true;
        dof.mode.value = DepthOfFieldMode.Gaussian;
        dof.gaussianStart.overrideState = true;
        dof.gaussianStart.value = 2.0f; // Start blurring very close to camera
        dof.gaussianEnd.overrideState = true;
        dof.gaussianEnd.value = 55.0f; // End blurring past Matadero (blur deep background forest)
        dof.gaussianMaxRadius.overrideState = true;
        dof.gaussianMaxRadius.value = 1.5f; // Clamped float for max radius (usually 0.5 to 1.5)

        // 3. Color Adjustments (Cinematic Color Grading)
        ColorAdjustments colorAdjust;
        if (!profile.TryGet(out colorAdjust)) colorAdjust = profile.Add<ColorAdjustments>(true);
        colorAdjust.active = true;
        colorAdjust.postExposure.overrideState = true;
        colorAdjust.postExposure.value = 0.15f; // Balance out ACES slight darkness
        colorAdjust.contrast.overrideState = true;
        colorAdjust.contrast.value = 18f;
        colorAdjust.saturation.overrideState = true;
        colorAdjust.saturation.value = -15f; // Desaturate slightly for horror realism
        colorAdjust.colorFilter.overrideState = true;
        colorAdjust.colorFilter.value = new Color(0.92f, 0.95f, 1.0f); // Cool blue-tinted filter

        // 4. Split Toning / Shadows Midtones Highlights
        ShadowsMidtonesHighlights smh;
        if (!profile.TryGet(out smh)) smh = profile.Add<ShadowsMidtonesHighlights>(true);
        smh.active = true;
        smh.shadows.overrideState = true;
        smh.shadows.value = new Vector4(0.9f, 0.92f, 1.0f, 0f); // Cold blue shadows
        smh.midtones.overrideState = true;
        smh.midtones.value = new Vector4(1.0f, 0.98f, 0.95f, 0f);

        EditorUtility.SetDirty(profile);
        AssetDatabase.SaveAssets();

        SceneView.RepaintAll();
        Debug.Log("Cinematic PS5 Post-Processing profile configured successfully (Highly Optimized).");
    }

    [MenuItem("Antigravity/Configure Fog In-Place")]
    public static void ConfigureFogInPlace()
    {
        GameObject root = GameObject.Find("= BACKGROUND");
        if (root == null)
        {
            Debug.LogError("= BACKGROUND root not found in scene!");
            return;
        }

        // 1. Reduce Global Fog Density to almost zero so the middle road is completely clear
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = 0.003f; // Reveals the Matadero perfectly clear

        // 2. Configure ThunderManager children states
        GameObject thunderGo = GameObject.Find("ThunderManager");
        if (thunderGo != null)
        {
            // Force active the rain system
            Transform centralRain = thunderGo.transform.Find("Rain_Particle");
            if (centralRain != null)
            {
                Undo.RegisterCompleteObjectUndo(centralRain.gameObject, "Enable Rain Particle");
                centralRain.gameObject.SetActive(true);
                Debug.Log("Enabled Rain_Particle under ThunderManager.");
            }

            // Force inactive the central fog system
            Transform centralFog = thunderGo.transform.Find("Fog_Particle");
            if (centralFog != null)
            {
                Undo.RegisterCompleteObjectUndo(centralFog.gameObject, "Disable Central Fog");
                centralFog.gameObject.SetActive(false);
                Debug.Log("Disabled default central Fog_Particle under ThunderManager.");
            }
        }

        // 3. Load Fog_Particle prefab to instantiate on left and right
        string fogPrefabPath = "Assets/AssetsDescargados/AdvancedMobileHorror/Prefabs/Others/Fog_Particle.prefab";
        GameObject fogPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(fogPrefabPath);
        if (fogPrefab == null)
        {
            Debug.LogError("Fog_Particle prefab not found at: " + fogPrefabPath);
            return;
        }

        // 4. Instantiate Left Fog (force active!)
        GameObject oldLeftFog = GameObject.Find("Left_Fog_Particles");
        if (oldLeftFog != null) Undo.DestroyObjectImmediate(oldLeftFog);

        GameObject leftFog = (GameObject)PrefabUtility.InstantiatePrefab(fogPrefab);
        leftFog.name = "Left_Fog_Particles";
        leftFog.transform.parent = root.transform;
        leftFog.transform.localPosition = new Vector3(-12f, 0f, 15f);
        leftFog.transform.localRotation = Quaternion.identity;
        leftFog.SetActive(true); // Ensure it is active!
        Undo.RegisterCreatedObjectUndo(leftFog, "Add Left Fog Particles");

        // 5. Instantiate Right Fog (force active!)
        GameObject oldRightFog = GameObject.Find("Right_Fog_Particles");
        if (oldRightFog != null) Undo.DestroyObjectImmediate(oldRightFog);

        GameObject rightFog = (GameObject)PrefabUtility.InstantiatePrefab(fogPrefab);
        rightFog.name = "Right_Fog_Particles";
        rightFog.transform.parent = root.transform;
        rightFog.transform.localPosition = new Vector3(12f, 0f, 15f);
        rightFog.transform.localRotation = Quaternion.identity;
        rightFog.SetActive(true); // Ensure it is active!
        Undo.RegisterCreatedObjectUndo(rightFog, "Add Right Fog Particles");

        SceneView.RepaintAll();
        Debug.Log("Fog configured successfully: Global density reduced, side particles activated.");
    }

    [MenuItem("Antigravity/Add Cinematic AAA Details In-Place")]
    public static void AddCinematicAAADetails()
    {
        // 1. Configure Gallows and Lantern Sway
        GameObject gallows = GameObject.Find("Creepy_Gallows");
        if (gallows != null)
        {
            Transform lantern = gallows.transform.Find("Lantern");
            Transform hangman = gallows.transform.Find("SM_Hangman");
            Transform hangmanMasked = gallows.transform.Find("SM_Hangman_Masked");
            Transform flame = gallows.transform.Find("Flame");
            
            if (lantern != null)
            {
                // Parent Flame (light) to Lantern if not already
                if (flame != null && flame.parent != lantern)
                {
                    Undo.SetTransformParent(flame, lantern, "Parent Flame to Lantern");
                }

                // Add Sway to Lantern
                var lanternSway = lantern.gameObject.GetComponent<MainMenuSway>();
                if (lanternSway == null) lanternSway = lantern.gameObject.AddComponent<MainMenuSway>();
                Undo.RecordObject(lanternSway, "Configure Lantern Sway");
                lanternSway.speedX = 1.3f;
                lanternSway.speedZ = 1.0f;
                lanternSway.maxAngleX = 4.0f;
                lanternSway.maxAngleZ = 4.5f;
                EditorUtility.SetDirty(lanternSway);
            }

            if (hangman != null)
            {
                var hangmanSway = hangman.gameObject.GetComponent<MainMenuSway>();
                if (hangmanSway == null) hangmanSway = hangman.gameObject.AddComponent<MainMenuSway>();
                Undo.RecordObject(hangmanSway, "Configure Hangman Sway");
                hangmanSway.speedX = 0.8f;
                hangmanSway.speedZ = 0.6f;
                hangmanSway.maxAngleX = 2.0f;
                hangmanSway.maxAngleZ = 2.5f;
                EditorUtility.SetDirty(hangmanSway);
            }

            if (hangmanMasked != null)
            {
                var hangmanMaskedSway = hangmanMasked.gameObject.GetComponent<MainMenuSway>();
                if (hangmanMaskedSway == null) hangmanMaskedSway = hangmanMasked.gameObject.AddComponent<MainMenuSway>();
                Undo.RecordObject(hangmanMaskedSway, "Configure Hangman Masked Sway");
                hangmanMaskedSway.speedX = 0.82f;
                hangmanMaskedSway.speedZ = 0.61f;
                hangmanMaskedSway.maxAngleX = 2.0f;
                hangmanMaskedSway.maxAngleZ = 2.5f;
                EditorUtility.SetDirty(hangmanMaskedSway);
            }
            Debug.Log("Sway components configured on Gallows, Lantern and Hangman.");
        }
        else
        {
            Debug.LogWarning("Creepy_Gallows not found!");
        }

        // 2. Configure Candle Flicker on all Candle Point Lights
        var allLights = GameObject.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int candleCount = 0;
        foreach (var l in allLights)
        {
            if (l.name == "Candle Point Light" || l.name == "Point Light" && l.transform.parent != null && l.transform.parent.name.Contains("Candle"))
            {
                var flicker = l.gameObject.GetComponent<MainMenuCandleFlicker>();
                if (flicker == null) flicker = l.gameObject.AddComponent<MainMenuCandleFlicker>();
                Undo.RecordObject(flicker, "Configure Candle Flicker");
                flicker.minFlickerSpeed = 4f;
                flicker.maxFlickerSpeed = 8f;
                flicker.intensityRange = 0.12f;
                EditorUtility.SetDirty(flicker);
                candleCount++;
            }
        }
        Debug.Log($"Configured storm-reactive CandleFlicker on {candleCount} candle lights.");

        // 3. Configure Crow Reaction (DEACTIVATED based on user request - destroy if exists)
        GameObject crowObj = GameObject.Find("Crow_Sign/CilindroSuelo/Crow");
        if (crowObj != null)
        {
            var crowReaction = crowObj.GetComponent<MainMenuCrowReaction>();
            if (crowReaction != null)
            {
                Undo.DestroyObjectImmediate(crowReaction);
                Debug.Log("Removed MainMenuCrowReaction component from Crow.");
            }
        }
        else
        {
            Debug.LogWarning("Crow GameObject not found at Crow_Sign/CilindroSuelo/Crow!");
        }

        // 4. Configure UI Logo Glitch
        GameObject logoObj = GameObject.Find("MAINMENU/Canvas/Background/Blur/MainMenu/Logo");
        if (logoObj != null)
        {
            var logoGlitch = logoObj.GetComponent<MainMenuUIGlitch>();
            if (logoGlitch == null) logoGlitch = logoObj.AddComponent<MainMenuUIGlitch>();
            Undo.RecordObject(logoGlitch, "Configure Logo Glitch");
            logoGlitch.maxPositionOffset = 6f;
            logoGlitch.maxRotationOffset = 1.8f;
            logoGlitch.glitchChance = 0.35f;
            EditorUtility.SetDirty(logoGlitch);
            Debug.Log("Configured MainMenuUIGlitch on Logo.");
        }
        else
        {
            Debug.LogWarning("Logo GameObject not found at MAINMENU/Canvas/Background/Blur/MainMenu/Logo!");
        }

        // 5. Configure Creepy_NPC Animator, Blink and Spasm
        GameObject npcObj = GameObject.Find("Creepy_NPC");
        if (npcObj != null)
        {
            // Configure Animator to use Zombie.controller in-place
            var animator = npcObj.GetComponent<Animator>();
            if (animator == null) animator = npcObj.AddComponent<Animator>();
            string npcAnimPath = "Assets/ThunderWire Studio/UHFPS/Content/Animation/Zombie/Zombie.controller";
            var npcController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(npcAnimPath);
            if (npcController != null && animator.runtimeAnimatorController != npcController)
            {
                Undo.RecordObject(animator, "Update NPC Animator Controller");
                animator.runtimeAnimatorController = npcController;
            }

            // Add or get components
            var blinkNPC = npcObj.GetComponent<MainMenuBlinkNPC>();
            if (blinkNPC == null) blinkNPC = npcObj.AddComponent<MainMenuBlinkNPC>();
            Undo.RecordObject(blinkNPC, "Configure Blink NPC");

            // Explicitly set eye color to spectral white/blue and lower intensity
            blinkNPC.eyeColor = new Color(0.8f, 0.88f, 1.0f);
            blinkNPC.eyeIntensity = 2.0f;

            var spasm = npcObj.GetComponent<MainMenuNPCSpasm>();
            if (spasm == null) spasm = npcObj.AddComponent<MainMenuNPCSpasm>();
            Undo.RecordObject(spasm, "Configure NPC Spasm");

            var animController = npcObj.GetComponent<MainMenuNPCAnimationController>();
            if (animController == null) animController = npcObj.AddComponent<MainMenuNPCAnimationController>();
            Undo.RecordObject(animController, "Configure NPC Animation Controller");
            if (animController.animationSequence == null || animController.animationSequence.Length == 0)
            {
                animController.animationSequence = new MainMenuNPCAnimationController.AnimationStep[]
                {
                    new MainMenuNPCAnimationController.AnimationStep { stateName = "Walk", duration = 5f, crossfadeTime = 0.25f },
                    new MainMenuNPCAnimationController.AnimationStep { stateName = "Idle", duration = 5f, crossfadeTime = 0.25f }
                };
            }
            animController.loopSequence = true;
            animController.playOnStart = true;
            EditorUtility.SetDirty(animController);

            // Configure very dim, flickering body light parented to Spine2 bone (DEACTIVATED based on user request)
            Transform spine = FindChildRecursive(npcObj.transform, "mixamorig:Spine2");
            Transform bodyLightParent = spine != null ? spine : npcObj.transform;
            for (int i = bodyLightParent.childCount - 1; i >= 0; i--)
            {
                if (bodyLightParent.GetChild(i).name == "NPCBodyLight")
                {
                    Undo.DestroyObjectImmediate(bodyLightParent.GetChild(i).gameObject);
                }
            }

            blinkNPC.enableGlowEyes = false;
            blinkNPC.eyeColor = new Color(0.8f, 0.88f, 1.0f);
            blinkNPC.eyeIntensity = 0f; // Disable eye intensity as well to be completely safe

            // Setup Waypoints dynamically using GetTerrainHeight
            Terrain terrainComp = GameObject.Find("= BACKGROUND/Terrain")?.GetComponent<Terrain>();
            if (terrainComp != null)
            {
                var waypoints = new MainMenuBlinkNPC.BlinkPosition[6];
                
                // Waypoint 0: Far (Default)
                waypoints[0] = new MainMenuBlinkNPC.BlinkPosition {
                    position = new Vector3(0.6f, GetTerrainHeight(0.6f, 28.0f, terrainComp) - 0.05f, 28.0f),
                    rotation = new Vector3(0f, 180f, 0f),
                    isVisible = true
                };

                // Waypoint 1: Mid (In the road clearing, half distance, right side)
                waypoints[1] = new MainMenuBlinkNPC.BlinkPosition {
                    position = new Vector3(1.2f, GetTerrainHeight(1.2f, 14.0f, terrainComp) - 0.05f, 14.0f),
                    rotation = new Vector3(0f, 190f, 0f),
                    isVisible = true
                };

                // Waypoint 2: Close (Right side next to fence/trees - Options)
                waypoints[2] = new MainMenuBlinkNPC.BlinkPosition {
                    position = new Vector3(2.8f, GetTerrainHeight(2.8f, 3.0f, terrainComp) - 0.05f, 3.0f),
                    rotation = new Vector3(0f, 210f, 0f),
                    isVisible = true
                };

                // Waypoint 3: Extreme Close (Foreground right near swing - Quit)
                waypoints[3] = new MainMenuBlinkNPC.BlinkPosition {
                    position = new Vector3(1.9f, GetTerrainHeight(1.9f, -5.5f, terrainComp) - 0.05f, -5.5f),
                    rotation = new Vector3(0f, 225f, 0f),
                    isVisible = true
                };

                // Waypoint 4: Intermediate Close (Right side further back - LoadGame)
                waypoints[4] = new MainMenuBlinkNPC.BlinkPosition {
                    position = new Vector3(2.1f, GetTerrainHeight(2.1f, 8.0f, terrainComp) - 0.05f, 8.0f),
                    rotation = new Vector3(0f, 200f, 0f),
                    isVisible = true
                };

                // Waypoint 5: Vanished (Invisible)
                waypoints[5] = new MainMenuBlinkNPC.BlinkPosition {
                    position = new Vector3(0.6f, GetTerrainHeight(0.6f, 28.0f, terrainComp) - 0.05f, 28.0f),
                    rotation = new Vector3(0f, 180f, 0f),
                    isVisible = false
                };

                blinkNPC.blinkPositions = waypoints;
            }
            
            // Adjust NPC look speed to be creepier/slower
            var creepyIdle = npcObj.GetComponent<MainMenuCreepyIdle>();
            if (creepyIdle != null)
            {
                Undo.RecordObject(creepyIdle, "Configure NPC Look Speed");
                creepyIdle.lookSpeed = 1.8f;
                EditorUtility.SetDirty(creepyIdle);
            }

            EditorUtility.SetDirty(blinkNPC);
            EditorUtility.SetDirty(spasm);
            Debug.Log("Configured BlinkNPC and NPCSpasm on Creepy_NPC.");
        }
        else
        {
            Debug.LogWarning("Creepy_NPC not found!");
        }

        // 6. Configure Button NPC Targets
        GameObject canvasGo = GameObject.Find("MAINMENU/Canvas");
        if (canvasGo != null)
        {
            Transform buttonsParent = canvasGo.transform.Find("Background/Blur/MainMenu/MenuButtons");
            if (buttonsParent != null)
            {
                string[] bNames = { "Continue", "NewGame", "LoadGame", "Options", "Quit" };
                foreach (string bName in bNames)
                {
                    Transform t = buttonsParent.Find(bName);
                    if (t != null)
                    {
                        var fx = t.gameObject.GetComponent<MainMenuButtonEffects>();
                        if (fx != null)
                        {
                            Undo.RecordObject(fx, "Configure Hover NPC Index");
                            if (bName == "NewGame") fx.npcTargetBlinkIndex = 1;      // Mid road clearing (inside flashlight beam)
                            else if (bName == "Quit") fx.npcTargetBlinkIndex = 3;     // Extreme close
                            else if (bName == "Options") fx.npcTargetBlinkIndex = 2;  // Close, behind sign
                            else if (bName == "LoadGame") fx.npcTargetBlinkIndex = 4; // Intermediate Close
                            else fx.npcTargetBlinkIndex = -1;
                            EditorUtility.SetDirty(fx);
                        }
                    }
                }
                Debug.Log("Configured NPC hover target indices on Main Menu buttons.");
            }
        }

        // 7. Configure Road Silhouette Light (Rim light backlight) in-place
        GameObject backlightGo = GameObject.Find("= BACKGROUND/Road Silhouette Light") ?? GameObject.Find("Road Silhouette Light");
        if (backlightGo == null)
        {
            backlightGo = new GameObject("Road Silhouette Light");
            GameObject backgroundRoot = GameObject.Find("= BACKGROUND");
            if (backgroundRoot != null)
            {
                backlightGo.transform.parent = backgroundRoot.transform;
            }
            Undo.RegisterCreatedObjectUndo(backlightGo, "Create Road Silhouette Light");
        }
        else
        {
            Undo.RecordObject(backlightGo.transform, "Update Road Silhouette Light Transform");
        }

        Terrain activeTerrain = GameObject.Find("= BACKGROUND/Terrain")?.GetComponent<Terrain>();
        float finalBacklightY = 5.0f;
        if (activeTerrain != null)
        {
            finalBacklightY = activeTerrain.SampleHeight(new Vector3(0f, 0f, 38f)) + activeTerrain.transform.position.y + 5.0f;
        }
        backlightGo.transform.position = new Vector3(0f, finalBacklightY, 38f);
        backlightGo.transform.localEulerAngles = new Vector3(15f, 180f, 0f);

        Light activeBacklight = backlightGo.GetComponent<Light>();
        if (activeBacklight == null)
        {
            activeBacklight = backlightGo.AddComponent<Light>();
        }
        Undo.RecordObject(activeBacklight, "Configure Road Silhouette Light");
        activeBacklight.type = LightType.Spot;
        activeBacklight.color = new Color(0.18f, 0.28f, 0.42f);
        activeBacklight.intensity = 8.5f;
        activeBacklight.range = 45f;
        activeBacklight.spotAngle = 65f;
        activeBacklight.innerSpotAngle = 25f;
        activeBacklight.shadows = LightShadows.None;
        EditorUtility.SetDirty(activeBacklight);

        var activeBacklightData = backlightGo.GetComponent<UniversalAdditionalLightData>();
        if (activeBacklightData == null)
        {
            activeBacklightData = backlightGo.AddComponent<UniversalAdditionalLightData>();
        }
        EditorUtility.SetDirty(activeBacklightData);
        Debug.Log("Configured Road Silhouette Light in-place.");

        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log("Cinematic AAA details configured and scene saved successfully!");
    }

    [MenuItem("Antigravity/Test NPC Waypoint 3 In-Place")]
    public static void TestNPCWaypoint3()
    {
        GameObject npcObj = GameObject.Find("Creepy_NPC");
        if (npcObj == null)
        {
            Debug.LogError("NPC not found!");
            return;
        }

        Terrain terrainComp = GameObject.Find("= BACKGROUND/Terrain")?.GetComponent<Terrain>();
        if (terrainComp == null) return;

        // Move to waypoint 3 (Extreme Close - Right side near swing)
        float height = terrainComp.SampleHeight(new Vector3(1.9f, 0f, -5.5f)) + activeTerrainPos(terrainComp) - 0.05f;
        npcObj.transform.position = new Vector3(1.9f, height, -5.5f);
        npcObj.transform.localEulerAngles = new Vector3(0f, 225f, 0f);

        // Find head
        Transform head = FindChildRecursive(npcObj.transform, "mixamorig:Head");
        if (head != null)
        {
            // Remove old eyes if any
            Transform oldL = head.Find("LeftEyeLight");
            if (oldL != null) Undo.DestroyObjectImmediate(oldL.gameObject);
            Transform oldR = head.Find("RightEyeLight");
            if (oldR != null) Undo.DestroyObjectImmediate(oldR.gameObject);

            // Apply Z-axis head tilt (broken neck)
            head.localRotation = Quaternion.Euler(0, 0, 30f);
        }

        SceneView.RepaintAll();
        Debug.Log("NPC configured at Waypoint 3 for editor preview!");
    }

    [MenuItem("Antigravity/Restore NPC Editor Preview")]
    public static void RestoreNPCEditorPreview()
    {
        GameObject npcObj = GameObject.Find("Creepy_NPC");
        if (npcObj == null) return;

        Terrain terrainComp = GameObject.Find("= BACKGROUND/Terrain")?.GetComponent<Terrain>();
        if (terrainComp == null) return;

        // Move to waypoint 0 (Far - Right side)
        float height = terrainComp.SampleHeight(new Vector3(0.6f, 0f, 28.0f)) + activeTerrainPos(terrainComp) - 0.05f;
        npcObj.transform.position = new Vector3(0.6f, height, 28.0f);
        npcObj.transform.localEulerAngles = new Vector3(0f, 180f, 0f);

        // Find head
        Transform head = FindChildRecursive(npcObj.transform, "mixamorig:Head");
        if (head != null)
        {
            // Remove old eyes
            Transform oldL = head.Find("LeftEyeLight");
            if (oldL != null) Undo.DestroyObjectImmediate(oldL.gameObject);
            Transform oldR = head.Find("RightEyeLight");
            if (oldR != null) Undo.DestroyObjectImmediate(oldR.gameObject);

            // Reset rotation
            head.localRotation = Quaternion.identity;
        }

        SceneView.RepaintAll();
        Debug.Log("NPC restored to default position in editor preview!");
    }

    private static float activeTerrainPos(Terrain terrain)
    {
        return terrain != null ? terrain.transform.position.y : 0f;
    }

    private static void CreateEyeSphereEditor(Transform parent)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Undo.RegisterCreatedObjectUndo(sphere, "Create Eye Sphere Editor");
        UnityEngine.Object.DestroyImmediate(sphere.GetComponent<Collider>());
        sphere.transform.parent = parent;
        sphere.transform.localPosition = Vector3.zero;
        sphere.transform.localScale = Vector3.one * 0.012f;

        Material mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        if (mat != null)
        {
            Color eyesColor = new Color(0.85f, 0.9f, 1.0f);
            mat.SetColor("_BaseColor", eyesColor * 3.5f);
            sphere.GetComponent<Renderer>().sharedMaterial = mat;
        }
    }

    private static Transform FindChildRecursive(Transform parent, string name)
    {
        if (parent.name == name) return parent;
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform found = FindChildRecursive(parent.GetChild(i), name);
            if (found != null) return found;
        }
        return null;
    }
}


