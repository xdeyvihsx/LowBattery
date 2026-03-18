using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenuController : MonoBehaviour
{
    [Header("UI Documents")]
    [SerializeField] private UIDocument uiDocument;
    public UIDocument inGameOptions;

    [Header("Escenas")]
    public string escenaGlobalLevels = "GlobalLevels";

    private const string ID_BTN_CONTINUE = "BtnContinue";
    private const string ID_BTN_OPTIONS  = "BtnOptions";
    private const string ID_BTN_QUIT     = "BtnQuitToMenu";
    private const string ID_ARROW_L      = "ArrowL";
    private const string ID_ARROW_R      = "ArrowR";

    private bool pausado    = false;
    private bool enOpciones = false;

    private Button btnContinue;
    private Button btnOptions;
    private Button btnQuit;
    private VisualElement arrowL;
    private VisualElement arrowR;
    private Button[] botones;
    private int sel = 0;

    private PlayerSoundController   sfxPlayer;
    private LevelAudioManager       sfxNivel;
    private InGameOptionsController optCtrl;

    private static readonly Color CA = new Color(0.94f, 0.95f, 1.00f, 1f);
    private static readonly Color CI = new Color(0.72f, 0.74f, 0.88f, 0.7f);

    void Awake()
    {
        if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
        sfxPlayer = FindFirstObjectByType<PlayerSoundController>();
        sfxNivel  = FindFirstObjectByType<LevelAudioManager>();
    }

    void Start()
    {
        // Ocultar todo al inicio usando display — NUNCA SetActive(false) en UIDocuments
        Ocultar(uiDocument);
        Ocultar(inGameOptions);

        if (inGameOptions != null)
            optCtrl = inGameOptions.GetComponent<InGameOptionsController>();
    }

    void OnEnable()
    {
        if (uiDocument?.rootVisualElement == null) return;
        var root = uiDocument.rootVisualElement;

        btnContinue = Btn(root, ID_BTN_CONTINUE, "continue", "continuar");
        btnOptions  = Btn(root, ID_BTN_OPTIONS,  "option",   "opciones");
        btnQuit     = Btn(root, ID_BTN_QUIT,     "quit",     "menu", "salir");
        arrowL      = root.Q<VisualElement>(ID_ARROW_L);
        arrowR      = root.Q<VisualElement>(ID_ARROW_R);

        botones = new Button[] { btnContinue, btnOptions, btnQuit };

        for (int i = 0; i < botones.Length; i++)
        {
            if (botones[i] == null) continue;
            int n = i;
            botones[i].RegisterCallback<MouseEnterEvent>(_ => Sel(n));
        }

        if (btnContinue != null) btnContinue.RegisterCallback<ClickEvent>(_ => Continuar());
        if (btnOptions  != null) btnOptions.RegisterCallback<ClickEvent> (_ => AbrirOpts());
        if (btnQuit     != null) btnQuit.RegisterCallback<ClickEvent>    (_ => Salir());
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (enOpciones)   CerrarOptions();
            else if (pausado) Continuar();
            else              Pausar();
            return;
        }

        if (!pausado || enOpciones) return;

        if (Keyboard.current.downArrowKey.wasPressedThisFrame  || Keyboard.current.sKey.wasPressedThisFrame)
            Sel((sel + 1) % botones.Length);
        if (Keyboard.current.upArrowKey.wasPressedThisFrame    || Keyboard.current.wKey.wasPressedThisFrame)
            Sel((sel - 1 + botones.Length) % botones.Length);
        if (Keyboard.current.enterKey.wasPressedThisFrame)
            Exec();
    }

    void Pausar()
    {
        pausado = true; Time.timeScale = 0f;
        sfxPlayer?.PausarAudio(); sfxNivel?.PausarAudio();
        Mostrar(uiDocument); Sel(0);
    }

    void Continuar()
    {
        CerrarOptions();
        pausado = false; Time.timeScale = 1f;
        sfxPlayer?.ReanudarAudio(); sfxNivel?.ReanudarAudio();
        Ocultar(uiDocument);
    }

    void Salir()
    {
        CerrarOptions();
        pausado = false; Time.timeScale = 1f;
        sfxPlayer?.ResetearAudio(); sfxNivel?.DetenerTodo();
        SceneManager.LoadScene(escenaGlobalLevels);
    }

    void AbrirOpts()
    {
        if (inGameOptions == null)
        {
            Debug.LogWarning("[Pause] inGameOptions no asignado.");
            return;
        }
        enOpciones = true;
        Ocultar(uiDocument);
        Mostrar(inGameOptions);
        optCtrl?.AlAbrir(this);
    }

    public void CerrarOptions()
    {
        if (!enOpciones) return;
        enOpciones = false;
        Ocultar(inGameOptions);
        optCtrl?.CerrarTodo();
        if (pausado) Mostrar(uiDocument);
    }

    // ── Visibilidad via display — nunca SetActive ──────────────
    void Mostrar(UIDocument doc)
    {
        if (doc?.rootVisualElement != null)
            doc.rootVisualElement.style.display = DisplayStyle.Flex;
    }

    void Ocultar(UIDocument doc)
    {
        if (doc?.rootVisualElement != null)
            doc.rootVisualElement.style.display = DisplayStyle.None;
    }

    void Sel(int n)
    {
        sel = n;
        for (int i = 0; i < botones.Length; i++)
        {
            if (botones[i] == null) continue;
            bool a = (i == n);
            botones[i].style.color   = new StyleColor(a ? CA : CI);
            botones[i].style.opacity = a ? 1f : 0.65f;
        }
        if (botones[n] == null) return;
        var p = botones[n].parent; if (p == null) return;
        if (arrowL != null) { arrowL.RemoveFromHierarchy(); p.Insert(0, arrowL); arrowL.style.opacity = 1f; }
        if (arrowR != null) { arrowR.RemoveFromHierarchy(); p.Add(arrowR);       arrowR.style.opacity = 1f; }
    }

    void Exec()
    {
        switch (sel) { case 0: Continuar(); break; case 1: AbrirOpts(); break; case 2: Salir(); break; }
    }

    void OnDestroy() { if (pausado) Time.timeScale = 1f; }

    Button Btn(VisualElement root, string id, params string[] palabras)
    {
        var b = root.Q<Button>(id);
        if (b != null) return b;
        return root.Query<Button>().Where(x => {
            string n = ((x.name ?? "") + " " + (x.text ?? "")).ToLower();
            foreach (var p in palabras) if (n.Contains(p)) return true;
            return false;
        }).First();
    }
}
