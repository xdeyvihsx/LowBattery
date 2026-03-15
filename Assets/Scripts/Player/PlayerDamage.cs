using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PlayerDamage : MonoBehaviour
{
    [Header("Dano segun GDD")]
    [Tooltip("-2% WhatsApp / -5% Llamada / -10% Teams")]
    public float danoBateria = 2f;

    [Tooltip("Segundos de cooldown entre golpes")]
    public float cooldown = 1.5f;

    [Header("Debug")]
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
        if (timerCooldown > 0f)
            timerCooldown -= Time.deltaTime;
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

        // Bajar bateria
        PlayerData.Instance.RecibirDano(danoBateria);

        // Reproducir sonido de dano
        PlayerSoundController snd = playerObj.GetComponent<PlayerSoundController>();
        snd?.PlayDamage();

        timerCooldown = cooldown;
        Debug.Log($"[{tipoObstaculo}] -{danoBateria}% bateria");
    }
}
