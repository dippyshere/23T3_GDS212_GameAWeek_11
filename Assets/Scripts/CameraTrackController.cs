using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

public class CameraTrackController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CinemachineCamera _camera;
    [SerializeField] private CinemachineCamera _cameraArea1;
    [SerializeField] private BoxCollider _collider;
    private PlayerController _player;
    [SerializeField] private bool loadNextArea = false;

    private void Start()
    {
        _collider.enabled = false;
        _player = FindAnyObjectByType<PlayerController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_collider.enabled)
        {
            return;
        }
        _collider.enabled = true;
        _camera.gameObject.SetActive(true);
        _cameraArea1.gameObject.SetActive(false);
        if (loadNextArea)
        {
            _player.LoadNextLevelByTrigger();
        }
    }
}
