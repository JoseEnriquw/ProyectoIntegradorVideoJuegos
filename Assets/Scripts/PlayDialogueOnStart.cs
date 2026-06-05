using UnityEngine;
using UHFPS.Runtime;

public class PlayDialogueOnStart : MonoBehaviour
{
    [Tooltip("El componente DialogueTrigger que se va a reproducir al inicio.")]
    public DialogueTrigger dialogueTrigger;

    [Tooltip("¿Desactivar los controles del jugador durante la cinemática?")]
    public bool congelarJugador = true;

    private void Awake()
    {
        // Si el script PlayerIntroMovement está presente en la escena, este se encarga
        // de toda la intro (incluyendo diágolo, congelar e interactividad de la pausa).
        // Nos destruimos para evitar duplicar el audio y conflictos del cursor.
        if (FindObjectOfType<PlayerIntroMovement>() != null)
        {
            Destroy(this);
        }
    }

    private System.Collections.IEnumerator Start()
    {
        yield return null;

        if (dialogueTrigger == null)
        {
            dialogueTrigger = GetComponent<DialogueTrigger>();
        }

        if (dialogueTrigger != null)
        {
            dialogueTrigger.TriggerType = DialogueTrigger.TriggerTypeEnum.Event;
        }

        if (congelarJugador && PlayerPresenceManager.HasReference)
        {
            yield return new WaitForSeconds(1.0f);
            PlayerPresenceManager.Instance.FreezeMovement(true);
            PlayerPresenceManager.Instance.FreezeLook(true);
        }

        if (dialogueTrigger != null)
        {
            dialogueTrigger.TriggerDialogue();
        }
    }
}
