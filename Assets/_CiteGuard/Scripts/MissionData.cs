using UnityEngine;

[System.Serializable]
public class EvidenceData
{
    public string evidenceId;

    [TextArea(2, 3)]
    public string evidenceText;

    [Range(0, 2)]
    public int idealRank;
}

[System.Serializable]
public class MissionData
{
    public string missionId;
    public string missionTitle;

    [TextArea(2, 4)]
    public string crewQuery;

    public EvidenceData[] evidenceCards = new EvidenceData[4];

    [TextArea(2, 4)]
    public string[] groundedAnswers;

    [TextArea(2, 4)]
    public string[] partialAnswers;

    [TextArea(2, 4)]
    public string[] riskyAnswers;

    [Header("Mission Consequences")]
    [TextArea(2, 4)]
    public string successConsequence;

    [TextArea(2, 4)]
    public string delayedConsequence;

    [TextArea(2, 4)]
    public string failureConsequence;
}