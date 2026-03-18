using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public static PlayerData Instance { get; private set; }

    [Header("Bateria")]
    [Tooltip("Maximo posible (100 = 100%)")]
    public float bateriaMaxima = 100f;

    [Tooltip("Bateria al iniciar el nivel - GDD: 15%")]
    public float bateriaInicial = 15f;

    [HideInInspector] public float bateriaActual;

    [Header("Drenaje pasivo (GDD: -1% cada 8s)")]
    public float drenajeSegundo = 0.125f;
    public bool  drenajeActivo  = true;

    public System.Action<float> OnBateriaChanged;
    public System.Action        OnBateriaVacia;

    private bool yaDisparo0 = false;
    private bool pausado    = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // FORZAR valores correctos en Awake por si el Inspector tiene valores viejos
        if (bateriaMaxima <= 15f)
        {
            Debug.LogWarning("[PlayerData] bateriaMaxima estaba en " + bateriaMaxima +
                             " — corrigiendo a 100. Ajusta el Inspector manualmente.");
            bateriaMaxima = 100f;
        }
        if (bateriaInicial > bateriaMaxima)
        {
            bateriaInicial = 15f;
        }
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

    public void Resetear()
    {
        bateriaActual = bateriaInicial;
        yaDisparo0    = false;
        pausado       = false;
        OnBateriaChanged?.Invoke(bateriaActual);

        Debug.Log("[PlayerData] Bateria reseteada a: " + bateriaActual + " / " + bateriaMaxima);
    }

    public void RecibirDano(float cantidad)
    {
        if (pausado || yaDisparo0) return;
        bateriaActual = Mathf.Max(bateriaActual - cantidad, 0f);
        OnBateriaChanged?.Invoke(bateriaActual);
        if (bateriaActual <= 0f) DispararMuerte();
    }

    public void RecargarBateria(float cantidad)
    {
        if (pausado) return;
        float antes = bateriaActual;
        bateriaActual = Mathf.Min(bateriaActual + cantidad, bateriaMaxima);
        OnBateriaChanged?.Invoke(bateriaActual);
        Debug.Log("[PlayerData] Recarga: +" + cantidad
                  + " → " + antes + " → " + bateriaActual
                  + " (max=" + bateriaMaxima + ")");
    }

    public float GetPorcentaje() => bateriaActual / bateriaMaxima;

    public void SetPausado(bool valor)
    {
        pausado = valor;
        if (valor) yaDisparo0 = true;
    }

    private void DispararMuerte()
    {
        yaDisparo0 = true;
        pausado    = true;
        OnBateriaVacia?.Invoke();
    }
}
