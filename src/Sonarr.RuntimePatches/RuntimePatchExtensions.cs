using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;

namespace NzbDrone.RuntimePatches
{
    public static class RuntimePatchExtensions
    {
        public static bool Matches(this List<CodeInstruction> instructions, params OpCode[] opcodes)
        {
            var codes = instructions.Select(v => v.opcode).Where(v => v != OpCodes.Nop).ToList();

            if (codes.Count != opcodes.Length)
            {
                return false;
            }

            for (var i = 0; i < codes.Count; i++)
            {
                if (codes[i] != opcodes[i])
                {
                    return false;
                }
            }

            return true;
        }

        public static Type[] GetParameterTypes(this MethodBase method)
        {
            return Array.ConvertAll(method.GetParameters(), v => v.ParameterType);
        }

        public static string GetSimplifiedName(this MethodBase method, bool includeNamespace = false)
        {
            return $"{method.DeclaringType.GetSimplifiedName()}.{method.Name}";
        }

        public static string GetSimplifiedName(this Type t, bool includeNamespace = false)
        {
            StringBuilder sb = new StringBuilder();

            if (includeNamespace && string.IsNullOrEmpty(t.Namespace))
            {
                sb.Append(t.Namespace);
                sb.Append('.');
            }

            if (t.IsGenericType)
            {
                sb.Append(t.Name, 0, t.Name.LastIndexOf('`'));
                sb.Append('<');
                var args = t.GetGenericArguments();
                for (int i = 0; i < args.Length; i++)
                {
                    if (i != 0)
                    {
                        sb.Append(", ");
                    }

                    sb.Append(GetSimplifiedName(args[i], includeNamespace));
                }

                sb.Append('>');
            }
            else
            {
                sb.Append(t.Name);
            }

            return sb.ToString();
        }
    }
}
