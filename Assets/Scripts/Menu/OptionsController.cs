using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

// ─────────────────────────────────────────────────────────────
// OptionsController — Controlador de la escena Options
//
// Gestiona:
//  - Mostrar/ocultar paneles (Audio, Video, etc.) sin solapamiento
//  - Solo un panel visible a la vez
//  - Boton Back vuelve a Menu
// ─────────────────────────────────────────────────────────────
public class OptionsController : MonoBehaviour
{
    [Header("UIDocuments")]
    [Tooltip("El UIDocument principal con los botones Audio/Video/Back")]
    public UIDocument docOpciones;

    [Tooltip("El UIDocument del panel de Audio (UIAudioOptions)")]
    public UIDocument docAudio;

    [Header("Escena Menu")]
    public string escenaMenu = "Menu";

    // IDs en OptionsUI.uxml
    private const string ID_BTN_AUDIO = "AudioBtn";
    private const string ID_BTN_VIDEO = "VideoBtn";
    private const string ID_BTN_BACK  = "BackBtn";

    // Panel activo actualmente
    private UIDocument panelActivo = null;

    void OnEnable()
    {
        // Ocultar todos los paneles al inicio
        OcultarPanel(docAudio);

        // Configurar el documento principal
        if (docOpciones == null) docOpciones = GetComponent<UIDocument>();
        if (docOpciones == null) { Debug.LogError("[Options] Falta docOpciones."); return; }

        var root = docOpciones.rootVisualElement;
        if (root == null) return;

        // Buscar botones con multiples nombres posibles
        var btnAudio = BuscarElemento(root, ID_BTN_AUDIO, "audio", "Audio");
        var btnVideo = BuscarElemento(root, ID_BTN_VIDEO, "video", "Video");
        var btnBack  = BuscarElemento(root, ID_BTN_BACK,  "back",  "Back", "volver");

        if (btnAudio != null) btnAudio.RegisterCallback<ClickEvent>(_ => MostrarPanel(docAudio));
        if (btnVideo != null) btnVideo.RegisterCallback<ClickEvent>(_ => Debug.Log("[Options] Video — implementar UIVideoOptions"));
        if (btnBack  != null) btnBack.RegisterCallback<ClickEvent>(_ => SceneManager.LoadScene(escenaMenu));

        // Hover
        foreach (var el in new[] { btnAudio, btnVideo, btnBack })
            if (el != null) el.RegisterCallback<MouseEnterEvent>(_ => el.style.opacity = 0.75f);

        foreach (var el in new[] { btnAudio, btnVideo, btnBack })
            if (el != null) el.RegisterCallback<MouseLeaveEvent>(_ => el.style.opacity = 1f);
    }

    // ── Control de paneles ────────────────────────────────────

    public void MostrarPanel(UIDocument panel)
    {
        if (panel == null) return;

        // Ocultar panel previo si hay uno abierto
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

        if (panelActivo == panel)
            panelActivo = null;
    }

    public void CerrarPanelActivo()
    {
        if (panelActivo != null)
            OcultarPanel(panelActivo);
    }

    // ── Helper ────────────────────────────────────────────────
    VisualElement BuscarElemento(VisualElement root, params string[] ids)
    {
        foreach (var id in ids)
        {
            var el = root.Q<VisualElement>(id);
            if (el != null) return el;
        }
        // Fallback: buscar por texto en Labels/Buttons
        foreach (var id in ids)
        {
            var el = root.Query<VisualElement>().Where(e => {
                string n = (e.name ?? "").ToLower();
                return n.Contains(id.ToLower());
            }).First();
            if (el != null) return el;
        }
        return null;
    }
}
