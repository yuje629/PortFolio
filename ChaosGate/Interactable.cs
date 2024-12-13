using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    public UnityEvent onInteractableEnter;
    public UnityEvent onInteractableExit;
    public UnityEvent onInteractable;

    private bool isTouch;

    private void Update()
    {
        if(isTouch)
        {
            if(Input.GetKeyDown(KeyCode.G))
            {
                if (onInteractable != null)
                    onInteractable.Invoke();
            }
        }
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            isTouch = true;
            if (onInteractableEnter != null)
                onInteractableEnter.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            isTouch = false;
            if (onInteractableExit != null)
                onInteractableExit.Invoke();
        }
    }
}
