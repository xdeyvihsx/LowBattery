using Unity.Cinemachine;
using UnityEngine;

public class CameraFollowPayer : MonoBehaviour
{

    [SerializeField] private CinemachineCamera cinemachineCamera;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        FollwPlayer();
    }

    private void FollwPlayer()
    {
        PlayerMovement player = FindAnyObjectByType<PlayerMovement>();

        if (player == null)
        {
            Debug.LogError("Player not found in the scene!");
            return;
        }

        Transform playerTransform = player.transform;
        cinemachineCamera.Follow = playerTransform;
    }

    // Update is called once per frame
    private void Update()
    {
        
    }
}
