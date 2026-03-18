using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections;

// ─────────────────────────────────────────────────────────────────
// ChargerTrigger — Se adjunta al cargador (cargador_white_0)
// Detecta cuando el Player lo toca y dispara la victoria
// ─────────────────────────────────────────────────────────────────
public class ChargerTrigger : MonoBehaviour
{
    [Header("Tag del player (debe coincidir con el Inspector)")]
    public string layerPlayer = "Player";

    private bool ganado = false;
    private int  layerPlayerInt;

    void Start()
    {
        layerPlayerInt = LayerMask.NameToLayer(layerPlayer);

        // Asegurarse de que el collider es trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D otro)
    {
        if (ganado) return;
        if (otro.gameObject.layer != layerPlayerInt) return;

        ganado = true;
        VictoryManager.Instance?.MostrarVictoria();
    }
}
