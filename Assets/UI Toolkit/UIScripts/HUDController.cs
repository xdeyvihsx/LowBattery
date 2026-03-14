using UnityEngine;
using UnityEngine.UIElements;

// Lee PlayerData y actualiza el HUD de UI Toolkit:
//  - Label #BatteryPercentage  → numero entero de bateria
//  - #Barras01..#Barras06      → Image Tint segun nivel
//    > 60%  → sin tinte (blanco, color original del sprite)
//    30-60% → amarillo
//    < 30%  → rojo
//  Las barras inactivas (vacias) se oscurecen.
public class HUDController : MonoBehaviour
{
    [Header("Colores Image Tint")]
    public Color tinteSinColor = Color.white;                           // > 60% — sin tinte
    public Color tinteAmarillo = new Color(1f,   0.85f, 0.10f, 1f);   // 30-60%
    public Color tinteRojo     = new Color(0.95f,0.15f, 0.10f, 1f);   // < 30%
    public Color tinteVacio    = new Color(0.15f,0.15f, 0.15f, 0.35f);// barra apagada

    [Header("Umbrales (0..1)")]
    public float umbralSinTinte = 0.60f;   // por encima → blanco (sin tinte)
    public float umbralAmarillo = 0.30f;   // por encima → amarillo; por debajo → rojo

    // IDs UXML — deben coincidir exactamente con tus nombres en el UI Builder
    private const string ID_PORCENTAJE = "BatteryPercentage";
    private const string ID_BARRA_BASE = "Barras0";   // Barras01 … Barras06
    private const int    TOTAL_BARRAS  = 6;

    private Label           labelPct;
    private VisualElement[] barras = new VisualElement[TOTAL_BARRAS];

    // ── Setup ──────────────────────────────────────────────────
    void OnEnable()
    {
        var doc = GetComponent<UIDocument>();
        if (doc == null) { Debug.LogError("[HUDController] Falta UIDocument."); return; }

        var root = doc.rootVisualElement;

        labelPct = root.Q<Label>(ID_PORCENTAJE);
        if (labelPct == null)
            Debug.LogWarning($"[HUDController] No encontre '{ID_PORCENTAJE}' en el UXML.");

        for (int i = 0; i < TOTAL_BARRAS; i++)
        {
            string id = ID_BARRA_BASE + (i + 1);
            barras[i] = root.Q<VisualElement>(id);
            if (barras[i] == null)
                Debug.LogWarning($"[HUDController] No encontre '{id}' en el UXML.");
        }
    }

    void Start()
    {
        if (PlayerData.Instance == null)
        {
            Debug.LogWarning("[HUDController] PlayerData no esta en la escena.");
            return;
        }
        // Suscribirse y pintar el estado inicial
        PlayerData.Instance.OnBateriaChanged += ActualizarHUD;
        ActualizarHUD(PlayerData.Instance.bateriaActual);
    }

    void OnDisable()
    {
        if (PlayerData.Instance != null)
            PlayerData.Instance.OnBateriaChanged -= ActualizarHUD;
    }

    // ── Logica del HUD ─────────────────────────────────────────
    void ActualizarHUD(float batActual)
    {
        float pct = PlayerData.Instance != null ? PlayerData.Instance.GetPorcentaje() : 0f;

        // 1. Texto numerico
        if (labelPct != null)
            labelPct.text = Mathf.CeilToInt(batActual).ToString();

        // 2. Elegir tinte segun porcentaje
        Color tinte;
        if (pct > umbralSinTinte)
            tinte = tinteSinColor;       // sin tinte (blanco)
        else if (pct > umbralAmarillo)
            tinte = tinteAmarillo;       // amarillo
        else
            tinte = tinteRojo;           // rojo

        // 3. Cuantas barras encendidas (proporcional a 0..1 × 6)
        int activas = Mathf.RoundToInt(pct * TOTAL_BARRAS);
        // Garantizar que con bateria > 0 haya al menos 1 barra
        if (batActual > 0f && activas == 0) activas = 1;

        // 4. Pintar barras via Image Tint
        for (int i = 0; i < TOTAL_BARRAS; i++)
        {
            if (barras[i] == null) continue;

            if (i < activas)
            {
                barras[i].style.unityBackgroundImageTintColor = new StyleColor(tinte);
                barras[i].style.opacity = 1f;
            }
            else
            {
                barras[i].style.unityBackgroundImageTintColor = new StyleColor(tinteVacio);
                barras[i].style.opacity = 0.35f;
            }
        }
    }
}
