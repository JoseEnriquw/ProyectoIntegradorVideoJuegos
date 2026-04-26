using UnityEngine;

/// <summary>
/// Triggers an AudioSource when the player enters the trigger area.
/// </summary>
public class MusicBoxTrigger : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("The AudioSource component of the Music Box.")]
    public AudioSource MusicBoxSource;
    
    [Tooltip("If true, the music will only trigger once.")]
    public bool TriggerOnce = true;

    private bool _hasTriggered = false;
    private void OnTriggerEnter(Collider other)
    {
        // Check if the music has already been triggered (if TriggerOnce is true)
        if (TriggerOnce && _hasTriggered) return;

        // Check if the object entering the trigger is the Player
        if (other.CompareTag("Player"))
        {
            if (MusicBoxSource != null)
            {
                // Start playing the music if it's not already playing
                if (!MusicBoxSource.isPlaying)
                {
                    MusicBoxSource.Play();
                    _hasTriggered = true;
                    Debug.Log("[MusicBoxTrigger] Player entered trigger. Music Box started playing.");
                }
            }
            else
            {
                Debug.LogWarning("[MusicBoxTrigger] Music Box Source is not assigned!");
            }
        }
    }
}
