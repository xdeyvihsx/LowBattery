using UnityEngine;

// FUENTE CENTRAL DE VERDAD: toda la logica de bateria vive aqui.
// PlayerDeath, PlayerDamage, HUDController y PowerUps hablan con este script.
public class PlayerData : MonoBehaviour
{
    public static PlayerData Instance { get; private set; }

    [Header("Bateria — el nivel empieza con 15%")]
    public float bateriaMaxima = 15f;

    [HideInInspector] public float bateriaActual;

    [Header("Drenaje pasivo (GDD: -1% cada 8s = 0.125/s)")]
    public float drenajeSegundo = 0.125f;
    public bool  drenajeActivo  = true;

    // ── Eventos ────────────────────────────────────────────────
    // Se dispara cada vez que cambia la bateria  → HUDController lo escucha
    public System.Action<float> OnBateriaChanged;
    // Se dispara UNA sola vez cuando llega a 0  → PlayerDeath lo escucha
    public System.Action        OnBateriaVacia;

    // Estado interno
    private bool yaDisparo0 = false;
    private bool pausado    = false;

    // ── Ciclo de vida ──────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        Resetear();
    }

    void Update()
    {
        if (pausado || !drenajeActivo || yaDisparo0) return;

        bateriaActual -= drenajeSegundo * Time.deltaTime;
        bateriaActual  = Mathf.Max(bateriaActual, 0f);
        OnBateriaChanged?.Invoke(bateriaActual);

        if (bateriaActual <= 0f)
            DispararMuerte();
    }

    // ── API pública ────────────────────────────────────────────

    /// Reinicia la bateria al maximo y habilita el drenaje.
    /// Llamado al inicio del nivel y en cada respawn.
    public void Resetear()
    {
        bateriaActual = bateriaMaxima;
        yaDisparo0    = false;
        pausado       = false;
        OnBateriaChanged?.Invoke(bateriaActual);
    }

    /// Obstaculos de dano (WhatsApp, Calls, Teams) llaman esto.
    public void RecibirDano(float cantidad)
    {
        if (pausado || yaDisparo0) return;

        bateriaActual = Mathf.Max(bateriaActual - cantidad, 0f);
        OnBateriaChanged?.Invoke(bateriaActual);

        if (bateriaActual <= 0f)
            DispararMuerte();
    }

    /// Power-ups llaman esto.
    public void RecargarBateria(float cantidad)
    {
        if (pausado) return;
        bateriaActual = Mathf.Min(bateriaActual + cantidad, bateriaMaxima);
        OnBateriaChanged?.Invoke(bateriaActual);
    }

    /// Devuelve 0..1 para el HUD.
    public float GetPorcentaje() => bateriaActual / bateriaMaxima;

    /// Congela el drenaje mientras el player esta muerto/en respawn.
    public void SetPausado(bool valor) => pausado = valor;

    // ── Privado ────────────────────────────────────────────────
    private void DispararMuerte()
    {
        yaDisparo0 = true;
        pausado    = true;
        OnBateriaVacia?.Invoke();
    }
}
