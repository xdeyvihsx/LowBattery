using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class TutorialPage
{
    [Header("Texto narrativo (vacio = oculto en juego)")]
    public string loreLine1 = "";
    public string loreLine2 = "";
    public string loreLine3 = "";
    public string loreLine4 = "";
    [Header("Etiquetas de paneles (vacio = oculto en juego)")]
    public string labelLeft  = "";
    public string labelRight = "";
    [Header("Silhouettes (null = oculto en juego)")]
    public Sprite silhouetteLeft;
    public Sprite silhouetteRight;
    [Header("Imagenes de input (null = oculto en juego)")]
    public Sprite imageLeft;
    public Sprite imageRight;
    [Header("Hints de input (vacio = oculto en juego)")]
    public string inputLeft  = "";
    public string inputRight = "";
}

[RequireComponent(typeof(UIDocument))]
public class TutorialController : MonoBehaviour
{
    [Header("Paginas del tutorial")]
    [SerializeField] private List<TutorialPage> pages = new List<TutorialPage>();
    [Header("Sorting Order")]
    public int sortingOrder = 100;
    [Header("Control de primera vez")]
    [Tooltip("Clave unica PlayerPrefs para este tutorial.")]
    public string clavePlayerPrefs = "tutorial_visto_Level_1";
    [Tooltip("Forzar tutorial en Editor aunque ya fue visto (solo para testing).")]
    public bool forzarEnEditor = false;

    private const string ID_LORE1       = "LoreLine1";
    private const string ID_LORE2       = "LoreLine2";
    private const string ID_LORE3       = "LoreLine3";
    private const string ID_LORE4       = "LoreLine4";
    private const string ID_LABEL_LEFT  = "LabelRun";
    private const string ID_LABEL_RIGHT = "LabelJump";
    private const string ID_SIL_LEFT    = "SilhouetteRun";
    private const string ID_SIL_RIGHT   = "SilhouetteJump";
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

        bool yaVisto     = PlayerPrefs.GetInt(clavePlayerPrefs, 0) == 1;
        bool debeMostrar = !yaVisto;

#if UNITY_EDITOR
        if (forzarEnEditor) { debeMostrar = true; Debug.Log("[Tutorial] Forzado en Editor."); }
#endif

        if (!debeMostrar)
        {
            Debug.Log("[Tutorial] Ya completado — omitiendo. Clave: " + clavePlayerPrefs);
            Time.timeScale      = 1f;
            AudioListener.pause = false;
            gameObject.SetActive(false);
            return;
        }

        Time.timeScale      = 0f;
        AudioListener.pause = true;
        Debug.Log("[Tutorial] Primera vez — mostrando tutorial.");
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
        for (int i = 0; i < 4; i++) dots[i] = root.Q<VisualElement>("Dot" + (i + 1));
        var bC = BuscarBtn(root, ID_BTN_CONT, "Continue");
        var bN = BuscarBtn(root, ID_BTN_NEXT, "Next");
        var bP = BuscarBtn(root, ID_BTN_PREV, "Previous");
        if (bC != null) bC.RegisterCallback<ClickEvent>(_ => Continuar());
        if (bN != null) bN.RegisterCallback<ClickEvent>(_ => CambiarPagina(+1));
        if (bP != null) bP.RegisterCallback<ClickEvent>(_ => CambiarPagina(-1));
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
        SetLabelVisible(root, ID_LORE1,       p.loreLine1);
        SetLabelVisible(root, ID_LORE2,       p.loreLine2);
        SetLabelVisible(root, ID_LORE3,       p.loreLine3);
        SetLabelVisible(root, ID_LORE4,       p.loreLine4);
        SetLabelVisible(root, ID_LABEL_LEFT,  p.labelLeft);
        SetLabelVisible(root, ID_LABEL_RIGHT, p.labelRight);
        SetSpriteVisible(root, ID_SIL_LEFT,  p.silhouetteLeft);
        SetSpriteVisible(root, ID_SIL_RIGHT, p.silhouetteRight);
        SetSpriteVisible(root, ID_IMG_LEFT,  p.imageLeft);
        SetSpriteVisible(root, ID_IMG_RIGHT, p.imageRight);
        ActualizarDots();
        var wP = root.Q<VisualElement>(ID_WRAP_PREV);
        var wN = root.Q<VisualElement>(ID_WRAP_NEXT);
        if (wP != null) wP.style.opacity = paginaActual > 0 ? 1f : 0.3f;
        if (wN != null) wN.style.opacity = paginaActual < pages.Count - 1 ? 1f : 0.3f;
    }

    void SetLabelVisible(VisualElement root, string id, string texto)
    {
        var l = root.Q<Label>(id); if (l == null) return;
        bool tiene = !string.IsNullOrWhiteSpace(texto);
        l.style.display = tiene ? DisplayStyle.Flex : DisplayStyle.None;
        if (tiene) l.text = texto;
    }

    void SetSpriteVisible(VisualElement root, string id, Sprite sprite)
    {
        var el = root.Q<VisualElement>(id); if (el == null) return;
        if (sprite != null)
        { el.style.display = DisplayStyle.Flex; el.style.backgroundImage = new StyleBackground(sprite); }
        else el.style.display = DisplayStyle.None;
    }

    void ActualizarDots()
    {
        if (dots == null) return;
        for (int i = 0; i < dots.Length; i++)
        {
            if (dots[i] == null) continue;
            if (i >= pages.Count) { dots[i].style.display = DisplayStyle.None; continue; }
            dots[i].style.display = DisplayStyle.Flex;
            bool a = (i == paginaActual);
            dots[i].style.width = a ? 16 : 5; dots[i].style.height = 5;
            float r = a ? 3 : 50;
            dots[i].style.borderTopLeftRadius = dots[i].style.borderTopRightRadius =
            dots[i].style.borderBottomLeftRadius = dots[i].style.borderBottomRightRadius = r;
            dots[i].style.backgroundColor = new StyleColor(a ? DOT_ACT : DOT_INAC);
        }
    }

    void CambiarPagina(int dir)
    { int n = paginaActual + dir; if (n >= 0 && n < pages.Count) MostrarPagina(n); }

    void Continuar()
    {
        if (terminado) return;
        terminado = true;
        PlayerPrefs.SetInt(clavePlayerPrefs, 1);
        PlayerPrefs.Save();
        Debug.Log("[Tutorial] Guardado en PlayerPrefs: " + clavePlayerPrefs + " = 1");
        if (doc?.rootVisualElement != null)
            doc.rootVisualElement.style.display = DisplayStyle.None;
        AudioListener.pause = false;
        Time.timeScale      = 1f;
        if (movimiento != null) movimiento.estaMuerto = false;
        sfxPlayer?.ReanudarAudio();
        audioNivel?.ReanudarAudio();
        if (MenuMusicManager.Instance != null) MenuMusicManager.Instance.ReanudarMusica();
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

    Button BuscarBtn(VisualElement root, params string[] ids)
    {
        foreach (var id in ids) { var b = root.Q<Button>(id); if (b != null) return b; }
        foreach (var id in ids)
        {
            var b = root.Query<Button>().Where(x => (x.name ?? "").ToLower().Contains(id.ToLower())).First();
            if (b != null) return b;
        }
        return null;
    }

    // Resetear tutorial desde opciones o debug
    public static void ResetearTutorial(string clave = "tutorial_visto_Level_1")
    {
        PlayerPrefs.DeleteKey(clave);
        PlayerPrefs.Save();
        Debug.Log("[Tutorial] Reseteado. Aparecera la proxima vez.");
    }
}