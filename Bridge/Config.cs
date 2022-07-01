namespace Bridge;

internal class Config
{
    public static OSKind OS { get; set; }
    public static ArchKind Arch { get; set; }

    public enum OSKind
    {
        //win32,
        win64,
        //lin32,
        //lin64,
        //mac32,
        //mac64
    }

    public enum ArchKind
    {
        //x86,
        x86_64,
        //arm,
        //arm_64
    }
}
