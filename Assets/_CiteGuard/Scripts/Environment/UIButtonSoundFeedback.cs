using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonSoundFeedback :
    MonoBehaviour,
    IPointerEnterHandler,
    IPointerDownHandler
{
    [SerializeField]
    private float hoverCooldown = 0.12f;

    private Button button;
    private float nextAllowedHoverTime;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void OnPointerEnter(
        PointerEventData eventData
    )
    {
        if (!button.interactable)
        {
            return;
        }

        if (Time.unscaledTime <
            nextAllowedHoverTime)
        {
            return;
        }

        nextAllowedHoverTime =
            Time.unscaledTime + hoverCooldown;

        UISoundManager.Instance?.PlayHover();
    }

    public void OnPointerDown(
        PointerEventData eventData
    )
    {
        if (!button.interactable)
        {
            return;
        }

        UISoundManager.Instance?.PlayClick();
    }
}