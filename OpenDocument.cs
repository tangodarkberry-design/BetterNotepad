namespace BetterNotepad
{
    public enum DocumentKind
    {
        Text,
        Code,
        Image,
        Video
    }

    public class OpenDocument
    {
        public string FilePath { get; set; } = "";
        public string Title { get; set; } = "Untitled";
        public string Text { get; set; } = "";
        public string Language { get; set; } = "Text";
        public DocumentKind Kind { get; set; } = DocumentKind.Text;
    }
}