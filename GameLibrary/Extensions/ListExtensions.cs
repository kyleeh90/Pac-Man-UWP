using System.Collections.Generic;

namespace GameLibrary
{
    public static class ListExtensions
    {
        /// <summary>
        /// Removes the element at the specified index and replaces it with the last element in the list.
        /// </summary>
        /// <param name="index">The index to remove from this list.</param>
        /// <remarks>Makes RemoveAt an O(1) operation instead of O(n) where n is list count - index.</remarks>
        public static void SwapRemoveAt<T>(this List<T> list, int index)
        {
            list[index] = list[list.Count - 1];
            list.RemoveAt(list.Count - 1);
        }
    }
}
