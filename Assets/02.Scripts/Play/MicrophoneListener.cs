using SPVR.Model;
using SPVR.Network;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 마이크 출력량 체크
/// </summary>
public class MicrophoneListener : SyncBehaviour
{
    public GameObject soundStateObj;

    public AudioClip aud;

    int sampleRate = 44100;
    private float[] samples;
    public float rmsValue;
    public float modulate;
    public int resultvalue;
    public int cutValue;

    void Start()
    {
        samples = new float[sampleRate];
#if UNITY_EDITOR
#elif UNITY_ANDROID
        if (gameObject.name.Equals(SPVRNetwork.Instance.Me.Number.ToString()))
        {
            aud = Microphone.Start(Microphone.devices[0].ToString(), true, 1, sampleRate);
        }
#endif
    }

    void Update()
    {
#if UNITY_EDITOR
#elif UNITY_ANDROID
        if (gameObject.name.Equals(SPVRNetwork.Instance.Me.Number.ToString()))
        {
            aud.GetData(samples, 0);
            float sum = 0;
            for (int i = 0; i < samples.Length; i++)
            {
                sum += samples[i] * samples[i];
            }
            rmsValue = Mathf.Sqrt(sum / samples.Length);
            rmsValue = (rmsValue * modulate) * 0.7f;
            rmsValue = Mathf.Clamp(rmsValue, 0, 500);
            resultvalue = Mathf.RoundToInt(rmsValue);
        }
#endif
        //Debug.Log("rmsValue : " + resultvalue);

        if (resultvalue >= 500)
        {
            soundStateObj.SetActive(true);
        }
        else
        {
            soundStateObj.SetActive(false);
        }

        IsDirty = true;

        if (resultvalue < cutValue)
        {
            resultvalue = 0;
        }
    }

    public override void SerializeView(BinaryWriter bw)
    {
        bw.Write(resultvalue);
    }

    public override void DeserializeView(BinaryReader br)
    {
        resultvalue = br.ReadInt32();
    }
}
