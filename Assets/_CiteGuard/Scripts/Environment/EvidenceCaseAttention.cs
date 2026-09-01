using UnityEngine;

public class EvidenceCaseAttention : MonoBehaviour
{
    [Header("Position Vibration")]
    [SerializeField] private float horizontalAmount = 0.025f;
    [SerializeField] private float verticalAmount = 0.008f;
    [SerializeField] private float vibrationSpeed = 12f;

    [Header("Rotation Vibration")]
    [SerializeField] private float rotationAmount = 1.2f;
    [SerializeField] private float rotationSpeed = 9f;

    private Vector3 restingLocalPosition;
    private Quaternion restingLocalRotation;

    private void Awake()
    {
        RecordRestingTransform();
    }

    private void OnEnable()
    {
        RecordRestingTransform();
    }

    private void Update()
    {
        float currentTime = Time.unscaledTime;

        float horizontalOffset =
            Mathf.Sin(
                currentTime * vibrationSpeed
            ) * horizontalAmount;

        float verticalOffset =
            Mathf.Abs(
                Mathf.Sin(
                    currentTime *
                    vibrationSpeed *
                    0.5f
                )
            ) * verticalAmount;

        float rotationOffset =
            Mathf.Sin(
                currentTime * rotationSpeed
            ) * rotationAmount;

        transform.localPosition =
            restingLocalPosition +
            new Vector3(
                horizontalOffset,
                verticalOffset,
                0f
            );

        transform.localRotation =
            restingLocalRotation *
            Quaternion.Euler(
                0f,
                0f,
                rotationOffset
            );
    }

    private void OnDisable()
    {
        transform.localPosition =
            restingLocalPosition;

        transform.localRotation =
            restingLocalRotation;
    }

    private void RecordRestingTransform()
    {
        restingLocalPosition =
            transform.localPosition;

        restingLocalRotation =
            transform.localRotation;
    }
}