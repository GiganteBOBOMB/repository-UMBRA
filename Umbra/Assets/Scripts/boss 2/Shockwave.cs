using UnityEngine;

// Onda que corre pelo chão. Vai em um prefab com sprite, Collider2D e BossHitbox (Start Active ligado).
[RequireComponent(typeof(BossHitbox))]
public class ShockWave : MonoBehaviour
{
    private int direction = 1;
    private float speed;

    // direction: 1 = direita, -1 = esquerda
    public void Launch(int direction, float speed, float lifetime)
    {
        this.direction = direction >= 0 ? 1 : -1;
        this.speed = speed;

        // Espelha o sprite conforme a direção
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * this.direction;
        transform.localScale = scale;

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.Translate(Vector3.right * direction * speed * Time.deltaTime, Space.World);
    }
}