using UnityEngine;

// Adjunta a obstaculos LETALES (gato, cables).
// Al tocar al player lo mata instantaneamente.
[RequireComponent(typeof(Collider2D))]
public class Obstacle : MonoBehaviour
{
    void Awake()
    {
        // Forzar todos los colliders como trigger
        foreach (var col in GetComponents<Collider2D>())
            col.isTrigger = true;
    }
}
