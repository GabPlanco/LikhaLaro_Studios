using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private PlayerControls playerControls;
    private PlayerControls.OnFeetActions onFeet;

    private PlayerMovement movement;

    // Start is called before the first frame update
    void Awake()
    {
        playerControls = new PlayerControls();
        onFeet = playerControls.OnFeet;
        movement = GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        movement.ProcessMove(onFeet.WASD.ReadValue<Vector2>());
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
