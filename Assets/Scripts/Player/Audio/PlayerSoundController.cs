using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerSoundController : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource sourceLoop;
    public AudioSource sourceOneShot;

    [Header("Movimiento")]
    public AudioClip RunSound;
    [Range(0.8f, 1f)]  public float runPitchMin = 0.95f;
    [Range(1f, 1.2f)]  public float runPitchMax = 1.05f;

    public AudioClip JumpSound;
    public AudioClip LandSound;

    [Header("Estado")]
    public AudioClip DeathSound;
    public AudioClip DamageSound;
    public AudioClip RespawnSound;

    [Header("Volumenes")]
    [Range(0f, 1f)] public float volCorrer = 0.5f;
    [Range(0f, 1f)] public float volAccion = 0.8f;
    [Range(0f, 1f)] public float volMuerte = 1.0f;

    private bool corriendo     = false;
    private bool estaMuerto    = false;
    private bool estabaEnSuelo = false;

    // Guardamos si el loop estaba activo antes de pausar
    private bool loopActivoAntesdePausa = false;

    void Awake()
    {
        AudioSource[] sources = GetComponents<AudioSource>();

        if (sourceLoop == null)
            sourceLoop = sources.Length > 0 ? sources[0] : gameObject.AddComponent<AudioSource>();

        if (sourceOneShot == null)
            sourceOneShot = sources.Length > 1 ? sources[1] : gameObject.AddComponent<AudioSource>();

        sourceLoop.loop         = true;
        sourceLoop.playOnAwake  = false;
        sourceLoop.volume       = volCorrer;
        sourceLoop.spatialBlend = 0f;

        sourceOneShot.loop         = false;
        sourceOneShot.playOnAwake  = false;
        sourceOneShot.volume       = volAccion;
        sourceOneShot.spatialBlend = 0f;
    }

    // ── API de movimiento ──────────────────────────────────────

    public void SetCorrer(bool activo)
    {
        if (estaMuerto) return;

        if (activo && !corriendo)
        {
            if (RunSound != null)
            {
                sourceLoop.clip  = RunSound;
                sourceLoop.pitch = Random.Range(runPitchMin, runPitchMax);
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

    public void PlayJump()
    {
        if (estaMuerto || JumpSound == null) return;
        sourceOneShot.volume = volAccion;
        sourceOneShot.PlayOneShot(JumpSound);
    }

    public void PlayLand()
    {
        if (estaMuerto || LandSound == null) return;
        sourceOneShot.volume = volAccion;
        sourceOneShot.PlayOneShot(LandSound);
    }

    public void PlayDamage()
    {
        if (estaMuerto || DamageSound == null) return;
        sourceOneShot.volume = volAccion;
        sourceOneShot.PlayOneShot(DamageSound);
    }

    public void PlayDeath()
    {
        estaMuerto = true;
        sourceLoop.Stop();
        corriendo = false;

        if (DeathSound != null)
        {
            sourceOneShot.volume = volMuerte;
            sourceOneShot.PlayOneShot(DeathSound);
        }
    }

    public void PlayRespawn()
    {
        estaMuerto = false;
        if (RespawnSound != null)
        {
            sourceOneShot.volume = volAccion;
            sourceOneShot.PlayOneShot(RespawnSound);
        }
    }

    public void ResetearAudio()
    {
        estaMuerto = false;
        corriendo  = false;
        sourceLoop.Stop();
        sourceOneShot.Stop();
    }

    public void ActualizarEstadoSuelo(bool esSuelo)
    {
        if (esSuelo && !estabaEnSuelo) PlayLand();
        estabaEnSuelo = esSuelo;
    }

    // ── API de pausa ───────────────────────────────────────────

    /// Llamado por PauseMenuController al pausar el juego
    public void PausarAudio()
    {
        // Guardar si el loop estaba sonando para poder reanudarlo exactamente
        loopActivoAntesdePausa = sourceLoop.isPlaying;

        if (sourceLoop.isPlaying)   sourceLoop.Pause();
        if (sourceOneShot.isPlaying) sourceOneShot.Pause();
    }

    /// Llamado por PauseMenuController al reanudar el juego
    public void ReanudarAudio()
    {
        if (loopActivoAntesdePausa) sourceLoop.UnPause();
        sourceOneShot.UnPause();
    }
}
