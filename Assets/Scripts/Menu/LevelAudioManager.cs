using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class LevelAudioManager : MonoBehaviour
{
    [Header("Musica del nivel")]
    public AudioClip musicaDelNivel;

    [Header("Ambiente (loop perpetuo)")]
    public AudioClip ambienceDelNivel;

    [Header("Volumenes")]
    [Range(0f, 1f)] public float volMusica   = 0.7f;
    [Range(0f, 1f)] public float volAmbience = 0.4f;

    public AudioSource sourceMusica;
    public AudioSource sourceAmbience;

    private bool pausado = false;

    void Awake()
    {
        AudioSource[] sources = GetComponents<AudioSource>();

        if (sourceMusica == null)
            sourceMusica = sources.Length > 0 ? sources[0] : gameObject.AddComponent<AudioSource>();

        if (sourceAmbience == null)
            sourceAmbience = sources.Length > 1 ? sources[1] : gameObject.AddComponent<AudioSource>();

        sourceMusica.loop         = false;
        sourceMusica.playOnAwake  = false;
        sourceMusica.volume       = volMusica;
        sourceMusica.spatialBlend = 0f;
        sourceMusica.priority     = 0;

        sourceAmbience.loop         = true;
        sourceAmbience.playOnAwake  = false;
        sourceAmbience.volume       = volAmbience;
        sourceAmbience.spatialBlend = 0f;
        sourceAmbience.priority     = 64;
    }

    void Start()
    {
        PlayerData pd = FindFirstObjectByType<PlayerData>();
        if (pd != null) pd.OnBateriaVacia += ReiniciarMusica;

        PlayerDeath death = FindFirstObjectByType<PlayerDeath>();
        if (death != null) death.OnPlayerMurio += ReiniciarMusica;

        IniciarMusica();
        IniciarAmbience();
    }

    void OnDestroy()
    {
        PlayerData pd = FindFirstObjectByType<PlayerData>();
        if (pd != null) pd.OnBateriaVacia -= ReiniciarMusica;

        PlayerDeath death = FindFirstObjectByType<PlayerDeath>();
        if (death != null) death.OnPlayerMurio -= ReiniciarMusica;
    }

    void IniciarMusica()
    {
        if (musicaDelNivel == null) { Debug.LogWarning("[LevelAudio] Asigna musicaDelNivel."); return; }
        sourceMusica.clip = musicaDelNivel;
        sourceMusica.time = 0f;
        sourceMusica.Play();
    }

    void IniciarAmbience()
    {
        if (ambienceDelNivel == null) { Debug.LogWarning("[LevelAudio] Asigna ambienceDelNivel."); return; }
        sourceAmbience.clip = ambienceDelNivel;
        sourceAmbience.Play();
    }

    public void ReiniciarMusica()
    {
        if (pausado) return;
        sourceMusica.Stop();
        sourceMusica.time = 0f;
        sourceMusica.Play();
    }

    public void PausarAudio()
    {
        pausado = true;
        if (sourceMusica.isPlaying)   sourceMusica.Pause();
        if (sourceAmbience.isPlaying) sourceAmbience.Pause();
    }

    public void ReanudarAudio()
    {
        pausado = false;
        sourceMusica.UnPause();
        sourceAmbience.UnPause();
    }

    public void DetenerTodo()
    {
        sourceMusica.Stop();
        sourceAmbience.Stop();
    }
}
