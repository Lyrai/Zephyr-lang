namespace Zephyr.SemanticAnalysis.Symbols;

public class ArrayTypeSymbol: TypeSymbol
{
    public TypeSymbol ElementType { get; private set; }

    public ArrayTypeSymbol(string name, TypeSymbol elementType) : base(name)
    {
        ElementType = elementType;
    }
    
    public string GetElementTypeFullName()
    {
        return ElementType.GetNetFullName() ?? ElementType.Name;
    }
    
    public string GetElementTypeName()
    {
        return ElementType.GetNetName() ?? ElementType.Name;
    }

    public override Type? GetNetType()
    {
        return ElementType.GetNetType().MakeArrayType();
    }
}
