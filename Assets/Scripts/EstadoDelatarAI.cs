using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UHFPS.Scriptable;
using UHFPS.Tools;
using UnityEngine.AI;

namespace UHFPS.Runtime.States
{
    [CreateAssetMenu(fileName = "EstadoDelatarAI", menuName = "UHFPS/AI States/EstadoDelatarAI")]
    public class EstadoDelatarAI : AIStateAsset
    {
        [Header("Configuracion de Delacion")]
        [Tooltip("Tiempo en segundos que el NPC se queda delatando/gritando antes de volver a patrullar (si no ve al jugador).")]
        public float tiempoDeDelacion = 3.0f;

        [Tooltip("Rango maximo de distancia para alertar a los guardias.")]
        public float rangoDeAlerta = 35.0f;

        [Tooltip("Numero maximo de guardias mas cercanos a alertar.")]
        public int maxGuardiasAAlertar = 2;

        [Tooltip("Tiempo de gracia (segundos) que espera el vigilante sin ver al jugador antes de bajar el brazo y volver a su estado base.")]
        public float tiempoPerdidaVista = 1.5f;

        [Header("Voces del Vigilante")]
        [Tooltip("Sonidos aleatorios que reproduce mientras detecta y apunta al jugador.")]
        public AudioClip[] sonidosVigilante;
        
        [Tooltip("Volumen de reproducción de los sonidos.")]
        [Range(0f, 1f)]
        public float volumenVigilante = 1.0f;

        [Tooltip("Tiempo mínimo en segundos entre reproducciones de voz si sigue viendo al jugador.")]
        public float intervaloVozMin = 4.0f;

        [Tooltip("Tiempo máximo en segundos entre reproducciones de voz si sigue viendo al jugador.")]
        public float intervaloVozMax = 7.0f;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (sonidosVigilante == null || sonidosVigilante.Length == 0)
            {
                string[] paths = new string[]
                {
                    "Assets/Sounds/NPC/ahi esta.wav",
                    "Assets/Sounds/NPC/atrapenlo.wav",
                    "Assets/Sounds/NPC/el intruso.wav",
                    "Assets/Sounds/NPC/un intruso.wav"
                };

                List<AudioClip> clips = new List<AudioClip>();
                foreach (var path in paths)
                {
                    AudioClip clip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                    if (clip != null) clips.Add(clip);
                }

                if (clips.Count > 0)
                {
                    sonidosVigilante = clips.ToArray();
                    UnityEditor.EditorUtility.SetDirty(this);
                }
            }
        }
#endif

        public override FSMAIState InitState(NPCStateMachine machine, AIStatesGroup group)
        {
            return new EstadoDelatarAI_State(machine, this, group);
        }

        public override string StateKey => "DelatarAI";
        public override string Name => "Estado de Delatar al Jugador";

        public class EstadoDelatarAI_State : FSMAIState
        {
            private EstadoDelatarAI asset;
            private CustomNPCStateGroup customGroup;

            private AudioSource audioSourcePrincipal;
            private float timerDelacion;
            private bool alertaEnviada;
            private float timerPerdidaVista;
            private float timerVoz;

            public EstadoDelatarAI_State(NPCStateMachine machine, EstadoDelatarAI stateAsset, AIStatesGroup group) : base(machine)
            {
                this.asset = stateAsset;
                this.customGroup = group as CustomNPCStateGroup;
            }

            public override Transition[] OnGetTransitions()
            {
                return new Transition[]
                {
                    // Si el NPC tiene el estado de Vigilancia Estática, vuelve a él cuando deja de ver al jugador
                    Transition.To<EstadoVigilanciaEstaticaAI>(() =>
                        (IsPlayerDead || timerPerdidaVista <= 0f) && TieneEstado<EstadoVigilanciaEstaticaAI>()),

                    // De lo contrario, vuelve a patrullar (si tiene el estado de Patrullaje)
                    Transition.To<EstadoPatrullajeAI>(() =>
                        (IsPlayerDead || timerPerdidaVista <= 0f) && !TieneEstado<EstadoVigilanciaEstaticaAI>() && TieneEstado<EstadoPatrullajeAI>())
                };
            }

            private bool TieneEstado<T>() where T : AIStateAsset
            {
                if (machine == null || machine.StatesAssetRuntime == null) return false;
                foreach (var stateData in machine.StatesAssetRuntime.AIStates)
                {
                    if (stateData.StateAsset != null && stateData.StateAsset is T && stateData.IsEnabled)
                    {
                        return true;
                    }
                }
                return false;
            }

            private bool NoVeAlPlayer()
            {
                if (playerMachine == null) return true;
                return playerMachine.IsCurrent(PlayerStateMachine.HIDING_STATE) || !SeesPlayer();
            }

            public override void OnStateEnter()
            {
                // Frenar al vigilante
                if (agent != null && agent.isOnNavMesh)
                {
                    agent.isStopped = true;
                    agent.velocity = Vector3.zero;
                }
                
                machine.RotateAgentManually = true;

                timerDelacion = asset.tiempoDeDelacion;
                timerPerdidaVista = asset.tiempoPerdidaVista;
                alertaEnviada = false;

                // AudioSource principal
                AudioSource[] sources = machine.GetComponentsInChildren<AudioSource>();
                if (sources.Length >= 1) audioSourcePrincipal = sources[0];

                if (audioSourcePrincipal == null)
                {
                    audioSourcePrincipal = machine.gameObject.AddComponent<AudioSource>();
                    audioSourcePrincipal.spatialBlend = 1f;
                    audioSourcePrincipal.maxDistance = 20f;
                }

                // Reproducir primer grito de voz de delación inmediatamente
                if (asset.sonidosVigilante != null && asset.sonidosVigilante.Length > 0)
                {
                    ReproducirVozAleatoria();
                    timerVoz = Random.Range(asset.intervaloVozMin, asset.intervaloVozMax);
                }
                else if (customGroup != null)
                {
                    if (customGroup.sonidosAlerta != null && customGroup.sonidosAlerta.Length > 0)
                    {
                        AudioClip clip = customGroup.ObtenerSonidoAleatorio(customGroup.sonidosAlerta);
                        if (clip != null)
                        {
                            audioSourcePrincipal.PlayOneShot(clip, customGroup.volumenAlerta);
                        }
                    }
                }

                if (customGroup != null)
                {
                    // Reset movement animations
                    customGroup.ResetMovementParameters(animator);
                    
                    // Si queremos, podemos activar un parametro en el animator (ej: un trigger o bool)
                    if (!string.IsNullOrEmpty(customGroup.InteractParameter) && animator != null)
                    {
                        animator.SetTrigger(customGroup.InteractParameter);
                    }
                }

                // Activar animación Pointing
                if (animator != null)
                {
                    string pointingParam = customGroup != null && !string.IsNullOrEmpty(customGroup.PointingParameter)
                        ? customGroup.PointingParameter
                        : "Pointing";
                    animator.SetBool(pointingParam, true);
                }

                // Alertar a los guardias mas cercanos
                AlertarGuardiasCercanos();
            }

            public override void OnStateExit()
            {
                machine.RotateAgentManually = false;
                if (agent != null && agent.isOnNavMesh)
                {
                    agent.isStopped = false;
                }

                // Desactivar animación Pointing
                if (animator != null)
                {
                    string pointingParam = customGroup != null && !string.IsNullOrEmpty(customGroup.PointingParameter)
                        ? customGroup.PointingParameter
                        : "Pointing";
                    animator.SetBool(pointingParam, false);
                }
            }

            public override void OnStateUpdate()
            {
                if (IsPlayerDead) return;

                // Contar tiempo de pérdida de vista
                if (NoVeAlPlayer())
                {
                    timerPerdidaVista -= Time.deltaTime;
                }
                else
                {
                    timerPerdidaVista = asset.tiempoPerdidaVista;

                    // Mirar al jugador mientras lo ve
                    Vector3 playerDir = PlayerPosition - machine.transform.position;
                    playerDir.y = 0f; // Evitar rotar en el eje Y/X inclinado
                    if (playerDir.sqrMagnitude > 0.1f)
                    {
                        Quaternion targetRot = Quaternion.LookRotation(playerDir);
                        machine.transform.rotation = Quaternion.Slerp(machine.transform.rotation, targetRot, Time.deltaTime * 5f);
                    }

                    // Sistema de voz repetitiva si sigue viendo al jugador
                    if (asset.sonidosVigilante != null && asset.sonidosVigilante.Length > 0)
                    {
                        timerVoz -= Time.deltaTime;
                        if (timerVoz <= 0f)
                        {
                            ReproducirVozAleatoria();
                            timerVoz = Random.Range(asset.intervaloVozMin, asset.intervaloVozMax);
                        }
                    }
                }
            }

            private void ReproducirVozAleatoria()
            {
                if (audioSourcePrincipal == null || asset.sonidosVigilante == null || asset.sonidosVigilante.Length == 0) return;
                
                AudioClip clip = asset.sonidosVigilante[Random.Range(0, asset.sonidosVigilante.Length)];
                if (clip != null)
                {
                    audioSourcePrincipal.PlayOneShot(clip, asset.volumenVigilante);
                }
            }

            private void AlertarGuardiasCercanos()
            {
                if (alertaEnviada) return;
                alertaEnviada = true;

                // Buscar todos los NPCStateMachine en la escena
                NPCStateMachine[] allNPCs = Object.FindObjectsByType<NPCStateMachine>(FindObjectsSortMode.None);
                if (allNPCs == null || allNPCs.Length == 0) return;

                // Filtrar los que son guardias (tienen el estado PersecucionAI en su StatesAssetRuntime)
                List<KeyValuePair<NPCStateMachine, float>> guardiasYDistancias = new List<KeyValuePair<NPCStateMachine, float>>();

                foreach (var npc in allNPCs)
                {
                    if (npc == machine) continue; // No auto-alertarse
                    if (!npc.gameObject.activeInHierarchy) continue;

                    if (EsGuardia(npc))
                    {
                        float dist = Vector3.Distance(machine.transform.position, npc.transform.position);
                        if (dist <= asset.rangoDeAlerta)
                        {
                            guardiasYDistancias.Add(new KeyValuePair<NPCStateMachine, float>(npc, dist));
                        }
                    }
                }

                // Ordenar por distancia (mas cercano primero)
                guardiasYDistancias.Sort((x, y) => x.Value.CompareTo(y.Value));

                // Alertar hasta el maximo permitido
                int alertadosCount = 0;
                for (int i = 0; i < guardiasYDistancias.Count; i++)
                {
                    if (alertadosCount >= asset.maxGuardiasAAlertar) break;

                    NPCStateMachine guardia = guardiasYDistancias[i].Key;
                    
                    // Forzar el estado de persecucion en el guardia
                    try
                    {
                        // Si el guardia no esta ya en persecucion, lo alertamos
                        if (guardia.CurrentStateKey != "PersecucionAI")
                        {
                            guardia.ChangeState("PersecucionAI");
                            Debug.Log($"[Vigilante: {machine.name}] Alerto al guardia mas cercano: {guardia.name} a {guardiasYDistancias[i].Value:F1} metros.");
                            alertadosCount++;
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"[Vigilante: {machine.name}] Error al cambiar estado de {guardia.name}: {ex.Message}");
                    }
                }
            }

            private bool EsGuardia(NPCStateMachine npc)
            {
                if (npc == null || npc.StatesAssetRuntime == null) return false;
                
                foreach (var stateData in npc.StatesAssetRuntime.AIStates)
                {
                    if (stateData.StateAsset != null && stateData.StateAsset.StateKey == "PersecucionAI")
                    {
                        return true;
                    }
                }
                return false;
            }
        }
    }
}
