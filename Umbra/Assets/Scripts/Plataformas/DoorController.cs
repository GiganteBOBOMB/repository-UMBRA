using System.Collections;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Referências")]
    public BoxCollider2D doorCollider;
    public SpriteRenderer doorSprite;

    [Header("Tempo")]
    public float openDuration = 5f;

    [Header("Movimento")]
    public Vector3 openOffset = new Vector3(0f, 2f, 0f);
    public float movementDuration = 0.5f;

    [Header("Aparência")]
    public Color closedColor = Color.white;
    public Color openColor = Color.gray;

    private Vector3 closedPosition;
    private Vector3 openPosition;

    private Coroutine closeCoroutine;
    private Coroutine movementCoroutine;

    private bool isOpen;

    private void Start()
    {
        if (doorCollider == null)
            doorCollider = GetComponent<BoxCollider2D>();

        if (doorSprite == null)
            doorSprite = GetComponent<SpriteRenderer>();

        // Guarda a posição original da porta.
        closedPosition = transform.localPosition;

        // Calcula onde a porta ficará quando aberta.
        openPosition = closedPosition + openOffset;

        isOpen = false;

        if (doorCollider != null)
            doorCollider.enabled = true;

        if (doorSprite != null)
            doorSprite.color = closedColor;
    }

    // Recebe a informação do botão.
    public void SetButtonIlluminated(bool illuminated)
    {
        if (illuminated)
        {
            // Se o botão voltar a receber luz,
            // cancela o fechamento programado.
            if (closeCoroutine != null)
            {
                StopCoroutine(closeCoroutine);
                closeCoroutine = null;
            }

            OpenDoor();
        }
        else
        {
            // Começa a contagem para fechar a porta.
            if (closeCoroutine != null)
                StopCoroutine(closeCoroutine);

            closeCoroutine = StartCoroutine(CloseAfterDelay());
        }
    }

    private void OpenDoor()
    {
        if (isOpen)
            return;

        isOpen = true;

        // Permite que o jogador atravesse a porta.
        if (doorCollider != null)
            doorCollider.enabled = false;

        if (doorSprite != null)
            doorSprite.color = openColor;

        // Inicia o movimento visual.
        StartMovement(openPosition);
    }

    private IEnumerator CloseAfterDelay()
    {
        // Aguarda o tempo configurado no Inspector.
        yield return new WaitForSeconds(openDuration);

        CloseDoor();

        closeCoroutine = null;
    }

    private void CloseDoor()
    {
        if (!isOpen)
            return;

        isOpen = false;

        if (doorSprite != null)
            doorSprite.color = closedColor;

        // Retorna a porta à posição original.
        StartMovement(closedPosition);
    }

    private void StartMovement(Vector3 targetPosition)
    {
        if (movementCoroutine != null)
            StopCoroutine(movementCoroutine);

        movementCoroutine = StartCoroutine(
            MoveDoor(targetPosition)
        );
    }

    private IEnumerator MoveDoor(Vector3 targetPosition)
    {
        Vector3 startPosition = transform.localPosition;

        float elapsed = 0f;

        while (elapsed < movementDuration)
        {
            elapsed += Time.deltaTime;

            float progress = movementDuration > 0f
                ? Mathf.Clamp01(elapsed / movementDuration)
                : 1f;

            transform.localPosition = Vector3.Lerp(
                startPosition,
                targetPosition,
                progress
            );

            yield return null;
        }

        transform.localPosition = targetPosition;

        // Só volta a bloquear a passagem quando termina de fechar.
        if (!isOpen && doorCollider != null)
            doorCollider.enabled = true;

        movementCoroutine = null;
    }
}