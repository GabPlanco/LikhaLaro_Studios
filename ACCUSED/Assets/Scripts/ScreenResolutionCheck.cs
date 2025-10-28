using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenResolutionCheck : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Screen Resolution: " + Screen.width + "x" + Screen.height);
    }
}
