using UnityEngine;

public class PowerUpPowerBank : PowerUpBase
{
    [Header("Power Bank (GDD: +10%)")]
    public float recargaBateria = 10f;

    protected override void AlRecoger()
    {
        if (PlayerData.Instance == null) return;

        PlayerData.Instance.RecargarBateria(recargaBateria);

        // Mostrar notificacion en el HUD
        if (HUDController.Instance != null)
            HUDController.Instance.MostrarNotifPowerUp(
                "+" + recargaBateria + "%",
                new Color(0.2f, 1f, 0.4f, 1f),
                "Power Bank"
            );

        Debug.Log("[PowerBank] +" + recargaBateria + "% bateria. Total: " + PlayerData.Instance.bateriaActual);
    }
}
