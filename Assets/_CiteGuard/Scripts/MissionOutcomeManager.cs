using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MissionOutcomeManager : MonoBehaviour
{
    [Header("Mission Content")]
    [SerializeField] private MissionContentManager missionContentManager;
    [Header("Main Panels")]
    [SerializeField] private GameObject llmPanel;
    [SerializeField] private GameObject validationPanel;
    [SerializeField] private GameObject missionOutcomePanel;

    [Header("Right-Side Interface")]
    [SerializeField] private GameObject rerankingCanvas;
    [SerializeField] private GameObject rankingWorkspacePanel;
    [SerializeField] private GameObject validationReferencePanel;

    [Header("Canvas Position")]
    [SerializeField] private RectTransform missionCanvas;
    [SerializeField] private Vector3 centeredPosition = new Vector3(0f, 1.8f, 3.85f);


    [Header("Outcome Text")]
    [SerializeField] private TMP_Text outcomeMessageText;
    [SerializeField] private TMP_Text consequenceMessageText;

    [SerializeField] private TMP_Text missionOutcomeHeadingText;

    [Header("Outcome Holograms")]
    [SerializeField] private Image missionStatusHologramImage;
    [SerializeField] private Sprite groundedHologram;
    [SerializeField] private Sprite partialHologram;
    [SerializeField] private Sprite riskyHologram;

    [Header("Outcome Buttons")]
    [SerializeField] private GameObject retryMissionButton;
    [SerializeField] private GameObject nextMissionButton;
    [SerializeField] private GameObject mainMenuButton;

    [Header("Final Summary")]
    [SerializeField] private GameObject finalSummaryPanel;
    [SerializeField] private TMP_Text finalSummaryText;

    [Header("Ranked Evidence")]
    [SerializeField] private RankingSlot[] rankingSlots;

    public void ProvideWithoutValidation()
    {
        ShowOutcome(false);
    }

    public void ProvideValidatedAnswer()
    {
        ShowOutcome(true);
    }

    public void RetryMission()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void NextMission()
    {
        bool sessionComplete =
            missionContentManager.MarkMissionCompleted();

        if (sessionComplete)
        {
            ShowFinalSummary();
            return;
        }

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name);
    }

    public void PlayAgain()
    {
        MissionContentManager.ResetSession();

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name);
    }

    private void ShowFinalSummary()
    {
        missionOutcomePanel.SetActive(false);
        finalSummaryPanel.SetActive(true);

        missionCanvas.position = centeredPosition;

        finalSummaryText.text =
            "Missions Completed: " +
            MissionContentManager.CompletedMissions +
            " / " +
            missionContentManager.MissionsPerSession +
            "\n\n" +

            "Grounded Answers: " +
            MissionContentManager.GroundedAnswers +
            "\n" +

            "Answers Validated: " +
            MissionContentManager.ValidatedAnswers +
            "\n" +

            "Validation Skipped: " +
            MissionContentManager.SkippedValidationAnswers +
            "\n\n" +

            "Average Trust Score: " +
            MissionContentManager.AverageTrustScore +
            "%";
    }

    public void ReturnToMainMenu()
    {
        MissionContentManager.ResetSession();
        SceneManager.LoadScene("00_MainMenu");
    }

    private void ShowOutcome(bool wasValidated)
    {
        int quality = EvaluateAnswerQuality();
        int trustScore =
            CalculateAttemptTrustScore(
                quality,
                wasValidated);

        missionContentManager.RecordAttempt(
            quality == 2,
            wasValidated,
            trustScore);

        llmPanel.SetActive(false);
        validationPanel.SetActive(false);
        rankingWorkspacePanel.SetActive(false);
        validationReferencePanel.SetActive(false);
        rerankingCanvas.SetActive(false);

        missionCanvas.position = centeredPosition;
        missionOutcomePanel.SetActive(true);

        if (quality == 2)
        {
            SetOutcomeHologram(groundedHologram);
            ShowGroundedOutcome(wasValidated);
        }
        else if (quality == 1)
        {
            SetOutcomeHologram(partialHologram);
            ShowPartialOutcome(wasValidated);
        }
        else
        {
            SetOutcomeHologram(riskyHologram);
            ShowRiskyOutcome(wasValidated);
        }

        MissionOutcomeAnimator outcomeAnimator =
            missionOutcomePanel.GetComponent<MissionOutcomeAnimator>();

        if (outcomeAnimator == null)
        {
            Debug.LogError(
                "MissionOutcomeAnimator is missing from MissionOutcomePanel.");

            return;
        }

        outcomeAnimator.Play();
    }

    private void SetOutcomeHologram(Sprite hologramSprite)
    {
        if (missionStatusHologramImage == null)
        {
            Debug.LogError(
                "Mission Status Hologram Image is not assigned.");

            return;
        }

        missionStatusHologramImage.sprite = hologramSprite;
        missionStatusHologramImage.color = Color.white;
        missionStatusHologramImage.preserveAspect = true;
    }

    private int CalculateAttemptTrustScore(
    int quality,
    bool wasValidated)
    {
        if (wasValidated)
        {
            return ValidationManager.LastFinalTrustScore;
        }

        if (quality == 2)
        {
            return 70;
        }

        if (quality == 1)
        {
            return 35;
        }

        return 5;
    }

    private int EvaluateAnswerQuality()
    {
        EvidenceCard firstCard = GetEvidenceCard(0);
        EvidenceCard secondCard = GetEvidenceCard(1);

        bool grounded =
            firstCard != null &&
            secondCard != null &&
            (
                (firstCard.IdealRank == 1 &&
                secondCard.IdealRank == 2) ||
                (firstCard.IdealRank == 2 &&
                secondCard.IdealRank == 1)
            );

        if (grounded)
        {
            return 2;
        }

        for (int i = 0; i < Mathf.Min(2, rankingSlots.Length); i++)
        {
            EvidenceCard card = GetEvidenceCard(i);

            if (card != null && card.IdealRank > 0)
            {
                return 1;
            }
        }

        return 0;
    }

    private EvidenceCard GetEvidenceCard(int index)
    {
        if (index < 0 || index >= rankingSlots.Length)
        {
            return null;
        }

        DraggableEvidenceCard draggedCard =
            rankingSlots[index].GetCurrentCard();

        if (draggedCard == null)
        {
            return null;
        }

        return draggedCard.GetComponent<EvidenceCard>();
    }

    
    
    private void SetOutcomeControls(
    string heading,
    bool showRetry,
    bool showNext)
    {
        missionOutcomeHeadingText.text = heading;

        retryMissionButton.SetActive(showRetry);
        nextMissionButton.SetActive(showNext);
        mainMenuButton.SetActive(true);
    }

    private void ShowGroundedOutcome(bool wasValidated)
    {
        SetOutcomeControls("MISSION SECURED", false, true);
        consequenceMessageText.color = new Color32(61, 220, 151, 255);

        if (wasValidated)
        {
            outcomeMessageText.text =
                "VALIDATED ANSWER PROVIDED\nThe crew receives a grounded response.";

            consequenceMessageText.text =
                missionContentManager.CurrentMission.successConsequence;
        }
        else
        {
            outcomeMessageText.text =
                "ANSWER PROVIDED WITHOUT VALIDATION\nThe answer happened to be grounded.";

            consequenceMessageText.text =
                missionContentManager.CurrentMission.successConsequence +
                "\n\nPROCESS WARNING: The correct answer was sent without validation.";
        }

    }

    private void ShowPartialOutcome(bool wasValidated)
    {
        SetOutcomeControls(
            wasValidated ? "MISSION PROTECTED" : "MISSION DELAYED",
            true,
            false);
        consequenceMessageText.color = new Color32(244, 201, 93, 255);

        if (wasValidated)
        {
            outcomeMessageText.text =
                "VALIDATION CAUGHT AN INCOMPLETE ANSWER\nThe response is held for more evidence.";

            consequenceMessageText.text =
                "MISSION PROTECTED: The crew delays the decision until the missing evidence is found.";
        }
        else
        {
            outcomeMessageText.text =
                "INCOMPLETE ANSWER PROVIDED\nThe crew cannot make a confident decision.";

            consequenceMessageText.text =
                missionContentManager.CurrentMission.delayedConsequence;
        }
    }

    private void ShowRiskyOutcome(bool wasValidated)
    {
        SetOutcomeControls(
            wasValidated ? "RISK BLOCKED" : "GAME OVER",
            true,
            false);
        consequenceMessageText.color = new Color32(255, 92, 92, 255);

        if (wasValidated)
        {
            outcomeMessageText.text =
                "VALIDATION CAUGHT A HALLUCINATION RISK\nThe unsafe response is blocked.";

            consequenceMessageText.text =
                "MISSION PROTECTED: Validation prevents the crew from acting on unsupported information.";
        }
        else
        {
            outcomeMessageText.text =
                "UNSUPPORTED ANSWER PROVIDED\nThe crew acts on weak evidence.";

            consequenceMessageText.text =
                missionContentManager.CurrentMission.failureConsequence;
        }
    }
}