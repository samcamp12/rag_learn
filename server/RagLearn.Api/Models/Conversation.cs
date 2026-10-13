namespace RagLearn.Api.Models;

public class Conversation
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public List<ConversationMessage> Messages { get; set; } = [];
}

public class ConversationMessage
{
    public int Id { get; set; }
    public Guid ConversationId { get; set; }
    public Conversation? Conversation { get; set; }
    public required string Role { get; set; }
    public required string Content { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
