// using System;
// using System.Collections;
// using TMPro;
// using UnityEngine;
// using UnityEngine.UI;

// [Serializable]
// public class DialogueLine
// {
//     public string characterName;

//     [TextArea(2, 5)]
//     public string dialogueText;

//     public Sprite characterSprite;
// }

// /// <summary>
// /// Весь диалог в одном скрипте: повесь на объект с Collider2D (Is Trigger).
// /// Окно рисуется отдельным Canvas, который привязан к камере (Screen Space - Camera)
// /// или ко всему экрану (Overlay) — см. поле followCamera.
// /// </summary>
// [RequireComponent(typeof(Collider2D))]
// public class DialogueTrigger : MonoBehaviour
// {
//     [Header("Dialogue Settings")]
//     public GameObject dialoguePrefab;
//     public DialogueLine[] dialogueLines;

//     [Tooltip("Сработать только один раз.")]
//     public bool playOnce = true;

//     [Tooltip("Скорость печати (символов в секунду). 0 = мгновенно.")]
//     public float charsPerSecond = 40f;

//     [Tooltip("Писать в консоль, что происходит с триггером (для отладки).")]
//     public bool debugLog = true;

//     [Tooltip("Исправлять корень префаба при создании: Scale = 1, якоря на весь Canvas, смещения 0. " +
//              "Нужно, если префаб собран как отдельный Canvas (Scale 0, размер 0, якоря в углу).")]
//     public bool fixPrefabRoot = true;

//     [Tooltip("ТЕСТ: показать на весь экран красную полупрозрачную панель с надписью. " +
//              "Если её не видно — проблема в Canvas/Game view, а не в вашем префабе.")]
//     public bool debugTestPanel = false;

//     [Header("Canvas / Camera")]
//     [Tooltip("Ваш готовый Canvas со сцены (как в самой первой версии скрипта). " +
//              "Пусто — взять первый подходящий Canvas сцены.")]
//     public Canvas targetCanvas;

//     [Tooltip("true — не использовать Canvas сцены, а создать отдельный собственный Canvas. " +
//              "Тогда работают поля ниже (Follow Camera, Target Camera, Reference Resolution).")]
//     public bool createOwnCanvas = false;

//     [Tooltip("Только для собственного Canvas: true — Screen Space - Camera (едет с камерой, " +
//              "учитывает Viewport Rect и Pixel Perfect), false — Overlay поверх всего окна.")]
//     public bool followCamera = true;

//     [Tooltip("Только для собственного Canvas. Пусто — Camera.main.")]
//     public Camera targetCamera;

//     [Tooltip("Только для собственного Canvas: Reference Resolution у Canvas Scaler.")]
//     public Vector2 referenceResolution = new Vector2(1920, 1080);

//     [Header("Переопределение раскладки (необязательно)")]
//     [Tooltip("Выключено — используется раскладка из самого префаба (рекомендуется).")]
//     public bool overrideLayout = false;

//     [Header("Character Name Layout")]
//     public Vector2 namePosition;
//     public Vector2 nameSize = new Vector2(300, 50);

//     [Header("Dialogue Text Layout")]
//     public Vector2 textPosition;
//     public Vector2 textSize = new Vector2(700, 150);

//     // Общее для всех триггеров на сцене
//     private static Canvas sharedCanvas;
//     private static DialogueTrigger activeTrigger;

//     /// <summary>true, пока идёт любой диалог. Проверяй в скрипте движения игрока.</summary>
//     public static bool IsActive => activeTrigger != null;

//     private Canvas usedCanvas;   // Canvas, в котором сейчас показано окно
//     private GameObject testPanel;
//     private GameObject window;
//     private TMP_Text nameText;
//     private TMP_Text dialogueText;
//     private Image characterSprite;
//     private RectTransform continueArrow;   // необязательный элемент "ContinueArrow"
//     private Vector2 arrowBasePos;

//     private int currentLine;
//     private bool isActive;
//     private bool isTyping;
//     private bool hasPlayed;
//     private int startFrame;
//     private Coroutine typingRoutine;

//     // Сброс статики при выключенном Domain Reload (Enter Play Mode Options)
//     [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
//     private static void ResetStatics()
//     {
//         sharedCanvas = null;
//         activeTrigger = null;
//     }

//     private void Reset()
//     {
//         GetComponent<Collider2D>().isTrigger = true;
//     }

//     private void OnTriggerEnter2D(Collider2D other)
//     {
//         Log($"OnTriggerEnter2D: вошёл '{other.name}' (тег '{other.tag}')");

//         if (!IsPlayer(other))
//         {
//             Log("Пропуск: у вошедшего объекта нет тега Player (ни у коллайдера, ни у его Rigidbody2D).");
//             return;
//         }

//         if (playOnce && hasPlayed)
//         {
//             Log("Пропуск: диалог уже проигран (включён Play Once).");
//             return;
//         }

//         if (activeTrigger != null)
//         {
//             Log("Пропуск: сейчас уже идёт другой диалог.");
//             return;
//         }

//         StartDialogue();
//     }

//     // Тег может стоять на корне игрока, а коллайдер — на дочернем объекте
//     private static bool IsPlayer(Collider2D other)
//     {
//         if (other.CompareTag("Player"))
//             return true;

//         Rigidbody2D rb = other.attachedRigidbody;
//         return rb != null && rb.CompareTag("Player");
//     }

//     private void Log(string message)
//     {
//         if (debugLog)
//             Debug.Log("[DialogueTrigger] " + message, this);
//     }

//     private void Update()
//     {
//         if (!isActive)
//             return;

//         // Покачивание стрелки "дальше"
//         if (continueArrow != null && continueArrow.gameObject.activeSelf)
//         {
//             continueArrow.anchoredPosition =
//                 arrowBasePos + Vector2.up * (Mathf.Sin(Time.unscaledTime * 6f) * 8f);
//         }

//         // Тот же клик/пробел, которым вошли в триггер, не должен пролистать первую реплику
//         if (Time.frameCount == startFrame)
//             return;

//         if (Input.GetKeyDown(KeyCode.Space) ||
//             Input.GetKeyDown(KeyCode.Return) ||
//             Input.GetMouseButtonDown(0))
//         {
//             Advance();
//         }
//     }

//     private void OnDisable()
//     {
//         // Если объект выключили/уничтожили посреди диалога — не оставляем окно висеть
//         if (isActive)
//             EndDialogue(markPlayed: false);
//     }

//     // ---------- Запуск ----------

//     private void StartDialogue()
//     {
//         if (dialoguePrefab == null ||
//             dialogueLines == null ||
//             dialogueLines.Length == 0)
//         {
//             Debug.LogError("Не заполнены настройки диалога!", this);
//             return;
//         }

//         usedCanvas = ResolveCanvas();

//         window = Instantiate(dialoguePrefab, usedCanvas.transform, false);
//         SetLayerRecursively(window, usedCanvas.gameObject.layer);

//         if (fixPrefabRoot)
//             NormalizeWindowRoot();

//         if (!(window.transform is RectTransform))
//         {
//             Debug.LogWarning("Корень префаба — не RectTransform. Собирайте префаб внутри Canvas " +
//                              "(UI → ...), иначе якоря и размеры не работают.", this);
//         }

//         // Ищем элементы на любой глубине вложенности префаба
//         Transform nameObject = FindDeep(window.transform, "CharacterName");
//         Transform textObject = FindDeep(window.transform, "DialogueText");
//         Transform spriteObject = FindDeep(window.transform, "CharacterSprite");
//         Transform arrowObject = FindDeep(window.transform, "ContinueArrow")   // необязательный
//                                 ?? FindDeep(window.transform, "Indicator");

//         if (nameObject == null || textObject == null || spriteObject == null)
//         {
//             Debug.LogError("В префабе нужны объекты с именами: CharacterName, DialogueText, CharacterSprite!", this);
//             Destroy(window);
//             return;
//         }

//         nameText = nameObject.GetComponentInChildren<TMP_Text>(true);
//         dialogueText = textObject.GetComponent<TMP_Text>();
//         characterSprite = spriteObject.GetComponent<Image>();

//         if (nameText == null || dialogueText == null || characterSprite == null)
//         {
//             Debug.LogError("Проверь компоненты TMP_Text и Image в префабе!", this);
//             Destroy(window);
//             return;
//         }

//         characterSprite.preserveAspect = true;

//         continueArrow = arrowObject as RectTransform;
//         if (continueArrow != null)
//         {
//             arrowBasePos = continueArrow.anchoredPosition;
//             continueArrow.gameObject.SetActive(false);
//         }

//         if (overrideLayout)
//             ApplyLayout();

//         if (debugTestPanel)
//             CreateTestPanel();

//         activeTrigger = this;
//         isActive = true;
//         currentLine = 0;
//         startFrame = Time.frameCount;

//         Log($"Диалог запущен (версия с исправлением корня префаба). Canvas '{usedCanvas.name}': {usedCanvas.renderMode}, " +
//             $"камера: {(usedCanvas.worldCamera != null ? usedCanvas.worldCamera.name : "нет")}, " +
//             $"слой: {LayerMask.LayerToName(usedCanvas.gameObject.layer)}, реплик: {dialogueLines.Length}");

//         ShowCurrentLine();

//         if (debugLog)
//             StartCoroutine(LogLayoutNextFrame());
//     }

//     // Префаб собран как отдельный Canvas: у корня Scale (0,0,0), размер 0 и якоря в углу.
//     // Внутри чужого Canvas такой корень схлопывает все дочерние элементы в точку (0,0).
//     private void NormalizeWindowRoot()
//     {
//         RectTransform root = window.transform as RectTransform;
//         if (root == null)
//             return;

//         Log($"Корень префаба до исправления: scale {root.localScale}, " +
//             $"anchorMin {root.anchorMin}, anchorMax {root.anchorMax}, sizeDelta {root.sizeDelta}");

//         // Убираем лишние компоненты "корневого Canvas" из префаба.
//         // Порядок важен: Canvas нельзя удалить раньше Canvas Scaler и Graphic Raycaster.
//         foreach (GraphicRaycaster raycaster in window.GetComponents<GraphicRaycaster>())
//             DestroyImmediate(raycaster);

//         foreach (CanvasScaler scaler in window.GetComponents<CanvasScaler>())
//             DestroyImmediate(scaler);

//         foreach (Canvas nestedCanvas in window.GetComponents<Canvas>())
//             DestroyImmediate(nestedCanvas);

//         root.localScale = Vector3.one;
//         root.localRotation = Quaternion.identity;
//         root.anchorMin = Vector2.zero;
//         root.anchorMax = Vector2.one;
//         root.pivot = new Vector2(0.5f, 0.5f);
//         root.offsetMin = Vector2.zero;
//         root.offsetMax = Vector2.zero;
//         root.anchoredPosition3D = Vector3.zero;
//     }

//     private Canvas ResolveCanvas()
//     {
//         if (!createOwnCanvas)
//         {
//             Canvas existing = targetCanvas != null ? targetCanvas : FindSceneCanvas();

//             if (existing != null)
//             {
//                 Log($"Использую Canvas сцены '{existing.name}'.");
//                 return existing.rootCanvas;
//             }

//             Debug.LogWarning("Canvas на сцене не найден — создаю собственный.", this);
//         }

//         EnsureCanvas();
//         ConfigureCanvas();
//         return sharedCanvas;
//     }

//     private static Canvas FindSceneCanvas()
//     {
//         Canvas fallback = null;

//         foreach (Canvas c in FindObjectsByType<Canvas>(FindObjectsSortMode.None))
//         {
//             if (!c.isRootCanvas || c == sharedCanvas || !c.gameObject.activeInHierarchy)
//                 continue;

//             // Предпочитаем экранные Canvas, World Space — только если других нет
//             if (c.renderMode != RenderMode.WorldSpace)
//                 return c;

//             if (fallback == null)
//                 fallback = c;
//         }

//         return fallback;
//     }

//     private void EnsureCanvas()
//     {
//         if (sharedCanvas != null)
//             return;

//         // Свой отдельный Canvas, а не FindFirstObjectByType:
//         // чужой Canvas может быть World Space или с другим масштабом.
//         var canvasObject = new GameObject("DialogueCanvas");

//         sharedCanvas = canvasObject.AddComponent<Canvas>();
//         sharedCanvas.sortingOrder = 1000; // поверх спрайтов и остального UI

//         var scaler = canvasObject.AddComponent<CanvasScaler>();
//         scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
//         scaler.referenceResolution = referenceResolution;
//         scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
//         scaler.matchWidthOrHeight = 0.5f;
//     }

//     private static int canvasLayer;

//     private void ConfigureCanvas()
//     {
//         Camera cam = targetCamera != null ? targetCamera : Camera.main;

//         // Самый верхний Sorting Layer: иначе спрайты и тайлмапы на более поздних
//         // слоях рисуются поверх UI, и окно "есть, но не видно".
//         SortingLayer[] layers = SortingLayer.layers;
//         sharedCanvas.sortingLayerID = layers[layers.Length - 1].id;
//         sharedCanvas.sortingOrder = 1000;

//         if (followCamera && cam != null)
//         {
//             sharedCanvas.renderMode = RenderMode.ScreenSpaceCamera;
//             sharedCanvas.worldCamera = cam;
//             sharedCanvas.planeDistance =
//                 Mathf.Clamp(1f, cam.nearClipPlane + 0.01f, cam.farClipPlane - 0.01f);

//             // Слой, который камера реально рисует (Culling Mask)
//             canvasLayer = PickRenderLayer(cam);
//         }
//         else
//         {
//             if (followCamera)
//                 Debug.LogWarning("Камера не найдена (нет Camera.main). Использую Overlay.", this);

//             sharedCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
//             sharedCanvas.worldCamera = null;

//             int ui = LayerMask.NameToLayer("UI");
//             canvasLayer = ui >= 0 ? ui : 0;
//         }

//         SetLayerRecursively(sharedCanvas.gameObject, canvasLayer);
//     }

//     private static int PickRenderLayer(Camera cam)
//     {
//         int mask = cam.cullingMask;

//         int ui = LayerMask.NameToLayer("UI");
//         if (ui >= 0 && (mask & (1 << ui)) != 0)
//             return ui;

//         for (int i = 0; i < 32; i++)
//         {
//             if ((mask & (1 << i)) != 0)
//                 return i;
//         }

//         return 0;
//     }

//     private static void SetLayerRecursively(GameObject go, int layer)
//     {
//         foreach (Transform t in go.GetComponentsInChildren<Transform>(true))
//             t.gameObject.layer = layer;
//     }

//     private void ApplyLayout()
//     {
//         RectTransform nameRect = nameText.rectTransform;
//         RectTransform textRect = dialogueText.rectTransform;

//         nameRect.anchoredPosition = namePosition;
//         nameRect.sizeDelta = nameSize;

//         textRect.anchoredPosition = textPosition;
//         textRect.sizeDelta = textSize;
//     }

//     private void CreateTestPanel()
//     {
//         testPanel = new GameObject("DEBUG_TestPanel", typeof(RectTransform), typeof(Image));
//         testPanel.layer = usedCanvas.gameObject.layer;
//         testPanel.transform.SetParent(usedCanvas.transform, false);

//         var rect = (RectTransform)testPanel.transform;
//         rect.anchorMin = Vector2.zero;
//         rect.anchorMax = Vector2.one;
//         rect.offsetMin = Vector2.zero;
//         rect.offsetMax = Vector2.zero;

//         testPanel.GetComponent<Image>().color = new Color(1f, 0f, 0f, 0.35f);

//         var labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
//         labelObject.layer = testPanel.layer;
//         labelObject.transform.SetParent(testPanel.transform, false);

//         var labelRect = (RectTransform)labelObject.transform;
//         labelRect.anchorMin = Vector2.zero;
//         labelRect.anchorMax = Vector2.one;
//         labelRect.offsetMin = Vector2.zero;
//         labelRect.offsetMax = Vector2.zero;

//         var label = labelObject.GetComponent<TextMeshProUGUI>();
//         label.text = "CANVAS OK";
//         label.alignment = TextAlignmentOptions.Center;
//         label.fontSize = 48;
//         label.color = Color.white;
//     }

//     // Через кадр (и через секунду), когда Canvas уже посчитал размеры,
//     // пишем где реально лежат элементы
//     private IEnumerator LogLayoutNextFrame()
//     {
//         yield return null;

//         if (window == null)
//         {
//             Log("Окно уже закрыто к следующему кадру (диалог завершился сразу после старта).");
//             yield break;
//         }

//         Canvas.ForceUpdateCanvases();
//         LogFullState("Через кадр");

//         yield return new WaitForSecondsRealtime(1f);

//         if (window == null)
//             yield break;

//         LogFullState("Через 1 секунду");
//     }

//     private void LogFullState(string when)
//     {
//         Log($"--- {when} ---");
//         Log($"Экран: {Screen.width}x{Screen.height}, Time.timeScale {Time.timeScale}");

//         RectTransform canvasRect = usedCanvas.GetComponent<RectTransform>();
//         CanvasScaler scaler = usedCanvas.GetComponent<CanvasScaler>();

//         Log($"Canvas '{usedCanvas.name}': режим {usedCanvas.renderMode}, включён {usedCanvas.enabled}, " +
//             $"rect {canvasRect.rect.size}, localScale {canvasRect.localScale}, " +
//             $"scaleFactor {usedCanvas.scaleFactor}, " +
//             $"scaler {(scaler != null ? scaler.uiScaleMode.ToString() : "нет")}, " +
//             $"sortingOrder {usedCanvas.sortingOrder}, " +
//             $"камера {(usedCanvas.worldCamera != null ? usedCanvas.worldCamera.name : "нет")}");

//         foreach (CanvasGroup group in window.GetComponentsInChildren<CanvasGroup>(true))
//             Log($"CanvasGroup на '{group.name}': alpha {group.alpha}");

//         LogRect("Окно", window.transform);
//         LogRect("CharacterName", nameText.transform);
//         LogRect("DialogueText", dialogueText.transform);
//         LogRect("CharacterSprite", characterSprite.transform);
//     }

//     private void LogRect(string label, Transform t)
//     {
//         RectTransform r = t as RectTransform;
//         if (r == null)
//         {
//             Log($"{label}: НЕ RectTransform!");
//             return;
//         }

//         Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(usedCanvas.worldCamera, r.position);
//         Log($"{label}: размер {r.rect.size}, anchorMin {r.anchorMin}, anchorMax {r.anchorMax}, " +
//             $"pos {r.anchoredPosition}, scale {r.localScale}, на экране {screenPos}");
//     }

//     private static Transform FindDeep(Transform root, string childName)
//     {
//         foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
//         {
//             if (t.name == childName)
//                 return t;
//         }
//         return null;
//     }

//     // ---------- Показ реплик ----------

//     private void ShowCurrentLine()
//     {
//         DialogueLine line = dialogueLines[currentLine];
//         Log($"Реплика {currentLine + 1}/{dialogueLines.Length}: {line.characterName}");

//         nameText.text = line.characterName;
//         characterSprite.sprite = line.characterSprite;
//         characterSprite.enabled = line.characterSprite != null;

//         dialogueText.text = line.dialogueText;

//         if (typingRoutine != null)
//             StopCoroutine(typingRoutine);

//         if (charsPerSecond <= 0f)
//         {
//             dialogueText.maxVisibleCharacters = int.MaxValue;
//             isTyping = false;
//             SetArrow(true);
//         }
//         else
//         {
//             typingRoutine = StartCoroutine(TypeText());
//         }
//     }

//     private IEnumerator TypeText()
//     {
//         isTyping = true;
//         SetArrow(false);

//         dialogueText.maxVisibleCharacters = 0;
//         dialogueText.ForceMeshUpdate();
//         int total = dialogueText.textInfo.characterCount;

//         float progress = 0f;
//         while (progress < total)
//         {
//             progress += Time.unscaledDeltaTime * charsPerSecond;
//             dialogueText.maxVisibleCharacters = Mathf.Min(total, Mathf.FloorToInt(progress));
//             yield return null;
//         }

//         FinishTyping();
//     }

//     private void FinishTyping()
//     {
//         if (typingRoutine != null)
//         {
//             StopCoroutine(typingRoutine);
//             typingRoutine = null;
//         }

//         dialogueText.maxVisibleCharacters = int.MaxValue;
//         isTyping = false;
//         SetArrow(true);
//     }

//     private void SetArrow(bool visible)
//     {
//         if (continueArrow == null)
//             return;

//         continueArrow.gameObject.SetActive(visible);

//         if (visible)
//             continueArrow.anchoredPosition = arrowBasePos;
//     }

//     private void Advance()
//     {
//         // Нажатие во время печати — показать реплику целиком
//         if (isTyping)
//         {
//             FinishTyping();
//             return;
//         }

//         currentLine++;

//         if (currentLine >= dialogueLines.Length)
//         {
//             EndDialogue(markPlayed: true);
//             return;
//         }

//         ShowCurrentLine();
//     }

//     private void EndDialogue(bool markPlayed)
//     {
//         Log("Диалог завершён.");
//         isActive = false;
//         isTyping = false;

//         if (markPlayed)
//             hasPlayed = true;

//         if (window != null)
//             Destroy(window);

//         if (testPanel != null)
//             Destroy(testPanel);

//         window = null;
//         testPanel = null;

//         if (activeTrigger == this)
//             activeTrigger = null;
//     }
// }





using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class DialogueLine
{
    public string characterName;

    [TextArea(2, 5)]
    public string dialogueText;

    public Sprite characterSprite;
}

/// <summary>
/// Весь диалог в одном скрипте: повесь на объект с Collider2D (Is Trigger).
/// Окно рисуется отдельным Canvas, который привязан к камере (Screen Space - Camera)
/// или ко всему экрану (Overlay) — см. поле followCamera.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue Settings")]
    public GameObject dialoguePrefab;
    public DialogueLine[] dialogueLines;

    [Tooltip("Сработать только один раз.")]
    public bool playOnce = true;

    [Tooltip("Скорость печати (символов в секунду). 0 = мгновенно.")]
    public float charsPerSecond = 40f;

    [Tooltip("Писать в консоль, что происходит с триггером (для отладки).")]
    public bool debugLog = true;

    [Tooltip("Исправлять корень префаба при создании: Scale = 1, якоря на весь Canvas, смещения 0. " +
             "Нужно, если префаб собран как отдельный Canvas (Scale 0, размер 0, якоря в углу).")]
    public bool fixPrefabRoot = true;

    [Header("Размер окна")]
    [Tooltip("Подгонять размер окна под экран. Нужно, если Canvas Scaler сцены уменьшает UI " +
             "(например, Reference Resolution 1920x1080 на маленьком экране). " +
             "Работает только вместе с Fix Prefab Root.")]
    public bool scaleToScreen = true;

    [Tooltip("Размер экрана в пикселях, под который рисовался префаб (при масштабе 1:1). " +
             "При таком экране окно будет ровно того размера, что в префабе.")]
    public Vector2 prefabDesignResolution = new Vector2(716, 468);

    [Tooltip("Дополнительный множитель размера окна: 1 = как в префабе, 1.5 = на 50% крупнее, 0.8 = мельче.")]
    public float uiScale = 1f;

    [Tooltip("ТЕСТ: показать на весь экран красную полупрозрачную панель с надписью. " +
             "Если её не видно — проблема в Canvas/Game view, а не в вашем префабе.")]
    public bool debugTestPanel = false;

    [Header("Canvas / Camera")]
    [Tooltip("Ваш готовый Canvas со сцены (как в самой первой версии скрипта). " +
             "Пусто — взять первый подходящий Canvas сцены.")]
    public Canvas targetCanvas;

    [Tooltip("true — не использовать Canvas сцены, а создать отдельный собственный Canvas. " +
             "Тогда работают поля ниже (Follow Camera, Target Camera, Reference Resolution).")]
    public bool createOwnCanvas = false;

    [Tooltip("Только для собственного Canvas: true — Screen Space - Camera (едет с камерой, " +
             "учитывает Viewport Rect и Pixel Perfect), false — Overlay поверх всего окна.")]
    public bool followCamera = true;

    [Tooltip("Только для собственного Canvas. Пусто — Camera.main.")]
    public Camera targetCamera;

    [Tooltip("Только для собственного Canvas: Reference Resolution у Canvas Scaler.")]
    public Vector2 referenceResolution = new Vector2(1920, 1080);

    [Header("Переопределение раскладки (необязательно)")]
    [Tooltip("Выключено — используется раскладка из самого префаба (рекомендуется).")]
    public bool overrideLayout = false;

    [Header("Character Name Layout")]
    public Vector2 namePosition;
    public Vector2 nameSize = new Vector2(300, 50);

    [Header("Dialogue Text Layout")]
    public Vector2 textPosition;
    public Vector2 textSize = new Vector2(700, 150);

    // Общее для всех триггеров на сцене
    private static Canvas sharedCanvas;
    private static DialogueTrigger activeTrigger;

    /// <summary>true, пока идёт любой диалог. Проверяй в скрипте движения игрока.</summary>
    public static bool IsActive => activeTrigger != null;

    private Canvas usedCanvas;   // Canvas, в котором сейчас показано окно
    private GameObject testPanel;
    private GameObject window;
    private TMP_Text nameText;
    private TMP_Text dialogueText;
    private Image characterSprite;
    private RectTransform continueArrow;   // необязательный элемент "ContinueArrow"
    private Vector2 arrowBasePos;

    private int currentLine;
    private bool isActive;
    private bool isTyping;
    private bool hasPlayed;
    private int startFrame;
    private Coroutine typingRoutine;

    // Сброс статики при выключенном Domain Reload (Enter Play Mode Options)
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        sharedCanvas = null;
        activeTrigger = null;
    }

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Log($"OnTriggerEnter2D: вошёл '{other.name}' (тег '{other.tag}')");

        if (!IsPlayer(other))
        {
            Log("Пропуск: у вошедшего объекта нет тега Player (ни у коллайдера, ни у его Rigidbody2D).");
            return;
        }

        if (playOnce && hasPlayed)
        {
            Log("Пропуск: диалог уже проигран (включён Play Once).");
            return;
        }

        if (activeTrigger != null)
        {
            Log("Пропуск: сейчас уже идёт другой диалог.");
            return;
        }

        StartDialogue();
    }

    // Тег может стоять на корне игрока, а коллайдер — на дочернем объекте
    private static bool IsPlayer(Collider2D other)
    {
        if (other.CompareTag("Player"))
            return true;

        Rigidbody2D rb = other.attachedRigidbody;
        return rb != null && rb.CompareTag("Player");
    }

    private void Log(string message)
    {
        if (debugLog)
            Debug.Log("[DialogueTrigger] " + message, this);
    }

    private void Update()
    {
        if (!isActive)
            return;

        // Окно подгоняется под размер экрана (на случай изменения размера окна игры)
        if (fixPrefabRoot && scaleToScreen)
            ApplyWindowScale();

        // Покачивание стрелки "дальше"
        if (continueArrow != null && continueArrow.gameObject.activeSelf)
        {
            continueArrow.anchoredPosition =
                arrowBasePos + Vector2.up * (Mathf.Sin(Time.unscaledTime * 6f) * 8f);
        }

        // Тот же клик/пробел, которым вошли в триггер, не должен пролистать первую реплику
        if (Time.frameCount == startFrame)
            return;

        if (Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.Return) ||
            Input.GetMouseButtonDown(0))
        {
            Advance();
        }
    }

    private void OnDisable()
    {
        // Если объект выключили/уничтожили посреди диалога — не оставляем окно висеть
        if (isActive)
            EndDialogue(markPlayed: false);
    }

    // ---------- Запуск ----------

    private void StartDialogue()
    {
        if (dialoguePrefab == null ||
            dialogueLines == null ||
            dialogueLines.Length == 0)
        {
            Debug.LogError("Не заполнены настройки диалога!", this);
            return;
        }

        usedCanvas = ResolveCanvas();

        window = Instantiate(dialoguePrefab, usedCanvas.transform, false);
        SetLayerRecursively(window, usedCanvas.gameObject.layer);

        if (fixPrefabRoot)
            NormalizeWindowRoot();

        if (!(window.transform is RectTransform))
        {
            Debug.LogWarning("Корень префаба — не RectTransform. Собирайте префаб внутри Canvas " +
                             "(UI → ...), иначе якоря и размеры не работают.", this);
        }

        // Ищем элементы на любой глубине вложенности префаба
        Transform nameObject = FindDeep(window.transform, "CharacterName");
        Transform textObject = FindDeep(window.transform, "DialogueText");
        Transform spriteObject = FindDeep(window.transform, "CharacterSprite");
        Transform arrowObject = FindDeep(window.transform, "ContinueArrow")   // необязательный
                                ?? FindDeep(window.transform, "Indicator");

        if (nameObject == null || textObject == null || spriteObject == null)
        {
            Debug.LogError("В префабе нужны объекты с именами: CharacterName, DialogueText, CharacterSprite!", this);
            Destroy(window);
            return;
        }

        nameText = nameObject.GetComponentInChildren<TMP_Text>(true);
        dialogueText = textObject.GetComponent<TMP_Text>();
        characterSprite = spriteObject.GetComponent<Image>();

        if (nameText == null || dialogueText == null || characterSprite == null)
        {
            Debug.LogError("Проверь компоненты TMP_Text и Image в префабе!", this);
            Destroy(window);
            return;
        }

        characterSprite.preserveAspect = true;

        continueArrow = arrowObject as RectTransform;
        if (continueArrow != null)
        {
            arrowBasePos = continueArrow.anchoredPosition;
            continueArrow.gameObject.SetActive(false);
        }

        if (overrideLayout)
            ApplyLayout();

        if (debugTestPanel)
            CreateTestPanel();

        activeTrigger = this;
        isActive = true;
        currentLine = 0;
        startFrame = Time.frameCount;

        Log($"Диалог запущен (версия с исправлением корня префаба). Canvas '{usedCanvas.name}': {usedCanvas.renderMode}, " +
            $"камера: {(usedCanvas.worldCamera != null ? usedCanvas.worldCamera.name : "нет")}, " +
            $"слой: {LayerMask.LayerToName(usedCanvas.gameObject.layer)}, реплик: {dialogueLines.Length}");

        ShowCurrentLine();

        if (debugLog)
            StartCoroutine(LogLayoutNextFrame());
    }

    // Префаб собран как отдельный Canvas: у корня Scale (0,0,0), размер 0 и якоря в углу.
    // Внутри чужого Canvas такой корень схлопывает все дочерние элементы в точку (0,0).
    private void NormalizeWindowRoot()
    {
        RectTransform root = window.transform as RectTransform;
        if (root == null)
            return;

        Log($"Корень префаба до исправления: scale {root.localScale}, " +
            $"anchorMin {root.anchorMin}, anchorMax {root.anchorMax}, sizeDelta {root.sizeDelta}");

        // Убираем лишние компоненты "корневого Canvas" из префаба.
        // Порядок важен: Canvas нельзя удалить раньше Canvas Scaler и Graphic Raycaster.
        foreach (GraphicRaycaster raycaster in window.GetComponents<GraphicRaycaster>())
            DestroyImmediate(raycaster);

        foreach (CanvasScaler scaler in window.GetComponents<CanvasScaler>())
            DestroyImmediate(scaler);

        foreach (Canvas nestedCanvas in window.GetComponents<Canvas>())
            DestroyImmediate(nestedCanvas);

        root.localScale = Vector3.one;
        root.localRotation = Quaternion.identity;
        root.anchorMin = Vector2.zero;
        root.anchorMax = Vector2.one;
        root.pivot = new Vector2(0.5f, 0.5f);
        root.offsetMin = Vector2.zero;
        root.offsetMax = Vector2.zero;
        root.anchoredPosition3D = Vector3.zero;

        if (scaleToScreen)
            ApplyWindowScale();
    }

    // Масштабирует корень окна так, чтобы элементы префаба (нарисованные в пикселях) имели
    // нормальный размер независимо от Canvas Scaler сцены. Корень получает масштаб s и размер
    // canvas/s, поэтому после масштабирования всё равно покрывает весь экран, а элементы,
    // привязанные к углам и краям, остаются у краёв.
    private void ApplyWindowScale()
    {
        if (window == null || usedCanvas == null)
            return;

        RectTransform root = window.transform as RectTransform;
        RectTransform canvasRect = usedCanvas.transform as RectTransform;
        if (root == null || canvasRect == null)
            return;

        Vector2 canvasSize = canvasRect.rect.size;
        if (canvasSize.x <= 0f || canvasSize.y <= 0f)
            return;

        float canvasScale = Mathf.Max(0.0001f, usedCanvas.scaleFactor);
        Vector2 screenPixels = canvasSize * canvasScale;

        float s = Mathf.Max(0.01f, uiScale) * DesignScale(screenPixels) / canvasScale;

        root.anchorMin = new Vector2(0.5f, 0.5f);
        root.anchorMax = new Vector2(0.5f, 0.5f);
        root.pivot = new Vector2(0.5f, 0.5f);
        root.anchoredPosition = Vector2.zero;
        root.sizeDelta = canvasSize / s;
        root.localScale = new Vector3(s, s, 1f);
    }

    // Как у Canvas Scaler (Match Width Or Height = 0.5), но относительно prefabDesignResolution
    private float DesignScale(Vector2 screenPixels)
    {
        if (prefabDesignResolution.x <= 0f || prefabDesignResolution.y <= 0f)
            return 1f;

        float logW = Mathf.Log(screenPixels.x / prefabDesignResolution.x, 2f);
        float logH = Mathf.Log(screenPixels.y / prefabDesignResolution.y, 2f);
        return Mathf.Pow(2f, Mathf.Lerp(logW, logH, 0.5f));
    }

    private Canvas ResolveCanvas()
    {
        if (!createOwnCanvas)
        {
            Canvas existing = targetCanvas != null ? targetCanvas : FindSceneCanvas();

            if (existing != null)
            {
                Log($"Использую Canvas сцены '{existing.name}'.");
                return existing.rootCanvas;
            }

            Debug.LogWarning("Canvas на сцене не найден — создаю собственный.", this);
        }

        EnsureCanvas();
        ConfigureCanvas();
        return sharedCanvas;
    }

    private static Canvas FindSceneCanvas()
    {
        Canvas fallback = null;

        foreach (Canvas c in FindObjectsByType<Canvas>(FindObjectsSortMode.None))
        {
            if (!c.isRootCanvas || c == sharedCanvas || !c.gameObject.activeInHierarchy)
                continue;

            // Предпочитаем экранные Canvas, World Space — только если других нет
            if (c.renderMode != RenderMode.WorldSpace)
                return c;

            if (fallback == null)
                fallback = c;
        }

        return fallback;
    }

    private void EnsureCanvas()
    {
        if (sharedCanvas != null)
            return;

        // Свой отдельный Canvas, а не FindFirstObjectByType:
        // чужой Canvas может быть World Space или с другим масштабом.
        var canvasObject = new GameObject("DialogueCanvas");

        sharedCanvas = canvasObject.AddComponent<Canvas>();
        sharedCanvas.sortingOrder = 1000; // поверх спрайтов и остального UI

        var scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = referenceResolution;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
    }

    private static int canvasLayer;

    private void ConfigureCanvas()
    {
        Camera cam = targetCamera != null ? targetCamera : Camera.main;

        // Самый верхний Sorting Layer: иначе спрайты и тайлмапы на более поздних
        // слоях рисуются поверх UI, и окно "есть, но не видно".
        SortingLayer[] layers = SortingLayer.layers;
        sharedCanvas.sortingLayerID = layers[layers.Length - 1].id;
        sharedCanvas.sortingOrder = 1000;

        if (followCamera && cam != null)
        {
            sharedCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            sharedCanvas.worldCamera = cam;
            sharedCanvas.planeDistance =
                Mathf.Clamp(1f, cam.nearClipPlane + 0.01f, cam.farClipPlane - 0.01f);

            // Слой, который камера реально рисует (Culling Mask)
            canvasLayer = PickRenderLayer(cam);
        }
        else
        {
            if (followCamera)
                Debug.LogWarning("Камера не найдена (нет Camera.main). Использую Overlay.", this);

            sharedCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            sharedCanvas.worldCamera = null;

            int ui = LayerMask.NameToLayer("UI");
            canvasLayer = ui >= 0 ? ui : 0;
        }

        SetLayerRecursively(sharedCanvas.gameObject, canvasLayer);
    }

    private static int PickRenderLayer(Camera cam)
    {
        int mask = cam.cullingMask;

        int ui = LayerMask.NameToLayer("UI");
        if (ui >= 0 && (mask & (1 << ui)) != 0)
            return ui;

        for (int i = 0; i < 32; i++)
        {
            if ((mask & (1 << i)) != 0)
                return i;
        }

        return 0;
    }

    private static void SetLayerRecursively(GameObject go, int layer)
    {
        foreach (Transform t in go.GetComponentsInChildren<Transform>(true))
            t.gameObject.layer = layer;
    }

    private void ApplyLayout()
    {
        RectTransform nameRect = nameText.rectTransform;
        RectTransform textRect = dialogueText.rectTransform;

        nameRect.anchoredPosition = namePosition;
        nameRect.sizeDelta = nameSize;

        textRect.anchoredPosition = textPosition;
        textRect.sizeDelta = textSize;
    }

    private void CreateTestPanel()
    {
        testPanel = new GameObject("DEBUG_TestPanel", typeof(RectTransform), typeof(Image));
        testPanel.layer = usedCanvas.gameObject.layer;
        testPanel.transform.SetParent(usedCanvas.transform, false);

        var rect = (RectTransform)testPanel.transform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        testPanel.GetComponent<Image>().color = new Color(1f, 0f, 0f, 0.35f);

        var labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelObject.layer = testPanel.layer;
        labelObject.transform.SetParent(testPanel.transform, false);

        var labelRect = (RectTransform)labelObject.transform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        var label = labelObject.GetComponent<TextMeshProUGUI>();
        label.text = "CANVAS OK";
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = 48;
        label.color = Color.white;
    }

    // Через кадр (и через секунду), когда Canvas уже посчитал размеры,
    // пишем где реально лежат элементы
    private IEnumerator LogLayoutNextFrame()
    {
        yield return null;

        if (window == null)
        {
            Log("Окно уже закрыто к следующему кадру (диалог завершился сразу после старта).");
            yield break;
        }

        Canvas.ForceUpdateCanvases();
        LogFullState("Через кадр");

        yield return new WaitForSecondsRealtime(1f);

        if (window == null)
            yield break;

        LogFullState("Через 1 секунду");
    }

    private void LogFullState(string when)
    {
        Log($"--- {when} ---");
        Log($"Экран: {Screen.width}x{Screen.height}, Time.timeScale {Time.timeScale}");

        RectTransform canvasRect = usedCanvas.GetComponent<RectTransform>();
        CanvasScaler scaler = usedCanvas.GetComponent<CanvasScaler>();

        Log($"Canvas '{usedCanvas.name}': режим {usedCanvas.renderMode}, включён {usedCanvas.enabled}, " +
            $"rect {canvasRect.rect.size}, localScale {canvasRect.localScale}, " +
            $"scaleFactor {usedCanvas.scaleFactor}, " +
            $"scaler {(scaler != null ? scaler.uiScaleMode.ToString() : "нет")}, " +
            $"sortingOrder {usedCanvas.sortingOrder}, " +
            $"камера {(usedCanvas.worldCamera != null ? usedCanvas.worldCamera.name : "нет")}");

        foreach (CanvasGroup group in window.GetComponentsInChildren<CanvasGroup>(true))
            Log($"CanvasGroup на '{group.name}': alpha {group.alpha}");

        LogRect("Окно", window.transform);
        LogRect("CharacterName", nameText.transform);
        LogRect("DialogueText", dialogueText.transform);
        LogRect("CharacterSprite", characterSprite.transform);
    }

    private void LogRect(string label, Transform t)
    {
        RectTransform r = t as RectTransform;
        if (r == null)
        {
            Log($"{label}: НЕ RectTransform!");
            return;
        }

        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(usedCanvas.worldCamera, r.position);
        Log($"{label}: размер {r.rect.size}, anchorMin {r.anchorMin}, anchorMax {r.anchorMax}, " +
            $"pos {r.anchoredPosition}, scale {r.localScale}, на экране {screenPos}");
    }

    private static Transform FindDeep(Transform root, string childName)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
        {
            if (t.name == childName)
                return t;
        }
        return null;
    }

    // ---------- Показ реплик ----------

    private void ShowCurrentLine()
    {
        DialogueLine line = dialogueLines[currentLine];
        Log($"Реплика {currentLine + 1}/{dialogueLines.Length}: {line.characterName}");

        nameText.text = line.characterName;
        characterSprite.sprite = line.characterSprite;
        characterSprite.enabled = line.characterSprite != null;

        dialogueText.text = line.dialogueText;

        if (typingRoutine != null)
            StopCoroutine(typingRoutine);

        if (charsPerSecond <= 0f)
        {
            dialogueText.maxVisibleCharacters = int.MaxValue;
            isTyping = false;
            SetArrow(true);
        }
        else
        {
            typingRoutine = StartCoroutine(TypeText());
        }
    }

    private IEnumerator TypeText()
    {
        isTyping = true;
        SetArrow(false);

        dialogueText.maxVisibleCharacters = 0;
        dialogueText.ForceMeshUpdate();
        int total = dialogueText.textInfo.characterCount;

        float progress = 0f;
        while (progress < total)
        {
            progress += Time.unscaledDeltaTime * charsPerSecond;
            dialogueText.maxVisibleCharacters = Mathf.Min(total, Mathf.FloorToInt(progress));
            yield return null;
        }

        FinishTyping();
    }

    private void FinishTyping()
    {
        if (typingRoutine != null)
        {
            StopCoroutine(typingRoutine);
            typingRoutine = null;
        }

        dialogueText.maxVisibleCharacters = int.MaxValue;
        isTyping = false;
        SetArrow(true);
    }

    private void SetArrow(bool visible)
    {
        if (continueArrow == null)
            return;

        continueArrow.gameObject.SetActive(visible);

        if (visible)
            continueArrow.anchoredPosition = arrowBasePos;
    }

    private void Advance()
    {
        // Нажатие во время печати — показать реплику целиком
        if (isTyping)
        {
            FinishTyping();
            return;
        }

        currentLine++;

        if (currentLine >= dialogueLines.Length)
        {
            EndDialogue(markPlayed: true);
            return;
        }

        ShowCurrentLine();
    }

    private void EndDialogue(bool markPlayed)
    {
        Log("Диалог завершён.");
        isActive = false;
        isTyping = false;

        if (markPlayed)
            hasPlayed = true;

        if (window != null)
            Destroy(window);

        if (testPanel != null)
            Destroy(testPanel);

        window = null;
        testPanel = null;

        if (activeTrigger == this)
            activeTrigger = null;
    }
}