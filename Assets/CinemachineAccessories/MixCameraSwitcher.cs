using System;
using Unity.Cinemachine;
using UnityEngine;

namespace CinemachineAccessories
{
    [RequireComponent(typeof(CinemachineMixingCamera))]
    public class MixCameraSwitcher : CinemachineExtension
    {
        
        private CinemachineMixingCamera mixCamera;

        public CinemachineMixingCamera MixCamera
        {
            get
            {
                mixCamera = mixCamera == null ? GetComponent<CinemachineMixingCamera>() : mixCamera;
                return mixCamera;
            }
        }
        private int curPriority;
        public int CurrentPriority => curPriority;
        public float lerpSpeed=5f;
        
        private readonly int[] priorities = { 1, 0, 0, 0, 0, 0, 0, 0 };
        public Action<int> onSetPriorityCallback;


        public void SetTarget(Transform target)
        {
            foreach (var childCam in MixCamera.ChildCameras)
            {
                childCam.Follow = target;
                childCam.LookAt = target;
            }
        }

        public void SetPriority(int index)
        {
            // for (var i = 0; i < priorities.Length; i++) priorities[i] = 0;
            // priorities[index] = 1;
            curPriority = index;
            onSetPriorityCallback?.Invoke(index);
        }

        public void SetNext()
        {
            var count = MixCamera.ChildCameras.Count;
            curPriority = ++curPriority % count;
        }
        
        public void SetPrevious()
        {
            var count = MixCamera.ChildCameras.Count;
            curPriority = --curPriority % count;
        }

        private void UpdateCameraPriority()
        {
            for (int i = 0; i < 8; i++)
            {
                var target = curPriority == i ? 1 : 0;
                var w = Mathf.Lerp(MixCamera.GetWeight(i), target, lerpSpeed * Time.deltaTime);
                MixCamera.SetWeight(i,w);
            }
            // MixCamera.m_Weight0 = Mathf.Abs(MixCamera.m_Weight0 - priorities[0]) > 0.001
            //     ? Mathf.Lerp(MixCamera.m_Weight0, priorities[0], 5f * Time.deltaTime)
            //     : priorities[0];
            // MixCamera.m_Weight1 = Mathf.Abs(MixCamera.m_Weight1 - priorities[1]) > 0.001
            //     ? Mathf.Lerp(MixCamera.m_Weight1, priorities[1], 5f * Time.deltaTime)
            //     : priorities[1];
            // MixCamera.m_Weight2 = Mathf.Abs(MixCamera.m_Weight2 - priorities[2]) > 0.001
            //     ? Mathf.Lerp(MixCamera.m_Weight2, priorities[2], 5f * Time.deltaTime)
            //     : priorities[2];
            // MixCamera.m_Weight3 = Mathf.Abs(MixCamera.m_Weight3 - priorities[3]) > 0.001
            //     ? Mathf.Lerp(MixCamera.m_Weight3, priorities[3], 5f * Time.deltaTime)
            //     : priorities[3];
            // MixCamera.m_Weight4 = Mathf.Abs(MixCamera.m_Weight4 - priorities[4]) > 0.001
            //     ? Mathf.Lerp(MixCamera.m_Weight4, priorities[4], 5f * Time.deltaTime)
            //     : priorities[4];
            // MixCamera.m_Weight5 = Mathf.Abs(MixCamera.m_Weight5 - priorities[5]) > 0.001
            //     ? Mathf.Lerp(MixCamera.m_Weight5, priorities[5], 5f * Time.deltaTime)
            //     : priorities[5];
            // MixCamera.m_Weight6 = Mathf.Abs(MixCamera.m_Weight6 - priorities[6]) > 0.001
            //     ? Mathf.Lerp(MixCamera.m_Weight6, priorities[6], 5f * Time.deltaTime)
            //     : priorities[6];
            // MixCamera.m_Weight7 = Mathf.Abs(MixCamera.m_Weight7 - priorities[7]) > 0.001
            //     ? Mathf.Lerp(MixCamera.m_Weight7, priorities[7], 5f * Time.deltaTime)
            //     : priorities[7];
        }

        protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
        {
            if (stage==CinemachineCore.Stage.Body)
            {
                UpdateCameraPriority();
            }
        }
    }
}