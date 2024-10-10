using UnityEngine;
using System.Collections;

public class Bandit : MonoBehaviour
{
    [SerializeField] float m_speed = 4.0f;
    [SerializeField] float m_jumpForce = 7.5f;
    [SerializeField] private bool isPlayerControlled = false;
    [SerializeField] float attackRange = 1.5f; // Rango de ataque
    [SerializeField] LayerMask banditLayer; // Capa para detectar otros Bandits
    [SerializeField] float maxHealth = 100f; // Vida máxima del bandido
    [SerializeField] float damagePerHit = 20f; // Daño que recibe por ataque
    [SerializeField] AudioClip attackSound; // Sonido de ataque
    [SerializeField] GameObject damageParticles; // Prefab de partículas para el daño

    private float currentHealth; // Vida actual del bandido
    private Animator m_animator;
    private Rigidbody2D m_body2d;
    private Sensor_Bandit m_groundSensor;
    private AudioSource m_audioSource; // Fuente de audio
    private bool m_grounded = false;
    private bool m_combatIdle = false;
    private bool m_isDead = false;

    void Start()
    {
        m_animator = GetComponent<Animator>();
        m_body2d = GetComponent<Rigidbody2D>();
        m_groundSensor = transform.Find("GroundSensor").GetComponent<Sensor_Bandit>();
        m_audioSource = GetComponent<AudioSource>();
        currentHealth = maxHealth; // Inicializar vida actual
    }

    void Update()
    {
        if (!isPlayerControlled) return;

        if (!m_grounded && m_groundSensor.State())
        {
            m_grounded = true;
            m_animator.SetBool("Grounded", m_grounded);
        }

        if (m_grounded && !m_groundSensor.State())
        {
            m_grounded = false;
            m_animator.SetBool("Grounded", m_grounded);
        }

        // -- Handle input and movement --
        float inputX = Input.GetAxis("Horizontal");

        // Swap direction of sprite depending on walk direction
        if (inputX > 0)
            transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
        else if (inputX < 0)
            transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

        // Move
        m_body2d.velocity = new Vector2(inputX * m_speed, m_body2d.velocity.y);

        // Set AirSpeed in animator
        m_animator.SetFloat("AirSpeed", m_body2d.velocity.y);

        // -- Handle Animations --
        // Death
        if (Input.GetKeyDown("e") && !m_isDead)
        {
            Die();
        }

        // Hurt
        else if (Input.GetKeyDown("q"))
            TakeDamage(damagePerHit);

        // Attack
        else if (Input.GetMouseButtonDown(0))
        {
            m_animator.SetTrigger("Attack");
            PlayAttackSound(); // Reproducir sonido de ataque

            // Verificar si hay otros bandits en rango para aplicar daño
            Collider2D[] hitBandits = Physics2D.OverlapCircleAll(transform.position, attackRange, banditLayer);
            foreach (Collider2D hit in hitBandits)
            {
                if (hit != null && hit.gameObject != gameObject)
                {
                    Bandit otherBandit = hit.GetComponent<Bandit>();
                    if (otherBandit != null)
                    {
                        otherBandit.TakeDamage(damagePerHit); // Aplicar daño al bandido enemigo
                    }
                }
            }
        }

        // Change between idle and combat idle
        else if (Input.GetKeyDown("f"))
            m_combatIdle = !m_combatIdle;

        // Jump
        else if (Input.GetKeyDown("space") && m_grounded)
        {
            m_animator.SetTrigger("Jump");
            m_grounded = false;
            m_animator.SetBool("Grounded", m_grounded);
            m_body2d.velocity = new Vector2(m_body2d.velocity.x, m_jumpForce);
            m_groundSensor.Disable(0.2f);
        }

        // Run
        else if (Mathf.Abs(inputX) > Mathf.Epsilon)
            m_animator.SetInteger("AnimState", 2);

        // Combat Idle
        else if (m_combatIdle)
            m_animator.SetInteger("AnimState", 1);

        // Idle
        else
            m_animator.SetInteger("AnimState", 0);
    }

    // Método para aplicar daño al bandido
    public void TakeDamage(float damage)
    {
        if (m_isDead) return; // Si ya está muerto, no toma más daño

        currentHealth -= damage; // Reducir la vida actual
        m_animator.SetTrigger("Hurt");
        Instantiate(damageParticles, transform.position, Quaternion.identity); // Instanciar partículas

        if (currentHealth <= 0)
        {
            Die(); // Ejecutar la animación de muerte si la vida es cero o menos
        }
    }

    // Método para manejar la muerte del bandido
    private void Die()
    {
        m_animator.SetTrigger("Death");
        m_isDead = true;
        m_body2d.velocity = Vector2.zero; // Detener al bandido
    }

    // Método para reproducir el sonido de ataque
    private void PlayAttackSound()
    {
        m_audioSource.PlayOneShot(attackSound);
    }

    // Para visualizar el rango de ataque en la escena
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
