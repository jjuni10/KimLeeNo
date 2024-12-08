using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class RankManager : MonoBehaviour
{
    private static RankManager _instance;

    public List<GameObject> playCar=new List<GameObject>();
    public List<TextMeshProUGUI> rankText=new List<TextMeshProUGUI>();

    public static RankManager Instance
    {
        get { return _instance; }
        set { }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            _instance = this;
        }
    }

    void Update()
    {
        RankUpdate();
        RankUIUpdate();
    }

    void RankUpdate()
    {
        playCar=playCar.OrderByDescending(obj=>obj.GetComponent<ReverseDetection>().rankData).ToList();
    }

    void RankUIUpdate()
    {
        // 플레이어의 전체 랭킹 배열을 문자열로 변환하여 저장
        List<string> rankList = new List<string>();

        for (int i = 0; i < playCar.Count; i++)
        {
            string rankInfo = $"{i + 1}-{playCar[i].GetComponent<ReverseDetection>().carName}";
            rankText[i].text = rankInfo;

            // 플레이어의 순위를 배열에 추가
            rankList.Add(rankInfo);

            if (playCar[i].GetComponent<ReverseDetection>().carName == "Player")
            {
                rankText[i].color = Color.red;
            }
            else
            {
                rankText[i].color = Color.white;
            }
        }

        // 배열을 문자열로 변환하여 PlayerPrefs에 저장
        string rankString = string.Join(",", rankList);
        PlayerPrefs.SetString("RankList", rankString);
        PlayerPrefs.Save();
    }
}
