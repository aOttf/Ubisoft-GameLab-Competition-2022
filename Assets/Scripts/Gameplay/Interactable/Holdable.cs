using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static InputManager;

public class Holdable : Interactable
{
    [SyncVar] private GameObject _parent;
    [SyncVar] private GameObject _offsetPosition;

    private Rigidbody _rb;
    private Collider _collider;

    [SerializeField] public float throwForce = 1000f;

    [SerializeField] public PlayerCustomization.COLOUR colour;

    protected override InputCommand InputCommand { get => InputCommand.PickUp; }

    public Type type;

    public enum Type
    {
        NONE,
        WATERING_CAN,
        ASSIGNMENT,
        SOCK
    }

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();

        // When a holdable item is interacting with, pick it up
        _unityEvent.AddListener(OnInteract);
    }

    public void OnInteract()
    {
        var canPickUp = HasFocus && Player.Instance.heldObject == null;

        Debug.Log($"Trying to pick up {gameObject.name}, success: {canPickUp}");

        if (canPickUp) Player.Instance.CmdGrab(this);
    }

    [Command]
    public void CmdToss(Vector3 direction)
    {
        Toss(direction);
    }

    [Server]
    public void Toss(Vector3 direction)
    {
        _rb.isKinematic = false;
        _rb.AddForce(direction * throwForce);
    }

    public void Grab(NetworkBehaviour grabber)
    {
        _parent = grabber.gameObject;

        if (isServer)
        {
            var netID = gameObject.GetComponent<NetworkIdentity>();
            netID.RemoveClientAuthority();
            netID.AssignClientAuthority(grabber.connectionToClient);
        }
        IsInteractable = false;
        _collider.enabled = false;
        foreach (var collider in GetComponentsInChildren<Collider>())
        {
            collider.enabled = false;
        }

        _rb.isKinematic = true;
    }

    public void Drop()
    {
        Debug.Log("Drop item");

        _parent = null;
        if (isServer) gameObject.GetComponent<NetworkIdentity>().RemoveClientAuthority();

        IsInteractable = true;
        _rb.isKinematic = false;
        _collider.enabled = true;

        foreach (var collider in GetComponentsInChildren<Collider>())
        {
            collider.enabled = true;
        }
    }

    /* Get rid of certain holdable items after using them for a minigame */
    public void Consume()
    {
        // Check if its a consumable type
        var isConsumable = false;
        switch (type)
        {
            case Type.ASSIGNMENT:
            case Type.SOCK:
                isConsumable = true;
                break;
        }

        if (isConsumable)
        {
            if (isConsumable)
            {
                NetworkDestroy(gameObject);
                Player.Instance.heldObject = null;
            }
        }
    }

    private void NetworkDestroy(GameObject obj)
    {
        if (isServer)
        {
            RpcNetworkDestroy(obj);
        }
        else
        {
            CmdNetworkDestroy(obj);
        }
    }

    [Command]
    private void CmdNetworkDestroy(GameObject obj)
    {
        RpcNetworkDestroy(obj);
    }

    [ClientRpc]
    private void RpcNetworkDestroy(GameObject obj)
    {
        GameObject.Destroy(obj);
    }
}
