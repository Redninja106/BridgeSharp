using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bridge.Text;

internal class DocumentBuildContext
{
    public Document Document { get; }
    public Dictionary<string, DefinitionBuilder> Definitions { get; } = new();
    public List<string> Errors { get; } = new();

    public DocumentBuildContext(Document document)
    {
        Document = document;
    }

    public void AddError(string error)
    {
        Errors.Add(error);
    }
}
