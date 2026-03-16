using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

[RequireComponent(typeof(UIDocument))]
public class AudioOptionsController : MonoBehaviour
{
    [Header("Datos de configuracion")]
    public AudioSettingsData audioData;

    // IDs exactos del UXML (confirmados en UI Builder)
    private const string ID_SLIDER_MASTER = "SliderMaster";
    private const string ID_SLIDER_SOUND  = "SliderSound";
    private const string ID_SLIDER_MUSIC  = "SliderMusic";
    private const string ID_BTN_RESET     = "BtnResetDefaults";
    private const string ID_BTN_BACK      = "BtnBack";
    private const string ID_LABEL_MASTER  = "ValueMaster";
    private const string ID_LABEL_SOUND   = "ValueSound";
    private const string ID_LABEL_MUSIC   = "ValueMusic";

    // Los sliders van de 0 a 10 (SliderInt en UXML)
    private const int SLIDER_MAX = 10;

    private UIDocument        doc;
    private SliderInt         sliderMaster;
    private SliderInt         sliderSound;
    private SliderInt         sliderMusic;
    private Label             labelMaster;
    private Label             labelSound;
    private Label             labelMusic;
    private OptionsController optionsCtrl;
    private bool              registrado = false;

    void Awake()
    {
        doc         = GetComponent<UIDocument>();
        optionsCtrl = FindFirstObjectByType<OptionsController>();
    }

    void OnEnable() { StartCoroutine(IniciarDelay()); }

    IEnumerator IniciarDelay() { yield return null; Iniciar(); }

    void Iniciar()
    {
        if (doc?.rootVisualElement == null) return;
        if (audioData == null)
        {
            Debug.LogError("[AudioOptions] audioData NO asignado en el Inspector.");
            return;
        }

        var root = doc.rootVisualElement;

        // Buscar SliderInt (no Slider — el UXML usa SliderInt con rango 0-10)
        sliderMaster = root.Q<SliderInt>(ID_SLIDER_MASTER);
        sliderSound  = root.Q<SliderInt>(ID_SLIDER_SOUND);
        sliderMusic  = root.Q<SliderInt>(ID_SLIDER_MUSIC);

        // Buscar labels de valor
        labelMaster = root.Q<Label>(ID_LABEL_MASTER);
        labelSound  = root.Q<Label>(ID_LABEL_SOUND);
        labelMusic  = root.Q<Label>(ID_LABEL_MUSIC);

        Debug.Log("[AudioOptions] SliderInt encontrados: " +
                  "Master=" + (sliderMaster != null) + " | " +
                  "Sound="  + (sliderSound  != null) + " | " +
                  "Music="  + (sliderMusic  != null));

        // Configurar rango 0 a 10
        ConfigurarSlider(sliderMaster);
        ConfigurarSlider(sliderSound);
        ConfigurarSlider(sliderMusic);

        // Cargar valores guardados
        CargarValores();

        // Registrar callbacks solo una vez
        if (!registrado)
        {
            if (sliderMaster != null)
                sliderMaster.RegisterCallback<ChangeEvent<int>>(e =>
                {
                    float vol = IntAFloat(e.newValue);
                    audioData.VolumenMaster = vol;
                    ActualizarLabel(labelMaster, e.newValue);
                });

            if (sliderSound != null)
                sliderSound.RegisterCallback<ChangeEvent<int>>(e =>
                {
                    float vol = IntAFloat(e.newValue);
                    audioData.VolumenSFX = vol;
                    ActualizarLabel(labelSound, e.newValue);
                });

            if (sliderMusic != null)
                sliderMusic.RegisterCallback<ChangeEvent<int>>(e =>
                {
                    float vol = IntAFloat(e.newValue);
                    audioData.VolumenMusica = vol;
                    ActualizarLabel(labelMusic, e.newValue);
                });

            // Boton Reset
            var btnReset = root.Q<VisualElement>(ID_BTN_RESET);
            if (btnReset != null)
                btnReset.RegisterCallback<ClickEvent>(_ => Resetear());
            else
                Debug.LogWarning("[AudioOptions] BtnResetDefaults no encontrado.");

            // Boton Back
            var btnBack = root.Q<VisualElement>(ID_BTN_BACK);
            if (btnBack != null)
                btnBack.RegisterCallback<ClickEvent>(_ => optionsCtrl?.CerrarPanelActivo());
            else
                Debug.LogWarning("[AudioOptions] BtnBack no encontrado.");

            registrado = true;
        }
    }

    // ── Cargar valores desde PlayerPrefs ──────────────────────
    void CargarValores()
    {
        SetSlider(sliderMaster, FloatAInt(audioData.VolumenMaster));
        SetSlider(sliderSound,  FloatAInt(audioData.VolumenSFX));
        SetSlider(sliderMusic,  FloatAInt(audioData.VolumenMusica));

        ActualizarLabel(labelMaster, FloatAInt(audioData.VolumenMaster));
        ActualizarLabel(labelSound,  FloatAInt(audioData.VolumenSFX));
        ActualizarLabel(labelMusic,  FloatAInt(audioData.VolumenMusica));

        // Aplicar volumenes al audio actual
        audioData.AplicarVolumenes();
    }

    // ── Reset a valores por defecto ───────────────────────────
    void Resetear()
    {
        audioData.Resetear();
        registrado = false;
        Iniciar();
        Debug.Log("[AudioOptions] Reset a valores por defecto.");
    }

    // ── Helpers de conversion ──────────────────────────────────
    // SliderInt va de 0 a 10 → float va de 0.0 a 1.0
    float IntAFloat(int valor)  => Mathf.Clamp01(valor / (float)SLIDER_MAX);
    int   FloatAInt(float valor) => Mathf.RoundToInt(Mathf.Clamp01(valor) * SLIDER_MAX);

    void ConfigurarSlider(SliderInt s)
    {
        if (s == null) return;
        s.lowValue  = 0;
        s.highValue = SLIDER_MAX;
    }

    void SetSlider(SliderInt s, int valor)
    {
        if (s != null) s.SetValueWithoutNotify(Mathf.Clamp(valor, 0, SLIDER_MAX));
    }

    void ActualizarLabel(Label label, int valor)
    {
        if (label != null)
            label.text = Mathf.RoundToInt(IntAFloat(valor) * 100f) + "%";
    }
}
