using UnityEngine;

// Marca o cenário da luta: altura do chão e as duas bordas da arena.
// Fica em um objeto vazio "Arena" na cena, com três filhos vazios posicionados à mão.
public class BossArena : MonoBehaviour
{
    [SerializeField] private Transform groundPoint;
    [SerializeField] private Transform leftEdge;
    [SerializeField] private Transform rightEdge;

    public float GroundY => groundPoint.position.y;
    public float LeftX => leftEdge.position.x;
    public float RightX => rightEdge.position.x;
    public float CenterX => (LeftX + RightX) * 0.5f;

    // Desenha a arena na Scene View para facilitar o posicionamento
    private void OnDrawGizmos()
    {
        if (groundPoint == null || leftEdge == null || rightEdge == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(new Vector3(LeftX, GroundY, 0f), new Vector3(RightX, GroundY, 0f));
        Gizmos.DrawWireSphere(new Vector3(LeftX, GroundY, 0f), 0.25f);
        Gizmos.DrawWireSphere(new Vector3(RightX, GroundY, 0f), 0.25f);
    }
}
