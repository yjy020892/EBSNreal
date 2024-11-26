using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using UnityEngine;

using SPVR.Core;
using SPVR.Model;

[SerializeField]
[StructLayout(LayoutKind.Sequential, Pack = 2)]
struct AvatarNodePosition
{
    public byte nodeIndex;
    public Vector3 position;
}

[SerializeField]
[StructLayout(LayoutKind.Sequential, Pack = 2)]
struct AvatarNodeRotation
{
    public byte nodeIndex;
    public Half quat_x;
    public Half quat_y;
    public Half quat_z;
    public Half quat_w;
}

[SerializeField]
[StructLayout(LayoutKind.Sequential, Pack = 2)]
struct AvatarHeaderStruct
{
    public int updatedPositionCount;
    public int updatedRotationCount;
}


[RequireComponent(typeof(BoneReferences))]
public class SyncAvatar : SyncBehaviour
{
    internal bool isInitialize = false;

    internal Dictionary<EAvatarBone, Vector3> oldBoneLocalPosition = new Dictionary<EAvatarBone, Vector3>();
    internal Dictionary<EAvatarBone, Quaternion> oldBoneLocalRotation = new Dictionary<EAvatarBone, Quaternion>();

    IntPtr headerPtrBuffer = IntPtr.Zero;
    IntPtr positionPtrBuffer = IntPtr.Zero;
    IntPtr rotationPtrBuffer = IntPtr.Zero;

    int headerBufferSize = 0;
    int positionBufferSize = 0;
    int rotationBufferSize = 0;
    const int maxPacketSize = 1100;

    AvatarHeaderStruct avatarHeader = new AvatarHeaderStruct();
    AvatarNodePosition[] positions = new AvatarNodePosition[(int)EAvatarBone.Count];
    AvatarNodeRotation[] rotations = new AvatarNodeRotation[(int)EAvatarBone.Count];
    bool[] position_Dirty = new bool[(int)EAvatarBone.Count];
    bool[] rotation_Dirty = new bool[(int)EAvatarBone.Count];

    byte[] byteBuffer = new byte[1500];

    BoneReferences boneRef = null;
    void OnEnable()
    {
        boneRef = GetComponent<BoneReferences>();

        if (isInitialize == false)
        {
            for (int i = 0; i < (int)EAvatarBone.Count; ++i)
            {
                oldBoneLocalPosition[(EAvatarBone)i] = Vector3.zero;
                oldBoneLocalRotation[(EAvatarBone)i] = Quaternion.identity;
            }

            isInitialize = true;
        }

        headerBufferSize = Marshal.SizeOf(avatarHeader);
        headerPtrBuffer = Marshal.AllocHGlobal(headerBufferSize);

        positionBufferSize = Marshal.SizeOf(typeof(AvatarNodePosition));
        positionPtrBuffer = Marshal.AllocHGlobal(positionBufferSize);

        rotationBufferSize = Marshal.SizeOf(typeof(AvatarNodeRotation));
        rotationPtrBuffer = Marshal.AllocHGlobal(rotationBufferSize);
    }

    void Update()
    {
        IsDirty = true;
    }

    void OnDisable()
    {

    }

    protected override void Reset()
    {
    }

    public override void SerializeView(BinaryWriter bw)
    {
        int updatedPositionCount = 0;
        int updatedRotationCount = 0;

        for (int i = 0; i < (int)EAvatarBone.Count; ++i)
        {
            position_Dirty[i] = false;
            rotation_Dirty[i] = false;
        }
        for (int boneIndex = 0; boneIndex < (int)EAvatarBone.LeftHandThumb; ++boneIndex)
        {
            int packetSize = headerBufferSize + (positionBufferSize * (updatedPositionCount + 1)) + (rotationBufferSize * (updatedRotationCount + 1));

            if (packetSize > maxPacketSize)
                break;

            Transform temBone = boneRef.GetTransform((EAvatarBone)boneIndex);

            if (temBone == null)
                continue;

            if (!VectorUtil.Compare(oldBoneLocalPosition[(EAvatarBone)boneIndex], temBone.localPosition))
            {
                updatedPositionCount++;
                oldBoneLocalPosition[(EAvatarBone)boneIndex] = temBone.localPosition;
                position_Dirty[boneIndex] = true;
                positions[boneIndex].nodeIndex = (byte)boneIndex;
                positions[boneIndex].position = temBone.localPosition;
            }

            Quaternion temCompQuat = oldBoneLocalRotation[(EAvatarBone)boneIndex];

            updatedRotationCount++;
            oldBoneLocalRotation[(EAvatarBone)boneIndex] = temBone.localRotation;
            rotation_Dirty[boneIndex] = true;
            rotations[boneIndex].nodeIndex = (byte)boneIndex;

            Quaternion temQuat = temBone.localRotation;
            rotations[boneIndex].quat_x = new Half(temQuat.x);
            rotations[boneIndex].quat_y = new Half(temQuat.y);
            rotations[boneIndex].quat_z = new Half(temQuat.z);
            rotations[boneIndex].quat_w = new Half(temQuat.w);
        }

        avatarHeader.updatedPositionCount = updatedPositionCount;
        avatarHeader.updatedRotationCount = updatedRotationCount;

        int dataSize = headerBufferSize + (positionBufferSize * avatarHeader.updatedPositionCount) + (rotationBufferSize * avatarHeader.updatedRotationCount);

        bw.Write(dataSize);

        int offset = 0;
        Marshal.StructureToPtr(avatarHeader, headerPtrBuffer, false);
        Marshal.Copy(headerPtrBuffer, byteBuffer, offset, headerBufferSize);
        offset += headerBufferSize;

        for (int boneIndex = 0; boneIndex < (int)EAvatarBone.LeftHandThumb; ++boneIndex)
        {
            if (position_Dirty[boneIndex] == true)
            {
                Marshal.StructureToPtr(positions[boneIndex], positionPtrBuffer, false);
                Marshal.Copy(positionPtrBuffer, byteBuffer, offset, positionBufferSize);
                offset += positionBufferSize;
            }
        }

        for (int boneIndex = 0; boneIndex < (int)EAvatarBone.LeftHandThumb; ++boneIndex)
        {
            if (rotation_Dirty[boneIndex] == true)
            {
                Marshal.StructureToPtr(rotations[boneIndex], rotationPtrBuffer, false);
                Marshal.Copy(rotationPtrBuffer, byteBuffer, offset, rotationBufferSize);
                offset += rotationBufferSize;
            }
        }

        bw.Write(byteBuffer, 0, dataSize);
        boneRef.HandSerializeView(bw);
    }

    public override void DeserializeView(BinaryReader br)
    {
        int dataSize = br.ReadInt32();
        byte[] byteArray = br.ReadBytes(dataSize);

        int offset = 0;
        Marshal.Copy(byteArray, offset, headerPtrBuffer, headerBufferSize);
        avatarHeader = (AvatarHeaderStruct)Marshal.PtrToStructure(headerPtrBuffer, typeof(AvatarHeaderStruct));
        offset += headerBufferSize;

        try
        {
            for (int i = 0; i < avatarHeader.updatedPositionCount; ++i)
            {
                Marshal.Copy(byteArray, offset, positionPtrBuffer, positionBufferSize);
                AvatarNodePosition anp = (AvatarNodePosition)Marshal.PtrToStructure(positionPtrBuffer, typeof(AvatarNodePosition));
                offset += positionBufferSize;

                Transform temBone = boneRef.GetTransform((EAvatarBone)anp.nodeIndex);
                temBone.localPosition = anp.position;
            }

            for (int i = 0; i < avatarHeader.updatedRotationCount; ++i)
            {
                Marshal.Copy(byteArray, offset, rotationPtrBuffer, rotationBufferSize);
                AvatarNodeRotation anr = (AvatarNodeRotation)Marshal.PtrToStructure(rotationPtrBuffer, typeof(AvatarNodeRotation));
                offset += rotationBufferSize;

                Transform temBone = boneRef.GetTransform((EAvatarBone)anr.nodeIndex);
                temBone.localRotation = new Quaternion(anr.quat_x, anr.quat_y, anr.quat_z, anr.quat_w);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message + " => " + e.StackTrace);
        }

        boneRef.HandDeserializeView(br);
    }
}
