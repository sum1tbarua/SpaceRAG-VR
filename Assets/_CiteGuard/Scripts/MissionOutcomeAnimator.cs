using System.Collections;
using TMPro;
using UnityEngine;

public class MissionOutcomeAnimator : MonoBehaviour
{
    [Header("Hologram")]
    [SerializeField] private RectTransform hologramRect;
    [SerializeField] private CanvasGroup hologramGroup;

    [Header("Outcome Elements")]
    [SerializeField] private CanvasGroup headingGroup;
    [SerializeField] private CanvasGroup outcomeMessageGroup;
    [SerializeField] private CanvasGroup consequenceGroup;
    [SerializeField] private TMP_Text consequenceText;

    [Header("Buttons")]
    [SerializeField] private CanvasGroup[] buttonGroups;

    [Header("Animation Timing")]
    [SerializeField] private float hologramRevealDuration = 0.7f;
    [SerializeField] private float textFadeDuration = 0.3f;
    [SerializeField] private float characterDelay = 0.018f;
    [SerializeField] private float buttonFadeDuration = 0.35f;

    private Vector3 hologramNormalScale;
    private Coroutine revealCoroutine;
    private Coroutine idlePulseCoroutine;

    private void Awake()
    {
        hologramNormalScale = hologramRect.localScale;
    }

    public void Play()
    {
        StopExistingAnimations();

        revealCoroutine =
            StartCoroutine(PlayRevealSequence());
    }

    private IEnumerator PlayRevealSequence()
    {
        PrepareHiddenState();

        // Materialize the hologram.
        float elapsedTime = 0f;

        while (elapsedTime < hologramRevealDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(
                elapsedTime / hologramRevealDuration);

            float smoothProgress = Mathf.SmoothStep(
                0f,
                1f,
                progress);

            hologramGroup.alpha = smoothProgress;

            hologramRect.localScale = Vector3.Lerp(
                hologramNormalScale * 0.55f,
                hologramNormalScale,
                smoothProgress);

            hologramRect.localRotation = Quaternion.Lerp(
                Quaternion.Euler(0f, 0f, -8f),
                Quaternion.identity,
                smoothProgress);

            yield return null;
        }

        hologramGroup.alpha = 1f;
        hologramRect.localScale = hologramNormalScale;
        hologramRect.localRotation = Quaternion.identity;

        // Brief shield pulse.
        elapsedTime = 0f;
        const float pulseDuration = 0.4f;

        while (elapsedTime < pulseDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(
                elapsedTime / pulseDuration);

            float pulseAmount =
                Mathf.Sin(progress * Mathf.PI) * 0.08f;

            hologramRect.localScale =
                hologramNormalScale *
                (1f + pulseAmount);

            yield return null;
        }

        hologramRect.localScale = hologramNormalScale;

        yield return FadeCanvasGroup(
            headingGroup,
            textFadeDuration);

        yield return FadeCanvasGroup(
            outcomeMessageGroup,
            textFadeDuration);

        yield return FadeCanvasGroup(
            consequenceGroup,
            textFadeDuration);

        yield return TypeConsequenceMessage();

        yield return FadeButtons();

        revealCoroutine = null;

        // Keep the hologram gently moving afterward.
        idlePulseCoroutine =
            StartCoroutine(PlayIdlePulse());
    }

    private void PrepareHiddenState()
    {
        hologramGroup.alpha = 0f;
        headingGroup.alpha = 0f;
        outcomeMessageGroup.alpha = 0f;
        consequenceGroup.alpha = 0f;

        consequenceText.maxVisibleCharacters = 0;

        hologramRect.localScale =
            hologramNormalScale * 0.55f;

        foreach (CanvasGroup buttonGroup in buttonGroups)
        {
            if (buttonGroup == null)
            {
                continue;
            }

            buttonGroup.alpha = 0f;
            buttonGroup.interactable = false;
            buttonGroup.blocksRaycasts = false;
        }
    }

    private IEnumerator FadeCanvasGroup(
        CanvasGroup canvasGroup,
        float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(
                elapsedTime / duration);

            canvasGroup.alpha = Mathf.SmoothStep(
                0f,
                1f,
                progress);

            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    private IEnumerator TypeConsequenceMessage()
    {
        consequenceText.ForceMeshUpdate();

        int totalCharacters =
            consequenceText.textInfo.characterCount;

        WaitForSecondsRealtime characterWait =
            new WaitForSecondsRealtime(
                Mathf.Max(0.005f, characterDelay));

        for (int visibleCharacters = 1;
             visibleCharacters <= totalCharacters;
             visibleCharacters++)
        {
            consequenceText.maxVisibleCharacters =
                visibleCharacters;

            yield return characterWait;
        }
    }

    private IEnumerator FadeButtons()
    {
        float elapsedTime = 0f;

        while (elapsedTime < buttonFadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;

            float progress = Mathf.SmoothStep(
                0f,
                1f,
                Mathf.Clamp01(
                    elapsedTime / buttonFadeDuration));

            foreach (CanvasGroup buttonGroup in buttonGroups)
            {
                if (buttonGroup != null &&
                    buttonGroup.gameObject.activeInHierarchy)
                {
                    buttonGroup.alpha = progress;
                }
            }

            yield return null;
        }

        foreach (CanvasGroup buttonGroup in buttonGroups)
        {
            if (buttonGroup != null &&
                buttonGroup.gameObject.activeInHierarchy)
            {
                buttonGroup.alpha = 1f;
                buttonGroup.interactable = true;
                buttonGroup.blocksRaycasts = true;
            }
        }
    }

    private IEnumerator PlayIdlePulse()
    {
        float elapsedTime = 0f;

        while (true)
        {
            elapsedTime += Time.unscaledDeltaTime;

            float pulse =
                1f +
                Mathf.Sin(elapsedTime * 2f) * 0.018f;

            hologramRect.localScale =
                hologramNormalScale * pulse;

            yield return null;
        }
    }

    private void StopExistingAnimations()
    {
        if (revealCoroutine != null)
        {
            StopCoroutine(revealCoroutine);
        }

        if (idlePulseCoroutine != null)
        {
            StopCoroutine(idlePulseCoroutine);
        }

        revealCoroutine = null;
        idlePulseCoroutine = null;
    }

    private void OnDisable()
    {
        StopExistingAnimations();

        consequenceText.maxVisibleCharacters =
            int.MaxValue;

        hologramRect.localScale =
            hologramNormalScale;

        hologramRect.localRotation =
            Quaternion.identity;
    }
}