using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorButton : Interactables
{
    [SerializeField] private GameObject door;
    private bool doorOpen;
    
    // Start is called before the first frame update
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
    }
}
