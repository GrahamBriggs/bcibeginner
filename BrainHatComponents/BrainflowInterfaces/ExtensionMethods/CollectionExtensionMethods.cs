using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using System.Linq;

namespace BrainflowInterfaces
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


        // Concurrent queue extension methods
        //

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
        /// remove all from concurrent queue
        /// </summary>
        public static void RemoveAll<T>(this ConcurrentStack<T> value)
        {
            try
            {
                while (!value.IsEmpty)
                    value.TryPop(out var nextItem);
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


       


        /// <summary>
        /// Median function for an enumerable of doubles
        /// </summary>
        public static double Median(this IEnumerable<double> value)
        {
            // thanks https://blogs.msmvps.com/deborahk/linq-mean-median-and-mode/ 
            int numberCount = value.Count();
            int halfIndex = value.Count() / 2;
            var sortedNumbers = value.OrderBy(n => n);
            double median;
            if ((numberCount % 2) == 0)
            {
                median = (sortedNumbers.ElementAt(halfIndex) + sortedNumbers.ElementAt(halfIndex - 1)) / 2;
            }
            else
            {
                median = sortedNumbers.ElementAt(halfIndex);
            }


            return median;
        }


        /// <summary>
        /// Standard Deviation from an enumerable of doubles
        /// </summary>
        public static double StdDev(this IEnumerable<double> value)
        {
            // thanks https://stackoverflow.com/questions/2253874/standard-deviation-in-linq 
            double ret = 0;
            int count = value.Count();
            if (count > 1)
            {
                //Compute the Average
                double avg = value.Average();

                //Perform the Sum of (value-avg)^2
                double sum = value.Sum(d => (d - avg) * (d - avg));

                //Put it all together
                ret = Math.Sqrt(sum / count);
            }
            return ret;
        }




    }
}
