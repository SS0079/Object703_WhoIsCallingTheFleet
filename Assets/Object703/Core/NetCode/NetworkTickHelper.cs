using Unity.NetCode;

namespace Object703.Core
{
    public static class NetworkTickHelper
    {
        public static NetworkTick AddSpan(this NetworkTick now, uint span)
        {
            var result = now;
            result.Add(span);
            return result;
        }

        public static NetworkTick SubSpan(this NetworkTick now, uint span)
        {
            var result = now;
            result.Subtract(span);
            return result;
        }
    }
}