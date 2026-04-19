using UnityEngine;

// ─────────────────────────────────────────────────────────────────
// NotificationReveal — Efecto fantasma + trigger de sonido
//
// NOTA: Este script NO debe tener NotificationSoundManager adjunto.
// El NotificationSoundManager vive en su propio GameObject dedicado.
// ─────────────────────────────────────────────────────────────────
[RequireComponent(typeof(SpriteRenderer))]
public class NotificationReveal : MonoBehaviour
{
    [Header("Rango de deteccion")]
    public float rangoDeteccion = 5f;
    public float rangoVisible   = 2.5f;
    public float velocidadFade  = 3f;

    [Header("Alpha")]
    [Range(0f, 0.3f)] public float alphaMinimo = 0f;
    [Range(0.5f, 1f)] public float alphaMaximo = 1f;

    [Header("Sonido de aparicion")]
    [Tooltip("Whatsapp | Teams | Llamada")]
    public string tipoNotificacion = NotificationSoundManager.TIPO_WHATSAPP;

    [Header("Umbral de alpha para disparar SFX (0..1)")]
    [Range(0.05f, 0.5f)] public float umbralSonido = 0.15f;

    private SpriteRenderer sr;
    private Transform      playerTr;
    private float          alphaActual     = 0f;
    private bool           sonidoDisparado = false;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        // Los colliders deben ser triggers para detectar al player
        foreach (var col in GetComponents<Collider2D>())
            col.isTrigger = true;

        // Empezar invisible
        SetAlpha(alphaMinimo);
    }

    void Start()
    {
        // Buscar al player por componente (no depende de tags)
        var pm = FindFirstObjectByType<PlayerMovement>();
        if (pm != null)
            playerTr = pm.transform;
        else
            Debug.LogWarning("[NotifReveal] No encontre PlayerMovement en " + gameObject.name);
    }

    void Update()
    {
        if (sr == null) return;

        float objetivo = alphaMinimo;

        if (playerTr != null)
        {
            float dist = Vector2.Distance(transform.position, playerTr.position);

            if (dist <= rangoVisible)
            {
                objetivo = alphaMaximo;
            }
            else if (dist <= rangoDeteccion)
            {
                float t = 1f - (dist - rangoVisible) / (rangoDeteccion - rangoVisible);
                objetivo = Mathf.Lerp(alphaMinimo, alphaMaximo, t);
            }
        }

        // Interpolacion suave con MoveTowards (mas predecible que Lerp)
        alphaActual = Mathf.MoveTowards(alphaActual, objetivo, velocidadFade * Time.deltaTime);
        SetAlpha(alphaActual);

        // Disparar SFX solo cuando cruza el umbral por primera vez
        if (!sonidoDisparado && alphaActual >= umbralSonido)
        {
            // Acceder al Singleton directamente
            NotificationSoundManager nsm = NotificationSoundManager.Instance;
            if (nsm != null)
                nsm.PlayNotificacion(tipoNotificacion);
            else
                Debug.LogWarning("[NotifReveal] NotificationSoundManager.Instance es null en " + gameObject.name);

            sonidoDisparado = true;
        }

        // Resetear flag cuando el sprite vuelve a estar completamente oculto
        if (sonidoDisparado && alphaActual <= alphaMinimo + 0.01f)
            sonidoDisparado = false;
    }

    void SetAlpha(float a)
    {
        Color c = sr.color;
        c.a      = Mathf.Clamp01(a);
        sr.color = c;
    }

    void OnDrawGizmosSelected()
    {
        // Circulo amarillo = zona de deteccion (empieza a aparecer)
        Gizmos.color = new Color(1f, 1f, 0f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, rangoDeteccion);
        // Circulo verde = zona de maxima visibilidad
        Gizmos.color = new Color(0f, 1f, 0f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, rangoVisible);
    }
}
