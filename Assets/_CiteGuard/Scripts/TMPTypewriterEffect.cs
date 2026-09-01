using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class TMPTypewriterEffect : MonoBehaviour
{
    [SerializeField]
    private float secondsPerCharacter = 0.025f;

    private TMP_Text textComponent;
    private Coroutine typingCoroutine;

    private void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        typingCoroutine = StartCoroutine(TypeAnswer());
    }

    private void OnDisable()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        if (textComponent != null)
        {
            textComponent.maxVisibleCharacters = int.MaxValue;
        }
    }

    private IEnumerator TypeAnswer()
    {
        // Updates TextMeshPro's information about the new answer.
        textComponent.ForceMeshUpdate();

        int totalCharacters =
            textComponent.textInfo.characterCount;

        textComponent.maxVisibleCharacters = 0;

        for (int visibleCharacters = 1;
             visibleCharacters <= totalCharacters;
             visibleCharacters++)
        {
            textComponent.maxVisibleCharacters =
                visibleCharacters;

            yield return new WaitForSeconds(
                secondsPerCharacter);
        }

        typingCoroutine = null;
    }

    public void ShowImmediately()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        textComponent.maxVisibleCharacters = int.MaxValue;
    }
}