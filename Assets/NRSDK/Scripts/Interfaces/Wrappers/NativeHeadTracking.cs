﻿/****************************************************************************
* Copyright 2019 Nreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.nreal.ai/        
* 
*****************************************************************************/

namespace NRKernal
{
    using System;
    using System.Runtime.InteropServices;
    using UnityEngine;

    /// <summary>
    /// 6-dof Head Tracking's Native API .
    /// </summary>
    internal partial class NativeHeadTracking
    {
        private NativeInterface m_NativeInterface;
        internal UInt64 headTrackingHandle = 0;

        public NativeHeadTracking(NativeInterface nativeInterface)
        {
            m_NativeInterface = nativeInterface;
        }

        public bool Create()
        {
            if (m_NativeInterface.TrackingHandle == 0)
            {
                return false;
            }
            var result = NativeApi.NRHeadTrackingCreate(m_NativeInterface.TrackingHandle, ref headTrackingHandle);
            NativeErrorListener.Check(result, this, "Create");
            return result == NativeResult.Success;
        }

        public bool GetHeadPose(ref Pose pose, UInt64 timestamp = 0, UInt64 predict = 0)
        {
            if (headTrackingHandle == 0)
            {
                pose = Pose.identity;
                return false;
            }
            UInt64 headPoseHandle = 0;
            UInt64 hmd_nanos = 0;
            var result = NativeApi.NRTrackingGetHMDTimeNanos(m_NativeInterface.TrackingHandle, ref hmd_nanos);
            if (result != NativeResult.Success)
            {
                return false;
            }
            if (timestamp != 0)
            {
                hmd_nanos = timestamp;
            }
            else if (predict != 0)
            {
                hmd_nanos -= predict;
            }
            else
            {
                UInt64 predict_time = 0;
                NativeApi.NRHeadTrackingGetRecommendPredictTime(m_NativeInterface.TrackingHandle, headTrackingHandle, ref predict_time);
                hmd_nanos += predict_time;
            }

            var acquireTrackingPoseResult = NativeApi.NRHeadTrackingAcquireTrackingPose(m_NativeInterface.TrackingHandle, headTrackingHandle, hmd_nanos, ref headPoseHandle);
            if (acquireTrackingPoseResult != NativeResult.Success)
            {
                return false;
            }

            NativeMat4f headpos_native = new NativeMat4f(Matrix4x4.identity);
            var getPoseResult = NativeApi.NRTrackingPoseGetPose(m_NativeInterface.TrackingHandle, headPoseHandle, ref headpos_native);

            if (getPoseResult != NativeResult.Success)
            {
                return false;
            }
            ConversionUtility.ApiPoseToUnityPose(headpos_native, out pose);
            NativeApi.NRTrackingPoseDestroy(m_NativeInterface.TrackingHandle, headPoseHandle);
            return (acquireTrackingPoseResult == NativeResult.Success) && (getPoseResult == NativeResult.Success);
        }

        public LostTrackingReason GetTrackingLostReason()
        {
            if (headTrackingHandle == 0)
            {
                return LostTrackingReason.INITIALIZING;
            }
            LostTrackingReason lost_tracking_reason = LostTrackingReason.NONE;
            var result = NativeApi.NRTrackingPoseGetTrackingReason(m_NativeInterface.TrackingHandle, headTrackingHandle, ref lost_tracking_reason);
            if (result != NativeResult.TrackingNotRunning)
            {
                NativeErrorListener.Check(result, this, "GetTrackingLostReason");
            }
            return lost_tracking_reason;
        }

        public void Destroy()
        {
            if (headTrackingHandle == 0)
            {
                return;
            }
            var result = NativeApi.NRHeadTrackingDestroy(m_NativeInterface.TrackingHandle, headTrackingHandle);
            headTrackingHandle = 0;
            NativeErrorListener.Check(result, this, "Destroy");
        }

        private partial struct NativeApi
        {
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRHeadTrackingCreate(UInt64 tracking_handle,
                ref UInt64 outHeadTrackingHandle);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRTrackingGetHMDTimeNanos(UInt64 tracking_handle,
                ref UInt64 out_hmd_time_nanos);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRHeadTrackingGetRecommendPredictTime(
                UInt64 tracking_handle, UInt64 head_tracking_handle, ref UInt64 out_predict_time_nanos);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRHeadTrackingAcquireTrackingPose(UInt64 sessionHandle,
                UInt64 head_tracking_handle, UInt64 hmd_time_nanos, ref UInt64 out_tracking_pose_handle);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRTrackingPoseGetPose(UInt64 tracking_handle,
                UInt64 tracking_pose_handle, ref NativeMat4f out_pose);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRTrackingPoseGetTrackingReason(UInt64 tracking_handle,
                UInt64 tracking_pose_handle, ref LostTrackingReason out_tracking_reason);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRTrackingPoseDestroy(UInt64 tracking_handle, UInt64 tracking_pose_handle);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRHeadTrackingDestroy(UInt64 tracking_handle, UInt64 head_tracking_handle);
        };
    }
}
