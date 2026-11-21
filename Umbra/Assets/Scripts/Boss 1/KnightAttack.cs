using System.Collections;
using UnityEngine;

// Base de todos os ataques do Knight. Cada ataque novo herda desta classe.
public abstract class KnightAttack : MonoBehaviour
{
    [Header("Ataque")]
    [SerializeField] private string attackName;
    [SerializeField] private bool isEnabled = true;
    [Tooltip("Pausa extra depois deste ataque, somada à pausa normal do cérebro.")]
    [SerializeField, Min(0f)] private float extraRecovery = 0f;

    public bool IsEnabled => isEnabled;
    public string AttackName => string.IsNullOrEmpty(attackName) ? GetType().Name : attackName;
    public float ExtraRecovery => extraRecovery;

    public void SetEnabled(bool value) => isEnabled = value;

    // Regra de liberação. O cérebro só sorteia ataques que respondem true aqui.
    // Padrão: ataques comuns só existem enquanto o boss ainda tem vida.
    // O ataque final sobrescreve para o contrário (só quando a vida zerou).
    public virtual bool CanUse(KnightContext context)
    {
        return context.Health == null || !context.Health.IsDepleted;
    }

    // true = depois deste ataque a luta acaba e o boss morre
    public virtual bool EndsBattleWhenFinished => false;

    // O ataque inteiro acontece aqui dentro. Quando a corrotina termina, o ataque acabou.
    public abstract IEnumerator Execute(KnightContext context);

    // Chamado pelo cérebro ao cancelar. Corrotinas que o ataque iniciou por conta própria
    // (StartCoroutine) não param sozinhas, então paramos aqui.
    // Ataques que ligam hitbox ou escondem o boss podem sobrescrever para desfazer.
    public virtual void Cancel()
    {
        StopAllCoroutines();
    }
}