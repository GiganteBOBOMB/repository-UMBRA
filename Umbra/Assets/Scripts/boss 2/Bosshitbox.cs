
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BossHitbox : MonoBehaviour
{
    [Header("Dano")]
    [SerializeField] private int damage = 1;
    [SerializeField] private string targetTag = "Player";

    [Header("Ativação")]
    [SerializeField] private bool startActive = false;

    private Collider2D area;
    private bool alreadyHit;

    private void Awake()
    {
        area = GetComponent<Collider2D>();

        area.isTrigger = true;
        area.enabled = startActive;
    }

    public void SetActive(bool value)
    {
        if (value)
            alreadyHit = false;

        area.enabled = value;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDamage(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryDamage(other);
    }

    private void TryDamage(Collider2D other)
    {
        if (alreadyHit || area == null || !area.enabled)
            return;

        // Procura o jogador, mesmo que o collider
        // esteja em um objeto filho.
        PlayerHealth playerHealth =
            other.GetComponentInParent<PlayerHealth>();

        if (playerHealth == null)
            return;

        // Confere a Tag do objeto principal do jogador.
        if (!playerHealth.CompareTag(targetTag))
            return;

        alreadyHit = true;

        playerHealth.TakeDamage(damage);

        Debug.Log(
            "[Boss] Acertou " + playerHealth.name +
            " causando " + damage + " de dano!"
        );
    }
}
