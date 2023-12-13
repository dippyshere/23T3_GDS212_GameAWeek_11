using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraTrackController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CinemachineVirtualCamera _camera;
    [SerializeField] private CinemachinePathBase _track;
    [SerializeField] private BoxCollider _collider;
    private GameObject _player;

    private void Start()
    {
        _collider.enabled = false;
        _player = FindObjectOfType<PlayerController>().gameObject;
    }

    private void OnTriggerEnter(Collider other)
    {
        _collider.enabled = true;
        if (_track != null)
        {
            _camera.AddCinemachineComponent<CinemachineTrackedDolly>();
            CinemachineTrackedDolly cinemachineTrackedDolly = _camera.GetComponent<CinemachineTrackedDolly>();
            cinemachineTrackedDolly.m_Path = _track;
            _camera.m_Follow = _player.transform;
            cinemachineTrackedDolly.m_AutoDolly.m_Enabled = true;
            cinemachineTrackedDolly.m_XDamping = 1;
            cinemachineTrackedDolly.m_YDamping = 1;
            cinemachineTrackedDolly.m_ZDamping = 1;
            cinemachineTrackedDolly.m_PathOffset = new Vector3(0, 10, 0);
            cinemachineTrackedDolly.m_AutoDolly.m_SearchRadius = 5;
        }
        else
        {
            _camera.DestroyCinemachineComponent<CinemachineTrackedDolly>();
            _camera.m_Follow = transform;
            CinemachineFramingTransposer cinemachineFramingConposer = _camera.AddCinemachineComponent<CinemachineFramingTransposer>();
            cinemachineFramingConposer.m_UnlimitedSoftZone = true;
            cinemachineFramingConposer.m_TargetMovementOnly = false;
        }
    }
}
