using System;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using System.Threading;

namespace MonoTorrent.Common
{
	public delegate long Operation<T>(T target);

    public static class Toolbox
    {
        private static Random r = new Random();
		public static int Count<T>(IEnumerable<T> enumerable, Predicate<T> predicate)
		{
			int count = 0;

			foreach (T t in enumerable)
				if (predicate(t))
					count++;

			return count;
		}

		public static long Accumulate<T>(IEnumerable<T> enumerable, Operation<T> action)
		{
            long count = 0;

			foreach (T t in enumerable)
				count += action(t);
		
			return count;
		}

        public static void RaiseAsyncEvent<T>(EventHandler<T> e, object o, T args)
            where T : EventArgs
        {
            if (e == null)
                return;

            ThreadPool.QueueUserWorkItem(delegate {
                if (e != null)
                    e(o, args);
            });
        }
        /// <summary>
        /// Randomizes the contents of the array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        public static void Randomize<T>(List<T> array)
        {
            List<T> clone = new List<T>(array);
            array.Clear();

            while (clone.Count > 0)
            {
                int index = r.Next(0, clone.Count);
                array.Add(clone[index]);
                clone.RemoveAt(index);
            }
        }

        /// <summary>
        /// Switches the positions of two elements in an array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="first"></param>
        /// <param name="second"></param>
        public static void Switch<T>(IList<T> array, int first, int second)
        {
            T obj = array[first];
            array[first] = array[second];
            array[second] = obj;
        }

        /// <summary>
        /// Checks to see if the contents of two byte arrays are equal
        /// </summary>
        /// <param name="array1">The first array</param>
        /// <param name="array2">The second array</param>
        /// <returns>True if the arrays are equal, false if they aren't</returns>
        public static bool ByteMatch(byte[] array1, byte[] array2)
        {
            if (array1 == null)
                throw new ArgumentNullException("array1");
            if (array2 == null)
                throw new ArgumentNullException("array2");

            if (array1.Length != array2.Length)
                return false;

            return ByteMatch(array1, 0, array2, 0, array1.Length);
        }

        /// <summary>
        /// Checks to see if the contents of two byte arrays are equal
        /// </summary>
        /// <param name="array1">The first array</param>
        /// <param name="array2">The second array</param>
        /// <param name="offset1">The starting index for the first array</param>
        /// <param name="offset2">The starting index for the second array</param>
        /// <param name="count">The number of bytes to check</param>
        /// <returns></returns>
        public static bool ByteMatch(byte[] array1, int offset1, byte[] array2, int offset2, int count)
        {
            if (array1 == null)
                throw new ArgumentNullException("array1");
            if (array2 == null)
                throw new ArgumentNullException("array2");

            // If either of the arrays is too small, they're not equal
            if ((array1.Length - offset1) < count || (array2.Length - offset2) < count)
                return false;

            // Check if any elements are unequal
            for (int i = 0; i < count; i++)
                if (array1[offset1 + i] != array2[offset2 + i])
                    return false;

            return true;
        }
    }
}
