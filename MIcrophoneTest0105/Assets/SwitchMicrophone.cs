using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchMicrophone : MonoBehaviour
{
    enum MicState
    {
        IDLE,//대기
        RECORD,//녹음중
        DONE,//녹음완료
        SAVE//저장
    }
    MicState micState;

    AudioSource _audio;

    float loudness = 0;
    bool Rec;
    bool Saved;
    public int VoiceRecTime = 2;
    float sensitivity = 100;
    void Start()
    {
        Rec = true;
        Saved = true;
        _audio = GetComponent<AudioSource>();
        micState = MicState.IDLE;
    }
    void Update()
    {
        Debug.Log(loudness);
        Debug.Log(micState);
        loudness = GetAveragedVolume() * sensitivity;

        if (!Rec)//rec == false
        {
            VoiceRecTime -= (int)Time.deltaTime;
            if (VoiceRecTime < 0)
            {
                micState = MicState.DONE;
            }
        }
        if(Saved==true)
        {
            SwitchState();
        }

    }
    void SwitchState()
    {
        switch (micState)
        {
            case MicState.IDLE:
                IDLE();
                micState = MicState.RECORD;
                break;
            case MicState.RECORD:
                RECORD();
                break;
            case MicState.DONE:
                DONE();
                break;
            case MicState.SAVE:
                SAVE();
                break;
        }
    }
    void IDLE()
    {
       _audio.clip = Microphone.Start(Microphone.devices[0], true, 1, 44100);
        //_audio.loop = true;
        //while (!(Microphone.GetPosition(null) > 0)) { }
        _audio.Play();
        Debug.Log(micState);
        if (loudness > 1)
        {
            Microphone.End(Microphone.devices[0]);
        }
    }
    void RECORD()
    {
        _audio.clip = Microphone.Start(Microphone.devices[0], true, 5, 44100);
        if (loudness < 1)
        {
            Rec = false;
        }
    }
    void DONE()
    {
        Rec = true;
        Microphone.End(Microphone.devices[0]);
        micState = MicState.SAVE;
    }
    void SAVE() 
    {
        SavWav.Save("Voice1", _audio.clip);
        Saved = false;
    }

    float GetAveragedVolume()
    {
        float[] data = new float[256];
        float a = 0;
        _audio.GetOutputData(data, 0);
        foreach (float s in data)
        {
            a += Mathf.Abs(s);
        }
        return a / 256;
    }

}

