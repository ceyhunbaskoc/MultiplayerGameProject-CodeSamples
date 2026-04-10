using System;
using Managers;
using Managers.Network;
using Managers.UI.ControlsTip;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Interactables
{
    public class NoteMachine : MonoBehaviour, IInteractable
    {
        [Header("UI References")]
        [SerializeField] private GameObject _machineInputPanel;
        [SerializeField] private TMP_InputField _noteInputField;
        
        [SerializeField] private Transform _inboxContainer;
        [SerializeField] private GameObject _inboxEntryPrefab;
        [SerializeField] private AudioSource _sendSound;
        [SerializeField] private AudioSource _receiveSound;

        [SerializeField] private InputActionReference _cancelInteractAction;

        public InputActionReference CancelActionReference => _cancelInteractAction;

        private readonly ControlsTipPayload _interactTip = new("Switch Typing Mode", "E");
        public ControlsTipPayload InteractTip => _interactTip;

        private readonly ControlsTipPayload _quitTip = new("Quit", "ESC");

        private bool _isTypingMode = false;
        
        private bool _isInteractable = true;
        public bool IsInteractable => _isInteractable;

        private void Awake()
        {
            _cancelInteractAction.action.Enable();
            Debug.Log($"_machineInputPanel null mu: {_machineInputPanel == null}");
            _machineInputPanel.SetActive(false);
            _noteInputField.text = "";
            _noteInputField.onSubmit.AddListener(_onSubmit);
        }

        private void OnEnable()
        {
            PneumaticTubeNetwork.OnMessageReceived += _onMessageReceived;
        }

        private void OnDisable()
        {
            PneumaticTubeNetwork.OnMessageReceived -= _onMessageReceived;
            _noteInputField.onSubmit.RemoveListener(_onSubmit);
        }

        private void Update()
        {
            if (_isTypingMode && _cancelInteractAction.action.WasPressedThisFrame())
                _closeMachine();
        }

        public void Interact()
        {
            _setTypingMode(true);
        }

        private void _setTypingMode(bool enable)
        {
            _isTypingMode = enable;

            if (enable)
            {
                _isInteractable = false;
                GameState.ChangeState(AppState.UIMenu);
                _machineInputPanel.SetActive(true);
                _noteInputField.Select();
                _noteInputField.ActivateInputField();
                ControlsTipManager.ShowTip(_quitTip);
            }
        }

        private void _onSubmit(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                _closeMachine();
                return;
            }

            PneumaticTubeNetwork.Instance.SendMessage(message, PlayerRoleManager.LocalRole);

            _addToInbox($"Sen: {message}");

            //_sendSound?.Play();

            _noteInputField.text = string.Empty;
            _closeMachine();
        }

        private void _onMessageReceived(string message, PlayerRole targetRole)
        {
            string prefix = targetRole == PlayerRole.Reception ? "Archive" : "Reception";
            _addToInbox($"{prefix}: {message}");
            //_receiveSound?.Play();
        }

        private void _addToInbox(string text)
        {
            if (_inboxContainer == null || _inboxEntryPrefab == null) return;

            GameObject entry = Instantiate(_inboxEntryPrefab, _inboxContainer);
            TextMeshProUGUI tmp = entry.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null) tmp.text = text;
        }

        private void _closeMachine()
        {
            _isInteractable = true;
            _isTypingMode = false;
            _machineInputPanel.SetActive(false);
            GameState.ChangeState(AppState.Gameplay);
            ControlsTipManager.HideTip(_interactTip);
            ControlsTipManager.HideTip(_quitTip);
        }

        public void ResetFields()
        {
            _isInteractable = true;
            _isTypingMode = false;
            _noteInputField.text = string.Empty;
            foreach (Transform child in _inboxContainer)
            {
                Destroy(child.gameObject);
            }
            _machineInputPanel.SetActive(false);
            ControlsTipManager.HideTip(_interactTip);
            ControlsTipManager.HideTip(_quitTip);
        }
    }
}