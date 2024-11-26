using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

using SPVR.Core;

public enum EAvatarBone : int
{
    Root = 0,
    Pelvis,
    Spine_1,
    Spine_2,
    Spine_3,
    Head,
    Eyes_1,
    Eyes_2,
    LeftThigh,
    LeftCalf,
    LeftFoot,
    RightThigh,
    RightCalf,
    RightFoot,
    LeftUpperArm,
    LeftForeArm,
    LeftHand,
    RightUpperArm,
    RightForeArm,
    RightHand,
    LeftHandThumb,
    LeftHandIndex,
    LeftHandMiddle,
    LeftHandRing,
    LeftHandPinky,
    RightHandThumb,
    RightHandIndex,
    RightHandMiddle,
    RightHandRing,
    RightHandPinky,
    Count
}

[System.Serializable]
public class BoneReferences : MonoBehaviour
{
    #region Transforms

    [Header("Body")]
    [SerializeField]
    public Transform Root;

    [SerializeField]
    public Transform Pelvis;

    [SerializeField]
    public Transform Spine_1;

    [SerializeField]
    public Transform Spine_2;

    [SerializeField]
    public Transform Spine_3;

    [Header("Head")]
    [SerializeField]
    public Transform Head;

    [SerializeField]
    public Transform Eyes_1;

    [SerializeField]
    public Transform Eyes_2;

    [Header("Left Leg")]
    [SerializeField]
    public Transform LeftThigh;

    [SerializeField]
    public Transform LeftCalf;

    [SerializeField]
    public Transform LeftFoot;

    [Header("Right Leg")]
    [SerializeField]
    public Transform RightThigh;

    [SerializeField]
    public Transform RightCalf;

    [SerializeField]
    public Transform RightFoot;

    [Header("Left Arm")]
    [SerializeField]
    public Transform LeftUpperArm;

    [SerializeField]
    public Transform LeftForearm;

    [SerializeField]
    public Transform LeftHand;

    [Header("Right Arm")]
    [SerializeField]
    public Transform RightUpperArm;

    [SerializeField]
    public Transform RightForearm;

    [SerializeField]
    public Transform RightHand;

    [Header("Left Hand")]
    [SerializeField]
    public Transform LeftHandThumb;

    [SerializeField]
    public Transform LeftHandIndex;

    [SerializeField]
    public Transform LeftHandMiddle;

    [SerializeField]
    public Transform LeftHandRing;

    [SerializeField]
    public Transform LeftHandPinky;

    [Header("Right Hand")]
    [SerializeField]
    public Transform RightHandThumb;

    [SerializeField]
    public Transform RightHandIndex;

    [SerializeField]
    public Transform RightHandMiddle;

    [SerializeField]
    public Transform RightHandRing;

    [SerializeField]
    public Transform RightHandPinky;
    #endregion

    private void OnEnable()
    {
        HandInitialize();
    }

    public Transform GetTransform(EAvatarBone boneIndex)
    {
        switch (boneIndex)
        {
            case EAvatarBone.Root:
                return Root;
            case EAvatarBone.Pelvis:
                return Pelvis;
            case EAvatarBone.Spine_1:
                return Spine_1;
            case EAvatarBone.Spine_2:
                return Spine_2;
            case EAvatarBone.Spine_3:
                return Spine_3;
            case EAvatarBone.Head:
                return Head;
            case EAvatarBone.Eyes_1:
                return Eyes_1;
            case EAvatarBone.Eyes_2:
                return Eyes_2;
            case EAvatarBone.LeftThigh:
                return LeftThigh;
            case EAvatarBone.LeftCalf:
                return LeftCalf;
            case EAvatarBone.LeftFoot:
                return LeftFoot;
            case EAvatarBone.RightThigh:
                return RightThigh;
            case EAvatarBone.RightCalf:
                return RightCalf;
            case EAvatarBone.RightFoot:
                return RightFoot;
            case EAvatarBone.LeftUpperArm:
                return LeftUpperArm;
            case EAvatarBone.LeftForeArm:
                return LeftForearm;
            case EAvatarBone.LeftHand:
                return LeftHand;
            case EAvatarBone.RightUpperArm:
                return RightUpperArm;
            case EAvatarBone.RightForeArm:
                return RightForearm;
            case EAvatarBone.RightHand:
                return RightHand;
            case EAvatarBone.LeftHandThumb:
                return LeftHandThumb;
            case EAvatarBone.LeftHandIndex:
                return LeftHandIndex;
            case EAvatarBone.LeftHandMiddle:
                return LeftHandMiddle;
            case EAvatarBone.LeftHandRing:
                return LeftHandRing;
            case EAvatarBone.LeftHandPinky:
                return LeftHandPinky;
            case EAvatarBone.RightHandThumb:
                return RightHandThumb;
            case EAvatarBone.RightHandIndex:
                return RightHandIndex;
            case EAvatarBone.RightHandMiddle:
                return RightHandMiddle;
            case EAvatarBone.RightHandRing:
                return RightHandRing;
            case EAvatarBone.RightHandPinky:
                return RightHandPinky;
            case EAvatarBone.Count:
                return null;
        }
        return null;
    }

    public Transform GetHandTransform(EHandType handType)
    {
        switch (handType)
        {
            case EHandType.Left:
                return LeftHand;
            case EHandType.Right:
                return RightHand;
        }

        return null;
    }

    public Transform GetUpperArmTransform(EHandType handType)
    {
        switch (handType)
        {
            case EHandType.Left:
                return LeftUpperArm;
            case EHandType.Right:
                return RightUpperArm;
        }

        return null;
    }

    public Transform GetForeArmTransform(EHandType handType)
    {
        switch (handType)
        {
            case EHandType.Left:
                return LeftForearm;
            case EHandType.Right:
                return RightForearm;
        }

        return null;
    }

    public Transform GetFingerRootTransform(EHandType handType, EFingerType fingerType)
    {
        if (handType == EHandType.Left)
        {
            switch (fingerType)
            {
                case EFingerType.Thumb:
                    return LeftHandThumb;
                case EFingerType.Index:
                    return LeftHandIndex;
                case EFingerType.Middle:
                    return LeftHandMiddle;
                case EFingerType.Ring:
                    return LeftHandRing;
                case EFingerType.Pinky:
                    return LeftHandPinky;
            }
        }
        else if (handType == EHandType.Right)
        {
            switch (fingerType)
            {
                case EFingerType.Thumb:
                    return RightHandThumb;
                case EFingerType.Index:
                    return RightHandIndex;
                case EFingerType.Middle:
                    return RightHandMiddle;
                case EFingerType.Ring:
                    return RightHandRing;
                case EFingerType.Pinky:
                    return RightHandPinky;
            }
        }

        return null;
    }

    public bool HasHandBones(EHandType handType)
    {
        if (handType == EHandType.Left)
        {
            if (LeftHandThumb != null ||
               LeftHandIndex != null ||
               LeftHandMiddle != null ||
               LeftHandRing != null ||
               LeftHandPinky != null)
                return true;
        }
        else if (handType == EHandType.Right)
        {

            if (RightHandThumb != null ||
                RightHandIndex != null ||
                RightHandMiddle != null ||
                RightHandRing != null ||
                RightHandPinky != null)
                return true;
        }

        return false;
    }

    public bool HasEyes()
    {
        if (Eyes_1 != null && Eyes_2 != null)
            return true;

        return false;
    }

    internal int[,] handFingerCount = new int[2, 5];        // 각 손가락의 마디 개수 (3개 or 4개)
    internal Transform[,,] fingerJointTransform = new Transform[2, 5, 4];   // 각 손가락의 Transforms
    internal Vector3[,,] fingerJointLocalPosition = new Vector3[2, 5, 4];   // 각 손가락의 Transforms

    public void HandInitialize()
    {
        for (int i = 0; i < 2; ++i)
        {
            for (int j = 0; j < 5; ++j)
            {
                handFingerCount[i, j] = 0;
                for (int k = 0; k < 4; ++k)
                {
                    fingerJointTransform[i, j, k] = null;
                    fingerJointLocalPosition[i, j, k] = Vector3.zero;
                }
            }
        }

        for (int i = 0; i < 2; ++i)
        {
            for (int j = 0; j < 5; ++j)
            {
                int fingerNode = 0;
                handFingerCount[i, j] = 0;
                var temTransform = GetFingerRootTransform((EHandType)i, (EFingerType)j);
                fingerJointLocalPosition[i, j, fingerNode] = temTransform.localPosition;
                fingerJointTransform[i, j, fingerNode++] = temTransform;
                if (temTransform != null)
                {
                    handFingerCount[i, j]++;

                    while (temTransform.childCount != 0)
                    {
                        handFingerCount[i, j]++;
                        temTransform = temTransform.GetChild(0);
                        fingerJointLocalPosition[i, j, fingerNode] = temTransform.localPosition;
                        fingerJointTransform[i, j, fingerNode++] = temTransform;
                    }
                }
            }
        }

        halfQuatBufferSize = Marshal.SizeOf(typeof(AvatarNodePosition));
        halfQuatPtrBuffer = Marshal.AllocHGlobal(halfQuatBufferSize);
    }

    public Transform GetFingerEndTransform(EHandType handType, EFingerType fingerType)
    {
        return fingerJointTransform[(int)handType, (int)fingerType, handFingerCount[(int)handType, (int)fingerType] - 1];
    }

    [SerializeField]
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    struct HalfQuat
    {
        public Half quat_x;
        public Half quat_y;
        public Half quat_z;
        public Half quat_w;
    }

    byte[] byteBuffer = new byte[1500];
    IntPtr halfQuatPtrBuffer = IntPtr.Zero;
    int halfQuatBufferSize = 0;

    public void HandSerializeView(BinaryWriter bw)
    {
        int offset = 0;
        for (int i = 0; i < 2; ++i)
        {
            for (int j = 0; j < 5; ++j)
            {
                for (int k = 0; k < handFingerCount[i, j]; ++k)
                {
                    Quaternion quat = fingerJointTransform[i, j, k].localRotation;
                    HalfQuat hQuat = new HalfQuat();
                    hQuat.quat_x = new Half(quat.x);
                    hQuat.quat_y = new Half(quat.y);
                    hQuat.quat_z = new Half(quat.z);
                    hQuat.quat_w = new Half(quat.w);

                    Marshal.StructureToPtr(hQuat, halfQuatPtrBuffer, false);
                    Marshal.Copy(halfQuatPtrBuffer, byteBuffer, offset, halfQuatBufferSize);
                    offset += halfQuatBufferSize;
                }
            }
        }

        bw.Write(offset);
        bw.Write(byteBuffer, 0, offset);
    }

    public void HandDeserializeView(BinaryReader br)
    {
        int dataSize = br.ReadInt32();
        byte[] byteArray = br.ReadBytes(dataSize);

        int offset = 0;
        for (int i = 0; i < 2; ++i)
        {
            for (int j = 0; j < 5; ++j)
            {
                for (int k = 0; k < handFingerCount[i, j]; ++k)
                {
                    Marshal.Copy(byteArray, offset, halfQuatPtrBuffer, halfQuatBufferSize);
                    HalfQuat hQ = (HalfQuat)Marshal.PtrToStructure(halfQuatPtrBuffer, typeof(HalfQuat));

                    offset += halfQuatBufferSize;

                    fingerJointTransform[i, j, k].localRotation =
                        new Quaternion(hQ.quat_x, hQ.quat_y, hQ.quat_z, hQ.quat_w);
                }
            }
        }
    }
}
