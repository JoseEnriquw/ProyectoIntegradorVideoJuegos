using UnityEngine;
using UnityEngine.EventSystems;
using System.IO;
using UHFPS.Runtime;

public static class IntroDebugger
{
    private static string logPath;
    private static GameObject debugGo;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void OnSceneLoaded()
    {
        logPath = Path.Combine(Application.dataPath, "intro_debug_log.txt");
        File.WriteAllText(logPath, $"=== INTRO DEBUGGER LOG START: {System.DateTime.Now} ===\n");
        
        Log("Scene loaded: " + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);

        // Spawn a helper component to log things every frame or on events
        debugGo = new GameObject("IntroDebuggerHelper");
        debugGo.AddComponent<IntroDebuggerHelper>();
        Object.DontDestroyOnLoad(debugGo);
    }

    public static void Log(string message)
    {
        string formatted = $"[{System.DateTime.Now:HH:mm:ss.fff}] {message}";
        Debug.Log(formatted);
        try
        {
            File.AppendAllText(logPath, formatted + "\n");
        }
        catch { }
    }

    private class IntroDebuggerHelper : MonoBehaviour
    {
        private float lastLogTime = 0f;

        private void Start()
        {
            Log("Helper component started.");
            
            // Check scripts and references
            var introMove = FindObjectOfType<PlayerIntroMovement>();
            if (introMove != null)
            {
                Log($"PlayerIntroMovement found: InitialDialogue = {(introMove.InitialDialogue != null ? introMove.InitialDialogue.name : "NULL")}");
            }
            else
            {
                Log("PlayerIntroMovement NOT found in scene!");
            }

            var dialogTriggers = FindObjectsOfType<DialogueTrigger>();
            Log($"Found {dialogTriggers.Length} DialogueTriggers in the scene:");
            foreach (var dt in dialogTriggers)
            {
                Log($" - Trigger: {dt.name}, TriggerType: {dt.TriggerType}, Repeatable: {dt.Repeatable}");
            }

            var playOnStart = FindObjectsOfType<PlayDialogueOnStart>();
            Log($"Found {playOnStart.Length} PlayDialogueOnStart components in the scene.");
        }

        private void Update()
        {
            // Log cursor and EventSystem state every 1.5 seconds
            if (Time.unscaledTime - lastLogTime > 1.5f)
            {
                lastLogTime = Time.unscaledTime;
                
                var es = EventSystem.current;
                string esInfo = es != null ? $"Active (Enabled: {es.enabled}, Current Selected: {(es.currentSelectedGameObject != null ? es.currentSelectedGameObject.name : "None")})" : "NULL";
                
                string cursorInfo = $"Visible: {Cursor.visible}, LockState: {Cursor.lockState}";

                bool isDialoguePlaying = DialogueSystem.HasReference && DialogueSystem.Instance.IsPlaying;
                float dialogueAlpha = DialogueSystem.HasReference && DialogueSystem.Instance.DialoguePanel != null ? DialogueSystem.Instance.DialoguePanel.alpha : -1f;

                Log($"Status Update -> EventSystem: {esInfo} | Cursor: {cursorInfo} | IsDialoguePlaying: {isDialoguePlaying} | DialoguePanelAlpha: {dialogueAlpha} | IsPaused: {(GameManager.HasReference ? GameManager.Instance.IsPaused.ToString() : "N/A")}");

                // Check if any UI element is blocking click
                if (es != null && es.enabled)
                {
                    PointerEventData eventData = new PointerEventData(es);
                    eventData.position = Input.mousePosition;
                    System.Collections.Generic.List<RaycastResult> results = new System.Collections.Generic.List<RaycastResult>();
                    es.RaycastAll(eventData, results);
                    if (results.Count > 0)
                    {
                        Log($"   UI Elements under mouse pointer: {results.Count}");
                        foreach (var res in results)
                        {
                            Log($"     - {res.gameObject.name} (Canvas: {res.gameObject.GetComponentInParent<Canvas>()?.name})");
                        }
                    }
                }
            }

            // Detect if Escape is pressed and log immediately
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Log("Escape pressed! Force logging state...");
                LogStateImmediate();
            }
        }

        private void LogStateImmediate()
        {
            var es = EventSystem.current;
            string esInfo = es != null ? $"Active (Enabled: {es.enabled})" : "NULL";
            string cursorInfo = $"Visible: {Cursor.visible}, LockState: {Cursor.lockState}";
            bool isDialoguePlaying = DialogueSystem.HasReference && DialogueSystem.Instance.IsPlaying;
            float dialogueAlpha = DialogueSystem.HasReference && DialogueSystem.Instance.DialoguePanel != null ? DialogueSystem.Instance.DialoguePanel.alpha : -1f;
            Log($"Immediate State -> EventSystem: {esInfo} | Cursor: {cursorInfo} | DialoguePlaying: {isDialoguePlaying} | DialogueAlpha: {dialogueAlpha} | IsPaused: {(GameManager.HasReference ? GameManager.Instance.IsPaused.ToString() : "N/A")}");
        }
    }
}
