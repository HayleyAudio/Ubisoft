using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AK.Wwise;   // IMPORTANT: Wwise namespace

namespace Gamekit3D.GameCommands
{
    public class SendOnTriggerEnter : TriggerCommand
    {
        public LayerMask layers;

        // Drag your Wwise Event here in Inspector
        public AK.Wwise.Event triggerSound;

        void OnTriggerEnter(Collider other)
        {
            if (0 != (layers.value & 1 << other.gameObject.layer))
            {
                // Play Wwise sound
                if (triggerSound != null)
                {
                    triggerSound.Post(gameObject);
                }

                Send();
            }
        }
    }
}