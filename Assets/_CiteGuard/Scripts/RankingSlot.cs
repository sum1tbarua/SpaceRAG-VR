using UnityEngine;
using UnityEngine.EventSystems;

public class RankingSlot : MonoBehaviour, IDropHandler
{
    private DraggableEvidenceCard currentCard;

    public DraggableEvidenceCard GetCurrentCard()
    {
        return currentCard;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (
            eventData.pointerDrag == null
        )
        {
            return;
        }

        DraggableEvidenceCard droppedCard =
            eventData.pointerDrag
                .GetComponent<DraggableEvidenceCard>();

        if (droppedCard == null)
        {
            return;
        }

        RankingSlot previousSlot =
            droppedCard.GetComponentInParent<
                RankingSlot
            >();

        // The card was dropped back onto its
        // current slot. Nothing needs to change.
        if (previousSlot == this)
        {
            return;
        }

        // If this slot is empty, move the card normally.
        if (currentCard == null)
        {
            if (previousSlot != null)
            {
                previousSlot.ClearSlot(
                    droppedCard
                );
            }

            currentCard = droppedCard;

            PlaceCardInSlot(
                droppedCard,
                this
            );

            return;
        }

        // An occupied slot can only swap with a card
        // that already came from another ranking slot.
        if (previousSlot == null)
        {
            return;
        }

        DraggableEvidenceCard displacedCard =
            currentCard;

        // Put the dragged card into this slot.
        currentCard = droppedCard;

        PlaceCardInSlot(
            droppedCard,
            this
        );

        // Move the displaced card into the dragged
        // card's previous slot.
        previousSlot.currentCard =
            displacedCard;

        PlaceCardInSlot(
            displacedCard,
            previousSlot
        );
    }

    private void PlaceCardInSlot(
        DraggableEvidenceCard card,
        RankingSlot destinationSlot
    )
    {
        RectTransform cardRect =
            card.GetComponent<RectTransform>();

        card.transform.SetParent(
            destinationSlot.transform,
            false
        );

        cardRect.anchoredPosition =
            Vector2.zero;

        cardRect.localRotation =
            Quaternion.identity;

        cardRect.localScale =
            Vector3.one;

        card.ApplyRankingLayout();
    }

    public void ClearSlot(
        DraggableEvidenceCard card
    )
    {
        if (currentCard == card)
        {
            currentCard = null;
        }
    }
}