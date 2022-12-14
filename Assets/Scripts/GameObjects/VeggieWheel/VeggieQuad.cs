using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VeggieQuad : InteractableObject
{
    public override bool isInteractable { get { return true; }}
    public override List<string> interactableObjects { get { return new List<string>{ "Pizza" }; }}

    public DroppableObject objectPrefab;

    private DroppableObject currInstance;

    [SerializeField] private Constants.Vegetables vegetableType = Constants.Vegetables.Mushroom;

    // TODO Tint on hover

    public override InputController.InputState OnClick()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        currInstance = Instantiate(objectPrefab, mousePosition, Quaternion.identity);
        SoundController.scInstance.PlaySingle("itemGrab");

        return InputController.InputState.Grabbing;
    }

    public override InputController.InputState OnRelease(List<InteractableObject> interactedObjects)
    {
        bool onPizza = false;
        foreach (InteractableObject interactable in interactedObjects)
        {
            if (interactable.name == "Pizza")
            {
                PlayController.instance.AddVegetable(vegetableType);
                onPizza = true;
            }
        }
        currInstance.Drop(onPizza);
        currInstance = null;
        return InputController.InputState.Default;
    }
}
