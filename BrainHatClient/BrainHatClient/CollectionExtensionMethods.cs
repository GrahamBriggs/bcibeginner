using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrainHatClient
{
    public static class CollectionExtensionMethods
    {
        /// <summary>
        /// Return a collection that includes only a single instance of any object in the original collection
        /// </summary>
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }


        /// <summary>
        /// Add range function for concurrent queue
        /// </summary>
        public static void AddRange<T>(this ConcurrentQueue<T> value, IEnumerable<T> toAdd)
        {
            foreach (var element in toAdd)
            {
                value.Enqueue(element);
            }
        }

        /// <summary>
        /// Add range function for concurrent bag
        /// </summary>
        public static void AddRange<T>(this ConcurrentBag<T> value, IEnumerable<T> toAdd)
        {
            foreach (var element in toAdd)
            {
                value.Add(element);
            }
        }

        /// <summary>
        /// remove all from concurrent queue
        /// </summary>
        public static void RemoveAll<T>(this ConcurrentQueue<T> value)
        {
            try
            {
                while (!value.IsEmpty)
                    value.TryDequeue(out var nextItem);
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// remove all from concurrent bag
        /// </summary>
        public static void RemoveAll<T>(this ConcurrentBag<T> value)
        {
            try
            {
                while (!value.IsEmpty)
                {
                    value.TryTake(out var nextItem);
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
