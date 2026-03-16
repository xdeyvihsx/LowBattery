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

    // ── Propiedades con PlayerPrefs ────────────────────────────
    public float VolumenMaster
    {
        get => PlayerPrefs.GetFloat(KEY_MASTER, DEFAULT_MASTER);
        set { PlayerPrefs.SetFloat(KEY_MASTER, Mathf.Clamp01(value)); AplicarVolumenes(); }
    }

    public float VolumenMusica
    {
        get => PlayerPrefs.GetFloat(KEY_MUSIC, DEFAULT_MUSIC);
        set { PlayerPrefs.SetFloat(KEY_MUSIC, Mathf.Clamp01(value)); AplicarVolumenes(); }
    }

    public float VolumenSFX
    {
        get => PlayerPrefs.GetFloat(KEY_SFX, DEFAULT_SFX);
        set { PlayerPrefs.SetFloat(KEY_SFX, Mathf.Clamp01(value)); AplicarVolumenes(); }
    }

    // ── Aplica en tiempo real a TODOS los sistemas de audio ────
    public void AplicarVolumenes()
    {
        float master = VolumenMaster;
        float music  = VolumenMusica;
        float sfx    = VolumenSFX;

        // 1. Musica del menu — MenuMusicManager (Singleton DontDestroyOnLoad)
        if (MenuMusicManager.Instance != null)
        {
            MenuMusicManager.Instance.volumen = music * master;
            MenuMusicManager.Instance.AplicarVolumen();
        }

        // 2. Musica del nivel — LevelAudioManager (solo existe en escenas de nivel)
        LevelAudioManager levelAudio = FindAnyObjectByType<LevelAudioManager>();
        if (levelAudio != null)
        {
            if (levelAudio.sourceMusica != null)
                levelAudio.sourceMusica.volume = music * master;
            levelAudio.volMusica = music * master;
        }

        // 3. Ambiente del nivel (tambien parte de LevelAudioManager)
        if (levelAudio != null && levelAudio.sourceAmbience != null)
            levelAudio.sourceAmbience.volume = sfx * master;

        // 4. SFX del Player — PlayerSoundController
        PlayerSoundController playerSfx = FindAnyObjectByType<PlayerSoundController>();
        if (playerSfx != null)
        {
            playerSfx.volCorrer  = sfx * master;
            playerSfx.volAccion  = sfx * master;
            playerSfx.volMuerte  = sfx * master;
            // Aplicar inmediatamente al source en loop (correr)
            if (playerSfx.sourceLoop != null)
                playerSfx.sourceLoop.volume = sfx * master;
        }

        // 5. SFX de UI — UISoundManager (Singleton DontDestroyOnLoad)
        if (UISoundManager.Instance != null)
            UISoundManager.Instance.volumen = sfx * master;

        // 6. AudioListener global — volumen maestro del sistema
        AudioListener.volume = master;

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
