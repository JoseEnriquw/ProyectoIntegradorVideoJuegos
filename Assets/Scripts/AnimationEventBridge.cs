using UHFPS.Runtime;
using Unity.VisualScripting;
using UnityEngine;

public class AnimationEventBridge : MonoBehaviour
{
    public NPCStateMachine machine;

    // ?? ESTE MÉTODO LO LLAMA LA ANIMACIÓN
    public void StartRun()
    {
        Debug.Log("EVENTO START RUN DISPARADO");

        if (machine != null)
        {
            machine.SendAnimationMessage("StartRun");
        }
    }

}

