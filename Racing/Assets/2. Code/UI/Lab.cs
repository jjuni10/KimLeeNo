using UnityEngine;
using TMPro;

public class LapCounter : MonoBehaviour
{
    public TextMeshProUGUI lapText; // TMP �ؽ�Ʈ ����
    public GameObject checkpoint;  // üũ����Ʈ ������Ʈ
    public int totalLaps = 3;      // �� Lap ��

    private int currentLap = 0;    // ���� Lap ��
    private bool canCountLap = true; // �ߺ� ���� ���� �÷���

    private void Start()
    {
        UpdateLapText();
    }

    private void OnTriggerEnter(Collider other)
    {
        // ������ üũ����Ʈ�� ����ϰ� �ߺ� ������ �ƴ� ��
        if (other.gameObject.CompareTag("Checkpoint") && canCountLap)
        {
            canCountLap = false; // �ߺ� ���� ����
            currentLap++;
            UpdateLapText();

            // ���̽� ���� ����
            if (currentLap > totalLaps)
            {
                Debug.Log("Finish!");
                lapText.text = "Finish!";
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // ������ üũ����Ʈ�� ����� �ٽ� ���� �����ϵ��� ����
        if (other.gameObject.CompareTag("Checkpoint"))
        {
            canCountLap = true;
        }
    }

    private void UpdateLapText()
    {
        lapText.text = "Lap " + currentLap + " / " + totalLaps;
    }
}