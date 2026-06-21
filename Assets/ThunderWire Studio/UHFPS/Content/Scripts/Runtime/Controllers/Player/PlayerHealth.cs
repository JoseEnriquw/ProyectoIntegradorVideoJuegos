using UnityEngine;
using UHFPS.Tools;
using UHFPS.Rendering;
using static UHFPS.Runtime.GameManager;

namespace UHFPS.Runtime
{
    public class PlayerHealth : BaseHealthEntity
    {
        public uint MaxHealth = 100;
        public uint StartHealth = 100;

        public bool UseHearthbeat;
        public float LowHealthPulse = 5f;
        public float HealthFadeTime = 0.1f;

        public uint MinHealthFade = 20;
        public float BloodDuration = 2f;
        public float BloodFadeInSpeed = 2f;
        public float BloodFadeOutSpeed = 1f;
        public float CloseEyesTime = 2f;
        public float CloseEyesSpeed = 2f;

        public bool EnableFallDamage;
        public MinMax FallDistance = new(5, 10);
        public MinMaxInt FallDamage = new(0, 15);

        public bool UseDamageSounds;
        public AudioClip[] DamageSounds;
        [Range(0f, 1f)]
        public float DamageVolume = 1f;

        public bool IsInvisibleToEnemies;
        public bool IsInvisibleToAllies;

        public GString TimeOutDeathGloc;

        [HideInInspector]
        public bool DeathByTimer;

        private PlayerStateMachine player;
        private GameManager gameManager;
        private EyeBlink eyeBlink;

        private float targetHealth;
        private float healthVelocity;

        private float bloodWeight;
        private float targetBlood;
        private float bloodTime;
        private float eyesTime;
        private float symptomBloodWeight;

        private int lastDamageSound;
        private Vector3 lastPosition;

        private bool wasInAir;
        private bool lastPosLoaded;

        private void Awake()
        {
            gameManager = GameManager.Instance;
            gameManager.HealthPPVolume.profile.TryGet(out eyeBlink);
            player = GetComponent<PlayerStateMachine>();

            if (!SaveGameManager.GameWillLoad || !SaveGameManager.GameStateExist)
                InitHealth();
        }

        private void Start()
        {
            if (TimeOutDeathGloc != null)
                TimeOutDeathGloc.SubscribeGloc();
        }

        private void Update()
        {
            if (!lastPosLoaded)
            {
                lastPosition = transform.position;
                lastPosLoaded = true;
            }

            if (gameManager.HealthBar != null)
            {
                float healthValue = gameManager.HealthBar.value;
                healthValue = Mathf.SmoothDamp(healthValue, targetHealth, ref healthVelocity, HealthFadeTime);
                gameManager.HealthBar.value = healthValue;
            }

            if (EntityHealth > MinHealthFade)
            {
                if (bloodTime > 0f) bloodTime -= Time.deltaTime;
                else
                {
                    targetBlood = 0f;
                    bloodTime = 0f;
                }
            }

            bloodWeight = Mathf.MoveTowards(bloodWeight, targetBlood, Time.deltaTime * (bloodTime > 0 ? BloodFadeInSpeed : BloodFadeOutSpeed));

            float targetSymptomBlood = 0f;
            if (PlayerSymptom.Instance != null && PlayerSymptom.Instance.CurrentSymptom != PlayerSymptom.SymptomType.None)
            {
                targetSymptomBlood = 1f;
            }

            symptomBloodWeight = Mathf.MoveTowards(symptomBloodWeight, targetSymptomBlood, Time.deltaTime * (targetSymptomBlood > 0f ? BloodFadeInSpeed : BloodFadeOutSpeed));

            float finalWeight = Mathf.Max(bloodWeight, symptomBloodWeight);
            gameManager.HealthPPVolume.weight = finalWeight;

            if (IsDead && eyeBlink != null)
            {
                if (eyesTime < CloseEyesTime)
                {
                    eyesTime += Time.deltaTime;
                }
                else
                {
                    float blinkValue = eyeBlink.Blink.value;
                    eyeBlink.Blink.value = Mathf.MoveTowards(blinkValue, 1f, Time.deltaTime * CloseEyesSpeed);
                }
            }

            if (!IsDead && EnableFallDamage)
            {
                if (player.StateGrounded)
                {
                    if(!wasInAir) lastPosition = transform.position;
                    else
                    {
                        Vector3 dropPosition = transform.position;
                        float fallDistance = Mathf.Clamp(lastPosition.y - dropPosition.y, 0, Mathf.Infinity);
                        float fallModifier = Mathf.InverseLerp(FallDistance.RealMin, FallDistance.RealMax, fallDistance);
                        float fallDamage = 0f;

                        if (fallModifier > 0f) fallDamage = Mathf.Lerp(FallDamage.RealMin, FallDamage.RealMax, fallModifier);
                        if (fallDamage > 1f) ApplyDamage(Mathf.RoundToInt(fallDamage));
                        wasInAir = false;
                    }
                }
                else if(!wasInAir)
                {
                    wasInAir = true;
                }
            }
        }

        public void InitHealth()
        {
            InitializeHealth((int)StartHealth, (int)MaxHealth);
            DeathByTimer = false;

            if (StartHealth <= MinHealthFade)
            {
                targetBlood = 1f;
                bloodTime = BloodDuration;
            }
        }

        public override void OnHealthChanged(int oldHealth, int newHealth)
        {
            gameManager.HealthPercent.text = newHealth.ToString();
            targetHealth = (float)newHealth / MaxHealth;

            if (UseHearthbeat)
            {
                Material hearthbeatMat = gameManager.Hearthbeat.material;

                if (newHealth <= 0)
                {
                    hearthbeatMat.EnableKeyword("ZERO_PULSE");
                }
                else
                {
                    float pulse = GameTools.Remap(0f, MaxHealth, LowHealthPulse, 1f, newHealth);
                    hearthbeatMat.SetFloat("_PulseMultiplier", pulse);
                    hearthbeatMat.DisableKeyword("ZERO_PULSE");
                }
            }
        }

        public override void ApplyDamage(int damage, Transform sender = null)
        {
            if (IsDead) return;

            base.ApplyDamage(damage, sender);

            if (UseDamageSounds && DamageSounds.Length > 0)
            {
                int damageSound = GameTools.RandomUnique(0, DamageSounds.Length, lastDamageSound);
                GameTools.PlayOneShot2D(transform.position, DamageSounds[damageSound], DamageVolume, "DamageSound");
                lastDamageSound = damageSound;
            }

            targetBlood = 1f;
            bloodTime = BloodDuration;
        }

        public override void ApplyHeal(int healAmount)
        {
            base.ApplyHeal(healAmount);
            if (EntityHealth > MinHealthFade)
                bloodTime = BloodDuration;
        }

        public override void OnHealthZero()
        {
            if (DeathByTimer && TimeOutDeathGloc != null && !string.IsNullOrEmpty(TimeOutDeathGloc.Value))
            {
                if (gameManager.DeadPanel != null)
                {
                    Transform titleTransform = gameManager.DeadPanel.transform.Find("Panel/Title");
                    if (titleTransform != null)
                    {
                        var titleText = titleTransform.GetComponent<TMPro.TMP_Text>();
                        if (titleText != null)
                        {
                            titleText.text = TimeOutDeathGloc.Value;
                        }
                    }
                }
            }

            gameManager.ShowPanel(PanelType.DeadPanel);
            gameManager.PlayerPresence.FreezePlayer(true, true);
            gameManager.PlayerPresence.PlayerManager.PlayerItems.DeactivateCurrentItem();
            targetBlood = 1f;
        }
    }
}