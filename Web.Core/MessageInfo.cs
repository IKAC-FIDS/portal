namespace TES.Web.Core
{
    public enum MessageType
    {
        Success = 0,
        Info = 1,
        Warning = 2,
        Danger = 3
    }

    public class MessageInfo
    {
        public MessageInfo(string message, MessageType messageType, string title = null, string Data = "")
        {
            Title = title;
            Message = message;
            MessageType = messageType;
            Data = Data;
        }

        public string Title { get; }
        public string Message { get; }
        public MessageType MessageType { get; }
    }
}