using UnityEngine;
using System.Collections;

// Gestiona la muerte y el respawn del player.
// Escucha dos fuentes de muerte:
//   1. Colision con Layer "Obstacles" (muerte instantanea)
//   2. PlayerData.OnBateriaVacia     (bateria llega a 0)
public class PlayerDeath : MonoBehaviour
{
    [Header("Tiempos")]
    public float duracionAnimMuerte = 1.5f;
    public float duracionInvencible = 2.5f;

    [Header("Efecto opcional")]
    public GameObject efectoMuerte;

    // Estado
    private bool estaMuerto   = false;
    private bool esInvencible = false;

    // Posicion de spawn guardada al inicio
    private Vector3 posicionSpawn;

    // Referencias — todas en el mismo GameObject
    private PlayerMovement movimiento;
    private PlayerData     playerData;
    private int            layerObstacles;

    // ── Ciclo de vida ──────────────────────────────────────────
    void Start()
    {
        movimiento     = GetComponent<PlayerMovement>();
        playerData     = GetComponent<PlayerData>();
        layerObstacles = LayerMask.NameToLayer("Obstacles");

        // Guardar posicion de inicio del nivel
        posicionSpawn = transform.position;

        // Suscribirse a muerte por bateria
        if (playerData != null)
            playerData.OnBateriaVacia += TriggerMuertePorBateria;
    }

    void OnDestroy()
    {
        if (playerData != null)
            playerData.OnBateriaVacia -= TriggerMuertePorBateria;
    }

    // ── Deteccion de colisiones letales ───────────────────────
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
    void TriggerMuertePorBateria() => IniciarMuerte();

    // ── Flujo principal ────────────────────────────────────────
    void IniciarMuerte()
    {
        if (estaMuerto) return;
        StartCoroutine(CoroutineMuerte());
    }

    IEnumerator CoroutineMuerte()
    {
        estaMuerto = true;

        // 1. Efecto visual opcional
        if (efectoMuerte != null)
            Instantiate(efectoMuerte, transform.position, Quaternion.identity);

        // 2. Congelar player + animacion Death
        if (movimiento != null)
            movimiento.ActivarMuerte();

        // 3. Pausar drenaje de bateria durante la muerte
        if (playerData != null)
            playerData.SetPausado(true);

        // 4. Esperar que termine la animacion de muerte
        yield return new WaitForSeconds(duracionAnimMuerte);

        // 5. Respawn
        Respawnear();
    }

    void Respawnear()
    {
        // Teleportar al punto de inicio del nivel
        transform.position = posicionSpawn;

        // Resetear bateria a 15 y reactivar drenaje
        if (playerData != null)
            playerData.Resetear();

        // Reactivar movimiento y animacion Run
        if (movimiento != null)
            movimiento.ActivarRespawn();

        // Limpiar estado de muerte
        estaMuerto = false;

        // Invencibilidad temporal para evitar bucle de muerte
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
