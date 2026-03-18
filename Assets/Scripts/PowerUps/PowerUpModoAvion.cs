using UnityEngine;
using System.Collections;

public class PowerUpModoAvion : PowerUpBase
{
    [Header("Modo Avion (GDD: +5% + escudo 3 seg)")]
    public float recargaBateria = 5f;
    public float duracionEscudo = 3f;
    public Color colorEscudo    = new Color(0.4f, 0.8f, 1f, 0.6f);

    public static bool EscudoActivo { get; private set; } = false;

    protected override void AlRecoger()
    {
        // 1. Recargar bateria
        if (PlayerData.Instance != null)
            PlayerData.Instance.RecargarBateria(recargaBateria);

        // 2. Activar escudo via PowerUpManager
        PowerUpManager manager = FindFirstObjectByType<PowerUpManager>();
        if (manager != null)
            manager.ActivarEscudoAvion(duracionEscudo, colorEscudo);

        // 3. Notificacion en HUD: muestra recarga + icono de escudo
        if (HUDController.Instance != null)
        {
            HUDController.Instance.MostrarNotifPowerUp(
                "+" + recargaBateria + "% ✈ ESCUDO",
                new Color(0.3f, 0.8f, 1f, 1f),
                "Modo Avion"
            );
        }

        Debug.Log("[ModoAvion] +" + recargaBateria + "% + escudo " + duracionEscudo + " seg");
    }
}
