using System.Collections;
using UnityEngine;

public abstract class BossAttack : MonoBehaviour
{
    [Header("Ataque")]
    [SerializeField] private string attackName;
    [SerializeField] private bool isEnabled = true;

    public bool IsEnabled => isEnabled;
    public string AttackName => string.IsNullOrEmpty(attackName) ? GetType().Name : attackName;

    // Se true, o controlador não alterna a mão depois deste ataque
    public virtual bool UsesBothHands => false;

    // Habilitar/desabilitar por código (ex.: liberar ataques novos em fases mais difíceis)
    public void SetEnabled(bool value) => isEnabled = value;

    // O ataque inteiro acontece aqui dentro. Quando a corrotina termina, o ataque acabou.
    public abstract IEnumerator Execute(AttackContext context);

    // Chamado pelo controlador ao cancelar. Corrotinas que o ataque iniciou por conta própria
    // (StartCoroutine) não param sozinhas quando o controlador para a dele, então paramos aqui.
    public virtual void Cancel()
    {
        StopAllCoroutines();
    }
}