using AutoRefTypes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SkyBot.Osu.AutoRef.Scripting
{
    public static class ScriptingCompiler
    {
        static readonly object _compilerLock = new object();

        public static bool TryCompile(string source, out MemoryStream assemblyStream, out List<Exception> exceptions)
        {
            lock (_compilerLock)
            {
                assemblyStream = new MemoryStream();
                CSharpCompilation compiled = Compile(source);
                exceptions = new List<Exception>();

                foreach (var diag in compiled.GetDiagnostics())
                {
                    if (diag.Severity == DiagnosticSeverity.Error)
                        exceptions.Add(new Exception(diag.ToString()));
                }

                if (exceptions.Count > 0)
                    return false;

                EmitResult result = compiled.Emit(assemblyStream);
                assemblyStream.Seek(0, SeekOrigin.Begin);

                if (result.Diagnostics != null)
                {
                    foreach (var diag in result.Diagnostics)
                    {
                        if (diag.Severity == DiagnosticSeverity.Error)
                            exceptions.Add(new Exception($"Exception at {diag.Location}: {diag.Descriptor.Title.ToString()}: {diag.Descriptor.Description.ToString()}"));
                    }

                    if (exceptions.Count > 0)
                        return false;
                }

                return true;
            }
        }

        static CSharpCompilation Compile(string source)
        {
            CSharpParseOptions options = new CSharpParseOptions(LanguageVersion.CSharp9);
            MetadataReference[] references = Program.MetaReferences;

            SourceText src = SourceText.From(source);
            SyntaxTree syntaxTree = SyntaxFactory.ParseSyntaxTree(src, options);
            CSharpCompilation compiled = CSharpCompilation.Create($"ArcScript.dll", new[] { syntaxTree }, references,
                                                                    new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default));

            return compiled;
        }
    }
}
