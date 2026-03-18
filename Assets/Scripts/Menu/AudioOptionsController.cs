using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

[RequireComponent(typeof(UIDocument))]
public class AudioOptionsController : MonoBehaviour
{
    [Header("Datos de configuracion")]
    public AudioSettingsData audioData;

    [Header("IDs exactos de tus sliders en el UXML")]
    public string idSliderMaster = "SliderMaster";
    public string idSliderMusic  = "SliderMusic";
    public string idSliderSFX    = "SliderSound";
    public string idBtnReset     = "BtnResetDefaults";
    public string idBtnBack      = "BtnBack";
    public string idLabelMaster  = "MasterValue";
    public string idLabelMusic   = "MusicValue";
    public string idLabelSFX     = "SFXValue";

    private const int SLIDER_MAX = 10;

    private UIDocument doc;
    private SliderInt  sliderMaster;
    private SliderInt  sliderMusic;
    private SliderInt  sliderSFX;
    private Label      labelMaster;
    private Label      labelMusic;
    private Label      labelSFX;
    private bool       registrado = false;

    // Referencia directa al controlador padre — se asigna desde fuera
    // En escena Options → asignado por OptionsController
    // En Level_1 Pause  → asignado por InGameOptionsController
    [HideInInspector] public System.Action onBack;

    void Awake()
    {
        doc = GetComponent<UIDocument>();
    }

    void OnEnable() { StartCoroutine(IniciarDelay()); }

    IEnumerator IniciarDelay() { yield return null; Iniciar(); }

    void Iniciar()
    {
        if (doc?.rootVisualElement == null) return;
        if (audioData == null) { Debug.LogError("[AudioOptions] audioData NO asignado."); return; }

        var root = doc.rootVisualElement;

        sliderMaster = BuscarSlider(root, idSliderMaster);
        sliderMusic  = BuscarSlider(root, idSliderMusic);
        sliderSFX    = BuscarSlider(root, idSliderSFX);
        labelMaster  = root.Q<Label>(idLabelMaster);
        labelMusic   = root.Q<Label>(idLabelMusic);
        labelSFX     = root.Q<Label>(idLabelSFX);

        var todos = root.Query<SliderInt>().ToList();
        if (sliderMaster == null && todos.Count >= 1) sliderMaster = todos[0];
        if (sliderMusic  == null && todos.Count >= 2) sliderMusic  = todos[1];
        if (sliderSFX    == null && todos.Count >= 3) sliderSFX    = todos[2];

        ConfigSlider(sliderMaster);
        ConfigSlider(sliderMusic);
        ConfigSlider(sliderSFX);
        CargarValores();

        if (!registrado)
        {
            if (sliderMaster != null)
                sliderMaster.RegisterCallback<ChangeEvent<int>>(e =>
                { audioData.VolumenMaster = IntAFloat(e.newValue); ActLabel(labelMaster, e.newValue); });

            if (sliderMusic != null)
                sliderMusic.RegisterCallback<ChangeEvent<int>>(e =>
                { audioData.VolumenMusica = IntAFloat(e.newValue); ActLabel(labelMusic, e.newValue); });

            if (sliderSFX != null)
                sliderSFX.RegisterCallback<ChangeEvent<int>>(e =>
                { audioData.VolumenSFX = IntAFloat(e.newValue); ActLabel(labelSFX, e.newValue); });

            var btnReset = BuscarEl(root, idBtnReset, "reset", "Reset");
            if (btnReset != null) btnReset.RegisterCallback<ClickEvent>(_ => Resetear());

            // Back: usa el callback onBack asignado por el padre
            var btnBack = BuscarEl(root, idBtnBack, "back", "Back", "close");
            if (btnBack != null)
                btnBack.RegisterCallback<ClickEvent>(_ =>
                {
                    if (onBack != null)
                        onBack.Invoke();
                    else
                        Debug.LogWarning("[AudioOptions] onBack no asignado.");
                });

            registrado = true;
        }
    }

    void CargarValores()
    {
        SetSlider(sliderMaster, FloatAInt(audioData.VolumenMaster));
        SetSlider(sliderMusic,  FloatAInt(audioData.VolumenMusica));
        SetSlider(sliderSFX,    FloatAInt(audioData.VolumenSFX));
        ActLabel(labelMaster, FloatAInt(audioData.VolumenMaster));
        ActLabel(labelMusic,  FloatAInt(audioData.VolumenMusica));
        ActLabel(labelSFX,    FloatAInt(audioData.VolumenSFX));
        audioData.AplicarVolumenes();
    }

    void Resetear() { audioData.Resetear(); registrado = false; Iniciar(); }

    float IntAFloat(int v)   => Mathf.Clamp01(v / (float)SLIDER_MAX);
    int   FloatAInt(float v) => Mathf.RoundToInt(Mathf.Clamp01(v) * SLIDER_MAX);

    void ConfigSlider(SliderInt s) { if (s != null) { s.lowValue = 0; s.highValue = SLIDER_MAX; } }
    void SetSlider(SliderInt s, int v) { if (s != null) s.SetValueWithoutNotify(Mathf.Clamp(v, 0, SLIDER_MAX)); }
    void ActLabel(Label l, int v) { if (l != null) l.text = Mathf.RoundToInt(IntAFloat(v) * 100f) + "%"; }

    SliderInt BuscarSlider(VisualElement root, string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        var s = root.Q<SliderInt>(id);
        if (s != null) return s;
        return root.Query<SliderInt>().Where(e => (e.name ?? "").ToLower().Contains(id.ToLower())).First();
    }

    VisualElement BuscarEl(VisualElement root, params string[] ids)
    {
        foreach (var id in ids) { var e = root.Q<VisualElement>(id); if (e != null) return e; }
        foreach (var id in ids) { var e = root.Query<VisualElement>().Where(x => (x.name ?? "").ToLower().Contains(id.ToLower())).First(); if (e != null) return e; }
        return null;
    }
}
