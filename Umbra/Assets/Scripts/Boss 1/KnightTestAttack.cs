using System.Collections;
using UnityEngine;

// Ataque de teste, só para ver o ciclo funcionando: a arma sobe, espera e volta.
// Pode apagar depois que os ataques reais estiverem prontos.
public class KnightTestAttack : KnightAttack
{
    [Header("Teste")]
    [SerializeField] private float raiseHeight = 2f;
    [SerializeField] private float moveTime = 0.4f;
    [SerializeField] private float holdTime = 0.2f;

    public override IEnumerator Execute(KnightContext context)
    {
        Vector3 start = context.Weapon.transform.position;

        yield return context.Weapon.MoveTo(start + Vector3.up * raiseHeight, moveTime);
        yield return new WaitForSeconds(holdTime);
        yield return context.Weapon.MoveTo(start, moveTime);

        // Quem devolve a arma ao boss é o cérebro (weapon.Follow()), aqui não precisa
    }
}