using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private GameObject kratos;
    private CharacterController controller;
    private Animator lakadNiKratos;
    private Vector3 playerVelocity;
    private bool isGrounded;
    public float speed = 5f;
    public float gravity = -9.8f;
    public float jumpHeight = 3f;

    // Networked walking flag (replicated to all clients)
    private NetworkVariable<bool> isWalking = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner
    );

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        lakadNiKratos = kratos.GetComponent<Animator>();

        lakadNiKratos.SetBool("NaglalakadBa", false);

        // Make all players (including others) update their Animator when this changes
        isWalking.OnValueChanged += (oldValue, newValue) =>
        {
            lakadNiKratos.SetBool("NaglalakadBa", newValue);
        };
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;

        isGrounded = controller.isGrounded;
    }

    public void ProcessMove (Vector2 input)
    {
        if (SceneManager.GetActiveScene().name == "MainMenu") return;
        if (SceneManager.GetActiveScene().name == "LoadingScene") return;
        if (MeetingPanel.meetingPanelIsActive == true) return;
        if (!IsOwner) return;

        // Determine if player is actually walking
        bool naglalakadNga = input.sqrMagnitude > 0.01f;

        // Only update when state changes (prevents flicker)
        if (isWalking.Value != naglalakadNga)
        {
            isWalking.Value = naglalakadNga;
        }

        Vector3 moveDirection = Vector3.zero;
        moveDirection.x = input.x;
        moveDirection.z = input.y;
        controller.Move(transform.TransformDirection(moveDirection) * speed * Time.deltaTime);
        playerVelocity.y += gravity * Time.deltaTime;
        if (isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f;
        }
        controller.Move(playerVelocity * Time.deltaTime);
        // Debug.Log(playerVelocity.y);
    }

    public void Jump()
    {
        if (!IsOwner) return;

        if (isGrounded)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -3.0f * gravity);
        }
    }

}
