using System;
using System.Collections.Immutable;
using System.Threading;

namespace BuzzOff.Server
{
    /// <summary>
    /// When Next() is called, attempts (but doesn't guarantee) to return the least recently accessed element of its internal buffer.
    /// This class is thread-safe.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ImmutableCircularBuffer<T>
    {
        private static readonly Random _rand = new Random();
        private readonly ImmutableArray<T> _data;
        private int idx;

        public ImmutableCircularBuffer(params T[] data)
        {
            _data = ImmutableArray.Create(data);

            // without this, the server always starts on the same thing, 
            // which will lead to problems if the server restarts while people are using it.
            lock (_rand)
            {
                idx = _rand.Next(0, _data.Length);
            }
        }

        /// <summary>
        /// Thread-safely returns an element that is either the least recently used or close to it.
        /// </summary>
        /// <returns></returns>
        public T Next()
        {
            // increment idx. because this is interlocked, every thread currently in this function for this array
            // will have different localIdxes (unless you have more threads than you have elements. Don't do this.)
            var localIdx = Interlocked.Increment(ref idx);

            if (localIdx >= _data.Length)
            {
                // guarantee that we have a valid index for the array
                var inBoundsIdx = localIdx % _data.Length;

                // basically, if we're the first one to set the index, set the index
                // this ensures that if multiple threads are all trying to wrap the index around to the start
                // of the array, only one will succeed.
                // It doesn't much matter which one.
                // This is where I believe it's possible to skip indices if there's thread contention.
                Interlocked.CompareExchange(ref idx, inBoundsIdx, localIdx);

                // make sure to use the guaranteed-in-bounds index
                localIdx = inBoundsIdx;
            }

            return _data[localIdx];
        }
    }

}
