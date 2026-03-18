using UnityEngine;
using System.Collections;

public class PlayerDeath : MonoBehaviour
{
    [Header("Tiempos")]
    public float duracionAnimMuerte = 1.5f;
    public float duracionInvencible = 1.5f;

    [Header("Efecto opcional")]
    public GameObject efectoMuerte;

    // Contador de muertes
    public static int TotalMuertes { get; private set; } = 0;

    // Eventos
    public System.Action OnPlayerMurio;
    public System.Action OnRespawn;       // ← PowerUps se suscriben aqui

    private bool    estaMuerto   = false;
    private bool    esInvencible = false;
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
        TotalMuertes   = 0;

        if (playerData != null)
            playerData.OnBateriaVacia += TriggerMuertePorBateria;
    }

    void OnDestroy()
    {
        if (playerData != null)
            playerData.OnBateriaVacia -= TriggerMuertePorBateria;
    }

    void OnTriggerEnter2D(Collider2D otro)
    {
        if (estaMuerto || esInvencible) return;
        if (otro.gameObject.layer == layerObstacles) IniciarMuerte();
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (estaMuerto || esInvencible) return;
        if (col.gameObject.layer == layerObstacles) IniciarMuerte();
    }

    void TriggerMuertePorBateria()
    {
        TotalMuertes++;
        Debug.Log("[PlayerDeath] Muerte por bateria. Total: " + TotalMuertes);

        if (estaMuerto) return;

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

    IEnumerator CoroutineMuerte(bool contarMuerte)
    {
        estaMuerto = true;

        if (contarMuerte)
        {
            TotalMuertes++;
            Debug.Log("[PlayerDeath] Muerte por obstaculo. Total: " + TotalMuertes);
        }

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

        if (playerData != null) playerData.Resetear();
        if (movimiento  != null) movimiento.ActivarRespawn();

        // Limpiar efectos de power-ups activos
        PowerUpManager.Instance?.LimpiarEfectos();

        estaMuerto = false;

        // Notificar a todos los power-ups para que se reactiven
        OnRespawn?.Invoke();
        Debug.Log("[PlayerDeath] Respawn — Power-ups reiniciados.");

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
