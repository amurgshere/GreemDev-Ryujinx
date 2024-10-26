namespace Ryujinx.UI.App.Common
{
    public class XCITrimmerApplicationData
    {
        public string Name { get; set;}
        public string Path { get; set; }
        public bool Trimmable { get; set; }
        public bool Untrimmable { get; set; }
        public ulong PotentialSavingsB { get; set; }
        public ulong CurrentSavingsB { get; set; }
    }
}
