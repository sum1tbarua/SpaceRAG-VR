using TMPro;
using UnityEngine;

public class ValidationManager : MonoBehaviour
{
    private enum AnswerQuality
    {
        Grounded,
        Partial,
        Risky
    }

    public static int LastFinalTrustScore
    {
        get;
        private set;
    }

    [Header("Ranked Evidence")]
    [SerializeField] private RankingSlot[] rankingSlots;

    [Header("Validation Panels")]
    [SerializeField] private GameObject validationChoicePanel;
    [SerializeField] private GameObject validationResultsPanel;

    [Header("Result Text")]
    [SerializeField] private TMP_Text validationResultMessageText;
    [SerializeField] private TMP_Text validationMetricsText;

    private void OnEnable()
    {
        if (validationChoicePanel != null &&
            validationResultsPanel != null)
        {
            validationChoicePanel.SetActive(true);
            validationResultsPanel.SetActive(false);
        }
    }

    public void ChooseGrounded()
    {
        EvaluatePlayerChoice(AnswerQuality.Grounded);
    }

    public void ChoosePartial()
    {
        EvaluatePlayerChoice(AnswerQuality.Partial);
    }

    public void ChooseRisky()
    {
        EvaluatePlayerChoice(AnswerQuality.Risky);
    }

    private void EvaluatePlayerChoice(
        AnswerQuality playerChoice)
    {
        int rankKey1 = FindIdealEvidenceRank(1);
        int rankKey2 = FindIdealEvidenceRank(2);
        

        AnswerQuality actualQuality =
            DetermineAnswerQuality(rankKey1, rankKey2);

        bool correctChoice =
            playerChoice == actualQuality;

        if (correctChoice)
        {
            validationResultMessageText.text =
                "CORRECT ASSESSMENT\nThis answer is " +
                GetQualityName(actualQuality) + ".";

            validationResultMessageText.color =
                new Color32(53, 183, 127, 255);
        }
        else
        {
            validationResultMessageText.text =
                "ASSESSMENT NEEDS REVIEW\nThis answer is actually " +
                GetQualityName(actualQuality) + ".";

            validationResultMessageText.color =
                new Color32(255, 179, 71, 255);
        }

        int retrievalScore =
            CalculateRetrievalScore(
                rankKey1,
                rankKey2);

        int rankingScore =
            CalculateRankingScore(
                rankKey1,
                rankKey2);

        int groundingScore;
        int citationScore;
        int hallucinationRisk;

        if (actualQuality == AnswerQuality.Grounded)
        {
            groundingScore = 100;
            citationScore = 100;
            hallucinationRisk = 0;
        }
        else if (actualQuality == AnswerQuality.Partial)
        {
            groundingScore = 60;
            citationScore = 50;
            hallucinationRisk = 40;
        }
        else
        {
            groundingScore = 10;
            citationScore = 0;
            hallucinationRisk = 90;
        }

        int finalTrustScore = Mathf.RoundToInt(
            (
                retrievalScore +
                rankingScore +
                groundingScore +
                citationScore +
                (100 - hallucinationRisk)
            ) / 5f);
        
        LastFinalTrustScore = finalTrustScore;

        validationMetricsText.text =
            "Retrieval Quality:        " +
            retrievalScore + "%\n" +

            "Ranking Quality:          " +
            rankingScore + "%\n" +

            "Grounding Score:          " +
            groundingScore + "%\n" +

            "Citation Support:         " +
            citationScore + "%\n" +

            "Hallucination Risk:       " +
            hallucinationRisk + "%\n" +

            "Final Trust Score:        " +
            finalTrustScore + "%";
        

        validationChoicePanel.SetActive(false);
        validationResultsPanel.SetActive(true);
    }

    private AnswerQuality DetermineAnswerQuality(
        int rankKey1,
        int rankKey2)
    {
        bool key1InTopTwo =
            rankKey1 > 0 && rankKey1 <= 2;

        bool key2InTopTwo =
            rankKey2 > 0 && rankKey2 <= 2;

        if (key1InTopTwo && key2InTopTwo)
        {
            return AnswerQuality.Grounded;
        }

        if (key1InTopTwo || key2InTopTwo)
        {
            return AnswerQuality.Partial;
        }

        return AnswerQuality.Risky;
    }

    private int CalculateRetrievalScore(
        int rankKey1,
        int rankKey2)
    {
        int score = 0;

        if (rankKey1 > 0 && rankKey1 <= 2)
        {
            score += 50;
        }

        if (rankKey2 > 0 && rankKey2 <= 2)
        {
            score += 50;
        }

        return score;
    }

    private int CalculateRankingScore(
        int rankKey1,
        int rankKey2)
    {
        int score = 0;

        if (rankKey1 > 0)
        {
            score += Mathf.Max(
                0,
                50 - Mathf.Abs(rankKey1 - 1) * 15);
        }

        if (rankKey2 > 0)
        {
            score += Mathf.Max(
                0,
                50 - Mathf.Abs(rankKey2 - 2) * 15);
        }

        return Mathf.Clamp(score, 0, 100);
    }

    private int FindIdealEvidenceRank(int idealRank)
    {
        for (int i = 0; i < rankingSlots.Length; i++)
        {
            DraggableEvidenceCard draggedCard =
                rankingSlots[i].GetCurrentCard();

            if (draggedCard == null)
            {
                continue;
            }

            EvidenceCard evidenceCard =
                draggedCard.GetComponent<EvidenceCard>();

            if (evidenceCard != null &&
                evidenceCard.IdealRank == idealRank)
            {
                return i + 1;
            }
        }

        return 0;
    }

    private int CountRankedDistractors()
    {
        int distractorCount = 0;

        for (int i = 0; i < rankingSlots.Length; i++)
        {
            DraggableEvidenceCard draggedCard =
                rankingSlots[i].GetCurrentCard();

            if (draggedCard == null)
            {
                continue;
            }

            EvidenceCard evidenceCard =
                draggedCard.GetComponent<EvidenceCard>();

            if (evidenceCard != null &&
                evidenceCard.IdealRank == 0)
            {
                distractorCount++;
            }
        }

        return distractorCount;
    }

    private string GetQualityName(
        AnswerQuality quality)
    {
        if (quality == AnswerQuality.Grounded)
        {
            return "Grounded";
        }

        if (quality == AnswerQuality.Partial)
        {
            return "Partially Supported";
        }

        return "Risky / Hallucinated";
    }
}