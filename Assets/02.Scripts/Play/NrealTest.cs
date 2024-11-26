using SPVR.Core;
using SPVR.Network;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SPVR.Model;
using System.IO;
using Lean.Gui;
using UnityEngine.Video;
using SPVR.Network.Controller;
using NRKernal;

/// <summary>
/// Play씬 컨트롤 클래스
/// </summary>
public class NrealTest : SyncBehaviour
{
    SPVRModel tvModel;
    SPVRModel volModel;

    public VideoPlayer tvVideoPlayer;
    public static NrealTest instance;

    private Camera centerCamera;
    private Camera leftCamera;
    private Camera rightCamera;
    private Camera worldCamera;
    private Button skipButton;

    public Transform mainCamera;
    public Transform modelPosition;
    public Transform objectSpawn;

    public GameObject narrationBObj;
    public GameObject[] narrationText;
    public GameObject SpawnTVUI;
    public GameObject SpawnVolUI;
    public GameObject[] charObjs;
    public GameObject exitUIObj;
    public GameObject exitUIPopupObj;
    public Transform volcanoTransform;

    [HideInInspector] public string volStepStr = string.Empty;

    public int tvFrame = 0;
    int narrationTextCnt = 0;

    #region BoolSet
    [HideInInspector] public bool b_Control = false;
    [HideInInspector] public bool b_Host = false;
    [HideInInspector] public bool b_Grab = false;
    [HideInInspector] public bool b_ChangeVolcano = false;
    [HideInInspector] public bool b_Section = false;
    [HideInInspector] public bool b_ChangeSection = false;
    [HideInInspector] public bool b_TV = false;
    private bool b_PlayTV = false;
    private bool b_StopTV = false;
    private bool b_StopTVHost = true;
    private bool b_SpawnPlayUI = false;
    private bool b_SpawnVolcanoSet = false;
    private bool b_SyncTVUI = false;
    private bool b_SyncVolcanoSet = false;
    private bool b_Click = false;
    private bool b_Narration = true;
    private bool b_Finish = false;
    #endregion

    void Awake()
    {
        if (NrealTest.instance == null)
        {
            NrealTest.instance = this;
        }
    }

    void Start()
    {
        mainCamera = GameObject.Find("NRCameraRig").GetComponent<Transform>();

        if (SPVRNetwork.Instance.Me.UniqueID.Equals(PlayerPrefs.GetString("HostID")))
        {
            narrationBObj.SetActive(true);
            b_Host = true;

            skipButton = GameObject.Find("Skip").GetComponent<Button>();
            skipButton.onClick.AddListener(() => {
                OnClickSkipNarration();
            });
        }
        else
        {
            GameObject obj = GameObject.Find("Skip");
            obj.SetActive(false);

            b_Host = false;
        }

#if UNITY_EDITOR
        centerCamera = GameObject.Find("CenterCamera").GetComponent<Camera>();

        centerCamera.cullingMask = ~(1 << 10 | 1 << 11);
        //OffLayerMask(centerCamera, 10);
        //OffLayerMask(centerCamera, 11);
#elif UNITY_ANDROID
        leftCamera = GameObject.Find("LeftCamera").GetComponent<Camera>();
        rightCamera = GameObject.Find("RightCamera").GetComponent<Camera>();

        //worldCamera = GameObject.Find("WorldCamera").GetComponent<Camera>(); //녹화기능 사용하면 풀기
        //worldCamera.cullingMask = ~(1 << 10 | 1 << 11);

        leftCamera.cullingMask = ~(1 << 10 | 1 << 11);
        rightCamera.cullingMask = ~(1 << 10 | 1 << 11);
        
        //OffLayerMask(leftCamera, rightCamera, 10);
        //OffLayerMask(leftCamera, rightCamera, 11);
#endif
    }

    /// <summary>
    /// cullingMask = -1은 everything과 같은 역할을 하며 기본 layer와, 추가한 모든 user layer를 활성화
    /// ~는 역 조건의 기능을 수행하는데 ~를 부착하게 되면 모든 Layer가 비활성화
    /// 1 << layerIndex는 해당하는 Index의 Layer만 활성화시키고 나머지는 전부 비활성화
    /// 반대로 ~(1 << layerIndex)는 Index의 Layer만 비활성화시키고 나머지는 전부 활성화
    /// </summary>
    /// <param name="cam">카메라</param>
    private void OffLayerMask(Camera cam, int layerIndex)
    {
        cam.cullingMask = ~(1 << layerIndex);
    }

    private void OffLayerMask(Camera cam1, Camera cam2, int layerIndex)
    {
        cam1.cullingMask = ~(1 << layerIndex);
        cam2.cullingMask = ~(1 << layerIndex);
    }

    void Update()
    {
        if(!exitUIObj.activeInHierarchy && b_Finish)
        {
            exitUIObj.SetActive(true);
            b_Finish = false;
        }

        if (b_Host)
        {
            if (b_TV)
            {
                if (b_StopTV && b_StopTVHost)
                {
                    tvModel.Remove();
                    b_Narration = true;
                    narrationBObj.SetActive(true);
                    narrationText[narrationTextCnt].SetActive(true);
                    b_StopTVHost = false;
                }
            }
        }
        else
        {
            if (b_SpawnPlayUI)
            {
                if (!b_SyncTVUI)
                {
                    b_SyncTVUI = true;
                    tvModel = ModelManager.Instance.CreateModelFromPrefab("Prefabs/UI/PlayUI", objectSpawn.position, Quaternion.identity, new Vector3(0.005f, 0.005f, 0.005f));
                    tvModel.gameObject.name = "PlayUI";
                    tvModel.transform.LookAt(mainCamera.position);
                    tvModel.transform.Rotate(new Vector3(0, 180, 0));
                    tvVideoPlayer = tvModel.transform.Find("TVFrame").GetComponent<VideoPlayer>();
                    //model.transform.SetParent(playUIModel.transform);
                    //model.transform.parent = playUIModel.transform;
                }
            }

            if (b_SpawnVolcanoSet)
            {
                if (!b_SyncVolcanoSet)
                {
                    b_SyncVolcanoSet = true;

                    volModel = ModelManager.Instance.CreateModelFromPrefab("Prefabs/Object/VolcanoSet", objectSpawn.position, Quaternion.identity, Vector3.one);
                    volModel.transform.LookAt(mainCamera.position);
                    volModel.gameObject.name = "VolcanoSet";
                    volcanoTransform = volModel.transform.Find("Volcano");
                }
            }

            if (b_TV && b_StopTVHost)
            {
                if (!b_PlayTV)
                {
                    b_PlayTV = true;
                    tvVideoPlayer.Play();
                    tvVideoPlayer.frame = tvFrame;
                }

                if (b_StopTV)
                {
                    b_TV = false;
                    tvModel.Remove();
                    b_StopTVHost = false;
                }
            }
            else
            {
                if (b_PlayTV)
                {
                    b_PlayTV = false;
                    if (tvVideoPlayer != null)
                    {
                        tvVideoPlayer.Pause();
                    }
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            Debug.Log("My number : " + SPVRNetwork.Instance.Me.Number);
            Debug.Log("My name : " + SPVRNetwork.Instance.Me.Name);
            Debug.Log("IsOwner : " + SPVRNetwork.Instance.Me.IsOwner);
            Debug.Log("IsReady : " + SPVRNetwork.Instance.Me.IsReady);
            Debug.Log("-----------------------------------------------------------------------------");
            Debug.Log("UniqueID : " + SPVRNetwork.Instance.CurrentRoom.Owner.UniqueID);
            Debug.Log("Name : " + SPVRNetwork.Instance.CurrentRoom.Owner.Name);
            Debug.Log("Number : " + SPVRNetwork.Instance.CurrentRoom.Owner.Number);
            Debug.Log("Others Count : " + SPVRNetwork.Instance.CurrentRoom.Others.Count);
            Debug.Log("Users Count : " + SPVRNetwork.Instance.CurrentRoom.Users.Count);
        }
    }

    public void SpawnTVUIListener()
    {
        tvModel = ModelManager.Instance.CreateModelFromPrefab("Prefabs/UI/PlayUI", objectSpawn.position, Quaternion.identity, new Vector3(0.005f, 0.005f, 0.005f));
        tvModel.gameObject.name = "PlayUI";
        tvModel.transform.LookAt(mainCamera.position);
        tvModel.transform.Rotate(new Vector3(0, 180, 0));
        tvVideoPlayer = tvModel.transform.Find("TVFrame").GetComponent<VideoPlayer>();
        //model.transform.SetParent(playUIModel.transform);
        //model.transform.parent = playUIModel.transform;
        SpawnTVUI.SetActive(false);

        b_SpawnPlayUI = true;
        SetDirty();
    }

    public void SpawnVolListener()
    {
        volModel = ModelManager.Instance.CreateModelFromPrefab("Prefabs/Object/VolcanoSet", objectSpawn.position, Quaternion.identity, Vector3.one);
        volModel.transform.LookAt(mainCamera.position);
        volModel.gameObject.name = "VolcanoSet";
        volcanoTransform = volModel.transform.Find("Volcano");
        //model.transform.SetParent(playUIModel.transform);
        //model.transform.parent = playUIModel.transform;
        SpawnVolUI.SetActive(false);

        b_SpawnVolcanoSet = true;
        SetDirty();
    }

    /// <summary>
    /// 로그아웃
    /// </summary>
    public void OnClickExitPop()
    {
        if(b_Click)
        {
            return;
        }

        b_Click = true;
        string str = EventSystem.current.currentSelectedGameObject.name;

        StartCoroutine(ExitPopup(str));
    }

    private IEnumerator ExitPopup(string str)
    {
        yield return new WaitForSeconds(0.5f);
        switch (str)
        {
            case "ExitButton":
                exitUIPopupObj.SetActive(true);
                exitUIObj.SetActive(false);
                break;

            case "Yes":
                QuitApplication();
                break;

            case "No":
                exitUIPopupObj.SetActive(false);
                exitUIObj.SetActive(true);
                break;
        }

        yield return new WaitForSeconds(0.5f);
        b_Click = false;
    }

    /// <summary>
    /// 종료(Nreal 홈으로)
    /// </summary>
    private void QuitApplication()
    {
        NRDevice.QuitApp();
    }

    /// <summary>
    /// 교육자 대사 스킵
    /// </summary>
    private void OnClickSkipNarration()
    {
        if(b_Click || !b_Narration)
        {
            return;
        }

        StartCoroutine(NarrationControl());
    }

    IEnumerator NarrationControl()
    {
        if (narrationTextCnt < narrationText.Length)
        {
            b_Click = true;
            if (narrationTextCnt.Equals(4))
            {
                yield return new WaitForSeconds(0.5f);
                SpawnTVUI.SetActive(true);
                b_Narration = false;
                narrationBObj.SetActive(false);
                //b_Click = false;
            }
            else if (narrationTextCnt.Equals(5))
            {
                yield return new WaitForSeconds(0.5f);
                volModel = ModelManager.Instance.CreateModelFromPrefab("Prefabs/Object/VolcanoSet", objectSpawn.position, Quaternion.identity, Vector3.one);
                volModel.transform.LookAt(mainCamera.position);
                volModel.gameObject.name = "VolcanoSet";
                volcanoTransform = volModel.transform.Find("Volcano");

                b_SpawnVolcanoSet = true;
                //b_Click = false;
                SetDirty();
            }
            else if(narrationTextCnt.Equals(7))
            {
                volcanoTransform.GetComponent<Volcano>().NarrationVolSection();
            }
            else if(narrationTextCnt.Equals(13))
            {
                volcanoTransform.GetComponent<Volcano>().NarrationVolStep("step1");
            }
            else if (narrationTextCnt.Equals(14))
            {
                volcanoTransform.GetComponent<Volcano>().NarrationVolStep("step2");
            }
            else if (narrationTextCnt.Equals(15))
            {
                volcanoTransform.GetComponent<Volcano>().NarrationVolStep("step3");
            }
            else if (narrationTextCnt.Equals(16))
            {
                volcanoTransform.GetComponent<Volcano>().NarrationVolStep("step4");
            }
            else if (narrationTextCnt.Equals(17))
            {
                volcanoTransform.GetComponent<Volcano>().NarrationVolStep("step5");
            }

            narrationText[narrationTextCnt].SetActive(false);
            narrationTextCnt = narrationTextCnt + 1;
            
            if (narrationTextCnt < narrationText.Length && !narrationTextCnt.Equals(5))
            {
                narrationBObj.SetActive(false);
                yield return new WaitForSeconds(0.3f);
                narrationBObj.SetActive(true);
                narrationText[narrationTextCnt].SetActive(true);
            }
            else
            {
                narrationBObj.SetActive(false);
            }

            if(narrationTextCnt.Equals(narrationText.Length))
            {
                yield return new WaitForSeconds(0.3f);
                b_Finish = true;
                SetDirty();
            }

            b_Click = false;
        }

        yield return null;
    }

    /// <summary>
    /// 동기화 상태값 넘기기
    /// </summary>
    public void PlayerState(string str, bool b)
    {
        switch (str)
        {
            case "PlayTV":
                b_TV = b;
                SetDirty();
                break;

            case "StopTV":
                b_StopTV = b;
                SetDirty();
                break;

            case "Control":
                b_Control = b;
                SetDirty();
                break;

            case "Grab":
                b_Grab = b;
                SetDirty();
                break;
        }
    }

    //public override void OnEnteredUser(IUser user)
    //{
    //    Debug.Log("---------------------OnEnteredUser---------------------");
    //    Debug.Log("user.Number : " + user.Number);
    //    Debug.Log("user.UniqueID : " + user.UniqueID);
    //    Debug.Log("user.Name : " + user.Name);
    //}

    //public override void OnLeftUser(IUser user)
    //{
    //    Debug.Log("---------------------OnLeftUser---------------------");
    //    Debug.Log("user.Number : " + user.Number);
    //    Debug.Log("user.UniqueID : " + user.UniqueID);
    //    Debug.Log("user.Name : " + user.Name);
    //}

    public override void SerializeView(BinaryWriter bw)
    {
        bw.Write(b_SpawnPlayUI);
        bw.Write(b_TV);
        bw.Write(tvFrame);
        bw.Write(b_StopTV);
        bw.Write(b_SpawnVolcanoSet);
        bw.Write(b_Control);
        bw.Write(b_Finish);
    }

    public override void DeserializeView(BinaryReader br)
    {
        b_SpawnPlayUI = br.ReadBoolean();
        b_TV = br.ReadBoolean();
        tvFrame = br.ReadInt32();
        b_StopTV = br.ReadBoolean();
        b_SpawnVolcanoSet = br.ReadBoolean();
        b_Control = br.ReadBoolean();
        b_Finish = br.ReadBoolean();
    }
}
