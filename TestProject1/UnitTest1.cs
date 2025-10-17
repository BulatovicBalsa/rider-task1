using ConsoleApp1;

namespace TestProject1;

internal static class AstAssert
{
  public static void EqualBlocks(Block expected, Block actual)
  {
    Assert.NotNull(actual);
    Assert.Equal(expected.Statements.Count, actual.Statements.Count);
    for (var i = 0; i < expected.Statements.Count; i++)
      EqualStmt(expected.Statements[i], actual.Statements[i]);
  }

  private static void EqualStmt(Stmt expected, Stmt actual)
  {
    Assert.Equal(expected.GetType(), actual.GetType());

    switch (expected, actual)
    {
      case (VarDecl ev, VarDecl av):
        Assert.Equal(ev.Name, av.Name);
        EqualExpr(ev.Init, av.Init);
        break;

      case (Return er, Return ar):
        EqualExpr(er.Value, ar.Value);
        break;

      case (Block eb, Block ab):
        EqualBlocks(eb, ab);
        break;

      default:
        throw new InvalidOperationException($"Unhandled stmt type {expected.GetType().Name}");
    }
  }

  private static void EqualExpr(Expr expected, Expr actual)
  {
    Assert.Equal(expected.GetType(), actual.GetType());

    switch (expected, actual)
    {
      case (Id eid, Id aid):
        Assert.Equal(eid.Name, aid.Name);
        break;

      case (Num en, Num an):
        Assert.Equal(en.Text, an.Text);
        break;

      case (TupleLiteral et, TupleLiteral at):
        Assert.Equal(et.Elements.Count, at.Elements.Count);
        for (var i = 0; i < et.Elements.Count; i++)
          EqualExpr(et.Elements[i], at.Elements[i]);
        break;

      case (NewExpr enew, NewExpr anew):
        Assert.Equal(enew.TypeName, anew.TypeName);
        Assert.Equal(enew.Args.Count, anew.Args.Count);
        for (var i = 0; i < enew.Args.Count; i++)
          EqualExpr(enew.Args[i], anew.Args[i]);
        break;

      default:
        throw new InvalidOperationException($"Unhandled expr type {expected.GetType().Name}");
    }
  }
}

public class TupleLiteralRewriterTests
{
  [Fact]
  public void Rewrites_SimpleTuple_InVarDecl()
  {
    var input = new Block([
      new VarDecl("p", new TupleLiteral([new Num("1"), new Num("2")]))
    ]);
    
    var result = TupleLiteralRewriter.Rewrite(input, "Pair");
    
    var expected = new Block([
      new VarDecl("p",
        new NewExpr("Pair", [new Num("1"), new Num("2")]))
    ]);
    
    AstAssert.EqualBlocks(expected, result);
  }

  [Fact]
  public void Rewrites_Tuple_Inside_Return()
  {
    var input = new Block([
      new Return(new TupleLiteral([new Id("x"), new Id("y")]))
    ]);

    var result = TupleLiteralRewriter.Rewrite(input, "Point");

    var expected = new Block([
      new Return(new NewExpr("Point", [new Id("x"), new Id("y")]))
    ]);

    AstAssert.EqualBlocks(expected, result);
  }

  [Fact]
  public void Rewrites_Tuple_Inside_NewExpr_Args()
  {
    var input = new Block([
      new VarDecl("list",
        new NewExpr("PointList", [
          new TupleLiteral([new Num("3"), new Num("4")])
        ]))
    ]);

    var result = TupleLiteralRewriter.Rewrite(input, "Point");

    var expected = new Block([
      new VarDecl("list",
        new NewExpr("PointList", [
          new NewExpr("Point", [new Num("3"), new Num("4")])
        ]))
    ]);

    AstAssert.EqualBlocks(expected, result);
  }

  [Fact]
  public void Rewrites_Nested_Tuples_Recursively()
  {
    var input = new Block([
      new VarDecl("nested",
        new TupleLiteral([
          new Num("1"),
          new TupleLiteral([new Num("2"), new Num("3")])
        ]))
    ]);

    var result = TupleLiteralRewriter.Rewrite(input, "Pair");

    var expected = new Block([
      new VarDecl("nested",
        new NewExpr("Pair", [
          new Num("1"),
          new NewExpr("Pair", [new Num("2"), new Num("3")])
        ]))
    ]);

    AstAssert.EqualBlocks(expected, result);
  }

  [Fact]
  public void Leaves_NonTuple_Nodes_Untouched_Besides_Children()
  {
    var input = new Block([
      new VarDecl("a", new Num("42")),
      new Return(new NewExpr("Wrapper", [new Id("a")]))
    ]);

    var result = TupleLiteralRewriter.Rewrite(input, "IgnoredTypeName");

    AstAssert.EqualBlocks(input, result);
  }

  [Fact]
  public void Rewrites_Tuple_Inside_Nested_Block()
  {
    var inner = new Block([
      new Return(new TupleLiteral([new Num("7"), new Num("8")]))
    ]);

    var input = new Block([
      new VarDecl("x", new Num("0")),
      inner
    ]);

    var result = TupleLiteralRewriter.Rewrite(input, "Pair");

    var expectedInner = new Block([
      new Return(new NewExpr("Pair", [new Num("7"), new Num("8")]))
    ]);

    var expected = new Block([
      new VarDecl("x", new Num("0")),
      expectedInner
    ]);

    AstAssert.EqualBlocks(expected, result);
  }
}