using System;
using DG.Tweening;
using Managers.UI.ControlsTip;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Interactables
{
    public class CallNextGuestButton : MonoBehaviour, IInteractable
    {
        [SerializeField] 
        private Transform _buttonTop;
        public InputActionReference CancelActionReference => null;

        private readonly ControlsTipPayload _interactTip = new ControlsTipPayload("Call Next Guest", "E");
        public ControlsTipPayload InteractTip => _interactTip;

        public static Action OnNextGuestRequested;

        private float _originalLocalY;
        private bool _isInteractable = true;
        public bool IsInteractable => _isInteractable;

        private void Start()
        {
            _originalLocalY=_buttonTop.localPosition.z;
        }

        public void Interact()
        {
            if (!_isInteractable) 
            {
                // TODO: "Hata" sesi çal
                return; 
            }

            _buttonTop.DOKill();
            _buttonTop.DOLocalMoveZ(_originalLocalY - 0.1f, 0.1f).OnComplete(() =>
            {
                _buttonTop.DOLocalMoveZ(_originalLocalY, 0.1f);
                OnNextGuestRequested?.Invoke(); // Sadece butona basıldığını haber ver
            });
        }

        public void SetInteractable(bool state)
        {
            _isInteractable = state;
        }
    }
}