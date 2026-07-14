namespace Jumia.DTOs
{
    public class ChatRequestDto
    {
        public string Message { get; set; } = string.Empty;
    }

    public class ChatReplyDto
    {
        public string Reply { get; set; } = string.Empty;
        public string[] Suggestions { get; set; } = Array.Empty<string>();
    }
}
