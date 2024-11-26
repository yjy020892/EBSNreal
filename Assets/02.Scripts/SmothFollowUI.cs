using NRKernal;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI가 화면에 고정되나 부드럽게 따라오게
/// </summary>
public class SmothFollowUI : MonoBehaviour
{
    Vector3 offset;

    public Transform followCam;

    Transform mainCamera;

    public Vector3 followCamPosition;

    void Start()
    {
        mainCamera = GameObject.Find("NRCameraRig").GetComponent<Transform>();
        followCam = GameObject.Find("FollowCam").GetComponent<Transform>();
        followCam.localPosition = followCamPosition;
        //offset = transform.position - mainCamera.position;
        offset = followCam.position - mainCamera.position;
        //followCam.position = offset;
        transform.position = followCam.position;
    }

    void Update()
    {
        //Pose pose = NRFrame.HeadPose;

        //transform.LookAt(pose.position);
        transform.LookAt(mainCamera.position);
        transform.Rotate(new Vector3(0, 180, 0));
        //transform.position = Vector3.Lerp(transform.position, offset, 2.5f * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, followCam.position, 4.0f * Time.deltaTime);
    }
}
