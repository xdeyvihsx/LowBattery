using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PlayerDamage : MonoBehaviour
{
    [Header("Dano segun GDD")]
    public float  danoBateria    = 2f;
    public float  cooldown       = 1.5f;
    public string tipoObstaculo  = NotificationSoundManager.TIPO_WHATSAPP;

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
        if (timerCooldown > 0f || otro.gameObject.layer != layerPlayer) return;
        AplicarDano(otro.gameObject);
    }

    void OnTriggerStay2D(Collider2D otro)
    {
        if (timerCooldown > 0f || otro.gameObject.layer != layerPlayer) return;
        AplicarDano(otro.gameObject);
    }

    void AplicarDano(GameObject playerObj)
    {
        if (PlayerData.Instance == null) return;

        // Verificar escudo Modo Avion
        if (PowerUpManager.EscudoAvionActivo)
        {
            Debug.Log("[" + tipoObstaculo + "] BLOQUEADO por Modo Avion.");
            timerCooldown = cooldown;
            return;
        }

        // Aplicar dano a la bateria
        PlayerData.Instance.RecibirDano(danoBateria);

        // ── SFX de impacto al player ───────────────────────────
        // Suena desde el NotificationSoundManager (2D, global)
        if (NotificationSoundManager.Instance != null)
            NotificationSoundManager.Instance.PlayImpacto();

        // SFX legacy del PlayerSoundController si existe
        playerObj.GetComponent<PlayerSoundController>()?.PlayDamage();

        timerCooldown = cooldown;
        Debug.Log("[" + tipoObstaculo + "] -" + danoBateria + "% bateria.");
    }
}
