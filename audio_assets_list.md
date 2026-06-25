# 🎵 Lista de Recursos de Audio (Audio Assets)

Este documento contiene un catálogo organizado de todos los archivos de audio detectados en el proyecto, clasificados en **Música (Music Tracks)**, **Efectos de Sonido (SFX)** y **Ambiente (Ambience)**.

---

## 🎵 1. Pistas de Música (Music Tracks)
Música de fondo, temas para escenas (cutscenes) y momentos específicos del juego:

*   **`piano-scary-stories.mp3`**
    *   *Ruta:* [piano-scary-stories.mp3](file:///d:/Unity/TallerIntegrador/ProyectoIntegradorVideoJuegos/Assets/AssetsDescargados/IntroCutScene/Music/piano-scary-stories.mp3)
    *   *Uso sugerido:* Música de introducción o escenas narrativas de suspenso.
*   **`Tana Nera.mp3`**
    *   *Ruta:* [Tana Nera.mp3](file:///d:/Unity/TallerIntegrador/ProyectoIntegradorVideoJuegos/Assets/Sounds/Music/Tana Nera.mp3)
    *   *Uso sugerido:* Tema principal, exploración tensa o menú de inicio.
*   **`Zamba del Viento.mp3`**
    *   *Ruta:* [Zamba del Viento.mp3](file:///d:/Unity/TallerIntegrador/ProyectoIntegradorVideoJuegos/Assets/Sounds/Music/Zamba del Viento.mp3)
    *   *Uso sugerido:* Tema melancólico, créditos o zonas seguras de descanso.

---

## 🔊 2. Efectos de Sonido (SFX)
Efectos interactivos clasificados por su funcionalidad.

### 👤 Acciones del Jugador (Player)
*   **Breathing & Stamina:**
    *   `Audio_StaminaBreathing.wav` (Respiración cansada / agitación al correr).
*   **Movement & Footsteps:**
    *   `Land_Grass_01.wav` a `Land_Grass_05.wav` (Aterrizaje en pasto).
    *   `Land_Wood_01.wav` a `Land_Wood_05.wav` (Aterrizaje en madera).
    *   *Pasos del jugador:* Integrados en el sistema UHFPS (pasto, madera, concreto, etc.).
*   **Zipline:**
    *   `zipline_in.wav` / `zipline_out.wav` / `zipline_sliding.wav` (Sonidos de uso y deslizamiento en tirolesa).

### 🛠️ Objetos del Jugador (Player Items / Weapons)
*   **Hacha (Axe):** `Axe_Whoosh.wav` (Ataque al aire).
*   **Cámara de Fotos/Video (Camera):** `Camera_Button.wav`, `Camera_Equip.wav`, `Camera_ZoomIn.wav`, `Camera_ZoomOut.wav`
*   **Vela (Candle):** `blow_candle.wav` (Soplar vela).
*   **Detector EMF:** `EMFRead.wav` (Lectura del detector de actividad paranormal).
*   **Linterna (Flashlight):** `Flashlight_On.wav`, `Flashlight_Off.wav`, `InsertBattery_1.wav`, `InsertBattery_2.wav`, `RemoveBatteries.wav`
*   **Cuchillo (Knife):** `Knife_Flesh.wav` (Impacto en carne), `Knife_Generic1.wav`/`2.wav`, `Knife_Whoosh.wav`, `Knife_Wood.wav`
*   **Farol (Lantern):** `Lantern_Draw.wav`, `Lantern_Hide.wav`, `Lantern_Reload.wav` (Aceite).
*   **Encendedor (Lighter):** `lighter_flick_01.wav`, `lighter_flick_02.wav` (Chispa / encendido).
*   **Pistola (Pistol):** `Pistol_magIn.wav`, `Pistol_magOut.wav`, `Pistol_Shoot.wav`, `Pistol_SlideRelease.wav`

### 🧩 Puzzles e Interacción (Puzzles & Interaction)
*   **Circuitos Eléctricos:** `Circuit_Rotate.wav` (Girar piezas).
*   **Tarjetas de Acceso:** `Keycard_Denied.wav` (Denegado), `Keycard_Granted.wav` (Aceptado).
*   **Teclado Numérico (Keypad):** `EnterCode.wav`, `Negative Beep.wav`, `Positive Beep.wav`
*   **Ganzúas (Lockpick):** `DoorUnlock_01.wav`, `DoorUnlock_02.wav`, `LockpickDrop_01.wav` a `03.wav`
*   **Caja Fuerte / Candados (Safe):** `PadlockUnlock.wav`, `SafeUnlock.wav`, `Turn1.wav` a `Turn3.wav` (Girar dial).

### 👻 Terror, Sustos y Enemigos (Horror, Jumpscares & NPCs)
*   **Jumpscares (Sustos repentinos):**
    *   `EvilClownLaugh.wav` (Risa siniestra de payaso).
    *   `Jumpscare_01.wav` al `Jumpscare_14.wav` (Golpes de audio rápidos y chillidos).
    *   `Jumpscare_Monster.wav` (Rugido/grito repentino de monstruo).
*   **Enemigos / NPCs:**
    *   `monster-roar.wav` (Rugido de monstruo en la distancia o combate).
    *   *Voces de alerta/persecución:* `ahi esta.wav`, `atrapenlo.wav`, `el intruso.wav`, `un intruso.wav`.
    *   *Risas de niños en el bosque:* `horror_evil_child_laugh_reversed_reverb.mp3`, `zapsplat_human_child_boy_9_years_old_laugh_hysterical_26598.mp3`.
    *   *Llantos:* `mujer_llanto.mp3` (Llanto de mujer en el pueblo).

### 🚪 Entorno y Props (Environment Props)
*   **Puertas y Rejas:**
    *   `403687__dbkeebler__sfx-single-door-bang.wav` (Portazo fuerte).
    *   `623701__mediatheksuche__baging_on_rattling_door_4x.wav` (Golpeteo y forcejeo de puerta).
    *   `reja1.mp3`, `reja2.mp3` (Metálico de rejas).
    *   `OpenRetractable.wav`, `CloseRetractable.wav` (Puertas retráctiles).
*   **Objetos Físicos del Escenario:**
    *   `columpio.wav` (Columpio de metal rechinando).
    *   `freesound_community-calesita-sonido-metal-39628.mp3` (Calesita metálica girando).
    *   `mollyroselee-falling-tree-ai-generated-431321.mp3` (Árbol cayendo).
    *   `ArmChairWood 1.wav` / `Audio_SofaDrop.wav` (Crujido de madera / caída de mueble pesado).
    *   `caida_frutas.mp3` (Frutas rodando o cayendo).
*   **Interacciones Generales:**
    *   `pickup-.mp3` (Recoger objeto / ítem).
    *   `glug-glug-glug-39140.mp3` (Beber o consumir líquido).
    *   `wood-smash-3-170418.mp3` (Romper o golpear madera).

### 📊 Interfaz (UI) & Sistema
*   **Misiones:**
    *   `new msion.mp3` / `new msion.wav` (Sonido al recibir nueva misión).
    *   `complete mision.mp3` / `complete mision.wav` (Sonido al completar misión).
*   **Sistema:**
    *   `guardado.mp3` (Confirmación acústica al guardar partida).

---

## 🍃 3. Ambientes (Ambience)
Sonidos continuos en bucle (loop) para definir la atmósfera de cada zona del juego:

*   **Bosque / Exterior Nocturno:**
    *   `NightAmbience.wav` (Grillos y viento suave nocturno en el bosque).
    *   `RainingAmbience.wav` (Lluvia constante).
*   **Pueblo / Zonas de Tensión:**
    *   `EerieAmbience.wav` (Ambiente tenso, zumbidos y suspenso general).
    *   `Atmosphere_010_Soft(SINGLE LOOP).wav` (Ambiente misterioso y sutil).
    *   `freesound_community-nightmare-sequence-23188.mp3` (Secuencia de pesadilla para momentos surrealistas).
*   **Mecánicas de Salud, Tensión y Síntomas (Sintomas):**
    *   `heartbeat.mp3` / `latido.mp3` (Latidos del corazón acelerados por pánico o salud baja).
    *   `drunk.mp3` (Filtro sonoro aturdido / mareado).
    *   `white-noise.mp3` / `creepy-distortion.mp3` / `vhs.mp3` / `whisper.mp3` (Distorsiones, estática de televisión, filtros VHS y susurros fantasmales que aumentan con la locura o eventos paranormales).
    *   `suspiro.mp3` / `Mucho mejor.mp3` (Sonidos de alivio al recuperarse).
