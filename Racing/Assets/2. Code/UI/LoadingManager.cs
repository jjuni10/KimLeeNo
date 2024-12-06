using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingManager : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(LoadTargetScene());
    }

    private System.Collections.IEnumerator LoadTargetScene()
    {
        yield return new WaitForSeconds(4f); 
        SceneManager.LoadScene(SceneLoader.TargetSceneName); // 저장된 목표 씬으로 이동
    }
}