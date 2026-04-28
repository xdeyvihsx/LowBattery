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

    private Vector3 posicionOriginal;
    private Vector3 escalaOriginal;
    private Vector3 posicionBase;

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
        posicionOriginal  = transform.position;
        escalaOriginal    = transform.localScale;
        posicionBase      = posicionOriginal;

        PlayerDeath pd = FindFirstObjectByType<PlayerDeath>();
        if (pd != null) pd.OnRespawn += Reiniciar;
    }

    void OnDestroy()
    {
        PlayerDeath pd = FindFirstObjectByType<PlayerDeath>();
        if (pd != null) pd.OnRespawn -= Reiniciar;
    }

    void Update()
    {
        if (recogido) return;
        float y = Mathf.Sin(Time.time * velocidadFlotacion) * amplitudFlotacion;
        transform.position = new Vector3(posicionBase.x, posicionBase.y + y, posicionBase.z);
        if (rotarSprite) transform.Rotate(0f, 0f, velocidadGiro * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D otro)
    {
        if (recogido) return;
        if (otro.gameObject.layer != LayerMask.NameToLayer("Player")) return;
        recogido = true;

        // 1. Logica especifica del power-up (recarga bateria, escudo, etc.)
        AlRecoger();

        // 2. Sonido secuencial: recogida → subida de bateria
        if (PowerUpAudioManager.Instance != null)
            PowerUpAudioManager.Instance.PlaySecuencia();
        else
            Debug.LogWarning("[PowerUpBase] PowerUpAudioManager.Instance es null en " + gameObject.name);

        // 3. Efecto visual pop + fade
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

        gameObject.SetActive(false);
    }

    void Reiniciar()
    {
        recogido             = false;
        transform.position   = posicionOriginal;
        transform.localScale = escalaOriginal;
        posicionBase         = posicionOriginal;
        if (sr != null) { Color c = sr.color; c.a = 1f; sr.color = c; }
        gameObject.SetActive(true);
    }

    protected abstract void AlRecoger();
}
