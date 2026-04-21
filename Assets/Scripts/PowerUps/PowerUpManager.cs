using UnityEngine;
using System.Collections;

public class PowerUpManager : MonoBehaviour
{
    public static PowerUpManager Instance { get; private set; }

    public static bool  EscudoAvionActivo    { get; private set; } = false;
    public static float TiempoEscudoRestante { get; private set; } = 0f;

    [Header("SFX Power-Up")]
    [Tooltip("Short_power-up_sound — suena al recoger")]
    public AudioClip sfxRecogida;

    [Tooltip("Short_player_power-up_sound — suena al subir bateria")]
    public AudioClip sfxSubidaBateria;

    [Header("Volumenes")]
    [Range(0f,1f)] public float volRecogida      = 0.9f;
    [Range(0f,1f)] public float volSubidaBateria = 0.85f;

    private SpriteRenderer playerSR;
    private Color          colorOriginalPlayer;
    private Coroutine      corEscudo;
    private Coroutine      corAudio;
    private AudioSource    srcRecogida;
    private AudioSource    srcSubida;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance    = this;
        srcRecogida = Crear("Src_PU_Recogida", 48);
        srcSubida   = Crear("Src_PU_Subida",   48);
    }

    void Start()
    {
        var mov = FindFirstObjectByType<PlayerMovement>();
        if (mov != null)
        {
            playerSR = mov.GetComponentInChildren<SpriteRenderer>(true);
            if (playerSR != null) colorOriginalPlayer = playerSR.color;
        }
    }

    // Llamado por PowerUpPowerBank y PowerUpModoAvion al recoger
    public void PlayPowerUpAudio()
    {
        if (corAudio != null) StopCoroutine(corAudio);
        corAudio = StartCoroutine(CorCadena());
    }

    IEnumerator CorCadena()
    {
        float vol = SFXVol();

        // Paso 1: sonido de recogida del power-up
        if (sfxRecogida != null && srcRecogida != null)
        {
            srcRecogida.PlayOneShot(sfxRecogida, volRecogida * vol);
            // Esperar la duracion exacta del clip con tiempo real (ignora timeScale)
            yield return new WaitForSecondsRealtime(sfxRecogida.length);
        }
        else
            Debug.LogWarning("[PowerUpMgr] sfxRecogida no asignado.");

        // Paso 2: sonido de subida de bateria
        if (sfxSubidaBateria != null && srcSubida != null)
            srcSubida.PlayOneShot(sfxSubidaBateria, volSubidaBateria * vol);
        else
            Debug.LogWarning("[PowerUpMgr] sfxSubidaBateria no asignado.");

        corAudio = null;
    }

    public void ActivarEscudoAvion(float duracion, Color colorTinte)
    {
        if (corEscudo != null) StopCoroutine(corEscudo);
        corEscudo = StartCoroutine(CorEscudo(duracion, colorTinte));
    }

    IEnumerator CorEscudo(float duracion, Color colorTinte)
    {
        EscudoAvionActivo    = true;
        TiempoEscudoRestante = duracion;
        if (playerSR != null) playerSR.color = colorTinte;

        float t = 0f;
        while (t < duracion)
        {
            t                    += Time.deltaTime;
            TiempoEscudoRestante  = duracion - t;
            if (TiempoEscudoRestante <= 0.8f && playerSR != null)
            {
                float p = Mathf.PingPong(Time.time * 8f, 1f);
                Color c = colorTinte; c.a = Mathf.Lerp(0.3f, 1f, p);
                playerSR.color = c;
            }
            yield return null;
        }

        EscudoAvionActivo    = false;
        TiempoEscudoRestante = 0f;
        if (playerSR != null) playerSR.color = colorOriginalPlayer;
        Debug.Log("[PowerUpMgr] Escudo desactivado.");
    }

    public void LimpiarEfectos()
    {
        if (corEscudo != null) StopCoroutine(corEscudo);
        if (corAudio  != null) StopCoroutine(corAudio);
        EscudoAvionActivo    = false;
        TiempoEscudoRestante = 0f;
        corAudio             = null;
        if (playerSR    != null) playerSR.color = colorOriginalPlayer;
        if (srcRecogida != null) srcRecogida.Stop();
        if (srcSubida   != null) srcSubida.Stop();
    }

    AudioSource Crear(string nombre, int priority)
    {
        var go = new GameObject(nombre);
        go.transform.SetParent(transform);
        var s = go.AddComponent<AudioSource>();
        s.spatialBlend = 0f; s.priority = priority;
        s.playOnAwake = false; s.loop = false;
        return s;
    }

    float SFXVol() =>
        PlayerPrefs.GetFloat("vol_master", 1f) *
        PlayerPrefs.GetFloat("vol_sfx", 0.8f);
}
