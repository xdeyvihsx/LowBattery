using UnityEngine;
using UnityEngine.InputSystem;

// Controla el movimiento del player.
// Expone estaMuerto para que PlayerDeath lo controle.
public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidadMovimiento = 2.8f;

    [Header("Salto")]
    public float fuerzaSalto    = 5.5f;
    public float doubleJumpForce = 8f;
    public bool  dobleJumpActivo = false;

    [Header("Deteccion de Suelo")]
    public Transform controladorSuelo;
    public Vector2   dimensionesCaja = new Vector2(0.5f, 0.28f);
    public LayerMask capasSalto;

    // Estado — PlayerDeath lo lee y escribe
    [HideInInspector] public bool estaMuerto = false;

    // Referencias privadas
    private Rigidbody2D rb2D;
    private Animator    animator;
    private bool        enSuelo;
    private bool        tieneDobleJump;

    // Hashes de parametros Animator
    private static readonly int pEnSuelo   = Animator.StringToHash("EnSuelo");
    private static readonly int pVelVert   = Animator.StringToHash("VelocidadVertical");
    private static readonly int pVelHoriz  = Animator.StringToHash("VelocidadHorizontal");
    private static readonly int pMuerto    = Animator.StringToHash("Muerto");

    // ── Ciclo de vida ──────────────────────────────────────────
    void Start()
    {
        rb2D     = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();

        if (controladorSuelo == null)
        {
            Transform fc = transform.Find("FloorController");
            if (fc != null) controladorSuelo = fc;
        }
        if (capasSalto.value == 0)
            capasSalto = LayerMask.GetMask("Suelo");
    }

    void Update()
    {
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

    // ── Logica interna ─────────────────────────────────────────
    void VerificarSuelo()
    {
        if (controladorSuelo != null)
            enSuelo = Physics2D.OverlapBox(controladorSuelo.position, dimensionesCaja, 0f, capasSalto);
        else
            enSuelo = Physics2D.Raycast(transform.position, Vector2.down, 0.65f, capasSalto);

        if (enSuelo) tieneDobleJump = dobleJumpActivo;
    }

    void ManejarSalto()
    {
        bool salto = (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
                  || (Mouse.current   != null && Mouse.current.leftButton.wasPressedThisFrame);
        if (!salto) return;

        if (enSuelo)
            Saltar(fuerzaSalto);
        else if (tieneDobleJump)
        {
            Saltar(doubleJumpForce);
            tieneDobleJump = false;
        }
    }

    void Saltar(float f) =>
        rb2D.linearVelocity = new Vector2(rb2D.linearVelocity.x, f);

    void ActualizarAnimaciones()
    {
        if (animator == null || animator.runtimeAnimatorController == null) return;
        animator.SetBool (pEnSuelo,  enSuelo);
        animator.SetFloat(pVelHoriz, velocidadMovimiento);
        // Invertido: sube(vel.y>0)→negativo→Jump=-1 / cae(vel.y<0)→positivo→Drop=+1
        animator.SetFloat(pVelVert, -rb2D.linearVelocity.y);
    }

    // ── API publica para PlayerDeath ───────────────────────────

    /// Congela al player y dispara la animacion de Death.
    public void ActivarMuerte()
    {
        estaMuerto = true;
        rb2D.linearVelocity = Vector2.zero;
        rb2D.simulated      = false;

        if (animator != null && animator.runtimeAnimatorController != null)
        {
            if (TieneParam(pMuerto))
                animator.SetBool(pMuerto, true);
            else
                animator.Play("Death");
        }
    }

    /// Restaura al player despues del respawn.
    public void ActivarRespawn()
    {
        rb2D.simulated      = true;
        rb2D.linearVelocity = Vector2.zero;
        estaMuerto          = false;

        if (animator != null && animator.runtimeAnimatorController != null)
        {
            if (TieneParam(pMuerto))
                animator.SetBool(pMuerto, false);
            animator.Play("Run");
        }
    }

    private bool TieneParam(int hash)
    {
        if (animator == null) return false;
        foreach (var p in animator.parameters)
            if (p.nameHash == hash) return true;
        return false;
    }

    void OnDrawGizmosSelected()
    {
        if (controladorSuelo == null) return;
        Gizmos.color = enSuelo ? Color.green : Color.red;
        Gizmos.DrawWireCube(controladorSuelo.position, dimensionesCaja);
    }
}
