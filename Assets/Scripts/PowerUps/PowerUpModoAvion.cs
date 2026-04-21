using UnityEngine;

public class PowerUpModoAvion : PowerUpBase
{
    [Header("Modo Avion (GDD: +5% + escudo 3 seg)")]
    public float recargaBateria = 5f;
    public float duracionEscudo = 3f;
    public Color colorEscudo    = new Color(0.4f, 0.8f, 1f, 0.6f);

    protected override void AlRecoger()
    {
        if (PlayerData.Instance != null)
            PlayerData.Instance.RecargarBateria(recargaBateria);

        // 1. Recargar bateria
        PowerUpManager.Instance?.ActivarEscudoAvion(duracionEscudo, colorEscudo);

        // 2. Cadena de audio: recogida -> subida bateria
        PowerUpManager.Instance?.PlayPowerUpAudio();

        // 3. Notificacion en HUD
        // if (HUDController.Instance != null)
        //     HUDController.Instance.MostrarNotifPowerUp(
        //         "+" + recargaBateria + "% ESCUDO",
        //         new Color(0.3f, 0.8f, 1f, 1f),
        //         "Modo Avion"
        //     );

        // Debug.Log("[ModoAvion] +" + recargaBateria + "% + escudo " + duracionEscudo + "s");
    }
}
