using TMPro;
using UnityEngine;

namespace Managers.UI.ControlsTip
{
    public class ControlsTipItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _infoText;
        [SerializeField] private TextMeshProUGUI _buttonText;

        public ControlsTipPayload Payload { get; private set; }
        public void Setup(ControlsTipPayload payload)
        {
            this.Payload = payload;
            
            _infoText.text = payload.InfoText;
            _infoText.color = payload.TextColor;
            _buttonText.text = payload.ButtonText;
        }
    }
}