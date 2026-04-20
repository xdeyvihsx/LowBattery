using UnityEngine;
using System.Collections;

// PlayerHitFlash — adjunto al GameObject "Sprite" que tiene el SpriteRenderer
// Jerarquia: Player -> Visuals -> Sprite (SpriteRenderer aqui)
public class PlayerHitFlash : MonoBehaviour
{
    public static PlayerHitFlash Instance { get; private set; }

    [Header("Color del flash de dano")]
    public Color colorFlash    = new Color(1f, 0.15f, 0.15f, 1f);
    public float duracionFlash = 0.4f;
    public int   numParpadeos  = 3;

    private SpriteRenderer sr;
    private Color          colorOriginal;
    private Coroutine      corFlash;

    void Awake()
    {
        Instance = this;

        // El SpriteRenderer esta en el MISMO GameObject que este script
        sr = GetComponent<SpriteRenderer>();

        if (sr == null)
            Debug.LogError("[HitFlash] SpriteRenderer no encontrado en " + gameObject.name
                + ". Asegurate de adjuntar PlayerHitFlash al GameObject 'Sprite'.");
        else
            colorOriginal = sr.color;
    }

    public void Flash()
    {
        if (sr == null) return;
        if (corFlash != null) StopCoroutine(corFlash);
        corFlash = StartCoroutine(CorFlash());
    }

    public void ResetColor()
    {
        if (corFlash != null) { StopCoroutine(corFlash); corFlash = null; }
        if (sr != null) sr.color = colorOriginal;
    }

    IEnumerator CorFlash()
    {
        float durPorCiclo = duracionFlash / (numParpadeos * 2f);
        for (int i = 0; i < numParpadeos; i++)
        {
            sr.color = colorFlash;
            yield return new WaitForSeconds(durPorCiclo);
            sr.color = colorOriginal;
            yield return new WaitForSeconds(durPorCiclo);
        }
        sr.color = colorOriginal;
        corFlash = null;
    }
}
