using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public static class SavWav
{
    const int HEADER_SIZE = 44;
    //RIFF, FMT, DATA������ ��� �������� 44����Ʈ�� ���´�.
    //�׷��� ��� ����� 44�ε��ϴ�.

    public static bool Save(string filename, AudioClip clip)
    {
        if (!filename.ToLower().EndsWith(".wav"))//�����̸��� .wav�� ������ �ʴ´ٸ�
        {
            filename += ".wav";
        }
        var filepath = Path.Combine(Application.persistentDataPath, filename);
        // >Path.Combine = ���ڿ��� ��η� �����Ѵ�
        // ex) Combine(String[]) - �ϳ��� ��η� ����
        //Combine (String, String) - �ΰ��� ���ڿ��� �ϳ��� ��η� ����
        //... (4������ ��밡���Ѱ����� ����)

        //Application.persistentDataPath = �Ʒ��� ���� ��ΰ� �����ȴ�.
        //On Win7 - C:/Users/Username/AppData/LocalLow/CompanyName/GameName
        //On Android - / Data / Data / com.companyname.gamename / Files

        Debug.Log(filepath);//���� ����� ���


        //���� ���丮�� �����ϴ� ��� ���丮�� �����ϴ��� Ȯ��.
        //�����Ѵٸ� ����
        //�������� �ʴ´ٸ� ������ �����. maybe?
        Directory.CreateDirectory(Path.GetDirectoryName(filepath));


        /////////////using�� �� ����Ͽ�����////////////
        //���������� using��
        //IDisposable��ü�� �ùٸ� ����� �����ϴ� ���� ������ �������ִ°��� �ٷ� using���̴�
        //File �� Font�� ���� Ŭ�������� �������� �ʴ� ���ҽ��� �׼��� �ϴ� ��ǥ���� Ŭ����
        //�� ���� �ش� Ŭ�������� �� ����� �Ŀ��� ������ �ñ⿡ '����(Dispose)�Ͽ� �ش� ���ҽ�(�ڿ�)�� �ٽ� 
        //'�ݳ�'�ؾ� �ϴ°�
        //������, �Ź� �������� �ʴ� ���ҽ��� �׼����ϴ� Ŭ������ üũ�ؼ� Dispose�ϴ°��� �ð��� �Ǽ��� �߱��Ѵ�.
        //�� �� using���� �̿��ϸ� �ش� ���ҽ� ������ ����� �� �� �ڵ����� Dispose�Ͽ� ������ �����ֱ⿡
        //����� �ð��� ���� �Ǽ��� �����ش�.
        using (var fileStream = CreateEmpty(filepath)) //���ο� ������ ����°�
        {
            ConvertAndWrite(fileStream, clip);

            WriteHeader(fileStream, clip);
        }
        return true; //TODO : ���� ���忡 �����ϸ� false�� ��ȯ
        //<<FileStream>>
        //FileStream = ���� �����
        //�Ű�ü Ŭ������ ���� �����Ͽ� �а�, �� �� �ִ�.
        //�Ϲ������� byte[]�迭�� �̿��� �б�/���⸦ ����

        //ex) FileStream fs = new FileStream("���� �̸�", FileMode.OpenOrCreate)
        //'������ �� �� ������ �����϶�' ��� ��
        // �аų� ���~
    }

    public static AudioClip TrimSilence(AudioClip clip, float min)//ħ�� �ٵ��? 
    {
        var samples = new float[clip.samples];
        //Ŭ���� ���� �����ͷ� �迭�� ä���.
        //������ -1.0f ~ 1.0f������ �ε� �Ҽ���
        //���ü��� float �迭�� ���̿� ���� ������.


        //<<Clip.GetData>>
        //offsetSamples�Ű� ������ ����Ͽ� Ŭ���� Ư�� ��ġ���� �б� ����
        //�������� �б� ���̰� Ŭ�� ���̺��� ��� �бⰡ ���� �ѷ��ΰ� Ŭ���� ���ۺκп��� ������ ������ �д´�.
        //����� ����� ������ ��� ����� �������⿡�� �ε������� �ҷ��������� ���� ������ ������ ��쿡��
        //���õ����͸� �˻��� �� �ִ�.
        //�׷��� ���� ��� �迭�� ���� ���� ���� ���� '0'���� ��ȯ
        clip.GetData(samples, 0);
        //Ŭ���� ������(���õ�����)�� 0������ �����´�.



        /////////////�Ʒ� �Լ��� �Ű������� �����ϴµ� �������ϱ�//////////////////////////
        return TrimSilence(new List<float>(samples), min, clip.channels, clip.frequency);
    }
    public static AudioClip TrimSilence(List<float> samples, float min, int channels, int hz)
    {
        return TrimSilence(samples, min, channels, hz, false, false);
        ///////////////�Ʒ��� �Լ� �Ű������� �����ε� �������� �𸣰ڴ�./////////////////////
    }
    public static AudioClip TrimSilence(List<float> samples, float min, int channels, int hz, bool _3D, bool stream)
    {
        int i;

        for (i = 0; i < samples.Count; i++)
        {
            //������ ���밪�� min���� Ŭ ��� 
            if (Mathf.Abs(samples[i]) > min)///////////////�׷��� min�̹���?//////////////////////
            {
                break;
            }
        }

        samples.RemoveRange(0, i); //min���� ū ������ ������Ų��

        for (i = samples.Count - 1; i > 0; i--)
        {
            if (Mathf.Abs(samples[i]) > min)
            {
                break;
            }
        }

        samples.RemoveRange(i, samples.Count - i);

        //var clip = AudioClip.Crate("TempClip", samples.Count, channels, hz, _3D, stream)����bool _3D�Ű� ������ �� ���� ������ ����.
        var clip = AudioClip.Create("TempClip", samples.Count, channels, hz, stream);
        //AudioClip.Create(Ŭ���̸�, ���� �����Ӽ�, �����Ӵ� ä�� ��, Ŭ���� ���� ���ļ�, (_3D)����� Ŭ���� 3D�� ��µ�,
        // Ŭ���� ��Ʈ���ֵǸ� true,�� pcmreadercallback�� �Ｎ���� �����͸� �����ϴ� ����̴�)



        // <<Clip.SetData>>
        //����� ������ 0.0f���� 1.0f�� float�������� �Ѵ�.
        //���� �ʰ��� ��Ƽ��Ʈ�� ������ �� ���ų� ���ǵ��� ���� ������ ����Ų��.
        //���� ���� float�迭�� ���� ������
        //Ŭ���� ������ ��ġ�� ���� ���, offsetSamples�� ���
        //Ŭ���� ���̺��� ������ ���̰� ū ��� = �б�� �ѹ��� ���� �������� Ŭ���� ������ġ���� ���ø�

        //���� : ����� ���������, ����� �����Ϳ��� "Load Type"�� "Decompress on Load"�� �����Ǿ� ������
        //���õ����͸� ������ �� �ۿ� �� �� ����.
        clip.SetData(samples.ToArray(), 0);

        return clip;
        //-------------------------------������---------------------------------
    }


    static FileStream CreateEmpty(string filepath)
    {
        var fileStream = new FileStream(filepath, FileMode.Create);
        //FileMode.Create = �ü������ �� ������ ���鵵�� �����Ѵ�.
        //������ �̹� �ִٸ� �����. Write������ �ʿ��ϴ�
        //��, ������ ������ CreateNew�� ����ϰ�, ������ ������ Truncate�� ����ϵ��� ��û�ϴ°Ͱ� ��������
        //������ �̹� ������ ���������̸� UnauthorizedAccessException ���ܰ� throw�ȴ�.
        byte emptyByte = new byte();

        for (int i = 0; i < HEADER_SIZE; i++)
        {
            //���Ͻ�Ʈ���� ���� ��ġ�� ����Ʈ�� ����.
            fileStream.WriteByte(emptyByte);
        }
        return fileStream;
    }

    static void ConvertAndWrite(FileStream fileStream, AudioClip clip)
    {
        var samples = new float[clip.samples];

        clip.GetData(samples, 0);

        //float[]���� Int16[], Int16[] ���� Byte[]�� 2�ܰ� ��ȯ
        Int16[] intData = new Int16[samples.Length];

        //bytesData�迭�� ũ���� �ι�?
        //Int16���� ��ȯ�� float�� 2����Ʈ�̱� ������ dataSource�迭�̴�
        Byte[] bytesData = new Byte[samples.Length * 2];

        int rescaleFactor = 32767;//float�� Int16���� ��ȯ (Int16������ ���� -32767~32767)

        for (int i = 0; i < samples.Length; i++)
        {
            intData[i] = (short)(samples[i] * rescaleFactor);
            Byte[] byteArr = new Byte[2];

            //������ �����͸� ����Ʈ �迭�� ��ȯ
            byteArr = BitConverter.GetBytes(intData[i]);
            //���� 1���� �迭�� ��� ��Ҹ� ������ 1���� �迭�� �����մϴ�.
            byteArr.CopyTo(bytesData, i * 2);
        }
        fileStream.Write(bytesData, 0, bytesData.Length);
    }

    static void WriteHeader(FileStream fileStream, AudioClip clip)
    {
        var hz = clip.frequency;//44100 1�ʵ��� �Ҹ��� ��� �������� �ɰ��� �����ϴ°�
        var channels = clip.channels;//ä�� 
        var samples = clip.samples;//����

        fileStream.Seek(0, SeekOrigin.Begin);
        //�� ��Ʈ���� ���� ��ġ�� ������ ������ ����
        //Seek( �̵��� ����Ʈ ũ��, �˻��� ������ ����� ���� )
        //[ex] filestream.Seek(5, SeekOrigin.Current) > ������ ��ġ( SeekOrigin.Current ���� 5����Ʈ �ڷ� �̵� )

        //wave���Ͽ� ���� ���� �� 
        //0\(null)�� ������ ���ڿ��� �ƴϴ�
        Byte[] riff = System.Text.Encoding.UTF8.GetBytes("RIFF");
        fileStream.Write(riff, 0, 4); //4����Ʈ�̰� RIFF�� ASCII�ڵ�� ��Ÿ��

        //������ �κп� ���� Size
        //���� ��ü �������, ���� 'Chunk ID'�� �ڱ� �ڽ��� 'Chunk Size'�� ������ ��
        //�׷��� (��ü ���� ũ�� -8byte) �� �ȴ�. (Format 4byte)
        //Little Endian ���̴� ���� ����� 0X00000010�� ��� �޸𸮿��� 10 00 00 00���� �����Ǿ� �ִ�.
        Byte[] chunkSize = BitConverter.GetBytes(fileStream.Length - 8);
        fileStream.Write(chunkSize, 0, 4);

        //���� ������ ��Ÿ����.
        //wave������ ���, 'WAVE'��� ���ڰ� ASCII�ڵ�� ����
        //wave���Ͽ� ���� ������
        Byte[] wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
        fileStream.Write(wave, 0, 4);

        //'fmt '��� �������� ����
        Byte[] fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
        fileStream.Write(fmt, 0, 4);

        //���� Header���� �ڿ� �̾����� ������ ������
        //��, ���� FMT��Ʈ ����� ��ü ũ�Ⱑ 24byte�ε�, ������ �̾����� �κ��� ũ��� 16�̴�
        //�������̴� (Chunk ID, Chunk Size�� �����ϸ� �̾����� �κ�(2+2+4+4+2+2 = 16))
        Byte[] subChunk1 = BitConverter.GetBytes(16);
        fileStream.Write(subChunk1, 0, 4);

       // UInt16 two = 2;
        UInt16 one = 1;

        //���������� 1�� ���ȴٰ� �����ϸ�ȴ�.
        //������ ���ϸ� PCM�� ����ε�, ��κ� wave������ PCM�Դϴ�.
        //Little Endian�̹Ƿ� 00 01�� �ƴ�, 01 00�Դϴ�.
        Byte[] audioFormat = BitConverter.GetBytes(one);
        fileStream.Write(audioFormat, 0, 2);

        //���������� ä�� ��
        //mono - 1, streo - 2....
        Byte[] numChannels = BitConverter.GetBytes(channels);
        fileStream.Write(numChannels, 0, 2);

        //Number of Sample Per Second , Hz����
        //1�� ������ �Ҹ��� ��� �������� �ɰ��� ����(�м�)�ϴ°�
        //10Hz = 0.1�� ������ �Ҹ��� �����ϰڴ�
        //���ڰ� Ŀ������ ������ ��������.
        Byte[] sampleRate = BitConverter.GetBytes(hz);
        fileStream.Write(sampleRate, 0, 4);



        //1�� ���� �Ҹ����µ� �ʿ��� byte�� 
        //(ex) sampleRate = 441000, monoä���̶��
        // 1�� ����� �ʿ��� byte�� =
        // (Sample 1���� �����ϴ� byte) * (1�ʴ� Sample��) * (ä�� ��)
        // = (Sample 1���� �����ϴ� byte) * 441000(sampleRate) * 1(���)
        // 
        // �׷��ٸ� Sample 1���� �����ϴ� byte�� �����ϱ�?
        // => 'Bits Per Sample'
        //sampleRate * bytesPerSample * number of channels, here 44100*2*2
        Byte[] byteRate = BitConverter.GetBytes(hz * channels * 2);
        fileStream.Write(byteRate, 0, 4);


        //Sample Frame�� ũ��
        //sample1�� ũ�Ⱑ �ƴ϶� sample Frame�� ũ��
        //mono = sampleũ�� * 1, streo = sampleũ�� * 2;
        UInt16 blockAlign = (ushort)(channels * 2);
        fileStream.Write(BitConverter.GetBytes(blockAlign), 0, 2);

        
        //sample �� ���� �� bit�� ��Ÿ�� ���̳� = Bit Depth
        //���� ���ؼ� '�������ļֶ�õ�'�� ���� 8����!  �� ���� 2�� 3��(8)�̰� ��Ʈ�� ��Ÿ���� 3�� �ȴ�.
        //�� ���� 8 �̶�� , 2�� 8���� 256. ������ �Ҹ��� 256������ ǥ���Ѵٴ� ��
        //�� ���� 16�̶�� 2�� 16�°��� ������ ������ ���� ��Ÿ���ٴٴ� ��
        //�� ���� Ŭ���� �翬�� ������ ��������.
        UInt16 bps = 16;
        Byte[] bitsPerSample = BitConverter.GetBytes(bps);
        fileStream.Write(bitsPerSample, 0, 2);

        //������
        //'data'��� ���ڰ� ASCII�� �� �ִ�
        Byte[] datastring = System.Text.Encoding.UTF8.GetBytes("data");
        fileStream.Write(datastring, 0, 4);


        //���̾� ���� data�� size
        //��, �Ҹ� ������ ����ִ� data�� ���� size��� ����
        //���� ��ü ũ�⿡�� header�� ������ ũ��
        Byte[] subChunk2 = BitConverter.GetBytes(samples * channels * 2);
        fileStream.Write(subChunk2, 0, 4);
    }
  
}
