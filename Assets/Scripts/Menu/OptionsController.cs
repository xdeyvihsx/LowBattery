using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class OptionsController : MonoBehaviour
{
    [Header("UIDocuments de paneles")]
    public UIDocument docOpciones;
    public UIDocument docAudio;
    public UIDocument docVideo;

    [Header("Escena Menu")]
    public string escenaMenu = "Menu";

    // IDs en OptionsUI.uxml
    private const string ID_BTN_AUDIO = "AudioBtn";
    private const string ID_BTN_VIDEO = "VideoBtn";
    private const string ID_BTN_BACK  = "BackBtn";

    private UIDocument panelActivo = null;

    void OnEnable()
    {
        // Ocultar todos los paneles al inicio
        OcultarPanel(docAudio);
        OcultarPanel(docVideo);

        if (docOpciones == null) docOpciones = GetComponent<UIDocument>();
        if (docOpciones == null) { Debug.LogError("[Options] Falta docOpciones."); return; }

        var root = docOpciones.rootVisualElement;
        if (root == null) return;

        var btnAudio = BuscarElemento(root, ID_BTN_AUDIO, "audio", "Audio");
        var btnVideo = BuscarElemento(root, ID_BTN_VIDEO, "video", "Video");
        var btnBack  = BuscarElemento(root, ID_BTN_BACK,  "back",  "Back", "volver");

        if (btnAudio != null) btnAudio.RegisterCallback<ClickEvent>(_ => MostrarPanel(docAudio));
        if (btnVideo != null) btnVideo.RegisterCallback<ClickEvent>(_ => MostrarPanel(docVideo));
        if (btnBack  != null) btnBack.RegisterCallback<ClickEvent> (_ => SceneManager.LoadScene(escenaMenu));

        // Hover en botones del menu principal
        foreach (var el in new[] { btnAudio, btnVideo, btnBack })
        {
            if (el == null) continue;
            el.RegisterCallback<MouseEnterEvent>(_ => el.style.opacity = 0.75f);
            el.RegisterCallback<MouseLeaveEvent>(_ => el.style.opacity = 1f);
        }
    }

    // ── Control de paneles — solo uno visible a la vez ─────────
    public void MostrarPanel(UIDocument panel)
    {
        if (panel == null) return;

        // Ocultar el panel previo si era diferente
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

    VisualElement BuscarElemento(VisualElement root, params string[] ids)
    {
        foreach (var id in ids)
        {
            var el = root.Q<VisualElement>(id);
            if (el != null) return el;
        }
        foreach (var id in ids)
        {
            var el = root.Query<VisualElement>().Where(e =>
                (e.name ?? "").ToLower().Contains(id.ToLower())).First();
            if (el != null) return el;
        }
        return null;
    }
}
