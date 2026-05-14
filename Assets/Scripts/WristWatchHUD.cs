using UnityEngine;
using TMPro;
using UHFPS.Runtime;

namespace UHFPS.Custom
{
    public class WristWatchHUD : MonoBehaviour
    {
        [Header("Input Settings")]
        public KeyCode WatchKey = KeyCode.G; // Tecla para mirar el reloj (G de Gear/Reloj)
        public bool HoldToView = true;      // Si se mantiene presionado o es toggle

        [Header("Animation Settings")]
        public GameObject WatchArmContainer; // El objeto que contiene el brazo (placeholder o real)
        public float AnimationSpeed = 8f;
        
        [Header("Positioning (relative to camera)")]
        public Vector3 HiddenOffset = new Vector3(0.4f, -1.2f, 0.5f);
        public Vector3 VisibleOffset = new Vector3(-0.15f, -0.35f, 0.55f);
        public Vector3 HiddenRotation = new Vector3(45, -30, 0);
        public Vector3 VisibleRotation = new Vector3(15, 10, -5);

        [Header("2D HUD Settings (Optional)")]
        public RectTransform Watch2DRect;
        public Vector2 Hidden2DPos = new Vector2(0, -600);
        public Vector2 Visible2DPos = new Vector2(0, -100);

        [Header("UI References")]
        public TMP_Text TimeText;
        public TMP_Text SymptomText;
        public CanvasGroup WatchCanvasGroup;

        private PlayerItemsManager playerItems;
        private int lastEquippedIndex = -1;
        private bool isViewing = false;
        private Transform cameraTransform;

        void Start()
        {
            if (Camera.main != null)
                cameraTransform = Camera.main.transform;

            // Cacheamos el Item Manager del jugador (UHFPS)
            if (GameManager.HasReference && GameManager.Instance.PlayerPresence != null)
            {
                playerItems = GameManager.Instance.PlayerPresence.PlayerManager.PlayerItems;
            }

            if (WatchArmContainer != null)
            {
                WatchArmContainer.transform.localPosition = HiddenOffset;
                WatchArmContainer.transform.localRotation = Quaternion.Euler(HiddenRotation);
            }

            if (WatchCanvasGroup != null)
                WatchCanvasGroup.alpha = 0f;
        }

        void Update()
        {
            HandleInput();
            AnimateWatch();
            UpdateClockUI();
        }

        private void HandleInput()
        {
            bool wasViewing = isViewing;

            if (HoldToView)
            {
                isViewing = UnityEngine.Input.GetKey(WatchKey);
            }
            else
            {
                if (UnityEngine.Input.GetKeyDown(WatchKey))
                    isViewing = !isViewing;
            }

            // --- INTEGRACIÓN UHFPS: Holster/Restaurar Item ---
            if (isViewing != wasViewing && playerItems != null)
            {
                if (isViewing)
                {
                    // Guardamos qué tenía en la mano y lo guardamos
                    lastEquippedIndex = playerItems.CurrentItemIndex;
                    playerItems.DeselectCurrent();
                }
                else
                {
                    // Restauramos el ítem que tenía antes
                    if (lastEquippedIndex != -1)
                    {
                        playerItems.SwitchPlayerItem(lastEquippedIndex);
                    }
                }
            }
        }

        private void AnimateWatch()
        {
            if (WatchArmContainer == null) return;

            // --- GESTIÓN DE ACTIVACIÓN ---
            // Si estamos mirando, el objeto DEBE estar activo.
            // Si no estamos mirando y ya terminó de bajar (alpha < 0.01), lo podemos desactivar para ahorrar recursos.
            if (isViewing) 
            {
                WatchArmContainer.SetActive(true);
            }
            else if (WatchCanvasGroup != null && WatchCanvasGroup.alpha < 0.05f)
            {
                // Solo lo desactivamos si no se ve nada (opcional)
                // WatchArmContainer.SetActive(false); 
            }

            // --- ANIMACIÓN 3D ---
            if (WatchArmContainer != null)
            {
                Vector3 targetPos = isViewing ? VisibleOffset : HiddenOffset;
                Quaternion targetRot = Quaternion.Euler(isViewing ? VisibleRotation : HiddenRotation);

                WatchArmContainer.transform.localPosition = Vector3.Lerp(WatchArmContainer.transform.localPosition, targetPos, Time.deltaTime * AnimationSpeed);
                WatchArmContainer.transform.localRotation = Quaternion.Slerp(WatchArmContainer.transform.localRotation, targetRot, Time.deltaTime * AnimationSpeed);
            }

            // --- ANIMACIÓN 2D ---
            if (Watch2DRect != null)
            {
                Vector2 target2D = isViewing ? Visible2DPos : Hidden2DPos;
                Watch2DRect.anchoredPosition = Vector2.Lerp(Watch2DRect.anchoredPosition, target2D, Time.deltaTime * AnimationSpeed);
            }

            if (WatchCanvasGroup != null)
            {
                WatchCanvasGroup.alpha = Mathf.Lerp(WatchCanvasGroup.alpha, isViewing ? 1f : 0f, Time.deltaTime * AnimationSpeed);
            }
        }

        private void UpdateClockUI()
        {
            // --- Actualizar Tiempo ---
            if (TimeText != null && SurvivalTimer.Instance != null)
            {
                TimeText.text = SurvivalTimer.Instance.TimeFormatted;

                if (SurvivalTimer.Instance.TimeRemaining < 60f) 
                {
                    TimeText.color = Color.red;
                }
                else
                {
                    TimeText.color = Color.white;
                }
            }

            // --- Actualizar Síntoma ---
            if (SymptomText != null && PlayerSymptom.Instance != null)
            {
                // Accedemos al síntoma actual. 
                // Nota: He visto en PlayerSymptom.cs que currentActiveSymptom es privado.
                // Tendríamos que hacerlo público o usar una propiedad.
                // Como soy el asistente, voy a sugerir una pequeña modificación en PlayerSymptom.cs
                SymptomText.text = GetSymptomName();
            }
        }

        private string GetSymptomName()
        {
            if (PlayerSymptom.Instance == null) return "SYSTEM ERROR";

            var symptom = PlayerSymptom.Instance.CurrentSymptom;
            if (symptom == PlayerSymptom.SymptomType.None) return "STATUS: STABLE";
            
            return "ALERTA: " + symptom.ToString().ToUpper();
        }
    }
}
