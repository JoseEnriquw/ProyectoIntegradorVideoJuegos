using System;
using System.Collections;
using UnityEngine;
using Unity.Cinemachine;
using System.Reactive.Disposables;
using UHFPS.Runtime;
using UHFPS.Tools;

namespace UHFPS.Runtime
{
    public class CinematicSlowDialogueZoom : MonoBehaviour
    {
        public enum ZoomModeEnum { ZoomInAndOut, ZoomInThenOut }

        [Header("References")]
        [Tooltip("The DialogueTrigger that triggers this cinematic zoom. If null, it will look for one on this GameObject.")]
        public DialogueTrigger TargetTrigger;

        [Tooltip("The CinemachineCamera that will perform the zoom.")]
        public CinemachineCamera ZoomCamera;

        [Tooltip("The GameObject or Transform to look at and focus on.")]
        public Transform TargetLookAt;

        [Header("Camera Offset Settings")]
        [Tooltip("Offset of the camera relative to the target. Z is distance in front, X is side offset, Y is height offset.")]
        public Vector3 CameraOffset = new Vector3(0.8f, 1.5f, 3.0f);

        [Header("Zoom Settings")]
        [Tooltip("How the zoom behaves during the dialogue.")]
        public ZoomModeEnum ZoomMode = ZoomModeEnum.ZoomInAndOut;

        [Tooltip("Field of View at the start of the zoom.")]
        public float StartFieldOfView = 60f;

        [Tooltip("Field of View at the peak zoom level (lower value is closer).")]
        public float TargetFieldOfView = 35f;

        [Header("Transition Settings")]
        [Tooltip("The time in seconds it takes to blend the camera from player to the zoom camera.")]
        public float BlendInTime = 2.0f;

        [Tooltip("The time in seconds it takes to blend the camera back to the player.")]
        public float BlendOutTime = 2.5f;

        [Tooltip("The blend style to use for the transitions.")]
        public CinemachineBlendDefinition.Styles TransitionStyle = CinemachineBlendDefinition.Styles.EaseInOut;

        private int originalPriority;
        private bool isZooming;
        private readonly CompositeDisposable disposables = new();

        private CinemachineBrain cinemachineBrain;
        private CinemachineBlendDefinition originalBlend;
        private Coroutine zoomCoroutine;
        private Coroutine restoreBlendCoroutine;

        private void Awake()
        {
            if (TargetTrigger == null)
                TargetTrigger = GetComponent<DialogueTrigger>();

            if (ZoomCamera != null)
                originalPriority = ZoomCamera.Priority.Value;
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
            if (isZooming) ResetZoom();
        }

        private void OnDialogueStarted()
        {
            if (DialogueSystem.Instance == null || TargetTrigger == null) return;

            if (DialogueSystem.Instance.CurrentTrigger == TargetTrigger)
            {
                isZooming = true;

                if (restoreBlendCoroutine != null)
                {
                    StopCoroutine(restoreBlendCoroutine);
                    restoreBlendCoroutine = null;
                }

                if (PlayerPresenceManager.HasReference)
                {
                    PlayerPresenceManager.Instance.FreezePlayer(true);

                    var playerCamera = PlayerPresenceManager.Instance.PlayerCamera;
                    if (playerCamera != null)
                    {
                        cinemachineBrain = playerCamera.GetComponent<CinemachineBrain>();
                        if (cinemachineBrain != null)
                        {
                            originalBlend = cinemachineBrain.DefaultBlend;

                            var customBlend = new CinemachineBlendDefinition
                            {
                                Style = TransitionStyle,
                                Time = BlendInTime
                            };
                            cinemachineBrain.DefaultBlend = customBlend;
                        }
                    }
                }

                if (ZoomCamera != null)
                {
                    if (TargetLookAt != null)
                    {
                        Vector3 targetPos = TargetLookAt.position;
                        Vector3 targetForward = TargetLookAt.forward;
                        Vector3 targetRight = TargetLookAt.right;

                        Vector3 camPos = targetPos + targetForward * CameraOffset.z + targetRight * CameraOffset.x + Vector3.up * CameraOffset.y;
                        ZoomCamera.transform.position = camPos;
                        ZoomCamera.LookAt = TargetLookAt;
                    }

                    // Reset lens to start FOV
                    var lens = ZoomCamera.Lens;
                    lens.FieldOfView = StartFieldOfView;
                    ZoomCamera.Lens = lens;

                    // Activate camera
                    var p = ZoomCamera.Priority;
                    p.Value = 20; // High active priority
                    ZoomCamera.Priority = p;

                    // Start slow zoom interpolation coroutine
                    AudioSource audioSource = TargetTrigger.DialogueType == DialogueTrigger.DialogueTypeEnum.Global
                        ? DialogueSystem.Instance.AudioSource
                        : TargetTrigger.DialogueAudio;

                    if (zoomCoroutine != null) StopCoroutine(zoomCoroutine);
                    zoomCoroutine = StartCoroutine(ZoomRoutine(audioSource));
                }
            }
        }

        private IEnumerator ZoomRoutine(AudioSource audioSource)
        {
            float duration = 5f; // Fallback default duration
            if (audioSource != null && audioSource.clip != null)
            {
                // Wait briefly for audio playback to initialize
                yield return new WaitForSeconds(0.1f);
                duration = audioSource.clip.length;
            }

            float elapsed = 0f;
            while (elapsed < duration && isZooming)
            {
                float progress = elapsed / duration;

                float currentFOV;
                if (ZoomMode == ZoomModeEnum.ZoomInAndOut)
                {
                    // Sine wave maps progress [0, 1] to [0, 1, 0]
                    float factor = Mathf.Sin(progress * Mathf.PI);
                    currentFOV = Mathf.Lerp(StartFieldOfView, TargetFieldOfView, factor);
                }
                else
                {
                    // Linear map from [Start] to [Target] FOV
                    currentFOV = Mathf.Lerp(StartFieldOfView, TargetFieldOfView, progress);
                }

                if (ZoomCamera != null)
                {
                    var lens = ZoomCamera.Lens;
                    lens.FieldOfView = currentFOV;
                    ZoomCamera.Lens = lens;
                }

                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        private void OnDialogueEnded()
        {
            if (isZooming) ResetZoom();
        }

        private void ResetZoom()
        {
            isZooming = false;
            if (zoomCoroutine != null)
            {
                StopCoroutine(zoomCoroutine);
                zoomCoroutine = null;
            }

            if (ZoomCamera != null)
            {
                var p = ZoomCamera.Priority;
                p.Value = originalPriority;
                ZoomCamera.Priority = p;
            }

            if (cinemachineBrain != null)
            {
                var returnBlend = new CinemachineBlendDefinition
                {
                    Style = TransitionStyle,
                    Time = BlendOutTime
                };
                cinemachineBrain.DefaultBlend = returnBlend;

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
