using TMPro;
using UnityEngine;

public class EvidenceCard : MonoBehaviour
{
    [SerializeField] private TMP_Text evidenceHeaderText;
    [SerializeField] private TMP_Text evidenceBodyText;

    public string EvidenceId { get; private set; }
    public int IdealRank { get; private set; }

    public string DisplayHeader => evidenceHeaderText.text;
    public string DisplayText => evidenceBodyText.text;

    public void Configure(string displayLetter, EvidenceData data)
    {
        EvidenceId = data.evidenceId;
        IdealRank = data.idealRank;

        evidenceHeaderText.text = "EVIDENCE " + displayLetter;
        evidenceBodyText.text = data.evidenceText;
    }
}