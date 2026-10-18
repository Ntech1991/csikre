namespace Ntech.AI.Common.Models
{
    /// <summary>
    /// Enumeration for message roles in chat
    /// </summary>
    public enum MessageRole
    {
        User = 0,
        Assistant = 1,
        System = 2
    }

    /// <summary>
    /// Represents a message in a chat conversation
    /// </summary>
    public class ChatMessage
    {
        public string Content { get; set; } = string.Empty;
        public MessageRole Role { get; set; } = MessageRole.User;
    }
}
