using UnityEngine;

// ─────────────────────────────────────────────────────────────────
// PlayerSoundController — Motor de audio del player
// Arquitectura profesional con AudioMixer-ready y pool de sources
// ─────────────────────────────────────────────────────────────────
[RequireComponent(typeof(AudioSource))]
public class PlayerSoundController : MonoBehaviour
{
    // ── Sources separados por categoria (buena practica de audio) ──
    [Header("Audio Sources")]
    [Tooltip("Para sonidos en loop (correr)")]
    public AudioSource sourceLoop;

    [Tooltip("Para one-shots: salto, aterrizaje, dano, muerte")]
    public AudioSource sourceOneShot;

    // ── Clips de movimiento ────────────────────────────────────────
    [Header("Movimiento")]
    public AudioClip RunSound;

    [Tooltip("Pitch minimo aleatorio para variar el sonido de correr")]
    [Range(0.8f, 1f)] public float runPitchMin = 0.95f;
    [Range(1f, 1.2f)] public float runPitchMax = 1.05f;

    public AudioClip JumpSound;
    public AudioClip LandSound;

    // ── Clips de estado ────────────────────────────────────────────
    [Header("Estado del player")]
    public AudioClip DeathSound;
    public AudioClip DamageSound;     // sonido al recibir dano (WhatsApp, Calls, Teams)
    public AudioClip RespawnSound;

    // ── Volúmenes por categoria ───────────────────────────────────
    [Header("Volumenes")]
    [Range(0f, 1f)] public float volCorrer  = 0.5f;
    [Range(0f, 1f)] public float volAccion  = 0.8f;
    [Range(0f, 1f)] public float volMuerte  = 1.0f;

    // ── Estado interno ─────────────────────────────────────────────
    private bool corriendo    = false;
    private bool estaMuerto   = false;
    private bool enSuelo      = false;
    private bool estabaEnSuelo = false;

    // ── Init ───────────────────────────────────────────────────────
    void Awake()
    {
        // Si no se asignaron en el Inspector, auto-crear los dos AudioSources
        AudioSource[] sources = GetComponents<AudioSource>();

        if (sourceLoop == null)
            sourceLoop = sources.Length > 0 ? sources[0] : gameObject.AddComponent<AudioSource>();

        if (sourceOneShot == null)
        {
            if (sources.Length > 1)
                sourceOneShot = sources[1];
            else
                sourceOneShot = gameObject.AddComponent<AudioSource>();
        }

        // Configurar source de loop
        sourceLoop.loop        = true;
        sourceLoop.playOnAwake = false;
        sourceLoop.volume      = volCorrer;
        sourceLoop.spatialBlend = 0f; // 2D puro para juego 2D

        // Configurar source de one-shots
        sourceOneShot.loop        = false;
        sourceOneShot.playOnAwake = false;
        sourceOneShot.volume      = volAccion;
        sourceOneShot.spatialBlend = 0f;
    }

    // ── API publica — llamada desde PlayerMovement y PlayerDeath ──

    /// Inicia o detiene el sonido de correr en loop
    public void SetCorrer(bool activo)
    {
        if (estaMuerto) return;

        if (activo && !corriendo)
        {
            if (RunSound != null)
            {
                sourceLoop.clip   = RunSound;
                sourceLoop.pitch  = Random.Range(runPitchMin, runPitchMax);
                sourceLoop.volume = volCorrer;
                sourceLoop.Play();
            }
            corriendo = true;
        }
        else if (!activo && corriendo)
        {
            sourceLoop.Stop();
            corriendo = false;
        }
    }

    /// Llamado cuando el player salta
    public void PlayJump()
    {
        if (estaMuerto || JumpSound == null) return;
        sourceOneShot.volume = volAccion;
        sourceOneShot.PlayOneShot(JumpSound);
    }

    /// Llamado cuando el player aterriza (transicion aire → suelo)
    public void PlayLand()
    {
        if (estaMuerto || LandSound == null) return;
        sourceOneShot.volume = volAccion;
        sourceOneShot.PlayOneShot(LandSound);
    }

    /// Llamado cuando el player recibe dano (WhatsApp, Calls, Teams)
    public void PlayDamage()
    {
        if (estaMuerto || DamageSound == null) return;
        sourceOneShot.volume = volAccion;
        sourceOneShot.PlayOneShot(DamageSound);
    }

    /// Llamado por PlayerDeath al morir
    public void PlayDeath()
    {
        estaMuerto = true;

        // Detener loop de correr inmediatamente
        sourceLoop.Stop();
        corriendo = false;

        if (DeathSound != null)
        {
            sourceOneShot.volume = volMuerte;
            sourceOneShot.PlayOneShot(DeathSound);
        }
    }

    /// Llamado por PlayerDeath al hacer respawn
    public void PlayRespawn()
    {
        estaMuerto = false;

        if (RespawnSound != null)
        {
            sourceOneShot.volume = volAccion;
            sourceOneShot.PlayOneShot(RespawnSound);
        }
    }

    /// Resetear estado (llamado en respawn)
    public void ResetearAudio()
    {
        estaMuerto = false;
        corriendo  = false;
        sourceLoop.Stop();
        sourceOneShot.Stop();
    }

    // ── Deteccion de aterrizaje (llamado desde PlayerMovement) ────
    public void ActualizarEstadoSuelo(bool esSuelo)
    {
        // Detectar transicion de aire a suelo → aterrizaje
        if (esSuelo && !estabaEnSuelo)
            PlayLand();

        estabaEnSuelo = esSuelo;
        enSuelo       = esSuelo;
    }
}
