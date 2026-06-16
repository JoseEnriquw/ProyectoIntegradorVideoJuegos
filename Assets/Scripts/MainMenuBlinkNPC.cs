using UnityEngine;

public class MainMenuBlinkNPC : MonoBehaviour
{
    [System.Serializable]
    public struct BlinkPosition
    {
        public Vector3 position;
        public Vector3 rotation;
        public bool isVisible;
    }

    [Header("Blink Positions")]
    public BlinkPosition[] blinkPositions;

    [Header("Spooky Glow Eyes")]
    public bool enableGlowEyes = false;
    public Color eyeColor = new Color(0.85f, 0.9f, 1.0f);
    public float eyeIntensity = 3.5f;

    private int currentIndex = 0;
    private bool isForced = false;
    private int savedIndex = 0;

    private Light leftEyeLight;
    private Light rightEyeLight;
    private Transform leftEyeSphere;
    private Transform rightEyeSphere;

    void Start()
    {
        if (enableGlowEyes)
        {
            CreateGlowEyes();
        }

        // Enforce default far position on start
        if (blinkPositions != null && blinkPositions.Length > 0)
        {
            ApplyPosition(0);
        }
    }

    public void OnBlackout()
    {
        if (blinkPositions == null || blinkPositions.Length == 0) return;
        if (isForced) return; // Do not cycle while forced by hover

        // Advance to next position
        currentIndex = (currentIndex + 1) % blinkPositions.Length;
        ApplyPosition(currentIndex);
    }

    public void ForceTeleport(int index)
    {
        if (blinkPositions == null || index < 0 || index >= blinkPositions.Length) return;

        if (!isForced)
        {
            savedIndex = currentIndex;
            isForced = true;
        }

        ApplyPosition(index);
    }

    public void ReleaseForce()
    {
        if (!isForced) return;

        isForced = false;
        ApplyPosition(savedIndex);
    }

    private void ApplyPosition(int index)
    {
        BlinkPosition bp = blinkPositions[index];
        
        // Disable character look target tracking at extreme close ranges to prevent breaking neck
        var creepyIdle = GetComponent<MainMenuCreepyIdle>();
        if (creepyIdle != null)
        {
            // Only track cursor at further distances (Index 0, 1, 2)
            creepyIdle.trackCursor = (index < 3);
        }

        transform.localPosition = bp.position;
        transform.localEulerAngles = bp.rotation;
        gameObject.SetActive(bp.isVisible);
        
        // LOD Dynamic eye scaling based on waypoint distance/index
        float sphereScale = 0.012f;
        float lightIntensity = eyeIntensity;
        float lightRange = 0.5f;

        if (index == 0) // Far
        {
            sphereScale = 0.08f;
            lightIntensity = eyeIntensity * 3.0f;
            lightRange = 1.5f;
        }
        else if (index == 1) // Mid
        {
            sphereScale = 0.04f;
            lightIntensity = eyeIntensity * 2.0f;
            lightRange = 1.0f;
        }
        else if (index == 2) // Close (Options)
        {
            sphereScale = 0.02f;
            lightIntensity = eyeIntensity * 1.2f;
            lightRange = 0.6f;
        }
        else if (index == 4) // Intermediate Close (LoadGame)
        {
            sphereScale = 0.03f;
            lightIntensity = eyeIntensity * 1.5f;
            lightRange = 0.8f;
        }
        else // Extreme Close (Index 3)
        {
            sphereScale = 0.012f;
            lightIntensity = eyeIntensity;
            lightRange = 0.4f;
        }

        if (leftEyeLight != null)
        {
            leftEyeLight.intensity = lightIntensity;
            leftEyeLight.range = lightRange;
        }
        if (rightEyeLight != null)
        {
            rightEyeLight.intensity = lightIntensity;
            rightEyeLight.range = lightRange;
        }
        if (leftEyeSphere != null)
        {
            leftEyeSphere.localScale = new Vector3(sphereScale, sphereScale, sphereScale);
        }
        if (rightEyeSphere != null)
        {
            rightEyeSphere.localScale = new Vector3(sphereScale, sphereScale, sphereScale);
        }

        Debug.Log($"BlinkNPC: Moved to waypoint {index} (Visible: {bp.isVisible}, Position: {bp.position})");
    }

    private void CreateGlowEyes()
    {
        Transform head = FindChildRecursive(transform, "mixamorig:Head");
        if (head == null) return;

        // Create left eye light
        GameObject leftEye = new GameObject("LeftEyeLight");
        leftEye.transform.parent = head;
        // Approximation offsets for the standard humanoid head bone structure
        leftEye.transform.localPosition = new Vector3(-0.065f, 0.10f, 0.085f);
        leftEye.transform.localRotation = Quaternion.identity;
        leftEyeLight = leftEye.AddComponent<Light>();
        leftEyeLight.type = LightType.Point;
        leftEyeLight.color = eyeColor;
        leftEyeLight.intensity = eyeIntensity;
        leftEyeLight.range = 0.5f;
        leftEyeLight.shadows = LightShadows.None;

        // Create right eye light
        GameObject rightEye = new GameObject("RightEyeLight");
        rightEye.transform.parent = head;
        rightEye.transform.localPosition = new Vector3(0.065f, 0.10f, 0.085f);
        rightEye.transform.localRotation = Quaternion.identity;
        rightEyeLight = rightEye.AddComponent<Light>();
        rightEyeLight.type = LightType.Point;
        rightEyeLight.color = eyeColor;
        rightEyeLight.intensity = eyeIntensity;
        rightEyeLight.range = 0.5f;
        rightEyeLight.shadows = LightShadows.None;

        // Create emissive eye spheres
        leftEyeSphere = CreateEyeSphere(leftEye.transform);
        rightEyeSphere = CreateEyeSphere(rightEye.transform);
    }

    private Transform CreateEyeSphere(Transform parent)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(sphere.GetComponent<Collider>());
        sphere.transform.parent = parent;
        sphere.transform.localPosition = Vector3.zero;
        sphere.transform.localScale = Vector3.one;

        // Simple unlit material for solid emission color (HDR base color)
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        if (mat != null)
        {
            Color hdrColor = eyeColor * eyeIntensity;
            mat.SetColor("_BaseColor", hdrColor);
            sphere.GetComponent<Renderer>().sharedMaterial = mat;
        }
        return sphere.transform;
    }

    private Transform FindChildRecursive(Transform parent, string name)
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
