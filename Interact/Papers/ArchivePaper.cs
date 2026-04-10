using System;
using DG.Tweening;
using Managers;
using Managers.Archive;
using Managers.Inspection;
using Managers.UI.ControlsTip;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Interactables.Archive
{
    public class ArchivePaper : MonoBehaviour,IInteractable
    {
        [SerializeField] private RuleCategory _category;
        [SerializeField] private Transform _deskSlot;
        [SerializeField] private Transform _archivePaperCover;
        [SerializeField] private float _animationDuration = 0.5f;
        private Transform _drawerParent;

        [SerializeField] private GameObject _paperObject;
        [SerializeField] private TextMeshProUGUI _paperContent;
        
        [SerializeField] private InputActionReference _cancelInteractAction;
        
        private Vector3 _drawerPosition;
        private Vector3 _drawerRotation;
        public bool IsOnDesk => _isOnDesk;
        public InputActionReference CancelActionReference => _cancelInteractAction;

        private ControlsTipPayload _interactTip = new("Take Archive", "E");
        public ControlsTipPayload InteractTip => _interactTip;
        
        private bool _isInteractable = true;
        
        public bool IsInteractable => _isInteractable;
        
        private bool _isOnDesk = false;
        private bool _isPaperOpen = false;
        
        private Vector3 _originalLocalPosition;
        private Vector3 _originalLocalRotation;

        private void Awake()
        {
            _drawerParent = transform.parent;
            _drawerPosition = transform.localPosition;
            _drawerRotation = transform.localEulerAngles;
            _cancelInteractAction.action.Enable();
        }

        public void Interact()
        {
            if(!_isInteractable) return;
            
            if (_isOnDesk)
                _openPaper();
            else
                PlaceOnDesk(); 
        }

        private void Update()
        {
            if(_isOnDesk && _isPaperOpen && CancelActionReference.action.WasPressedThisFrame())
                _closePaper();
        }

        public void PlaceOnDesk()
        {
            transform.SetParent(null);
            ArchiveDeskManager.Instance.OpenPaper(this);
            _isOnDesk = true;
            _isInteractable = false;
            
            transform.DOMove(_deskSlot.position, _animationDuration).SetEase(Ease.InOutSine);
            transform.DORotate(_deskSlot.rotation.eulerAngles, _animationDuration).SetEase(Ease.InOutSine)
                .OnComplete(() =>
                {
                    ControlsTipManager.HideTip(_interactTip);
                    _interactTip = new ControlsTipPayload("Search Archive", "E");
                    _openPaper();
                });
        }

        public void ReturnToDrawer()
        {
            transform.SetParent(_drawerParent);
            _isOnDesk = false;
            _isInteractable = false;
            _closePaper();
            ArchiveDeskManager.Instance.ClearDesk();
            
            transform.DOLocalMove(_drawerPosition, _animationDuration).SetEase(Ease.InOutSine);
            transform.DOLocalRotate(_drawerRotation, _animationDuration).SetEase(Ease.InOutSine)
                .OnComplete(() =>
                {
                    ControlsTipManager.HideTip(_interactTip);
                    _interactTip = new ControlsTipPayload("Take Archive", "E");
                });
        }
        
        private void _openPaper()
        {
            _switchDocumentMode(true);
            ControlsTipManager.ShowTip(new ControlsTipPayload("Close Paper", "ESC"));
            transform.DOKill();
            _paperObject.SetActive(true);
            _paperContent.text = _category switch
            {
                RuleCategory.All         => RulesArchive.GetAllRulesGroupedByCategory(),
                RuleCategory.ActiveRules => RulesArchive.GetActiveRulesGroupedByCategory(),
                _                        => RulesArchive.GetRuleTextByCategory(_category)
            };
            _archivePaperCover.DOLocalRotate(new Vector3(0, 0, 180), _animationDuration).SetEase(Ease.InOutSine);
        }
        
        private void _closePaper()
        {
            _switchDocumentMode(false);
            transform.DOKill();
            ControlsTipManager.HideTip(new ControlsTipPayload("Close Paper", "ESC"));
            _archivePaperCover.DOLocalRotate(new Vector3(0, 0, 0), _animationDuration).SetEase(Ease.InOutSine).OnComplete(() =>
            {
                _isInteractable = true;
                _paperObject.SetActive(false);
                _paperContent.text = "";
            });
        }

        private void _switchDocumentMode(bool enable)
        {
            _isPaperOpen = enable;
            
            GameState.ChangeState(enable ? AppState.DocumentMode : AppState.Gameplay);
        }

        public void SetInteractable(bool state)
        {
            _isInteractable = state;
        }

        #region ResetLogic
        public void ReturnToDrawerImmediately()
        {
            transform.SetParent(_drawerParent);
            _isOnDesk = false;
            _isInteractable = false;
            _closePaperImmediately();
            ArchiveDeskManager.Instance.ClearDesk();
            
            transform.position=_drawerPosition;
            transform.eulerAngles = _drawerRotation;
            ControlsTipManager.HideTip(_interactTip);
            _interactTip = new ControlsTipPayload("Take Archive", "E");
        }
        private void _closePaperImmediately()
        {
            _switchDocumentMode(false);
            ControlsTipManager.HideTip(new ControlsTipPayload("Close Paper", "ESC"));
            _archivePaperCover.transform.localRotation = Quaternion.Euler(0, 0, 0);
            _isInteractable = true;
            _paperObject.SetActive(false);
            _paperContent.text = "";
        }
        #endregion
    }
}