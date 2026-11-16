using UnityEngine;

public class Arrow : MonoBehaviour
{
    [Header("Configuração")]
    public float lifetime = 5f;

    [Tooltip("Use -90 se o sprite original aponta para cima.")]
    public float spriteRotationOffset = -90f;

    private Rigidbody2D rb;
    private float damage;
    private bool launched;

    private void Awake()
    {
        // Obtém o Rigidbody2D da flecha.
        rb = GetComponent<Rigidbody2D>();
    }

    public void Launch(Vector2 direction, float speed, float arrowDamage)
    {
        // Guarda o dano e normaliza a direção.
        damage = arrowDamage;
        direction.Normalize();

        // Faz a flecha se movimentar na direção do alvo.
        rb.linearVelocity = direction * speed;

        // Calcula o ângulo da trajetória.
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Corrige a orientação original do sprite.
        transform.rotation = Quaternion.Euler(
            0f,
            0f,
            angle + spriteRotationOffset
        );

        launched = true;

        // Remove a flecha depois de algum tempo.
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Ignora colisões antes do lançamento.
        if (!launched)
            return;

        // Não causa dano aos inimigos.
        if (other.GetComponentInParent<EnemyBase>() != null)
            return;

        // Causa dano ao jogador.
        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();

        if (health != null)
        {
            health.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // Destrói a flecha ao acertar uma parede ou outro objeto.
        Destroy(gameObject);
    }
}
