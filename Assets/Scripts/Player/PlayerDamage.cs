using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PlayerDamage : MonoBehaviour
{
    [Header("Dano segun GDD")]
    public float  danoBateria   = 2f;
    public float  cooldown      = 1.5f;
    public string tipoObstaculo = NotificationSoundManager.TIPO_WHATSAPP;

    private float       timerCooldown = 0f;
    private bool        golpeado      = false;
    private int         layerPlayer;
    private PlayerDeath playerDeath;

    void Awake()
    {
        foreach (var col in GetComponents<Collider2D>())
            col.isTrigger = true;
        layerPlayer = LayerMask.NameToLayer("Player");
    }

    void Start()
    {
        playerDeath = FindFirstObjectByType<PlayerDeath>();
        if (playerDeath != null)
            playerDeath.OnRespawn += AlRespawn;
        else
            Debug.LogWarning("[PlayerDamage] PlayerDeath no encontrado: " + gameObject.name);
    }

    void OnDestroy()
    {
        if (playerDeath != null) playerDeath.OnRespawn -= AlRespawn;
    }

    void Update()
    {
        if (timerCooldown > 0f) timerCooldown -= Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D otro)
    {
        if (golpeado || timerCooldown > 0f) return;
        if (otro.gameObject.layer != layerPlayer) return;
        AplicarDano(otro.gameObject);
    }

    void OnTriggerStay2D(Collider2D otro)
    {
        if (golpeado || timerCooldown > 0f) return;
        if (otro.gameObject.layer != layerPlayer) return;
        AplicarDano(otro.gameObject);
    }

    void AplicarDano(GameObject playerObj)
    {
        if (PlayerData.Instance == null) return;

        // Escudo Modo Avion — bloquea dano, notificacion NO desaparece
        if (PowerUpManager.EscudoAvionActivo)
        {
            timerCooldown = cooldown;
            Debug.Log("[" + tipoObstaculo + "] BLOQUEADO por Modo Avion.");
            return;
        }

        // 1. Dano a la bateria
        PlayerData.Instance.RecibirDano(danoBateria);

        // 2. Flash rojo — usar el Singleton directamente (esta en el GO "Sprite")
        PlayerHitFlash flash = PlayerHitFlash.Instance;
        if (flash != null)
            flash.Flash();
        else
            Debug.LogWarning("[PlayerDamage] PlayerHitFlash.Instance es null. "
                + "Asegurate de adjuntar PlayerHitFlash al GameObject 'Sprite'.");

        // 3. SFX de impacto
        if (NotificationSoundManager.Instance != null)
            NotificationSoundManager.Instance.PlayImpacto();
        else
            Debug.LogWarning("[PlayerDamage] NotificationSoundManager.Instance es null.");

        // 4. Notificacion desaparece instantaneamente
        golpeado      = true;
        timerCooldown = cooldown;
        gameObject.SetActive(false);

        Debug.Log("[" + tipoObstaculo + "] -" + danoBateria + "% | desaparecida.");
    }

    void AlRespawn()
    {
        golpeado      = false;
        timerCooldown = 0f;
        gameObject.SetActive(true);
    }
}
