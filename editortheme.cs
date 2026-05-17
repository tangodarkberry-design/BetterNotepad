namespace BetterNotepad
{
    public class EditorTheme
    {
        public string Name { get; set; } = "Custom Theme";

        public string WindowBackground { get; set; } = "#1E1E1E";
        public string MenuBackground { get; set; } = "#D9D9D9";
        public string MenuText { get; set; } = "#000000";

        public string EditorBackground { get; set; } = "#232323";
        public string EditorText { get; set; } = "#DCDCDC";

        // Put image files in the Themes folder.
        // Example: "background.png"
        // Leave blank for no image background.
        public string EditorBackgroundImage { get; set; } = "";

        // Stretch options: None, Fill, Uniform, UniformToFill
        public string EditorBackgroundImageStretch { get; set; } = "UniformToFill";

        public double EditorBackgroundImageOpacity { get; set; } = 0.35;

        public string StatusBackground { get; set; } = "#E5E5E5";
        public string StatusText { get; set; } = "#000000";

        public string LineNumberBackground { get; set; } = "#F0F0F0";
        public string LineNumberText { get; set; } = "#000000";

        public string OutputBackground { get; set; } = "#DDDDDD";
        public string OutputHeaderBackground { get; set; } = "#CCCCCC";
        public string OutputText { get; set; } = "#000000";
    }
}