using System.Collections;
using UnityEngine;
using TMPro; // TextMeshPro ���

public class CountdownManager : MonoBehaviour
{
    public TextMeshProUGUI countdownText; // TMP �ؽ�Ʈ ����
    public MonoBehaviour vehicleController; // ���� ���� ��ũ��Ʈ
    public Rigidbody vehicleRigidbody;     // ���� Rigidbody
    public int countdownTime = 5;          // ī��Ʈ�ٿ� �ð�

    private void Start()
    {
        // ������ ���� ��ũ��Ʈ�� ��Ȱ��ȭ
        if (vehicleController != null)
        {
            vehicleController.enabled = false;
        }

        // Rigidbody�� ���� ������ ����
        if (vehicleRigidbody != null)
        {
            vehicleRigidbody.isKinematic = true;
        }

        // ī��Ʈ�ٿ� ����
        StartCoroutine(CountdownRoutine());
    }

    private IEnumerator CountdownRoutine()
    {
        int timeRemaining = countdownTime;

        // ī��Ʈ�ٿ� ����
        while (timeRemaining > 0)
        {
            countdownText.text = timeRemaining.ToString(); // �ؽ�Ʈ ����
            yield return new WaitForSeconds(1f);
            timeRemaining--;
        }

        // ������ �ؽ�Ʈ "Go!" ǥ�� �� ��� ��� �غ�
        countdownText.text = "Go!";

        // ���� ���� ��ũ��Ʈ Ȱ��ȭ
        if (vehicleController != null)
        {
            vehicleController.enabled = true;
        }

        // Rigidbody ���� ���� �簳
        if (vehicleRigidbody != null)
        {
            vehicleRigidbody.isKinematic = false;
        }

        // "Go!" �ؽ�Ʈ�� ��� ������ �� ����
        yield return new WaitForSeconds(1f);
        countdownText.gameObject.SetActive(false);
    }
}