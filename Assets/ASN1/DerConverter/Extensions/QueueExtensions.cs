﻿using System.Collections.Generic;
using System.Linq;

namespace DerConverter
{
    internal static class QueueExtensions
    {
        public static IEnumerable<T> Dequeue<T>(this Queue<T> queue, long count)
        {
            for (long i = 0; i < count; i++) yield return queue.Dequeue();
        }

        public static IEnumerable<T> DequeueAll<T>(this Queue<T> queue)
        {
            while (queue.Any()) yield return queue.Dequeue();
        }
    }
}
