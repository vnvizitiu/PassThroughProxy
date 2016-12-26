namespace Proxy.Configurations
{
    public class Proxy
    {
        public Proxy(bool enabled, string address, int port, Authentication authentication)
        {
            Enabled = enabled;
            Address = address;
            Port = port;
            Authentication = authentication;
        }

        public bool Enabled { get; private set; }
        public string Address { get; private set; }
        public int Port { get; private set; }
        public Authentication Authentication { get; private set; }
    }
}