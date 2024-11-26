using Lean.Transition;
using NRKernal;
using SPVR.Model;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Policy;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.EventSystems;

/// <summary>
/// 동영상 컨트롤
/// </summary>
public class ObjectControl : MonoBehaviour
{
    public RawImage screen = null;
    public VideoPlayer videoPlayer = null;
    public Slider videoSlider;

    public GameObject closeUIObj;

    private bool b_DragSlider = false;

    int hours, minute, second;

    bool b_SyncPlayTV = false;

    private void Start()
    {
        if (NrealTest.instance.b_Host && transform.root.name.Equals("PlayUI"))
        {
            //closeUIObj.SetActive(true);
        }

        if (gameObject.name.Equals("TVFrame"))
        {
            videoPlayer = GetComponent<VideoPlayer>();

            transform.localPosition = Vector3.zero;
            transform.localEulerAngles = Vector3.zero;

            StartCoroutine(PrepareVideo());
        }

        if(transform.root.name.Equals("PlayUI(Clone)"))
        {
            transform.root.gameObject.SetActive(false);
            //gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if(gameObject.name.Equals("TVFrame"))
        {
            if (videoPlayer != null && videoPlayer.isPlaying)
            {
                if (NrealTest.instance.b_Host)
                {
                    if (!b_DragSlider)
                    {
                        videoSlider.value = videoPlayer.frame;

                        if(videoSlider.value >= videoSlider.maxValue - 2)
                        {
                            videoPlayer.Stop();
                            NrealTest.instance.PlayerState("StopTV", true);
                            Debug.Log("Finish Host TV");
                        }

                        //if(videoSlider.value > videoPlayer.frameCount - 5)
                        //{
                        //    videoPlayer.Stop();
                        //    NrealTest.instance.PlayerState("StopTV", true);
                        //    Debug.Log("Finish Host TV");
                        //}

                        //int num = (int)videoPlayer.time;
                        //Debug.Log(string.Format("{0:D2}:{1:D2}", (int)videoPlayer.clip.length % 3600 / 60, (int)videoPlayer.clip.length % 3600 % 60));

                        //minute = num % 3600 / 60;
                        //second = num % 3600 % 60;

                        //Debug.Log(string.Format("{0:D2}:{1:D2}", minute, second));
                    }
                    //videoSlider.value = videoPlayer.clip.
                }
                else
                {
                    //Debug.Log("DHost Play Video");

                    if (!b_DragSlider)
                    {
                        videoSlider.value = videoPlayer.frame;
                    }
                }
            }
            else if(videoPlayer != null && !videoPlayer.isPlaying && videoPlayer.isPrepared)
            {
                //Debug.Log("!isPlaying TV");
                if(!videoSlider.maxValue.Equals(1))
                {
                    if (videoSlider.value >= videoSlider.maxValue - 2)
                    {
                        videoPlayer.Stop();
                        NrealTest.instance.PlayerState("StopTV", true);
                        Debug.Log("Finish Host TV");
                    }
                }
            }
            
            //if(!NrealTest.instance.b_Host)
            //{
            //    if (videoPlayer != null && !videoPlayer.isPlaying)
            //    {
            //        if(b_SyncPlayTV)
            //        {
            //            SPVRModel[] modelMyArray = ModelManager.Instance.GetModelsArray(ModelManager.ModelsArrayQuery.OwnerMe);

            //            Debug.Log("DHost modelArray : " + modelMyArray.Length);

            //            for (int i = 0; i < modelMyArray.Length; i++)
            //            {
            //                Debug.Log(modelMyArray[i].gameObject.name);
            //                if (modelMyArray[i].gameObject.name.Equals("TVFrame"))
            //                {
            //                    modelMyArray[i].gameObject.name = "dhostTVFrame";
            //                }
            //            }
            //            //videoPlayer.Play();
            //        }
            //    }
            //    else if(videoPlayer != null && videoPlayer.isPlaying)
            //    {
            //        //videoSlider.value = videoPlayer.frame;
            //    }
            //}
        }
    }
    
    IEnumerator PrepareVideo()
    {
        videoPlayer.Prepare();

        while(!videoPlayer.isPrepared)
        {
            yield return new WaitForSeconds(0.5f);
        }

        screen.texture = videoPlayer.texture;
        videoSlider.maxValue = (float)videoPlayer.frameCount/* - 1.0f*/;

        yield return new WaitForSeconds(0.5f);

        if (transform.root.name.Equals("PlayUI"))
        {
            if (videoPlayer != null && videoPlayer.isPrepared)
            {
                NrealTest.instance.b_TV = true;
                // 비디오 재생
                videoPlayer.Play();
            }
        }  
    }

    public void CloseVideo()
    {
        if (NrealTest.instance.b_Host)
        {
            NrealTest.instance.PlayerState("StopTV", true);
            videoPlayer.Stop();
        }
    }

    public void PlayVideo()
    {
        if(NrealTest.instance.b_Host)
        {
            if (videoPlayer != null && videoPlayer.isPrepared)
            {
                NrealTest.instance.PlayerState("PlayTV", true);
                // 비디오 재생
                videoPlayer.Play();
            }
        }
    }

    public void StopVideo()
    {
        if(NrealTest.instance.b_Host)
        {
            if (videoPlayer != null && videoPlayer.isPrepared)
            {
                NrealTest.instance.tvFrame = (int)videoSlider.value;
                NrealTest.instance.PlayerState("PlayTV", false);
                // 비디오 멈춤
                videoPlayer.Pause();
            }
        }
    }

    public void DragSlider()
    {
        if(NrealTest.instance.b_Host)
        {
            b_DragSlider = true;
            videoPlayer.frame = (int)videoSlider.value;
            NrealTest.instance.PlayerState("PlayTV", false);
        }
    }

    public void EndDragSlider()
    {
        if(NrealTest.instance.b_Host)
        {
            b_DragSlider = false;
            videoPlayer.frame = (int)videoSlider.value;
            NrealTest.instance.tvFrame = (int)videoSlider.value;
            videoPlayer.Play();
            NrealTest.instance.PlayerState("PlayTV", true);
        }
    }
}
