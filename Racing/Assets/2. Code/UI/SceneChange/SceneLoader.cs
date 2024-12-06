using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static string TargetSceneName; // 로딩 후 이동할 씬 이름

    public void LoadSceneWithLoading(string targetScene)
    {
        TargetSceneName = targetScene; // 목표 씬 저장
        SceneManager.LoadScene("Loading"); // 로딩 씬으로 이동
    }
}