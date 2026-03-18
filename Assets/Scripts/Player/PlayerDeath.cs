using UnityEngine;
using System.Collections;

public class PlayerDeath : MonoBehaviour
{
    [Header("Tiempos")]
    public float duracionAnimMuerte = 1.5f;
    public float duracionInvencible = 1.5f;

    [Header("Efecto opcional")]
    public GameObject efectoMuerte;

    // ── Contador de muertes — static persiste entre respawns ──
    public static int TotalMuertes { get; private set; } = 0;

    // Evento para LevelAudioManager (reiniciar musica)
    public System.Action OnPlayerMurio;

    private bool estaMuerto   = false;
    private bool esInvencible = false;
    private Vector3 posicionSpawn;

    private PlayerMovement        movimiento;
    private PlayerData            playerData;
    private PlayerSoundController sonido;
    private int layerObstacles;

    void Start()
    {
        movimiento     = GetComponent<PlayerMovement>();
        playerData     = GetComponent<PlayerData>();
        sonido         = GetComponent<PlayerSoundController>();
        layerObstacles = LayerMask.NameToLayer("Obstacles");
        posicionSpawn  = transform.position;

        // Resetear contador al INICIAR el nivel (no en cada respawn)
        TotalMuertes = 0;

        if (playerData != null)
            playerData.OnBateriaVacia += TriggerMuertePorBateria;
    }

    void OnDestroy()
    {
        if (playerData != null)
            playerData.OnBateriaVacia -= TriggerMuertePorBateria;
    }

    // ── Colisiones con obstaculos letales ─────────────────────
    void OnTriggerEnter2D(Collider2D otro)
    {
        if (estaMuerto || esInvencible) return;
        if (otro.gameObject.layer == layerObstacles)
            IniciarMuerte();
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (estaMuerto || esInvencible) return;
        if (col.gameObject.layer == layerObstacles)
            IniciarMuerte();
    }

    // ── Muerte por bateria = 0 ────────────────────────────────
    // Se llama desde PlayerData.OnBateriaVacia
    // NO tiene guard de estaMuerto/esInvencible porque la bateria
    // puede llegar a 0 en cualquier momento, incluso durante invencibilidad
    void TriggerMuertePorBateria()
    {
        // Contar SIEMPRE la muerte por bateria, sin importar el estado
        TotalMuertes++;
        Debug.Log("[PlayerDeath] Muerte por bateria. Total: " + TotalMuertes);

        // Si ya esta en proceso de morir, no lanzar otra coroutine
        if (estaMuerto) return;

        // Si esta en invencibilidad, cancelarla y morir igual
        if (esInvencible)
        {
            StopAllCoroutines();
            esInvencible = false;
        }

        StartCoroutine(CoroutineMuerte(contarMuerte: false));
    }

    void IniciarMuerte()
    {
        if (estaMuerto) return;
        StartCoroutine(CoroutineMuerte(contarMuerte: true));
    }

    // contarMuerte = true  → muerte por obstaculo (se cuenta aqui)
    // contarMuerte = false → muerte por bateria (ya se conto en TriggerMuertePorBateria)
    IEnumerator CoroutineMuerte(bool contarMuerte)
    {
        estaMuerto = true;

        // Contar solo si es muerte por obstaculo
        if (contarMuerte)
        {
            TotalMuertes++;
            Debug.Log("[PlayerDeath] Muerte por obstaculo. Total: " + TotalMuertes);
        }

        // Notificar para reiniciar musica
        OnPlayerMurio?.Invoke();

        if (efectoMuerte != null)
            Instantiate(efectoMuerte, transform.position, Quaternion.identity);

        if (movimiento != null) movimiento.ActivarMuerte();
        if (playerData  != null) playerData.SetPausado(true);

        yield return new WaitForSeconds(duracionAnimMuerte);

        Respawnear();
    }

    void Respawnear()
    {
        transform.position = posicionSpawn;
        if (playerData  != null) playerData.Resetear();
        if (movimiento  != null) movimiento.ActivarRespawn();
        estaMuerto = false;
        StartCoroutine(CoroutineInvencible());
    }

    IEnumerator CoroutineInvencible()
    {
        esInvencible = true;
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        float t = 0f;
        while (t < duracionInvencible)
        {
            if (sr != null) sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(0.15f);
            t += 0.15f;
        }
        if (sr != null) sr.enabled = true;
        esInvencible = false;
    }
}
