using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(UIDocument))]
public class InGameOptionsController : MonoBehaviour
{
    [Header("Paneles hijos")]
    public UIDocument docAudio;
    public UIDocument docVideo;

    private const string ID_ROW_GAME  = "GameRow";
    private const string ID_ROW_AUDIO = "AudioRow";
    private const string ID_ROW_VIDEO = "VideoRow";
    private const string ID_BACK      = "BackBtn";
    private const string ID_ICON_L    = "StartIconLeft";
    private const string ID_ICON_R    = "StartIconRight";
    private const float  FADE         = 0.18f;

    private UIDocument          doc;
    private PauseMenuController pauseCtrl;
    private UIDocument          panelActivo = null;

    private List<VisualElement>                  rows  = new List<VisualElement>();
    private Dictionary<VisualElement, Coroutine> fades = new Dictionary<VisualElement, Coroutine>();
    private bool registrado = false;

    void Awake() { doc = GetComponent<UIDocument>(); }

    void Start()
    {
        Ocultar(docAudio);
        Ocultar(docVideo);
        Registrar();
    }

    public void AlAbrir(PauseMenuController pause)
    {
        pauseCtrl   = pause;
        panelActivo = null;
        Ocultar(docAudio);
        Ocultar(docVideo);
        ResetIconos();
    }

    public void CerrarTodo()
    {
        Ocultar(docAudio);
        Ocultar(docVideo);
        panelActivo = null;
    }

    // Llamado por AudioOptionsController.onBack
    public void CerrarPanelAudio()
    {
        Ocultar(docAudio);
        panelActivo = null;
        // Volver a mostrar OptionsInGame
        if (doc?.rootVisualElement != null)
            doc.rootVisualElement.style.display = DisplayStyle.Flex;
    }

    // Llamado por VideoOptionsController.onBack
    public void CerrarPanelVideo()
    {
        Ocultar(docVideo);
        panelActivo = null;
        // Volver a mostrar OptionsInGame
        if (doc?.rootVisualElement != null)
            doc.rootVisualElement.style.display = DisplayStyle.Flex;
    }

    void Registrar()
    {
        if (doc?.rootVisualElement == null) return;
        if (registrado) return;

        var root = doc.rootVisualElement;
        rows.Clear();
        fades.Clear();

        BindRow(root, ID_ROW_GAME,  () => Debug.Log("[InGameOpts] Game"));
        BindRow(root, ID_ROW_AUDIO, () => AbrirPanel(docAudio));
        BindRow(root, ID_ROW_VIDEO, () => AbrirPanel(docVideo));

        var btnBack = root.Q<VisualElement>(ID_BACK);
        if (btnBack != null)
        {
            btnBack.RegisterCallback<MouseEnterEvent>(_ => btnBack.style.opacity = 0.6f);
            btnBack.RegisterCallback<MouseLeaveEvent>(_ => btnBack.style.opacity = 1f);
            btnBack.RegisterCallback<ClickEvent>(_ =>
            {
                CerrarTodo();
                if (doc?.rootVisualElement != null)
                    doc.rootVisualElement.style.display = DisplayStyle.None;
                pauseCtrl?.CerrarOptions();
            });
        }

        registrado = true;
    }

    void AbrirPanel(UIDocument panel)
    {
        if (panel == null) return;

        // Ocultar OptionsInGame, mostrar el panel hijo
        if (doc?.rootVisualElement != null)
            doc.rootVisualElement.style.display = DisplayStyle.None;

        if (panelActivo != null && panelActivo != panel) Ocultar(panelActivo);
        panelActivo = panel;

        // Asignar el callback onBack al panel hijo ANTES de mostrarlo
        if (panel == docAudio)
        {
            var audioCtrl = panel.GetComponent<AudioOptionsController>();
            if (audioCtrl != null)
                audioCtrl.onBack = CerrarPanelAudio;
        }
        else if (panel == docVideo)
        {
            var videoCtrl = panel.GetComponent<VideoOptionsController>();
            if (videoCtrl != null)
                videoCtrl.onBack = CerrarPanelVideo;
        }

        if (panel.rootVisualElement != null)
            panel.rootVisualElement.style.display = DisplayStyle.Flex;
    }

    void ResetIconos() { foreach (var r in rows) SetOp(r, 0f); }

    void BindRow(VisualElement root, string id, System.Action accion)
    {
        var row = root.Q<VisualElement>(id);
        if (row == null) { Debug.LogWarning("[InGameOpts] No encontre: " + id); return; }
        rows.Add(row);
        row.RegisterCallback<MouseEnterEvent>(_ => { foreach (var r in rows) if (r != row) Fade(r, 0f); Fade(row, 1f); });
        row.RegisterCallback<MouseLeaveEvent>(_ => Fade(row, 0f));
        row.RegisterCallback<ClickEvent>(_ => accion?.Invoke());
    }

    void Fade(VisualElement row, float to)
    {
        if (fades.TryGetValue(row, out var c) && c != null) StopCoroutine(c);
        fades[row] = StartCoroutine(DoFade(row, to));
    }

    IEnumerator DoFade(VisualElement row, float to)
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
        float from = izq != null ? izq.resolvedStyle.opacity : 0f;
        float t = 0f;
        while (t < FADE)
        {
            t += Time.unscaledDeltaTime;
            float op = Mathf.Lerp(from, to, Mathf.Clamp01(t / FADE));
            if (izq != null) izq.style.opacity = op;
            if (der != null) der.style.opacity = op;
            yield return null;
        }
        if (izq != null) izq.style.opacity = to;
        if (der != null) der.style.opacity = to;
    }

    void SetOp(VisualElement row, float op)
    {
        var izq = row.Q<VisualElement>(ID_ICON_L);
        var der = row.Q<VisualElement>(ID_ICON_R);
        if (izq != null) izq.style.opacity = op;
        if (der != null) der.style.opacity = op;
    }

    void Ocultar(UIDocument panel)
    {
        if (panel?.rootVisualElement != null)
            panel.rootVisualElement.style.display = DisplayStyle.None;
        if (panelActivo == panel) panelActivo = null;
    }
}
