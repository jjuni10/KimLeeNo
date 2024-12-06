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
        SceneManager.LoadScene(SceneLoader.TargetSceneName); // ����� ��ǥ ������ �̵�
    }
}