using Sprache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bridge.Text;
internal class Parser
{
    private ModuleBuilder builder = Module.Create();

    public void AddSource(string source)
    {
        if (builder is null)
            throw new Exception();

        var decls = declarationsParser.Parse(source);

        foreach (var declaration in decls)
        {
            DataType mode = DataType.I32;
            switch (declaration)
            {
                case RoutineDeclaration r:
                    var routine = this.builder.AddRoutine(r.Name);
                    routine.ReturnType = r.ReturnType;
                    foreach (var param in r.Parameters)
                    {
                        routine.AddParameter(param);
                    }
                    var code = routine.GetCodeBuilder();

                    Dictionary<string, Local> locals = new();
                    foreach (var local in r.Locals)
                    {
                        locals.Add(local.Name, code.AddLocal(local.Type));
                    }
                    
                    Dictionary<string, Label> labels = new();
                    foreach (var statement in r.Statements)
                    {
                        switch (statement)
                        {
                            case InstructionStatement inst:
                                //code.Emit(inst.Instruction);
                                break;
                            case LabelStatement lab:
                                labels.Add(lab.Name, code.AddLabel());
                                break;
                            default:
                                throw new Exception();
                        }
                    }
                    break;
                case InlineDeclaration:
                default:
                    throw new Exception();
            }
        }
    }

    public Module CreateModule()
    {
        if (builder is null)
            throw new Exception();

        return builder.CreateModule();
    }

    static Parser()
    {
        parseIdentifierToken = Parse.Identifier(Parse.Letter.Or(Parse.Chars('_', '#', '$')), Parse.LetterOrDigit.Or(Parse.Chars('_', '#', '$'))).Token();
            
        parseDataType = parseIdentifierToken.Select(s => s switch
        {
            "*" => DataType.Pointer,
            "i64" => DataType.I64,
            "i32" => DataType.I32,
            "i16" => DataType.I16,
            "i8" => DataType.I8,
            "u64" => DataType.U64,
            "u32" => DataType.U32,
            "u16" => DataType.U16,
            "u8" => DataType.U8,
            "f64" => DataType.F64,
            "f32" => DataType.F32,
            _ => DataType.Void,
        }).Where(type => type != DataType.Void);

        parseDataTypeOptional = parseDataType.Or(Parse.Return(DataType.Void));


        parseParameterList =
            Parse.Contained(
                Parse.DelimitedBy(
                    parseDataType,
                    Parse.Char(',')
                    ).Optional(),
                Parse.Char('(').Token(),
                Parse.Char(')').Token())
            .Select(opt => opt.GetOrElse(Enumerable.Empty<DataType>())
            );

        declarationsParser = Declaration.GetParser().Once();//.Many().End();

        string[] keywords = new[]
        {
            "inline",
            "extern",
            "routine",
            "i64", "i32", "i16", "i8",
            "u64", "u32", "u16", "u8",
            "f64", "f32",
        };
    }

    private static readonly Parser<string> parseIdentifierToken;
    private static readonly Parser<IEnumerable<DataType>> parseParameterList;
    private static readonly Parser<DataType> parseDataType;
    private static readonly Parser<DataType> parseDataTypeOptional;
    private static readonly Parser<IEnumerable<Declaration>> declarationsParser;

    private abstract record Declaration(string Name)
    {
        public static Parser<Declaration> GetParser() => InlineDeclaration.GetParser().Or<Declaration>(RoutineDeclaration.GetParser()).Or(ExternDeclaration.GetParser());
    }
    
    private record InlineDeclaration(string Name, IEnumerable<Statement> Statements) : Declaration(Name)
    {
        public static new Parser<InlineDeclaration> GetParser() =>
            from keyword in Parse.String("inline").Token()
            from name in parseIdentifierToken
            from statements in Parse.Contained(Statement.GetParser().Many(), Parse.Char('{').Token(), Parse.Char('}').Token())
            select new InlineDeclaration(name, statements);
    }

    private record RoutineDeclaration(string Name, DataType ReturnType, IEnumerable<DataType> Parameters, IEnumerable<LocalDeclaration> Locals, IEnumerable<Statement> Statements)
        : Declaration(Name)
    {
        public static new Parser<RoutineDeclaration> GetParser() =>
            from keyword in Parse.String("define").Or(Parse.String("routine")).Token()
            from returnType in parseDataTypeOptional
            from name in parseIdentifierToken
            from parameters in parseParameterList.Optional()
            from openBracket in Parse.Char('{').Token()
            from locals in LocalDeclaration.GetParser().Many()
            from statements in Statement.GetParser().Repeat(4)
            from closeBracket in Parse.Char('}').Token()
            select new RoutineDeclaration(
                name,
                returnType,
                parameters.GetOrElse(Enumerable.Empty<DataType>()),
                locals,
                statements
                );
    }
    
    private record ExternDeclaration(string Name, DataType ReturnType, IEnumerable<DataType> Parameters) 
        : Declaration(Name)
    {
        public static new Parser<ExternDeclaration> GetParser() =>
            from keyword in Parse.String("extern").Token()
            from returnType in parseDataType.Optional()
            from name in parseIdentifierToken
            from parameters in parseParameterList.Optional()
            select new ExternDeclaration(
                name,
                returnType.GetOrElse(DataType.Void),
                parameters.GetOrElse(Enumerable.Empty<DataType>())
                );
    }

    private abstract record Statement()
    {
        public static new Parser<Statement> GetParser() => LabelStatement.GetParser().Or<Statement>(InstructionStatement.GetParser());
    }

    private record LocalDeclaration(DataType Type, string Name)
    {
        public static new Parser<LocalDeclaration> GetParser() =>
            from keyword in Parse.String("local")
            from dot in Parse.Char('.').Token()
            from type in parseDataType.Token()
            from name in parseIdentifierToken.Optional()
            select new LocalDeclaration(type, name.GetOrDefault());
    }
    
    private record LabelStatement(string Name) : Statement()
    {
        public static new Parser<LabelStatement> GetParser() =>
            from id in parseIdentifierToken
            from colon in Parse.Char(':').Token()
            select new LabelStatement(id);
    }
    
    private record InstructionStatement(OpCode OpCode, string Modifier, DataType Type, string Arg) : Statement
    {
        private static readonly Parser<OpCode> opCodeParser = parseIdentifierToken.Where(s => !s.Any(char.IsUpper) && Enum.TryParse<OpCode>(s, true, out _)).Select(s => Enum.Parse<OpCode>(s, true));

        private static readonly Parser<string> parseInt =
            from sign in Parse.Char('-').Or(Parse.Char('+')).Optional()
            from digits in Parse.Digit.Many()
            select string.Concat(sign.IsDefined ? digits.Prepend(sign.Get()) : digits);
        
        private static readonly Parser<string> parseDecimal =
            from sign in Parse.Char('-').Or(Parse.Char('+')).Optional()
            from wholeDigits in Parse.Digit.Many()
            from dot in Parse.Char('.')
            from fractionDigits in Parse.Digit.Many()
            select string.Concat((sign.IsDefined ? wholeDigits.Prepend(sign.Get()) : wholeDigits).Append('.').Concat(fractionDigits));

        private static readonly Parser<InstructionStatement> modifiedTypedOpParser =
            from opcode in opCodeParser.Token()
            from dot1 in Parse.Char('.').Token()
            from mod in parseIdentifierToken
            from dot2 in Parse.Char('.').Token()
            from type in parseDataType.Token()
            from arg in parseIdentifierToken.Optional()
            select new InstructionStatement(opcode, mod, type, arg.GetOrDefault());
        
        private static readonly Parser<InstructionStatement> typedOpParser =
            from opcode in opCodeParser.Token()
            from dot1 in Parse.Char('.').Token()
            from type in parseDataType.Token()
            from arg in parseIdentifierToken.Optional()
            select new InstructionStatement(opcode, null, type, arg.GetOrDefault());

        public static new Parser<InstructionStatement> GetParser()
        {
            return null;
        }

        
        //public static new Parser<InstructionStatement> GetParser() =>
        //        from opCode in opCodeParser
        //        from resultType in Parse.Return(GetInstructionType(opCode))
        //        from argTypes in Parse.Return(resultType.GetGenericArguments())
        //        from ctor in Parse.Return(resultType.GetConstructor(argTypes.Prepend(typeof(OpCode)).ToArray()))
        //        from args in argTypes.Aggregate(
        //            Parse.Return((object)opCode).Once(),
        //            (parser, type) => parser.Concat(GetParser(type).Once()))
        //           select new InstructionStatement((Instruction)ctor.Invoke(args.ToArray()));


        //private static Parser<object> GetParser(Type type)
        //{
        //    if (type == typeof(int))
        //    {
        //        return Parse.Digit.Many()
        //            .Select(string.Concat)
        //            .Select(int.Parse)
        //            .Select(x => (object)x);
        //    }
        //    if (type == typeof(byte))
        //    {
        //        return Parse.Digit.Many()
        //            .Select(string.Concat)
        //            .Select(byte.Parse)
        //            .Select(x => (object)x);
        //    }
        //    if (type == typeof(DataType))
        //    {
        //        return Parse.Char('.').Token().Then(c => parseDataType.Select(x => (object)x));
        //    }
        //    if (type == typeof(TypedValue))
        //    {
        //        return
        //            from dot in Parse.Char('.').Token()
        //            from dataType in parseDataType.Token()
        //            from value in GetValueParser(dataType)
        //            select (object)value;
        //    }
        //    if (type == typeof(ComparisonKind))
        //    {
        //        const ComparisonKind invalid = (ComparisonKind)255;

        //        return parseIdentifierToken.Select(s => s switch
        //        {
        //            "lt" => ComparisonKind.LessThan,
        //            "lte" => ComparisonKind.LessThanEqual,
        //            "gt" => ComparisonKind.GreaterThan,
        //            "gte" => ComparisonKind.GreaterThanEqual,
        //            "eq" => ComparisonKind.Equal,
        //            "neq" => ComparisonKind.NotEqual,
        //            "zero" => ComparisonKind.Zero,
        //            "nz" => ComparisonKind.NotZero,
        //            _ => invalid
        //        })
        //            .Where(k => k != invalid)
        //            .Select(x => (object)x);
        //    }
        //    if (type.IsEnum)
        //    {
        //        var names = type.GetEnumNames().Select(s=>s.ToLower()).ToArray();
        //        var values = type.GetEnumValues();
        //        return Parse.Char('.').Token().Then(d=> parseIdentifierToken.Where(s => names.Contains(s)).Select(s => values.GetValue(Array.IndexOf(names, s))));
        //    }

        //    return Parse.Return<object>(null);
        //}

        //private static Parser<TypedValue> GetValueParser(DataType dataType)
        //{
        //    return dataType switch
        //    {
        //        DataType.Pointer =>     GetIntegerParser(nint.Parse, false),
        //        DataType.I64 =>         GetIntegerParser(long.Parse, true),
        //        DataType.I32 =>         GetIntegerParser(int.Parse, true),
        //        DataType.I16 =>         GetIntegerParser(short.Parse, true),
        //        DataType.I8 =>          GetIntegerParser(sbyte.Parse, true),
        //        DataType.U64 =>         GetIntegerParser(ulong.Parse, false),
        //        DataType.U32 =>         GetIntegerParser(uint.Parse, false),
        //        DataType.U16 =>         GetIntegerParser(ushort.Parse, false),
        //        DataType.U8 =>          GetIntegerParser(byte.Parse, false),
        //        DataType.F64 =>         GetFloatParser(double.Parse),
        //        DataType.F32 =>         GetFloatParser(float.Parse),
        //        _ or DataType.Void => Parse.Return(TypedValue.CreateDefault(DataType.Void))
        //    };
        //}

        //private static Parser<TypedValue> GetIntegerParser<T>(Func<string, T> parse, bool signed) where T : unmanaged
        //{
        //    var number =
        //        from sign in Parse.Char('-').Or(Parse.Char('+')).Optional()
        //        from digits in Parse.Digit.Many()
        //        select digits.Prepend(sign.GetOrElse('+'));

        //    return number.Select(string.Concat).Select(parse).Select(TypedValue.Create);
        //}

        //private static Parser<TypedValue> GetFloatParser<T>(Func<string, T> parse) where T : unmanaged
        //{
        //    var number =
        //        from sign in Parse.Char('-').Or(Parse.Char('+')).Optional()
        //        from digits in Parse.Digit.Many()
        //        from dot in Parse.Char('.')
        //        from fraction in Parse.Digit.Many()
        //        select digits.Prepend(sign.GetOrElse('+')).Append(dot).Concat(fraction);

        //    return number
        //        .Select(string.Concat)
        //        .Select(parse)
        //        .Select(TypedValue.Create);
        //}

        //private static Type GetInstructionType(OpCode opCode)
        //{
        //    switch (opCode)
        //    {
        //        case OpCode.Return:
        //            return typeof(Instruction);
        //        case OpCode.Call:
        //            return typeof(Instruction<int>);
        //        case OpCode.Jump:
        //            return typeof(Instruction<Label>);
        //        case OpCode.If:
        //            return typeof(Instruction<ComparisonKind, DataType>);
        //        case OpCode.Print:
        //        case OpCode.Add:
        //            return typeof(Instruction<DataType>);
        //        case OpCode.Push:
        //            return typeof(Instruction<StackOpKind, TypedValue>);
        //        default:
        //            throw new ArgumentException(null, nameof(opCode));
        //    }
        //}
    }
}