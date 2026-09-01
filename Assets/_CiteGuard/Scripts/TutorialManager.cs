using System.Collections;
using UnityEngine;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject welcomePanel;
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private GameObject gameModePanel;
    [SerializeField] private GameObject tutorialRobotDisplay;

    [Header("Tutorial Text")]
    [SerializeField] private TMP_Text stepCounterText;
    [SerializeField] private TMP_Text conceptTitleText;
    [SerializeField] private TMP_Text conceptBodyText;
    [SerializeField] private TMP_Text missionObjectiveText;
    [Header("Mission Objective Typewriter")]
    [SerializeField] private float missionObjectiveStartDelay = 0.3f;
    [SerializeField] private float missionObjectiveCharacterDelay = 0.035f;

    [Header("Typewriter Timing")]
    [SerializeField] private float titleCharacterDelay = 0.05f;
    [SerializeField] private float betweenTextsDelay = 0.2f;
    [SerializeField] private float bodyCharacterDelay = 0.018f;

    private int currentStep = 0;
    private Coroutine typingCoroutine;
    private bool isTyping;

    private string[] conceptTitles =
    {
        "User Query",
        "Evidence",
        "Retrieval",
        "Reranking",
        "LLM Generation",
        "Validation",
        "Grounding",
        "Hallucination"
    };

    private string[] conceptDescriptions =
    {
        "A user query is the question the AI must answer.\nIn this mission, it comes from a crew member who needs reliable information.",

        "Evidence consists of short, trusted facts that may help answer the crew member's question.",

        "Retrieval means selecting the evidence cards that are most relevant to the question.",

        "Reranking means ordering the selected evidence from the strongest match to the weakest match.",

        "The LLM uses the question and selected evidence to generate an answer for the crew.",

        "Validation checks whether the generated answer is accurate and supported by the selected evidence.",

        "An answer is grounded when its important claims are supported by the available evidence.",

        "A hallucination is an unsupported or invented claim. In a space mission, trusting one could put the crew at risk."
    };

    private void Start()
    {
        ShowWelcomeScreen();
    }

    public void StartTutorial()
    {
        StopTyping();

        currentStep = 0;

        welcomePanel.SetActive(false);
        gameModePanel.SetActive(false);
        tutorialPanel.SetActive(true);
        tutorialRobotDisplay.SetActive(true);

        ShowCurrentStep();
    }

    public void NextStep()
    {
        // The first click during typing completes the current text.
        if (isTyping)
        {
            CompleteCurrentText();
            return;
        }

        // A later click advances to the next tutorial step.
        if (currentStep < conceptTitles.Length - 1)
        {
            currentStep++;
            ShowCurrentStep();
        }
        else
        {
            OpenGameModeSelection();
        }
    }

    public void OpenGameModeSelection()
    {
        StopTyping();

        welcomePanel.SetActive(false);
        tutorialPanel.SetActive(false);
        gameModePanel.SetActive(true);
        tutorialRobotDisplay.SetActive(false);
        StartCoroutine(TypeMissionObjective());
    }

    private void ShowWelcomeScreen()
    {
        StopTyping();

        welcomePanel.SetActive(true);
        tutorialPanel.SetActive(false);
        gameModePanel.SetActive(false);
        tutorialRobotDisplay.SetActive(false);
    }

    private void ShowCurrentStep()
    {
        StopTyping();

        stepCounterText.text =
            (currentStep + 1).ToString("0") +
            " / " +
            conceptTitles.Length.ToString("0");

        conceptTitleText.text =
            conceptTitles[currentStep];

        conceptBodyText.text =
            conceptDescriptions[currentStep];

        conceptTitleText.maxVisibleCharacters = 0;
        conceptBodyText.maxVisibleCharacters = 0;

        typingCoroutine =
            StartCoroutine(TypeCurrentStep());
    }

    private IEnumerator TypeCurrentStep()
    {
        isTyping = true;

        conceptTitleText.ForceMeshUpdate();

        int titleCharacterCount =
            conceptTitleText.textInfo.characterCount;

        WaitForSeconds titleWait =
            new WaitForSeconds(
                Mathf.Max(0.01f, titleCharacterDelay)
            );

        for (
            int visibleCharacters = 1;
            visibleCharacters <= titleCharacterCount;
            visibleCharacters++
        )
        {
            conceptTitleText.maxVisibleCharacters =
                visibleCharacters;

            yield return titleWait;
        }

        yield return new WaitForSeconds(
            betweenTextsDelay
        );

        conceptBodyText.ForceMeshUpdate();

        int bodyCharacterCount =
            conceptBodyText.textInfo.characterCount;

        WaitForSeconds bodyWait =
            new WaitForSeconds(
                Mathf.Max(0.01f, bodyCharacterDelay)
            );

        for (
            int visibleCharacters = 1;
            visibleCharacters <= bodyCharacterCount;
            visibleCharacters++
        )
        {
            conceptBodyText.maxVisibleCharacters =
                visibleCharacters;

            yield return bodyWait;
        }

        isTyping = false;
        typingCoroutine = null;
    }

    private void CompleteCurrentText()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        conceptTitleText.maxVisibleCharacters =
            int.MaxValue;

        conceptBodyText.maxVisibleCharacters =
            int.MaxValue;

        isTyping = false;
    }

    private void StopTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        isTyping = false;
    }

    private IEnumerator TypeMissionObjective()
    {
        if (missionObjectiveText == null)
        {
            yield break;
        }

        missionObjectiveText.maxVisibleCharacters = 0;

        // Wait one frame so TextMeshPro can rebuild after the panel opens.
        yield return null;

        missionObjectiveText.ForceMeshUpdate();

        yield return new WaitForSeconds(
            missionObjectiveStartDelay
        );

        int totalCharacters =
            missionObjectiveText.textInfo.characterCount;

        WaitForSeconds characterWait =
            new WaitForSeconds(
                Mathf.Max(
                    0.01f,
                    missionObjectiveCharacterDelay
                )
            );

        for (
            int visibleCharacters = 1;
            visibleCharacters <= totalCharacters;
            visibleCharacters++
        )
        {
            missionObjectiveText.maxVisibleCharacters =
                visibleCharacters;

            yield return characterWait;
        }
    }
}