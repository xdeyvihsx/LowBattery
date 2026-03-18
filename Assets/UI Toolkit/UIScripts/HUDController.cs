using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

public class HUDController : MonoBehaviour
{
    [Header("Colores Image Tint")]
    public Color tinteSinColor = Color.white;
    public Color tinteAmarillo = new Color(1f,    0.85f, 0.10f, 1f);
    public Color tinteRojo     = new Color(0.95f, 0.15f, 0.10f, 1f);
    public Color tinteVacio    = new Color(0.15f, 0.15f, 0.15f, 0.35f);

    [Header("Umbrales (0..1)")]
    public float umbralSinTinte = 0.60f;
    public float umbralAmarillo = 0.30f;

    [Header("Notificacion Power-Up")]
    public Color colorRecargaVerde = new Color(0.2f, 1f, 0.4f, 1f);
    public Color colorEscudoAzul   = new Color(0.3f, 0.8f, 1f, 1f);
    public float duracionNotif     = 1.8f;

    private const string ID_PORCENTAJE    = "BatteryPercentage";
    private const string ID_BARRA_BASE    = "Barras0";
    private const int    TOTAL_BARRAS     = 6;
    private const string ID_POWERUP_NOTIF = "PowerUpNotif";

    private Label           labelPct;
    private Label           labelPowerUpNotif;
    private VisualElement[] barras = new VisualElement[TOTAL_BARRAS];
    private UIDocument      uiDocument;
    private Coroutine       corNotif;

    public static HUDController Instance { get; private set; }

    void Awake()
    {
        Instance   = this;
        uiDocument = GetComponent<UIDocument>();
        if (uiDocument != null) uiDocument.sortingOrder = 0;
    }

    void OnEnable()
    {
        uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null) return;
        var root = uiDocument.rootVisualElement;

        labelPct = root.Q<Label>(ID_PORCENTAJE);

        for (int i = 0; i < TOTAL_BARRAS; i++)
            barras[i] = root.Q<VisualElement>(ID_BARRA_BASE + (i + 1));

        // Buscar o crear label de notificacion
        labelPowerUpNotif = root.Q<Label>(ID_POWERUP_NOTIF);
        if (labelPowerUpNotif == null)
        {
            labelPowerUpNotif = new Label();
            labelPowerUpNotif.name = ID_POWERUP_NOTIF;
            EstilarLabelNotif(labelPowerUpNotif);
            var charInfo = root.Q<VisualElement>("CharacterInfo") ?? root;
            charInfo.Add(labelPowerUpNotif);
        }
        labelPowerUpNotif.style.display = DisplayStyle.None;
    }

    void Start()
    {
        if (PlayerData.Instance != null)
        {
            PlayerData.Instance.OnBateriaChanged += ActualizarHUD;
            ActualizarHUD(PlayerData.Instance.bateriaActual);
        }
        else
        {
            Debug.LogWarning("[HUD] PlayerData.Instance es null en Start.");
        }
    }

    void OnDisable()
    {
        if (PlayerData.Instance != null)
            PlayerData.Instance.OnBateriaChanged -= ActualizarHUD;
    }

    // ── HUD de bateria ─────────────────────────────────────────
    void ActualizarHUD(float batActual)
    {
        if (PlayerData.Instance == null) return;

        float pct = PlayerData.Instance.GetPorcentaje(); // 0..1

        // Mostrar el valor entero de la bateria actual
        if (labelPct != null)
            labelPct.text = Mathf.CeilToInt(batActual).ToString();

        // Color del tinte segun porcentaje
        Color tinte;
        if      (pct > umbralSinTinte)  tinte = tinteSinColor;
        else if (pct > umbralAmarillo)  tinte = tinteAmarillo;
        else                            tinte = tinteRojo;

        // Cuantas barras activas (proporcional al porcentaje)
        int activas = Mathf.RoundToInt(pct * TOTAL_BARRAS);
        if (batActual > 0f && activas == 0) activas = 1;

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

    // ── Notificacion flotante de Power-Up ──────────────────────
    public void MostrarNotifPowerUp(string texto, Color color, string nombrePowerUp = "")
    {
        if (labelPowerUpNotif == null) return;
        if (corNotif != null) StopCoroutine(corNotif);
        corNotif = StartCoroutine(AnimarNotif(texto, color));
        Debug.Log("[HUD] Power-Up: " + nombrePowerUp + " " + texto);
    }

    IEnumerator AnimarNotif(string texto, Color color)
    {
        if (labelPowerUpNotif == null) yield break;

        labelPowerUpNotif.text  = texto;
        labelPowerUpNotif.style.color   = new StyleColor(color);
        labelPowerUpNotif.style.display = DisplayStyle.Flex;
        labelPowerUpNotif.style.opacity = 1f;

        float t = 0f;
        while (t < duracionNotif)
        {
            t += Time.deltaTime;
            float prog = t / duracionNotif;
            float yOffset = Mathf.Lerp(0f, -60f, prog);
            labelPowerUpNotif.style.marginBottom = yOffset;
            float alpha = prog < 0.5f ? 1f : 1f - ((prog - 0.5f) / 0.5f);
            labelPowerUpNotif.style.opacity = alpha;
            yield return null;
        }

        labelPowerUpNotif.style.display = DisplayStyle.None;
        labelPowerUpNotif.style.opacity = 1f;
    }

    void EstilarLabelNotif(Label label)
    {
        label.style.position                    = Position.Absolute;
        label.style.fontSize                    = 28;
        label.style.unityFontStyleAndWeight     = FontStyle.Bold;
        label.style.left                        = new StyleLength(new Length(50, LengthUnit.Percent));
        label.style.bottom                      = new StyleLength(120);
        label.style.color                       = new StyleColor(colorRecargaVerde);
        label.style.display                     = DisplayStyle.None;
        label.style.textShadow                  = new StyleTextShadow(new TextShadow {
            offset     = new Vector2(1, 1),
            blurRadius = 2,
            color      = new Color(0, 0, 0, 0.8f)
        });
    }
}
