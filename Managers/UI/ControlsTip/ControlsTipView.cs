using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Managers.UI.ControlsTip
{
    public class ControlsTipView : MonoBehaviour
    {
        [SerializeField] 
        private ControlsTipItem _controlsTipItem;
        
        [SerializeField]
        private Transform _verticalContainer;

        private List<ControlsTipItem> _activeTips = new List<ControlsTipItem>();
        
        private void OnEnable()
        {
            ControlsTipManager.OnShowTipRequested += HandleShowTip;
            ControlsTipManager.OnHideTipRequested += HandleHideTip;
        }

        private void OnDisable()
        {
            ControlsTipManager.OnShowTipRequested -= HandleShowTip;
            ControlsTipManager.OnHideTipRequested -= HandleHideTip;
        }

        private void HandleShowTip(ControlsTipPayload payload)
        {
            //TODO: Implement pooling for better performance if tips are shown frequently
            
            foreach (var tip in _activeTips)
            {
                if (tip.Payload.InfoText == payload.InfoText) return; 
            }
            GameObject tipObject = Instantiate(_controlsTipItem.gameObject, _verticalContainer);
            if (tipObject.TryGetComponent(out ControlsTipItem controlsTipItem))
            {
                controlsTipItem.Setup(payload);
                _activeTips.Add(controlsTipItem);
                return;
            }
        }

        private void HandleHideTip(ControlsTipPayload payloadToHide)
        {
            for (int i = _activeTips.Count - 1; i >= 0; i--)
            {
                if (_activeTips[i].Payload.Equals(payloadToHide)) 
                {
                    Destroy(_activeTips[i].gameObject);
                    _activeTips.RemoveAt(i);
                    break; 
                }
            }
        }
    }
    
}