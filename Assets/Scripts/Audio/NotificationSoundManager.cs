using UnityEngine;

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

    private AudioSource src;
    private float tWhatsapp = 0f;
    private float tTeams    = 0f;
    private float tRing     = 0f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // Silencioso — no es un error, es comportamiento esperado del Singleton
            Destroy(this);
            return;
        }
        Instance = this;

        src = GetComponent<AudioSource>();
        if (src == null) src = gameObject.AddComponent<AudioSource>();
        src.spatialBlend = 0f;
        src.playOnAwake  = false;
        src.loop         = false;
        src.priority     = 32;
    }

    void Update()
    {
        float d = Time.deltaTime;
        if (tWhatsapp > 0f) tWhatsapp -= d;
        if (tTeams    > 0f) tTeams    -= d;
        if (tRing     > 0f) tRing     -= d;
    }

    public void PlayNotificacion(string tipo)
    {
        if (!Valido("PlayNotificacion")) return;
        float vol = volNotificacion * SFXVol();
        switch (tipo)
        {
            case TIPO_WHATSAPP:
                if (tWhatsapp > 0f || sfxWhatsapp == null) return;
                src.PlayOneShot(sfxWhatsapp, vol); tWhatsapp = cooldownNotif; break;
            case TIPO_TEAMS:
                if (tTeams > 0f || sfxTeams == null) return;
                src.PlayOneShot(sfxTeams, vol); tTeams = cooldownNotif; break;
            case TIPO_LLAMADA:
                if (tRing > 0f || sfxRingtone == null) return;
                src.PlayOneShot(sfxRingtone, vol); tRing = cooldownNotif; break;
            default:
                Debug.LogWarning("[NotifSoundMgr] Tipo desconocido: " + tipo); break;
        }
    }

    public void PlayImpacto()
    {
        if (!Valido("PlayImpacto")) return;
        if (sfxImpacto == null) { Debug.LogWarning("[NotifSoundMgr] sfxImpacto no asignado."); return; }
        src.PlayOneShot(sfxImpacto, volImpacto * SFXVol());
    }

    bool Valido(string caller)
    {
        if (src == null) { Debug.LogError("[NotifSoundMgr] AudioSource null en " + caller); return false; }
        if (!src.enabled) src.enabled = true;
        if (!gameObject.activeInHierarchy) { Debug.LogError("[NotifSoundMgr] GO inactivo en " + caller); return false; }
        return true;
    }

    float SFXVol() =>
        PlayerPrefs.GetFloat("vol_master", 1f) *
        PlayerPrefs.GetFloat("vol_sfx",    0.8f);
}
