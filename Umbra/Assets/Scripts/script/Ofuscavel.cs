using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Coloque em qualquer objeto ou inimigo que possa ficar "ofuscado".
/// Ofuscado: inimigo não toma dano / objeto sem colisor.
/// Revelado (depois de focar a lanterna): inimigo toma dano / objeto vira físico por X segundos.
/// </summary>
public class Ofuscavel : MonoBehaviour
{
    public enum Tipo { Objeto, Inimigo }

    // Lista estática: a lanterna não precisa de collider para achar os ofuscáveis
    // (objetos ofuscados têm o collider desligado, então o Physics2D não os encontraria).
    public static readonly List<Ofuscavel> Todos = new List<Ofuscavel>();

    [Header("Configuração")]
    public Tipo tipo = Tipo.Objeto;
    [Tooltip("Segundos de luz focada necessários para revelar")]
    public float tempoDeFoco = 1.5f;
    [Tooltip("Quanto tempo fica revelado antes de voltar a ofuscar")]
    public float duracaoRevelado = 6f;
    [Tooltip("Velocidade com que o foco se perde quando a luz sai de cima")]
    public float velocidadeDecaimento = 1f;

    [Header("Referências")]
    [Tooltip("Collider físico do objeto (usado só no tipo Objeto)")]
    public Collider2D colisorFisico;
    public SpriteRenderer render;

    [Header("Visual")]
    [Range(0f, 1f)] public float alphaOfuscado = 0.3f;

    public bool Ofuscado { get; private set; } = true;
    public bool PodeTomarDano => tipo == Tipo.Inimigo && !Ofuscado;
    public float ProgressoFoco => tempoDeFoco > 0f ? Mathf.Clamp01(focoAtual / tempoDeFoco) : 1f;

    private float focoAtual;
    private float timerRevelado;
    private bool iluminadoNesteFrame;

    private void OnEnable() => Todos.Add(this);
    private void OnDisable() => Todos.Remove(this);

    private void Start()
    {
        Ofuscar();
    }

    /// <summary>Chamado pela lanterna a cada frame em que a luz está em cima deste objeto.</summary>
    public void Iluminar(float deltaTime)
    {
        if (!Ofuscado) return;

        iluminadoNesteFrame = true;
        focoAtual += deltaTime;

        if (focoAtual >= tempoDeFoco)
            Revelar();
    }

    private void LateUpdate()
    {
        if (Ofuscado)
        {
            // Sem luz em cima: o foco vai se perdendo
            if (!iluminadoNesteFrame)
                focoAtual = Mathf.Max(0f, focoAtual - velocidadeDecaimento * Time.deltaTime);
        }
        else
        {
            timerRevelado -= Time.deltaTime;
            if (timerRevelado <= 0f)
                Ofuscar();
        }

        AtualizarVisual();
        iluminadoNesteFrame = false;
    }

    private void Revelar()
    {
        Ofuscado = false;
        timerRevelado = duracaoRevelado;

        if (tipo == Tipo.Objeto && colisorFisico != null)
            colisorFisico.enabled = true;
    }

    private void Ofuscar()
    {
        Ofuscado = true;
        focoAtual = 0f;

        if (tipo == Tipo.Objeto && colisorFisico != null)
            colisorFisico.enabled = false;
    }

    private void AtualizarVisual()
    {
        if (render == null) return;

        // Enquanto foca, o sprite vai ficando mais nítido; revelado = 100% visível
        float alpha = Ofuscado ? Mathf.Lerp(alphaOfuscado, 1f, ProgressoFoco) : 1f;
        Color c = render.color;
        c.a = alpha;
        render.color = c;
    }
}