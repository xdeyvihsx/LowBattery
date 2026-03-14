using UnityEngine;

// Adjunta este script a cada obstaculo.
// Fuerza el collider como Trigger en tiempo de ejecucion.
public class Obstacle : MonoBehaviour
{
    void Awake()
    {
        // Activar isTrigger en TODOS los colliders del objeto
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.isTrigger = true;
        }
    }
}
