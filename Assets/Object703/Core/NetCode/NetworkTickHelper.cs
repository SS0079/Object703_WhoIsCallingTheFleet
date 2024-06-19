using Unity.NetCode;

namespace Object703.Core.NetCode
{
    public static class NetworkTickHelper
    {
        public static NetworkTick AddSpan(this NetworkTick now, uint span)
        {
            var result = now;
            result.Add(span);
            return result;
        }
    }
}