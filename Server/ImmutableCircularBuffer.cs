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

		/// <summary>
		/// Creates a new <see cref="ImmutableCircularBuffer{T}"/> with its internal buffer filled with the elements in <paramref name="data"/>. The first element returned is random.
		/// </summary>
		/// <param name="data"></param>
		public ImmutableCircularBuffer(params T[] data)
		{
			// without this, the server always starts on the same thing, 
			// which will lead to problems if the server restarts while people are using it.
			// plus, shuffling the list on every startup increases the variety of possible names
			lock (_rand)
			{
				for (int i = 0; i < data.Length - 1; i++)
                {
					T temp = data[i];
					int swapIdx = _rand.Next(i, data.Length);
					data[i] = data[swapIdx];
					data[swapIdx] = temp;
                }
				idx = _rand.Next(0, data.Length);
			}

			_data = ImmutableArray.Create(data);
		}


		/// <summary>
		/// Thread-safely returns an element that is either the least recently returned or close to it.
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
				// That's fine for our purposes- it only matters that each thread has a different index.
				Interlocked.CompareExchange(ref idx, inBoundsIdx, localIdx);

				// make sure to use the guaranteed in-bounds index
				localIdx = inBoundsIdx;
			}

			return _data[localIdx];
		}
	}

}
