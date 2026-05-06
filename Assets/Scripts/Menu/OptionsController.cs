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
    public UIDocument docTutorial;

    [Header("Escenas")]
    public string escenaMenu = "Menu";

    [Header("Velocidad fade iconos en segundos")]
    public float velocidadFade = 0.18f;

    private const string ID_ROW_AUDIO = "AudioRow";
    private const string ID_ROW_VIDEO = "VideoRow";
    private const string ID_ROW_TUTORIAL = "TutorialRow";
    private const string ID_BACK      = "BackBtn";
    private const string ID_ICON_L    = "StartIconLeft";
    private const string ID_ICON_R    = "StartIconRight";

    private List<VisualElement>                  rows  = new List<VisualElement>();
    private Dictionary<VisualElement, Coroutine> fades = new Dictionary<VisualElement, Coroutine>();
    private UIDocument panelActivo = null;

    void OnEnable()
    {
        OcultarPanel(docAudio);
        OcultarPanel(docVideo);
        OcultarPanel(docTutorial);

        if (docOpciones == null) docOpciones = GetComponent<UIDocument>();
        if (docOpciones == null) { Debug.LogError("[Options] Falta docOpciones."); return; }

        var root = docOpciones.rootVisualElement;
        if (root == null) return;

        rows.Clear();
        fades.Clear();

        BindRow(root, ID_ROW_AUDIO, () => AbrirPanel(docAudio));
        BindRow(root, ID_ROW_VIDEO, () => AbrirPanel(docVideo));
        BindRow(root, ID_ROW_TUTORIAL, () => AbrirPanel(docTutorial));

        var btnBack = root.Q<VisualElement>(ID_BACK);
        if (btnBack != null)
        {
            btnBack.RegisterCallback<MouseEnterEvent>(_ => btnBack.style.opacity = 0.6f);
            btnBack.RegisterCallback<MouseLeaveEvent>(_ => btnBack.style.opacity = 1f);
            btnBack.RegisterCallback<ClickEvent>(_ => SceneManager.LoadScene(escenaMenu));
        }

        foreach (var row in rows) SetOpacidadIconos(row, 0f);
    }

    void BindRow(VisualElement root, string id, System.Action accion)
    {
        var row = root.Q<VisualElement>(id);
        if (row == null) { Debug.LogWarning("[Options] No encontre: " + id); return; }
        rows.Add(row);
        row.RegisterCallback<MouseEnterEvent>(_ => { foreach (var r in rows) if (r != row) IniciarFade(r, 0f); IniciarFade(row, 1f); });
        row.RegisterCallback<MouseLeaveEvent>(_ => IniciarFade(row, 0f));
        row.RegisterCallback<ClickEvent>(_ => accion?.Invoke());
    }

    void AbrirPanel(UIDocument panel)
    {
        if (panel == null) return;
        if (panelActivo != null && panelActivo != panel) OcultarPanel(panelActivo);
        panelActivo = panel;

        // Asignar onBack al panel hijo para que vuelva a OptionsUI
        if (panel == docAudio)
        {
            var audioCtrl = panel.GetComponent<AudioOptionsController>();
            if (audioCtrl != null)
                audioCtrl.onBack = CerrarPanelActivo;
        }
        else if (panel == docVideo)
        {
            var videoCtrl = panel.GetComponent<VideoOptionsController>();
            if (videoCtrl != null)
                videoCtrl.onBack = CerrarPanelActivo;
        }
        else if (panel == docTutorial)
        {
            var tutorialCtrl = panel.GetComponent<TutorialOptionsController>();
            if (tutorialCtrl != null)
                tutorialCtrl.onBack = CerrarPanelActivo;
        }

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

    void IniciarFade(VisualElement row, float objetivo)
    {
        if (fades.TryGetValue(row, out var c) && c != null) StopCoroutine(c);
        fades[row] = StartCoroutine(DoFade(row, objetivo));
    }

    IEnumerator DoFade(VisualElement row, float objetivo)
    {
        var izq = row.Q<VisualElement>(ID_ICON_L);
        var der = row.Q<VisualElement>(ID_ICON_R);
        if (izq == null && der == null)
        {
            var h = new List<VisualElement>();
            foreach (var ch in row.Children()) h.Add(ch);
            if (h.Count >= 1) izq = h[0];
            if (h.Count >= 2) der = h[h.Count - 1];
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
        if (izq != null) izq.style.opacity = op;
        if (der != null) der.style.opacity = op;
    }
}
