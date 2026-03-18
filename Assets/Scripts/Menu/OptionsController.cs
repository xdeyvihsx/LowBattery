using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class OptionsController : MonoBehaviour
{
    [Header("UIDocuments de paneles")]
    public UIDocument docOpciones;
    public UIDocument docAudio;
    public UIDocument docVideo;

    [Header("Escenas")]
    public string escenaMenu = "Menu";

    [Header("Velocidad fade iconos en segundos")]
    public float velocidadFade = 0.18f;

    // IDs exactos del OptionsUI.uxml (confirmados en UI Builder)
    private const string ID_ROW_GAME  = "GameRow";
    private const string ID_ROW_AUDIO = "AudioRow";
    private const string ID_ROW_VIDEO = "VideoRow";
    private const string ID_BACK      = "BackBtn";

    // Icono izq/der dentro de cada row
    private const string ID_ICON_L = "StartIconLeft";
    private const string ID_ICON_R = "StartIconRight";

    // Estado
    private List<VisualElement>                    rows  = new List<VisualElement>();
    private Dictionary<VisualElement, Coroutine>   fades = new Dictionary<VisualElement, Coroutine>();
    private UIDocument                             panelActivo = null;

    void OnEnable()
    {
        OcultarPanel(docAudio);
        OcultarPanel(docVideo);

        if (docOpciones == null) docOpciones = GetComponent<UIDocument>();
        if (docOpciones == null) { Debug.LogError("[Options] Falta docOpciones."); return; }

        var root = docOpciones.rootVisualElement;
        if (root == null) return;

        rows.Clear();
        fades.Clear();

        // Registrar cada row con su accion y fade de iconos
        BindRow(root, ID_ROW_GAME,  () => Debug.Log("[Options] Game — implementar panel"));
        BindRow(root, ID_ROW_AUDIO, () => MostrarPanel(docAudio));
        BindRow(root, ID_ROW_VIDEO, () => MostrarPanel(docVideo));

        // Boton Back → volver al Menu
        var btnBack = root.Q<VisualElement>(ID_BACK);
        if (btnBack != null)
        {
            btnBack.RegisterCallback<MouseEnterEvent>(_ => btnBack.style.opacity = 0.6f);
            btnBack.RegisterCallback<MouseLeaveEvent>(_ => btnBack.style.opacity = 1f);
            btnBack.RegisterCallback<ClickEvent>(_ => SceneManager.LoadScene(escenaMenu));
        }
        else
            Debug.LogWarning("[Options] No encontre BackBtn.");

        // Ocultar todos los iconos al inicio
        foreach (var row in rows) SetOpacidadIconos(row, 0f);
    }

    // ── Bind de row con hover + click ─────────────────────────
    void BindRow(VisualElement root, string rowId, System.Action accion)
    {
        var row = root.Q<VisualElement>(rowId);
        if (row == null)
        {
            Debug.LogWarning("[Options] No encontre row: " + rowId);
            return;
        }

        rows.Add(row);

        row.RegisterCallback<MouseEnterEvent>(_ =>
        {
            // Ocultar iconos de los demas rows
            foreach (var r in rows) if (r != row) IniciarFade(r, 0f);
            // Mostrar iconos de este row
            IniciarFade(row, 1f);
        });

        row.RegisterCallback<MouseLeaveEvent>(_ => IniciarFade(row, 0f));
        row.RegisterCallback<ClickEvent>(_ => accion?.Invoke());
    }

    // ── Fade suave de iconos ───────────────────────────────────
    void IniciarFade(VisualElement row, float objetivo)
    {
        if (fades.TryGetValue(row, out var c) && c != null)
            StopCoroutine(c);
        fades[row] = StartCoroutine(DoFade(row, objetivo));
    }

    IEnumerator DoFade(VisualElement row, float objetivo)
    {
        var izq = row.Q<VisualElement>(ID_ICON_L);
        var der = row.Q<VisualElement>(ID_ICON_R);

        // Fallback: primer y ultimo hijo si no se encuentran por ID
        if (izq == null && der == null)
        {
            var hijos = new List<VisualElement>();
            foreach (var h in row.Children()) hijos.Add(h);
            if (hijos.Count >= 1) izq = hijos[0];
            if (hijos.Count >= 2) der = hijos[hijos.Count - 1];
        }

        float desde = izq != null ? izq.resolvedStyle.opacity : 0f;
        float t = 0f;

        while (t < velocidadFade)
        {
            t += Time.deltaTime;
            float op = Mathf.Lerp(desde, objetivo, Mathf.Clamp01(t / velocidadFade));
            if (izq != null) izq.style.opacity = op;
            if (der != null) der.style.opacity = op;
            yield return null;
        }

        if (izq != null) izq.style.opacity = objetivo;
        if (der != null) der.style.opacity = objetivo;
    }

    void SetOpacidadIconos(VisualElement row, float op)
    {
        var izq = row.Q<VisualElement>(ID_ICON_L);
        var der = row.Q<VisualElement>(ID_ICON_R);

        if (izq == null && der == null)
        {
            var hijos = new List<VisualElement>();
            foreach (var h in row.Children()) hijos.Add(h);
            if (hijos.Count >= 1) hijos[0].style.opacity = op;
            if (hijos.Count >= 2) hijos[hijos.Count - 1].style.opacity = op;
            return;
        }

        if (izq != null) izq.style.opacity = op;
        if (der != null) der.style.opacity = op;
    }

    // ── Control de paneles — solo uno visible a la vez ─────────
    public void MostrarPanel(UIDocument panel)
    {
        if (panel == null) return;
        if (panelActivo != null && panelActivo != panel)
            OcultarPanel(panelActivo);

        panelActivo = panel;
        panel.gameObject.SetActive(true);
        if (panel.rootVisualElement != null)
            panel.rootVisualElement.style.display = DisplayStyle.Flex;
    }

    public void OcultarPanel(UIDocument panel)
    {
        if (panel == null) return;
        if (panel.rootVisualElement != null)
            panel.rootVisualElement.style.display = DisplayStyle.None;
        if (panelActivo == panel) panelActivo = null;
    }

    public void CerrarPanelActivo()
    {
        if (panelActivo != null) OcultarPanel(panelActivo);
    }
}
