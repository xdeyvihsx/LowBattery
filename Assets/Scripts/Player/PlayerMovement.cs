using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidadMovimiento = 2.8f;

    [Header("Salto")]
    public float fuerzaSalto     = 5.5f;
    public float doubleJumpForce = 8f;
    public bool  dobleJumpActivo = false;

    [Header("Deteccion de Suelo")]
    public Transform controladorSuelo;
    public Vector2   dimensionesCaja = new Vector2(0.5f, 0.28f);
    public LayerMask capasSalto;

    [Header("Audio")]
    public PlayerSoundController playerSoundController;

    // Estado — PlayerDeath lo lee y escribe
    [HideInInspector] public bool estaMuerto = false;

    // Referencias privadas
    private Rigidbody2D rb2D;
    private Animator    animator;
    private bool        enSuelo;
    private bool        estabaEnSuelo;
    private bool        estabaMoviendo;

    // Hashes Animator
    private static readonly int pEnSuelo  = Animator.StringToHash("EnSuelo");
    private static readonly int pVelVert  = Animator.StringToHash("VelocidadVertical");
    private static readonly int pVelHoriz = Animator.StringToHash("VelocidadHorizontal");
    private static readonly int pMuerto   = Animator.StringToHash("Muerto");

    void Start()
    {
        rb2D     = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();

        if (playerSoundController == null)
            playerSoundController = GetComponent<PlayerSoundController>();

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
        ActualizarAudio();
    }

    void FixedUpdate()
    {
        if (estaMuerto) return;
        rb2D.linearVelocity = new Vector2(velocidadMovimiento, rb2D.linearVelocity.y);
    }

    void VerificarSuelo()
    {
        estabaEnSuelo = enSuelo;

        if (controladorSuelo != null)
            enSuelo = Physics2D.OverlapBox(controladorSuelo.position, dimensionesCaja, 0f, capasSalto);
        else
            enSuelo = Physics2D.Raycast(transform.position, Vector2.down, 0.65f, capasSalto);
    }

    void ManejarSalto()
    {
        bool salto = (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
                  || (Mouse.current   != null && Mouse.current.leftButton.wasPressedThisFrame);
        if (!salto) return;

        if (enSuelo)
        {
            Saltar(fuerzaSalto);
            playerSoundController?.PlayJump();
        }
        else if (dobleJumpActivo)
        {
            Saltar(doubleJumpForce);
            playerSoundController?.PlayJump();
        }
    }

    void Saltar(float f) =>
        rb2D.linearVelocity = new Vector2(rb2D.linearVelocity.x, f);

    void ActualizarAnimaciones()
    {
        if (animator == null || animator.runtimeAnimatorController == null) return;
        animator.SetBool (pEnSuelo,  enSuelo);
        animator.SetFloat(pVelHoriz, velocidadMovimiento);
        animator.SetFloat(pVelVert, -rb2D.linearVelocity.y);
    }

    void ActualizarAudio()
    {
        if (playerSoundController == null) return;

        // Notificar estado de suelo al audio (detecta aterrizajes)
        playerSoundController.ActualizarEstadoSuelo(enSuelo);

        // Sonido de correr: activo cuando esta en suelo y moviendose
        bool moviendose = Mathf.Abs(rb2D.linearVelocity.x) > 0.1f && enSuelo;
        if (moviendose != estabaMoviendo)
        {
            playerSoundController.SetCorrer(moviendose);
            estabaMoviendo = moviendose;
        }
    }

    // ── API para PlayerDeath ───────────────────────────────────────

    public void ActivarMuerte()
    {
        estaMuerto          = true;
        rb2D.linearVelocity = Vector2.zero;
        rb2D.simulated      = false;

        // Sonido de muerte
        playerSoundController?.PlayDeath();

        if (animator != null && animator.runtimeAnimatorController != null)
        {
            if (TieneParam(pMuerto))
                animator.SetBool(pMuerto, true);
            else
                animator.Play("Death");
        }
    }

    public void ActivarRespawn()
    {
        rb2D.simulated      = true;
        rb2D.linearVelocity = Vector2.zero;
        estaMuerto          = false;

        // Resetear audio y reproducir sonido de respawn
        playerSoundController?.ResetearAudio();
        playerSoundController?.PlayRespawn();

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
