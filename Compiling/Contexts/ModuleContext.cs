using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Zephyr.Compiling.Contexts
{
    public class ModuleContext : CompilationContext
    {
        public ModuleBuilder Builder { get; }
        private Dictionary<string, Type> _types;

        public ModuleContext(string? name, CompilationContext? parent, ModuleBuilder builder) : base(name, parent)
        {
            _types = new Dictionary<string, Type>();
            Builder = builder;
        }
    }
}