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
        for(int i=0;i<playCar.Count; i++)
        {
            rankText[i].text = $"{i+1}-{playCar[i].GetComponent<ReverseDetection>().carName}";

            if (playCar[i].GetComponent<ReverseDetection>().carName == "Player")
            {
                rankText[i].color = Color.red;
            }
            else
            {
                rankText[i].color = Color.white;
            }
        }
    }
}
