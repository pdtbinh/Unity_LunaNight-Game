using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CamShake : MonoBehaviour
{
    private CinemachineVirtualCamera vc;
    private CinemachineBasicMultiChannelPerlin channel;

    private float shakeTime;      // numerator
    private float shakeTimeTotal; // denominator

    private float startingIntensity = 0f;

    // Start is called before the first frame update
    void Start()
    {
        vc = GetComponent<CinemachineVirtualCamera>();
        channel = vc.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    // Update is called once per frame
    void Update()
    {
        if (shakeTime > 0f)
        {
            shakeTime -= Time.deltaTime;

            // To make sure this variable clamps between 0f and 1f
            float interpolatedCoefficient = 1 - (shakeTime / shakeTimeTotal);
            interpolatedCoefficient = (interpolatedCoefficient > 1f) ? 1f : interpolatedCoefficient;

            channel.m_AmplitudeGain = Mathf.Lerp(startingIntensity, 0f, interpolatedCoefficient);

            // To make sire amplitude does not go below 0f
            if (shakeTime < 0f) { channel.m_AmplitudeGain = 0f; }
        }
    }

    public void ShakeCamera(float intensity, int freq, float duration)
    {
        channel.m_AmplitudeGain = intensity;
        startingIntensity       = intensity;
        channel.m_FrequencyGain = freq;
        shakeTime               = duration;
        shakeTimeTotal          = duration;
    }

    public void FollowPlayer(Transform player)
    {
        vc.Follow = player;
    }
}
