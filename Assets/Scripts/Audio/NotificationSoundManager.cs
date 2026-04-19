using UnityEngine;

// ─────────────────────────────────────────────────────────────────
// NotificationSoundManager — Singleton dedicado de audio
//
// DEBE vivir en UN SOLO GameObject en la escena.
// NO adjuntar a notificaciones individuales.
// ─────────────────────────────────────────────────────────────────
public class NotificationSoundManager : MonoBehaviour
{
    public static NotificationSoundManager Instance { get; private set; }

    [Header("SFX Notificaciones (Assets/SFX/Notifications/)")]
    public AudioClip sfxWhatsapp;
    public AudioClip sfxTeams;
    public AudioClip sfxRingtone;

    [Header("SFX Dano jugador (Assets/SFX/Player/Damage/)")]
    public AudioClip sfxImpacto;

    [Header("Volumenes base")]
    [Range(0f,1f)] public float volNotificacion = 0.75f;
    [Range(0f,1f)] public float volImpacto      = 0.90f;

    [Header("Cooldown entre reproducciones (seg)")]
    public float cooldownNotif   = 1.2f;
    public float cooldownImpacto = 0.2f;

    public const string TIPO_WHATSAPP = "Whatsapp";
    public const string TIPO_TEAMS    = "Teams";
    public const string TIPO_LLAMADA  = "Llamada";

    private AudioSource srcNotif;
    private AudioSource srcImpacto;

    private float tWhatsapp = 0f;
    private float tTeams    = 0f;
    private float tRing     = 0f;
    private float tImpacto  = 0f;

    void Awake()
    {
        // Singleton estricto — si ya existe una instancia, destruir esta
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[NotifSoundMgr] Instancia duplicada detectada en '"
                + gameObject.name + "' — destruyendo. "
                + "El NotificationSoundManager debe estar solo en su propio GameObject.");
            Destroy(this);   // Destruir solo el componente, no el GameObject completo
            return;
        }

        Instance = this;

        // Crear AudioSources hijos dedicados
        srcNotif   = CrearAudioSource("Src_Notificaciones", priority: 64);
        srcImpacto = CrearAudioSource("Src_ImpactoJugador", priority: 32);
    }

    void Update()
    {
        float d = Time.deltaTime;
        if (tWhatsapp > 0f) tWhatsapp -= d;
        if (tTeams    > 0f) tTeams    -= d;
        if (tRing     > 0f) tRing     -= d;
        if (tImpacto  > 0f) tImpacto  -= d;
    }

    // ── API publica ────────────────────────────────────────────

    public void PlayNotificacion(string tipo)
    {
        float vol = volNotificacion * SFXVolume();

        switch (tipo)
        {
            case TIPO_WHATSAPP:
                if (tWhatsapp > 0f || sfxWhatsapp == null) return;
                srcNotif.PlayOneShot(sfxWhatsapp, vol);
                tWhatsapp = cooldownNotif;
                break;

            case TIPO_TEAMS:
                if (tTeams > 0f || sfxTeams == null) return;
                srcNotif.PlayOneShot(sfxTeams, vol);
                tTeams = cooldownNotif;
                break;

            case TIPO_LLAMADA:
                if (tRing > 0f || sfxRingtone == null) return;
                srcNotif.PlayOneShot(sfxRingtone, vol);
                tRing = cooldownNotif;
                break;

            default:
                Debug.LogWarning("[NotifSoundMgr] Tipo desconocido: '" + tipo
                    + "'. Usa: Whatsapp | Teams | Llamada");
                break;
        }
    }

    public void PlayImpacto()
    {
        if (tImpacto > 0f || sfxImpacto == null) return;
        srcImpacto.PlayOneShot(sfxImpacto, volImpacto * SFXVolume());
        tImpacto = cooldownImpacto;
    }

    // ── Helpers internos ───────────────────────────────────────

    AudioSource CrearAudioSource(string nombre, int priority)
    {
        var go            = new GameObject(nombre);
        go.transform.SetParent(transform);
        var src           = go.AddComponent<AudioSource>();
        src.spatialBlend  = 0f;     // 2D — suena igual en toda la pantalla
        src.priority      = priority;
        src.playOnAwake   = false;
        src.loop          = false;
        src.outputAudioMixerGroup = null;
        return src;
    }

    // Lee vol_master y vol_sfx de PlayerPrefs (mismo sistema que AudioSettingsData)
    float SFXVolume()
    {
        return PlayerPrefs.GetFloat("vol_master", 1f)
             * PlayerPrefs.GetFloat("vol_sfx",    0.8f);
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        // Advertir en Editor si hay multiples instancias en la escena
        var todos = FindObjectsByType<NotificationSoundManager>(FindObjectsSortMode.None);
        if (todos.Length > 1)
            Debug.LogError("[NotifSoundMgr] HAY " + todos.Length
                + " INSTANCIAS en la escena. Solo debe haber UNA. "
                + "Quita el componente de las notificaciones individuales.");
    }
#endif
}
