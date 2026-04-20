using UnityEngine;

// NotificationSoundManager v3 — fix AudioSource disabled
// Usa un AudioSource en el propio GameObject en lugar de crear hijos
// para evitar el bug de "Can not play a disabled audio source"
public class NotificationSoundManager : MonoBehaviour
{
    public static NotificationSoundManager Instance { get; private set; }

    [Header("SFX Notificaciones")]
    public AudioClip sfxWhatsapp;
    public AudioClip sfxTeams;
    public AudioClip sfxRingtone;

    [Header("SFX Dano jugador")]
    public AudioClip sfxImpacto;

    [Header("Volumenes base")]
    [Range(0f,1f)] public float volNotificacion = 0.75f;
    [Range(0f,1f)] public float volImpacto      = 1.0f;

    [Header("Cooldown notificaciones (seg)")]
    public float cooldownNotif = 1.5f;

    public const string TIPO_WHATSAPP = "Whatsapp";
    public const string TIPO_TEAMS    = "Teams";
    public const string TIPO_LLAMADA  = "Llamada";

    // Un unico AudioSource en el propio GO — nunca se deshabilita
    private AudioSource src;

    private float tWhatsapp = 0f;
    private float tTeams    = 0f;
    private float tRing     = 0f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[NotifSoundMgr] Duplicado eliminado de: " + gameObject.name);
            Destroy(this);
            return;
        }
        Instance = this;

        // Usar el AudioSource del propio GameObject
        // Si no existe, agregar uno
        src = GetComponent<AudioSource>();
        if (src == null)
            src = gameObject.AddComponent<AudioSource>();

        src.spatialBlend = 0f;   // 2D — suena igual en toda la pantalla
        src.playOnAwake  = false;
        src.loop         = false;
        src.priority     = 32;

        Debug.Log("[NotifSoundMgr] Listo. src=" + (src != null && src.enabled));
    }

    void Update()
    {
        float d = Time.deltaTime;
        if (tWhatsapp > 0f) tWhatsapp -= d;
        if (tTeams    > 0f) tTeams    -= d;
        if (tRing     > 0f) tRing     -= d;
    }

    // ── Sonido de aparicion de notificacion ───────────────────
    public void PlayNotificacion(string tipo)
    {
        if (!ValidarSource("PlayNotificacion")) return;
        float vol = volNotificacion * SFXVol();

        switch (tipo)
        {
            case TIPO_WHATSAPP:
                if (tWhatsapp > 0f || sfxWhatsapp == null) return;
                src.PlayOneShot(sfxWhatsapp, vol);
                tWhatsapp = cooldownNotif;
                break;
            case TIPO_TEAMS:
                if (tTeams > 0f || sfxTeams == null) return;
                src.PlayOneShot(sfxTeams, vol);
                tTeams = cooldownNotif;
                break;
            case TIPO_LLAMADA:
                if (tRing > 0f || sfxRingtone == null) return;
                src.PlayOneShot(sfxRingtone, vol);
                tRing = cooldownNotif;
                break;
            default:
                Debug.LogWarning("[NotifSoundMgr] Tipo desconocido: " + tipo);
                break;
        }
    }

    // ── Sonido de impacto al player ────────────────────────────
    public void PlayImpacto()
    {
        if (!ValidarSource("PlayImpacto")) return;
        if (sfxImpacto == null)
        {
            Debug.LogWarning("[NotifSoundMgr] sfxImpacto no asignado en Inspector.");
            return;
        }
        // Sin cooldown — la notificacion ya desaparece, no puede spamear
        src.PlayOneShot(sfxImpacto, volImpacto * SFXVol());
        Debug.Log("[NotifSoundMgr] Impacto reproducido.");
    }

    // ── Validacion robusta del AudioSource ────────────────────
    bool ValidarSource(string caller)
    {
        if (src == null)
        {
            Debug.LogError("[NotifSoundMgr] AudioSource null en " + caller);
            return false;
        }
        if (!src.enabled)
        {
            Debug.LogError("[NotifSoundMgr] AudioSource DISABLED en " + caller
                + ". Habilitandolo...");
            src.enabled = true;
        }
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogError("[NotifSoundMgr] GameObject inactivo en " + caller);
            return false;
        }
        return true;
    }

    float SFXVol() =>
        PlayerPrefs.GetFloat("vol_master", 1f) *
        PlayerPrefs.GetFloat("vol_sfx",    0.8f);
}
