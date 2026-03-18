using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PlayerDamage : MonoBehaviour
{
    [Header("Dano segun GDD")]
    public float danoBateria   = 2f;
    public float cooldown      = 1.5f;
    public string tipoObstaculo = "Notificacion";

    private float timerCooldown = 0f;
    private int   layerPlayer;

    void Awake()
    {
        foreach (var col in GetComponents<Collider2D>())
            col.isTrigger = true;
        layerPlayer = LayerMask.NameToLayer("Player");
    }

    void Update()
    {
        if (timerCooldown > 0f) timerCooldown -= Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D otro)
    {
        if (timerCooldown > 0f) return;
        if (otro.gameObject.layer != layerPlayer) return;
        AplicarDano(otro.gameObject);
    }

    void OnTriggerStay2D(Collider2D otro)
    {
        if (timerCooldown > 0f) return;
        if (otro.gameObject.layer != layerPlayer) return;
        AplicarDano(otro.gameObject);
    }

    void AplicarDano(GameObject playerObj)
    {
        if (PlayerData.Instance == null) return;

        // Verificar escudo — tanto el static como el Instance
        bool escudoActivo = PowerUpManager.EscudoAvionActivo;
        if (!escudoActivo && PowerUpManager.Instance != null)
            escudoActivo = PowerUpManager.EscudoAvionActivo;

        if (escudoActivo)
        {
            Debug.Log("[" + tipoObstaculo + "] BLOQUEADO por Modo Avion!");
            timerCooldown = cooldown;
            return;
        }

        PlayerData.Instance.RecibirDano(danoBateria);
        playerObj.GetComponent<PlayerSoundController>()?.PlayDamage();
        timerCooldown = cooldown;
        Debug.Log("[" + tipoObstaculo + "] -" + danoBateria + "%");
    }
}
