using UnityEngine;
using UnityEngine.InputSystem;

public class TesteDanoBoss : MonoBehaviour
{
    [SerializeField] private BossHealth boss;
    [SerializeField] private Key damageKey = Key.K;
    [SerializeField, Min(1)] private int damage = 10;

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current[damageKey].wasPressedThisFrame)
            boss.TakeDamage(damage);
    }
}