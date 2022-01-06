using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public static class SavWav
{
    const int HEADER_SIZE = 44;
    //RIFF, FMT, DATA형식을 모두 더했을때 44바이트가 나온다.
    //그래서 헤더 사이즈가 44인듯하다.

    public static bool Save(string filename, AudioClip clip)
    {
        if (!filename.ToLower().EndsWith(".wav"))//파일이름이 .wav로 끝나지 않는다면
        {
            filename += ".wav";
        }
        var filepath = Path.Combine(Application.persistentDataPath, filename);
        // >Path.Combine = 문자열을 경로로 결합한다
        // ex) Combine(String[]) - 하나의 경로로 결합
        //Combine (String, String) - 두개의 문자열을 하나의 경로로 결합
        //... (4개까지 사용가능한것으로 보임)

        //Application.persistentDataPath = 아래와 같이 경로가 지정된다.
        //On Win7 - C:/Users/Username/AppData/LocalLow/CompanyName/GameName
        //On Android - / Data / Data / com.companyname.gamename / Files

        Debug.Log(filepath);//파일 저장된 경로


        //하위 디렉토리에 저장하는 경우 디렉토리가 존재하는지 확인.
        //존재한다면 리턴
        //존재하지 않는다면 폴더를 만든다. maybe?
        Directory.CreateDirectory(Path.GetDirectoryName(filepath));


        /////////////using을 왜 사용하였는지////////////
        //문장형태의 using문
        //IDisposable객체의 올바른 사용을 보장하는 편리한 구문을 제공해주는것이 바로 using문이다
        //File 및 Font와 같은 클래스들은 관리되지 않는 리소스에 액세스 하는 대표적인 클래스
        //이 말은 해당 클래스들을 다 사용한 후에는 적절한 시기에 '해제(Dispose)하여 해당 리소스(자원)을 다시 
        //'반납'해야 하는것
        //하지만, 매번 관리되지 않는 리소스에 액세서하는 클래스를 체크해서 Dispose하는것은 시간과 실수를 야기한다.
        //이 때 using문을 이용하면 해당 리소스 범위를 벗어나게 될 시 자동으로 Dispose하여 관리를 도와주기에
        //수고와 시간은 물론 실수도 덜어준다.
        using (var fileStream = CreateEmpty(filepath)) //새로운 파일을 만드는것
        {
            ConvertAndWrite(fileStream, clip);

            WriteHeader(fileStream, clip);
        }
        return true; //TODO : 파일 저장에 실패하면 false를 반환
        //<<FileStream>>
        //FileStream = 파일 입출력
        //매개체 클래스를 통해 접근하여 읽고, 쓸 수 있다.
        //일반적으로 byte[]배열을 이용해 읽기/쓰기를 수행

        //ex) FileStream fs = new FileStream("파일 이름", FileMode.OpenOrCreate)
        //'파일을 열 되 없으면 생성하라' 라는 뜻
        // 읽거나 써라~
    }

    public static AudioClip TrimSilence(AudioClip clip, float min)//침묵 다듬기? 
    {
        var samples = new float[clip.samples];
        //클립의 샘플 데이터로 배열을 채운다.
        //샘플은 -1.0f ~ 1.0f범위의 부동 소수점
        //샘플수는 float 배열의 길이에 의해 결정됨.


        //<<Clip.GetData>>
        //offsetSamples매개 변수를 사용하여 클립의 특정 위치에서 읽기 시작
        //오프셋의 읽기 길이가 클립 길이보다 길면 읽기가 랩을 둘러싸고 클립의 시작부분에서 나머지 샘플을 읽는다.
        //압축된 오디오 파일의 경우 오디오 가져오기에서 로드유형이 불러와졌을시 압축 해제로 설정된 경우에만
        //샘플데이터를 검색할 수 있다.
        //그렇지 않은 경우 배열의 보든 샘플 값에 대해 '0'으로 반환
        clip.GetData(samples, 0);
        //클립의 데이터(샘플데이터)를 0번부터 가져온다.



        /////////////아래 함수의 매개변수를 리턴하는데 무슨뜻일까//////////////////////////
        return TrimSilence(new List<float>(samples), min, clip.channels, clip.frequency);
    }
    public static AudioClip TrimSilence(List<float> samples, float min, int channels, int hz)
    {
        return TrimSilence(samples, min, channels, hz, false, false);
        ///////////////아래의 함수 매개변수의 느낌인데 무엇인지 모르겠다./////////////////////
    }
    public static AudioClip TrimSilence(List<float> samples, float min, int channels, int hz, bool _3D, bool stream)
    {
        int i;

        for (i = 0; i < samples.Count; i++)
        {
            //샘플의 절대값이 min보다 클 경우 
            if (Mathf.Abs(samples[i]) > min)///////////////그래서 min이뭐지?//////////////////////
            {
                break;
            }
        }

        samples.RemoveRange(0, i); //min보다 큰 샘플은 삭제시킨다

        for (i = samples.Count - 1; i > 0; i--)
        {
            if (Mathf.Abs(samples[i]) > min)
            {
                break;
            }
        }

        samples.RemoveRange(i, samples.Count - i);

        //var clip = AudioClip.Crate("TempClip", samples.Count, channels, hz, _3D, stream)에서bool _3D매개 변수는 은 요즘 사용되지 않음.
        var clip = AudioClip.Create("TempClip", samples.Count, channels, hz, stream);
        //AudioClip.Create(클립이름, 샘플 프레임수, 프레임당 채널 수, 클립의 샘플 주파수, (_3D)오디오 클립이 3D로 출력됨,
        // 클립이 스트리밍되면 true,즉 pcmreadercallback이 즉석에서 데이터를 생성하는 경우이다)



        // <<Clip.SetData>>
        //참고로 샘플은 0.0f에서 1.0f의 float범위여야 한다.
        //제한 초과시 아티팩트를 리드할 수 없거나 정의되지 않은 동작을 일으킨다.
        //샘플 수는 float배열에 의해 결정함
        //클립의 임의의 위치에 쓰는 경우, offsetSamples를 사용
        //클립의 길이보다 오프셋 길이가 큰 경우 = 읽기는 한바퀴 돌고 나머지를 클립의 시작위치에서 샘플링

        //주의 : 압축된 오디오파일, 오디오 임포터에서 "Load Type"이 "Decompress on Load"로 설정되어 있을때
        //샘플데이터를 꺼내는 것 밖에 할 수 없다.
        clip.SetData(samples.ToArray(), 0);

        return clip;
        //-------------------------------여까지---------------------------------
    }


    static FileStream CreateEmpty(string filepath)
    {
        var fileStream = new FileStream(filepath, FileMode.Create);
        //FileMode.Create = 운영체제에서 새 파일을 만들도록 지정한다.
        //파일이 이미 있다면 덮어쓴다. Write권한이 필요하다
        //즉, 파일이 없으면 CreateNew를 사용하고, 파일이 있으면 Truncate를 사용하도록 요청하는것과 마찬가지
        //파일이 이미 있지만 숨김파일이면 UnauthorizedAccessException 예외가 throw된다.
        byte emptyByte = new byte();

        for (int i = 0; i < HEADER_SIZE; i++)
        {
            //파일스트림의 현재 위치에 바이트를 쓴다.
            fileStream.WriteByte(emptyByte);
        }
        return fileStream;
    }

    static void ConvertAndWrite(FileStream fileStream, AudioClip clip)
    {
        var samples = new float[clip.samples];

        clip.GetData(samples, 0);

        //float[]에서 Int16[], Int16[] 에서 Byte[]로 2단계 변환
        Int16[] intData = new Int16[samples.Length];

        //bytesData배열은 크기의 두배?
        //Int16에서 변환된 float은 2바이트이기 때문에 dataSource배열이다
        Byte[] bytesData = new Byte[samples.Length * 2];

        int rescaleFactor = 32767;//float를 Int16으로 변환 (Int16형식의 범위 -32767~32767)

        for (int i = 0; i < samples.Length; i++)
        {
            intData[i] = (short)(samples[i] * rescaleFactor);
            Byte[] byteArr = new Byte[2];

            //지정된 데이터를 바이트 배열로 변환
            byteArr = BitConverter.GetBytes(intData[i]);
            //현재 1차원 배열의 모든 요소를 지정된 1차원 배열에 복사합니다.
            byteArr.CopyTo(bytesData, i * 2);
        }
        fileStream.Write(bytesData, 0, bytesData.Length);
    }

    static void WriteHeader(FileStream fileStream, AudioClip clip)
    {
        var hz = clip.frequency;//44100 1초동안 소리를 몇개의 조각으로 쪼개서 저장하는가
        var channels = clip.channels;//채널 
        var samples = clip.samples;//샘플

        fileStream.Seek(0, SeekOrigin.Begin);
        //이 스트림의 현재 위치를 지정된 값으로 설정
        //Seek( 이동할 바이트 크기, 검색을 시작할 상대적 시점 )
        //[ex] filestream.Seek(5, SeekOrigin.Current) > 현재의 위치( SeekOrigin.Current 에서 5바이트 뒤로 이동 )

        //wave파일에 대한 고정 값 
        //0\(null)로 끝나는 문자열이 아니다
        Byte[] riff = System.Text.Encoding.UTF8.GetBytes("RIFF");
        fileStream.Write(riff, 0, 4); //4바이트이고 RIFF를 ASCII코드로 나타냄

        //나머지 부분에 대한 Size
        //파일 전체 사이즈에서, 위에 'Chunk ID'와 자기 자신인 'Chunk Size'를 제외한 값
        //그래서 (전체 파일 크기 -8byte) 가 된다. (Format 4byte)
        //Little Endian 값이니 파일 사이즈가 0X00000010일 경우 메모리에는 10 00 00 00으로 지정되어 있다.
        Byte[] chunkSize = BitConverter.GetBytes(fileStream.Length - 8);
        fileStream.Write(chunkSize, 0, 4);

        //파일 형식을 나타낸다.
        //wave파일인 경우, 'WAVE'라는 문자가 ASCII코드로 들어간다
        //wave파일에 대한 고정값
        Byte[] wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
        fileStream.Write(wave, 0, 4);

        //'fmt '라는 고정값이 들어간다
        Byte[] fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
        fileStream.Write(fmt, 0, 4);

        //현재 Header에서 뒤에 이어지는 값들의 사이즈
        //즉, 현제 FMT파트 헤더의 전체 크기가 24byte인데, 다음에 이어지는 부분의 크기는 16이다
        //고정값이다 (Chunk ID, Chunk Size를 제외하면 이어지는 부분(2+2+4+4+2+2 = 16))
        Byte[] subChunk1 = BitConverter.GetBytes(16);
        fileStream.Write(subChunk1, 0, 4);

       // UInt16 two = 2;
        UInt16 one = 1;

        //고정값으로 1이 사용된다고 생각하면된다.
        //엄밀히 말하면 PCM인 경우인데, 대부분 wave파일은 PCM입니다.
        //Little Endian이므로 00 01이 아닌, 01 00입니다.
        Byte[] audioFormat = BitConverter.GetBytes(one);
        fileStream.Write(audioFormat, 0, 2);

        //음성파일의 채널 수
        //mono - 1, streo - 2....
        Byte[] numChannels = BitConverter.GetBytes(channels);
        fileStream.Write(numChannels, 0, 2);

        //Number of Sample Per Second , Hz단위
        //1초 동안의 소리를 몇개의 조각으로 쪼개서 저장(분석)하는가
        //10Hz = 0.1초 단위로 소리를 저장하겠다
        //숫자가 커질수록 음질이 좋아진다.
        Byte[] sampleRate = BitConverter.GetBytes(hz);
        fileStream.Write(sampleRate, 0, 4);



        //1초 동안 소리내는데 필요한 byte수 
        //(ex) sampleRate = 441000, mono채널이라면
        // 1초 재생에 필요한 byte수 =
        // (Sample 1개가 차지하는 byte) * (1초당 Sample수) * (채널 수)
        // = (Sample 1개가 차지하는 byte) * 441000(sampleRate) * 1(모노)
        // 
        // 그렇다면 Sample 1개가 차지하는 byte가 무엇일까?
        // => 'Bits Per Sample'
        //sampleRate * bytesPerSample * number of channels, here 44100*2*2
        Byte[] byteRate = BitConverter.GetBytes(hz * channels * 2);
        fileStream.Write(byteRate, 0, 4);


        //Sample Frame의 크기
        //sample1개 크기가 아니라 sample Frame의 크기
        //mono = sample크기 * 1, streo = sample크기 * 2;
        UInt16 blockAlign = (ushort)(channels * 2);
        fileStream.Write(BitConverter.GetBytes(blockAlign), 0, 2);

        
        //sample 한 개를 몇 bit로 나타낼 것이냐 = Bit Depth
        //쉽게 말해서 '도레미파솔라시도'는 음이 8가지!  이 경우는 2의 3승(8)이고 비트로 나타내면 3이 된다.
        //이 값이 8 이라면 , 2의 8승인 256. 순간의 소리를 256레벨로 표현한다는 것
        //이 값이 16이라면 2의 16승개의 레벨로 순간의 음을 나타내겟다는 것
        //이 값이 클수록 당연히 음질이 좋아진다.
        UInt16 bps = 16;
        Byte[] bitsPerSample = BitConverter.GetBytes(bps);
        fileStream.Write(bitsPerSample, 0, 2);

        //고정값
        //'data'라는 문자가 ASCII로 들어가 있다
        Byte[] datastring = System.Text.Encoding.UTF8.GetBytes("data");
        fileStream.Write(datastring, 0, 4);


        //뒤이어 나올 data의 size
        //즉, 소리 정보가 들어있는 data의 실제 size라고 생각
        //파일 전체 크기에서 header를 제외한 크기
        Byte[] subChunk2 = BitConverter.GetBytes(samples * channels * 2);
        fileStream.Write(subChunk2, 0, 4);
    }
  
}
