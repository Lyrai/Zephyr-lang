using System.Diagnostics.CodeAnalysis;
using PrimitiveTypeCode = Microsoft.Cci.PrimitiveTypeCode;

namespace Zephyr.SemanticAnalysis.Symbols
{
    public class TypeSymbol : Symbol
    {
        public bool IsArray => Name.StartsWith("[");

        public TypeSymbol(string name) : base(name)
        { }

        public override string ToString()
        {
            return Name;
        }

        public static bool operator ==(TypeSymbol t1, TypeSymbol t2)
        {
            return t1?.Name == t2?.Name;
        }

        public static bool operator !=(TypeSymbol t1, TypeSymbol t2)
        {
            return !(t1 == t2);
        }

        [return: NotNullIfNotNull("symbol")]
        public static TypeSymbol? FromObject(object? symbol)
        {
            return symbol switch
            {
                VarSymbol varSymbol => varSymbol.Type,
                FuncSymbol funcSymbol => funcSymbol.Type,
                null => null,
                _ => (TypeSymbol) symbol
            };
        }

        public string GetNetName()
        {
            return GetNetType()?.Name ?? Name;
        }

        internal PrimitiveTypeCode GetTypeCode()
        {
            switch (Name)
            {
                case "int":
                case "string":
                case "void":
                    Enum.TryParse(GetNetName(), out PrimitiveTypeCode res);
                    return res;
                case "long":
                    return PrimitiveTypeCode.Int64;
                case "double":
                    return PrimitiveTypeCode.Float64;
                case "bool":
                    return PrimitiveTypeCode.Boolean;
                default:
                    return PrimitiveTypeCode.NotPrimitive;
            }
        }

        public string GetNetFullName()
        {
            return GetNetType()?.FullName ?? Name;
        }

        public bool IsValueType()
        {
            return Name is "int" or "double" or "bool" or "long";
        }

        public virtual Type? GetNetType()
        {
            return Name switch
            {
                "int" => typeof(int),
                "double" => typeof(double),
                "bool" => typeof(bool),
                "string" => typeof(string),
                "void" => typeof(void),
                "long" => typeof(long),
                "object" => typeof(object),
                _ => null
            };
        }
    }
}
