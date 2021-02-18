using System.Linq;

namespace SimulWatch.Utility
{
    public static class Extension
    {
        public static T[] Concatenate<T>(this T[] first, T[] second)
        {
            if (first == null) {
                return second;
            }
            if (second == null) {
                return first;
            }
 
            return first.Concat(second).ToArray();
        }
    }
}