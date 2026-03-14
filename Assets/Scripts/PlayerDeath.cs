using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerDeath : MonoBehaviour
{
    [Header("Respawn")]
    public float tiempoEsperaRespawn = 1.5f;

    [Header("Efecto visual al morir (opcional)")]
    public GameObject efectoMuerte;

    private bool estaMuerto = false;
    private PlayerMovement movimiento;
    private int layerObstacles;

    void Start()
    {
        movimiento     = GetComponent<PlayerMovement>();
        layerObstacles = LayerMask.NameToLayer("Obstacles");
    }

    // Colision con Trigger (obstaculos con isTrigger = true)
    void OnTriggerEnter2D(Collider2D otro)
    {
        if (estaMuerto) return;
        if (otro.gameObject.layer == layerObstacles)
            StartCoroutine(MorirYRespawnear());
    }

    // Colision fisica (obstaculos sin isTrigger)
    void OnCollisionEnter2D(Collision2D colision)
    {
        if (estaMuerto) return;
        if (colision.gameObject.layer == layerObstacles)
            StartCoroutine(MorirYRespawnear());
    }

    IEnumerator MorirYRespawnear()
    {
        estaMuerto = true;

        // 1. Efecto visual si esta asignado
        if (efectoMuerte != null)
            Instantiate(efectoMuerte, transform.position, Quaternion.identity);

        // 2. Delegar al PlayerMovement: detiene fisicas y lanza animacion Death
        if (movimiento != null)
            movimiento.ActivarMuerte();

        // 3. Esperar que la animacion de muerte se reproduzca
        yield return new WaitForSeconds(tiempoEsperaRespawn);

        // 4. Reiniciar la escena (estilo Geometry Dash)
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
