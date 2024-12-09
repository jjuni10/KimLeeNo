using UnityEngine;
using TMPro;

public class Rank : MonoBehaviour
{
    public TextMeshProUGUI rankListText;  // ��� ȭ�鿡�� ��ü ��ŷ�� ǥ���� UI �ؽ�Ʈ

    private void Start()
    {
        // PlayerPrefs���� ����� ��ŷ ����Ʈ�� �ҷ���
        string rankString = PlayerPrefs.GetString("RankList", "");

        if (!string.IsNullOrEmpty(rankString))
        {
            // ','�� �������� ��ŷ ����Ʈ �и�
            string[] rankArray = rankString.Split(',');

            // ��ŷ �迭�� UI�� ǥ��
            string rankDisplay = "";
            foreach (var rank in rankArray)
            {
                rankDisplay += rank + "\n";  // �� ��ŷ�� �ٹٲ����� �����Ͽ� ǥ��
            }

            rankListText.text = rankDisplay;
        }
       
    }
}
