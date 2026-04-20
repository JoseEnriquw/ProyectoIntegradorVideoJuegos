using UnityEngine;
using UHFPS.Runtime;

public class AnimationEventBridge : MonoBehaviour
{
    public NPCStateMachine machine;

    // ?? ESTE MÉTODO LO LLAMA LA ANIMACIÓN
    public void StartRun()
    {
        if (machine != null)
        {
            machine.SendAnimationMessage("StartRun");
        }
    }
}