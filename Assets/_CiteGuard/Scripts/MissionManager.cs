using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MissionManager : MonoBehaviour
{
    [Header("Random Mission Content")]
    [SerializeField] private MissionContentManager missionContentManager;
    [Header("Mission Panels")]
    [SerializeField] private GameObject missionBriefingPanel;
    [SerializeField] private GameObject missionGameplayPanel;
    [SerializeField] private GameObject gameplayMissionCrew;
    [SerializeField] private GameObject missionScreenPanel;

    [Header("Query Acknowledgement")]
    [SerializeField] private TMP_Text crewQueryText;
    [SerializeField] private CanvasGroup beginAnalysisButtonGroup;
    [SerializeField] private TMP_Text officerResponseText;
    [SerializeField] private GameObject beginAnalysisButton;
    [SerializeField] private GameObject evidenceCase;

    [Header("Retrieval Visualization")]
    [SerializeField] private TMP_Text retrievalStatusText;
    [SerializeField] private CanvasGroup retrievalStatusGroup;
    [SerializeField] private float encodingDuration = 1.2f;
    [SerializeField] private float vectorSearchDuration = 2.5f;
    [SerializeField] private float retrievedMessageDuration = 0.8f;
    [SerializeField] private float retrievalFadeDuration = 0.2f;

    [SerializeField] private float responseCharacterDelay = 0.04f;
    [SerializeField] private float responseHoldDuration = 1.0f;

    [Header("Query Introduction")]
    [SerializeField] private float queryStartDelay = 0.8f;
    [SerializeField] private float queryCharacterDelay = 0.035f;
    [SerializeField] private float analysisButtonFadeDuration = 0.4f;

    [Header("Gameplay Panels")]
    [SerializeField] private GameObject queryPanel;
    [SerializeField] private GameObject evidencePanel;
    [SerializeField] private GameObject llmLoadingPanel;
    [SerializeField] private GameObject llmPanel;
    [SerializeField] private GameObject validationPanel;

    [Header("Canvas Layout")]
    [SerializeField] private RectTransform missionCanvas;
    [SerializeField] private GameObject rerankingCanvas;
    [SerializeField] private GameObject rankingWorkspacePanel;
    [SerializeField] private GameObject validationReferencePanel;

    [Header("LLM Output")]
    [SerializeField] private RankingSlot[] rankingSlots;
    [SerializeField] private TMP_Text generatedAnswerText;

    [Header("Validation Reference")]
    [SerializeField] private TMP_Text referenceAnswerText;
    [SerializeField] private TMP_Text[] referenceRankTexts;

    [SerializeField] private TMP_Text loadingText;

    private readonly Vector3 centeredCanvasPosition =
        new Vector3(0f, 1.8f, 3.85f);

    private readonly Vector3 evidenceCanvasPosition =
        new Vector3(-1.8f, 1.8f, 3.85f);
    
    private Coroutine acknowledgementCoroutine;
    private bool isAcknowledgingQuery;
    private Coroutine evidenceRevealCoroutine;
    private Coroutine queryIntroCoroutine;
    private bool isRevealingEvidence;

    private readonly string[] officerResponses =
    {
        "On it!",
        "Looking into it!",
        "Give me a moment!",
        "Sure! One moment...",
        "Copy that!"
    };

    private void Start()
    {
        StartCoroutine(BeginMissionAfterInitialization());
    }

    private IEnumerator BeginMissionAfterInitialization()
    {
        // Allow MissionContentManager and other scene
        // components to complete their Start methods.
        yield return null;

        BeginMission();
    }

    public void BeginMission()
    {
        if (acknowledgementCoroutine != null)
        {
            StopCoroutine(acknowledgementCoroutine);
            acknowledgementCoroutine = null;
        }

        if (queryIntroCoroutine != null)
        {
            StopCoroutine(queryIntroCoroutine);
            queryIntroCoroutine = null;
        }

        isAcknowledgingQuery = false;
        isRevealingEvidence = false;

        missionCanvas.gameObject.SetActive(true);
        Image gameplayBackgroundImage =
            missionGameplayPanel.GetComponent<Image>();

        if (gameplayBackgroundImage != null)
        {
            gameplayBackgroundImage.enabled = false;
            gameplayBackgroundImage.raycastTarget = false;
        }

        officerResponseText.text = "";
        officerResponseText.maxVisibleCharacters =
            int.MaxValue;
        retrievalStatusText.text = "";
        retrievalStatusText.gameObject.SetActive(false);

        SetCanvasGroupState(
            retrievalStatusGroup,
            0f,
            false
        );

        // Hide the query and button before the first
        // rendered frame of the mission.
        crewQueryText.maxVisibleCharacters = 0;

        beginAnalysisButton.SetActive(true);

        SetCanvasGroupState(
            beginAnalysisButtonGroup,
            0f,
            false
        );

        evidenceCase.SetActive(false);
        gameplayMissionCrew.SetActive(true);

        missionCanvas.localPosition =
            centeredCanvasPosition;

        rerankingCanvas.SetActive(false);

        missionScreenPanel.SetActive(true);
        missionBriefingPanel.SetActive(false);
        missionGameplayPanel.SetActive(true);

        queryPanel.SetActive(true);
        evidencePanel.SetActive(false);
        llmLoadingPanel.SetActive(false);
        llmPanel.SetActive(false);
        validationPanel.SetActive(false);

        rankingWorkspacePanel.SetActive(true);
        validationReferencePanel.SetActive(false);

        queryIntroCoroutine =
            StartCoroutine(
                PlayQueryIntroduction()
            );
    }

    private IEnumerator PlayQueryIntroduction()
    {
        // Allow the robot to descend before the query begins.
        yield return new WaitForSecondsRealtime(
            queryStartDelay
        );

        crewQueryText.ForceMeshUpdate();

        int totalCharacters =
            crewQueryText.textInfo.characterCount;

        WaitForSecondsRealtime characterWait =
            new WaitForSecondsRealtime(
                Mathf.Max(
                    0.01f,
                    queryCharacterDelay
                )
            );

        for (
            int visibleCharacters = 1;
            visibleCharacters <= totalCharacters;
            visibleCharacters++
        )
        {
            crewQueryText.maxVisibleCharacters =
                visibleCharacters;

            yield return characterWait;
        }

        yield return new WaitForSecondsRealtime(0.2f);

        float elapsedTime = 0f;

        while (
            elapsedTime <
            analysisButtonFadeDuration
        )
        {
            elapsedTime += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(
                elapsedTime /
                Mathf.Max(
                    0.01f,
                    analysisButtonFadeDuration
                )
            );

            float smoothProgress =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    progress
                );

            SetCanvasGroupState(
                beginAnalysisButtonGroup,
                smoothProgress,
                false
            );

            yield return null;
        }

        SetCanvasGroupState(
            beginAnalysisButtonGroup,
            1f,
            true
        );

        queryIntroCoroutine = null;
    }

    public void BeginAnalysis()
    {
        if (isAcknowledgingQuery)
        {
            return;
        }

        if (queryIntroCoroutine != null)
        {
            StopCoroutine(queryIntroCoroutine);
            queryIntroCoroutine = null;
        }

        isAcknowledgingQuery = true;
        beginAnalysisButton.SetActive(false);

        acknowledgementCoroutine =
            StartCoroutine(
                PlayAcknowledgementSequence()
            );
    }

    private IEnumerator PlayAcknowledgementSequence()
    {
        int responseIndex = Random.Range(
            0,
            officerResponses.Length
        );

        officerResponseText.text =
            officerResponses[responseIndex];

        officerResponseText.maxVisibleCharacters = 0;
        officerResponseText.ForceMeshUpdate();

        int totalCharacters =
            officerResponseText.textInfo.characterCount;

        WaitForSecondsRealtime characterWait =
            new WaitForSecondsRealtime(
                Mathf.Max(
                    0.01f,
                    responseCharacterDelay
                )
            );

        for (
            int visibleCharacters = 1;
            visibleCharacters <= totalCharacters;
            visibleCharacters++
        )
        {
            officerResponseText.maxVisibleCharacters =
                visibleCharacters;

            yield return characterWait;
        }

        yield return new WaitForSecondsRealtime(
            responseHoldDuration
        );

        // Remove the acknowledgement and original query
        // before presenting the retrieval process.
        officerResponseText.text = "";
        crewQueryText.maxVisibleCharacters = 0;

        retrievalStatusText.gameObject.SetActive(true);

        yield return StartCoroutine(
            ShowRetrievalStage(
                "ENCODING CREW QUERY...",
                encodingDuration
            )
        );

        yield return StartCoroutine(
            ShowRetrievalStage(
                "SEARCHING VECTOR ARCHIVE...",
                vectorSearchDuration
            )
        );

        yield return StartCoroutine(
            ShowRetrievalStage(
                "RELEVANT EVIDENCE RETRIEVED",
                retrievedMessageDuration
            )
        );

        // Hide the final retrieval message.
        SetCanvasGroupState(
            retrievalStatusGroup,
            0f,
            false
        );

        retrievalStatusText.gameObject.SetActive(false);

        // The crew transmission ends after retrieval.
        gameplayMissionCrew.SetActive(false);

        // Hide the holographic mission interface.
        missionCanvas.gameObject.SetActive(false);

        // Reveal the physical evidence box on the table.
        evidenceCase.SetActive(true);

        acknowledgementCoroutine = null;
    }

    private IEnumerator ShowRetrievalStage(
        string statusMessage,
        float displayDuration
    )
    {
        retrievalStatusText.text = statusMessage;
        retrievalStatusText.maxVisibleCharacters =
            int.MaxValue;

        SetCanvasGroupState(
            retrievalStatusGroup,
            0f,
            false
        );

        float fadeElapsedTime = 0f;

        // Fade the status message in.
        while (
            fadeElapsedTime <
            retrievalFadeDuration
        )
        {
            fadeElapsedTime +=
                Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(
                fadeElapsedTime /
                Mathf.Max(
                    0.01f,
                    retrievalFadeDuration
                )
            );

            SetCanvasGroupState(
                retrievalStatusGroup,
                progress,
                false
            );

            yield return null;
        }

        SetCanvasGroupState(
            retrievalStatusGroup,
            1f,
            false
        );

        yield return new WaitForSecondsRealtime(
            displayDuration
        );

        fadeElapsedTime = 0f;

        // Fade the status message out.
        while (
            fadeElapsedTime <
            retrievalFadeDuration
        )
        {
            fadeElapsedTime +=
                Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(
                fadeElapsedTime /
                Mathf.Max(
                    0.01f,
                    retrievalFadeDuration
                )
            );

            SetCanvasGroupState(
                retrievalStatusGroup,
                1f - progress,
                false
            );

            yield return null;
        }

        SetCanvasGroupState(
            retrievalStatusGroup,
            0f,
            false
        );
    }

    public void OpenEvidenceBox()
    {
        if (isRevealingEvidence)
        {
            return;
        }

        isRevealingEvidence = true;

        evidenceRevealCoroutine =
            StartCoroutine(
                RevealEvidenceInterfaces()
            );
    }

    private IEnumerator RevealEvidenceInterfaces()
    {
        // Save the physical box's normal scale.
        Vector3 originalCaseScale =
            evidenceCase.transform.localScale;

        // Pulse the box briefly after selection.
        float pulseDuration = 0.30f;
        float pulseElapsedTime = 0f;

        while (pulseElapsedTime < pulseDuration)
        {
            pulseElapsedTime +=
                Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(
                pulseElapsedTime / pulseDuration
            );

            float pulseAmount =
                Mathf.Sin(progress * Mathf.PI) * 0.10f;

            evidenceCase.transform.localScale =
                originalCaseScale *
                (1f + pulseAmount);

            yield return null;
        }

        evidenceCase.transform.localScale =
            originalCaseScale;

        evidenceCase.SetActive(false);
        gameplayMissionCrew.SetActive(false);

        // Restore the complete mission interface.
        missionCanvas.gameObject.SetActive(true);

        missionScreenPanel.SetActive(false);
        missionBriefingPanel.SetActive(false);
        missionGameplayPanel.SetActive(true);

        missionCanvas.localPosition =
            evidenceCanvasPosition;

        rerankingCanvas.SetActive(true);

        rankingWorkspacePanel.SetActive(true);
        validationReferencePanel.SetActive(false);

        queryPanel.SetActive(false);
        evidencePanel.SetActive(true);
        llmLoadingPanel.SetActive(false);
        llmPanel.SetActive(false);
        validationPanel.SetActive(false);

        // Locate the Canvas Groups you just added.
        CanvasGroup evidenceGroup =
            evidencePanel.GetComponent<CanvasGroup>();

        CanvasGroup rankingGroup =
            rankingWorkspacePanel.GetComponent<CanvasGroup>();

        RectTransform evidenceRect =
            evidencePanel.GetComponent<RectTransform>();

        RectTransform rankingRect =
            rankingWorkspacePanel.GetComponent<RectTransform>();

        if (
            evidenceGroup == null ||
            rankingGroup == null ||
            evidenceRect == null ||
            rankingRect == null
        )
        {
            Debug.LogError(
                "Evidence reveal requires Canvas Group " +
                "components on EvidencePanel and " +
                "RankingWorkspacePanel."
            );

            isRevealingEvidence = false;
            evidenceRevealCoroutine = null;
            yield break;
        }

        Vector2 evidenceFinalPosition =
            evidenceRect.anchoredPosition;

        Vector2 rankingFinalPosition =
            rankingRect.anchoredPosition;

        Vector3 evidenceFinalScale =
            evidenceRect.localScale;

        Vector3 rankingFinalScale =
            rankingRect.localScale;

        float riseDistance = 120f;

        evidenceRect.anchoredPosition =
            evidenceFinalPosition +
            Vector2.down * riseDistance;

        rankingRect.anchoredPosition =
            rankingFinalPosition +
            Vector2.down * riseDistance;

        evidenceRect.localScale =
            evidenceFinalScale * 0.92f;

        rankingRect.localScale =
            rankingFinalScale * 0.92f;

        SetCanvasGroupState(
            evidenceGroup,
            0f,
            false
        );

        SetCanvasGroupState(
            rankingGroup,
            0f,
            false
        );

        // Wait one frame so the panels initialize hidden.
        yield return null;

        float revealDuration = 0.60f;
        float revealElapsedTime = 0f;

        while (revealElapsedTime < revealDuration)
        {
            revealElapsedTime +=
                Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(
                revealElapsedTime / revealDuration
            );

            float smoothProgress =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    progress
                );

            evidenceRect.anchoredPosition =
                Vector2.Lerp(
                    evidenceFinalPosition +
                    Vector2.down * riseDistance,
                    evidenceFinalPosition,
                    smoothProgress
                );

            rankingRect.anchoredPosition =
                Vector2.Lerp(
                    rankingFinalPosition +
                    Vector2.down * riseDistance,
                    rankingFinalPosition,
                    smoothProgress
                );

            evidenceRect.localScale =
                Vector3.Lerp(
                    evidenceFinalScale * 0.92f,
                    evidenceFinalScale,
                    smoothProgress
                );

            rankingRect.localScale =
                Vector3.Lerp(
                    rankingFinalScale * 0.92f,
                    rankingFinalScale,
                    smoothProgress
                );

            SetCanvasGroupState(
                evidenceGroup,
                smoothProgress,
                false
            );

            SetCanvasGroupState(
                rankingGroup,
                smoothProgress,
                false
            );

            yield return null;
        }

        evidenceRect.anchoredPosition =
            evidenceFinalPosition;

        rankingRect.anchoredPosition =
            rankingFinalPosition;

        evidenceRect.localScale =
            evidenceFinalScale;

        rankingRect.localScale =
            rankingFinalScale;

        SetCanvasGroupState(
            evidenceGroup,
            1f,
            true
        );

        SetCanvasGroupState(
            rankingGroup,
            1f,
            true
        );

        isRevealingEvidence = false;
        evidenceRevealCoroutine = null;
    }

    private void SetCanvasGroupState(
    CanvasGroup canvasGroup,
    float alpha,
    bool allowInteraction
    )
    {
        canvasGroup.alpha = alpha;
        canvasGroup.interactable =
            allowInteraction;
        canvasGroup.blocksRaycasts =
            allowInteraction;
    }

    public void SendToLLM()
    {
        StartCoroutine(GenerateAnswer());
    }

    private IEnumerator GenerateAnswer()
    {
        missionCanvas.localPosition = centeredCanvasPosition;
        rerankingCanvas.SetActive(false);

        queryPanel.SetActive(false);
        evidencePanel.SetActive(false);
        llmPanel.SetActive(false);
        validationPanel.SetActive(false);
        llmLoadingPanel.SetActive(true);

        loadingText.text =
            "ANALYZING RANKED EVIDENCE...";

        yield return new WaitForSeconds(3.0f);

        loadingText.text =
            "CHECKING SOURCE RELEVANCE...";

        yield return new WaitForSeconds(2.5f);

        loadingText.text =
            "CONSTRUCTING AI RESPONSE...";

        yield return new WaitForSeconds(1.8f);

        CreateGeneratedAnswer();

        llmLoadingPanel.SetActive(false);
        llmPanel.SetActive(true);
    }

    public void OpenValidation()
    {
        missionCanvas.localPosition = evidenceCanvasPosition;
        rerankingCanvas.SetActive(true);

        rankingWorkspacePanel.SetActive(false);
        validationReferencePanel.SetActive(true);

        queryPanel.SetActive(false);
        evidencePanel.SetActive(false);
        llmLoadingPanel.SetActive(false);
        llmPanel.SetActive(false);
        validationPanel.SetActive(true);

        referenceAnswerText.text = generatedAnswerText.text;
        UpdateReferenceEvidence();
    }

    private void CreateGeneratedAnswer()
    {
        EvidenceCard firstCard = GetRankedEvidenceCard(0);
        EvidenceCard secondCard = GetRankedEvidenceCard(1);

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
            generatedAnswerText.text =
                missionContentManager.GetGroundedAnswer();

            return;
        }

        bool hasKeyEvidence = false;

        for (int i = 0; i < Mathf.Min(2, rankingSlots.Length); i++)
        {
            EvidenceCard card = GetRankedEvidenceCard(i);

            if (card != null && card.IdealRank > 0)
            {
                hasKeyEvidence = true;
                break;
            }
        }

        if (hasKeyEvidence)
        {
            generatedAnswerText.text =
                missionContentManager.GetPartialAnswer();
        }
        else
        {
            generatedAnswerText.text =
                missionContentManager.GetRiskyAnswer();
        }
    }

    private EvidenceCard GetRankedEvidenceCard(int index)
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

    private void UpdateReferenceEvidence()
    {
        for (int i = 0; i < rankingSlots.Length; i++)
        {
            DraggableEvidenceCard draggableCard =
                rankingSlots[i].GetCurrentCard();

            if (draggableCard == null)
            {
                referenceRankTexts[i].text =
                    "RANK " + (i + 1) +
                    "\nNo evidence ranked";

                continue;
            }

            EvidenceCard evidenceCard =
                draggableCard.GetComponent<EvidenceCard>();

            if (evidenceCard == null)
            {
                referenceRankTexts[i].text =
                    "RANK " + (i + 1) +
                    "\nEvidence unavailable";

                continue;
            }

            referenceRankTexts[i].text =
                "RANK " + (i + 1) + "\n" +
                evidenceCard.DisplayHeader + ": " +
                evidenceCard.DisplayText;
        }
    }

    private string GetEvidenceSummary(string evidenceName)
    {
        if (evidenceName == "EvidenceCardA")
            return "Evidence A: Shield needs battery above 40%.";

        if (evidenceName == "EvidenceCardB")
            return "Evidence B: Low battery mode is below 25%.";

        if (evidenceName == "EvidenceCardC")
            return "Evidence C: Oxygen uses backup power.";

        if (evidenceName == "EvidenceCardD")
            return "Evidence D: Shield blocks radiation storms.";

        return "Unknown evidence";
    }

    private int FindEvidenceRank(string evidenceName)
    {
        for (int i = 0; i < rankingSlots.Length; i++)
        {
            DraggableEvidenceCard card =
                rankingSlots[i].GetCurrentCard();

            if (card != null && card.gameObject.name == evidenceName)
            {
                return i + 1;
            }
        }

        return 0;
    }

    private void ShowMissionBriefing()
    {
        missionCanvas.localPosition = centeredCanvasPosition;
        rerankingCanvas.SetActive(false);

        missionBriefingPanel.SetActive(true);
        missionGameplayPanel.SetActive(false);

        llmLoadingPanel.SetActive(false);
        llmPanel.SetActive(false);
        validationPanel.SetActive(false);

        rankingWorkspacePanel.SetActive(true);
        validationReferencePanel.SetActive(false);
    }
}