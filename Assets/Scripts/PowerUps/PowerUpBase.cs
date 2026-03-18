using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public abstract class PowerUpBase : MonoBehaviour
{
    [Header("Animacion flotante")]
    public float amplitudFlotacion  = 0.15f;
    public float velocidadFlotacion = 2f;

    [Header("Rotacion")]
    public bool  rotarSprite   = false;
    public float velocidadGiro = 45f;

    [Header("Efecto al recoger")]
    public float duracionEfectoRecogida = 0.35f;

    // Estado original guardado al inicio
    private Vector3 posicionOriginal;
    private Vector3 escalaOriginal;
    private Vector3 posicionBase;   // posicion que usa la animacion flotante

    protected SpriteRenderer sr;
    private   bool            recogido = false;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        foreach (var col in GetComponents<Collider2D>())
            col.isTrigger = true;
    }

    void Start()
    {
        // Guardar estado original
        posicionOriginal  = transform.position;
        escalaOriginal    = transform.localScale;
        posicionBase      = posicionOriginal;

        // Suscribirse al evento OnRespawn del PlayerDeath
        PlayerDeath pd = FindFirstObjectByType<PlayerDeath>();
        if (pd != null)
            pd.OnRespawn += Reiniciar;
        else
            Debug.LogWarning("[PowerUpBase] No encontre PlayerDeath para suscribirse.");
    }

    void OnDestroy()
    {
        PlayerDeath pd = FindFirstObjectByType<PlayerDeath>();
        if (pd != null) pd.OnRespawn -= Reiniciar;
    }

    void Update()
    {
        if (recogido) return;

        // Animacion flotante
        float y = Mathf.Sin(Time.time * velocidadFlotacion) * amplitudFlotacion;
        transform.position = new Vector3(posicionBase.x, posicionBase.y + y, posicionBase.z);

        if (rotarSprite)
            transform.Rotate(0f, 0f, velocidadGiro * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D otro)
    {
        if (recogido) return;
        if (otro.gameObject.layer != LayerMask.NameToLayer("Player")) return;
        recogido = true;
        AlRecoger();
        StartCoroutine(EfectoYOcultar());
    }

    IEnumerator EfectoYOcultar()
    {
        float   t   = 0f;
        Vector3 e0  = transform.localScale;
        Vector3 e1  = e0 * 1.4f;
        Color   c0  = sr.color;

        while (t < duracionEfectoRecogida)
        {
            t += Time.deltaTime;
            float p = t / duracionEfectoRecogida;
            transform.localScale = Vector3.Lerp(e0, e1, p);
            Color c = c0; c.a = 1f - p;
            sr.color = c;
            yield return null;
        }

        // Ocultar SIN destruir — se reactiva en Reiniciar()
        gameObject.SetActive(false);
    }

    // ── Se llama desde PlayerDeath.OnRespawn ──────────────────
    void Reiniciar()
    {
        recogido             = false;
        transform.position   = posicionOriginal;
        transform.localScale = escalaOriginal;
        posicionBase         = posicionOriginal;

        // Restaurar alpha del sprite
        if (sr != null)
        {
            Color c = sr.color;
            c.a      = 1f;
            sr.color = c;
        }

        gameObject.SetActive(true);
        Debug.Log("[PowerUp] Reiniciado: " + gameObject.name);
    }

    protected abstract void AlRecoger();
}
