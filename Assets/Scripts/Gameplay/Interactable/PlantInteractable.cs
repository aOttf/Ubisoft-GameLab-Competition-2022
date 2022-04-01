using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantInteractable : MinigameInteractable
{
    [SyncVar] [SerializeField] public PlayerCustomization.COLOUR colour;
    [SyncVar] [SerializeField] public PlayerCustomization.PLANT plant;

    // Start is called before the first frame update
    void Start()
    {
        // When the player interacts with this object it'll start the minigame
        _unityEvent.AddListener(() =>
        {
            IsInteractable = false;
            MinigameManager.Instance.StartMinigame(this, MinigamePrefab, out var minigame);
            minigame.OnCompleteMinigame.AddListener(() => OnCompleteMinigame.Invoke());
            minigame.OnCompleteMinigame.AddListener(() => IsInteractable = true);

            // TODO: customization thing
            (minigame as PlantMinigame).SetPlantSelection(colour, plant);

            if (requiredObject != Holdable.Type.NONE)
            {
                // Should never be null but could be a bug and it'll hang the player
                if (Player.Instance.heldObject != null)
                {
                    // Try consuming the required object after use
                    minigame.OnCompleteMinigame.AddListener(() => Player.Instance.heldObject.Consume());
                }
            }
        });
    }

    [Server]
    public void SetSelection(PlayerCustomization.PLANT selection)
    {
        plant = selection;
    }
}