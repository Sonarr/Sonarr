using System.Threading;

namespace Microsoft.AspNet.SignalR.Infrastructure
{

    public static class InterlockedHelper
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "0#", Justification="This is an interlocked helper...")]
        public static bool CompareExchangeOr(ref int location, int value, int comparandA, int comparandB)
        {
            return Interlocked.CompareExchange(ref location, value, comparandA) == comparandA ||
                   Interlocked.CompareExchange(ref location, value, comparandB) == comparandB;
        }
    }

}