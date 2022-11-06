namespace Zephyr.Compiling.Contexts
{
    public abstract class CompilationContext
    {
        public CompilationContext? Parent { get; }
        private string? _name;

        protected CompilationContext(string? name, CompilationContext? parent)
        {
            _name = name;
            Parent = parent;
        }

        public virtual string? Name()
        {
            return _name;
        }
    }
}