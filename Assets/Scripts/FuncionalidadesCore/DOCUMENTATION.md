# FuncionalidadesCore - Documentación Completa

> Sistema de funcionalidades core desacopladas del framework UHFPS de ThunderWire Studio.
> Cada sistema funciona de forma independiente sin depender de UI, modelos 3D, o assets propietarios.

---

## Tabla de Contenidos

1. [Arquitectura General](#arquitectura-general)
2. [Core (Base)](#1-core-base)
3. [Health (Sistema de Salud)](#2-health-sistema-de-salud)
4. [SaveGame (Guardado/Carga)](#3-savegame-guardadocarga)
5. [StateMachine (Máquina de Estados)](#4-statemachine-máquina-de-estados)
6. [Objectives (Objetivos)](#5-objectives-objetivos)
7. [Inventory (Inventario)](#6-inventory-inventario)
8. [Input (Entrada)](#7-input-entrada)
9. [Options (Opciones)](#8-options-opciones)
10. [Interaction (Interacción)](#9-interaction-interacción)
11. [Triggers](#10-triggers)
12. [Utilities](#11-utilities)
13. [Abstractions (Interfaces)](#12-abstractions)
14. [FirstPerson (Player Controller)](#13-firstperson-player-controller)

---

## Arquitectura General

```
Assets/FuncionalidadesCore/
├── Abstractions/     ← Interfaces de abstracción (IGameContext, IInputProvider, etc.)
├── Core/             ← Clases base (Singleton, StorableCollection, etc.)
├── Health/           ← Sistema de salud (daño, curación, muerte)
├── SaveGame/         ← Sistema de guardado/carga con JSON + AES
├── StateMachine/     ← FSM genérica para jugador y AI
├── Objectives/       ← Sistema de objetivos con estados
├── Inventory/        ← Inventario lógico (items, stack, combine)
├── Input/            ← Wrapper de Unity Input System
├── Options/          ← Sistema de opciones con Observer pattern
├── Interaction/      ← Interfaces para interacción en el mundo
├── Triggers/         ← Componentes de trigger (daño, eventos)
├── Utilities/        ← Structs y herramientas auxiliares
└── FirstPerson/      ← Controlador de jugador y cámara 1ra persona
```

**Principio de diseño**: Cada sistema expone **interfaces** para que tu juego implemente la parte visual/concreta. La lógica pura siempre está en las clases `*Core`. Excepto `FirstPerson/`, que es una implementación completa y lista para usar.

---

## 1. Core (Base)

### [Singleton.cs](file:///D:/Unity/Proyects/lineaf-horrorgame/Assets/FuncionalidadesCore/Core/Singleton.cs)

**¿Qué hace?** Patrón Singleton genérico para MonoBehaviours. Garantiza una única instancia global.

**Cómo usar:**
```csharp
// Tu manager hereda de Singleton<T>
public class AudioManager : Singleton<AudioManager>
{
    public void PlaySound(string clipName) { /* ... */ }
}

// Acceder desde cualquier script:
AudioManager.Instance.PlaySound("explosion");

// Verificar si existe sin crear:
if (AudioManager.HasReference)
{
    AudioManager.Instance.PlaySound("click");
}
```

**Cómo agregar UI:** No aplica, es una utilidad base.

---

### [StorableCollection.cs](file:///D:/Unity/Proyects/lineaf-horrorgame/Assets/FuncionalidadesCore/Core/StorableCollection.cs)

**¿Qué hace?** Diccionario tipado `Dictionary<string, object>` con métodos de acceso seguro. Se usa para transferir datos entre sistemas (save/load, estados, etc.).

**Cómo usar:**
```csharp
// Guardar datos
var data = new StorableCollection
{
    { "health", 100 },
    { "position", transform.position.ToSaveable() },
    { "isAlive", true }
};

// Leer datos con tipo seguro
int health = data.Get<int>("health");

// Leer con verificación
if (data.TryGetValue<bool>("isAlive", out bool alive))
{
    Debug.Log($"Is alive: {alive}");
}
```

---

### [ManagerModuleCore.cs](file:///D:/Unity/Proyects/lineaf-horrorgame/Assets/FuncionalidadesCore/Core/ManagerModuleCore.cs)

**¿Qué hace?** Base para módulos extensibles que se agregan al Game Manager. Cada módulo tiene su propio lifecycle (Awake/Start/Update).

**Cómo usar:**
```csharp
// Crear un módulo personalizado
[System.Serializable]
public class WeatherModule : ManagerModuleCore
{
    public float rainIntensity;
    
    public override void OnAwake()
    {
        // Inicialización
    }
    
    public override void OnUpdate()
    {
        // Lógica de clima cada frame
        if (rainIntensity > 0.5f)
            GameContext.ShowHintMessage("It's raining heavily!", 2f);
    }
}
```

**Cómo agregar UI:**
```csharp
// En el módulo, acceder al GameContext para mostrar mensajes
public override void OnStart()
{
    // El GameContext expone métodos de UI genéricos
    GameContext.ShowHintMessage("Weather system initialized", 3f);
}
```

---

### [PlayerComponentCore.cs](file:///D:/Unity/Proyects/lineaf-horrorgame/Assets/FuncionalidadesCore/Core/PlayerComponentCore.cs)

**¿Qué hace?** Base para componentes que van en el GameObject del jugador. Permite habilitar/deshabilitar lógicamente.

**Cómo usar:**
```csharp
public class PlayerStamina : PlayerComponentCore
{
    private float stamina = 1f;
    
    void Update()
    {
        if (!IsEnabled) return;
        
        // Lógica de stamina solo cuando está habilitado
        stamina = Mathf.MoveTowards(stamina, 1f, Time.deltaTime * 0.5f);
    }
}

// Desde otro script:
playerStamina.SetEnabled(false); // Deshabilita la stamina
```

---

## 2. Health (Sistema de Salud)

### [HealthInterfaces.cs](file:///D:/Unity/Proyects/lineaf-horrorgame/Assets/FuncionalidadesCore/Health/HealthInterfaces.cs)

**Interfaces:**
- `IDamagable` — Entidades que pueden recibir daño
- `IHealable` — Entidades que pueden ser curadas
- `IHealthEntity` — Combina ambas + salud actual/máxima
- `IBreakableEntity` — Objetos destructibles

### [BaseHealthEntity.cs](file:///D:/Unity/Proyects/lineaf-horrorgame/Assets/FuncionalidadesCore/Health/BaseHealthEntity.cs)

**¿Qué hace?** Clase base completa para entidades con sistema de salud. Maneja daño, curación, muerte, y expone callbacks virtuales para UI.

**Cómo usar (solo lógica):**
```csharp
// Entidad enemiga simple
public class EnemyHealth : BaseHealthEntity
{
    protected override void OnHealthZero()
    {
        Debug.Log("Enemy died!");
        Destroy(gameObject);
    }
}
```

**Cómo agregar UI (barra de vida, efectos visuales):**
```csharp
public class PlayerHealthWithUI : BaseHealthEntity
{
    [Header("UI References")]
    [SerializeField] private UnityEngine.UI.Slider healthBar;
    [SerializeField] private UnityEngine.UI.Image damageFlash;
    [SerializeField] private TMPro.TextMeshProUGUI healthText;
    [SerializeField] private CanvasGroup deathScreen;
    [SerializeField] private AudioSource heartbeatAudio;
    
    protected override void OnHealthChanged(int oldHealth, int newHealth)
    {
        // Actualizar barra de vida
        if (healthBar != null)
            healthBar.value = HealthPercent;
        
        // Actualizar texto
        if (healthText != null)
            healthText.text = $"{newHealth}/{MaxEntityHealth}";
        
        // Flash rojo al recibir daño
        if (newHealth < oldHealth && damageFlash != null)
        {
            damageFlash.color = new Color(1, 0, 0, 0.3f);
            StartCoroutine(FadeDamageFlash());
        }
        
        // Heartbeat cuando la vida está baja
        if (HealthPercent < 0.3f && heartbeatAudio != null)
        {
            heartbeatAudio.volume = 1f - HealthPercent;
            if (!heartbeatAudio.isPlaying) heartbeatAudio.Play();
        }
        else if (heartbeatAudio != null)
        {
            heartbeatAudio.Stop();
        }
    }
    
    protected override void OnHealthZero()
    {
        // Mostrar pantalla de muerte
        if (deathScreen != null)
        {
            deathScreen.alpha = 1;
            deathScreen.interactable = true;
            deathScreen.blocksRaycasts = true;
        }
    }
    
    protected override void OnHealthMax()
    {
        if (heartbeatAudio != null) heartbeatAudio.Stop();
    }
    
    private System.Collections.IEnumerator FadeDamageFlash()
    {
        while (damageFlash.color.a > 0)
        {
            var c = damageFlash.color;
            c.a -= Time.deltaTime * 2f;
            damageFlash.color = c;
            yield return null;
        }
    }
}
```

**Cómo causar daño desde otro script:**
```csharp
// Desde un arma, proyectil, trampa, etc.
if (target.TryGetComponent<IDamagable>(out var damagable))
{
    damagable.OnApplyDamage(25, transform);
}

// Matar instantáneamente
damagable.ApplyDamageMax();

// Curar
if (target.TryGetComponent<IHealable>(out var healable))
{
    healable.OnApplyHeal(50);
}
```

---

## 3. SaveGame (Guardado/Carga)

### [SaveInterfaces.cs](file:///D:/Unity/Proyects/lineaf-horrorgame/Assets/FuncionalidadesCore/SaveGame/SaveInterfaces.cs)

**Interfaces:**
- `ISaveable` — Para objetos que guardan/cargan estado
- `IRuntimeSaveable` — Para objetos instanciados en runtime
- `ISaveableCustom` — Guardado personalizado adicional

### [SaveGameCore.cs](file:///D:/Unity/Proyects/lineaf-horrorgame/Assets/FuncionalidadesCore/SaveGame/SaveGameCore.cs)

**¿Qué hace?** Motor de serialización JSON con encriptación AES opcional. Maneja archivos de guardado.

**Cómo usar:**
```csharp
// Crear instancia del sistema de guardado
var saveSystem = new SaveGameCore(
    saveFolderPath: Application.persistentDataPath + "/Saves",
    encryptionKey: "1234567890123456", // 16 chars para AES-128
    useEncryption: true
);

// Guardar datos
var gameState = new Dictionary<string, object>
{
    { "level", 3 },
    { "score", 15000 },
    { "playerPosition", transform.position.ToSaveable() }
};
await saveSystem.SaveAsync("slot1.json", gameState);

// Cargar datos
var loaded = await saveSystem.LoadAsync<Dictionary<string, object>>("slot1.json");

// Verificar y eliminar
if (saveSystem.SaveExists("slot1.json"))
    saveSystem.DeleteSave("slot1.json");
```

**Cómo hacer un objeto saveable:**
```csharp
public class Door : MonoBehaviour, ISaveable
{
    private bool isOpen;
    
    public StorableCollection OnSave()
    {
        return new StorableCollection
        {
            { "isOpen", isOpen },
            { "rotation", transform.eulerAngles.ToSaveable() }
        };
    }
    
    public void OnLoad(JToken data)
    {
        isOpen = data["isOpen"].ToObject<bool>();
        Vector3 rotation = data["rotation"].ToObject<Vector3>();
        transform.eulerAngles = rotation;
    }
}
```

**Cómo agregar UI de guardado:**
```csharp
public class SaveUI : MonoBehaviour
{
    [SerializeField] private GameObject savingIcon;
    [SerializeField] private TMPro.TextMeshProUGUI saveStatus;
    
    public async void OnSaveButton()
    {
        savingIcon.SetActive(true);
        saveStatus.text = "Saving...";
        
        await saveSystem.SaveAsync("autosave.json", CollectGameState());
        
        savingIcon.SetActive(false);
        saveStatus.text = "Game Saved!";
        StartCoroutine(HideStatusAfter(2f));
    }
}
```

---

## 4. StateMachine (Máquina de Estados)

### [StateMachineCore.cs](file:///D:/Unity/Proyects/lineaf-horrorgame/Assets/FuncionalidadesCore/StateMachine/StateMachineCore.cs)

**¿Qué hace?** FSM genérica con estados, transiciones condicionales, y lifecycle completo. Sirve tanto para jugador como para AI.

**Cómo usar (Player FSM):**
```csharp
// 1. Crear los estados
public class IdleState : FSMStateBase
{
    private PlayerController player;
    
    public IdleState(PlayerController player) { this.player = player; }
    
    public override void OnStateEnter()
    {
        player.Animator.SetBool("IsMoving", false);
    }
    
    public override void OnStateUpdate()
    {
        // Lógica de idle
    }
}

public class WalkState : FSMStateBase
{
    private PlayerController player;
    
    public WalkState(PlayerController player) { this.player = player; }
    
    public override void OnStateEnter()
    {
        player.Animator.SetBool("IsMoving", true);
    }
    
    public override void OnStateUpdate()
    {
        // Aplicar movimiento
        player.Move(player.InputDirection * player.WalkSpeed);
    }
}

// 2. Crear el controller con la StateMachine
public class PlayerController : StateMachineCore
{
    public Animator Animator;
    public float WalkSpeed = 3f;
    public Vector3 InputDirection;
    
    private void Awake()
    {
        // Registrar estados
        var idle = new IdleState(this);
        var walk = new WalkState(this);
        
        // Definir transiciones
        idle.Transitions = new List<StateTransition>
        {
            new("Walk", () => InputDirection.magnitude > 0.1f)
        };
        
        walk.Transitions = new List<StateTransition>
        {
            new("Idle", () => InputDirection.magnitude < 0.1f)
        };
        
        RegisterState("Idle", idle);
        RegisterState("Walk", walk);
        SetInitialState("Idle");
    }
    
    private void Update()
    {
        // Leer input
        InputDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        
        // Actualizar la máquina de estados
        UpdateStateMachine();
    }
    
    public void Move(Vector3 motion)
    {
        transform.Translate(motion * Time.deltaTime);
    }
}
```

**Cómo agregar UI de estado:**
```csharp
public class StateDebugUI : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    [SerializeField] private TMPro.TextMeshProUGUI stateText;
    
    void Start()
    {
        player.OnStateChanged += (stateName) =>
        {
            stateText.text = $"State: {stateName}";
        };
    }
}
```

**Cómo usar para AI:**
```csharp
public class EnemyAI : StateMachineCore
{
    private INavigationAgent navigator;
    private ITargetProvider target;
    
    private void Awake()
    {
        var patrol = new PatrolState(this);
        var chase = new ChaseState(this);
        var attack = new AttackState(this);
        
        patrol.Transitions = new List<StateTransition>
        {
            new("Chase", () => target.DistanceToTarget(transform.position) < 10f)
        };
        
        chase.Transitions = new List<StateTransition>
        {
            new("Attack", () => target.DistanceToTarget(transform.position) < 2f),
            new("Patrol", () => target.DistanceToTarget(transform.position) > 20f)
        };
        
        RegisterState("Patrol", patrol);
        RegisterState("Chase", chase);
        RegisterState("Attack", attack);
        SetInitialState("Patrol");
    }
    
    private void Update() => UpdateStateMachine();
}
```

---

## 5. Objectives (Objetivos)

### [ObjectiveManagerCore.cs](file:///D:/Unity/Proyects/lineaf-horrorgame/Assets/FuncionalidadesCore/Objectives/ObjectiveManagerCore.cs)

**¿Qué hace?** Gestiona objetivos y sub-objetivos con estados. La UI se inyecta via `IObjectiveDisplay`.

**Cómo usar:**
```csharp
// Desde cualquier script
objectiveManager.AddObjective("find_key");
objectiveManager.CompleteSubObjective("find_key", "search_room", 1);
objectiveManager.CompleteObjective("find_key");

// Verificar estado
if (objectiveManager.IsObjectiveCompleted("find_key"))
    OpenDoor();
```

**Cómo agregar UI de objetivos:**
```csharp
public class ObjectiveUI : MonoBehaviour, IObjectiveDisplay
{
    [SerializeField] private Transform objectiveListParent;
    [SerializeField] private GameObject objectivePrefab;
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private TMPro.TextMeshProUGUI notificationText;
    
    private Dictionary<string, GameObject> objectiveElements = new();
    
    public void OnObjectiveAdded(string key, string title)
    {
        var go = Instantiate(objectivePrefab, objectiveListParent);
        go.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = title;
        objectiveElements[key] = go;
    }
    
    public void OnObjectiveCompleted(string key)
    {
        if (objectiveElements.TryGetValue(key, out var go))
        {
            // Tachar el texto o cambiar color
            var text = go.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            text.fontStyle = TMPro.FontStyles.Strikethrough;
            text.color = Color.green;
        }
    }
    
    public void ShowNotification(string title, bool isCompleted)
    {
        notificationText.text = isCompleted ? $"✓ {title}" : $"New: {title}";
        notificationPanel.SetActive(true);
        StartCoroutine(HideAfter(3f));
    }
    
    // ... implementar los demás métodos de IObjectiveDisplay
    public void OnObjectiveDiscarded(string key) { /* ... */ }
    public void OnSubObjectiveAdded(string objKey, string subKey, string text) { /* ... */ }
    public void OnSubObjectiveCompleted(string objKey, string subKey) { /* ... */ }
    public void OnSubObjectiveCountChanged(string objKey, string subKey, ushort current, ushort required) { /* ... */ }
    
    private System.Collections.IEnumerator HideAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        notificationPanel.SetActive(false);
    }
}

// Conectar en el setup del juego:
objectiveManager.SetDisplay(objectiveUI);
```

---

## 6. Inventory (Inventario)

### [InventoryCore.cs](file:///D:/Unity/Proyects/lineaf-horrorgame/Assets/FuncionalidadesCore/Inventory/InventoryCore.cs)

**¿Qué hace?** Lógica completa de inventario: agregar/remover items, stacking, crafting, combinación. Sin grid UI.

### Componentes Listos Para Usar (Carpeta Components/)
Para facilitar la integración en Unity sin tener que programar de cero, el paquete incluye scripts "Plug and Play" en `Assets/FuncionalidadesCore/Inventory/Components/`:
- **`InventoryDatabase`**: Ponlo en un objeto vacío. Te permite configurar todos tus items visualmente desde el Inspector de Unity e inicializa el Core automáticamente.
- **`InventoryItemPickup`**: Ponlo en los modelos 3D del mundo. Permite interactuar con ellos ("Pulsar E") para agregarlos al inventario y destruir el modelo 3D.
- **`InventoryDemoUI`**: Plantilla base para tu menú que se suscribe automáticamente a los eventos de crear/destruir slots.

**Cómo usar la lógica pura (Core):**
```csharp
// Configurar base de datos de items
inventory.SetItemDatabase(myItemsList);

// Agregar items
inventory.AddItem("potion_health", 3);
inventory.AddItem("key_dungeon", 1);

// Verificar y usar
if (inventory.HasItem("key_dungeon"))
{
    inventory.UseItem("key_dungeon");
    OpenDungeonDoor();
}

// Combinar
inventory.CombineItems("herb_red", "herb_blue"); // resultado según CombineSettings

// Escuchar eventos
inventory.OnItemAdded += (guid, qty) => Debug.Log($"Added {qty}x {guid}");
inventory.OnItemUsed += (guid) => Debug.Log($"Used {guid}");
```

**Cómo agregar UI de inventario (grid, slots, etc.):**
```csharp
public class InventoryUI : MonoBehaviour
{
    [SerializeField] private InventoryCore inventory;
    [SerializeField] private Transform gridParent;
    [SerializeField] private GameObject slotPrefab;
    
    private Dictionary<string, GameObject> slotElements = new();
    
    void Start()
    {
        inventory.OnItemAdded += OnItemAdded;
        inventory.OnItemRemoved += OnItemRemoved;
        
        // Crear slots iniciales
        foreach (var slot in inventory.Slots)
            CreateSlotUI(slot);
    }
    
    private void OnItemAdded(string guid, ushort qty)
    {
        // Si el slot existe, actualizar cantidad
        if (slotElements.TryGetValue(guid, out var go))
        {
            var text = go.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            text.text = inventory.GetItemQuantity(guid).ToString();
        }
        else
        {
            // Crear nuevo slot visual
            var slot = inventory.Slots.First(s => s.Item.GUID == guid);
            CreateSlotUI(slot);
        }
    }
    
    private void CreateSlotUI(InventorySlot slot)
    {
        var go = Instantiate(slotPrefab, gridParent);
        go.GetComponent<UnityEngine.UI.Image>().sprite = slot.Item.Icon;
        go.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = slot.Quantity.ToString();
        
        // Click para usar
        go.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() =>
        {
            inventory.UseItem(slot.Item.GUID);
        });
        
        slotElements[slot.Item.GUID] = go;
    }
    
    private void OnItemRemoved(string guid, ushort qty)
    {
        if (inventory.GetItemQuantity(guid) <= 0 && slotElements.TryGetValue(guid, out var go))
        {
            Destroy(go);
            slotElements.Remove(guid);
        }
    }
}
```

---

## 7. Input (Entrada)

### [InputManagerCore.cs](file:///D:/Unity/Proyects/lineaf-horrorgame/Assets/FuncionalidadesCore/Input/InputManagerCore.cs)

**¿Qué hace?** Wrapper de Unity Input System con acciones cacheadas, botones toggle, botones once (no repiten while held).

**Cómo usar:**
```csharp
// Leer movimiento
Vector2 movement = InputManagerCore.Instance.ReadInput<Vector2>(Controls.MOVEMENT);

// Leer botón
if (InputManagerCore.Instance.ReadButton(Controls.JUMP))
    Jump();

// Botón una sola vez (no repite)
if (InputManagerCore.Instance.ReadButtonOnce("player", Controls.INTERACT))
    Interact();

// Botón toggle
bool isSprinting = InputManagerCore.Instance.ReadButtonToggle("player", Controls.SPRINT);

// Suscribirse a evento performed
InputManagerCore.Instance.Performed(Controls.FIRE, ctx =>
{
    Shoot();
});
```

**A través de la interfaz (más desacoplado):**
```csharp
public class MyComponent : MonoBehaviour
{
    private IInputProvider input;
    
    void Start()
    {
        input = InputManagerCore.Instance; // O inyectado
    }
    
    void Update()
    {
        if (input.ReadButton(Controls.JUMP))
            Jump();
    }
}
```

---

## 8. Options (Opciones)

### [OptionsManagerCore.cs](file:///D:/Unity/Proyects/lineaf-horrorgame/Assets/FuncionalidadesCore/Options/OptionsManagerCore.cs)

**¿Qué hace?** Sistema de opciones con patrón Observer (BehaviorSubject). Desacoplado de URP y UI concreta.

**Cómo usar:**
```csharp
// Inicializar con implementaciones
optionsManager.Initialize(graphicsApplier, optionsPersistence);

// Registrar opciones
optionsManager.RegisterOption("master_volume", 1.0f);
optionsManager.RegisterOption("sfx_volume", 0.8f);
optionsManager.RegisterOption("shadows_quality", 2);

// Observar cambios
optionsManager.ObserveOption("master_volume", volume =>
{
    AudioListener.volume = (float)volume;
});

// Cambiar valor
optionsManager.SetOptionValue("master_volume", 0.5f);

// Guardar
optionsManager.ApplyOptions();
```

**Cómo agregar UI de opciones:**
```csharp
public class OptionsUI : MonoBehaviour
{
    [SerializeField] private OptionsManagerCore options;
    [SerializeField] private UnityEngine.UI.Slider volumeSlider;
    [SerializeField] private UnityEngine.UI.Toggle fullscreenToggle;
    
    void Start()
    {
        // Sincronizar UI con valores actuales
        volumeSlider.value = options.GetOptionValue<float>("master_volume", 1f);
        
        // Escuchar cambios del slider
        volumeSlider.onValueChanged.AddListener(val =>
        {
            options.SetOptionValue("master_volume", val);
        });
        
        // Implementar IGraphicsOptionsApplier
        options.Initialize(new MyGraphicsApplier(), new MyPersistence());
    }
    
    public void OnApplyButton() => options.ApplyOptions();
    public void OnDiscardButton() => options.DiscardChanges();
}

// Implementar las interfaces según tu pipeline de rendering
public class MyGraphicsApplier : IGraphicsOptionsApplier
{
    public void ApplyResolution(int w, int h, FullScreenMode mode)
        => Screen.SetResolution(w, h, mode);
    
    public void ApplyQualitySetting(string name, object value) { /* URP/HDRP */ }
    public void ApplyAntiAliasing(int level) => QualitySettings.antiAliasing = level;
    public void ApplyVSync(bool enabled) => QualitySettings.vSyncCount = enabled ? 1 : 0;
}
```

---

## 9. Interaction (Interacción)

### [InteractionInterfaces.cs](file:///D:/Unity/Proyects/lineaf-horrorgame/Assets/FuncionalidadesCore/Interaction/InteractionInterfaces.cs)

**¿Qué hace?** Define contratos para interacción, examinación, y arrastre de objetos.

**Cómo usar:**
```csharp
// Objeto interactable simple
public class PickupItem : MonoBehaviour, IInteractStart, IInteractInfo
{
    public string InteractTitle => "Pick up Key";
    
    public bool CanInteract() => true;
    
    public void InteractStart()
    {
        // Lógica de recoger
        inventory.AddItem("key", 1);
        Destroy(gameObject);
    }
}

// Objeto con interacción temporizada (progress bar)
public class Lockpick : MonoBehaviour, IInteractTimed
{
    public float InteractTime => 3f; // 3 segundos
    
    public void InteractStartTimed()
    {
        // Se llama cuando completa el tiempo
        UnlockDoor();
    }
}

// Detector de interacción (raycaster)
public class InteractionDetector : MonoBehaviour
{
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private TMPro.TextMeshProUGUI interactPrompt;
    
    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit, interactDistance))
        {
            if (hit.collider.TryGetComponent<IInteractStart>(out var interactable))
            {
                // Mostrar prompt
                if (hit.collider.TryGetComponent<IInteractInfo>(out var info))
                    interactPrompt.text = $"[E] {info.InteractTitle}";
                
                if (Input.GetKeyDown(KeyCode.E) && interactable.CanInteract())
                    interactable.InteractStart();
            }
        }
    }
}
```

---

## 10. Triggers

### [TriggerComponents.cs](file:///D:/Unity/Proyects/lineaf-horrorgame/Assets/FuncionalidadesCore/Triggers/TriggerComponents.cs)

**Cómo usar DamageTriggerCore:**
1. Agregar a un GameObject con Collider (marcado como Trigger)
2. Configurar damage amount, mode (OnEnter/OnStay), interval
3. Cualquier objeto con `IDamagable` que entre recibirá el daño

**Cómo usar TriggerEventsCore:**
1. Agregar a un GameObject con Collider (marcado como Trigger)
2. Configurar layer mask y opciones (triggerOnce, etc.)
3. Conectar UnityEvents en el Inspector

---

## 11. Utilities

### Structs

| Struct | Archivo | Uso |
|---|---|---|
| `MinMax` / `MinMaxInt` | [MinMax.cs](file:///D:/Unity/Proyects/lineaf-horrorgame/Assets/FuncionalidadesCore/Utilities/Structs/MinMax.cs) | Rangos con conversión a Vector2 |
| `Layer` / `Tag` / `SoundClip` | [CommonStructs.cs](file:///D:/Unity/Proyects/lineaf-horrorgame/Assets/FuncionalidadesCore/Utilities/Structs/CommonStructs.cs) | Wrappers de Unity |
| `ObservableValue<T>` | [ObservableValue.cs](file:///D:/Unity/Proyects/lineaf-horrorgame/Assets/FuncionalidadesCore/Utilities/Structs/ObservableValue.cs) | Detección de cambios |
| `UniqueID` | [UniqueID.cs](file:///D:/Unity/Proyects/lineaf-horrorgame/Assets/FuncionalidadesCore/Utilities/Structs/UniqueID.cs) | IDs para save/load |

### Tools

| Tool | Archivo | Uso |
|---|---|---|
| `CanvasGroupFader` | [UITools.cs](file:///D:/Unity/Proyects/lineaf-horrorgame/Assets/FuncionalidadesCore/Utilities/Tools/UITools.cs) | Fade in/out de CanvasGroup |
| `CoroutineRunner` | UITools.cs | Coroutines temporales auto-cleanup |

---

## 12. Abstractions

Las interfaces de abstracción son el corazón del desacoplamiento. Implementar cada una para tu proyecto:

| Interfaz | Archivo | Abstrae |
|---|---|---|
| `IGameContext` | [IGameContext.cs](file:///D:/Unity/Proyects/lineaf-horrorgame/Assets/FuncionalidadesCore/Abstractions/IGameContext.cs) | GameManager |
| `IInputProvider` | [IInputProvider.cs](file:///D:/Unity/Proyects/lineaf-horrorgame/Assets/FuncionalidadesCore/Abstractions/IInputProvider.cs) | InputManager |
| `IInventoryData` | [IInventoryData.cs](file:///D:/Unity/Proyects/lineaf-horrorgame/Assets/FuncionalidadesCore/Abstractions/IInventoryData.cs) | Inventory |
| `IObjectiveDisplay` | [IObjectiveDisplay.cs](file:///D:/Unity/Proyects/lineaf-horrorgame/Assets/FuncionalidadesCore/Abstractions/IObjectiveDisplay.cs) | UI de objetivos |
| `INavigationAgent` | [INavigationAgent.cs](file:///D:/Unity/Proyects/lineaf-horrorgame/Assets/FuncionalidadesCore/Abstractions/INavigationAgent.cs) | NavMeshAgent |
| `ITargetProvider` | [ITargetProvider.cs](file:///D:/Unity/Proyects/lineaf-horrorgame/Assets/FuncionalidadesCore/Abstractions/ITargetProvider.cs) | PlayerPresenceManager |
| `IMovementProvider` | [IMovementProvider.cs](file:///D:/Unity/Proyects/lineaf-horrorgame/Assets/FuncionalidadesCore/Abstractions/IMovementProvider.cs) | CharacterController |
| `IGraphicsOptionsApplier` | [IGraphicsOptionsApplier.cs](file:///D:/Unity/Proyects/lineaf-horrorgame/Assets/FuncionalidadesCore/Abstractions/IGraphicsOptionsApplier.cs) | URP Settings |
| `IOptionsPersistence` | IGraphicsOptionsApplier.cs | File I/O de opciones |

---

## 13. FirstPerson (Player Controller)

### Construido usando la StateMachineCore

A diferencia de los demás sistemas que son lógicas abstractas, esta carpeta provee una **implementación completa y lista para usar** de un controlador de primera persona de Survival Horror, similar al del paquete FPS pero totalmente desacoplado.

**Archivos Principales:**
- `FirstPersonController.cs`: Hereda de `StateMachineCore`. Controla las físicas y gravedad.
- `FirstPersonLook.cs`: Controla la cámara (Mouse Look) y el balanceo al caminar (Headbob).
- `FPStates.cs`: Contiene `FPIdleState`, `FPWalkState`, `FPRunState`, y `FPJumpState`.

**Cómo Instalar el Jugador en tu Escena:**
1. Haz click derecho en tu jerarquía y crea un **Empty GameObject**. Nómbralo `Player`.
2. Agrégale el componente nativo **CharacterController** (Height: 2, Radius: 0.4).
3. Agrégale el script **`FirstPersonController`**.
4. Haz click derecho sobre el `Player` y crea una **Camera**. Ponla a una altura (eje Y) de `1.6`.
5. Agrégale a la cámara el script **`FirstPersonLook`**.
6. En el inspector de la cámara, arrastra tu objeto `Player` al campo **Player Body**. Draggea la misma cámara al campo **Camera Transform**, y el controlador al campo **Controller**.
7. ¡Dale Play! Tienes un personaje funcional con físicas, gravedad, y soporte nativo para WASD + Shift + Espacio usando la máquina de estados desacoplada.
