using SPVR.Model;
using SPVR.Network;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 본 예제는 최대 4명의 동시접속을 기준으로 제작
/// </summary>
public class ExampleManager : RoomBehaviour, ILobbyCallback
{
    Animator animatorPlay= null;
    Animator animatorParam = null;

    public Button openPlayBtn;
    public Button closePlayBtn;

    public Button openParamBtn;
    public Button closeParamBtn;

    void Start()
    {//버튼 클릭 시 기능 할당
        openPlayBtn.onClick.AddListener(() => BoxAnimatorControlWithPlayFunc(true));
        closePlayBtn.onClick.AddListener(() => BoxAnimatorControlWithPlayFunc(false));

        openParamBtn.onClick.AddListener(() => BoxAnimatorControlWithParam(true));
        closeParamBtn.onClick.AddListener(() => BoxAnimatorControlWithParam(false));
    }

    #region 서버 연결 후 로비상에서 방 참가
    public void OnJoinedLobby(string name)
    {//인터페이스 멤버 함수이기 때문에 구현 됨.
    }


    public void OnUpdateRoomList(List<IRoom> roomList)
    {
        if (SPVRNetwork.Instance?.CurrentRoom == null)
        {
            if (roomList.Count == 0)
                SPVRNetwork.Instance.CreateRoom("Room", 4, false);
            else
                SPVRNetwork.Instance.JoinRoom(roomList[0]);

            //서버에 룸이 없다면 "Room"이라는 룸을 생성, (첫번째 참가 플레이어)
            //서버에 룸이 존재 한다면 첫번 째 룸으로 입장 (이 후 참가 플레이어)
        }
    }

    public override void OnEnteredUser(IUser user)
    {//방에 접속 시 방 정보에 따라 플레이어 상태 설정
        if (SPVRNetwork.Instance != null)
        {
            if (user.Equals(SPVRNetwork.Instance.Me))
            {
                if (SPVRNetwork.Instance.CurrentRoom.IsPlaying)
                {
                    SPVRNetwork.Instance.Start();
                }
                else
                {
                    if (user.IsOwner)
                        SPVRNetwork.Instance.Start();
                    else
                        SPVRNetwork.Instance.Ready();
                }
            }
        }
    }
    #endregion

    public void BoxAnimatorControlWithPlayFunc(bool isOpen)
    {
        SPVRModel animatorTargetModel = animatorPlay.GetComponent<SPVRModel>();
        if (animatorTargetModel.OwnerID != SPVRNetwork.Instance.Me.Number)
        {
            animatorTargetModel.RequestGetOwner(success=>
            {//해당 SPVRModel에 대해 Owner가 될 수 있게 요청
                if(success)
                {
                    if (isOpen)
                        animatorPlay.Play("Open");
                    else
                        animatorPlay.Play("Close");
                }
            });
        }
        else
        {
            if (isOpen)
                animatorPlay.Play("Open");
            else
                animatorPlay.Play("Close");
        }
    }

    public void BoxAnimatorControlWithParam(bool isOpen)
    {
        SPVRModel animatorTargetModel = animatorParam.GetComponent<SPVRModel>();
        if (animatorTargetModel.OwnerID != SPVRNetwork.Instance.Me.Number)
        {
            animatorTargetModel.RequestGetOwner(success =>
            {//해당 SPVRModel에 대해 Owner가 될 수 있게 요청
                if (success)
                {
                    if (isOpen)
                        animatorParam.SetBool("Close", false);
                    else
                        animatorParam.SetBool("Close", true);
                }
            });
        }
        else
        {
            if (isOpen)
                animatorParam.SetBool("Close", false);
            else
                animatorParam.SetBool("Close", true);
        }
    }

    void Update()
    {
        if (ModelManager.Instance.ModelCount >= 2)
        {//애니메이터 할당
            if (animatorParam == null || animatorPlay == null)
            {
                Animator[] anims = FindObjectsOfType<Animator>();
                for (int i = 0; i < anims.Length; i++)
                {
                    print($"name : {anims[i].name}");
                    if (anims[i].name.Contains("BoxParam"))
                        animatorParam = anims[i];
                    if (anims[i].name.Contains("BoxP"))
                        animatorPlay = anims[i];
                }
            }
            return;
        }
        else
        {
            if (SPVRNetwork.Instance.CurrentRoom != null)
            {
                if (SPVRNetwork.Instance.CurrentRoom.Owner.UniqueID == SPVRNetwork.Instance.Me.UniqueID)
                {//동기화 될 모델 생성
                    ModelManager.Instance.CreateModelFromPrefab("BoxPlay", new Vector3(-5.5f, 1.75f, 5.35f), Quaternion.Euler(-35, 0, 0), Vector3.one);
                    ModelManager.Instance.CreateModelFromPrefab("BoxParam", new Vector3(5.5f, 1.75f, 5.35f), Quaternion.Euler(-35, 0, 0), Vector3.one);
                }
            }
        }
    }
}
