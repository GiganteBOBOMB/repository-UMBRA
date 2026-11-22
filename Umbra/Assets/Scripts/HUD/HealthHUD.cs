using UnityEngine;
using UnityEngine.UI;

public class HealthHUD : MonoBehaviour
{
    [Header("Barra de Vida")]
    public Image healthFill;

    [Header("Barra de Foco")]
    public Image focusFill;

    private void Awake()
    {
        if (healthFill != null)
        {
            healthFill.fillAmount = 1f;
        }

        if (focusFill != null)
        {
            focusFill.fillAmount = 1f;
        }
    }

    public void UpdateHealth(
        float currentHealth,
        float maxHealth)
    {
        if (healthFill == null || maxHealth <= 0f)
            return;

        healthFill.fillAmount =
            Mathf.Clamp01(currentHealth / maxHealth);
    }

    public void UpdateFocus(
        float currentFocus,
        float maxFocus)
    {
        if (focusFill == null || maxFocus <= 0f)
            return;

        focusFill.fillAmount =
            Mathf.Clamp01(currentFocus / maxFocus);
    }
}