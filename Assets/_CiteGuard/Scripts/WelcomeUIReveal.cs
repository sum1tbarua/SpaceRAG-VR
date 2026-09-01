using System.Collections;
using TMPro;
using UnityEngine;

public class WelcomeUIReveal : MonoBehaviour
{
    [Header("Text Elements")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text subtitleText;

    [Header("Button Groups")]
    [SerializeField] private CanvasGroup startTrainingButton;
    [SerializeField] private CanvasGroup enterMissionButton;

    [Header("Typewriter Timing")]
    [SerializeField] private float startDelay = 0.5f;
    [SerializeField] private float titleCharacterDelay = 0.08f;
    [SerializeField] private float betweenTextsDelay = 0.25f;
    [SerializeField] private float subtitleCharacterDelay = 0.035f;

    [Header("Button Reveal")]
    [SerializeField] private float buttonDelay = 0.25f;
    [SerializeField] private float buttonFadeDuration = 0.5f;

    private void OnEnable()
    {
        StopAllCoroutines();
        HideWelcomeContent();
        StartCoroutine(PlayWelcomeSequence());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private void HideWelcomeContent()
    {
        if (titleText != null)
        {
            titleText.maxVisibleCharacters = 0;
        }

        if (subtitleText != null)
        {
            subtitleText.maxVisibleCharacters = 0;
        }

        SetButtonState(startTrainingButton, 0f, false);
        SetButtonState(enterMissionButton, 0f, false);
    }

    private IEnumerator PlayWelcomeSequence()
    {
        yield return new WaitForSeconds(startDelay);

        yield return StartCoroutine(
            TypeText(titleText, titleCharacterDelay)
        );

        yield return new WaitForSeconds(betweenTextsDelay);

        yield return StartCoroutine(
            TypeText(subtitleText, subtitleCharacterDelay)
        );

        yield return new WaitForSeconds(buttonDelay);

        yield return StartCoroutine(FadeInButtons());
    }

    private IEnumerator TypeText(
        TMP_Text textElement,
        float characterDelay
    )
    {
        if (textElement == null)
        {
            yield break;
        }

        textElement.ForceMeshUpdate();

        int totalCharacters =
            textElement.textInfo.characterCount;

        textElement.maxVisibleCharacters = 0;

        WaitForSeconds characterWait =
            new WaitForSeconds(
                Mathf.Max(0.01f, characterDelay)
            );

        for (
            int visibleCharacters = 1;
            visibleCharacters <= totalCharacters;
            visibleCharacters++
        )
        {
            textElement.maxVisibleCharacters =
                visibleCharacters;

            yield return characterWait;
        }
    }

    private IEnumerator FadeInButtons()
    {
        float elapsedTime = 0f;

        while (elapsedTime < buttonFadeDuration)
        {
            elapsedTime += Time.deltaTime;

            float progress = Mathf.Clamp01(
                elapsedTime /
                Mathf.Max(0.01f, buttonFadeDuration)
            );

            float smoothProgress =
                Mathf.SmoothStep(0f, 1f, progress);

            SetButtonState(
                startTrainingButton,
                smoothProgress,
                false
            );

            SetButtonState(
                enterMissionButton,
                smoothProgress,
                false
            );

            yield return null;
        }

        SetButtonState(startTrainingButton, 1f, true);
        SetButtonState(enterMissionButton, 1f, true);
    }

    private void SetButtonState(
        CanvasGroup buttonGroup,
        float alpha,
        bool allowInteraction
    )
    {
        if (buttonGroup == null)
        {
            return;
        }

        buttonGroup.alpha = alpha;
        buttonGroup.interactable = allowInteraction;
        buttonGroup.blocksRaycasts = allowInteraction;
    }
}