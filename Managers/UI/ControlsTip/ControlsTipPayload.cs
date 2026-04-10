namespace Managers.UI.ControlsTip
{
    public struct ControlsTipPayload
    {
        public string InfoText;
        public string ButtonText;
        public UnityEngine.Color TextColor;

        public ControlsTipPayload(string info, string button, UnityEngine.Color color = default)
        {
            InfoText = info;
            ButtonText = button;
            TextColor = color == default ? UnityEngine.Color.white : color;
        }
        
        public static bool operator ==(ControlsTipPayload a, ControlsTipPayload b)
            => a.InfoText == b.InfoText && a.ButtonText == b.ButtonText;

        public static bool operator !=(ControlsTipPayload a, ControlsTipPayload b)
            => !(a == b);

        public override bool Equals(object obj)
            => obj is ControlsTipPayload other && this == other;

        public override int GetHashCode()
            => System.HashCode.Combine(InfoText, ButtonText);

    }
}