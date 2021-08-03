using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace NzbDrone.RuntimePatches.Mono
{
    // Mono 6.0 - 6.x bug 16122
    // Unimplemented method used in GzipStream initiated via the http stack, the method existed as far back as 5.10
    public class DeflateStreamAsyncPatch : MonoRuntimePatchBase
    {
        private static DeflateStreamAsyncPatch Instance;

        public override Version MonoMinVersion => new Version(6, 0);
        public override Version MonoMaxVersion => new Version(6, 0, 0, 334);

        protected override void Patch()
        {
            Instance = this;

            TryPatchMethod(typeof(DeflateStream), "ReadAsyncMemory", "Memory<Byte>", "CancellationToken");
            TryPatchMethod(typeof(DeflateStream), "WriteAsyncMemory", "ReadOnlyMemory<Byte>", "CancellationToken");
        }

        // We need a Transpiler coz these methods are for net4.7.2 so we cannot access the types directly

        // internal ValueTask<int> ReadAsyncMemory(Memory<byte> destination, CancellationToken cancellationToken)
        // {
        // - throw new NotImplementedException();
        // + return base.ReadAsync(destination, cancellationToken);
        // }
        private static IEnumerable<CodeInstruction> Transpiler_ReadAsyncMemory(IEnumerable<CodeInstruction> instructions, MethodBase method)
        {
            var codes = instructions.ToList();

            var patchable = codes.Matches(OpCodes.Newobj, OpCodes.Throw);

            var readAsync = method.DeclaringType.BaseType.GetMethod("ReadAsync", method.GetParameterTypes());

            if (patchable && readAsync != null)
            {
                codes.Clear();

                codes.Add(new CodeInstruction(OpCodes.Ldarg_0));
                codes.Add(new CodeInstruction(OpCodes.Ldarg_1));
                codes.Add(new CodeInstruction(OpCodes.Ldarg_2));
                codes.Add(new CodeInstruction(OpCodes.Call, readAsync));
                codes.Add(new CodeInstruction(OpCodes.Ret));

                Instance.Debug($"Patch applied to method {method.GetSimplifiedName()}");
            }
            else
            {
                Instance.Error($"Skipped patching method {method.GetSimplifiedName()}: Method construct different than expected");
            }

            return codes;
        }

        // internal ValueTask WriteAsyncMemory(ReadOnlyMemory<byte> source, CancellationToken cancellationToken)
        // {
        // - throw new NotImplementedException();
        // + return base.WriteAsync(source, cancellationToken);
        // }
        private static IEnumerable<CodeInstruction> Transpiler_WriteAsyncMemory(IEnumerable<CodeInstruction> instructions, MethodBase method)
        {
            var codes = instructions.ToList();

            var patchable = codes.Matches(OpCodes.Newobj, OpCodes.Throw);

            var writeAsync = method.DeclaringType.BaseType.GetMethod("WriteAsync", method.GetParameterTypes());

            if (patchable && writeAsync != null)
            {
                codes.Clear();

                codes.Add(new CodeInstruction(OpCodes.Ldarg_0));
                codes.Add(new CodeInstruction(OpCodes.Ldarg_1));
                codes.Add(new CodeInstruction(OpCodes.Ldarg_2));
                codes.Add(new CodeInstruction(OpCodes.Call, writeAsync));
                codes.Add(new CodeInstruction(OpCodes.Ret));

                Instance.Debug($"Patch applied to method {method.GetSimplifiedName()}");
            }
            else
            {
                Instance.Error($"Skipped patching method {method.GetSimplifiedName()}: Method construct different than expected");
            }

            return codes;
        }
    }
}
