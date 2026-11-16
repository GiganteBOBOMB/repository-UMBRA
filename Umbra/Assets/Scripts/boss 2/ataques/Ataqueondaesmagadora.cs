using UnityEngine;

// Herda todo o comportamento do Esmagamento e só acrescenta as ondas no impacto.
public class AtaqueOndaEsmagadora : AtaqueEsmagamento
{
    [Header("Onda")]
    [SerializeField] private ShockWave wavePrefab;
    [SerializeField] private float waveSpeed = 8f;
    [SerializeField] private float waveLifetime = 2.5f;

    protected override void OnImpact(BossHand hand, Vector3 impactPoint, AttackContext context)
    {
        base.OnImpact(hand, impactPoint, context);

        if (wavePrefab == null)
        {
            Debug.LogWarning($"[{AttackName}] Wave Prefab não atribuído.", this);
            return;
        }

        SpawnWave(impactPoint, -1); // esquerda
        SpawnWave(impactPoint, 1);  // direita
    }

    private void SpawnWave(Vector3 position, int direction)
    {
        ShockWave wave = Instantiate(wavePrefab, position, Quaternion.identity);
        wave.Launch(direction, waveSpeed, waveLifetime);
    }
}