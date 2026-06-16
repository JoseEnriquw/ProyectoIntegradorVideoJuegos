using System.Collections;
using UnityEngine;

public class MainMenuHorrorEvents : MonoBehaviour
{
    [Header("Lights to Control")]
    public Light lanternLight;
    public Light flashlightSpot;

    [Header("Entities to Toggle")]
    public GameObject creepyMaiden;
    public GameObject creepyNpc;

    [Header("Event Timing")]
    public float minInterval = 25f;
    public float maxInterval = 55f;
    public float blackoutDuration = 2.5f;

    [Header("Sounds")]
    public AudioClip flickerSound;
    public AudioClip whisperSound;
    private AudioSource audioSource;

    private float nextEventTime;
    private bool isEventRunning = false;

    public bool IsEventRunning => isEventRunning;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // If references are not manually assigned, try to find them in the scene
        if (lanternLight == null)
        {
            GameObject go = GameObject.Find("Player Lantern Light");
            if (go != null) lanternLight = go.GetComponent<Light>();
        }
        if (flashlightSpot == null)
        {
            GameObject go = GameObject.Find("FlashlightSpotlight");
            if (go != null) flashlightSpot = go.GetComponent<Light>();
        }
        if (creepyMaiden == null)
        {
            creepyMaiden = GameObject.Find("Creepy_Maiden");
        }
        if (creepyNpc == null)
        {
            creepyNpc = GameObject.Find("Creepy_NPC");
        }

        nextEventTime = Time.time + Random.Range(minInterval, maxInterval);
    }

    void Update()
    {
        if (!isEventRunning && Time.time > nextEventTime)
        {
            StartCoroutine(TriggerBlackoutEvent());
        }
    }

    IEnumerator TriggerBlackoutEvent()
    {
        isEventRunning = true;

        // 1. Flicker stage (aggressive flashlight failure)
        int flickers = Random.Range(6, 12);
        if (flickerSound != null)
        {
            audioSource.PlayOneShot(flickerSound, 0.4f);
        }

        bool lanternOrigEnabled = lanternLight != null ? lanternLight.enabled : false;
        bool flashOrigEnabled = flashlightSpot != null ? flashlightSpot.enabled : false;

        // Turn off any flicker components temporarily during this event
        var lanternFlickerComp = lanternLight != null ? lanternLight.GetComponent<MainMenuLightFlicker>() : null;
        var flashFlickerComp = flashlightSpot != null ? flashlightSpot.GetComponent<MainMenuLightFlicker>() : null;
        if (lanternFlickerComp != null) lanternFlickerComp.enabled = false;
        if (flashFlickerComp != null) flashFlickerComp.enabled = false;

        for (int i = 0; i < flickers; i++)
        {
            if (lanternLight != null) lanternLight.enabled = !lanternLight.enabled;
            if (flashlightSpot != null) flashlightSpot.enabled = !flashlightSpot.enabled;
            yield return new WaitForSeconds(Random.Range(0.04f, 0.12f));
        }

        // 2. Blackout stage (total darkness)
        if (lanternLight != null) lanternLight.enabled = false;
        if (flashlightSpot != null) flashlightSpot.enabled = false;

        yield return new WaitForSeconds(0.4f);

        // Creepy whisper during darkness
        if (whisperSound != null)
        {
            audioSource.PlayOneShot(whisperSound, 0.5f);
        }

        yield return new WaitForSeconds(blackoutDuration - 0.4f);

        // 3. Toggle active state of entities in the dark
        if (creepyMaiden != null)
        {
            creepyMaiden.SetActive(!creepyMaiden.activeSelf);
        }
        if (creepyNpc != null)
        {
            var blinkNPC = creepyNpc.GetComponent<MainMenuBlinkNPC>();
            if (blinkNPC != null)
            {
                blinkNPC.OnBlackout();
            }
            else
            {
                if (Random.value > 0.5f)
                {
                    creepyNpc.SetActive(!creepyNpc.activeSelf);
                }
            }
        }

        // 4. Recovery stage (flicker back on)
        flickers = Random.Range(3, 6);
        for (int i = 0; i < flickers; i++)
        {
            if (lanternLight != null) lanternLight.enabled = !lanternLight.enabled;
            if (flashlightSpot != null) flashlightSpot.enabled = !flashlightSpot.enabled;
            yield return new WaitForSeconds(Random.Range(0.05f, 0.1f));
        }

        // Restore components and original active states
        if (lanternLight != null) lanternLight.enabled = lanternOrigEnabled;
        if (flashlightSpot != null) flashlightSpot.enabled = flashOrigEnabled;
        if (lanternFlickerComp != null) lanternFlickerComp.enabled = true;
        if (flashFlickerComp != null) flashFlickerComp.enabled = true;

        isEventRunning = false;
        nextEventTime = Time.time + Random.Range(minInterval, maxInterval);
    }
}
