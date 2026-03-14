using UnityEngine;

// Adjunta a objetos de dano (WhatsApp, Calls, Teams).
// Quita bateria sin matar al player.
// Usa cooldown para evitar dano continuo por permanencia.
[RequireComponent(typeof(Collider2D))]
public class PlayerDamage : MonoBehaviour
{
    [Header("Dano segun GDD")]
    [Tooltip("-2% WhatsApp / -5% Llamada / -10% Teams")]
    public float danoBateria = 2f;

    [Tooltip("Segundos entre cada golpe de dano (evita spam)")]
    public float cooldown = 1.5f;

    [Header("Debug")]
    public string tipoObstaculo = "Notificacion";

    private float timerCooldown = 0f;
    private int   layerPlayer;

    void Awake()
    {
        // Forzar trigger para que no bloquee fisicamente al player
        foreach (var col in GetComponents<Collider2D>())
            col.isTrigger = true;

        layerPlayer = LayerMask.NameToLayer("Player");
    }

    void Update()
    {
        if (timerCooldown > 0f)
            timerCooldown -= Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D otro)
    {
        if (timerCooldown > 0f) return;
        if (otro.gameObject.layer != layerPlayer) return;
        AplicarDano();
    }

    void OnTriggerStay2D(Collider2D otro)
    {
        if (timerCooldown > 0f) return;
        if (otro.gameObject.layer != layerPlayer) return;
        AplicarDano();
    }

    void AplicarDano()
    {
        if (PlayerData.Instance == null) return;
        PlayerData.Instance.RecibirDano(danoBateria);
        timerCooldown = cooldown;
        Debug.Log($"[{tipoObstaculo}] -{danoBateria}% bateria");
    }
}
