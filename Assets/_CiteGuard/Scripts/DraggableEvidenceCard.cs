using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class DraggableEvidenceCard : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    [Header("Evidence Text")]
    [SerializeField] private TMP_Text cardHeaderText;
    [SerializeField] private TMP_Text cardBodyText;

    private RectTransform cardRectTransform;
    private CanvasGroup canvasGroup;

    private Transform startingParent;
    private Vector3 startingWorldPosition;

    private Vector2 evidenceCardSize;

    private Vector2 evidenceHeaderPosition;
    private Vector2 evidenceHeaderSize;
    private float evidenceHeaderFontSize;
    private bool evidenceHeaderAutoSizing;

    private Vector2 evidenceBodyPosition;
    private Vector2 evidenceBodySize;
    private float evidenceBodyFontSize;
    private bool evidenceBodyAutoSizing;

    private void Awake()
    {
        cardRectTransform =
            GetComponent<RectTransform>();

        canvasGroup =
            GetComponent<CanvasGroup>();

        if (
            cardHeaderText == null ||
            cardBodyText == null
        )
        {
            Debug.LogError(
                gameObject.name +
                " requires CardHeaderText and " +
                "CardBodyText references."
            );

            return;
        }

        // Save the original evidence-panel layout.
        evidenceCardSize =
            cardRectTransform.sizeDelta;

        evidenceHeaderPosition =
            cardHeaderText.rectTransform.anchoredPosition;

        evidenceHeaderSize =
            cardHeaderText.rectTransform.sizeDelta;

        evidenceHeaderFontSize =
            cardHeaderText.fontSize;

        evidenceHeaderAutoSizing =
            cardHeaderText.enableAutoSizing;

        evidenceBodyPosition =
            cardBodyText.rectTransform.anchoredPosition;

        evidenceBodySize =
            cardBodyText.rectTransform.sizeDelta;

        evidenceBodyFontSize =
            cardBodyText.fontSize;

        evidenceBodyAutoSizing =
            cardBodyText.enableAutoSizing;
    }

    public void OnBeginDrag(
        PointerEventData eventData
    )
    {
        startingParent = transform.parent;

        startingWorldPosition =
            cardRectTransform.position;

        canvasGroup.blocksRaycasts = false;

        transform.SetAsLastSibling();
    }

    public void OnDrag(
        PointerEventData eventData
    )
    {
        Camera eventCamera =
            eventData.pressEventCamera;

        if (
            RectTransformUtility
                .ScreenPointToWorldPointInRectangle(
                    cardRectTransform,
                    eventData.position,
                    eventCamera,
                    out Vector3 worldPosition
                )
        )
        {
            cardRectTransform.position =
                worldPosition;
        }
    }

    public void OnEndDrag(
        PointerEventData eventData
    )
    {
        canvasGroup.blocksRaycasts = true;

        if (transform.parent == startingParent)
        {
            cardRectTransform.position =
                startingWorldPosition;
        }
    }

    public void ApplyRankingLayout()
    {
        if (
            cardHeaderText == null ||
            cardBodyText == null
        )
        {
            return;
        }

        cardRectTransform.sizeDelta =
            new Vector2(750f, 98f);

        RectTransform headerRect =
            cardHeaderText.rectTransform;

        headerRect.anchoredPosition =
            new Vector2(0f, 27f);

        headerRect.sizeDelta =
            new Vector2(700f, 32f);

        cardHeaderText.enableAutoSizing = true;
        cardHeaderText.fontSizeMin = 26f;
        cardHeaderText.fontSizeMax = 30f;
        cardHeaderText.color =
            new Color32(20, 28, 35, 255);

        cardHeaderText.outlineColor =
            new Color32(20, 28, 35, 255);

        cardHeaderText.outlineWidth = 0.04f;

        RectTransform bodyRect =
            cardBodyText.rectTransform;

        bodyRect.anchoredPosition =
            new Vector2(0f, -18f);

        bodyRect.sizeDelta =
            new Vector2(700f, 60f);

        cardBodyText.enableAutoSizing = true;
        cardBodyText.fontSizeMin = 20f;
        cardBodyText.fontSizeMax = 26f;
        cardBodyText.color =
            new Color32(30, 38, 45, 255);

        cardBodyText.outlineColor =
            new Color32(30, 38, 45, 255);

        cardBodyText.outlineWidth = 0.035f;

        cardHeaderText.ForceMeshUpdate();
        cardBodyText.ForceMeshUpdate();
    }

    public void RestoreEvidenceLayout()
    {
        if (
            cardHeaderText == null ||
            cardBodyText == null
        )
        {
            return;
        }

        cardRectTransform.sizeDelta =
            evidenceCardSize;

        cardHeaderText.rectTransform
            .anchoredPosition =
            evidenceHeaderPosition;

        cardHeaderText.rectTransform.sizeDelta =
            evidenceHeaderSize;

        cardHeaderText.enableAutoSizing =
            evidenceHeaderAutoSizing;

        cardHeaderText.fontSize =
            evidenceHeaderFontSize;

        cardBodyText.rectTransform
            .anchoredPosition =
            evidenceBodyPosition;

        cardBodyText.rectTransform.sizeDelta =
            evidenceBodySize;

        cardBodyText.enableAutoSizing =
            evidenceBodyAutoSizing;

        cardBodyText.fontSize =
            evidenceBodyFontSize;

        cardHeaderText.ForceMeshUpdate();
        cardBodyText.ForceMeshUpdate();
    }
}