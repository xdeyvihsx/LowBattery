using UnityEngine;

[CreateAssetMenu(fileName = "AudioSettingsData", menuName = "LowBattery/AudioSettingsData")]
public class AudioSettingsData : ScriptableObject
{
    private const string KEY_MASTER = "vol_master";
    private const string KEY_MUSIC  = "vol_music";
    private const string KEY_SFX    = "vol_sfx";

    public const float DEFAULT_MASTER = 1.0f;
    public const float DEFAULT_MUSIC  = 0.7f;
    public const float DEFAULT_SFX    = 0.8f;

    public float VolumenMaster
    {
        get => PlayerPrefs.GetFloat(KEY_MASTER, DEFAULT_MASTER);
        set { PlayerPrefs.SetFloat(KEY_MASTER, Mathf.Clamp01(value)); AplicarVolumenes(); }
    }

    public float VolumenMusica
    {
        get => PlayerPrefs.GetFloat(KEY_MUSIC, DEFAULT_MUSIC);
        set { PlayerPrefs.SetFloat(KEY_MUSIC,  Mathf.Clamp01(value)); AplicarVolumenes(); }
    }

    public float VolumenSFX
    {
        get => PlayerPrefs.GetFloat(KEY_SFX, DEFAULT_SFX);
        set { PlayerPrefs.SetFloat(KEY_SFX, Mathf.Clamp01(value)); AplicarVolumenes(); }
    }

    // ── Aplica en tiempo real a TODOS los sistemas ─────────────
    public void AplicarVolumenes()
    {
        float master = VolumenMaster;
        float music  = VolumenMusica;
        float sfx    = VolumenSFX;

        // 1. AudioListener global (afecta TODO el audio del juego)
        AudioListener.volume = master;

        // 2. Musica del menu — actualiza volumen Y source directamente
        if (MenuMusicManager.Instance != null)
        {
            MenuMusicManager.Instance.volumen = music * master;
            MenuMusicManager.Instance.AplicarVolumen();
        }

        // 3. Musica del nivel
        LevelAudioManager levelAudio = FindAnyObjectByType<LevelAudioManager>();
        if (levelAudio != null)
        {
            levelAudio.volMusica   = music * master;
            levelAudio.volAmbience = sfx   * master;
            if (levelAudio.sourceMusica   != null) levelAudio.sourceMusica.volume   = music * master;
            if (levelAudio.sourceAmbience != null) levelAudio.sourceAmbience.volume = sfx   * master;
        }

        // 4. SFX del Player
        PlayerSoundController playerSfx = FindAnyObjectByType<PlayerSoundController>();
        if (playerSfx != null)
        {
            playerSfx.volCorrer = sfx * master;
            playerSfx.volAccion = sfx * master;
            playerSfx.volMuerte = sfx * master;
            if (playerSfx.sourceLoop != null)
                playerSfx.sourceLoop.volume = sfx * master;
        }

        // 5. SFX de UI
        if (UISoundManager.Instance != null)
            UISoundManager.Instance.volumen = sfx * master;

        // Guardar en disco
        PlayerPrefs.Save();
    }

    public void Resetear()
    {
        PlayerPrefs.SetFloat(KEY_MASTER, DEFAULT_MASTER);
        PlayerPrefs.SetFloat(KEY_MUSIC,  DEFAULT_MUSIC);
        PlayerPrefs.SetFloat(KEY_SFX,    DEFAULT_SFX);
        AplicarVolumenes();
    }
}
