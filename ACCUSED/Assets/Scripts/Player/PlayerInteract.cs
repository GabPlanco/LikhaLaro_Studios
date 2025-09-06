using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerInteract : NetworkBehaviour
{
    private Camera cam;
    [SerializeField] private float distance = 3f;
    [SerializeField] private LayerMask mask;
    // private PlayerUI playerUI;
    private InputManager inputManager;
    
    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<PlayerLook>().cam;
        // playerUI = GetComponent<PlayerUI>();
        inputManager = GetComponent<InputManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;

        // sa gitna ng camera, may linya, palabas
        // playerUI.UpdateText(string.Empty);
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        Debug.DrawRay(ray.origin, ray.direction * distance);

        RaycastHit hitInfo; 
        if (Physics.Raycast(ray, out hitInfo, distance, mask))
        {
            if (hitInfo.collider.GetComponent<Interactables>() != null) 
            {
                Interactables interactables = hitInfo.collider.GetComponent<Interactables>();
                // playerUI.UpdateText(interactables.promptMessage);
                if (inputManager.onFeet.Interact.triggered)
                {
                    interactables.BaseInteract();
                }
            }
        }
    }
}
