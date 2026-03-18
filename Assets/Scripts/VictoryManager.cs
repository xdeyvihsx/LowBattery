using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections;

public class VictoryManager : MonoBehaviour
{
    public static VictoryManager Instance { get; private set; }

    [Header("UIDocument de victoria (LegendUI)")]
    public UIDocument uiVictory;

    [Header("Escenas")]
    public string escenaMenu = "Menu";

    [Header("Sorting Order")]
    public int sortingOrder = 50;

    private const string ID_BTN_MENU   = "BtnMainMenu";
    private const string ID_BTN_RETRY  = "BtnTryAgain";
    private const string ID_ROW_MENU   = "MainMenuRow";
    private const string ID_ROW_RETRY  = "TryAgainRow";
    private const string ID_ARR_LM     = "ArrowL_Menu";
    private const string ID_ARR_RM     = "ArrowR_Menu";
    private const string ID_ARR_LT     = "ArrowL_Try";
    private const string ID_ARR_RT     = "ArrowR_Try";

    private static readonly Color C_ACT  = new Color(0.94f, 0.95f, 1.00f, 1f);
    private static readonly Color C_INAC = new Color(0.72f, 0.74f, 0.88f, 0.7f);
    private static readonly Color I_VIS  = new Color(1f, 1f, 1f, 1f);
    private static readonly Color I_HID  = new Color(1f, 1f, 1f, 0f);

    private bool registrado = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (uiVictory != null) uiVictory.sortingOrder = sortingOrder;
        Ocultar();
    }

    void Start() { StartCoroutine(RegistrarDelay()); }

    IEnumerator RegistrarDelay() { yield return null; Registrar(); }

    void Registrar()
    {
        if (uiVictory?.rootVisualElement == null || registrado) return;
        var root = uiVictory.rootVisualElement;

        var rowMenu  = Buscar(root, ID_ROW_MENU,  "MainMenuRow",  "menurow");
        var rowRetry = Buscar(root, ID_ROW_RETRY, "TryAgainRow",  "tryrow");
        var btnMenu  = Buscar(root, ID_BTN_MENU,  "BtnMainMenu",  "mainmenu");
        var btnRetry = Buscar(root, ID_BTN_RETRY, "BtnTryAgain",  "tryagain");

        if (rowMenu  != null)
        {
            rowMenu.RegisterCallback<MouseEnterEvent>(_ => HoverFila(root, true,  false));
            rowMenu.RegisterCallback<MouseLeaveEvent>(_ => HoverFila(root, false, false));
        }
        if (rowRetry != null)
        {
            rowRetry.RegisterCallback<MouseEnterEvent>(_ => HoverFila(root, false, true));
            rowRetry.RegisterCallback<MouseLeaveEvent>(_ => HoverFila(root, false, false));
        }

        if (btnMenu  != null) btnMenu.RegisterCallback<ClickEvent>(_ => IrAlMenu());
        if (btnRetry != null) btnRetry.RegisterCallback<ClickEvent>(_ => ReiniciarNivel());

        registrado = true;
        Debug.Log("[Victory] Registrado. Menu=" + (btnMenu != null) + " Retry=" + (btnRetry != null));
    }

    public void MostrarVictoria()
    {
        // Detener player
        PlayerMovement mov = FindFirstObjectByType<PlayerMovement>();
        if (mov != null) mov.estaMuerto = true;

        // Detener bateria
        if (PlayerData.Instance != null) PlayerData.Instance.SetPausado(true);

        // Detener audio
        LevelAudioManager audio = FindFirstObjectByType<LevelAudioManager>();
        audio?.PausarAudio();

        // Pausar tiempo
        Time.timeScale = 0f;

        // Mostrar UI
        if (uiVictory != null) uiVictory.sortingOrder = sortingOrder;
        if (uiVictory?.rootVisualElement != null)
            uiVictory.rootVisualElement.style.display = DisplayStyle.Flex;

        Debug.Log("[Victory] Mostrado!");
    }

    void IrAlMenu()   { Time.timeScale = 1f; SceneManager.LoadScene(escenaMenu); }
    void ReiniciarNivel() { Time.timeScale = 1f; SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); }

    void Ocultar()
    {
        if (uiVictory?.rootVisualElement != null)
            uiVictory.rootVisualElement.style.display = DisplayStyle.None;
    }

    void HoverFila(VisualElement root, bool menu, bool retry)
    {
        SetCol(root, ID_ARR_LM, menu  ? I_VIS : I_HID);
        SetCol(root, ID_ARR_RM, menu  ? I_VIS : I_HID);
        SetCol(root, ID_ARR_LT, retry ? I_VIS : I_HID);
        SetCol(root, ID_ARR_RT, retry ? I_VIS : I_HID);

        var bm = Buscar(root, ID_BTN_MENU,  "mainmenu");
        var br = Buscar(root, ID_BTN_RETRY, "tryagain");
        if (bm != null) bm.style.color = new StyleColor(menu  ? C_ACT : C_INAC);
        if (br != null) br.style.color = new StyleColor(retry ? C_ACT : C_INAC);
    }

    void SetCol(VisualElement root, string id, Color c)
    { var e = root.Q<VisualElement>(id); if (e != null) e.style.color = new StyleColor(c); }

    VisualElement Buscar(VisualElement root, params string[] ids)
    {
        foreach (var id in ids) { var e = root.Q<VisualElement>(id); if (e != null) return e; }
        foreach (var id in ids) { var e = root.Query<VisualElement>().Where(x => (x.name ?? "").ToLower().Contains(id.ToLower())).First(); if (e != null) return e; }
        return null;
    }
}
