using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Reports whether a UI button is being held down.
/// Unity's Button only raises onClick on release, which makes a movement
/// button feel unresponsive: the player has to tap over and over instead of
/// holding a direction.
/// </summary>
public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    /// <summary>True while a finger or the mouse is pressing this button.</summary>
    public bool IsHeld { get; private set; }

    public void OnPointerDown(PointerEventData eventData)
    {
        IsHeld = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        IsHeld = false;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Sliding a finger off the button should stop the movement, otherwise
        // the release lands somewhere else and the button stays stuck on.
        IsHeld = false;
    }

    private void OnDisable()
    {
        IsHeld = false;
    }
}
