using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class TutorialPage
{
    public string loreLine1 = "";
    public string loreLine2 = "";
    public string labelLeft  = "";
    public string labelRight = "";
    public Sprite silhouetteLeft;
    public Sprite silhouetteRight;
    public string inputLeft  = "";
    public string inputRight = "";
    public string loreLine3 = "";
    public string loreLine4 = "";
    public Sprite leftButton;
    public Sprite rightButton;
}

[RequireComponent(typeof(UIDocument))]
public class TutorialController : MonoBehaviour
{
    [SerializeField] private List<TutorialPage> pages = new List<TutorialPage>();
    public int sortingOrder = 100;

    private const string ID_LORE1       = "LoreLine1";
    private const string ID_LORE2       = "LoreLine2";
    private const string ID_LABEL_LEFT  = "LabelRun";
    private const string ID_LABEL_RIGHT = "LabelJump";
    private const string ID_SIL_LEFT    = "SilhouetteRun";
    private const string ID_SIL_RIGHT   = "SilhouetteJump";
    private const string ID_LORE3       = "LoreLine3";
    private const string ID_LORE4       = "LoreLine4";
    private const string ID_IMG_LEFT    = "LeftButton";
    private const string ID_IMG_RIGHT   = "RightButton";
    private const string ID_BTN_CONT    = "BtnContinue";
    private const string ID_BTN_NEXT    = "BtnNext";
    private const string ID_BTN_PREV    = "BtnPrevious";
    private const string ID_WRAP_NEXT   = "BtnNextWrap";
    private const string ID_WRAP_PREV   = "BtnPrevWrap";

    private static readonly Color DOT_ACT  = new Color(1f, 1f, 1f, 0.85f);
    private static readonly Color DOT_INAC = new Color(1f, 1f, 1f, 0.25f);

    private UIDocument      doc;
    private VisualElement[] dots;
    private int  paginaActual = 0;
    private bool terminado    = false;
    private bool registrado   = false;

    private PlayerMovement        movimiento;
    private PlayerSoundController sfxPlayer;
    private LevelAudioManager     audioNivel;

    void Awake()
    {
        doc = GetComponent<UIDocument>();
        if (doc != null) doc.sortingOrder = sortingOrder;
        // Pausar en Awake antes de que otros Start() corran
        Time.timeScale      = 0f;
        AudioListener.pause = true;
    }

    void Start()
    {
        movimiento = FindFirstObjectByType<PlayerMovement>();
        sfxPlayer  = FindFirstObjectByType<PlayerSoundController>();
        audioNivel = FindFirstObjectByType<LevelAudioManager>();

        if (movimiento != null) movimiento.estaMuerto = true;
        sfxPlayer?.PausarAudio();
        audioNivel?.PausarAudio();
        if (MenuMusicManager.Instance != null) MenuMusicManager.Instance.PausarMusica();

        if (doc?.rootVisualElement != null)
            doc.rootVisualElement.style.display = DisplayStyle.Flex;

        StartCoroutine(RegistrarDelay());
    }

    IEnumerator RegistrarDelay()
    {
        yield return new WaitForSecondsRealtime(0.05f);
        Registrar();
    }

    void Registrar()
    {
        if (doc?.rootVisualElement == null || registrado) return;
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

        registrado = true;
        MostrarPagina(0);
        Debug.Log("[Tutorial] Listo. Paginas=" + pages.Count);
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
        SetLabel(root, ID_LORE3,       p.loreLine3);
        SetLabel(root, ID_LORE4,       p.loreLine4);

        if (p.silhouetteLeft  != null) { var s = root.Q<VisualElement>(ID_SIL_LEFT);  if (s != null) s.style.backgroundImage = new StyleBackground(p.silhouetteLeft);  }
        if (p.silhouetteRight != null) { var s = root.Q<VisualElement>(ID_SIL_RIGHT); if (s != null) s.style.backgroundImage = new StyleBackground(p.silhouetteRight); }
        if (p.leftButton     != null) { var s = root.Q<VisualElement>(ID_IMG_LEFT);    if (s != null) s.style.backgroundImage = new StyleBackground(p.leftButton);     }
        if (p.rightButton    != null) { var s = root.Q<VisualElement>(ID_IMG_RIGHT);   if (s != null) s.style.backgroundImage = new StyleBackground(p.rightButton);    }

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
            dots[i].style.width = a ? 16 : 5; dots[i].style.height = 5;
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

        // Reanudar TODO
        AudioListener.pause = false;
        Time.timeScale      = 1f;

        if (movimiento != null) movimiento.estaMuerto = false;
        sfxPlayer?.ReanudarAudio();
        audioNivel?.ReanudarAudio();
        if (MenuMusicManager.Instance != null) MenuMusicManager.Instance.ReanudarMusica();

        Debug.Log("[Tutorial] Completado — juego reanudado.");
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
