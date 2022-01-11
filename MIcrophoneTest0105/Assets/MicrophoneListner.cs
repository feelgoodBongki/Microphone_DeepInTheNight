using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
public class MicrophoneListner : MonoBehaviour
{
    public float sensitivity = 100;
    public float loudness = 0;
    public float pitch = 0;
    AudioSource _audio;

    public float rmsValue;
    public float dbValue;
    public float pitchValue;

    private const int qSamples = 1024;
    private const float refValue = 0.1f;
    private const float threshold = 0.02f;

    float[] _samples;
    private float[] _spectrum;
    private float _fSample;

    public bool startMicOnStartUp = true;

    public bool stopMicrophoneListener = false;
    public bool startMicrophoneListener = false;

    private bool microphoneListenerOn = false;

    //����ũ ����� ���ϴ� ��� ����Ŀ�� ���� �ӽ� û�븦 ����ϴ� public
    //But, ���������δ� ��� ���带 ����� �ҽ��� ����Ŀ�� ���
    //����ũ �����ʰ� �����ְų� ���������� ����
    private bool disableOutputSound = false;

    //����� �ҽ��� �� ��ũ��Ʈ�� ������ ��ü�� ÷��
    AudioSource src;

    //����� �ͼ� ���� �� AudioMixer�� �巡��
    //����� �ͼ� ���� > "�׷�"���� �� + Ŭ��
    //�ڽ����� ������ �׷쿡 �߰��ϰ� "Microphone"���� �̸� �ٲ��ش�
    //�׸��� ����� �ҽ��� ��� �ɼǿ��� ��� ������ �� ��ü�� �������ش�
    //����� �ͼ� �ν����� â���� ���ư� ��ݸ��� Microphone�� Ŭ���ϰ�
    //�ν����� â���� "����"�� ���콺 ������ ��ư���� Ŭ���ϰ� ��ũ��Ʈ�� "���� ����(����ũ)"�� ����
    //�׸��� audiomixer â ������ ���� Exposed Parameters�� Ŭ��
    //"����"���� �̸��� �ٲ��ش�.
    public AudioMixer masterMixer;

    float timeSinceRestart = 0;




    void Start()
    {

        if (startMicOnStartUp)
        {
            RestartMicrophoneListener();
            StartMicrophoneListener();

            _audio = GetComponent<AudioSource>();
            _audio.clip = Microphone.Start(null, true, 10, 44100);
            _audio.loop = true;
            while (!(Microphone.GetPosition(null) > 0)) { }
            _audio.Play();
            _samples = new float[qSamples];
            _spectrum = new float[qSamples];
            _fSample = AudioSettings.outputSampleRate;
            //����Ƽ 5.x���ʹ� audio source���� mute�� �ϸ� ���������� ������ �ȳ��´�
            //audio mixer���� master volume�� db�� -80���� �Ͽ� �Ҹ� ��¸� �ȵǵ��� �ϸ� �ȴ�.
            //_audio.mute = true;
        }
    }

    void Update()
    {//�ν����Ϳ� ��Ÿ���� ������ ����ϰų� �ٸ� ��ũ��Ʈ���� ���� �Լ��� ȣ���� �� �ִ�.
        if (stopMicrophoneListener)
        {
            StopMicrophoneListener();
        }
        if(startMicrophoneListener)
        {
            StartMicrophoneListener();
        }

        //�ѹ��� �����ϱ� ���ؼ� �Ű������� false�� ����
        stopMicrophoneListener = false;
        startMicrophoneListener = false;

        //������Ʈ���� �۵��Ѵ� ( �ٸ� �޼��忡�� �۵��� ���Ѵ� )
        MicrophoneIntoAudioSource(microphoneListenerOn);

        //�ν����Ϳ��� ���Ұ� ������ ������ ���� �ִ�.
        DisableSound(!disableOutputSound);

        loudness = GetAveragedVolume() * sensitivity;
        GetPitch();
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

    void GetPitch()
    {
        //���÷� �迭 ä���
        GetComponent<AudioSource>().GetOutputData(_samples, 0);
        int i;
        float sum = 0;
        for (i = 0; i < qSamples; i++)
        {
            //������ ������ �� = sum
            sum += _samples[i] * _samples[i];
        }
        //rms = ����� ������
        rmsValue = Mathf.Sqrt(sum / qSamples);
        dbValue = 20 * Mathf.Log10(rmsValue / refValue);
        if (dbValue < -160)
        {
            dbValue = -160;//-160���ú��� ����
            //���� ����Ʈ��
            GetComponent<AudioSource>().GetSpectrumData(_spectrum, 0, FFTWindow.BlackmanHarris);
            //BlackmanHarris =
            //    W[n] = 0.35875 - (0.48829 * COS(1.0*n/N)) + (0.14128*COs(2.0*n/N)) - (0.01168*COS(3.0*n/N)).
            float maxV = 0;
            var maxN = 0;
            for (i = 0; i < qSamples; i++)
            {
                if (!(_spectrum[i] > maxV) || !(_spectrum[i] > threshold))
                {
                    continue;
                    maxV = _spectrum[i];
                    maxN = i; //maxN�� �ְ� �ε���
                }
                float freqN = maxN; //�ε��� ���� �Ѱ��ش�.
                if (maxN > 0 && maxN < qSamples - 1)
                {
                    var dL = _spectrum[maxN - 1] / _spectrum[maxN];
                    var dR = _spectrum[maxN + 1] / _spectrum[maxN];
                    freqN += 0.5f * (dR * dR - dL * dL);
                }
                //�ε����� �󵵷� ��ȯ
                pitchValue = freqN * (_fSample / 2) / qSamples;
            }
        }
    }

    public void StopMicrophoneListener()//Stop Everything and returns audioclip to null
    {
        //stop the microphoneListener
        microphoneListenerOn = false;
        //�ͼ����� ������ ���� �ٽ� Ȱ��ȭ
        disableOutputSound = false;
        //remove mic from audiosource clip
        src.Stop();
        src.clip = null;

        Microphone.End(null);
    }

    public void StartMicrophoneListener()
    {   //����ũ ������ ����
        microphoneListenerOn = true;
        //���� ��� ��Ȱ��ȭ(��¿��� ����ũ �Է��� ������ �ʽ��ϴ�!)
        disableOutputSound = true;
        //reset the audiosource
        RestartMicrophoneListener();
    }

    //������ ���� �ִ��� ���θ� ����
    //����ũ �Է¿� '����'�� ��� ( �ڽ��� �����Է��� ��� ���� ������ )
    //���� �Է��� ��� '�ѱ�'
    public void DisableSound(bool SoundOn)
    {
        float volume = 0;

        if (SoundOn)
        {
            volume = 0.0f;
        }
        else
        {
            volume = -80.0f;
        }
        masterMixer.SetFloat("MasterVolume", volume);
    }

    //����ũ �ٽý��� �� ����� Ŭ�� ����
    public void RestartMicrophoneListener()
    {
        src = GetComponent<AudioSource>();
        //����� �ҽ��� �ִ� ����� ������ ���� ����
        src.clip = null;

        timeSinceRestart = Time.time;
    }

    //����ũ�� ����� �ҽ��� �Է�
    void MicrophoneIntoAudioSource(bool MicrophoneListenerOn)
    {
        if (MicrophoneListenerOn)
        {
            //������ ���׸� ���ϱ� ���� Ŭ���� �����ϱ��� ��� ����
            if (Time.time - timeSinceRestart > 0.5f && !Microphone.IsRecording(null))
            {
                src.clip = Microphone.Start(null, true, 10, 44100);

                //����ũ �������� ã�� �� ���� ���
                while (!(Microphone.GetPosition(null) > 0))
                {
                    src.Play();
                }
            }
        }
    }

}
