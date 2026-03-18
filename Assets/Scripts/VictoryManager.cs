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
    public string escenaMenu = "GlobalLevels";

    [Header("Sorting Order")]
    public int sortingOrder = 50;

    private const string ID_BTN_MENU  = "BtnMainMenu";
    private const string ID_BTN_RETRY = "BtnTryAgain";
    private const string ID_ROW_MENU  = "MainMenuRow";
    private const string ID_ROW_RETRY = "TryAgainRow";
    private const string ID_ARR_LM   = "ArrowL_Menu";
    private const string ID_ARR_RM   = "ArrowR_Menu";
    private const string ID_ARR_LT   = "ArrowL_Try";
    private const string ID_ARR_RT   = "ArrowR_Try";
    // ID exacto del label de muertes en LegendUI.uxml
    private const string ID_DEATHS   = "StatDeathsValue";

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

    void Start()
    {
        // Registrar botones usando tiempo real para no depender de timeScale
        StartCoroutine(RegistrarDelay());
    }

    IEnumerator RegistrarDelay()
    {
        // Esperar un frame real para que el UIDocument cargue
        yield return new WaitForSecondsRealtime(0.1f);
        Registrar();
    }

    void Registrar()
    {
        if (uiVictory?.rootVisualElement == null || registrado) return;
        var root = uiVictory.rootVisualElement;

        var rowMenu  = root.Q<VisualElement>(ID_ROW_MENU);
        var rowRetry = root.Q<VisualElement>(ID_ROW_RETRY);
        var btnMenu  = root.Q<Button>(ID_BTN_MENU);
        var btnRetry = root.Q<Button>(ID_BTN_RETRY);

        // Fallback por nombre parcial
        if (rowMenu  == null) rowMenu  = Buscar(root, "MainMenuRow",  "menurow");
        if (rowRetry == null) rowRetry = Buscar(root, "TryAgainRow",  "tryrow");
        if (btnMenu  == null) btnMenu  = (Button)Buscar(root, "BtnMainMenu",  "mainmenu");
        if (btnRetry == null) btnRetry = (Button)Buscar(root, "BtnTryAgain",  "tryagain");

        if (rowMenu  != null) { rowMenu.RegisterCallback<MouseEnterEvent>(_ => Hover(root, true, false)); rowMenu.RegisterCallback<MouseLeaveEvent>(_ => Hover(root, false, false)); }
        if (rowRetry != null) { rowRetry.RegisterCallback<MouseEnterEvent>(_ => Hover(root, false, true)); rowRetry.RegisterCallback<MouseLeaveEvent>(_ => Hover(root, false, false)); }
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
        FindFirstObjectByType<LevelAudioManager>()?.PausarAudio();

        // Congelar tiempo
        Time.timeScale = 0f;

        // Mostrar panel
        if (uiVictory != null) uiVictory.sortingOrder = sortingOrder;
        if (uiVictory?.rootVisualElement != null)
            uiVictory.rootVisualElement.style.display = DisplayStyle.Flex;

        // Actualizar el label de muertes usando tiempo real
        // (WaitForSecondsRealtime ignora Time.timeScale = 0)
        StartCoroutine(ActualizarDeathsDelay());
    }

    // Espera que el panel sea visible y luego actualiza el texto
    IEnumerator ActualizarDeathsDelay()
    {
        // Esperar 2 frames reales para que UI Toolkit renderice el panel
        yield return new WaitForSecondsRealtime(0.05f);
        yield return new WaitForSecondsRealtime(0.05f);

        if (uiVictory?.rootVisualElement == null) yield break;
        var root = uiVictory.rootVisualElement;

        // Buscar el label por ID exacto
        var label = root.Q<Label>(ID_DEATHS);

        // Fallback por nombre parcial si el ID no coincide
        if (label == null)
        {
            label = root.Query<Label>().Where(l =>
                (l.name ?? "").ToLower().Contains("death") ||
                (l.name ?? "").ToLower().Contains("deathvalue") ||
                (l.name ?? "").ToLower().Contains("statdeath")).First();
        }

        if (label != null)
        {
            label.text = PlayerDeath.TotalMuertes.ToString();
            Debug.Log("[Victory] StatDeathsValue actualizado a: " + PlayerDeath.TotalMuertes);
        }
        else
        {
            Debug.LogWarning("[Victory] No encontre label con ID '" + ID_DEATHS + "'. Labels disponibles:");
            // Imprimir todos los labels para diagnostico
            foreach (var l in root.Query<Label>().ToList())
                Debug.Log("  Label: name='" + l.name + "' text='" + l.text + "'");
        }
    }

    void IrAlMenu()       { Time.timeScale = 1f; SceneManager.LoadScene(escenaMenu); }
    void ReiniciarNivel() { Time.timeScale = 1f; SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); }

    void Ocultar()
    {
        if (uiVictory?.rootVisualElement != null)
            uiVictory.rootVisualElement.style.display = DisplayStyle.None;
    }

    void Hover(VisualElement root, bool menu, bool retry)
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
        foreach (var id in ids)
        {
            var e = root.Query<VisualElement>().Where(x =>
                (x.name ?? "").ToLower().Contains(id.ToLower())).First();
            if (e != null) return e;
        }
        return null;
    }
}
