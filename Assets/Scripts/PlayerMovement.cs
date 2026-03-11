using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidadMovimiento = 7f;

    [Header("Salto")]
    public float fuerzaSalto = 7f;
    public float doubleJumpForce = 6f;
    public bool dobleJumpActivo = false;

    [Header("Deteccion de Suelo")]
    public Transform controladorSuelo;
    public Vector2 dimensionesCaja = new Vector2(0.5f, 0.28f);
    public LayerMask capasSalto;

    // Referencias privadas
    private Rigidbody2D rb2D;
    private Animator animator;
    private bool enSuelo;
    private bool tieneDobleJump;
    private string animActual = "";

    void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();

        // Buscar Animator en el hijo "Sprite" donde esta el AnimatorController
        animator = GetComponentInChildren<Animator>();

        if (animator == null)
            Debug.LogWarning("[PlayerMovement] No se encontro Animator en los hijos. Asegurate de que el objeto Sprite tenga un Animator con AnimatorController asignado.");

        // Auto-buscar FloorController si no esta asignado
        if (controladorSuelo == null)
        {
            Transform fc = transform.Find("FloorController");
            if (fc != null)
                controladorSuelo = fc;
            else
                Debug.LogWarning("[PlayerMovement] No se encontro FloorController como hijo del Player.");
        }
    }

    void Update()
    {
        VerificarSuelo();
        ManejarSalto();
        ActualizarAnimaciones();
    }

    void FixedUpdate()
    {
        // Movimiento automatico hacia la derecha (estilo Geometry Dash)
        rb2D.linearVelocity = new Vector2(velocidadMovimiento, rb2D.linearVelocity.y);
    }

    void VerificarSuelo()
    {
        if (controladorSuelo != null)
        {
            enSuelo = Physics2D.OverlapBox(controladorSuelo.position, dimensionesCaja, 0f, capasSalto);
        }
        else
        {
            // Fallback con raycast si no hay FloorController
            enSuelo = Physics2D.Raycast(transform.position, Vector2.down, 0.65f, capasSalto);
        }

        // Recargar doble salto al tocar el suelo
        if (enSuelo)
            tieneDobleJump = dobleJumpActivo;
    }

    void ManejarSalto()
    {
        // New Input System — wasPressedThisFrame evita saltos por mantener la tecla
        bool saltoPresionado = (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
                            || (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame);

        if (!saltoPresionado) return;

        if (enSuelo)
        {
            // Salto normal: solo cuando esta pisando el suelo o una plataforma
            Saltar(fuerzaSalto);
        }
        else if (tieneDobleJump)
        {
            // Doble salto (disponible desde Nivel 2)
            Saltar(doubleJumpForce);
            tieneDobleJump = false;
        }
        // En el aire sin doble jump: el input se ignora completamente
    }

    void Saltar(float fuerza)
    {
        rb2D.linearVelocity = new Vector2(rb2D.linearVelocity.x, fuerza);
    }

    void ReproducirAnimacion(string nombreAnim)
    {
        // Evitar reiniciar la animacion si ya se esta reproduciendo
        if (animator == null || animActual == nombreAnim) return;

        // Verificar que el Animator tiene un controller valido antes de reproducir
        if (animator.runtimeAnimatorController == null)
        {
            Debug.LogWarning("[PlayerMovement] El Animator no tiene un AnimatorController asignado.");
            return;
        }

        animActual = nombreAnim;
        animator.Play(nombreAnim);
    }

    void ActualizarAnimaciones()
    {
        if (!enSuelo)
            ReproducirAnimacion("Jump");
        else
            ReproducirAnimacion("Run");
    }

    // Visualizar el area de deteccion de suelo en la Scene View
    void OnDrawGizmosSelected()
    {
        if (controladorSuelo != null)
        {
            Gizmos.color = enSuelo ? Color.green : Color.red;
            Gizmos.DrawWireCube(controladorSuelo.position, dimensionesCaja);
        }
    }
}
