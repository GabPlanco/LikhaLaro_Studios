using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    private PlayerControls playerControls;
    private PlayerControls.OnFeetActions onFeet;

    // Start is called before the first frame update
    void Awake()
    {
        playerControls = new PlayerControls();
        onFeet = playerControls.OnFeet;
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
