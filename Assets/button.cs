using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class UIButtonWwise : MonoBehaviour
{
    // Assign your Wwise Event in the Inspector
    public AK.Wwise.Event buttonSound;

    // This function gets called by the UI Button
    public void PlayButtonSound()
    {
        if (buttonSound != null)
        {
            buttonSound.Post(gameObject);
            Debug.Log("UI button sound played.");
        }
    }
}