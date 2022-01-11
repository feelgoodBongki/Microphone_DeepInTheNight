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

    //마이크 출력을 원하는 경우 스피커를 통한 임시 청취를 허용하는 public
    //But, 내부적으로는 출력 사운드를 오디오 소스의 스피커로 토글
    //마이크 리스너가 켜져있거나 꺼져있으면 켜짐
    private bool disableOutputSound = false;

    //오디오 소스도 이 스크립트와 동일한 객체에 첨부
    AudioSource src;

    //오디오 믹서 생성 후 AudioMixer에 드래그
    //오디오 믹서 선택 > "그룹"섹션 옆 + 클릭
    //자식으로 마스터 그룹에 추가하고 "Microphone"으로 이름 바꿔준다
    //그리고 오디오 소스의 출력 옵션에서 방금 생성한 이 객체를 선택해준다
    //오디오 믹서 인스펙터 창으로 돌아가 방금만든 Microphone를 클릭하고
    //인스펙터 창에서 "볼륨"을 마우스 오른쪽 버튼으로 클릭하고 스크립트에 "볼륨 노출(마이크)"를 선택
    //그리고 audiomixer 창 우측을 눌러 Exposed Parameters를 클릭
    //"볼륨"으로 이름을 바꿔준다.
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
            //유니티 5.x부터는 audio source에서 mute를 하면 정상적으로 음성이 안나온다
            //audio mixer에서 master volume의 db를 -80으로 하여 소리 출력만 안되도록 하면 된다.
            //_audio.mute = true;
        }
    }

    void Update()
    {//인스펙터에 나타나는 변수를 사용하거나 다른 스크립트에서 직접 함수를 호출할 수 있다.
        if (stopMicrophoneListener)
        {
            StopMicrophoneListener();
        }
        if(startMicrophoneListener)
        {
            StartMicrophoneListener();
        }

        //한번만 실행하기 위해서 매개변수를 false로 설정
        stopMicrophoneListener = false;
        startMicrophoneListener = false;

        //업데이트에서 작동한다 ( 다른 메서드에서 작동을 안한다 )
        MicrophoneIntoAudioSource(microphoneListenerOn);

        //인스펙터에서 음소거 해제를 선택할 수도 있다.
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
        //샘플로 배열 채우기
        GetComponent<AudioSource>().GetOutputData(_samples, 0);
        int i;
        float sum = 0;
        for (i = 0; i < qSamples; i++)
        {
            //샘플을 제곱한 값 = sum
            sum += _samples[i] * _samples[i];
        }
        //rms = 평균의 제곱근
        rmsValue = Mathf.Sqrt(sum / qSamples);
        dbValue = 20 * Mathf.Log10(rmsValue / refValue);
        if (dbValue < -160)
        {
            dbValue = -160;//-160데시벨로 고정
            //사운드 스펙트럼
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
                    maxN = i; //maxN은 최고 인덱스
                }
                float freqN = maxN; //인덱스 값을 넘겨준다.
                if (maxN > 0 && maxN < qSamples - 1)
                {
                    var dL = _spectrum[maxN - 1] / _spectrum[maxN];
                    var dR = _spectrum[maxN + 1] / _spectrum[maxN];
                    freqN += 0.5f * (dR * dR - dL * dL);
                }
                //인덱스를 빈도로 변환
                pitchValue = freqN * (_fSample / 2) / qSamples;
            }
        }
    }

    public void StopMicrophoneListener()//Stop Everything and returns audioclip to null
    {
        //stop the microphoneListener
        microphoneListenerOn = false;
        //믹서에서 마스터 사운드 다시 활성화
        disableOutputSound = false;
        //remove mic from audiosource clip
        src.Stop();
        src.clip = null;

        Microphone.End(null);
    }

    public void StartMicrophoneListener()
    {   //마이크 리스너 시작
        microphoneListenerOn = true;
        //사운드 출력 비활성화(출력에서 마이크 입력을 듣고싶지 않습니다!)
        disableOutputSound = true;
        //reset the audiosource
        RestartMicrophoneListener();
    }

    //볼륨이 켜져 있는지 여부를 제어
    //마이크 입력에 '꺼짐'을 사용 ( 자신의 음성입력을 듣고 싶지 않을때 )
    //음악 입력의 경우 '켜기'
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

    //마이크 다시시작 및 오디오 클립 삭제
    public void RestartMicrophoneListener()
    {
        src = GetComponent<AudioSource>();
        //오디오 소스에 있는 오디오 파일이 뭐든 삭제
        src.clip = null;

        timeSinceRestart = Time.time;
    }

    //마이크를 오디오 소스에 입력
    void MicrophoneIntoAudioSource(bool MicrophoneListenerOn)
    {
        if (MicrophoneListenerOn)
        {
            //지연과 버그를 피하기 위해 클립을 설정하기전 잠시 멈춤
            if (Time.time - timeSinceRestart > 0.5f && !Microphone.IsRecording(null))
            {
                src.clip = Microphone.Start(null, true, 10, 44100);

                //마이크 포지션을 찾을 때 까지 대기
                while (!(Microphone.GetPosition(null) > 0))
                {
                    src.Play();
                }
            }
        }
    }

}
