using UnityEngine;
using UnityEngine.SceneManagement;

// ─────────────────────────────────────────────────────────────────
// AudioBootstrap — Aplica los volumenes guardados al arrancar
//
// Se ejecuta en TODAS las escenas gracias a [RuntimeInitializeOnLoadMethod]
// No necesita estar en ningun GameObject — Unity lo ejecuta automaticamente.
//
// Flujo:
//   1. Juego arranca → AudioBootstrap.Init() corre antes de cualquier Start()
//   2. Lee PlayerPrefs y aplica AudioListener.volume inmediatamente
//   3. Cuando MenuMusicManager.Start() corre, ya tiene el volumen correcto
//   4. Al cambiar de escena → OnSceneLoaded() vuelve a aplicar los volumenes
// ─────────────────────────────────────────────────────────────────
public static class AudioBootstrap
{
    private const string KEY_MASTER = "vol_master";
    private const string KEY_MUSIC  = "vol_music";
    private const string KEY_SFX    = "vol_sfx";

    // Se ejecuta automaticamente al arrancar el juego, antes de cualquier Start()
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        // Aplicar el volumen master inmediatamente via AudioListener
        float master = PlayerPrefs.GetFloat(KEY_MASTER, 1.0f);
        AudioListener.volume = master;

        // Suscribirse a cambios de escena para reaplicar volumenes
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Esperar un frame para que los Singletons esten listos
        // Usamos un helper MonoBehaviour temporal
        var go = new UnityEngine.GameObject("_AudioBootstrapHelper");
        go.AddComponent<AudioBootstrapHelper>();
        UnityEngine.Object.DontDestroyOnLoad(go);
    }
}

// Helper temporal que espera un frame y aplica volumenes
public class AudioBootstrapHelper : MonoBehaviour
{
    private const string KEY_MASTER = "vol_master";
    private const string KEY_MUSIC  = "vol_music";
    private const string KEY_SFX    = "vol_sfx";

    void Start()
    {
        AplicarTodos();
        // Autodestruirse despues de aplicar
        Destroy(gameObject, 0.1f);
    }

    void AplicarTodos()
    {
        float master = PlayerPrefs.GetFloat(KEY_MASTER, 1.0f);
        float music  = PlayerPrefs.GetFloat(KEY_MUSIC,  0.7f);
        float sfx    = PlayerPrefs.GetFloat(KEY_SFX,    0.8f);

        // 1. Volumen maestro global — siempre funciona
        AudioListener.volume = master;

        // 2. Musica del menu (Singleton persistente entre escenas)
        if (MenuMusicManager.Instance != null)
        {
            MenuMusicManager.Instance.volumen = music * master;
            MenuMusicManager.Instance.AplicarVolumen();
        }

        // 3. Musica del nivel (solo existe en escenas Level_)
        LevelAudioManager levelAudio = FindFirstObjectByType<LevelAudioManager>();
        if (levelAudio != null)
        {
            if (levelAudio.sourceMusica != null)
                levelAudio.sourceMusica.volume = music * master;
            if (levelAudio.sourceAmbience != null)
                levelAudio.sourceAmbience.volume = sfx * master;
        }

        // 4. SFX del player
        PlayerSoundController playerSfx = FindFirstObjectByType<PlayerSoundController>();
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
    }
}
