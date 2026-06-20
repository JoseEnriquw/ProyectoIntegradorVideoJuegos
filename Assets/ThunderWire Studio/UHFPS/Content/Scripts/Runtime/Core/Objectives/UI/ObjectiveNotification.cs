using System.Collections;
using UnityEngine;
using ThunderWire.Attributes;
using TMPro;
using UHFPS.Tools;

namespace UHFPS.Runtime
{
    [InspectorHeader("Objective Notification")]
    public class ObjectiveNotification : MonoBehaviour
    {
        public Animator Animator;
        public TMP_Text Title;

        [Header("Animation")]
        public string ShowTrigger = "Show";
        public string HideTrigger = "Hide";
        public string HideState = "Hide";

        private bool isShowed;

        public void ShowNotification(string title, float duration, SoundClip sound = null)
        {
            if (isShowed)
                return;

            Title.text = title;
            Animator.SetTrigger(ShowTrigger);
            StartCoroutine(OnShowNotification(duration));
            isShowed = true;

            if (sound != null && sound.audioClip != null)
            {
                GameTools.PlayOneShot2D(transform.position, sound, "NotificationSound");
            }
        }

        IEnumerator OnShowNotification(float duration)
        {
            yield return new WaitForSeconds(duration);
            Animator.SetTrigger(HideTrigger);
            yield return new WaitForAnimatorStateExit(Animator, HideState);
            isShowed = false;
        }
    }
}