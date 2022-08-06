namespace Bridge;

/// <summary>
/// Provides a mechnism for builders to close builders that are dependent on them.
/// </summary>
internal interface IBuilder
{
    void Close();
}