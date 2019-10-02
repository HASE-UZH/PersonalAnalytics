namespace WindowRecommender.Data
{
    internal sealed class EventName
    {
        private readonly string _name;

        public static readonly EventName Initial = new EventName("Initial");
        public static readonly EventName Open = new EventName("Open");
        public static readonly EventName Focus = new EventName("Focus");
        public static readonly EventName Close = new EventName("Close");
        public static readonly EventName Minimize = new EventName("Minimize");

        private EventName(string name)
        {
            _name = name;
        }

        public override string ToString()
        {
            return _name;
        }
    }
}