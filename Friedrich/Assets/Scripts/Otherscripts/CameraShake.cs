using System.Collections;
using UnityEngine;
using Cinemachine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake instance;
    [SerializeField] private float globalShakeForce = 0.2f; // Lowered for subtle shake

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void TriggerCameraShake(CinemachineImpulseSource impulseSource)
    {
        impulseSource.GenerateImpulseWithForce(globalShakeForce);
    }
}
