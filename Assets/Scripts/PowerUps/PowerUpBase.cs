using UnityEngine;
using System.Collections;

// ─────────────────────────────────────────────────────────────────
// PowerUpBase — Clase base para todos los power-ups
//
// Maneja:
//  - Animacion flotante (bob up/down)
//  - Animacion de rotacion suave
//  - Efecto de recogida (escala + fade)
//  - Colision con el player via trigger
//  - Destruccion o desactivacion al ser recogido
// ─────────────────────────────────────────────────────────────────
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public abstract class PowerUpBase : MonoBehaviour
{
    [Header("Animacion flotante")]
    public float amplitudFlotacion = 0.15f;
    public float velocidadFlotacion = 2f;

    [Header("Rotacion")]
    public bool rotarSprite    = false;
    public float velocidadGiro = 45f;

    [Header("Efecto al recoger")]
    public float duracionEfectoRecogida = 0.35f;

    protected SpriteRenderer sr;
    private   Vector3         posicionBase;
    private   bool            recogido = false;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        // Asegurarse de que el collider es trigger
        foreach (var col in GetComponents<Collider2D>())
            col.isTrigger = true;
    }

    void Start()
    {
        posicionBase = transform.position;
    }

    void Update()
    {
        if (recogido) return;

        // Animacion flotante
        float offsetY = Mathf.Sin(Time.time * velocidadFlotacion) * amplitudFlotacion;
        transform.position = new Vector3(
            posicionBase.x,
            posicionBase.y + offsetY,
            posicionBase.z
        );

        // Rotacion opcional
        if (rotarSprite)
            transform.Rotate(0f, 0f, velocidadGiro * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D otro)
    {
        if (recogido) return;
        if (otro.gameObject.layer != LayerMask.NameToLayer("Player")) return;

        recogido = true;
        AlRecoger();
        StartCoroutine(EfectoYDestruir());
    }

    // Coroutine de efecto visual al recoger (escala pop + fade)
    IEnumerator EfectoYDestruir()
    {
        float t = 0f;
        Vector3 escalaInicial = transform.localScale;
        Vector3 escalaFinal   = escalaInicial * 1.4f;
        Color   colorInicial  = sr.color;

        while (t < duracionEfectoRecogida)
        {
            t += Time.deltaTime;
            float prog = t / duracionEfectoRecogida;

            // Pop de escala y fade de alpha
            transform.localScale = Vector3.Lerp(escalaInicial, escalaFinal, prog);
            Color c = colorInicial;
            c.a = 1f - prog;
            sr.color = c;

            yield return null;
        }

        gameObject.SetActive(false);
    }

    // Implementar en cada power-up hijo
    protected abstract void AlRecoger();
}
