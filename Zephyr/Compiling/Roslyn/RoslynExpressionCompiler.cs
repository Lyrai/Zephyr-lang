// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.Emit;
#if NET472
using Roslyn.Utilities;
#endif
using Zephyr.SyntaxAnalysis.ASTNodes;
using BindingDiagnosticBag = Microsoft.CodeAnalysis.CSharp.BindingDiagnosticBag;

namespace Zephyr.Compiling.Roslyn;

internal class RoslynExpressionCompiler: BaseRoslynCompiler<object>
{
    private readonly PEModuleBuilder _moduleBuilder;
    
    public RoslynExpressionCompiler(CSharpCompilation compilation, PEModuleBuilder moduleBuilder)
    {
        _compilation = compilation;
        _moduleBuilder = moduleBuilder;
    }

    public RoslynExpressionCompiler(RoslynDeclarationsCompiler compiler)
    {
        compiler.CompilationFinished();
        _compilation = compiler.Compilation;
        _moduleBuilder = compiler.CreateModuleBuilder(_compilation);
        compiler.SetEntryPoint(_moduleBuilder);
    }

    public void Compile(ImmutableDictionary<string, Node> functions)
    {
        var bindingDiagnostics = new BindingDiagnosticBag(DiagnosticBag.GetInstance());
        var embeddedTextsCompiler = new Microsoft.CodeAnalysis.CSharp.MethodCompiler(
            _compilation,
            _moduleBuilder,
            false,
            false,
            true,
            bindingDiagnostics,
            null,
            null,
            CancellationToken.None);
        embeddedTextsCompiler.CompileSynthesizedMethods(_moduleBuilder.GetEmbeddedTypes(bindingDiagnostics), bindingDiagnostics);
        
        foreach (var (qualifiedName, node) in functions)
        {
            var (className, functionName) = ParseQualifiedName(qualifiedName);
            
            var builder = new ILBuilder(_moduleBuilder, new LocalSlotManager(null), OptimizationLevel.Release, true);
            var method = GetSymbol(_moduleBuilder, className, functionName) as MethodSymbol;
            var compiler = new MethodCompiler(builder, node as FuncDeclNode, _moduleBuilder, method);
            
            var methodBody = compiler.Compile();
            
            _moduleBuilder.SetMethodBody(method, methodBody);
        }
    }

    public bool Emit(string path, DiagnosticBag diagnostics)
    {
        _moduleBuilder.CompilationFinished();
        var peStreamProvider = new Compilation.SimpleEmitStreamProvider(File.Open(path, FileMode.Create));
        var success = _compilation.SerializeToPeStream(
            _moduleBuilder,
            peStreamProvider,
            null,
            null,
            null,
            null,
            diagnostics,
            EmitOptions.Default,
            null,
            CancellationToken.None
        );

        if (success)
        {
            GenerateRuntimeConfig(Path.GetFullPath(path));
        }

        return success;
    }

    private (string className, string functionName) ParseQualifiedName(string qualifiedName)
    {
        var split = qualifiedName.Split(new [] { "::" }, StringSplitOptions.None);
        Debug.Assert(split.Length == 2);
        return (split[0], split[1]);
    }

    private void GenerateRuntimeConfig(string path)
    {
        var configPath = Path.Combine(Directory.GetParent(path).FullName, Path.GetFileNameWithoutExtension(path) + ".runtimeconfig.json");
        using var stream = new StreamWriter(configPath);
        var netCoreVersion = "6.0.0";
        stream.WriteLine(
@"{
    ""runtimeOptions"": {
        ""tfm"": ""net6.0"",
        ""framework"": {
            ""name"": ""Microsoft.NETCore.App"",
            ""version"": """ + netCoreVersion + @"""
        }
    }
}");
    }
}
