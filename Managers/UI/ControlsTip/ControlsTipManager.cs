using System;
using UnityEngine;

namespace Managers.UI.ControlsTip
{
    public static class ControlsTipManager
    {
        public static Action<ControlsTipPayload> OnShowTipRequested;
        public static Action<ControlsTipPayload> OnHideTipRequested; 

        public static void ShowTip(ControlsTipPayload payload)
        {
            OnShowTipRequested?.Invoke(payload);
        }

        public static void HideTip(ControlsTipPayload payload)
        {
            OnHideTipRequested?.Invoke(payload);
        }
    }
}