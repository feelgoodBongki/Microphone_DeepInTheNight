using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
public class MicrophoneSetting : MonoBehaviour
{
    public GameObject Sphere;
    private AudioSource _audio;
    public AudioMixerGroup audioMixer;

    float loudness = 0;
    float RecTime = 5;
    bool Rec;
    bool Saved;
    public int VoiceRecTime = 2;
    public float sensitivity = 100;
    void Start()
    {
        _audio = GetComponent<AudioSource>();
        _audio.clip = Microphone.Start(Microphone.devices[0], true, 1, 44100);
        _audio.loop = true;
        // 1번째 마이크장치가 있을때 (Microphone.GetPosition(Microphone.device[0]);과 같음.
        while (!(Microphone.GetPosition(null) > 0)) { }
        _audio.Play();
        // _audio.mute = true;
        Saved = false;
        Rec = true;
        Debug.Log(loudness);
    }
    void Update()
    {
        Debug.Log("Loudness : " + loudness);
        //사운드 측정
        loudness = GetAveragedVolume() * sensitivity;
        if (Rec)
        {
            if (loudness > 1)
            {
                StartCoroutine(VoiceRec());
                Debug.Log("dd");
            }
            else if (loudness < 1)
            {
                Debug.Log(RecTime);
                RecTime -= Time.deltaTime;
                //Debug.Log(RecTime);
                if (RecTime < 0)
                {
                    RecTime = VoiceRecTime;
                    Rec = false;
                    Saved = false;
                }
                return;
            }
        }
        else if (Rec == false && Saved == false)
        {
            Microphone.End(Microphone.devices[0]);
            SaveWaveFile();
            Saved = true;
            ///마이크 녹음 길이 길게하고 소리가 일정 이하로 떨어지면 녹음처리
        }
    }
    IEnumerator VoiceRec()
    {
        Microphone.End(Microphone.devices[0]);//재생중이던 마이크 정지
        //마이크 다시 녹음 시작
        _audio.clip = Microphone.Start(Microphone.devices[0], true, VoiceRecTime, 44100);
        if (Microphone.IsRecording(Microphone.devices[0]))
        {
            Debug.Log("VOICE : " + VoiceRecTime);
            VoiceRecTime += (int)Time.deltaTime;
        }
        yield return null;
    }
    //레코딩하는 동안

    //if (loudness > 1)
    //{
    //    RecTime = 2;
    //}
    //else if (loudness < 1)
    //{
    //    RecTime -= Time.deltaTime;
    //    if (RecTime < 0)
    //    {
    //        Microphone.End(Microphone.devices[0]);
    //        //Invoke(nameof(SaveWaveFile), VoiceRecTime);
    //        SaveWaveFile();
    //        return;
    //    }
    //}
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


