using NRKernal;
using SPVR.Model;
using SPVR.Network;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum VolcanoState
{
    NONE,
    MOVE,
    ROTATE,
    SCALE
}

public enum VolcanoStep
{
    Step1,
    Step2,
    Step3,
    Step4,
    Step5
}

/// <summary>
/// 화산 오브젝트, UI 컨트롤 및 동기화
/// </summary>
public class Volcano : SyncBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public VolcanoState volcanoState = VolcanoState.NONE;
    public VolcanoStep volcanoStep = VolcanoStep.Step1;

    private ControllerHandEnum m_CurrentDebugHand = ControllerHandEnum.Right;

    InteractionState interactionState;
    public CanvasRaycastTarget canvasRaycastTarget;
    private CanvasRaycastTarget hostRaycastUI;

    Animator volcanoSecAnim;

    public GameObject[] sectionObjs;
    public GameObject[] stepObjs;
    public GameObject controlObj;
    public GameObject[] volcanoObj;

    public Text testText;

    public Image[] stateImage;
    public Sprite[] moveSprite;
    public Sprite[] rotateSprite;
    public Sprite[] scaleSprite;
    public Image[] stepImage;
    public Sprite[] stepSpritesDefault;
    public Sprite[] stepSpritesRollOver;
    public Sprite[] controlSprite;
    public Image sectionImage;
    public Sprite[] sectionSprite;

    public Transform selectUI;
    public RectTransform selectUIRectTransform;

    Transform reticleTransform;

    private bool b_MoveState = false;
    private bool b_RotateState = false;
    private bool b_ScaleState = false;

    Vector3 updateVolcanoVec;

    private Vector3 screenPoint;
    private Vector3 offset;
    private Vector3 offsetController;
    private Vector3 offsetStandard = Vector3.zero;
    private Vector2 offsetTouchPad;
    private Vector3 volcanoMinScale = new Vector3(0.5f, 0.5f, 0.5f);
    private Vector3 volcanoMaxScale = new Vector3(1.3f, 1.3f, 1.3f);

    //string volStepStr = string.Empty;
    int preStepNumber = 0;
    int currentStepNumber = 0;

    bool b_OwnGrab = false;
    bool b_ChangeVolcano = false;
    bool b_ChangeSection = false;
    bool b_SectionAnim = false;

    void Start()
    {
        interactionState = GetComponent<InteractionState>();
        reticleTransform = GameObject.Find("ReticleR").transform;

        if (NrealTest.instance.b_Host)
        {
            controlObj.SetActive(true);
            hostRaycastUI = GameObject.Find("PlayHostUI").GetComponent<CanvasRaycastTarget>();
        }

        stepObjs[currentStepNumber].SetActive(true);
        SetDirty();

        if (transform.root.name.Equals("VolcanoSet(Clone)"))
        {
            gameObject.GetComponent<BoxCollider>().enabled = false;

            transform.root.transform.GetChild(1).GetComponent<CanvasRaycastTarget>().enabled = false;

            Transform[] tran = transform.root.gameObject.GetComponentsInChildren<Transform>();
            foreach (Transform t in tran)
            {
                t.gameObject.layer = 10;
            }
        }
    }
    
    void Update()
    {
        #region isGrabing (화산 오브젝트 상호작용 진행)
        if (interactionState.IsGrabing)
        {
            // 이동
            if (volcanoState.Equals(VolcanoState.MOVE))
            {
                canvasRaycastTarget.enabled = false;
                if (NrealTest.instance.b_Host)
                {
                    hostRaycastUI.enabled = false;
                }

                Vector3 cursorScreenPoint = reticleTransform.position;
                Vector3 cursorPosition = cursorScreenPoint + offset;
                //transform.position = cursorPosition;
                Vector3 currentPosition = transform.localPosition;
                transform.position = new Vector3(cursorPosition.x, cursorPosition.y, cursorPosition.z + ((NRInput.GetTouch(m_CurrentDebugHand).y - offsetTouchPad.y) * 2.0f * Time.deltaTime));
                offsetStandard = transform.localPosition - currentPosition;
                b_MoveState = true;
                SetDirty();
                //Debug.Log(string.Format("x: {0}, y: {1}, z: {2}",offsetStandard.x, offsetStandard.y, offsetStandard.z));
            }
            // 회전
            else if (volcanoState.Equals(VolcanoState.ROTATE))
            {
                Vector3 currentRotation = transform.localEulerAngles;
                transform.Rotate(new Vector3(0, -(NRInput.GetTouch(m_CurrentDebugHand).x - offsetTouchPad.x) * 2.0f, 0));
                offsetStandard = transform.localEulerAngles - currentRotation;
                b_RotateState = true;
                SetDirty();

                //"rotation y : " + (NRInput.GetTouch(m_CurrentDebugHand).y - offsetTouchPad.y).ToString("F2");
            }
            // 크기 조절
            else if (volcanoState.Equals(VolcanoState.SCALE))
            {
                Vector3 currentScale = transform.localScale;

                if (transform.localScale.x <= 1.3f && transform.localScale.x >= 0.5f)
                {
                    transform.localScale = new Vector3(transform.localScale.x + ((NRInput.GetTouch(m_CurrentDebugHand).x - offsetTouchPad.x) * 0.2f * Time.deltaTime),
                    transform.localScale.y + ((NRInput.GetTouch(m_CurrentDebugHand).x - offsetTouchPad.x) * 0.2f * Time.deltaTime),
                    transform.localScale.z + ((NRInput.GetTouch(m_CurrentDebugHand).x - offsetTouchPad.x) * 0.2f * Time.deltaTime));

                    offsetStandard = transform.localScale - currentScale;
                    b_ScaleState = true;
                    SetDirty();
                }
                else if (transform.localScale.x < 0.5f)
                {
                    transform.localScale = volcanoMinScale;
                }
                else if (transform.localScale.x > 1.3f)
                {
                    transform.localScale = volcanoMaxScale;
                }
            }
        }
        #endregion

        #region SyncVolcano (동기화 받은 쪽에서 화산, UI 상호작용 실행)
        if (NrealTest.instance.b_ChangeVolcano && transform.root.name.Equals("VolcanoSet"))
        {
            NrealTest.instance.b_ChangeVolcano = false;

            if (!string.IsNullOrEmpty(NrealTest.instance.volStepStr))
            {
                for (int i = 0; i < stepImage.Length; i++)
                {
                    stepImage[i].sprite = stepSpritesDefault[i];
                }

                switch (NrealTest.instance.volStepStr)
                {
                    case "Step1":
                        preStepNumber = currentStepNumber;
                        stepObjs[currentStepNumber].SetActive(false);

                        volcanoStep = VolcanoStep.Step1;
                        currentStepNumber = (int)volcanoStep;
                        stepObjs[currentStepNumber].SetActive(true);
                        break;

                    case "Step2":
                        preStepNumber = currentStepNumber;
                        stepObjs[currentStepNumber].SetActive(false);

                        volcanoStep = VolcanoStep.Step2;
                        currentStepNumber = (int)volcanoStep;
                        stepObjs[currentStepNumber].SetActive(true);
                        break;

                    case "Step3":
                        preStepNumber = currentStepNumber;
                        stepObjs[currentStepNumber].SetActive(false);

                        volcanoStep = VolcanoStep.Step3;
                        currentStepNumber = (int)volcanoStep;
                        stepObjs[currentStepNumber].SetActive(true);
                        break;

                    case "Step4":
                        preStepNumber = currentStepNumber;
                        stepObjs[currentStepNumber].SetActive(false);

                        volcanoStep = VolcanoStep.Step4;
                        currentStepNumber = (int)volcanoStep;
                        stepObjs[currentStepNumber].SetActive(true);
                        break;

                    case "Step5":
                        preStepNumber = currentStepNumber;
                        stepObjs[currentStepNumber].SetActive(false);

                        volcanoStep = VolcanoStep.Step5;
                        currentStepNumber = (int)volcanoStep;
                        stepObjs[currentStepNumber].SetActive(true);
                        break;
                }

                SetDirty();

                if (NrealTest.instance.b_Section)
                {
                    sectionObjs[currentStepNumber].SetActive(false);
                }
                else if (!NrealTest.instance.b_Section)
                {
                    sectionObjs[currentStepNumber].SetActive(true);
                }

                stepImage[currentStepNumber].sprite = stepSpritesRollOver[currentStepNumber];
                selectUI.SetParent(stepImage[currentStepNumber].transform);
                selectUIRectTransform.anchoredPosition = new Vector2(0, 100);
                NrealTest.instance.volStepStr = string.Empty;
            }
        }

        if (NrealTest.instance.b_ChangeSection && transform.root.name.Equals("VolcanoSet"))
        {
            //Debug.Log("b_ChangeSection");
            NrealTest.instance.b_ChangeSection = false;

            if (NrealTest.instance.b_Section)
            {
                b_SectionAnim = true;
                volcanoSecAnim = volcanoObj[currentStepNumber].GetComponent<Animator>();
                volcanoSecAnim.SetTrigger("Open");
                sectionImage.sprite = sectionSprite[1];
            }
            else if (!NrealTest.instance.b_Section)
            {
                b_SectionAnim = true;
                sectionObjs[currentStepNumber].SetActive(true);
                volcanoSecAnim = volcanoObj[currentStepNumber].GetComponent<Animator>();
                volcanoSecAnim.SetTrigger("Close");
                sectionImage.sprite = sectionSprite[0];
            }

            SetDirty();
        }
        #endregion
        
        if(b_SectionAnim && transform.root.name.Equals("VolcanoSet"))
        {
            if (volcanoSecAnim != null)
            {
                if(volcanoSecAnim.GetCurrentAnimatorStateInfo(0).IsName("Open") || volcanoSecAnim.GetCurrentAnimatorStateInfo(0).IsName("Close"))
                {
                    //Debug.Log(volcanoSecAnim.GetCurrentAnimatorStateInfo(0).normalizedTime);
                    if (volcanoSecAnim.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.95f)
                    {
                        b_SectionAnim = false;
                        if (NrealTest.instance.b_Section)
                        {
                            sectionObjs[currentStepNumber].SetActive(false);
                        }
                    }
                }
            }
        }
    }

    #region Grab
    public void OnPointerDown(PointerEventData eventData)
    {
        // 이름으로 본인 오브젝트 확인
        if (eventData.pointerEnter.transform.root.name.Equals("VolcanoSet"))
        {
            if (!NrealTest.instance.b_Grab)
            {
                if (NrealTest.instance.b_Host)
                {
                    NrealTest.instance.b_Grab = true;
                    SetDirty();
                    
                    b_OwnGrab = true;
                    interactionState.Grab();
                }
                else
                {
                    if (NrealTest.instance.b_Control)
                    {
                        NrealTest.instance.b_Grab = true;
                        SetDirty();
                        
                        b_OwnGrab = true;
                        interactionState.Grab();
                    }
                }
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!canvasRaycastTarget.enabled)
        {
            canvasRaycastTarget.enabled = true;
        }
        if (NrealTest.instance.b_Host && !hostRaycastUI.enabled)
        {
            hostRaycastUI.enabled = true;
        }

        if (b_OwnGrab)
        {
            b_OwnGrab = false;
            if (volcanoState.Equals(VolcanoState.MOVE))
            {
                offsetStandard = Vector3.zero;
                b_MoveState = false;
                SetDirty();
            }
            else if (volcanoState.Equals(VolcanoState.ROTATE))
            {
                offsetStandard = Vector3.zero;
                b_RotateState = false;
                SetDirty();
            }
            else if (volcanoState.Equals(VolcanoState.SCALE))
            {
                offsetStandard = Vector3.zero;
                b_ScaleState = false;
                SetDirty();
            }

            NrealTest.instance.b_Grab = false;
            SetDirty();
        }
        interactionState.ReleaseGrab();
    }

    /// <summary>
    /// InteractionState Grab 이벤트 구현 함수입니다.
    /// InteractionState 인스펙터에 컴포넌트 Grab 이벤트에 연결되어 있습니다.
    /// 모델의 소유권 유저에게 모델의 소유권을 요청하고 전달받으면 콜백 호출됩니다.
    /// 보통은 성공하지만, 모델의 소유권을 전달받지 못할 경우 실패할 수 있습니다. 
    /// </summary>
    /// <param name="grabSuccess">Grab 성공여부</param>
    public void OnGrab(bool grabSuccess)
    {
        if (grabSuccess)
        {
            if (volcanoState.Equals(VolcanoState.MOVE))
            {
                screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
                offset = gameObject.transform.position - reticleTransform.position;
                offsetTouchPad = NRInput.GetTouch(m_CurrentDebugHand);
            }
            else if (volcanoState.Equals(VolcanoState.ROTATE))
            {
                offsetTouchPad = NRInput.GetTouch(m_CurrentDebugHand);
            }
            else if (volcanoState.Equals(VolcanoState.SCALE))
            {
                offsetTouchPad = NRInput.GetTouch(m_CurrentDebugHand);
            }
        }
    }
    #endregion

    #region Section, Control
    /// <summary>
    /// 단면, 제어 상태 설정
    /// </summary>
    public void OnClickButton()
    {
        if(b_SectionAnim)
        {
            return;
        }

        string str = EventSystem.current.currentSelectedGameObject.name;
        Image img = EventSystem.current.currentSelectedGameObject.transform.GetChild(0).GetComponent<Image>();

        if (!EventSystem.current.currentSelectedGameObject.transform.root.name.Equals("VolcanoSet"))
        {
            return;
        }

        // 교육자일때
        if (NrealTest.instance.b_Host)
        {
            switch (str)
            {
                // 단면 버튼
                case "Section":
                    volcanoSecAnim = volcanoObj[currentStepNumber].GetComponent<Animator>();

                    if (!volcanoSecAnim.GetCurrentAnimatorStateInfo(0).IsName("Open") || !volcanoSecAnim.GetCurrentAnimatorStateInfo(0).IsName("Close"))
                    {
                        if (img.sprite.Equals(sectionSprite[0]))
                        {
                            NrealTest.instance.b_Section = true;
                            b_ChangeSection = true;
                            b_SectionAnim = true;
                            //sectionObjs[currentStepNumber].SetActive(false);

                            volcanoSecAnim.SetTrigger("Open");

                            img.sprite = sectionSprite[1];

                        }
                        else if (img.sprite.Equals(sectionSprite[1]))
                        {
                            NrealTest.instance.b_Section = false;
                            b_ChangeSection = true;
                            b_SectionAnim = true;
                            sectionObjs[currentStepNumber].SetActive(true);

                            volcanoSecAnim.SetTrigger("Close");
                            img.sprite = sectionSprite[0];
                        }

                        SetDirty();
                    }
                    break;

                // 제어 버튼
                case "Control":
                    if (img.sprite.Equals(controlSprite[0]))
                    {
                        img.sprite = controlSprite[1];
                        NrealTest.instance.PlayerState("Control", false);
                    }
                    else if (img.sprite.Equals(controlSprite[1]))
                    {
                        img.sprite = controlSprite[0];
                        NrealTest.instance.PlayerState("Control", true);
                    }
                    break;
            }
        }
        else
        {
            if (NrealTest.instance.b_Control)
            {
                switch (str)
                {
                    case "Section":
                        if (img.sprite.Equals(sectionSprite[0]))
                        {
                            volcanoSecAnim = volcanoObj[currentStepNumber].GetComponent<Animator>();

                            if (!volcanoSecAnim.GetCurrentAnimatorStateInfo(0).IsName("Open") || !volcanoSecAnim.GetCurrentAnimatorStateInfo(0).IsName("Close"))
                            {
                                NrealTest.instance.b_Section = true;
                                b_ChangeSection = true;
                                b_SectionAnim = true;
                                //sectionObjs[currentStepNumber].SetActive(false);

                                volcanoSecAnim.SetTrigger("Open");
                                img.sprite = sectionSprite[1];
                            }
                        }
                        else if (img.sprite.Equals(sectionSprite[1]))
                        {
                            volcanoSecAnim = volcanoObj[currentStepNumber].GetComponent<Animator>();

                            if (!volcanoSecAnim.GetCurrentAnimatorStateInfo(0).IsName("Open") || !volcanoSecAnim.GetCurrentAnimatorStateInfo(0).IsName("Close"))
                            {
                                NrealTest.instance.b_Section = false;
                                b_ChangeSection = true;
                                b_SectionAnim = true;
                                sectionObjs[currentStepNumber].SetActive(true);

                                volcanoSecAnim.SetTrigger("Close");
                                img.sprite = sectionSprite[0];
                            }
                        }

                        SetDirty();

                        break;

                    case "Control":
                        break;
                }
            }
        }
    }

    public void NarrationVolSection()
    {
        if (sectionImage.sprite.Equals(sectionSprite[0]))
        {
            volcanoSecAnim = volcanoObj[currentStepNumber].GetComponent<Animator>();

            if (!volcanoSecAnim.GetCurrentAnimatorStateInfo(0).IsName("Open") || !volcanoSecAnim.GetCurrentAnimatorStateInfo(0).IsName("Close"))
            {
                sectionImage.sprite = sectionSprite[1];

                NrealTest.instance.b_Section = true;
                b_ChangeSection = true;
                b_SectionAnim = true;
                //sectionObjs[currentStepNumber].SetActive(false);

                volcanoSecAnim.SetTrigger("Open");
            }
        }

        SetDirty();
    }
    #endregion

    #region ClickState
    /// <summary>
    /// 이동,회전,크기 조작 상태 설정
    /// </summary>
    public void OnClickState()
    {
        if (b_SectionAnim)
        {
            return;
        }

        string str = EventSystem.current.currentSelectedGameObject.name;
        Image img = EventSystem.current.currentSelectedGameObject.transform.GetChild(0).GetComponent<Image>();

        if (!EventSystem.current.currentSelectedGameObject.transform.root.name.Equals("VolcanoSet"))
        {
            return;
        }

        if (NrealTest.instance.b_Host)
        {
            switch (str)
            {
                // 이동
                case "Move":
                    stateImage[1].sprite = rotateSprite[0];
                    stateImage[2].sprite = scaleSprite[0];

                    if (img.sprite.Equals(moveSprite[0]))
                    {
                        volcanoState = VolcanoState.MOVE;
                        img.sprite = moveSprite[1];
                    }
                    else
                    {
                        volcanoState = VolcanoState.NONE;
                        img.sprite = moveSprite[0];
                    }
                    break;
                    
                // 회전
                case "Rotate":
                    stateImage[0].sprite = moveSprite[0];
                    stateImage[2].sprite = scaleSprite[0];

                    if (img.sprite.Equals(rotateSprite[0]))
                    {
                        volcanoState = VolcanoState.ROTATE;
                        img.sprite = rotateSprite[1];
                    }
                    else
                    {
                        volcanoState = VolcanoState.NONE;
                        img.sprite = rotateSprite[0];
                    }
                    break;

                // 크기
                case "Scale":
                    stateImage[0].sprite = moveSprite[0];
                    stateImage[1].sprite = rotateSprite[0];

                    if (img.sprite.Equals(scaleSprite[0]))
                    {
                        volcanoState = VolcanoState.SCALE;
                        img.sprite = scaleSprite[1];
                    }
                    else
                    {
                        volcanoState = VolcanoState.NONE;
                        img.sprite = scaleSprite[0];
                    }
                    break;
            }
        }
        else
        {
            if (NrealTest.instance.b_Control)
            {
                switch (str)
                {
                    case "Move":
                        stateImage[1].sprite = rotateSprite[0];
                        stateImage[2].sprite = scaleSprite[0];

                        if (img.sprite.Equals(moveSprite[0]))
                        {
                            volcanoState = VolcanoState.MOVE;
                            img.sprite = moveSprite[1];
                        }
                        else
                        {
                            volcanoState = VolcanoState.NONE;
                            img.sprite = moveSprite[0];
                        }
                        break;

                    case "Rotate":
                        stateImage[0].sprite = moveSprite[0];
                        stateImage[2].sprite = scaleSprite[0];

                        if (img.sprite.Equals(rotateSprite[0]))
                        {
                            volcanoState = VolcanoState.ROTATE;
                            img.sprite = rotateSprite[1];
                        }
                        else
                        {
                            volcanoState = VolcanoState.NONE;
                            img.sprite = rotateSprite[0];
                        }
                        break;

                    case "Scale":
                        stateImage[0].sprite = moveSprite[0];
                        stateImage[1].sprite = rotateSprite[0];

                        if (img.sprite.Equals(scaleSprite[0]))
                        {
                            volcanoState = VolcanoState.SCALE;
                            img.sprite = scaleSprite[1];
                        }
                        else
                        {
                            volcanoState = VolcanoState.NONE;
                            img.sprite = scaleSprite[0];
                        }
                        break;
                }
            }
        }
    }
    #endregion

    #region ClickStep
/// <summary>
/// 단계 설정
/// </summary>
    public void OnClickStep()
    {
        if (b_SectionAnim)
        {
            return;
        }

        string str = EventSystem.current.currentSelectedGameObject.name;
        Transform t = EventSystem.current.currentSelectedGameObject.transform;
        Image img = EventSystem.current.currentSelectedGameObject.GetComponent<Image>();

        if (!EventSystem.current.currentSelectedGameObject.transform.root.name.Equals("VolcanoSet"))
        {
            return;
        }

        if (NrealTest.instance.b_Host)
        {
            if (volcanoStep.ToString().Equals(str))
            {
                return;
            }

            for (int i = 0; i < stepImage.Length; i++)
            {
                stepImage[i].sprite = stepSpritesDefault[i];
            }

            switch (str)
            {
                case "Step1":
                    preStepNumber = currentStepNumber;
                    stepObjs[currentStepNumber].SetActive(false);

                    volcanoStep = VolcanoStep.Step1;
                    currentStepNumber = (int)volcanoStep;
                    stepObjs[currentStepNumber].SetActive(true);
                    b_ChangeVolcano = true;

                    SetDirty();
                    break;

                case "Step2":
                    preStepNumber = currentStepNumber;
                    stepObjs[currentStepNumber].SetActive(false);

                    volcanoStep = VolcanoStep.Step2;
                    currentStepNumber = (int)volcanoStep;
                    stepObjs[currentStepNumber].SetActive(true);
                    b_ChangeVolcano = true;

                    SetDirty();
                    break;

                case "Step3":
                    preStepNumber = currentStepNumber;
                    stepObjs[currentStepNumber].SetActive(false);

                    volcanoStep = VolcanoStep.Step3;
                    currentStepNumber = (int)volcanoStep;
                    stepObjs[currentStepNumber].SetActive(true);
                    b_ChangeVolcano = true;

                    SetDirty();
                    break;

                case "Step4":
                    preStepNumber = currentStepNumber;
                    stepObjs[currentStepNumber].SetActive(false);

                    volcanoStep = VolcanoStep.Step4;
                    currentStepNumber = (int)volcanoStep;
                    stepObjs[currentStepNumber].SetActive(true);
                    b_ChangeVolcano = true;

                    SetDirty();
                    break;

                case "Step5":
                    preStepNumber = currentStepNumber;
                    stepObjs[currentStepNumber].SetActive(false);

                    volcanoStep = VolcanoStep.Step5;
                    currentStepNumber = (int)volcanoStep;
                    stepObjs[currentStepNumber].SetActive(true);
                    b_ChangeVolcano = true;

                    SetDirty();
                    break;
            }

            // 단계 클릭 후 단면설정
            if (NrealTest.instance.b_Section)
            {
                sectionObjs[currentStepNumber].SetActive(false);
            }
            else if (!NrealTest.instance.b_Section)
            {
                sectionObjs[currentStepNumber].SetActive(true);
            }

            img.sprite = stepSpritesRollOver[currentStepNumber];
            selectUI.SetParent(t);
            selectUIRectTransform.anchoredPosition = new Vector2(0, 100);
        }
        else
        {
            if (NrealTest.instance.b_Control)
            {
                if (volcanoStep.ToString().Equals(str))
                {
                    return;
                }

                for (int i = 0; i < stepImage.Length; i++)
                {
                    stepImage[i].sprite = stepSpritesDefault[i];
                }

                switch (str)
                {
                    case "Step1":
                        preStepNumber = currentStepNumber;
                        stepObjs[currentStepNumber].SetActive(false);

                        volcanoStep = VolcanoStep.Step1;
                        currentStepNumber = (int)volcanoStep;
                        stepObjs[currentStepNumber].SetActive(true);
                        b_ChangeVolcano = true;

                        SetDirty();
                        break;

                    case "Step2":
                        preStepNumber = currentStepNumber;
                        stepObjs[currentStepNumber].SetActive(false);

                        volcanoStep = VolcanoStep.Step2;
                        currentStepNumber = (int)volcanoStep;
                        stepObjs[currentStepNumber].SetActive(true);
                        b_ChangeVolcano = true;

                        SetDirty();
                        break;

                    case "Step3":
                        preStepNumber = currentStepNumber;
                        stepObjs[currentStepNumber].SetActive(false);

                        volcanoStep = VolcanoStep.Step3;
                        currentStepNumber = (int)volcanoStep;
                        stepObjs[currentStepNumber].SetActive(true);
                        b_ChangeVolcano = true;

                        SetDirty();
                        break;

                    case "Step4":
                        preStepNumber = currentStepNumber;
                        stepObjs[currentStepNumber].SetActive(false);
                        
                        volcanoStep = VolcanoStep.Step4;
                        currentStepNumber = (int)volcanoStep;
                        stepObjs[currentStepNumber].SetActive(true);
                        b_ChangeVolcano = true;

                        SetDirty();
                        break;

                    case "Step5":
                        preStepNumber = currentStepNumber;
                        stepObjs[currentStepNumber].SetActive(false);

                        volcanoStep = VolcanoStep.Step5;
                        currentStepNumber = (int)volcanoStep;
                        stepObjs[currentStepNumber].SetActive(true);
                        b_ChangeVolcano = true;

                        SetDirty();
                        break;
                }

                if (NrealTest.instance.b_Section)
                {
                    sectionObjs[currentStepNumber].SetActive(false);
                }
                else if (!NrealTest.instance.b_Section)
                {
                    sectionObjs[currentStepNumber].SetActive(true);
                }

                img.sprite = stepSpritesRollOver[currentStepNumber];
                selectUI.SetParent(t);
                selectUIRectTransform.anchoredPosition = new Vector2(0, 100);
            }
        }
    }

    /// <summary>
    /// 대사와 단계 동기화
    /// </summary>
    public void NarrationVolStep(string stepStr)
    {
        for (int i = 0; i < stepImage.Length; i++)
        {
            stepImage[i].sprite = stepSpritesDefault[i];
        }

        switch (stepStr)
        {
            case "step1":
                preStepNumber = currentStepNumber;
                stepObjs[currentStepNumber].SetActive(false);

                volcanoStep = VolcanoStep.Step1;
                currentStepNumber = (int)volcanoStep;
                stepObjs[currentStepNumber].SetActive(true);
                b_ChangeVolcano = true;

                SetDirty();
                break;

            case "step2":
                preStepNumber = currentStepNumber;
                stepObjs[currentStepNumber].SetActive(false);

                volcanoStep = VolcanoStep.Step2;
                currentStepNumber = (int)volcanoStep;
                stepObjs[currentStepNumber].SetActive(true);
                b_ChangeVolcano = true;

                SetDirty();
                break;

            case "step3":
                preStepNumber = currentStepNumber;
                stepObjs[currentStepNumber].SetActive(false);

                volcanoStep = VolcanoStep.Step3;
                currentStepNumber = (int)volcanoStep;
                stepObjs[currentStepNumber].SetActive(true);
                b_ChangeVolcano = true;

                SetDirty();
                break;

            case "step4":
                preStepNumber = currentStepNumber;
                stepObjs[currentStepNumber].SetActive(false);

                volcanoStep = VolcanoStep.Step4;
                currentStepNumber = (int)volcanoStep;
                stepObjs[currentStepNumber].SetActive(true);
                b_ChangeVolcano = true;

                SetDirty();
                break;

            case "step5":
                preStepNumber = currentStepNumber;
                stepObjs[currentStepNumber].SetActive(false);

                volcanoStep = VolcanoStep.Step5;
                currentStepNumber = (int)volcanoStep;
                stepObjs[currentStepNumber].SetActive(true);
                b_ChangeVolcano = true;

                SetDirty();
                break;
        }

        if (NrealTest.instance.b_Section)
        {
            sectionObjs[currentStepNumber].SetActive(false);
        }
        else if (!NrealTest.instance.b_Section)
        {
            sectionObjs[currentStepNumber].SetActive(true);
        }

        stepImage[currentStepNumber].sprite = stepSpritesRollOver[currentStepNumber];
        selectUI.SetParent(stepImage[currentStepNumber].transform);
        selectUIRectTransform.anchoredPosition = new Vector2(0, 100);
    }
    #endregion

    #region Sync
    public override void SerializeView(BinaryWriter bw)
    {
        //Debug.Log("SerializeView");
        bw.Write(NrealTest.instance.b_Grab);
        bw.Write(b_MoveState);
        bw.Write(b_RotateState);
        bw.Write(b_ScaleState);
        bw.Write(offsetStandard);
        bw.Write(preStepNumber);
        bw.Write(currentStepNumber);
        bw.Write(b_ChangeVolcano);
        bw.Write(volcanoStep.ToString());
        bw.Write(NrealTest.instance.b_Section);
        bw.Write(b_ChangeSection);
        b_ChangeVolcano = false;
        b_ChangeSection = false;
    }

    public override void DeserializeView(BinaryReader br)
    {
        //Debug.Log("DeserializeView");
        NrealTest.instance.b_Grab = br.ReadBoolean();
        b_MoveState = br.ReadBoolean();
        b_RotateState = br.ReadBoolean();
        b_ScaleState = br.ReadBoolean();
        updateVolcanoVec = br.ReadVector3();
        preStepNumber = br.ReadInt32();
        currentStepNumber = br.ReadInt32();
        NrealTest.instance.b_ChangeVolcano = br.ReadBoolean();
        NrealTest.instance.volStepStr = br.ReadString();
        NrealTest.instance.b_Section = br.ReadBoolean();
        NrealTest.instance.b_ChangeSection = br.ReadBoolean();

        for (int i = 0; i < stepImage.Length; i++)
        {
            stepImage[i].sprite = stepSpritesDefault[i];
        }

        stepImage[currentStepNumber].sprite = stepSpritesRollOver[currentStepNumber];
        selectUI.SetParent(stepImage[currentStepNumber].transform);
        selectUIRectTransform.anchoredPosition = new Vector2(0, 100);
        stepObjs[preStepNumber].SetActive(false);
        stepObjs[currentStepNumber].SetActive(true);

        if (NrealTest.instance.b_Section)
        {
            sectionImage.sprite = sectionSprite[1];
            sectionObjs[currentStepNumber].SetActive(false);
        }
        else if (!NrealTest.instance.b_Section)
        {
            sectionImage.sprite = sectionSprite[0];
            sectionObjs[currentStepNumber].SetActive(true);

            if (!sectionObjs[currentStepNumber].layer.Equals(10))
            {
                Transform[] tran = sectionObjs[currentStepNumber].GetComponentsInChildren<Transform>();
                foreach (Transform t in tran)
                {
                    t.gameObject.layer = 10;
                }
            }
        }

        if (!volcanoObj[currentStepNumber].layer.Equals(10) && volcanoObj[currentStepNumber].activeInHierarchy)
        {
            Transform[] tran = volcanoObj[currentStepNumber].GetComponentsInChildren<Transform>();
            foreach (Transform t in tran)
            {
                t.gameObject.layer = 10;
            }
        }

        if (b_MoveState)
        {
            NrealTest.instance.volcanoTransform.localPosition = NrealTest.instance.volcanoTransform.localPosition + updateVolcanoVec;
        }
        if (b_RotateState)
        {
            NrealTest.instance.volcanoTransform.localEulerAngles = NrealTest.instance.volcanoTransform.localEulerAngles + updateVolcanoVec;
        }
        if (b_ScaleState)
        {
            if (NrealTest.instance.volcanoTransform.localScale.x <= 1.3f && NrealTest.instance.volcanoTransform.localScale.x >= 0.5f)
            {
                NrealTest.instance.volcanoTransform.localScale = NrealTest.instance.volcanoTransform.localScale + updateVolcanoVec;
            }
            else if (NrealTest.instance.volcanoTransform.localScale.x < 0.5f)
            {
                NrealTest.instance.volcanoTransform.localScale = volcanoMinScale;
            }
            else if (NrealTest.instance.volcanoTransform.localScale.x > 1.3f)
            {
                NrealTest.instance.volcanoTransform.localScale = volcanoMaxScale;
            }
        }
    }
    #endregion
}