using UnityEngine;

public class debugging : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("ENTER TRIGGER: " + other.name);

        if (other.CompareTag("Player"))
        {
            Debug.Log("PLAYER ENTERED ZONE");
        }
        else
        {
            Debug.Log("Something else entered: " + other.tag);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("EXIT TRIGGER: " + other.name);
    }
}