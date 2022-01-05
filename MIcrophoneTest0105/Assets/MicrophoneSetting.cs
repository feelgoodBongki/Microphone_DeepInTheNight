using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MicrophoneSetting : MonoBehaviour
{
    public float sensitivity = 100;
    public float loudness = 0;
    public GameObject Sphere;
    private AudioSource _audio;

    private bool Rec = false; //Dance
    private void Awake()
    {
        _audio = GetComponent<AudioSource>();
    }
    void Start()
    {
        _audio.clip = Microphone.Start(null, true, 10, 44100);
        _audio.loop = true;
        _audio.mute = false;
        while (!(Microphone.GetPosition(null) > 0)) { }
        _audio.Play();
    }

    void Update()
    {
        loudness = GetAveragedVolume() * sensitivity;
        if(loudness > 7)
        {
            Rec = true;
            Sphere.transform.position += new Vector3(0, 1, 0);
        }
        else
        {
            Rec = false;
            Debug.Log("Ready");
        }
    }



    float GetAveragedVolume()//소리를 수치화
    {
        float[] data = new float[256];
        float a = 0;
        _audio.GetOutputData(data, 0);
        foreach(float s in data)
        {
            a += Mathf.Abs(s);//s의 절대값
        }
        return a / 256;
    }
}
