using UnityEngine;

// Tudo que um ataque do Knight pode precisar. O cérebro monta e entrega pronto,
// assim o ataque nunca precisa procurar nada na cena.
public struct KnightContext
{
    public KnightMotor Motor;       // movimento do boss
    public FloatingWeapon Weapon;   // a arma flutuante
    public BossHealth Health;       // vida
    public BossArena Arena;         // chão e bordas
    public Transform Target;        // normalmente o Player
}