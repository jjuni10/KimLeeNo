using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private static SoundManager _instance;

    public AudioSource boostAudio;
    public AudioSource driftAudio;
    public AudioSource countAudio;
    public AudioSource bgmAudio;
    public AudioSource engineAudio;

    public static SoundManager Instance
    {
        get { return _instance; }
        set { }
    }

    void Awake()
    {
        if (Instance == null)
        {
            _instance = this;
        }
    }
}
