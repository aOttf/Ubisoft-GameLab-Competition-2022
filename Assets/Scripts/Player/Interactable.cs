using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class Interactable : MonoBehaviour
{
    public UnityEvent onInteract;
    public int ID;
    public Sprite interactIcon;
    // Start is called before the first frame update
    void Start()
    {
        ID = Random.Range(0,999999);
    }

    public void Interact()
    {
        onInteract.Invoke();
    }
}
