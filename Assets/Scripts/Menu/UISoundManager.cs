using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class UISoundManager : MonoBehaviour
{
    public static UISoundManager Instance { get; private set; }

    [Header("Clips de sonido UI")]
    public AudioClip sfxConfirm;
    public AudioClip sfxHover;
    public AudioClip sfxBack;
    public AudioClip sfxSwipe;

    [Range(0f, 1f)] public float volumen = 0.8f;

    [Header("IDs de botones Back/Quit (usan sfxBack)")]
    public string[] idsBack = {
        "BackBtn","BackButton","BtnBack","Back",
        "BtnQuitToMenu","QuitGameRow","QuitGame"
    };

    [Header("IDs de Sliders (usan sfxSwipe)")]
    public string[] idsSlider = {
        "VolumeSlider","MusicSlider","SFXSlider","BrightnessSlider"
    };

    private static readonly HashSet<string> ESCENAS_MENU =
        new HashSet<string>{ "Menu","GlobalLevels","Options","Extras" };

    private AudioSource source;
    private float       lastHover  = -1f;
    private const float HOVER_CD   = 0.08f;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        source             = GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop        = false;
        source.spatialBlend= 0f;
        source.priority    = 32;
    }

    void Start()
    {
        SceneManager.sceneLoaded += OnScene;
        Conectar();
    }

    void OnDestroy() { SceneManager.sceneLoaded -= OnScene; }

    void OnScene(Scene s, LoadSceneMode m)
    {
        if (ESCENAS_MENU.Contains(s.name)) StartCoroutine(ConectarDelay());
    }

    IEnumerator ConectarDelay() { yield return null; Conectar(); }

    void Conectar()
    {
        if (!ESCENAS_MENU.Contains(SceneManager.GetActiveScene().name)) return;
        foreach (var doc in FindObjectsByType<UIDocument>(FindObjectsSortMode.None))
        {
            if (doc?.rootVisualElement != null)
                Registrar(doc.rootVisualElement);
        }
    }

    void Registrar(VisualElement root)
    {
        foreach (var el in root.Query<VisualElement>().ToList())
        {
            if (el == null) continue;
            if (EsSlider(el))       RegistrarSlider(el);
            else if (EsBack(el.name)) RegistrarBack(el);
            else if (EsUI(el))      RegistrarBoton(el);
        }
    }

    void RegistrarBoton(VisualElement el)
    {
        el.RegisterCallback<MouseEnterEvent>(_ => PlayHover(),   TrickleDown.TrickleDown);
        el.RegisterCallback<ClickEvent>     (_ => PlayConfirm(), TrickleDown.TrickleDown);
    }

    void RegistrarBack(VisualElement el)
    {
        el.RegisterCallback<MouseEnterEvent>(_ => PlayHover(), TrickleDown.TrickleDown);
        el.RegisterCallback<ClickEvent>     (_ => PlayBack(),  TrickleDown.TrickleDown);
    }

    void RegistrarSlider(VisualElement el)
    {
        el.RegisterCallback<MouseEnterEvent>    (_ => PlayHover(),  TrickleDown.TrickleDown);
        el.RegisterCallback<ChangeEvent<float>> (_ => PlaySwipe(),  TrickleDown.TrickleDown);
        el.RegisterCallback<ChangeEvent<int>>   (_ => PlaySwipe(),  TrickleDown.TrickleDown);
    }

    bool EsBack(string n)
    {
        if (string.IsNullOrEmpty(n)) return false;
        string l = n.ToLower();
        foreach (var id in idsBack) if (l == id.ToLower()) return true;
        return l.Contains("back") || l.Contains("quit") || l.Contains("exit");
    }

    bool EsSlider(VisualElement el)
    {
        if (el is Slider || el is SliderInt) return true;
        string n = (el.name ?? "").ToLower();
        foreach (var id in idsSlider) if (n.Contains(id.ToLower())) return true;
        return n.Contains("slider");
    }

    bool EsUI(VisualElement el)
    {
        if (el is Button || el is Toggle || el is DropdownField) return true;
        string n = (el.name ?? "").ToLower();
        return n.Contains("btn") || n.Contains("button")
            || n.Contains("row") || n.Contains("slot");
    }

    public void PlayHover()
    {
        if (Time.unscaledTime - lastHover < HOVER_CD) return;
        lastHover = Time.unscaledTime;
        Play(sfxHover);
    }

    public void PlayConfirm() => Play(sfxConfirm);
    public void PlayBack()    => Play(sfxBack);
    public void PlaySwipe()   => Play(sfxSwipe);

    void Play(AudioClip clip)
    {
        if (clip == null || source == null) return;
        source.volume = volumen;
        source.PlayOneShot(clip);
    }
}
