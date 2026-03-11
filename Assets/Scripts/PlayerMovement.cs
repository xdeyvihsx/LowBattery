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

    // Parametros exactos del AnimatorController (imagen del Animator)
    // BlendTree usa "VelocidadVertical" con threshold: Jump=-1, Drop=1
    // Transicion Run->AnimationVertical usa "EnSuelo" = false
    // Transicion AnimationVertical->Run usa "VelocidadHorizontal" < 0.1
    private static readonly int paramEnSuelo            = Animator.StringToHash("EnSuelo");
    private static readonly int paramVelocidadVertical  = Animator.StringToHash("VelocidadVertical");
    private static readonly int paramVelocidadHorizontal = Animator.StringToHash("VelocidadHorizontal");

    void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();

        // Animator esta en el hijo "Sprite"
        animator = GetComponentInChildren<Animator>();

        if (animator == null)
            Debug.LogWarning("[PlayerMovement] No se encontro Animator en los hijos.");

        // Auto-buscar FloorController hijo si no esta asignado en Inspector
        if (controladorSuelo == null)
        {
            Transform fc = transform.Find("FloorController");
            if (fc != null)
                controladorSuelo = fc;
            else
                Debug.LogWarning("[PlayerMovement] No se encontro FloorController como hijo del Player.");
        }

        if (capasSalto.value == 0)
            capasSalto = LayerMask.GetMask("Default");
    }

    void Update()
    {
        VerificarSuelo();
        ManejarSalto();
        ActualizarAnimaciones();
    }

    void FixedUpdate()
    {
        // Auto-movimiento horizontal (estilo Geometry Dash)
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

        // EnSuelo: activa/desactiva la transicion hacia AnimationVertical
        animator.SetBool(paramEnSuelo, enSuelo);

        // VelocidadHorizontal: para la transicion de regreso a Run
        animator.SetFloat(paramVelocidadHorizontal, velocidadMovimiento);

        // VelocidadVertical: decide entre Jump y Drop dentro del BlendTree
        // Tu BlendTree tiene Jump threshold=-1 y Drop threshold=1
        // Por eso invertimos el signo: subiendo (vel.y > 0) -> valor negativo -> Jump
        //                              cayendo  (vel.y < 0) -> valor positivo -> Drop
        float velVertical = -rb2D.linearVelocity.y;
        animator.SetFloat(paramVelocidadVertical, velVertical);
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
