using UnityEngine;
using UnityEngine.UI;

public class RankingCompletionManager : MonoBehaviour
{
    [Header("Ranking Slots")]
    [SerializeField] private RankingSlot[] rankingSlots;

    [Header("Send Button")]
    [SerializeField] private Button sendToLlmButton;

    private void Update()
    {
        UpdateButtonState();
    }

    private void UpdateButtonState()
    {
        bool allSlotsFilled =
            rankingSlots != null &&
            rankingSlots.Length > 0;

        for (int i = 0; i < rankingSlots.Length; i++)
        {
            if (rankingSlots[i] == null ||
                rankingSlots[i].GetCurrentCard() == null)
            {
                allSlotsFilled = false;
                break;
            }
        }

        if (sendToLlmButton != null)
        {
            sendToLlmButton.interactable =
                allSlotsFilled;
        }
    }
}