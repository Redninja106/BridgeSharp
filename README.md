# BridgeSharp
 A library for compiling assembly-like instructions to different platforms

## Examples
The following example creates a popup with the title `this is a title` and  
the text `omg some content` for the content.
```csharp
Bridge b = new Bridge();

b.Store("content", "omg some content");
long content = b.GetDataAddress("content");

b.Store("title", "this is a title");
long title = b.GetDataAddress("title");

b.Import("MessageBoxA", "user32.dll");
long msgbox = b.GetImportAddress("MessageBoxA");

b.Import("ExitProcess", "kernel32.dll");
long extprc = b.GetImportAddress("ExitProcess");

b.Call(msgbox, 0, content, title, 0);
b.Call(extprc, 0);
```