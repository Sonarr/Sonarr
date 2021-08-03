using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace NzbDrone.RuntimePatches.Mono
{
    // Mono 5.x - 6.x bug 19886
    // The BoringTLS provider does not enable the trust-first option that's default on in openssl 1.1.0 and up.
    // This prevents it from building the short trusted chain and errors out on old (expired) chains included in the certificate.
    // This is a problem with Cross-Signed certificates that have an expired legacy root signing the new root.
    // The Flags default is 0, while X509_V_FLAG_TRUSTED_FIRST is 0x8000.
    // There's no way to override the default flags via an option in mono so we have to hook in.
    public class BoringTLSVerifyFlagsPatch : MonoRuntimePatchBase
    {
        private static BoringTLSVerifyFlagsPatch Instance;

        public override Version MonoMinVersion => new Version(5, 0);
        public override Version MonoMaxVersion => new Version(7, 0);

        protected override void Patch()
        {
            Instance = this;

            TryPatchMethod("Mono.Btls.MonoBtlsX509VerifyParam, System", "GetSslServer");
        }

        // We need a Transpiler coz MonoBtlsX509VerifyParam is non-public
        // Note that MonoBtlsProvider.GetVerifyParam would be a more 'correct' patch location

        // public static MonoBtlsX509VerifyParam GetSslServer ()
        // {
        // - return Lookup("ssl_server", true);
        // + var orig = Lookup("ssl_server", true);
        // + var copy = orig.Copy();
        // + orig.Dispose();
        // + copy.SetFlags(0x8000);
        // + return copy;
        // }
        private static IEnumerable<CodeInstruction> Transpiler_GetSslServer(IEnumerable<CodeInstruction> instructions, MethodBase method, ILGenerator generator)
        {
            var codes = instructions.ToList();

            var patchable = codes.Matches(OpCodes.Ldstr, OpCodes.Ldc_I4_1, OpCodes.Call, OpCodes.Ret);

            Instance.DebugOpcodes("Before", codes);

            var targetType = method.DeclaringType;
            var copyMethod = targetType.GetMethod("Copy", new Type[0]);
            var disposeMethod = targetType.GetMethod("Dispose", new Type[0]);
            var setFlagsMethod = targetType.GetMethod("SetFlags", new[] { typeof(ulong) });

            if (patchable && copyMethod != null && disposeMethod != null && setFlagsMethod != null)
            {
                var copy = generator.DeclareLocal(targetType);

                // Remove Ret
                codes.RemoveAt(codes.Count - 1);

                codes.Add(new CodeInstruction(OpCodes.Dup));
                codes.Add(new CodeInstruction(OpCodes.Call, copyMethod));           // Copy the readonly original
                codes.Add(new CodeInstruction(OpCodes.Stloc, copy));
                codes.Add(new CodeInstruction(OpCodes.Callvirt, disposeMethod));    // Dispose the original
                codes.Add(new CodeInstruction(OpCodes.Ldloc, copy));
                codes.Add(new CodeInstruction(OpCodes.Dup));
                codes.Add(new CodeInstruction(OpCodes.Ldc_I8, 0x8000L));            // X509_V_FLAG_TRUSTED_FIRST
                codes.Add(new CodeInstruction(OpCodes.Call, setFlagsMethod));       // SetFlags is an or-operation
                codes.Add(new CodeInstruction(OpCodes.Ret));

                Instance.DebugOpcodes("After", codes);

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
