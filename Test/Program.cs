using Bridge.Text;

var scanned = Scanner.Scan(new StringReader(@"define hello {
    pushconst 0
    print
    add
    sub
    mul
    div
    pop x
    pop .x
    pop @x
    push x
    push .x
    push @x
}"));

var parser = new Parser();

parser.AddSource(new TokenReader(scanned));

parser.GetResult().Dump(Console.Out);
