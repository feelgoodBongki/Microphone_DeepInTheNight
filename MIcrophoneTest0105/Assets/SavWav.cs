//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using System;
//using System.IO;

//public static class SavWav : MonoBehaviour
//{
//    const int HEADER_SIZE = 44;
//    //RIFF, FMT, DATA형식을 모두 더했을때 44바이트가 나온다.
//    //그래서 헤더 사이즈가 44인듯하다.

//    public static bool Save(string filename, AudioClip clip)
//    {
//        if (!filename.ToLower().EndsWith(".wav"))//파일이름이 .wav로 끝나지 않는다면
//        {
//            filename += ".wav";
//        }
//        var filepath = Path.Combine(Application.persistentDataPath, filename);
//        // >Path.Combine = 문자열을 경로로 결합한다
//        // ex) Combine(String[]) - 하나의 경로로 결합
//        //Combine (String, String) - 두개의 문자열을 하나의 경로로 결합
//        //... (4개까지 사용가능한것으로 보임)

//        //Application.persistentDataPath = 아래와 같이 경로가 지정된다.
//        //On Win7 - C:/Users/Username/AppData/LocalLow/CompanyName/GameName
//        //On Android - / Data / Data / com.companyname.gamename / Files

//        Debug.Log(filepath);//파일 저장된 경로


//        //하위 디렉토리에 저장하는 경우 디렉토리가 존재하는지 확인.
//        //존재한다면 리턴
//        //존재하지 않는다면 폴더를 만든다. maybe?
//        Directory.CreateDirectory(Path.GetDirectoryName(filepath));


//        /////////////using을 왜 사용하였는지////////////
//        //문장형태의 using문
//        //IDisposable객체의 올바른 사용을 보장하는 편리한 구문을 제공해주는것이 바로 using문이다
//        //File 및 Font와 같은 클래스들은 관리되지 않는 리소스에 액세스 하는 대표적인 클래스
//        //이 말은 해당 클래스들을 다 사용한 후에는 적절한 시기에 '해제(Dispose)하여 해당 리소스(자원)을 다시 
//        //'반납'해야 하는것
//        //하지만, 매번 관리되지 않는 리소스에 액세서하는 클래스를 체크해서 Dispose하는것은 시간과 실수를 야기한다.
//        //이 때 using문을 이용하면 해당 리소스 범위를 벗어나게 될 시 자동으로 Dispose하여 관리를 도와주기에
//        //수고와 시간은 물론 실수도 덜어준다.
//        using (var fileStream = CreateEmpty(filepath)) //새로운 파일을 만드는것
//        {
//            ConverAndWrite(fileStream, clip);

//            WriteHeader(fileStream.clip);
//        }
//        return true; //TODO : 파일 저장에 실패하면 false를 반환
//        //<<FileStream>>
//        //FileStream = 파일 입출력
//        //매개체 클래스를 통해 접근하여 읽고, 쓸 수 있다.
//        //일반적으로 byte[]배열을 이용해 읽기/쓰기를 수행

//        //ex) FileStream fs = new FileStream("파일 이름", FileMode.OpenOrCreate)
//        //'파일을 열 되 없으면 생성하라' 라는 뜻
//        //읽거나 써라~
//    }

//    public static AudioClip TrimSilence(AudioClip clip, float min)//침묵 다듬기? 
//    {
//        var samples = new float[clip.samples];
//        //클립의 샘플 데이터로 배열을 채운다.
//        //샘플은 -1.0f ~ 1.0f범위의 부동 소수점
//        //샘플수는 float 배열의 길이에 의해 결정됨.


//        //<<Clip.GetData>>
//        //offsetSamples매개 변수를 사용하여 클립의 특정 위치에서 읽기 시작
//        //오프셋의 읽기 길이가 클립 길이보다 길면 읽기가 랩을 둘러싸고 클립의 시작부분에서 나머지 샘플을 읽는다.
//        //압축된 오디오 파일의 경우 오디오 가져오기에서 로드유형이 불러와졌을시 압축 해제로 설정된 경우에만
//        //샘플데이터를 검색할 수 있다.
//        //그렇지 않은 경우 배열의 보든 샘플 값에 대해 '0'으로 반환
//        clip.GetData(samples, 0);
//        //클립의 데이터(샘플데이터)를 0번부터 가져온다.



//        /////////////아래 함수의 매개변수를 리턴하는데 무슨뜻일까//////////////////////////
//        return TrimSilence(new List<float>(samples), min, clip.channels, clip.frequency);
//    }
//    public static AudioClip TrimSilence(List<float> samples, float min, int channels, int hz)
//    {
//        return TrimSilence(samples, min, channels, hz, false, false);
//        ///////////////아래의 함수 매개변수의 느낌인데 무엇인지 모르겠다./////////////////////
//    }
//    public static AudioClip TrimSilence(List<float> samples, float min, int channels, int hz, bool _3D, bool stream)
//    {
//        int i; //샘플의 절대값이 min보다 작을 경우 멈추는데 
//        for (i = 0; i < samples.Count; i++)
//        {
//            if (Mathf.Abs(samples[i]) > min)
//            {
//                break;
//            }
//        }

//        samples.RemoveRange(0, i); //min보다 작은 샘플은 삭제시킨다

//        for(i = samples.Count -1; i> 0; i--)//샘플
//        {
//            if (Mathf.Abs(samples[i]) > min)
//            {
//                break;
//            }
//        }
//        //-------------------------------여까지---------------------------------
//    }

//    void Start()
//    {

//    }

//    void Update()
//    {

//    }
//}
