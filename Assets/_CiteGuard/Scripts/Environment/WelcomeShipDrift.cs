using UnityEngine;

public class WelcomeShipDrift : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float horizontalDistance = 0.04f;
    [SerializeField] private float verticalDistance = 0.07f;
    [SerializeField] private float movementSpeed = 2f;

    [Header("Rotation")]
    [SerializeField] private float rollAmount = 0.6f;
    [SerializeField] private float rollSpeed = 0.35f;

    private Vector3 startingLocalPosition;
    private Quaternion startingLocalRotation;

    private void Awake()
    {
        startingLocalPosition = transform.localPosition;
        startingLocalRotation = transform.localRotation;
    }

    private void Update()
    {
        float currentTime = Time.time;

        float horizontalOffset =
            Mathf.Sin(currentTime * movementSpeed * 0.7f) * horizontalDistance;

        float verticalOffset =
            Mathf.Sin(currentTime * movementSpeed) * verticalDistance;

        float roll =
            Mathf.Sin(currentTime * rollSpeed) * rollAmount;

        transform.localPosition =
            startingLocalPosition +
            new Vector3(horizontalOffset, verticalOffset, 0f);

        transform.localRotation =
            startingLocalRotation * Quaternion.Euler(0f, 0f, roll);
    }
}