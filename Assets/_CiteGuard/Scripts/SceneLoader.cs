using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadSpaceMission()
    {
        SceneManager.LoadScene("01_SpaceMission");
    }
}