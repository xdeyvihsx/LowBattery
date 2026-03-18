using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class MenuMusicManager : MonoBehaviour
{
    public static MenuMusicManager Instance { get; private set; }

    [Header("Canciones del Menu (Audio/Music/Menu)")]
    public AudioClip cancion1;
    public AudioClip cancion2;
    public AudioClip cancion3;

    [Header("Volumen base (se multiplica por PlayerPrefs al arrancar)")]
    [Range(0f, 1f)] public float volumen = 0.6f;

    private const string KEY_MASTER = "vol_master";
    private const string KEY_MUSIC  = "vol_music";

    private AudioSource source;
    private AudioClip[] canciones;
    private List<int>   cola          = new List<int>();
    private int         indexCola     = 0;
    private int         ultimaCancion = -1;
    private bool        pausado       = false;
    private bool        silenciado    = false;
    private Coroutine   corActiva;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        source              = GetComponent<AudioSource>();
        source.loop         = false;
        source.playOnAwake  = false;
        source.spatialBlend = 0f;
        source.priority     = 0;
    }

    void Start()
    {
        canciones = new AudioClip[] { cancion1, cancion2, cancion3 };

        // Leer volumen guardado en PlayerPrefs al arrancar
        float master = PlayerPrefs.GetFloat(KEY_MASTER, 1.0f);
        float music  = PlayerPrefs.GetFloat(KEY_MUSIC,  volumen);
        volumen       = music;
        source.volume = music * master;

        SceneManager.sceneLoaded += AlCargarEscena;
        GenerarCola();

        if (!EsNivel(SceneManager.GetActiveScene().name))
            Siguiente();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= AlCargarEscena;
    }

    void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        if (EsNivel(escena.name))
        {
            if (!silenciado)
            {
                silenciado = true;
                if (corActiva != null) StopCoroutine(corActiva);
                source.Stop();
            }
        }
        else
        {
            if (silenciado)
            {
                silenciado = false;
                pausado    = false;

                // Reaplicar volumen guardado al volver al menu
                float master = PlayerPrefs.GetFloat(KEY_MASTER, 1.0f);
                float music  = PlayerPrefs.GetFloat(KEY_MUSIC,  volumen);
                source.volume = music * master;

                GenerarCola();
                Siguiente();
            }
        }
    }

    bool EsNivel(string nombre) => nombre.StartsWith("Level_");

    void GenerarCola()
    {
        cola.Clear();
        for (int i = 0; i < canciones.Length; i++)
            if (canciones[i] != null) cola.Add(i);

        for (int i = cola.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int tmp = cola[i]; cola[i] = cola[j]; cola[j] = tmp;
        }

        if (cola.Count > 1 && cola[0] == ultimaCancion)
        {
            int tmp = cola[0]; cola[0] = cola[1]; cola[1] = tmp;
        }
        indexCola = 0;
    }

    void Siguiente()
    {
        if (silenciado || pausado || cola.Count == 0) return;
        if (indexCola >= cola.Count) GenerarCola();

        int idx = cola[indexCola++];
        AudioClip clip = canciones[idx];
        if (clip == null) { Siguiente(); return; }

        ultimaCancion = idx;
        source.clip   = clip;
        source.Play();

        if (corActiva != null) StopCoroutine(corActiva);
        corActiva = StartCoroutine(Esperar(clip.length));
    }

    IEnumerator Esperar(float dur)
    {
        yield return new WaitForSeconds(dur);
        if (!silenciado && !pausado) Siguiente();
    }

    // ── API publica ─────────────────────────────────────────────
    public void AplicarVolumen()
    {
        if (source != null) source.volume = volumen;
    }

    public void PausarMusica()   { if (!silenciado) { pausado = true;  source.Pause();   } }
    public void ReanudarMusica() { if (!silenciado) { pausado = false; source.UnPause(); } }
}
