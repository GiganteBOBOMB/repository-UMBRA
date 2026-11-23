using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class LightWall : MonoBehaviour
{
    [Min(0f)] public float openDuration = 5f;
    [Range(0f, 1f)] public float openAlpha = 0.2f;

    private Collider2D wallCollider;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isIlluminated;
    private Coroutine closeCoroutine;

    private void Awake()
    {
        wallCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
        CloseWall();
    }

    public void SetIlluminated(bool illuminated)
    {
        if (!isActiveAndEnabled || isIlluminated == illuminated)
            return;

        isIlluminated = illuminated;
        if (illuminated)
        {
            if (closeCoroutine != null)
            {
                StopCoroutine(closeCoroutine);
                closeCoroutine = null;
            }
            wallCollider.enabled = false;
            Color color = originalColor;
            color.a = openAlpha;
            spriteRenderer.color = color;
        }
        else
        {
            closeCoroutine = StartCoroutine(CloseAfterDelay());
        }
    }

    private IEnumerator CloseAfterDelay()
    {
        yield return new WaitForSeconds(openDuration);
        if (!isIlluminated)
            CloseWall();
        closeCoroutine = null;
    }

    private void CloseWall()
    {
        wallCollider.enabled = true;
        spriteRenderer.color = originalColor;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        closeCoroutine = null;
        isIlluminated = false;
        if (wallCollider != null && spriteRenderer != null)
            CloseWall();
    }
}
