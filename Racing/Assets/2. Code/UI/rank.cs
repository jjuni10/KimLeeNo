using UnityEngine;
using TMPro;

public class Rank : MonoBehaviour
{
    public TextMeshProUGUI rankListText;  // 결과 화면에서 전체 랭킹을 표시할 UI 텍스트

    private void Start()
    {
        // PlayerPrefs에서 저장된 랭킹 리스트를 불러옴
        string rankString = PlayerPrefs.GetString("RankList", "");

        if (!string.IsNullOrEmpty(rankString))
        {
            // ','를 기준으로 랭킹 리스트 분리
            string[] rankArray = rankString.Split(',');

            // 랭킹 배열을 UI에 표시
            string rankDisplay = "";
            foreach (var rank in rankArray)
            {
                rankDisplay += rank + "\n";  // 각 랭킹을 줄바꿈으로 구분하여 표시
            }

            rankListText.text = rankDisplay;
        }
       
    }
}
