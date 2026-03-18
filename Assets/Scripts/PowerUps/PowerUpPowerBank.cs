using UnityEngine;

// ─────────────────────────────────────────────────────────────────
// PowerUpPowerBank — Recarga +10% de bateria (GDD)
// ─────────────────────────────────────────────────────────────────
public class PowerUpPowerBank : PowerUpBase
{
    [Header("Power Bank")]
    [Tooltip("Porcentaje de bateria que recarga (GDD: +10%)")]
    public float recargaBateria = 10f;

    protected override void AlRecoger()
    {
        if (PlayerData.Instance == null) return;

        PlayerData.Instance.RecargarBateria(recargaBateria);
        Debug.Log("[PowerBank] +" + recargaBateria + "% bateria. Total: " + PlayerData.Instance.bateriaActual);
    }
}
