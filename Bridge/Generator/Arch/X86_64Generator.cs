namespace Bridge;

internal class X86_64Generator : ArchGenerator
{ 
    public override void Compile(Stream stream, IEnumerable<Instruction> instructions)
    {
        for (int i = 0; i < instructions.Count(); i++)
        {
            Instruction inst = instructions.ElementAt(i);
            GetHandler(inst.GetType())(stream, inst);
        }
    }

    public override void CompilePush(Stream stream, Instruction inst)
    {
        Push push = inst as Push;

        // movabs imm64
        // encoding: rexprefix + (opcode + register) + value
        WriteREXPrefix(stream, true); // rexprefix
        stream.Write(0xB8 + 0b000);   // 0xB8 + rax
        stream.Write(push.Value);     // value

        // push r64
        // encoding: (opcode + register)
        stream.Write(0x50 + 0b000); // 0x50 + rax
    }
    
    public override void CompilePop(Stream stream, Instruction inst)
    {
        // pop r64
        // encoding: (opcode + register)
        stream.Write(0x58 + 0b000); // 0x58 + rax
    }

    public override void CompileCall(Stream stream, Instruction inst)
    {
        Call call = inst as Call;
        
        for (int i = 0; i < call.Parameters.Length; i++)
        {
            // push parameter
            CompilePush(stream, new Push(call.Parameters[i]));
        }

        // movabs imm64
        // encoding: rexprefix + (opcode + register) + value
        WriteREXPrefix(stream, true); // rexprefix
        stream.Write(0xB8 + 0b000);   // 0xB8 + rax
        stream.Write(call.Address);   // value

        // call r64
        // encoding: modrm(mode, opcode, register)
        WriteModRM(stream, true, 0b010, 0b000); // modrm(indirect, 2, rax)
    }

    private void WriteREXPrefix(Stream stream, bool is64Bit)
    {
        byte rexop = 0b0100 << 4;
        byte is64 = (byte)((is64Bit ? 1 : 0) << 3);
        byte extensions = 0;

        stream.Write(rexop | is64 | extensions);
    }

    private void WriteModRM(Stream stream, bool isIndirect, byte register, byte rm)
    {
        byte mode = (byte)((isIndirect ? 0b00 : 0b11) << 6);
        byte reg = (byte)(register << 3);

        stream.Write(mode | reg | rm);
    }
}