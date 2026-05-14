using UnityEngine;
using UHFPS.Scriptable;
using UHFPS.Tools; // Para las transiciones y utilidades

namespace UHFPS.Runtime.States
{
    [CreateAssetMenu(fileName = "EstadoPersecucionAI", menuName = "UHFPS/AI States/EstadoPersecucionAI")]
    public class EstadoPersecucionAI : AIStateAsset
    {
        [Header("Configuracion de Persecucion")]
        public float velocidadPersecucion = 3.5f;
        [Tooltip("A qué distancia se detiene para golpear/atrapar")]
        public float distanciaDeAtaque = 1.0f;
        
        [Header("Perdida de Vision")]
        [Tooltip("Cuántos segundos busca en el mismo lugar antes de volver a patrullar si te pierde de vista")]
        public float tiempoParaRendirse = 4f;
        [Tooltip("Radio extra por si le pasas muy por la espalda mientras te busca")]
        public float radioDeteccionCercana = 1.5f;

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Fuerza a la ventana de escena de Unity a redibujarse cuando cambias 
            // tus valores en el asset (para que veas crecer/achicarse las esferas al instante)
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this != null) UnityEditor.SceneView.RepaintAll();
            };
        }
#endif

        // Inicializador
        public override FSMAIState InitState(NPCStateMachine machine, AIStatesGroup group)
        {
            return new EstadoPersecucionAI_State(machine, this, group);
        }

        public override string StateKey => "PersecucionAI";
        public override string Name => "Estado de Persecución Personalizado";

        public class EstadoPersecucionAI_State : FSMAIState
        {
            private EstadoPersecucionAI asset;
            private CustomNPCStateGroup customGroup;

            // ── Audio ──────────────────────────────────────────────────
            // AudioSource principal: PlayOneShot para sonidos cortos (alerta, ataque, pérdida).
            // AudioSource secundario (opcional): loop de tensión durante la persecución.
            private AudioSource audioSourcePrincipal;
            private AudioSource audioSourceLoop;

            // Timer para los sonidos periódicos durante la persecución
            private float timerSonidoPersecucion;
            // ──────────────────────────────────────────────────────────

            private float timerNoVisto;
            private bool atacando;
            private float coolDownAtaque;

            public EstadoPersecucionAI_State(NPCStateMachine machine, EstadoPersecucionAI stateAsset, AIStatesGroup group) : base(machine)
            {
                this.asset = stateAsset;
                this.customGroup = group as CustomNPCStateGroup;
                // Los AudioSources se resuelven en OnStateEnter para evitar el
                // mismo problema de orden de inicialización que tenían los waypoints.
            }

            public override Transition[] OnGetTransitions()
            {
                return new Transition[]
                {
                    // Volver a Patrullar si se rinde buscándote, te escondes en un locker, o mueres
                    Transition.To<EstadoPatrullajeAI>(() => 
                        timerNoVisto > asset.tiempoParaRendirse || 
                        playerMachine.IsCurrent(PlayerStateMachine.HIDING_STATE) || 
                        IsPlayerDead)
                };
            }

            public override void OnStateEnter()
            {
                agent.speed = asset.velocidadPersecucion;
                agent.stoppingDistance = asset.distanciaDeAtaque;
                
                machine.RotateAgentManually = true;
                timerNoVisto = 0f;
                atacando = false;
                coolDownAtaque = 0f;

                // Resolvemos los AudioSources aquí para garantizar que Unity ya los inicializó.
                AudioSource[] sources = machine.GetComponents<AudioSource>();
                if (sources.Length >= 1) audioSourcePrincipal = sources[0];
                if (sources.Length >= 2) audioSourceLoop      = sources[1];
                else                     audioSourceLoop      = sources.Length > 0 ? sources[0] : null;

                if (customGroup != null)
                {
                    // ① Sonido de ALERTA al detectar (one-shot aleatorio)
                    ReproducirOneShot(customGroup.sonidosAlerta, customGroup.volumenAlerta);

                    // ② Iniciar timer para sonidos periódicos
                    timerSonidoPersecucion = Random.Range(customGroup.intervaloSonidoMin, customGroup.intervaloSonidoMax);

                    // ③ Sonido ambiental en LOOP durante la persecución
                    if (customGroup.usarSonidoLoop && customGroup.sonidoLoop != null && audioSourceLoop != null)
                    {
                        audioSourceLoop.clip   = customGroup.sonidoLoop;
                        audioSourceLoop.loop   = true;
                        audioSourceLoop.volume = customGroup.volumenLoop;
                        audioSourceLoop.Play();
                    }
                }

                // Empezamos la animación de carrera
                UpdateAnimator(isWalking: false, isRunning: true, isIdle: false);
            }

            public override void OnStateExit()
            {
                machine.RotateAgentManually = false;
                if (agent.isOnNavMesh) agent.ResetPath();

                // ⑤ Sonido al PERDER al jugador (frustración, renuncia)
                if (customGroup != null)
                    ReproducirOneShot(customGroup.sonidosPerdidaVista, customGroup.volumenPerdidaVista);

                // Detener el loop de tensión
                if (audioSourceLoop != null && audioSourceLoop.isPlaying &&
                    customGroup != null && customGroup.usarSonidoLoop)
                {
                    audioSourceLoop.Stop();
                    audioSourceLoop.loop = false;
                }

                // Limpiamos animaciones y velocidad
                UpdateAnimator(false, false, false); 
                if (animator != null) animator.speed = 1f;
            }

            public override void OnStateUpdate()
            {
                if (IsPlayerDead) return;

                coolDownAtaque -= Time.deltaTime;

                // ② Tick del timer de sonidos periódicos durante la persecución
                if (customGroup != null && customGroup.repetirDurantePersecucion)
                {
                    timerSonidoPersecucion -= Time.deltaTime;
                    if (timerSonidoPersecucion <= 0f)
                    {
                        ReproducirOneShot(customGroup.sonidosPersecucion, customGroup.volumenPersecucion);
                        timerSonidoPersecucion = Random.Range(customGroup.intervaloSonidoMin, customGroup.intervaloSonidoMax);
                    }
                }

                if (SeesPlayerOrClose(asset.radioDeteccionCercana))
                {
                    timerNoVisto = 0f;
                    SetDestination(PlayerPosition);

                    if (InPlayerDistance(asset.distanciaDeAtaque) && coolDownAtaque <= 0f)
                    {
                        agent.isStopped = true;
                        
                        // Si el jugador está en el círculo de sal, nos quedamos parados (no ataca)
                        if (CirculoDeSal.jugadorProtegido)
                        {
                            UpdateAnimator(false, false, true); // Se queda en Idle gruñendo/esperando
                        }
                        else
                        {
                            AtacarJugador();
                            UpdateAnimator(false, false, true); // Idle falso mientras ataca
                        }
                    }
                    else
                    {
                        agent.isStopped = false;
                        UpdateAnimator(false, true, false); // Corriendo
                    }
                }
                else
                {
                    SetDestination(PlayerPosition);
                    
                    if (PathDistanceCompleted()) 
                    {
                        agent.isStopped = true;
                        UpdateAnimator(false, false, true); 
                        timerNoVisto += Time.deltaTime;
                    }
                }
            }

            private void AtacarJugador()
            {
                if (customGroup == null) return;
                
                machine.transform.rotation = Quaternion.Slerp(machine.transform.rotation,
                    Quaternion.LookRotation(PlayerPosition - machine.transform.position), Time.deltaTime * 10f);

                if (!string.IsNullOrEmpty(customGroup.AttackTrigger) && animator != null)
                {
                    animator.SetTrigger(customGroup.AttackTrigger);
                }

                // ④ Sonido de ATAQUE
                ReproducirOneShot(customGroup.sonidosAtaque, customGroup.volumenAtaque);

                if (customGroup.InstakillOnCatch)
                    playerHealth.ApplyDamage(9999, machine.transform);
                else
                    playerHealth.ApplyDamage(customGroup.DamageRange.Random(), machine.transform);

                coolDownAtaque = 2f; 
            }

            // ─── Helper: elige un clip al azar del array y lo reproduce ───────────
            private void ReproducirOneShot(AudioClip[] array, float volumen)
            {
                if (audioSourcePrincipal == null || customGroup == null) return;
                AudioClip clip = customGroup.ObtenerSonidoAleatorio(array);
                if (clip != null)
                    audioSourcePrincipal.PlayOneShot(clip, volumen);
            }

            private void UpdateAnimator(bool isWalking, bool isRunning, bool isIdle)
            {
                if (animator == null || customGroup == null) return;

                // Devolvemos el control 100% nativo a tus estados y transiciones en Unity
                animator.SetBool(customGroup.WalkParameter, isWalking);
                animator.SetBool(customGroup.RunParameter, isRunning);
                animator.SetBool(customGroup.IdleParameter, isIdle);
                
                // Le pasamos la magnitud de velocidad actual al parámetro de Multiplicador que configuraste
                if (agent != null) 
                {
                    animator.SetFloat("Speed", agent.velocity.magnitude);
                }
            }
        }
    }
}
