using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class TutorialPage
{
    [Header("Texto narrativo")]
    public string loreLine1 = "";
    public string loreLine2 = "";
    [Header("Etiquetas de paneles")]
    public string labelLeft  = "";
    public string labelRight = "";
    [Header("Silhouettes")]
    public Sprite silhouetteLeft;
    public Sprite silhouetteRight;
    [Header("Hints de input")]
    public string inputLeft  = "";
    public string inputRight = "";
}

[RequireComponent(typeof(UIDocument))]
public class TutorialController : MonoBehaviour
{
    [Header("Paginas del tutorial")]
    [SerializeField] private List<TutorialPage> pages = new List<TutorialPage>();

    [Header("Sorting Order (encima de todo)")]
    public int sortingOrder = 100;

    private const string ID_LORE1       = "LoreLine1";
    private const string ID_LORE2       = "LoreLine2";
    private const string ID_LABEL_LEFT  = "LabelRun";
    private const string ID_LABEL_RIGHT = "LabelJump";
    private const string ID_SIL_LEFT    = "SilhouetteRun";
    private const string ID_SIL_RIGHT   = "SilhouetteJump";
    private const string ID_BTN_CONT    = "BtnContinue";
    private const string ID_BTN_NEXT    = "BtnNext";
    private const string ID_BTN_PREV    = "BtnPrevious";
    private const string ID_WRAP_NEXT   = "BtnNextWrap";
    private const string ID_WRAP_PREV   = "BtnPrevWrap";

    private static readonly Color DOT_ACT  = new Color(1f, 1f, 1f, 0.85f);
    private static readonly Color DOT_INAC = new Color(1f, 1f, 1f, 0.25f);

    private UIDocument      doc;
    private VisualElement[] dots;
    private int             paginaActual = 0;
    private bool            terminado    = false;

    private PlayerMovement        movimiento;
    private PlayerSoundController sfxPlayer;
    private LevelAudioManager     audioNivel;

    void Awake()
    {
        doc = GetComponent<UIDocument>();
        if (doc != null) doc.sortingOrder = sortingOrder;
    }

    void Start()
    {
        movimiento = FindFirstObjectByType<PlayerMovement>();
        sfxPlayer  = FindFirstObjectByType<PlayerSoundController>();
        audioNivel = FindFirstObjectByType<LevelAudioManager>();
        IniciarTutorial();
    }

    void IniciarTutorial()
    {
        Time.timeScale = 0f;
        if (movimiento != null) movimiento.estaMuerto = true;
        sfxPlayer?.PausarAudio();
        audioNivel?.PausarAudio();
        if (doc?.rootVisualElement != null)
            doc.rootVisualElement.style.display = DisplayStyle.Flex;
        StartCoroutine(RegistrarDelay());
    }

    IEnumerator RegistrarDelay()
    {
        yield return new WaitForSecondsRealtime(0.1f);
        Registrar();
    }

    void Registrar()
    {
        if (doc?.rootVisualElement == null) return;
        var root = doc.rootVisualElement;

        dots = new VisualElement[4];
        for (int i = 0; i < 4; i++)
            dots[i] = root.Q<VisualElement>("Dot" + (i + 1));

        var btnCont = BuscarBtn(root, ID_BTN_CONT, "Continue");
        var btnNext = BuscarBtn(root, ID_BTN_NEXT, "Next");
        var btnPrev = BuscarBtn(root, ID_BTN_PREV, "Previous");

        if (btnCont != null) btnCont.RegisterCallback<ClickEvent>(_ => Continuar());
        if (btnNext != null) btnNext.RegisterCallback<ClickEvent>(_ => CambiarPagina(+1));
        if (btnPrev != null) btnPrev.RegisterCallback<ClickEvent>(_ => CambiarPagina(-1));

        Debug.Log("[Tutorial] Cont=" + (btnCont != null) + " Next=" + (btnNext != null) + " Prev=" + (btnPrev != null));
        MostrarPagina(0);
    }

    void MostrarPagina(int idx)
    {
        if (pages == null || pages.Count == 0) return;
        paginaActual = Mathf.Clamp(idx, 0, pages.Count - 1);
        var root = doc.rootVisualElement;
        var p    = pages[paginaActual];

        SetLabel(root, ID_LORE1,       p.loreLine1);
        SetLabel(root, ID_LORE2,       p.loreLine2);
        SetLabel(root, ID_LABEL_LEFT,  p.labelLeft);
        SetLabel(root, ID_LABEL_RIGHT, p.labelRight);

        if (p.silhouetteLeft != null)
        { var s = root.Q<VisualElement>(ID_SIL_LEFT);  if (s != null) s.style.backgroundImage = new StyleBackground(p.silhouetteLeft); }
        if (p.silhouetteRight != null)
        { var s = root.Q<VisualElement>(ID_SIL_RIGHT); if (s != null) s.style.backgroundImage = new StyleBackground(p.silhouetteRight); }

        ActualizarDots();

        var wP = root.Q<VisualElement>(ID_WRAP_PREV);
        var wN = root.Q<VisualElement>(ID_WRAP_NEXT);
        if (wP != null) wP.style.opacity = paginaActual > 0 ? 1f : 0.3f;
        if (wN != null) wN.style.opacity = paginaActual < pages.Count - 1 ? 1f : 0.3f;
    }

    void ActualizarDots()
    {
        if (dots == null) return;
        for (int i = 0; i < dots.Length; i++)
        {
            if (dots[i] == null) continue;
            bool a = (i == paginaActual);
            dots[i].style.width = a ? 16 : 5;
            dots[i].style.height = 5;
            float r = a ? 3 : 50;
            dots[i].style.borderTopLeftRadius = dots[i].style.borderTopRightRadius =
            dots[i].style.borderBottomLeftRadius = dots[i].style.borderBottomRightRadius = r;
            dots[i].style.backgroundColor = new StyleColor(a ? DOT_ACT : DOT_INAC);
        }
    }

    void CambiarPagina(int dir)
    {
        int n = paginaActual + dir;
        if (n >= 0 && n < pages.Count) MostrarPagina(n);
    }

    void Continuar()
    {
        if (terminado) return;
        terminado = true;
        if (doc?.rootVisualElement != null)
            doc.rootVisualElement.style.display = DisplayStyle.None;
        Time.timeScale = 1f;
        if (movimiento != null) movimiento.estaMuerto = false;
        sfxPlayer?.ReanudarAudio();
        audioNivel?.ReanudarAudio();
        Debug.Log("[Tutorial] Completado.");
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (terminado || Keyboard.current == null) return;
        if (Keyboard.current.rightArrowKey.wasPressedThisFrame || Keyboard.current.dKey.wasPressedThisFrame) CambiarPagina(+1);
        if (Keyboard.current.leftArrowKey.wasPressedThisFrame  || Keyboard.current.aKey.wasPressedThisFrame) CambiarPagina(-1);
        if (Keyboard.current.enterKey.wasPressedThisFrame) Continuar();
    }

    void SetLabel(VisualElement root, string id, string texto)
    { var l = root.Q<Label>(id); if (l != null) l.text = texto; }

    Button BuscarBtn(VisualElement root, params string[] ids)
    {
        foreach (var id in ids) { var b = root.Q<Button>(id); if (b != null) return b; }
        foreach (var id in ids) { var b = root.Query<Button>().Where(x => (x.name ?? "").ToLower().Contains(id.ToLower())).First(); if (b != null) return b; }
        return null;
    }
}
