using UnityEngine;

public class LightButton : MonoBehaviour
{
    [Header("Referências")]
    public SpriteRenderer buttonSprite;
    public DoorController connectedDoor;

    [Header("Aparência")]
    public Color inactiveColor = Color.white;
    public Color activeColor = Color.green;

    private bool isIlluminated;

    private void Start()
    {
        if (buttonSprite == null)
            buttonSprite = GetComponent<SpriteRenderer>();

        SetIlluminated(false);
    }

    public void SetIlluminated(bool illuminated)
    {
        if (isIlluminated == illuminated)
            return;

        isIlluminated = illuminated;

        if (buttonSprite != null)
        {
            buttonSprite.color = illuminated
                ? activeColor
                : inactiveColor;
        }

        if (connectedDoor != null)
        {
            connectedDoor.SetButtonIlluminated(illuminated);
        }
    }
}
