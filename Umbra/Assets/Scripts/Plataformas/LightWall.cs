using System.Collections;
using UnityEngine;

public class LightWall : MonoBehaviour
{
    [Header("Referências")]
    public BoxCollider2D wallCollider;
    public SpriteRenderer wallSprite;

    [Header("Tempo de abertura")]
    public float openDuration = 5f;

    [Header("Aparência")]
    public Color normalColor = Color.white;
    public Color illuminatedColor = new Color(1f, 1f, 1f, 0.3f);

    private bool isIlluminated;
    private bool isOpen;

    private Coroutine closeCoroutine;

    private void Start()
    {
        if (wallCollider == null)
            wallCollider = GetComponent<BoxCollider2D>();

        if (wallSprite == null)
            wallSprite = GetComponent<SpriteRenderer>();

        // Começa com a parede fechada.
        isIlluminated = false;
        isOpen = false;

        ApplyWallState();
    }

    public void SetIlluminated(bool illuminated)
    {
        // Evita executar a mesma ação repetidamente.
        if (isIlluminated == illuminated)
            return;

        isIlluminated = illuminated;

        if (illuminated)
        {
            // Cancela o fechamento se a parede
            // voltar a receber iluminação.
            if (closeCoroutine != null)
            {
                StopCoroutine(closeCoroutine);
                closeCoroutine = null;
            }

            OpenWall();
        }
        else
        {
            // Começa a contagem para fechar a parede.
            if (closeCoroutine != null)
                StopCoroutine(closeCoroutine);

            closeCoroutine = StartCoroutine(CloseAfterDelay());
        }
    }

    private void OpenWall()
    {
        isOpen = true;
        ApplyWallState();
    }

    private IEnumerator CloseAfterDelay()
    {
        // Espera o tempo configurado no Inspector.
        yield return new WaitForSeconds(openDuration);

        CloseWall();

        closeCoroutine = null;
    }

    private void CloseWall()
    {
        isOpen = false;
        ApplyWallState();
    }

    private void ApplyWallState()
    {
        // Quando aberta, a parede não possui colisão.
        if (wallCollider != null)
            wallCollider.enabled = !isOpen;

        // Atualiza a aparência.
        if (wallSprite != null)
        {
            wallSprite.color = isOpen
                ? illuminatedColor
                : normalColor;
        }
    }

    private void OnDisable()
    {
        if (closeCoroutine != null)
        {
            StopCoroutine(closeCoroutine);
            closeCoroutine = null;
        }

        isOpen = false;
        isIlluminated = false;
    }
}
