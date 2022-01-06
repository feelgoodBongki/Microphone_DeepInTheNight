using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MicrophoneSetting : MonoBehaviour
{
    public float sensitivity = 100;
    public float loudness = 0;
    public GameObject Sphere;
    private AudioSource _audio;
    public bool Rec = false;
    Rigidbody rigid;
    private void Awake()
    {
        _audio = GetComponent<AudioSource>();
        rigid = GetComponent<Rigidbody>();
    }
    void Start()
    {
        //_audio.clip = Microphone.Start(Microphone.devices[0].ToString(), true, 5, 44100);
        //_audio.loop = true;
        //_audio.mute = false;
        //while (!(Microphone.GetPosition(null) > 0)) { }
        _audio.clip = Microphone.Start(Microphone.devices[0].ToString(), true, 1, 44100);
        //Invoke(nameof(SaveWaveFile), 1);
        _audio.Play();
    }

    void Update()
    {
        loudness = GetAveragedVolume() * sensitivity;
        Debug.Log(loudness);
        if (loudness > 1 && Rec == false)
        {
            Debug.Log("in");
            Microphone.End(Microphone.devices[0]);
            _audio.clip = Microphone.Start(Microphone.devices[0], true, 5, 44100);
            rigid.AddForce(Vector3.up, ForceMode.Force);
            //Invoke(nameof(SaveWaveFile), 5);
            Rec = true;
        }
    }
    float GetAveragedVolume()//소리를 수치화
    {
        float[] data = new float[256];
        float a = 0;
        _audio.GetOutputData(data, 0);
        foreach (float s in data)
        {
            a += Mathf.Abs(s);//s의 절대값
        }
        return a / 256;
    }
    void SaveWaveFile()
    {
        SavWav.Save("녹음", _audio.clip);
    }
}
