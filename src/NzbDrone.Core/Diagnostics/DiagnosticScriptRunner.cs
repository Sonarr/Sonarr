using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Composition;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Core.Diagnostics
{
    public interface IDiagnosticScriptRunner
    {
        ScriptValidationResult Validate(ScriptRequest request);
        ScriptExecutionResult Execute(ScriptRequest request);
    }
    internal class CompilationContext
    {
        public HashSet<string> GlobalUsings { get; set; }
        public ScriptOptions Options { get; set; }
        public string Code { get; set; }
        public Script Script { get; set; }
        public Compilation LastCompilation { get; set; }
    }

    public class DiagnosticScriptRunner : IDiagnosticScriptRunner
    {
        private static readonly Regex _regexResolve = new Regex(@"=\s+Resolve<(I\w+)>", RegexOptions.Compiled);
        private static readonly Assembly[] _assemblies = new[] {
            typeof(AppFolderInfo).Assembly,
            typeof(DiagnosticScriptRunner).Assembly
        };

        private readonly IContainer _container;
        private readonly Logger _logger;

        private readonly ICached<object> _scriptStateCache;

        private WeakReference<CompilationContext> _lastCompilation;

        public DiagnosticScriptRunner(IContainer container, ICacheManager cacheManager, Logger logger)
        {
            _container = container;
            _logger = logger;

            // Note: using object instead of ScriptState to avoid the Scripting assembly to be loaded on startup.
            _scriptStateCache = cacheManager.GetCache<object>(GetType());

            _lastCompilation = new WeakReference<CompilationContext>(null);

            CheckScriptingAssemblyDelayLoad();
        }

        private void CheckScriptingAssemblyDelayLoad()
        {
            var scriptingLoaded = AppDomain.CurrentDomain.GetAssemblies().Any(v => v.FullName.Contains("Microsoft.CodeAnalysis"));
            if (scriptingLoaded)
            {
                // If we reach this code, then the class has been changed and as a result the Microsoft.CodeAnalysis.CSharp.Scripting assembly
                // was loaded on startup. This should be avoided since it takes more memory and is not used in normal situations.
                if (!RuntimeInfo.IsProduction)
                {
                    _logger.Error("Scripting assembly loaded prematurely.");
                }
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            }
        }

        public ScriptValidationResult Validate(ScriptRequest request)
        {
            lock (this)
            {
                var globalUsings = GetGlobalUsings(request.Code);

                _lastCompilation.TryGetTarget(out var lastCompilation);

                // Swapping SyntaxTree is significantly faster and uses less memory
                if (lastCompilation != null && lastCompilation.Code == request.Code)
                {
                    // Unchanged
                }
                else if (lastCompilation != null && lastCompilation.GlobalUsings == globalUsings)
                {
                    var newSyntaxTree = CSharpSyntaxTree.ParseText(request.Code, CSharpParseOptions.Default.WithKind(SourceCodeKind.Script));

                    lastCompilation.Script = null;
                    lastCompilation.Code = request.Code;
                    lastCompilation.LastCompilation = lastCompilation.LastCompilation.ReplaceSyntaxTree(lastCompilation.LastCompilation.SyntaxTrees.First(), newSyntaxTree);
                }
                else
                {
                    var options = GetOptions(globalUsings, request.Debug);

                    var script = CSharpScript.Create(request.Code, options, globalsType: typeof(ScriptContext));

                    var compilation = script.GetCompilation();

                    lastCompilation = new CompilationContext
                    {
                        GlobalUsings = globalUsings,
                        Options = options,
                        Code = request.Code,
                        Script = script,
                        LastCompilation = compilation
                    };

                    _lastCompilation.SetTarget(lastCompilation);
                }

                var diagnostics = lastCompilation.LastCompilation.GetDiagnostics();

                return new ScriptValidationResult
                {
                    Messages = diagnostics.Select(v => new ScriptDiagnostic(v)).ToArray()
                };
            }
        }

        public ScriptExecutionResult Execute(ScriptRequest request)
        {
            if (request.StateId != null)
            {
                return ExecuteAsync(request, request.StateId).GetAwaiter().GetResult();
            }
            else
            {
                return ExecuteAsync(request).GetAwaiter().GetResult();
            }
        }

        public Task<ScriptExecutionResult> ExecuteAsync(ScriptRequest request)
        {
            Script script;

            lock (this)
            {
                var globalUsings = GetGlobalUsings(request.Code);

                _lastCompilation.TryGetTarget(out var lastCompilation);

                if (lastCompilation != null && lastCompilation.Code == request.Code && lastCompilation.Script != null &&
                    lastCompilation.Options.EmitDebugInformation == request.Debug)
                {
                    script = lastCompilation.Script;
                }
                else
                {
                    try
                    {
                        var options = GetOptions(globalUsings, request.Debug);

                        // Note: Using classic Task pipeline since async-await early loads the Scripts assembly
                        script = CSharpScript.Create(request.Code, options, globalsType: typeof(ScriptContext));

                        var compilation = script.GetCompilation();

                        lastCompilation = new CompilationContext
                        {
                            GlobalUsings = globalUsings,
                            Options = options,
                            Code = request.Code,
                            Script = script,
                            LastCompilation = compilation
                        };

                        _lastCompilation.SetTarget(lastCompilation);
                    }
                    catch (CompilationErrorException ex)
                    {
                        return Task.FromResult(GetResult(ex));
                    }
                }
            }

            try
            {
                return script.RunAsync(new ScriptContext(_container, _logger), ex => true).ContinueWith(t =>
                {
                    var state = t.Result;

                    if (state.Exception != null)
                    {
                        return GetResult(state.Exception, request.Code);
                    }
                    else
                    {
                        return GetResult(state, request.StoreState);
                    }
                });
            }
            catch (CompilationErrorException ex)
            {
                return Task.FromResult(GetResult(ex));
            }
        }

        public Task<ScriptExecutionResult> ExecuteAsync(ScriptRequest request, string stateId)
        {
            var options = GetOptions(GetGlobalUsings(request.Code), request.Debug);

            var script = GetState(stateId);

            try
            {
                // Note: Using classic Task pipeline since async-await early loads the Scripts assembly
                return script.ContinueWithAsync(request.Code, options, ex => true).ContinueWith(t =>
                {
                    var state = t.Result;

                    if (state.Exception != null)
                    {
                        return GetResult(state.Exception, request.Code);
                    }
                    else
                    {
                        return GetResult(state, request.StoreState);
                    }
                });
            }
            catch (CompilationErrorException ex)
            {
                return Task.FromResult(GetResult(ex));
            }
        }

        private HashSet<string> GetGlobalUsings(string source)
        {
            var result = new HashSet<string>();

            // Make the syntax easier by parsing Resolve<I..> and auto add using
            var matches = _regexResolve.Matches(source);
            foreach (Match match in matches)
            {
                foreach (var ns in ResolveNamespaces(match.Groups[1].Value))
                {
                    result.Add(ns);
                }
            }

            return result;
        }

        private ScriptOptions GetOptions(HashSet<string> globalUsings, bool debug = false)
        {
            var options = ScriptOptions.Default
                .AddReferences(_assemblies)
                .AddImports(typeof(Task).Namespace)
                .AddImports(typeof(Enumerable).Namespace);

            if (debug)
            {
                options = options.WithEmitDebugInformation(true)
                                 .WithFilePath("ScriptConsole.cs")
                                 .WithFileEncoding(Encoding.UTF8);
            }

            // Make the syntax easier by parsing Resolve<I..> and auto add using
            foreach (var ns in globalUsings)
            {
                options = options.AddImports(ns);
            }

            return options;
        }

        private List<string> ResolveNamespaces(string type)
        {
            var types = _assemblies
                .SelectMany(v => v.GetExportedTypes())
                .Where(v => v.Name == type);

            var namespaces = types.Select(v => v.Namespace)
                .Distinct()
                .ToList();

            return namespaces;
        }

        private ScriptExecutionResult GetResult(ScriptState state, bool storeState)
        {
            var variables = state.Variables.Where(v => !v.Type.IsInterface || !v.Type.Namespace.StartsWith("NzbDrone")).ToDictionary(v => v.Name, v => v.Value);

            var result = new ScriptExecutionResult
            {
                StateId = storeState ? StoreState(state) : null,
                ReturnValue = state.ReturnValue,
                Variables = variables.Any() ? variables : null
            };

            return result;
        }

        private ScriptExecutionResult GetResult(CompilationErrorException ex)
        {
            return new ScriptExecutionResult
            {
                Exception = ex,
                Validation = new ScriptValidationResult { Messages = ex.Diagnostics.Select(v => new ScriptDiagnostic(v)).ToArray() }
            };
        }
        
        private ScriptExecutionResult GetResult(Exception ex, string code)
        {
            var result = new ScriptExecutionResult
            {
                Exception = ex
            };

            StackFrame firstScriptFrame = null;
            var stackTrace = new StackTrace(ex, true);
            
            for (int i = 0; i < stackTrace.FrameCount; i++)
            {
                var frame = stackTrace.GetFrame(i);
                if (frame.GetFileName() == "ScriptConsole.cs" && result.Validation == null)
                {
                    firstScriptFrame = frame;
                    break;
                }
            }

            // Get the full message text till the scripting runtime
            var fullMessage = ex.ToString();
            var fullMessageLines = fullMessage.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            var idx = Array.FindIndex(fullMessageLines, 1, v => v.Contains("RunSubmissionsAsync")) - 2;
            if (idx > 2 && fullMessageLines[idx - 1].StartsWith("---"))
            {
                idx--;
            }

            if (idx > 1)
            {
                fullMessage = string.Join("\n", fullMessageLines.Take(idx));
            }

            var lines = code.Split('\n');
            ScriptDiagnostic diagnostic;
            if (firstScriptFrame != null)
            {
                var startLineNumber = firstScriptFrame.GetFileLineNumber();
                var startColumn = firstScriptFrame.GetFileColumnNumber();
                var endLineNumber = startLineNumber;
                var endColumn = startColumn == 1 ? lines[startLineNumber - 1].Length : startColumn;
                diagnostic = new ScriptDiagnostic(ex, startLineNumber, startColumn, endLineNumber, endColumn, fullMessage);
            }
            else
            {
                diagnostic = new ScriptDiagnostic(ex, 1, 1, lines.Length, lines.Last().Length, fullMessage);
            }

            result.Validation = new ScriptValidationResult
            {
                Messages = new[]
                {
                    diagnostic
                }
            };

            return result;
        }

        private ScriptState GetState(string stateID)
        {
            var state = _scriptStateCache.Find(stateID);

            if (state == null)
                throw new KeyNotFoundException($"ScriptState {stateID} no longer exists");

            return state as ScriptState;
        }

        private void RemoveState(string stateID)
        {
            _scriptStateCache.Remove(stateID);
        }

        private string StoreState(ScriptState state)
        {
            var key = Guid.NewGuid().ToString();

            _scriptStateCache.Set(key, state, TimeSpan.FromHours(1));

            return key;
        }
    }
}
