using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WwiseInitState : MonoBehaviour
{
    public AK.Wwise.State startingState;

    void Start()
    {
        if (startingState != null)
        {
            startingState.SetValue();
            Debug.Log("Initial Wwise state set: " + startingState.Name);
        }
    }
}