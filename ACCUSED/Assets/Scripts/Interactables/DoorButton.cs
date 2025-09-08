using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class DoorButton : Interactables
{
    [SerializeField] private GameObject door;
    private Animator doorAnimator;

    // Network-synced variable for door open state
    private NetworkVariable<bool> doorOpen = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private void Awake()
    {
        doorAnimator = door.GetComponent<Animator>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Listen for changes to the door state
        doorOpen.OnValueChanged += OnDoorStateChanged;

        // Apply current state (important for late-joiners)
        doorAnimator.SetBool("BukasBa", doorOpen.Value);
    }

    private new void OnDestroy()
    {
        doorOpen.OnValueChanged -= OnDoorStateChanged;
    }

    protected override void Interact()
    {
        // Only the server should change the state
        if (IsServer)
        {
            doorOpen.Value = !doorOpen.Value;
        }
        else
        {
            // Tell the server to toggle door
            ToggleDoorServerRpc();
        }
    }

    // ServerRpc to toggle door state
    [ServerRpc(RequireOwnership = false)]
    private void ToggleDoorServerRpc()
    {
        doorOpen.Value = !doorOpen.Value;
    }

    // Called on all clients when door state changes
    private void OnDoorStateChanged(bool oldValue, bool newValue)
    {
        doorAnimator.SetBool("BukasBa", newValue);
    }

    /* // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // dito nakalagay yung mga interaction sa button
    protected override void Interact()
    {
        doorOpen = !doorOpen;
        door.GetComponent<Animator>().SetBool("BukasBa", doorOpen);
    } */
}
