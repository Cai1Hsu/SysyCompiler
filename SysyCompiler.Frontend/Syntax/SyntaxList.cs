using System.Collections;
using System.Collections.Immutable;

namespace SysyCompiler.Frontend.Syntax;

public abstract class SyntaxList : SyntaxNode
{
    public override SyntaxKind Kind => SyntaxKind.SyntaxList;

    public abstract IEnumerable<SyntaxNode> GetItems();
}

public class SyntaxList<T> : SyntaxList, IReadOnlyList<T>, IReadOnlyCollection<T>
    where T : SyntaxNode
{
    public override SyntaxKind Kind => SyntaxKind.SyntaxList;

    private ImmutableArray<T> Items { get; }

    public override IEnumerable<SyntaxNode> GetItems() => Items.Cast<SyntaxNode>();

    public int Count => Items.Length;

    public T this[int index] => Items[index];

    public SyntaxList(ImmutableArray<T> items)
    {
        Items = items;
    }

    public SyntaxList() : this(ImmutableArray<T>.Empty) { }

    public override SyntaxMember[] GetMembers() => new SyntaxMember[] {
        new(nameof(Items), Items)
    };

    public IEnumerator<T> GetEnumerator()
        => ((IEnumerable<T>)Items).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable)Items).GetEnumerator();
}