using UnityEngine;
using System.Collections;

public class PowerUpAudioManager : MonoBehaviour
{
    public static PowerUpAudioManager Instance { get; private set; }

    [Header("SFX Power-Up")]
    [Tooltip("Sonido al recoger el power-up: Short_power-up_sound")]
    public AudioClip sfxRecogida;

    [Tooltip("Sonido de subida de bateria: Short_player_power-up_sound")]
    public AudioClip sfxSubidaBateria;

    [Header("Volumenes")]
    [Range(0f, 1f)] public float volRecogida      = 0.9f;
    [Range(0f, 1f)] public float volSubidaBateria = 0.85f;

    private AudioSource src;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
        src = gameObject.AddComponent<AudioSource>();
        src.spatialBlend = 0f;
        src.playOnAwake  = false;
        src.loop         = false;
        src.priority     = 16;
    }

    // Llamado por PowerUpBase.AlRecoger()
    public void PlaySecuencia()
    {
        if (src == null || !gameObject.activeInHierarchy) return;
        StartCoroutine(CorSecuencia());
    }

    IEnumerator CorSecuencia()
    {
        float vol = SFXVol();

        // 1. Sonido de recogida inmediato
        if (sfxRecogida != null)
        {
            src.PlayOneShot(sfxRecogida, volRecogida * vol);
            // Esperar la duracion exacta del clip
            yield return new WaitForSeconds(sfxRecogida.length);
        }
        else
            Debug.LogWarning("[PowerUpAudio] sfxRecogida no asignado.");

        // 2. Sonido de subida de bateria (encadenado)
        if (sfxSubidaBateria != null)
            src.PlayOneShot(sfxSubidaBateria, volSubidaBateria * vol);
        else
            Debug.LogWarning("[PowerUpAudio] sfxSubidaBateria no asignado.");
    }

    float SFXVol() =>
        PlayerPrefs.GetFloat("vol_master", 1f) *
        PlayerPrefs.GetFloat("vol_sfx",    0.8f);
}
