using UnityEngine;
using System.Collections;

public class PowerUpModoAvion : PowerUpBase
{
    [Header("Modo Avion (GDD)")]
    public float recargaBateria = 5f;
    public float duracionEscudo = 3f;
    public Color colorEscudo = new Color(0.4f, 0.8f, 1f, 0.6f);

    public static bool EscudoActivo { get; private set; } = false;

    protected override void AlRecoger()
    {
        if (PlayerData.Instance != null)
            PlayerData.Instance.RecargarBateria(recargaBateria);

        PowerUpManager manager = FindFirstObjectByType<PowerUpManager>();
        if (manager != null)
            manager.ActivarEscudoAvion(duracionEscudo, colorEscudo);
    }
}
