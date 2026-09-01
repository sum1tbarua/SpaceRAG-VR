using System.Collections;
using UnityEngine;

public class MissionCrewEntrance : MonoBehaviour
{
    [Header("Entrance Movement")]
    [SerializeField] private float heightOffset = 3.5f;
    [SerializeField] private float entranceDuration = 0.9f;

    [Header("Pop Effect")]
    [SerializeField] private float startingScale = 0.82f;
    [SerializeField] private float popAmount = 0.08f;

    private Vector3 finalLocalPosition;
    private Vector3 finalLocalScale;
    private Coroutine entranceCoroutine;

    private void Awake()
    {
        finalLocalPosition = transform.localPosition;
        finalLocalScale = transform.localScale;
    }

    private void OnEnable()
    {
        if (entranceCoroutine != null)
        {
            StopCoroutine(entranceCoroutine);
        }

        transform.localPosition =
            finalLocalPosition + Vector3.up * heightOffset;

        transform.localScale =
            finalLocalScale * startingScale;

        entranceCoroutine =
            StartCoroutine(PlayEntrance());
    }

    private void OnDisable()
    {
        if (entranceCoroutine != null)
        {
            StopCoroutine(entranceCoroutine);
            entranceCoroutine = null;
        }
    }

    private IEnumerator PlayEntrance()
    {
        Vector3 startingPosition =
            finalLocalPosition + Vector3.up * heightOffset;

        float elapsedTime = 0f;

        while (elapsedTime < entranceDuration)
        {
            elapsedTime += Time.deltaTime;

            float progress = Mathf.Clamp01(
                elapsedTime /
                Mathf.Max(0.01f, entranceDuration)
            );

            float smoothProgress =
                Mathf.SmoothStep(0f, 1f, progress);

            transform.localPosition = Vector3.Lerp(
                startingPosition,
                finalLocalPosition,
                smoothProgress
            );

            // Creates one gentle scale overshoot during entry.
            float pop =
                Mathf.Sin(progress * Mathf.PI) *
                popAmount;

            float scaleMultiplier = Mathf.Lerp(
                startingScale,
                1f,
                smoothProgress
            ) + pop;

            transform.localScale =
                finalLocalScale * scaleMultiplier;

            yield return null;
        }

        transform.localPosition = finalLocalPosition;
        transform.localScale = finalLocalScale;
        entranceCoroutine = null;
    }
}