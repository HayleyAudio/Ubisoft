using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaStateTrigger : MonoBehaviour
{
    [Header("Wwise States")]
    public AK.Wwise.State enterState;
    public AK.Wwise.State exitState;

    public static string debugCurrentState = "outside";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (enterState != null)
            {
                enterState.SetValue();
                debugCurrentState = enterState.Name;
                Debug.Log("Entered Wwise State: " + enterState.Name);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (exitState != null)
            {
                exitState.SetValue();
                debugCurrentState = exitState.Name;
                Debug.Log("Exited to Wwise State: " + exitState.Name);
            }
        }
    }
}