using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static string TargetSceneName; // �ε� �� �̵��� �� �̸�

    public void LoadSceneWithLoading(string targetScene)
    {
        TargetSceneName = targetScene; // ��ǥ �� ����
        SceneManager.LoadScene("Loading"); // �ε� ������ �̵�
    }
}