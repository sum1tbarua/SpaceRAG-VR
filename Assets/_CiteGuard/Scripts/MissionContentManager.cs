using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MissionContentManager : MonoBehaviour
{
    [Header("Mission Pool")]
    [SerializeField] private MissionData[] missions;
    [SerializeField] private int missionsPerSession = 3;

    [Header("Mission UI")]
    [SerializeField] private TMP_Text missionProgressText;
    [SerializeField] private TMP_Text crewQueryText;
    [SerializeField] private TMP_Text activeQueryText;
    [SerializeField] private EvidenceCard[] evidenceCardViews;

    private static int completedMissions;
    private static int groundedAnswers;
    private static int validatedAnswers;
    private static int skippedValidationAnswers;
    private static int totalTrustScore;
    private static int recordedAttempts;

    private static readonly List<int> usedMissionIndices =
        new List<int>();

    private MissionData currentMission;

    public MissionData CurrentMission => currentMission;
    public int MissionsPerSession => missionsPerSession;

    public static int CompletedMissions =>
        completedMissions;

    public static int GroundedAnswers =>
        groundedAnswers;

    public static int ValidatedAnswers =>
        validatedAnswers;

    public static int SkippedValidationAnswers =>
        skippedValidationAnswers;

    public static int AverageTrustScore
    {
        get
        {
            if (recordedAttempts == 0)
            {
                return 0;
            }

            return Mathf.RoundToInt(
                (float)totalTrustScore / recordedAttempts);
        }
    }

    private void Start()
    {
        LoadRandomMission();
    }

    private void LoadRandomMission()
    {
        if (missions == null || missions.Length == 0)
        {
            Debug.LogError(
                "No missions were assigned to MissionContentManager.");

            return;
        }

        int missionIndex = ChooseUnusedMissionIndex();
        currentMission = missions[missionIndex];

        missionProgressText.text =
            "MISSION " + (completedMissions + 1) +
            " / " + missionsPerSession;

        crewQueryText.text = currentMission.crewQuery;
        activeQueryText.text = currentMission.crewQuery;

        DisplayShuffledEvidence();
    }

    private int ChooseUnusedMissionIndex()
    {
        if (usedMissionIndices.Count >= missions.Length)
        {
            usedMissionIndices.Clear();
        }

        List<int> availableIndices = new List<int>();

        for (int i = 0; i < missions.Length; i++)
        {
            if (!usedMissionIndices.Contains(i))
            {
                availableIndices.Add(i);
            }
        }

        int randomPosition =
            Random.Range(0, availableIndices.Count);

        int selectedIndex =
            availableIndices[randomPosition];

        usedMissionIndices.Add(selectedIndex);

        return selectedIndex;
    }

    private void DisplayShuffledEvidence()
    {
        List<EvidenceData> shuffledEvidence =
            new List<EvidenceData>(
                currentMission.evidenceCards);

        for (int i = 0;
             i < shuffledEvidence.Count;
             i++)
        {
            int randomIndex =
                Random.Range(i, shuffledEvidence.Count);

            EvidenceData temporaryCard =
                shuffledEvidence[i];

            shuffledEvidence[i] =
                shuffledEvidence[randomIndex];

            shuffledEvidence[randomIndex] =
                temporaryCard;
        }

        int cardCount = Mathf.Min(
            evidenceCardViews.Length,
            shuffledEvidence.Count);

        for (int i = 0; i < cardCount; i++)
        {
            string displayLetter =
                ((char)('A' + i)).ToString();

            evidenceCardViews[i].Configure(
                displayLetter,
                shuffledEvidence[i]);
        }
    }

    public string GetGroundedAnswer()
    {
        return GetRandomText(
            currentMission.groundedAnswers);
    }

    public string GetPartialAnswer()
    {
        return GetRandomText(
            currentMission.partialAnswers);
    }

    public string GetRiskyAnswer()
    {
        return GetRandomText(
            currentMission.riskyAnswers);
    }

    private string GetRandomText(string[] choices)
    {
        if (choices == null || choices.Length == 0)
        {
            return "No predefined answer was assigned.";
        }

        string selectedText =
            choices[Random.Range(0, choices.Length)];

        return selectedText
            .Replace(
                "{KEY1}",
                FindEvidenceLabel(1))
            .Replace(
                "{KEY2}",
                FindEvidenceLabel(2));
    }

    private string FindEvidenceLabel(int idealRank)
    {
        for (int i = 0;
             i < evidenceCardViews.Length;
             i++)
        {
            if (evidenceCardViews[i].IdealRank ==
                idealRank)
            {
                string letter =
                    ((char)('A' + i)).ToString();

                return "Evidence " + letter;
            }
        }

        return "the ranked evidence";
    }

    public void RecordAttempt(
        bool grounded,
        bool wasValidated,
        int trustScore)
    {
        recordedAttempts++;
        totalTrustScore += trustScore;

        if (grounded)
        {
            groundedAnswers++;
        }

        if (wasValidated)
        {
            validatedAnswers++;
        }
        else
        {
            skippedValidationAnswers++;
        }
    }

    public bool MarkMissionCompleted()
    {
        completedMissions++;

        return completedMissions >=
               missionsPerSession;
    }

    public static void ResetSession()
    {
        completedMissions = 0;
        groundedAnswers = 0;
        validatedAnswers = 0;
        skippedValidationAnswers = 0;
        totalTrustScore = 0;
        recordedAttempts = 0;

        usedMissionIndices.Clear();
    }
}