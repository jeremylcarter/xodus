using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace System.Linq
{
    public static class IEnumerableExtensions
    {
        public static Task<T[]> WhenAll<T>(this IEnumerable<Task<T>> source)
        {
            return Task.WhenAll(source);
        }
    }
}

namespace Xodus
{
    public static class Extensions
    {
        public static IEnumerable<T> GetChildrenOfType<T>(this DependencyObject start) where T : class
        {
            var queue = new Queue<DependencyObject>();
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                var item = queue.Dequeue();

                var realItem = item as T;
                if (realItem != null)
                    yield return realItem;

                if (item == null)
                    continue;

                var count = VisualTreeHelper.GetChildrenCount(item);
                for (var i = 0; i < count; i++)
                    queue.Enqueue(VisualTreeHelper.GetChild(item, i));
            }
        }
    }
}