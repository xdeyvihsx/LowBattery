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

    // Flag para bloquear todo cuando el player muere
    [HideInInspector] public bool estaMuerto = false;

    // Parametros del Animator
    private static readonly int paramEnSuelo             = Animator.StringToHash("EnSuelo");
    private static readonly int paramVelocidadVertical   = Animator.StringToHash("VelocidadVertical");
    private static readonly int paramVelocidadHorizontal = Animator.StringToHash("VelocidadHorizontal");
    private static readonly int paramMuerto              = Animator.StringToHash("Muerto");

    void Start()
    {
        rb2D     = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();

        if (animator == null)
            Debug.LogWarning("[PlayerMovement] No se encontro Animator en los hijos.");

        if (controladorSuelo == null)
        {
            Transform fc = transform.Find("FloorController");
            if (fc != null) controladorSuelo = fc;
        }

        if (capasSalto.value == 0)
            capasSalto = LayerMask.GetMask("Default");
    }

    void Update()
    {
        // Si el player esta muerto, no hacer nada
        if (estaMuerto) return;

        VerificarSuelo();
        ManejarSalto();
        ActualizarAnimaciones();
    }

    void FixedUpdate()
    {
        if (estaMuerto) return;
        rb2D.linearVelocity = new Vector2(velocidadMovimiento, rb2D.linearVelocity.y);
    }

    void VerificarSuelo()
    {
        if (controladorSuelo != null)
            enSuelo = Physics2D.OverlapBox(controladorSuelo.position, dimensionesCaja, 0f, capasSalto);
        else
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.65f, capasSalto);
            enSuelo = hit.collider != null;
        }

        if (enSuelo)
            tieneDobleJump = dobleJumpActivo;
    }

    void ManejarSalto()
    {
        bool saltoPresionado = (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
                            || (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame);

        if (!saltoPresionado) return;

        if (enSuelo)
            Saltar(fuerzaSalto);
        else if (tieneDobleJump)
        {
            Saltar(doubleJumpForce);
            tieneDobleJump = false;
        }
    }

    void Saltar(float fuerza)
    {
        rb2D.linearVelocity = new Vector2(rb2D.linearVelocity.x, fuerza);
    }

    void ActualizarAnimaciones()
    {
        if (animator == null || animator.runtimeAnimatorController == null) return;

        animator.SetBool(paramEnSuelo, enSuelo);
        animator.SetFloat(paramVelocidadHorizontal, velocidadMovimiento);

        // Invertido: subiendo(vel.y>0) -> negativo -> threshold Jump=-1
        //            cayendo(vel.y<0) -> positivo -> threshold Drop=+1
        animator.SetFloat(paramVelocidadVertical, -rb2D.linearVelocity.y);
    }

    // Llamado por PlayerDeath para activar la animacion de muerte
    public void ActivarMuerte()
    {
        estaMuerto = true;

        // Detener fisicas
        rb2D.linearVelocity = Vector2.zero;
        rb2D.simulated      = false;

        // Activar animacion de Death en el Animator
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            // Usar el parametro Bool "Muerto" si existe, o forzar Play directo
            // Intentamos SetBool primero; si no existe el param usamos Play como fallback
            bool tieneParam = TieneParametro(paramMuerto);
            if (tieneParam)
            {
                animator.SetBool(paramMuerto, true);
            }
            else
            {
                // Forzar reproduccion directa del estado Death
                animator.Play("Death");
            }
        }
    }

    // Verifica si el Animator tiene un parametro por hash
    private bool TieneParametro(int hash)
    {
        foreach (AnimatorControllerParameter p in animator.parameters)
            if (p.nameHash == hash) return true;
        return false;
    }

    void OnDrawGizmosSelected()
    {
        if (controladorSuelo != null)
        {
            Gizmos.color = enSuelo ? Color.green : Color.red;
            Gizmos.DrawWireCube(controladorSuelo.position, dimensionesCaja);
        }
    }
}
