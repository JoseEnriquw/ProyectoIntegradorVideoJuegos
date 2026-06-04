using UnityEngine;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine.Events;
using UHFPS.Runtime;
using UnityEditor.SceneManagement;

public class ApplyJumpscareSettings
{
    [MenuItem("Antigravity/Apply Jumpscare Settings to Selected")]
    public static void ApplySettings()
    {
        if (Selection.activeGameObject == null)
        {
            Debug.LogWarning("Por favor, selecciona en la jerarquía el GameObject que tiene el JumpscareTrigger.");
            return;
        }

        var t = Selection.activeGameObject.GetComponent<JumpscareTrigger>();
        if (t == null)
        {
            Debug.LogWarning("El objeto seleccionado no tiene un componente JumpscareTrigger.");
            return;
        }

        Undo.RecordObject(t, "Apply Jumpscare Glitch Settings");
        
        t.InfluenceWobble = true;
        t.WobbleAmplitudeGain = 3f;
        t.WobbleFrequencyGain = 3f;
        t.WobbleDuration = 0.2f;

        t.InfluenceFear = true;
        t.FearDuration = 0.2f;
        t.VignetteStrength = 1f;
        t.TentaclesIntensity = 0.5f;

        // Si es de tipo Audio pero no tiene un clip, el JumpscareManager lo cancela al instante (0 segundos).
        // Por lo tanto, necesitamos forzar un sonido de 0.2s o hacer que no termine por audio.
        if (t.JumpscareType == JumpscareTrigger.JumpscareTypeEnum.Audio && t.JumpscareSound.audioClip == null)
        {
            Debug.LogWarning("¡Atención! Este jumpscare es de tipo Audio pero no tiene un AudioClip asignado. El efecto visual no durará nada. Por favor, asígnale un sonido o cambia el tipo a Indirect.");
        }

        // Para jumpscares con modelos que no desaparecen automáticamente
        if (t.Animator != null)
        {
            // Remover listeners viejos de SetActive para no duplicar
            for (int i = t.OnJumpscareEnded.GetPersistentEventCount() - 1; i >= 0; i--)
            {
                if (t.OnJumpscareEnded.GetPersistentTarget(i) == t.Animator.gameObject && t.OnJumpscareEnded.GetPersistentMethodName(i) == "SetActive")
                {
                    UnityEventTools.RemovePersistentListener(t.OnJumpscareEnded, i);
                }
            }

            UnityAction<bool> action = new UnityAction<bool>(t.Animator.gameObject.SetActive);
            UnityEventTools.AddBoolPersistentListener(t.OnJumpscareEnded, action, false);
        }

        EditorUtility.SetDirty(t);
        EditorSceneManager.MarkSceneDirty(t.gameObject.scene);
        Debug.Log($"Configuración de Glitch aplicada con éxito a: {t.gameObject.name}");
    }
}
