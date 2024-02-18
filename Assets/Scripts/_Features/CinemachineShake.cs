using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Cinemachine;

namespace Features
{
    public class CinemachineShake : MonoBehaviour
    {
        public static CinemachineShake Instance { get; private set; }

        private CinemachineVirtualCamera cinemachineVirtualCamera;
        private CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin;
        private float duration, durationTotal;
        private float startingIntensity;

        private void Awake()
        {
            Instance = this;
            cinemachineVirtualCamera = GetComponent<CinemachineVirtualCamera>();
            cinemachineBasicMultiChannelPerlin = cinemachineVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }

        private void Update()
        {
            if (duration > 0)
            {
                duration -= Time.deltaTime;
                cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = math.lerp(startingIntensity, 0f, 1 - (duration / durationTotal));
            }
        }

        public void ShakeCamera(float intensity, float durationP)
        {
            //Debug.LogWarning("Shake: " + intensity + " : " + durationP);
            cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = intensity;

            startingIntensity = intensity;
            durationTotal = durationP;
            duration = durationP;
        }
    }
}