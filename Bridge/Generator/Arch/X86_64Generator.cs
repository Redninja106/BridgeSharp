namespace Bridge;

internal class X86_64Generator : ArchGenerator
{ 
    public override IEnumerable<byte> Compile(IEnumerable<Instruction> instructions)
    {
        List<byte> bytes = new List<byte>();

        for (int i = 0; i < instructions.Count(); i++)
        {
            Instruction inst = instructions.ElementAt(i);
            byte[] instBytes = GetHandler(inst.GetType())(inst).ToArray();

            bytes.AddRange(instBytes);
        }
            
        return bytes;
    }

    public override IEnumerable<byte> CompilePush(Instruction inst)
    {
        Push push = inst as Push;
        
        return null;
    }

    public override IEnumerable<byte> CompilePop(Instruction inst)
    {
        Pop pop = inst as Pop;

        return null;
    }

    public override IEnumerable<byte> CompileCall(Instruction inst)
    {
        Call call = inst as Call;

        return null;
    }
}