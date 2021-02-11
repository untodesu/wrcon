using System.Windows.Media;

namespace WRcon.Core
{
    internal class ClientStatus
    {
        public static readonly ClientStatus Disconnected = new ClientStatus("Disconnected", Brushes.OrangeRed);
        public static readonly ClientStatus Connecting = new ClientStatus("Connecting", Brushes.BlueViolet);
        public static readonly ClientStatus Connected = new ClientStatus("Connected", Brushes.Green);

        public readonly string Text;
        public readonly Brush Color;

        public ClientStatus(string text, Brush color)
        {
            Text = text;
            Color = color;
        }
    }
}
