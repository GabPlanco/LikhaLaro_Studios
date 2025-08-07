using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private PlayerControls playerControls;
    private PlayerControls.OnFeetActions onFeet;

    private PlayerMovement movement;
    private PlayerLook look;

    // Start is called before the first frame update
    void Awake()
    {
        playerControls = new PlayerControls();
        onFeet = playerControls.OnFeet;

        movement = GetComponent<PlayerMovement>();
        look = GetComponent<PlayerLook>();

        onFeet.Jump.performed += ctx => movement.Jump();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        movement.ProcessMove(onFeet.Walk.ReadValue<Vector2>());
    }

    private void LateUpdate()
    {
        look.ProcessLook(onFeet.Look.ReadValue<Vector2>());
    }

    private void OnEnable()
    {
        onFeet.Enable();
    }

    private void OnDisable()
    {
        onFeet.Disable();
    }
}
