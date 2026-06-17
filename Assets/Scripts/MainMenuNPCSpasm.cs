using UnityEngine;
using System.Collections;

public class MainMenuNPCSpasm : MonoBehaviour
{
    private Transform headBone;
    private Transform spineBone;

    [Header("Twitch Timing Settings")]
    public float minInterval = 2.5f;
    public float maxInterval = 6.0f;
    public float minDuration = 0.15f;
    public float maxDuration = 0.45f;

    [Header("Jitter Settings")]
    public float twitchSpeed = 45f;
    public float headTwitchAngle = 14f;
    public float spineTwitchAngle = 5f;

    [Header("Broken Neck Settings")]
    public float defaultHeadTiltZ = 30f;

    private float currentSpasmWeight = 0f;
    private float targetSpasmWeight = 0f;
    private float spasmWeightVelocity = 0f;

    private float nextTwitchTime;
    private float noiseSeedX;
    private float noiseSeedY;

    void Start()
    {
        // Recursively find bone transforms by name
        headBone = FindChildRecursive(transform, "mixamorig:Head");
        spineBone = FindChildRecursive(transform, "mixamorig:Spine2");

        if (headBone == null) Debug.LogWarning("MainMenuNPCSpasm: headBone ('mixamorig:Head') not found!");
        if (spineBone == null) Debug.LogWarning("MainMenuNPCSpasm: spineBone ('mixamorig:Spine2') not found!");

        nextTwitchTime = Time.time + Random.Range(minInterval, maxInterval);
        noiseSeedX = Random.Range(0f, 100f);
        noiseSeedY = Random.Range(0f, 100f);
    }

    void LateUpdate()
    {
        // Apply constant broken neck tilt (Z roll rotation)
        if (headBone != null && defaultHeadTiltZ != 0f)
        {
            headBone.localRotation *= Quaternion.Euler(0f, 0f, defaultHeadTiltZ);
        }

        // Randomly trigger spasms
        if (targetSpasmWeight == 0f && Time.time > nextTwitchTime)
        {
            StartCoroutine(TriggerSpasm());
            nextTwitchTime = Time.time + Random.Range(minInterval, maxInterval);
        }

        // Smoothly interpolate spasm weight
        currentSpasmWeight = Mathf.SmoothDamp(currentSpasmWeight, targetSpasmWeight, ref spasmWeightVelocity, 0.05f);

        // Apply spasmodic offset on top of animators in LateUpdate
        if (currentSpasmWeight > 0.01f)
        {
            float noiseX = (Mathf.PerlinNoise(noiseSeedX + Time.time * twitchSpeed, 0f) - 0.5f) * 2f;
            float noiseY = (Mathf.PerlinNoise(0f, noiseSeedY + Time.time * twitchSpeed) - 0.5f) * 2f;

            if (headBone != null)
            {
                // Incorporate Z-axis roll jitter to simulate bone cracking/clicking during spasm
                Quaternion headOffset = Quaternion.Euler(
                    noiseX * headTwitchAngle * currentSpasmWeight, 
                    noiseY * headTwitchAngle * currentSpasmWeight, 
                    noiseX * headTwitchAngle * 0.5f * currentSpasmWeight
                );
                headBone.localRotation *= headOffset;
            }

            if (spineBone != null)
            {
                Quaternion spineOffset = Quaternion.Euler(
                    noiseX * spineTwitchAngle * currentSpasmWeight, 
                    0f, 
                    noiseY * spineTwitchAngle * currentSpasmWeight
                );
                spineBone.localRotation *= spineOffset;
            }
        }
    }

    IEnumerator TriggerSpasm()
    {
        targetSpasmWeight = 1f;
        yield return new WaitForSeconds(Random.Range(minDuration, maxDuration) * 0.5f);
        targetSpasmWeight = 0f;
        yield return new WaitForSeconds(0.15f); // Wait for weight to damp back to zero
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
