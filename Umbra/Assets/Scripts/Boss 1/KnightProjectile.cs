using UnityEngine;

// Projétil do boss. Vai em um prefab com sprite, Collider2D (trigger), Rigidbody2D (Kinematic,
// Gravity Scale 0) e BossHitbox (com Start Active LIGADO).
[RequireComponent(typeof(BossHitbox))]
public class KnightProjectile : MonoBehaviour
{
    [Header("Colisão")]
    [SerializeField] private string targetTag = "Player";
    [Tooltip("Camadas que destroem o projétil ao tocar (chão, paredes). Vazio = só some no tempo ou no player.")]
    [SerializeField] private LayerMask destroyOnLayers;

    [Header("Visual")]
    [Tooltip("Ligado: gira o sprite para a direção do voo.")]
    [SerializeField] private bool rotateToDirection = true;
    [Tooltip("Para onde o sprite aponta sem rotação. 0 = direita, 90 = cima.")]
    [SerializeField] private float spriteAngle = 0f;

    private Vector2 direction;
    private float speed;

    public void Launch(Vector2 direction, float speed, float lifetime)
    {
        this.direction = direction.normalized;
        this.speed = speed;

        if (rotateToDirection)
        {
            float angle = Mathf.Atan2(this.direction.y, this.direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle - spriteAngle);
        }

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // O dano é do BossHitbox (no mesmo objeto). Aqui só decidimos quando o projétil some.
        bool hitTarget = other.CompareTag(targetTag);
        bool hitSolid = (destroyOnLayers.value & (1 << other.gameObject.layer)) != 0;

        if (hitTarget || hitSolid)
            Destroy(gameObject);
    }
}