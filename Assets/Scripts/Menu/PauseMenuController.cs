using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenuController : MonoBehaviour
{
    [Header("UI Document del Pause")]
    [SerializeField] private UIDocument uiDocument;

    [Header("Escenas")]
    public string escenaGlobalLevels = "GlobalLevels";

    private const string ID_BTN_CONTINUE = "BtnContinue";
    private const string ID_BTN_OPTIONS  = "BtnOptions";
    private const string ID_BTN_QUIT     = "BtnQuitToMenu";
    private const string ID_ARROW_L      = "ArrowL";
    private const string ID_ARROW_R      = "ArrowR";

    private bool pausado = false;
    private Button btnContinue;
    private Button btnOptions;
    private Button btnQuit;
    private VisualElement arrowL;
    private VisualElement arrowR;
    private Button[] botones;
    private int indexSeleccionado = 0;

    private static readonly Color COLOR_ACTIVO   = new Color(0.94f, 0.95f, 1.00f, 1f);
    private static readonly Color COLOR_INACTIVO = new Color(0.72f, 0.74f, 0.88f, 0.7f);

    void Awake()
    {
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();
    }

    void OnEnable()
    {
        if (uiDocument == null) return;
        var root = uiDocument.rootVisualElement;
        if (root == null) return;

        btnContinue = root.Q<Button>(ID_BTN_CONTINUE);
        btnOptions  = root.Q<Button>(ID_BTN_OPTIONS);
        btnQuit     = root.Q<Button>(ID_BTN_QUIT);
        arrowL      = root.Q<VisualElement>(ID_ARROW_L);
        arrowR      = root.Q<VisualElement>(ID_ARROW_R);

        if (btnContinue == null) btnContinue = BuscarBoton(root, "continue", "continuar", "resume");
        if (btnOptions  == null) btnOptions  = BuscarBoton(root, "option", "opciones");
        if (btnQuit     == null) btnQuit     = BuscarBoton(root, "quit", "menu", "salir");

        botones = new Button[] { btnContinue, btnOptions, btnQuit };

        for (int i = 0; i < botones.Length; i++)
        {
            if (botones[i] == null) continue;
            int idx = i;
            botones[i].RegisterCallback<MouseEnterEvent>(_ => SeleccionarBoton(idx));
        }

        if (btnContinue != null) btnContinue.RegisterCallback<ClickEvent>(_ => Continuar());
        if (btnOptions  != null) btnOptions.RegisterCallback<ClickEvent>(_ => Debug.Log("[Pause] Opciones"));
        if (btnQuit     != null) btnQuit.RegisterCallback<ClickEvent>(_ => SalirANiveles());

        MostrarPanel(false);
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (pausado) Continuar();
            else         Pausar();
            return;
        }

        if (!pausado) return;

        if (Keyboard.current.downArrowKey.wasPressedThisFrame ||
            Keyboard.current.sKey.wasPressedThisFrame)
            SeleccionarBoton((indexSeleccionado + 1) % botones.Length);

        if (Keyboard.current.upArrowKey.wasPressedThisFrame ||
            Keyboard.current.wKey.wasPressedThisFrame)
            SeleccionarBoton((indexSeleccionado - 1 + botones.Length) % botones.Length);

        if (Keyboard.current.enterKey.wasPressedThisFrame)
            EjecutarSeleccionado();
    }

    void Pausar()
    {
        pausado        = true;
        Time.timeScale = 0f;
        MostrarPanel(true);
        SeleccionarBoton(0);
    }

    void Continuar()
    {
        pausado        = false;
        Time.timeScale = 1f;
        MostrarPanel(false);
    }

    void SalirANiveles()
    {
        pausado        = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(escenaGlobalLevels);
    }

    void SeleccionarBoton(int index)
    {
        indexSeleccionado = index;
        for (int i = 0; i < botones.Length; i++)
        {
            if (botones[i] == null) continue;
            bool activo = (i == index);
            botones[i].style.color   = new StyleColor(activo ? COLOR_ACTIVO : COLOR_INACTIVO);
            botones[i].style.opacity = activo ? 1f : 0.65f;
        }
        MoverFlechas(botones[index]);
    }

    void EjecutarSeleccionado()
    {
        switch (indexSeleccionado)
        {
            case 0: Continuar();                        break;
            case 1: Debug.Log("[Pause] Opciones");      break;
            case 2: SalirANiveles();                    break;
        }
    }

    void MoverFlechas(VisualElement boton)
    {
        if (boton == null) return;
        var padre = boton.parent;
        if (padre == null) return;
        if (arrowL != null) { arrowL.RemoveFromHierarchy(); padre.Insert(0, arrowL); arrowL.style.opacity = 1f; }
        if (arrowR != null) { arrowR.RemoveFromHierarchy(); padre.Add(arrowR);       arrowR.style.opacity = 1f; }
    }

    void MostrarPanel(bool mostrar)
    {
        if (uiDocument == null) return;
        uiDocument.rootVisualElement.style.display =
            mostrar ? DisplayStyle.Flex : DisplayStyle.None;
    }

    void OnDestroy()
    {
        if (pausado) Time.timeScale = 1f;
    }

    Button BuscarBoton(VisualElement root, params string[] palabras)
    {
        return root.Query<Button>().Where(b => {
            string n = ((b.name ?? "") + " " + (b.text ?? "")).ToLower();
            foreach (var p in palabras) if (n.Contains(p)) return true;
            return false;
        }).First();
    }
}
