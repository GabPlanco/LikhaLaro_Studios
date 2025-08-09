using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.LowLevel;

public class InputManager : MonoBehaviour
{
    private PlayerControls playerControls;
    private PlayerControls.OnFeetActions onFeet;

    private PlayerMovement movement;
    private PlayerLook look;
    public RectTransform joystickArea;

    // Start is called before the first frame update
    void Awake()
    {
        playerControls = new PlayerControls();
        onFeet = playerControls.OnFeet;

        movement = GetComponent<PlayerMovement>();
        look = GetComponent<PlayerLook>();

        onFeet.Jump.performed += ctx => movement.Jump();
    }

    void Update()
    {
        foreach (Touch touch in Input.touches)
        {
            // Skip if touch is inside joystick area
            if (RectTransformUtility.RectangleContainsScreenPoint(joystickArea, touch.position))
                continue;

            if (touch.phase == UnityEngine.TouchPhase.Moved)
            {
                Vector2 delta = touch.deltaPosition;
                look.ProcessLook(delta); // Call your existing look method
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        movement.ProcessMove(onFeet.Walk.ReadValue<Vector2>());
    }

    
    private void LateUpdate()
    {
       // look.ProcessLook(onFeet.Look.ReadValue<Vector2>()); // Try lang 
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
