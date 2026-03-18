using UnityEngine;

// ─────────────────────────────────────────────────────────────────
// NotificationReveal — Efecto fantasma para notificaciones
//
// El sprite empieza invisible (alpha=0) y aparece gradualmente
// cuando el player entra en el rango de deteccion.
// Cuando el player se aleja, vuelve a desaparecer.
// ─────────────────────────────────────────────────────────────────
[RequireComponent(typeof(SpriteRenderer))]
public class NotificationReveal : MonoBehaviour
{
    [Header("Rango de vision")]
    [Tooltip("Distancia a la que el sprite empieza a aparecer")]
    public float rangoDeteccion = 5f;

    [Tooltip("Distancia a la que el sprite llega a ser completamente visible")]
    public float rangoVisible = 2.5f;

    [Header("Velocidad del fade")]
    [Tooltip("Que tan rapido aparece y desaparece (mayor = mas rapido)")]
    public float velocidadFade = 3f;

    [Header("Alpha minimo cuando no hay player cerca")]
    [Range(0f, 0.3f)]
    public float alphaMinimo = 0f;

    [Header("Alpha maximo cuando el player esta cerca")]
    [Range(0.5f, 1f)]
    public float alphaMaximo = 1f;

    // Referencias privadas
    private SpriteRenderer sr;
    private Transform      playerTransform;
    private float          alphaActual = 0f;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        // Empezar completamente invisible
        SetAlpha(alphaMinimo);
    }

    void Start()
    {
        // Buscar el player por tag o por componente
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null)
        {
            // Fallback: buscar por componente PlayerMovement
            PlayerMovement pm = FindFirstObjectByType<PlayerMovement>();
            if (pm != null) playerObj = pm.gameObject;
        }

        if (playerObj != null)
            playerTransform = playerObj.transform;
        else
            Debug.LogWarning("[NotificationReveal] No se encontro el Player.");
    }

    void Update()
    {
        if (sr == null) return;

        float alphaObjetivo = alphaMinimo;

        if (playerTransform != null)
        {
            float distancia = Vector2.Distance(transform.position, playerTransform.position);

            if (distancia <= rangoVisible)
            {
                // Dentro del rango visible → alpha maximo
                alphaObjetivo = alphaMaximo;
            }
            else if (distancia <= rangoDeteccion)
            {
                // En la zona de transicion → interpolar
                float t = 1f - (distancia - rangoVisible) / (rangoDeteccion - rangoVisible);
                alphaObjetivo = Mathf.Lerp(alphaMinimo, alphaMaximo, t);
            }
        }

        // Interpolar suavemente hacia el alpha objetivo
        alphaActual = Mathf.Lerp(alphaActual, alphaObjetivo, Time.deltaTime * velocidadFade);
        SetAlpha(alphaActual);
    }

    void SetAlpha(float alpha)
    {
        Color c = sr.color;
        c.a     = Mathf.Clamp01(alpha);
        sr.color = c;
    }

    // Visualizar el rango en la Scene View
    void OnDrawGizmosSelected()
    {
        // Rango de deteccion (empieza a aparecer)
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, rangoDeteccion);

        // Rango visible (completamente visible)
        Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, rangoVisible);
    }
}
