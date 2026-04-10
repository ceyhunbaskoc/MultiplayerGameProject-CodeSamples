using Managers.UI.ControlsTip;
using UnityEngine;
using UnityEngine.InputSystem;

public interface IInteractable
{
    ControlsTipPayload InteractTip { get; }
    InputActionReference CancelActionReference { get; }
    public void Interact();
    
    public bool IsInteractable { get; }
}
