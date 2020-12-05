using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.IO;
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
                assemblyStream = null;
                CSharpCompilation compiled = Compile(source);
                exceptions = new List<Exception>();

                foreach (var diag in compiled.GetDiagnostics())
                {
                    if (diag.Severity == DiagnosticSeverity.Error)
                        exceptions.Add(new Exception($"Exception at {diag.Location}: {diag.Descriptor.Title.ToString()}: {diag.Descriptor.Description.ToString()}"));
                }

                if (exceptions.Count > 0)
                    return false;

                MemoryStream mstream = new MemoryStream();
                EmitResult result = compiled.Emit(mstream);
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
            CSharpParseOptions options = new CSharpParseOptions(LanguageVersion.CSharp8);
            MetadataReference[] references = CreateReferences();

            SourceText src = SourceText.From(source);
            SyntaxTree syntaxTree = SyntaxFactory.ParseSyntaxTree(src, options);

            CSharpCompilation compiled = CSharpCompilation.Create($"ArcScript.dll", new[] { syntaxTree }, references,
                                                                    new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default));

            return compiled;
        }

        static MetadataReference[] CreateReferences()
        {
            MetadataReference[] references = new MetadataReference[]
            {
                Ref<object>(),
                Ref<string>(),
                Ref<int>(),
                Ref<double>(),
                Ref<float>(),
                Ref<bool>()
            };

            //TODO: add library for communcation between scripts

            return references;

            MetadataReference Ref<T>()
            {
                return MetadataReference.CreateFromFile(typeof(T).Assembly.Location);
            }
        }
    }
}
