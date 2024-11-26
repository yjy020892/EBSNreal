using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SPVR.Network;
using SPVR.Core;
using System;
using Lean.Gui;

/// <summary>
/// 교실 컨트롤 클래스
/// </summary>
public class LobbyController : RoomBehaviour, ILobbyCallback
{
    //public Button createRoomButton;

    public GameObject roomPrefab;
    private List<GameObject> roomObjList = new List<GameObject>();
    public Transform trRoomListContent;
    public LeanButton createRoomButton;

    public Sprite roomNormalSprite;
    public Sprite roomSelectSprite;

    //private IRoom iRoom = null;

    //protected override void Awake()
    //{
    //    base.Awake();

    //    // 룸 생성 버튼을 눌렀을 경우 방을 생성합니다.
    //    // 룸 이름, 방에 입장할 최대 유저수, 비공개방 여부를 인자로 합니다.
    //    createRoomButton.OnDown.AddListener(() =>
    //    {
    //        Debug.Log("createRoomButton");
    //        SPVRNetwork.Instance?.CreateRoom($"ModelExample - {DateTime.Now}", 4, false);
    //    });
    //}

    public void OnUpdateRoomList(List<IRoom> roomList)
    {
        Debug.Log("OnUpdateRoomList()");

        // 유니티의 UI를 수정하거나 접근할때는 유니티스레드에서 처리되어야 합니다.
        // 콜백함수는 유니티스레드가 아니기 때문에 Dispatcher를 사용해서 유니티 스레드에서 처리되도록 합니다.
        Dispatcher.Instance?.Enqueue(() =>
        {
            //기존의 생성한 룸 리스트를 모두 제거합니다.
            foreach (Transform item in trRoomListContent.transform)
            {
                Destroy(item.gameObject);
            }

            //룸 리스트를 생성하고 버튼을 클릭하면 그 룸으로 입장합니다.
            foreach (var item in roomList)
            {
                GameObject room = Instantiate(roomPrefab, trRoomListContent);
                roomObjList.Add(room);
                room.GetComponentInChildren<Text>().text = item.Name;
                room.GetComponent<LeanButton>().OnDown.AddListener(() =>
                {
                    //OnClickRoom(room, item);
                    OnClickJoinRoom(item);
                });
            }
        });
    }

    // 룸 입장후 시작하기 전에 호출되는 콜백함수입니다. 룸에 대한 정보(룸에 입장한 유저 정보)를 표시할수 있습니다.
    public override void OnUpdateRoomInfo(IRoom roomInfo)
    {
        Debug.Log("OnUpdateRoomInfo()");
        Dispatcher.Instance?.Enqueue(() =>
        {
            foreach (Transform item in trRoomListContent.transform)
                Destroy(item.gameObject);

            //foreach (var item in roomInfo.Users)
            //{
            //    GameObject room = Instantiate(userPrefab, trRoomListContent);
            //    room.GetComponentInChildren<Text>().text = item.Name;
            //}
        });
    }

    public void OnClickRoom(GameObject roomObj, IRoom room)
    {
        if(!roomObj.GetComponent<Image>().sprite.Equals(roomSelectSprite))
        {
            //Debug.Log(roomObjList.Count);
            for (int i = 0; i < roomObjList.Count; i++)
            {
                roomObjList[i].gameObject.GetComponent<Image>().sprite = roomNormalSprite;
            }

            roomObj.GetComponent<Image>().sprite = roomSelectSprite;
            //iRoom = room;
        }
    }

    public void OnClickJoinRoom(IRoom iRoom)
    {
        if(iRoom != null)
        {
            SPVRNetwork.Instance?.JoinRoom(iRoom);
        }
    }

    public void OnJoinedLobby(string name)
    {

    }

    public void OnClickCreateRoom()
    {
        PlayerPrefs.SetString("HostID", SPVRNetwork.Instance.Me.UniqueID);
        //Debug.Log(PlayerPrefs.GetString("HostID", "None"));

        SPVRNetwork.Instance?.CreateRoom($"EBS화산 - {DateTime.Now}", 4, false);
    }

    /// <summary>
    /// 유저 입장시 호출되는 콜백함수 입니다.
    /// </summary>
    public override void OnEnteredUser(IUser user)
    {
        //Debug.Log("OnEnteredUser()");
        if (SPVRNetwork.Instance != null)
        {
            if (user.Equals(SPVRNetwork.Instance.Me))
            {
                // 이미 플레이 중인 룸으로 입장을 했다면 바로 시작합니다.
                if (SPVRNetwork.Instance.CurrentRoom.IsPlaying)
                {
                    SPVRNetwork.Instance?.Start();
                }
                else
                {
                    if (user.IsOwner)
                    {
                        SPVRNetwork.Instance.Start();
                    }
                    else
                    {
                        SPVRNetwork.Instance.Ready();
                    }
                }
            }
        }
    }

    /// <summary>
    /// SPVRNetwork.Instance.Start() 함수를 통해 시작되면 호출되는 콜백 함수 입니다.
    /// </summary>
    public override void OnStartedWorld()
    {
        Debug.Log("OnStartedWorld()");
        base.OnStartedWorld();

        Dispatcher.Instance?.Enqueue(() =>
        {
            StartCoroutine(LoadSceneToPlay());
        });
    }

    IEnumerator LoadSceneToPlay()
    {
        AsyncOperation async = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(1);
        async.allowSceneActivation = false;

        while (!async.isDone)
        {
            if (async.progress >= 0.9f)
            {
                async.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
