using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace NzbDrone.RuntimePatches
{
    public abstract class RuntimePatchBase
    {
        private Harmony _harmony;

        internal static bool IsDebug;

        public virtual bool ShouldPatch() => true;
        protected abstract void Patch();

        public void Patch(Harmony harmony)
        {
            _harmony = harmony;

            if (ShouldPatch())
            {
                Patch();
            }
        }

        protected const BindingFlags DefaultBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        protected static MethodInfo FindMethod(Type type, string methodName, params string[] paramTypes)
        {
            foreach (var methodInfo in type.GetMethods(DefaultBindingFlags))
            {
                if (methodInfo.Name != methodName)
                {
                    continue;
                }

                var parameters = methodInfo.GetParameters();

                if (parameters.Length != paramTypes.Length)
                {
                    continue;
                }

                var parametersMatch = true;
                for (var i = 0; i < parameters.Length; i++)
                {
                    if (parameters[i].ParameterType.Name != paramTypes[i] &&
                        parameters[i].ParameterType.FullName != paramTypes[i] &&
                        parameters[i].ParameterType.GetSimplifiedName() != paramTypes[i] &&
                        parameters[i].ParameterType.GetSimplifiedName(true) != paramTypes[i])
                    {
                        parametersMatch = false;
                        break;
                    }
                }

                if (!parametersMatch)
                {
                    continue;
                }

                return methodInfo;
            }

            return null;
        }

        protected void PatchMethod(MethodInfo methodInfo)
        {
            var prefix = GetPatchMethod("Prefix_" + methodInfo.Name);
            var postfix = GetPatchMethod("Postfix_" + methodInfo.Name);
            var transpiler = GetPatchMethod("Transpiler_" + methodInfo.Name);

            _harmony.Patch(methodInfo, prefix, postfix, transpiler);
        }

        protected void TryPatchMethod(string typeName, string methodName, params string[] paramTypes)
        {
            var type = Type.GetType(typeName);

            if (type != null)
            {
                TryPatchMethod(type, "GetSslServer");
            }
            else
            {
                Debug($"Skipped patching method {typeName}.{methodName}: Type not found");
            }
        }

        protected void TryPatchMethod(Type type, string methodName, params string[] paramTypes)
        {
            var methodInfo = FindMethod(type, methodName, paramTypes);
            if (methodInfo != null)
            {
                PatchMethod(methodInfo);
            }
            else
            {
                Debug($"Skipped patching method {type.GetSimplifiedName()}.{methodName}: Method not found");
            }
        }

        private HarmonyMethod GetPatchMethod(string name)
        {
            var patch = GetType().GetMethod(name, DefaultBindingFlags);
            if (patch != null)
            {
                return new HarmonyMethod(patch);
            }

            return null;
        }

        protected void DebugOpcodes(string prefix, List<CodeInstruction> codes)
        {
            if (IsDebug)
            {
                Log($"Opcodes {prefix}:");
                foreach (var code in codes)
                {
                    Console.WriteLine($"  {code}");
                }
            }
        }

        protected void Debug(string log)
        {
            if (IsDebug)
            {
                Log(log);
            }
        }

        protected void Error(string log)
        {
            Log(log);
        }

        protected virtual void Log(string log)
        {
            Console.WriteLine($"RuntimePatch {GetType().Name}: {log}");
        }
    }
}
