using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System;
public class MicrophoneSetting : MonoBehaviour
{
    public GameObject Sphere;
    private AudioSource _audio;
    public AudioMixerGroup audioMixer;

    public float loudness = 0;
    float RecTime = 10f;
    //마이크 상태
    bool rec; //대기상태, 녹음가능
    bool voiceIn; //녹음중 여부
    bool saved;//저장여부

    int VoiceRecTime = 100;
    public float sensitivity = 100;
    void Start()
    {
        //마이크 설정
        rec = true;
        saved = false;
        _audio = GetComponent<AudioSource>();
        _audio.clip = Microphone.Start(Microphone.devices[0], true, 1, 44100);
        _audio.loop = true;
        // 1번째 마이크장치가 있을때 (Microphone.GetPosition(Microphone.device[0]);과 같음.
        while (!(Microphone.GetPosition(Microphone.devices[0]) > 0)) { }
        _audio.Play();
        // _audio.mute = true;
    }
    void Update()
    {

        loudness = GetAveragedVolume() * sensitivity;
        //사운드 측정
        SecondWay2();
        Debug.Log("Loudness : " + (int)loudness);
        Debug.Log(voiceIn);
    }
    void SecondWay2()
    {
        //목소리 입력 시작
        if (rec)
        {
            //사운드 측정
            if (loudness > 1)
            {
                voiceIn = true;
            }
            else if (loudness < 1)
            {
                voiceIn = false;
                saved = true;
                
            }
            //if (loudness < 1)
            //{
            //    voiceIn = false;
            //    //saved = true;
            //}
            //else if (loudness > 1)
            //{
            //    voiceIn = true;
            //}

            //소리1이상
            if (voiceIn)
            {
                StartCoroutine(VoiceRec());
                //RecTime = 2;
            }
            //소리 1이하
            else if (saved && voiceIn == false)
            {
                RecTime -= Time.deltaTime;
                ThirdWay(_audio, Microphone.devices[0]);
                if (RecTime < 0)
                {
                   
                    voiceIn = true;
                    rec = false;
                }
            }
        }
    }


    IEnumerator VoiceRec()
    {
        Microphone.End(Microphone.devices[0]);//재생중이던 마이크 정지
                                              //마이크 다시 녹음 시작
        _audio.clip = Microphone.Start(Microphone.devices[0], true, VoiceRecTime, 44100);

        //if (Microphone.IsRecording(Microphone.devices[0]))
        //{
        //    VoiceRecTime += (int)Time.deltaTime;
        //}
        yield return null;
    }
    void FirstWay()
    {
        //사운드 측정
        loudness = GetAveragedVolume() * sensitivity;
        if (rec)
        {
            if (loudness > 1)
            {
                StartCoroutine(VoiceRec());
                voiceIn = false; //녹음끝
            }
            else if (voiceIn == false)
            {
                if (loudness < 0.01f)
                {
                    RecTime -= Time.deltaTime;
                    return;
                }
                if (RecTime < 0)
                {
                    rec = false;
                    saved = false;
                }
                return;
            }
        }
        else if (rec == false && saved == false)
        {
            Microphone.End(Microphone.devices[0]);
            SaveWaveFile();
            saved = true;
            ///마이크 녹음 길이 길게하고 소리가 일정 이하로 떨어지면 녹음처리
        }
    }
    void ThirdWay(AudioSource audS, string deviceName)
    {
        //Capture the current clip data
        AudioClip recordedClip = audS.clip;
        var position = Microphone.GetPosition(Microphone.devices[0]);
        var soundData = new float[recordedClip.samples * recordedClip.channels];
        recordedClip.GetData(soundData, 0);

        //Create shortened array for the data that was used for recording
        var newData = new float[position * recordedClip.channels];

        //Copy the used samples to a new array
        for (int i = 0; i < newData.Length; i++)
        {
            newData[i] = soundData[i];
        }

        //One does not simply shorten an AudioClip,
        //so we make a new one with the appropriate length

        var newClip = AudioClip.Create(recordedClip.name, position, recordedClip.channels, recordedClip.frequency, false);
        newClip.SetData(newData, 0); //Give it the data from the old clip
        //Replace the old Clip
        AudioClip.Destroy(recordedClip);
        audS.clip = newClip;
        SaveWaveFile();
    }

    void SecondWay()
    {
        //사운드 측정
        loudness = GetAveragedVolume() * sensitivity;
        if (rec)
        {//true false false
            if (loudness > 1)
            {
                voiceIn = true;
                saved = false;
                //true true false
            }
            else if (loudness < 1)
            {
                voiceIn = false;
                saved = true;
               
                //true false true
            }

            if (/*rec && */saved == false)
            {//true true false

                StartCoroutine(VoiceRec());
                RecTime = 2;
                voiceIn = false;
                //true false false
            }
            else if (saved && voiceIn == false)
            {//true false true
                RecTime -= Time.deltaTime;
                if (RecTime < 0)
                {
                    //SaveWaveFile();
                    voiceIn = true;
                    rec = false;
                    //false true true
                }
            }
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

    //void StopRecordMicrophone()
    //{
    //    int lastTime = Microphone.GetPosition(null);
    //    if (lastTime == 0)
    //    {
    //        Debug.Log("asdfsdasf");
    //        return;
    //    }
    //    else
    //    {
    //        Microphone.End(Microphone.devices[0]);
    //        float[] samples = new float[_audio.clip.samples];
    //        _audio.clip.GetData(samples, 0);
    //        float[] cutSamples = new float[lastTime];
    //        Array.Copy(samples, cutSamples, cutSamples.Length - 1);
    //        _audio.clip = AudioClip.Create("Voice", cutSamples.Length, 1, 44100, false);
    //        _audio.clip.SetData(cutSamples, 0);
    //        SaveWaveFile();
    //        Debug.Log("dddd");
    //    }
    //}

    void ThirdWay_Two()
    {
        //Capturing the current clip data
        AudioClip recordedClip = _audio.clip;
        var position = Microphone.GetPosition(Microphone.devices[0]);
        var soundData = new float[recordedClip.samples * recordedClip.channels];
        recordedClip.GetData(soundData, 0);
        //Create shortened array for the data that was used for recording
        var newData = new float[position * recordedClip.channels];

        //Microphone.End(null)
        for (int i = 0; i < newData.Length; i++)
        {
            newData[i] = soundData[i];
        }

        //One does not simply shorten an AudioClip,
        //so we make a new one with the appropriate length
        var newClip = AudioClip.Create(recordedClip.name, position, recordedClip.channels, recordedClip.frequency, false);
        newClip.SetData(newData, 0);//Give it the data from the old clip

        //Replace the old clip
        AudioClip.Destroy(recordedClip);
        _audio.clip = newClip;
    }
}



