using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class WwisePlayAndStop : MonoBehaviour
{
    // Assign these in the Inspector
    public AK.Wwise.Event playEvent;
    public AK.Wwise.Event stopEvent;

    private void Start()
    {
        // Play sound when scene starts
        playEvent.Post(gameObject);

        Debug.Log("Wwise sound started.");
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only trigger when player enters
        if (other.CompareTag("Player"))
        {
            // Stop the sound
            stopEvent.Post(gameObject);

            Debug.Log("Wwise sound stopped.");
        }
    }
}