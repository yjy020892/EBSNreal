using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SPVR.Model;
using SPVR.Network;
using NRKernal;
using NRKernal.Record;

public enum LobbyState
{
    None,
    Ready,
    Job,
    Sex,
    Character,
    Name,
    Room,
}

public enum CharacterSet
{
    None,
    HairStyle,
    HairColor,
    HairAcc,
    FaceAcc,
    Top,
}

public struct CharacterInfo
{
    public static string Job { get; set; }
    public static string Sex { get; set; }
    public static int HairStyle { get; set; }
    public static int HairColor { get; set; }
    public static int HairAcc { get; set; }
    public static int FaceAcc { get; set; }
    public static int TopStyle { get; set; }
    public static int TopShape { get; set; }
    public static string Name { get; set; }
}

/// <summary>
/// Lobby, Play씬에서 전반적으로 아바타 설정을 담당
/// </summary>
public class SetCharacter : MonoBehaviour
{
    WaitForSeconds secCo = new WaitForSeconds(0.5f);

    public LobbyState lobbyState;
    CharacterSet characterSet = CharacterSet.None;

    #region public
    [Header("UI")]
    public GameObject readyUI;
    public GameObject jobUI;
    public GameObject sexUI;
    public GameObject roomUI;
    public GameObject characterUI;
    public GameObject nameUI;
    public GameObject[] allUI;
    public GameObject popupUI;
    public RectTransform leftArrow;
    public RectTransform rightArrow;
    public InputField nameInput;
    public GameObject createRoomUI;

    [Header("Character Object")]
    public GameObject studentManObj;
    public GameObject studentWomanObj;

    [Header("Student(Boy)")]
    public GameObject[] boyHairStyle;
    public Texture2D[] boyHairColorsTexture;
    public GameObject[] boyHairAcc;
    public GameObject[] boyFaceAcc;
    public Texture2D[] boyTopTexture;
    public GameObject[] boyTopStyle;

    [Header("Student(Girl)")]
    public GameObject[] girlHairStyle;
    public Texture2D[] girlHairColorsTexture;
    public GameObject[] girlHairAcc;
    public GameObject[] girlFaceAcc;
    public Texture2D[] girlTopTexture;
    public GameObject[] girlTopStyle;

    [Header("Play")]
    public Transform[] spawnPosition;
    #endregion

    private GameObject m_CameraTarget;

    private SPVRModel model;
    private MeshRenderer studentRenderer;

    private Vector3 characterPosi = Vector3.zero;
    private Vector3 characterSize;

    private bool b_Click = false;
    private bool b_Check = false;
    private bool b_SpawnCharacter = false;

    private float timer = 0.0f;
    private int index = 0;
    private int topIndex = 0;

    void Start()
    {
        characterSize = new Vector3(0.2f, 0.2f, 0.2f);

        if (GameObject.Find("CameraTarget") == null)
        {
            m_CameraTarget = new GameObject("CameraTarget");
            m_CameraTarget.transform.position = Vector3.zero;
            m_CameraTarget.transform.rotation = Quaternion.identity;
            DontDestroyOnLoad(m_CameraTarget);

            GameObject.Find("NRCameraRig").GetComponent<Transform>().parent = m_CameraTarget.transform;

#if UNITY_EDITOR
#elif UNITY_ANDROID
            // REC
            //Transform trans = GameObject.Find("NRRecorderBehaviour(Clone)").GetComponent<Transform>();
            //trans.position = new Vector3(trans.position.x, trans.position.y - 0.35f, trans.position.z); 
            //trans.parent = m_CameraTarget.transform;
#endif
        }
        else
        {
            m_CameraTarget = GameObject.Find("CameraTarget");
        }

        if (SceneManager.GetActiveScene().name.Equals("Lobby"))
        {
            CharacterInfoInit();
        }
        else if (SceneManager.GetActiveScene().name.Equals("Play"))
        {
            CreateCharacter();
        }
    }

    void CharacterInfoInit()
    {
        lobbyState = LobbyState.Ready;

        CharacterInfo.HairStyle = 0;
        CharacterInfo.HairColor = 0;
        CharacterInfo.HairAcc = -1;
        CharacterInfo.FaceAcc = -1;
        CharacterInfo.TopStyle = 0;
        CharacterInfo.TopShape = 0;

        characterSet = CharacterSet.HairStyle;
        index = CharacterInfo.HairStyle;

        leftArrow.anchoredPosition = new Vector2(leftArrow.anchoredPosition.x, 0.0f);
        rightArrow.anchoredPosition = new Vector2(rightArrow.anchoredPosition.x, 0.0f);
    }

    /// <summary>
    /// 아바타 생성
    /// </summary>
    void CreateCharacter()
    {
        Pose pose = NRFrame.HeadPose;

        if (SPVRNetwork.Instance.Me.Number.Equals(1))
        {
            characterPosi = spawnPosition[0].position;
        }
        else if (SPVRNetwork.Instance.Me.Number.Equals(2))
        {
            characterPosi = spawnPosition[1].position;
        }
        else if (SPVRNetwork.Instance.Me.Number.Equals(3))
        {
            characterPosi = spawnPosition[2].position;
        }
        else if (SPVRNetwork.Instance.Me.Number.Equals(4))
        {
            characterPosi = spawnPosition[3].position;
        }

        if (CharacterInfo.Sex.Equals("Man"))
        {
            //NrealTest.instance.mainCamera.position = characterPosi;
            if(CharacterInfo.Job.Equals("Teacher"))
            {
                model = ModelManager.Instance.CreateModelFromPrefab("Prefabs/Teacher/Teacher_Man", Vector3.zero, Quaternion.identity, Vector3.one);
            }
            else if(CharacterInfo.Job.Equals("Student"))
            {
                model = ModelManager.Instance.CreateModelFromPrefab("Prefabs/Student/Student_Man", Vector3.zero, Quaternion.identity, Vector3.one);
            }
            
            model.name = string.Format("{0}", SPVRNetwork.Instance.Me.Number);
            Transform modelTrans = model.transform;

            modelTrans.position = characterPosi;
            //model.transform.parent = NrealTest.instance.mainCamera;

            //m_CameraTarget.transform.position = characterPosi;
            m_CameraTarget.transform.position = new Vector3(characterPosi.x, characterPosi.y + 0.5f, characterPosi.z);

            //m_CameraTarget.transform.position = pose.position;
            NREmulatorManager.Instance?.NativeEmulatorApi?.SetHeadTrackingPose(characterPosi, Quaternion.identity);
            //pose.position = m_CameraTarget.transform.position;

            b_SpawnCharacter = true;
        }
        else if (CharacterInfo.Sex.Equals("Woman"))
        {
            if (CharacterInfo.Job.Equals("Teacher"))
            {
                model = ModelManager.Instance.CreateModelFromPrefab("Prefabs/Teacher/Teacher_Woman", Vector3.zero, Quaternion.identity, Vector3.one);
            }
            else if (CharacterInfo.Job.Equals("Student"))
            {
                model = ModelManager.Instance.CreateModelFromPrefab("Prefabs/Student/Student_Woman", Vector3.zero, Quaternion.identity, Vector3.one);
            }
            
            model.name = string.Format("{0}", SPVRNetwork.Instance.Me.Number);
            Transform modelTrans = model.transform;

            modelTrans.position = characterPosi;
            //m_CameraTarget.transform.position = characterPosi;
            m_CameraTarget.transform.position = new Vector3(characterPosi.x, characterPosi.y + 0.5f, characterPosi.z);
            NREmulatorManager.Instance?.NativeEmulatorApi?.SetHeadTrackingPose(characterPosi, Quaternion.identity);

            b_SpawnCharacter = true;
        }
    }

    void Update()
    {
        if (b_SpawnCharacter)
        {
            Pose pose = NRFrame.HeadPose;

            //m_CameraTarget.transform.position = pose.position;
            //NREmulatorManager.Instance?.NativeEmulatorApi?.SetHeadTrackingPose(m_CameraTarget.transform.position, Quaternion.identity);
            pose.position = m_CameraTarget.transform.position;
            //Debug.Log(pose.position);
            //Debug.Log(pose.rotation);
            //model.transform.position = pose.position;

            //model.transform.position = m_CameraTarget.transform.position;

            model.transform.position = new Vector3(pose.position.x, pose.position.y - 0.5f, pose.position.z);
            model.transform.rotation = pose.rotation;
            //model.transform.rotation = m_CameraTarget.transform.rotation;
        }

        if (SceneManager.GetActiveScene().name.Equals("Lobby"))
        {
            if (!b_Check)
            {
                switch (lobbyState)
                {
                    case LobbyState.Ready:
                        b_Check = true;

                        StartCoroutine(ReadyTime());
                        readyUI.SetActive(true);
                        break;

                    case LobbyState.Job:
                        b_Check = true;

                        jobUI.SetActive(true);
                        break;

                    case LobbyState.Sex:
                        b_Check = true;

                        sexUI.SetActive(true);
                        break;

                    case LobbyState.Character:
                        b_Check = true;

                        if (CharacterInfo.Job.Equals("Teacher"))
                        {
                            CharacterInfo.Name = "선생님";
                            lobbyState = LobbyState.Room;
                            b_Check = false;
                        }
                        else if (CharacterInfo.Job.Equals("Student"))
                        {
                            if (CharacterInfo.Sex.Equals("Man"))
                            {
                                studentManObj.SetActive(true);
                            }
                            else if (CharacterInfo.Sex.Equals("Woman"))
                            {
                                studentWomanObj.SetActive(true);
                            }

                            characterUI.SetActive(true);
                            //CharacterSetState("Next");
                        }
                        break;

                    case LobbyState.Name:
                        b_Check = true;

                        nameUI.SetActive(true);
                        break;

                    case LobbyState.Room:
                        b_Check = true;

                        roomUI.SetActive(true);

                        if (CharacterInfo.Job.Equals("Student"))
                        {
                            createRoomUI.SetActive(false);
                        }
                        break;
                }
            }
        }
    }

    IEnumerator ReadyTime()
    {
        while(true)
        {
            timer += Time.deltaTime;

            if (timer >= 5.0f)
            {
                readyUI.SetActive(false);

                yield return secCo;

                lobbyState = LobbyState.Job;

                b_Check = false;
                break;
            }

            yield return null;
        }
        
    }

    public void Onlickbutton()
    {
        if (b_Click)
        {
            return;
        }

        b_Click = true;
        string str = EventSystem.current.currentSelectedGameObject.name;

        StartCoroutine(OnClickContents(str));
    }

    public IEnumerator OnClickContents(string str)
    {
        switch (str)
        {
            #region Teacher
            case "Teacher":
                yield return secCo;
                jobUI.SetActive(false);
                lobbyState = LobbyState.Sex;
                CharacterInfo.Job = "Teacher";

                yield return secCo;
                b_Check = false;
                break;
            #endregion

            #region Student
            case "Student":
                yield return secCo;
                jobUI.SetActive(false);
                lobbyState = LobbyState.Sex;
                CharacterInfo.Job = "Student";

                yield return secCo;
                b_Check = false;
                break;
            #endregion

            #region Man
            case "Man":
                yield return secCo;
                sexUI.SetActive(false);
                lobbyState = LobbyState.Character;
                CharacterInfo.Sex = "Man";

                yield return secCo;
                b_Check = false;
                break;
            #endregion

            #region Woman
            case "Woman":
                yield return secCo;
                sexUI.SetActive(false);
                lobbyState = LobbyState.Character;
                CharacterInfo.Sex = "Woman";

                yield return secCo;
                b_Check = false;
                break;
            #endregion

            #region Name
            case "NameOK":
                yield return secCo;
                if(!string.IsNullOrEmpty(nameInput.text))
                {
                    CharacterInfo.Name = nameInput.text;
                }
                else
                {
                    CharacterInfo.Name = "학습자";
                }

                nameUI.SetActive(false);
                lobbyState = LobbyState.Room;
                b_Check = false;
                break;
            #endregion

            #region Select
            case "Select":
                yield return secCo;
                for (int i = 0; i < allUI.Length; i++)
                {
                    allUI[i].SetActive(false);
                }

                if (CharacterInfo.Job.Equals("Student"))
                {
                    if (CharacterInfo.Sex.Equals("Man"))
                    {
                        studentManObj.SetActive(false);

                    }
                    else
                    {
                        studentWomanObj.SetActive(false);
                    }
                }

                popupUI.SetActive(true);
                break;
            #endregion

            #region YES OR NO
            case "Yes":
                yield return secCo;
                characterUI.SetActive(false);
                lobbyState = LobbyState.Name;
                b_Check = false;
                //roomUI.SetActive(true);
                break;

            case "No":
                yield return secCo;
                for (int i = 0; i < allUI.Length; i++)
                {
                    allUI[i].SetActive(true);
                }

                if (CharacterInfo.Job.Equals("Student"))
                {
                    if (CharacterInfo.Sex.Equals("Man"))
                    {
                        studentManObj.SetActive(true);
                    }
                    else
                    {
                        studentWomanObj.SetActive(true);
                    }
                }

                popupUI.SetActive(false);
                break;
                #endregion
        }

        b_Click = false;
    }

    public void CharacterSetState()
    {
        string str = EventSystem.current.currentSelectedGameObject.name;

        switch (str)
        {
            case "HairStyle":
                leftArrow.anchoredPosition = new Vector2(leftArrow.anchoredPosition.x, 0.0f);
                rightArrow.anchoredPosition = new Vector2(rightArrow.anchoredPosition.x, 0.0f);
                characterSet = CharacterSet.HairStyle;
                index = CharacterInfo.HairStyle;
                break;

            case "HairColor":
                leftArrow.anchoredPosition = new Vector2(leftArrow.anchoredPosition.x, 0.0f);
                rightArrow.anchoredPosition = new Vector2(rightArrow.anchoredPosition.x, 0.0f);
                characterSet = CharacterSet.HairColor;
                index = CharacterInfo.HairColor;
                break;

            case "HairAcc":
                leftArrow.anchoredPosition = new Vector2(leftArrow.anchoredPosition.x, 0.0f);
                rightArrow.anchoredPosition = new Vector2(rightArrow.anchoredPosition.x, 0.0f);
                //boyHairAcc[0].transform.GetChild(0).GetComponent<MeshRenderer>().material.SetTexture("_MainTex", boyHairColorsTexture[CharacterInfo.hairColor]);
                characterSet = CharacterSet.HairAcc;
                index = CharacterInfo.HairAcc;
                break;

            case "FaceAcc":
                leftArrow.anchoredPosition = new Vector2(leftArrow.anchoredPosition.x, -23.0f);
                rightArrow.anchoredPosition = new Vector2(rightArrow.anchoredPosition.x, -23.0f);
                characterSet = CharacterSet.FaceAcc;
                index = CharacterInfo.FaceAcc;
                break;

            case "Top":
                leftArrow.anchoredPosition = new Vector2(leftArrow.anchoredPosition.x, -75.0f);
                rightArrow.anchoredPosition = new Vector2(rightArrow.anchoredPosition.x, -75.0f);
                characterSet = CharacterSet.Top;
                topIndex = CharacterInfo.TopStyle;
                index = CharacterInfo.TopShape;
                break;

            default:
                break;
        }
    }

    public void OnClickCharacterArrow()
    {
        string str = EventSystem.current.currentSelectedGameObject.name;

        switch (str)
        {
            #region LeftArrow
            case "LeftArrow":
                #region HairStyle
                if (characterSet.Equals(CharacterSet.HairStyle))
                {
                    if (CharacterInfo.Sex.Equals("Man"))
                    {
                        boyHairAcc[0].SetActive(false);

                        if (!CharacterInfo.HairAcc.Equals(-1))
                        {
                            boyHairAcc[CharacterInfo.HairAcc].SetActive(false);
                        }

                        boyHairStyle[index].SetActive(false);

                        if (index.Equals(0))
                        {
                            index = boyHairStyle.Length;
                        }

                        boyHairStyle[index - 1].SetActive(true);
                        index--;

                        CharacterInfo.HairStyle = index;
                        studentRenderer = boyHairStyle[index].transform.GetChild(0).GetComponent<MeshRenderer>();
                    }
                    else if (CharacterInfo.Sex.Equals("Woman"))
                    {
                        girlHairAcc[0].SetActive(false);
                        girlHairAcc[1].SetActive(false);

                        if (!CharacterInfo.HairAcc.Equals(-1))
                        {
                            girlHairAcc[CharacterInfo.HairAcc].SetActive(false);
                        }

                        girlHairStyle[index].SetActive(false);

                        if (index.Equals(0))
                        {
                            index = girlHairStyle.Length;
                        }

                        girlHairStyle[index - 1].SetActive(true);
                        index--;

                        CharacterInfo.HairStyle = index;
                        studentRenderer = girlHairStyle[index].transform.GetChild(0).GetComponent<MeshRenderer>();
                    }
                }
                #endregion
                #region HairColor
                else if (characterSet.Equals(CharacterSet.HairColor))
                {
                    if (CharacterInfo.Sex.Equals("Man"))
                    {
                        for (int i = 0; i < boyHairStyle.Length; i++)
                        {
                            studentRenderer = boyHairStyle[i].transform.GetChild(0).GetComponent<MeshRenderer>();

                            if (studentRenderer.material.mainTexture.name.Equals(boyHairColorsTexture[0].name))
                            {
                                index = boyHairColorsTexture.Length;
                            }

                            studentRenderer.material.SetTexture("_MainTex", boyHairColorsTexture[index - 1]);

                            boyHairAcc[0].transform.GetChild(0).
                        GetComponent<MeshRenderer>().material.SetTexture("_MainTex", boyHairColorsTexture[index - 1]);
                        }

                        index--;
                        CharacterInfo.HairColor = index;
                    }
                    else if (CharacterInfo.Sex.Equals("Woman"))
                    {
                        for (int i = 0; i < girlHairStyle.Length; i++)
                        {
                            studentRenderer = girlHairStyle[i].transform.GetChild(0).GetComponent<MeshRenderer>();

                            if (studentRenderer.material.mainTexture.name.Equals(girlHairColorsTexture[0].name))
                            {
                                index = girlHairColorsTexture.Length;
                            }

                            studentRenderer.material.SetTexture("_MainTex", girlHairColorsTexture[index - 1]);

                            girlHairAcc[0].transform.GetChild(0).
                        GetComponent<MeshRenderer>().material.SetTexture("_MainTex", girlHairColorsTexture[index - 1]);
                            girlHairAcc[1].transform.GetChild(0).
                        GetComponent<MeshRenderer>().material.SetTexture("_MainTex", girlHairColorsTexture[index - 1]);
                        }

                        index--;
                        CharacterInfo.HairColor = index;
                    }
                }
                #endregion
                #region HairAcc
                else if (characterSet.Equals(CharacterSet.HairAcc))
                {
                    if (CharacterInfo.Sex.Equals("Man"))
                    {
                        if (index.Equals(-1))
                        {
                            index = boyHairAcc.Length;
                        }
                        else
                        {
                            boyHairAcc[index].SetActive(false);
                        }

                        if (index.Equals(1))
                        {
                            index = 0;
                        }

                        if (!index.Equals(0))
                        {
                            boyHairAcc[index - 1].SetActive(true);
                            boyHairAcc[0].SetActive(true);
                            boyHairStyle[CharacterInfo.HairStyle].SetActive(false);
                        }
                        else
                        {
                            boyHairAcc[0].SetActive(false);
                            boyHairStyle[CharacterInfo.HairStyle].SetActive(true);
                        }
                        index--;
                        CharacterInfo.HairAcc = index;
                    }
                    else if (CharacterInfo.Sex.Equals("Woman"))
                    {
                        if (index.Equals(-1))
                        {
                            index = girlHairAcc.Length;
                        }
                        else
                        {
                            girlHairAcc[index].SetActive(false);
                        }

                        if (index.Equals(1) || index.Equals(2))
                        {
                            index = 0;
                        }

                        if (!index.Equals(0))
                        {
                            girlHairAcc[index - 1].SetActive(true);

                            if (!girlHairAcc[index - 1].name.Equals(string.Format("ribbon{0}_girl", index - 7)))
                            {
                                if (CharacterInfo.HairStyle.Equals(0))
                                {
                                    girlHairAcc[0].SetActive(true);
                                }
                                else if (CharacterInfo.HairStyle.Equals(1))
                                {
                                    girlHairAcc[1].SetActive(true);
                                }

                                girlHairStyle[CharacterInfo.HairStyle].SetActive(false);
                            }
                            else
                            {
                                if (CharacterInfo.HairStyle.Equals(0))
                                {
                                    girlHairAcc[0].SetActive(false);
                                }
                                else if (CharacterInfo.HairStyle.Equals(1))
                                {
                                    girlHairAcc[1].SetActive(false);
                                }

                                girlHairStyle[CharacterInfo.HairStyle].SetActive(true);
                            }
                        }
                        else
                        {
                            girlHairAcc[0].SetActive(false);
                            girlHairAcc[1].SetActive(false);
                            girlHairStyle[CharacterInfo.HairStyle].SetActive(true);
                        }
                        index--;
                        CharacterInfo.HairAcc = index;
                    }
                }
                #endregion
                #region FaceAcc
                else if (characterSet.Equals(CharacterSet.FaceAcc))
                {
                    if (CharacterInfo.Sex.Equals("Man"))
                    {
                        if (index.Equals(-1))
                        {
                            index = boyFaceAcc.Length;
                        }
                        else
                        {
                            boyFaceAcc[index].SetActive(false);
                        }

                        if (!index.Equals(0))
                        {
                            boyFaceAcc[index - 1].SetActive(true);
                        }
                        index--;
                        CharacterInfo.FaceAcc = index;
                    }
                    else if (CharacterInfo.Sex.Equals("Woman"))
                    {
                        if (index.Equals(-1))
                        {
                            index = girlFaceAcc.Length;
                        }
                        else
                        {
                            girlFaceAcc[index].SetActive(false);
                        }

                        if (!index.Equals(0))
                        {
                            girlFaceAcc[index - 1].SetActive(true);
                        }
                        index--;
                        CharacterInfo.FaceAcc = index;
                    }
                }
                #endregion
                #region Top
                else if (characterSet.Equals(CharacterSet.Top))
                {
                    if (CharacterInfo.Sex.Equals("Man"))
                    {
                        if (topIndex.Equals(0) && index.Equals(0))
                        {
                            boyTopStyle[topIndex].SetActive(false);
                            boyTopStyle[boyTopStyle.Length - 1].SetActive(true);
                            boyTopStyle[boyTopStyle.Length - 1].transform.GetChild(0).GetComponent<MeshRenderer>().material.SetTexture("_MainTex", boyTopTexture[boyTopTexture.Length - 1]);

                            topIndex = boyTopStyle.Length - 1;
                            index = boyTopTexture.Length - 1;

                            CharacterInfo.TopStyle = topIndex;
                            CharacterInfo.TopShape = index;
                        }
                        else if (!topIndex.Equals(0) && index.Equals(0))
                        {
                            boyTopStyle[topIndex].SetActive(false);
                            boyTopStyle[topIndex - 1].SetActive(true);
                            boyTopStyle[topIndex - 1].transform.GetChild(0).GetComponent<MeshRenderer>().material.SetTexture("_MainTex", boyTopTexture[boyTopTexture.Length - 1]);

                            topIndex -= 1;
                            index = boyTopTexture.Length - 1;

                            CharacterInfo.TopStyle = topIndex;
                            CharacterInfo.TopShape = index;
                        }
                        else
                        {
                            boyTopStyle[topIndex].transform.GetChild(0).GetComponent<MeshRenderer>().material.SetTexture("_MainTex", boyTopTexture[index - 1]);
                            index -= 1;

                            CharacterInfo.TopShape = index;
                        }
                    }
                    else if (CharacterInfo.Sex.Equals("Woman"))
                    {
                        if (topIndex.Equals(0) && index.Equals(0))
                        {
                            girlTopStyle[topIndex].SetActive(false);
                            girlTopStyle[girlTopStyle.Length - 1].SetActive(true);
                            girlTopStyle[girlTopStyle.Length - 1].transform.GetChild(0).GetComponent<MeshRenderer>().material.SetTexture("_MainTex", girlTopTexture[girlTopTexture.Length - 1]);

                            topIndex = girlTopStyle.Length - 1;
                            index = girlTopTexture.Length - 1;

                            CharacterInfo.TopStyle = topIndex;
                            CharacterInfo.TopShape = index;
                        }
                        else if (!topIndex.Equals(0) && index.Equals(0))
                        {
                            girlTopStyle[topIndex].SetActive(false);
                            girlTopStyle[topIndex - 1].SetActive(true);
                            girlTopStyle[topIndex - 1].transform.GetChild(0).GetComponent<MeshRenderer>().material.SetTexture("_MainTex", girlTopTexture[girlTopTexture.Length - 1]);

                            topIndex -= 1;
                            index = girlTopTexture.Length - 1;

                            CharacterInfo.TopStyle = topIndex;
                            CharacterInfo.TopShape = index;
                        }
                        else
                        {
                            girlTopStyle[topIndex].transform.GetChild(0).GetComponent<MeshRenderer>().material.SetTexture("_MainTex", girlTopTexture[index - 1]);
                            index -= 1;

                            CharacterInfo.TopShape = index;
                        }
                    }
                }
                #endregion

                break;
            #endregion

            #region RightArrow
            case "RightArrow":
                #region HairStyle
                if (characterSet.Equals(CharacterSet.HairStyle))
                {
                    if (CharacterInfo.Sex.Equals("Man"))
                    {
                        boyHairAcc[0].SetActive(false);

                        if (!CharacterInfo.HairAcc.Equals(-1))
                        {
                            boyHairAcc[CharacterInfo.HairAcc].SetActive(false);
                        }

                        boyHairStyle[index].SetActive(false);

                        if (index.Equals(boyHairStyle.Length - 1))
                        {
                            index = -1;
                        }

                        boyHairStyle[index + 1].SetActive(true);
                        index++;

                        CharacterInfo.HairStyle = index;
                        studentRenderer = boyHairStyle[index].transform.GetChild(0).GetComponent<MeshRenderer>();
                    }
                    else if (CharacterInfo.Sex.Equals("Woman"))
                    {
                        girlHairAcc[0].SetActive(false);
                        girlHairAcc[1].SetActive(false);

                        if (!CharacterInfo.HairAcc.Equals(-1))
                        {
                            girlHairAcc[CharacterInfo.HairAcc].SetActive(false);
                        }

                        girlHairStyle[index].SetActive(false);

                        if (index.Equals(girlHairStyle.Length - 1))
                        {
                            index = -1;
                        }

                        girlHairStyle[index + 1].SetActive(true);
                        index++;

                        CharacterInfo.HairStyle = index;
                        studentRenderer = girlHairStyle[index].transform.GetChild(0).GetComponent<MeshRenderer>();
                    }
                }
                #endregion
                #region HairColor
                else if (characterSet.Equals(CharacterSet.HairColor))
                {
                    if (CharacterInfo.Sex.Equals("Man"))
                    {
                        for (int i = 0; i < boyHairStyle.Length; i++)
                        {
                            studentRenderer = boyHairStyle[i].transform.GetChild(0).GetComponent<MeshRenderer>();

                            if (studentRenderer.material.mainTexture.name.Equals(boyHairColorsTexture[boyHairColorsTexture.Length - 1].name))
                            {
                                index = -1;
                            }

                            studentRenderer.material.SetTexture("_MainTex", boyHairColorsTexture[index + 1]);

                            boyHairAcc[0].transform.GetChild(0).
                        GetComponent<MeshRenderer>().material.SetTexture("_MainTex", boyHairColorsTexture[index + 1]);
                        }

                        index++;
                        CharacterInfo.HairColor = index;
                    }
                    else if (CharacterInfo.Sex.Equals("Woman"))
                    {
                        for (int i = 0; i < girlHairStyle.Length; i++)
                        {
                            studentRenderer = girlHairStyle[i].transform.GetChild(0).GetComponent<MeshRenderer>();

                            if (studentRenderer.material.mainTexture.name.Equals(girlHairColorsTexture[girlHairColorsTexture.Length - 1].name))
                            {
                                index = -1;
                            }

                            studentRenderer.material.SetTexture("_MainTex", girlHairColorsTexture[index + 1]);

                            girlHairAcc[0].transform.GetChild(0).
                        GetComponent<MeshRenderer>().material.SetTexture("_MainTex", girlHairColorsTexture[index + 1]);
                            girlHairAcc[1].transform.GetChild(0).
                        GetComponent<MeshRenderer>().material.SetTexture("_MainTex", girlHairColorsTexture[index + 1]);
                        }

                        index++;
                        CharacterInfo.HairColor = index;
                    }
                }
                #endregion
                #region HairAcc
                else if (characterSet.Equals(CharacterSet.HairAcc))
                {
                    if (CharacterInfo.Sex.Equals("Man"))
                    {
                        if (!index.Equals(-1))
                        {
                            boyHairAcc[index].SetActive(false);
                        }

                        if (index.Equals(-1))
                        {
                            index = 0;
                        }

                        if (!index.Equals(boyHairAcc.Length - 1))
                        {
                            boyHairAcc[index + 1].SetActive(true);
                            boyHairAcc[0].SetActive(true);
                            boyHairStyle[CharacterInfo.HairStyle].SetActive(false);
                        }
                        else
                        {
                            boyHairAcc[0].SetActive(false);
                            boyHairStyle[CharacterInfo.HairStyle].SetActive(true);
                            index = -2;
                        }
                        index++;
                        CharacterInfo.HairAcc = index;
                    }
                    else if (CharacterInfo.Sex.Equals("Woman"))
                    {
                        if (!index.Equals(-1))
                        {
                            girlHairAcc[index].SetActive(false);
                        }

                        if (index.Equals(-1))
                        {
                            index = 1;
                        }

                        if (!index.Equals(girlHairAcc.Length - 1))
                        {
                            girlHairAcc[index + 1].SetActive(true);

                            if (!girlHairAcc[index + 1].name.Equals(string.Format("ribbon{0}_girl", index - 5)))
                            {
                                if (CharacterInfo.HairStyle.Equals(0))
                                {
                                    girlHairAcc[0].SetActive(true);
                                }
                                else if (CharacterInfo.HairStyle.Equals(1))
                                {
                                    girlHairAcc[1].SetActive(true);
                                }
                                girlHairStyle[CharacterInfo.HairStyle].SetActive(false);
                            }
                            else
                            {
                                if (CharacterInfo.HairStyle.Equals(0))
                                {
                                    girlHairAcc[0].SetActive(false);
                                }
                                else if (CharacterInfo.HairStyle.Equals(1))
                                {
                                    girlHairAcc[1].SetActive(false);
                                }
                                girlHairStyle[CharacterInfo.HairStyle].SetActive(true);
                            }
                        }
                        else
                        {
                            girlHairAcc[0].SetActive(false);
                            girlHairStyle[CharacterInfo.HairStyle].SetActive(true);
                            index = -2;
                        }
                        index++;
                        CharacterInfo.HairAcc = index;
                    }
                }
                #endregion
                #region FaceAcc
                else if (characterSet.Equals(CharacterSet.FaceAcc))
                {
                    if (CharacterInfo.Sex.Equals("Man"))
                    {
                        if (!index.Equals(-1))
                        {
                            boyFaceAcc[index].SetActive(false);
                        }

                        if (!index.Equals(boyFaceAcc.Length - 1))
                        {
                            boyFaceAcc[index + 1].SetActive(true);
                        }
                        else
                        {
                            index = -2;
                        }

                        index++;
                        CharacterInfo.FaceAcc = index;
                    }
                    else if (CharacterInfo.Sex.Equals("Woman"))
                    {
                        if (!index.Equals(-1))
                        {
                            girlFaceAcc[index].SetActive(false);
                        }

                        if (!index.Equals(girlFaceAcc.Length - 1))
                        {
                            girlFaceAcc[index + 1].SetActive(true);
                        }
                        else
                        {
                            index = -2;
                        }

                        index++;
                        CharacterInfo.FaceAcc = index;
                    }
                }
                #endregion
                #region Top
                else if (characterSet.Equals(CharacterSet.Top))
                {
                    if (CharacterInfo.Sex.Equals("Man"))
                    {
                        if (topIndex.Equals(boyTopStyle.Length - 1) && index.Equals(boyTopTexture.Length - 1))
                        {
                            boyTopStyle[topIndex].SetActive(false);
                            boyTopStyle[0].SetActive(true);
                            boyTopStyle[0].transform.GetChild(0).GetComponent<MeshRenderer>().material.SetTexture("_MainTex", boyTopTexture[0]);

                            topIndex = 0;
                            index = 0;

                            CharacterInfo.TopStyle = 0;
                            CharacterInfo.TopShape = 0;
                        }
                        else if (!topIndex.Equals(boyTopStyle.Length - 1) && index.Equals(boyTopTexture.Length - 1))
                        {
                            boyTopStyle[topIndex].SetActive(false);
                            boyTopStyle[topIndex + 1].SetActive(true);
                            boyTopStyle[topIndex + 1].transform.GetChild(0).GetComponent<MeshRenderer>().material.SetTexture("_MainTex", boyTopTexture[0]);

                            topIndex += 1;
                            index = 0;

                            CharacterInfo.TopStyle = topIndex;
                            CharacterInfo.TopShape = index;
                        }
                        else
                        {
                            boyTopStyle[topIndex].transform.GetChild(0).GetComponent<MeshRenderer>().material.SetTexture("_MainTex", boyTopTexture[index + 1]);
                            index += 1;

                            CharacterInfo.TopShape = index;
                        }
                    }
                    else if (CharacterInfo.Sex.Equals("Woman"))
                    {
                        if (topIndex.Equals(girlTopStyle.Length - 1) && index.Equals(girlTopTexture.Length - 1))
                        {
                            girlTopStyle[topIndex].SetActive(false);
                            girlTopStyle[0].SetActive(true);
                            girlTopStyle[0].transform.GetChild(0).GetComponent<MeshRenderer>().material.SetTexture("_MainTex", girlTopTexture[0]);

                            topIndex = 0;
                            index = 0;

                            CharacterInfo.TopStyle = 0;
                            CharacterInfo.TopShape = 0;
                        }
                        else if (!topIndex.Equals(girlTopStyle.Length - 1) && index.Equals(girlTopTexture.Length - 1))
                        {
                            girlTopStyle[topIndex].SetActive(false);
                            girlTopStyle[topIndex + 1].SetActive(true);
                            girlTopStyle[topIndex + 1].transform.GetChild(0).GetComponent<MeshRenderer>().material.SetTexture("_MainTex", girlTopTexture[0]);

                            topIndex += 1;
                            index = 0;

                            CharacterInfo.TopStyle = topIndex;
                            CharacterInfo.TopShape = index;
                        }
                        else
                        {
                            girlTopStyle[topIndex].transform.GetChild(0).GetComponent<MeshRenderer>().material.SetTexture("_MainTex", girlTopTexture[index + 1]);
                            index += 1;

                            CharacterInfo.TopShape = index;
                        }
                    }
                }
                #endregion

                break;
            #endregion

            default:
                break;
        }
    }
}
