using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MissionEntranceController : MonoBehaviour
{
    [Header("Current Screen")]
    [SerializeField] private GameObject missionLaunchPanel;
    [SerializeField] private GameObject welcomeShipDisplay;

    [Header("Entrance Screen")]
    [SerializeField] private GameObject missionEntrancePanel;
    [SerializeField] private GameObject commandCenterEntranceDisplay;
    [SerializeField] private GameObject missionCrewDisplay;

    [Header("Transmission")]
    [SerializeField] private TMP_Text transmissionTitle;
    [SerializeField] private TMP_Text crewDialogueText;
    [SerializeField] private CanvasGroup enterCommandCenterButton;
    [SerializeField] private CanvasGroup transmissionPanelGroup;

    [Header("Door Materials")]
    [SerializeField] private Renderer entranceRenderer;
    [SerializeField] private Material closedDoorMaterial;
    [SerializeField] private Material openDoorMaterial;

    [Header("Sequence Timing")]
    [SerializeField] private float crewArrivalDelay = 1f;
    [SerializeField] private float dialogueCharacterDelay = 0.025f;
    [SerializeField] private float buttonFadeDuration = 0.45f;
    [SerializeField] private float openDoorDisplayDuration = 1.4f;
    [SerializeField] private float transmissionFadeDuration = 0.4f;

    [Header("Gameplay Scene")]
    [SerializeField] private string gameplaySceneName =
        "01_SpaceMission";

    private Coroutine sequenceCoroutine;
    private bool entranceStarted;
    private bool doorOpening;

    private void Start()
    {
        PrepareInitialState();
    }

    private void PrepareInitialState()
    {
        missionEntrancePanel.SetActive(false);
        commandCenterEntranceDisplay.SetActive(false);
        missionCrewDisplay.SetActive(false);

        if (entranceRenderer != null &&
            closedDoorMaterial != null)
        {
            entranceRenderer.sharedMaterial =
                closedDoorMaterial;
        }

        HideTransmissionContent();
    }

    private void HideTransmissionContent()
    {
        if (transmissionPanelGroup != null)
        {
            transmissionPanelGroup.alpha = 0f;
            transmissionPanelGroup.interactable = false;
            transmissionPanelGroup.blocksRaycasts = false;
        }
        if (transmissionTitle != null)
        {
            transmissionTitle.maxVisibleCharacters = 0;
        }

        if (crewDialogueText != null)
        {
            crewDialogueText.maxVisibleCharacters = 0;
        }

        SetButtonState(0f, false);
    }

    public void BeginMissionEntrance()
    {
        if (entranceStarted)
        {
            return;
        }

        entranceStarted = true;

        missionLaunchPanel.SetActive(false);

        if (welcomeShipDisplay != null)
        {
            welcomeShipDisplay.SetActive(false);
        }

        commandCenterEntranceDisplay.SetActive(true);
        missionEntrancePanel.SetActive(true);
        missionCrewDisplay.SetActive(true);

        HideTransmissionContent();

        sequenceCoroutine =
            StartCoroutine(PlayTransmissionSequence());
    }

    private IEnumerator PlayTransmissionSequence()
    {
        // Wait for the robot to descend.
        yield return new WaitForSecondsRealtime(
            crewArrivalDelay
        );

        // Reveal the transmission panel afterward.
        yield return StartCoroutine(
            FadeInTransmissionPanel()
        );

        if (transmissionTitle != null)
        {
            transmissionTitle.maxVisibleCharacters =
                int.MaxValue;
        }

        yield return new WaitForSecondsRealtime(0.25f);

        yield return StartCoroutine(TypeDialogue());

        yield return new WaitForSecondsRealtime(0.2f);

        yield return StartCoroutine(FadeInEnterButton());

        sequenceCoroutine = null;
    }

    private IEnumerator FadeInTransmissionPanel()
    {
        if (transmissionPanelGroup == null)
        {
            yield break;
        }

        float elapsedTime = 0f;

        while (elapsedTime < transmissionFadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(
                elapsedTime /
                Mathf.Max(
                    0.01f,
                    transmissionFadeDuration
                )
            );

            transmissionPanelGroup.alpha =
                Mathf.SmoothStep(0f, 1f, progress);

            yield return null;
        }

        transmissionPanelGroup.alpha = 1f;
        transmissionPanelGroup.interactable = true;
        transmissionPanelGroup.blocksRaycasts = true;
    }

    private IEnumerator TypeDialogue()
    {
        if (crewDialogueText == null)
        {
            yield break;
        }

        crewDialogueText.ForceMeshUpdate();

        int totalCharacters =
            crewDialogueText.textInfo.characterCount;

        crewDialogueText.maxVisibleCharacters = 0;

        WaitForSecondsRealtime characterWait =
            new WaitForSecondsRealtime(
                Mathf.Max(
                    0.01f,
                    dialogueCharacterDelay
                )
            );

        for (
            int visibleCharacters = 1;
            visibleCharacters <= totalCharacters;
            visibleCharacters++
        )
        {
            crewDialogueText.maxVisibleCharacters =
                visibleCharacters;

            yield return characterWait;
        }
    }

    private IEnumerator FadeInEnterButton()
    {
        float elapsedTime = 0f;

        while (elapsedTime < buttonFadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(
                elapsedTime /
                Mathf.Max(0.01f, buttonFadeDuration)
            );

            float smoothProgress =
                Mathf.SmoothStep(0f, 1f, progress);

            SetButtonState(smoothProgress, false);

            yield return null;
        }

        SetButtonState(1f, true);
    }

    private void SetButtonState(
        float alpha,
        bool allowInteraction
    )
    {
        if (enterCommandCenterButton == null)
        {
            return;
        }

        enterCommandCenterButton.alpha = alpha;
        enterCommandCenterButton.interactable =
            allowInteraction;
        enterCommandCenterButton.blocksRaycasts =
            allowInteraction;
    }

    public void EnterCommandCenter()
    {
        if (doorOpening)
        {
            return;
        }

        doorOpening = true;

        if (sequenceCoroutine != null)
        {
            StopCoroutine(sequenceCoroutine);
            sequenceCoroutine = null;
        }

        SetButtonState(0f, false);

        missionEntrancePanel.SetActive(false);
        missionCrewDisplay.SetActive(false);

        if (entranceRenderer != null &&
            openDoorMaterial != null)
        {
            entranceRenderer.sharedMaterial =
                openDoorMaterial;
        }

        StartCoroutine(
            LoadGameplayAfterDoorOpens()
        );
    }

    private IEnumerator LoadGameplayAfterDoorOpens()
    {
        yield return new WaitForSecondsRealtime(
            openDoorDisplayDuration
        );

        SceneManager.LoadScene(gameplaySceneName);
    }
}