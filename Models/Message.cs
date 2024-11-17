namespace WebApi.Models
{
    public interface IMessage
    {
        Guid Guid { get; set; }
        string UserName { get; set; }
        string MessageValue { get; set; }
        DateTime Time { get; set; }

    }

    public class Message : IMessage
    {
        public Guid Guid { get; set; }
        public string UserName { get; set; }
        public string MessageValue { get; set; }
        public DateTime Time { get; set; }
    }
}