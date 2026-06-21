using System;
using System.Collections;
using UnityEngine;
using Unity.Cinemachine;
using System.Reactive.Disposables;
using UHFPS.Tools;

namespace UHFPS.Runtime
{
    public class CinematicDialogueZoom : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The DialogueTrigger that triggers this cinematic zoom. If null, it will look for one on the same GameObject.")]
        public DialogueTrigger TargetTrigger;

        [Tooltip("The CinemachineCamera that will zoom on the NPC.")]
        public CinemachineCamera ZoomCamera;

        [Tooltip("The GameObject or Transform to look at and focus on.")]
        public Transform TargetLookAt;

        [Header("Camera Zoom Settings")]
        [Tooltip("If true, the camera will be dynamically positioned relative to the target. If false, it will keep its default scene position and rotation.")]
        public bool UseDynamicPositioning = false;

        [Tooltip("Offset of the camera relative to the target. Z is distance in front, X is side offset, Y is height offset.")]
        public Vector3 CameraOffset = new Vector3(0.8f, 1.5f, 3.0f);

        [Tooltip("The Lens Field of View (zoom level). Lower values zoom closer.")]
        public float FieldOfView = 40f;

        [Tooltip("The priority of the ZoomCamera when active.")]
        public int ActivePriority = 20;

        [Header("Transition Settings")]
        [Tooltip("The time in seconds it takes to blend the camera from player to the NPC.")]
        public float BlendInTime = 1.5f;

        [Tooltip("The time in seconds it takes to blend the camera from the NPC back to the player.")]
        public float BlendOutTime = 2.5f;

        [Tooltip("The blend style to use for the transitions.")]
        public CinemachineBlendDefinition.Styles TransitionStyle = CinemachineBlendDefinition.Styles.EaseInOut;

        private int originalPriority;
        private bool isZooming;
        private readonly CompositeDisposable disposables = new();

        private CinemachineBrain cinemachineBrain;
        private CinemachineBlendDefinition originalBlend;
        private Coroutine restoreBlendCoroutine;

        private void Awake()
        {
            if (TargetTrigger == null)
            {
                TargetTrigger = GetComponent<DialogueTrigger>();
            }

            if (ZoomCamera != null)
            {
                originalPriority = ZoomCamera.Priority.Value;
            }
        }

        private void OnEnable()
        {
            if (DialogueSystem.Instance == null) return;

            DialogueSystem.Instance.OnDialogueStart.Subscribe(_ => OnDialogueStarted()).AddTo(disposables);
            DialogueSystem.Instance.OnDialogueEnd.Subscribe(_ => OnDialogueEnded()).AddTo(disposables);
        }

        private void OnDisable()
        {
            disposables.Clear();
            if (isZooming)
            {
                ResetZoom();
            }
        }

        private void OnDialogueStarted()
        {
            if (DialogueSystem.Instance == null) return;

            if (DialogueSystem.Instance.CurrentTrigger == TargetTrigger)
            {
                isZooming = true;

                // Stop any pending blend restoration
                if (restoreBlendCoroutine != null)
                {
                    StopCoroutine(restoreBlendCoroutine);
                    restoreBlendCoroutine = null;
                }

                if (PlayerPresenceManager.HasReference)
                {
                    PlayerPresenceManager.Instance.FreezePlayer(true);

                    // Get CinemachineBrain and override the default blend
                    var playerCamera = PlayerPresenceManager.Instance.PlayerCamera;
                    if (playerCamera != null)
                    {
                        cinemachineBrain = playerCamera.GetComponent<CinemachineBrain>();
                        if (cinemachineBrain != null)
                        {
                            originalBlend = cinemachineBrain.DefaultBlend;

                            var customBlend = new CinemachineBlendDefinition();
                            customBlend.Style = TransitionStyle;
                            customBlend.Time = BlendInTime;
                            cinemachineBrain.DefaultBlend = customBlend;
                        }
                    }
                }

                if (ZoomCamera != null)
                {
                    // Dynamically position camera relative to target
                    if (UseDynamicPositioning && TargetLookAt != null)
                    {
                        Vector3 targetPos = TargetLookAt.position;
                        Vector3 targetForward = TargetLookAt.forward;
                        Vector3 targetRight = TargetLookAt.right;

                        // Position camera in front of target using offset values
                        Vector3 camPos = targetPos + targetForward * CameraOffset.z + targetRight * CameraOffset.x + Vector3.up * CameraOffset.y;
                        ZoomCamera.transform.position = camPos;
                        ZoomCamera.transform.LookAt(targetPos);
                        ZoomCamera.LookAt = TargetLookAt;
                    }

                    // Apply zoom level (FOV)
                    var lens = ZoomCamera.Lens;
                    lens.FieldOfView = FieldOfView;
                    ZoomCamera.Lens = lens;

                    // Activate camera
                    var p = ZoomCamera.Priority;
                    p.Value = ActivePriority;
                    ZoomCamera.Priority = p;
                }
            }
        }

        private void OnDialogueEnded()
        {
            if (isZooming)
            {
                ResetZoom();
            }
        }

        private void ResetZoom()
        {
            isZooming = false;
            if (ZoomCamera != null)
            {
                var p = ZoomCamera.Priority;
                p.Value = originalPriority;
                ZoomCamera.Priority = p;
            }

            // Set blend out definition on the brain
            if (cinemachineBrain != null)
            {
                var returnBlend = new CinemachineBlendDefinition();
                returnBlend.Style = TransitionStyle;
                returnBlend.Time = BlendOutTime;
                cinemachineBrain.DefaultBlend = returnBlend;

                // Start coroutine to restore the original blend after transition completes
                restoreBlendCoroutine = StartCoroutine(RestoreOriginalBlend(BlendOutTime));
            }

            if (PlayerPresenceManager.HasReference)
            {
                PlayerPresenceManager.Instance.FreezePlayer(false);
            }
        }

        private IEnumerator RestoreOriginalBlend(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (cinemachineBrain != null)
            {
                cinemachineBrain.DefaultBlend = originalBlend;
                cinemachineBrain = null;
            }
            restoreBlendCoroutine = null;
        }
    }
}
