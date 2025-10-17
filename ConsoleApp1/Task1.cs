namespace ConsoleApp1;

using System;
using System.Collections.Generic;
using System.Linq;

public abstract record Expr;
public sealed record Id(string Name) : Expr;
public sealed record Num(string Text) : Expr;
public sealed record TupleLiteral(IReadOnlyList<Expr> Elements) : Expr;
public sealed record NewExpr(string TypeName, IReadOnlyList<Expr> Args) : Expr;

public abstract record Stmt;
public sealed record VarDecl(string Name, Expr Init) : Stmt;
public sealed record Return(Expr Value) : Stmt;
public sealed record Block(IReadOnlyList<Stmt> Statements) : Stmt;

public static class TupleLiteralRewriter
{
    /// <summary>
    /// Replace tuple literals with 'new &lt;typeName&gt;(...)' everywhere in the tree.
    /// </summary>
    public static Block Rewrite(Block root, string typeName)
    {
        ArgumentNullException.ThrowIfNull(root);
        ArgumentNullException.ThrowIfNull(typeName);

        Stmt RewriteStmt(Stmt s) => s switch
        {
            VarDecl v => v with { Init = RewriteExpr(v.Init) },
            Return r  => r with { Value = RewriteExpr(r.Value) },
            Block b   => b with { Statements = b.Statements.Select(RewriteStmt).ToArray() },
            _         => s
        };

        Expr RewriteExpr(Expr e) => e switch
        {
            TupleLiteral t => new NewExpr(typeName, t.Elements.Select(RewriteExpr).ToArray()),
            NewExpr n      => n with { Args = n.Args.Select(RewriteExpr).ToArray() },
            _              => e
        };

        return (Block)RewriteStmt(root);
    }
}
