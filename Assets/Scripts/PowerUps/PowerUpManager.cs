using UnityEngine;
using System.Collections;

// ─────────────────────────────────────────────────────────────────
// PowerUpManager — Gestiona efectos activos de power-ups
//
// - Escudo de Modo Avion: bloquea dano de notificaciones
// - Efecto visual en el player (tinte de color + parpadeo)
// - Barra de tiempo restante opcional via HUD
// ─────────────────────────────────────────────────────────────────
public class PowerUpManager : MonoBehaviour
{
    public static PowerUpManager Instance { get; private set; }

    // ── Estado del escudo ──────────────────────────────────────
    public static bool EscudoAvionActivo { get; private set; } = false;
    public static float TiempoEscudoRestante { get; private set; } = 0f;

    private SpriteRenderer playerSR;
    private Color          colorOriginalPlayer;
    private Coroutine      corEscudo;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        // Buscar el SpriteRenderer del player para efectos visuales
        PlayerMovement mov = FindFirstObjectByType<PlayerMovement>();
        if (mov != null)
        {
            playerSR = mov.GetComponentInChildren<SpriteRenderer>();
            if (playerSR != null)
                colorOriginalPlayer = playerSR.color;
        }
    }

    // ── API publica ────────────────────────────────────────────

    public void ActivarEscudoAvion(float duracion, Color colorTinte)
    {
        // Cancelar escudo anterior si habia uno activo
        if (corEscudo != null) StopCoroutine(corEscudo);
        corEscudo = StartCoroutine(CoroutineEscudo(duracion, colorTinte));
        Debug.Log("[PowerUpManager] Escudo Modo Avion activado: " + duracion + " seg");
    }

    IEnumerator CoroutineEscudo(float duracion, Color colorTinte)
    {
        EscudoAvionActivo    = true;
        TiempoEscudoRestante = duracion;

        // Aplicar tinte azul al player
        if (playerSR != null)
            playerSR.color = colorTinte;

        float tiempoTranscurrido = 0f;

        while (tiempoTranscurrido < duracion)
        {
            tiempoTranscurrido   += Time.deltaTime;
            TiempoEscudoRestante  = duracion - tiempoTranscurrido;

            // En los ultimos 0.8 segundos hacer parpadeo de aviso
            if (TiempoEscudoRestante <= 0.8f && playerSR != null)
            {
                float parpadeo = Mathf.PingPong(Time.time * 8f, 1f);
                Color c = colorTinte;
                c.a = Mathf.Lerp(0.3f, 1f, parpadeo);
                playerSR.color = c;
            }

            yield return null;
        }

        // Desactivar escudo
        EscudoAvionActivo    = false;
        TiempoEscudoRestante = 0f;

        // Restaurar color original del player
        if (playerSR != null)
            playerSR.color = colorOriginalPlayer;

        Debug.Log("[PowerUpManager] Escudo Modo Avion desactivado.");
    }

    // Llamado cuando el player muere para limpiar efectos activos
    public void LimpiarEfectos()
    {
        if (corEscudo != null) StopCoroutine(corEscudo);
        EscudoAvionActivo    = false;
        TiempoEscudoRestante = 0f;
        if (playerSR != null) playerSR.color = colorOriginalPlayer;
    }
}
