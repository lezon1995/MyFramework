using System;
using System.Collections.Generic;

namespace MoreMountains
{
    /// <summary>
    /// A generic heap (priority queue) implementation for efficient retrieval of the highest-priority item.
    /// </summary>
    /// <typeparam name="T">The type of elements stored in the heap, which must implement IHeapItem<T>.</typeparam>
    public class Heap<T> where T : IHeapItem<T>
    {
        T[] Content;

        // Current size of the heap
        int itemCount;

        /// <summary>
        /// Initializes a new heap with a predefined size.
        /// </summary>
        /// <param name="size">The maximum number of elements the heap can store.</param>
        public Heap(int size)
        {
            Content = new T[size];
            itemCount = 0;
        }

        public void Clear()
        {
            Array.Clear(Content, 0, Content.Length);
            itemCount = 0;
        }


        /// <summary>
        /// Gets the current number of elements in the heap.
        /// </summary>
        public int Count => itemCount;

        /// <summary>
        /// Adds an item to the heap while maintaining the heap property.
        /// </summary>
        /// <param name="item">The item to be added.</param>
        public void Add(T item)
        {
            item.Index = itemCount;
            Content[itemCount] = item;
            SortUp(item);
            itemCount++;
        }

        /// <summary>
        /// Removes and returns the highest-priority item from the heap.
        /// </summary>
        /// <returns>The highest-priority item in the heap.</returns>
        public T Pop()
        {
            T root = Content[0];
            itemCount--;
            Content[0] = Content[itemCount];
            Content[0].Index = 0;
            SortDown(Content[0]);
            return root;
        }

        /// <summary>
        /// Checks whether the heap contains a specific item.
        /// </summary>
        /// <param name="item">The item to check for.</param>
        /// <returns>True if the item is in the heap, false otherwise.</returns>
        public bool Contains(T item)
        {
            return EqualityComparer<T>.Default.Equals(Content[item.Index], item);
        }

        /// <summary>
        /// Updates an item�s position in the heap based on its new priority.
        /// </summary>
        /// <param name="item">The item to be updated.</param>
        public void Update(T item)
        {
            SortUp(item);
        }

        int GetLeftChildIndex(T item)
        {
            return item.Index * 2 + 1;
        }

        int GetRightChildIndex(T item)
        {
            return item.Index * 2 + 2;
        }

        T GetParent(T item)
        {
            return Content[(item.Index - 1) / 2];
        }

        void SortDown(T item)
        {
            while (true)
            {
                int leftChildIndex = GetLeftChildIndex(item);
                int rightChildIndex = GetRightChildIndex(item);
                if (leftChildIndex >= itemCount)
                {
                    return;
                }

                T swapItem = Content[leftChildIndex];
                if (rightChildIndex < itemCount)
                {
                    if (Content[leftChildIndex].CompareTo(Content[rightChildIndex]) < 0)
                    {
                        swapItem = Content[rightChildIndex];
                    }
                }

                if (item.CompareTo(swapItem) < 0)
                {
                    Swap(item, swapItem);
                }
                else
                {
                    return;
                }
            }
        }

        void SortUp(T item)
        {
            while (true)
            {
                T parent = GetParent(item);
                if (item.CompareTo(parent) > 0)
                {
                    Swap(item, parent);
                }
                else
                {
                    return;
                }
            }
        }

        void Swap(T item1, T item2)
        {
            Content[item1.Index] = item2;
            Content[item2.Index] = item1;
            (item1.Index, item2.Index) = (item2.Index, item1.Index);
        }
    }

    /// <summary>
    /// Represents an item that can be stored in the heap and supports priority-based comparisons.
    /// </summary>
    /// <typeparam name="T">The type implementing this interface.</typeparam>
    public interface IHeapItem<in T> : IComparable<T>
    {
        /// <summary>
        /// Gets or sets the index of the item within the heap.
        /// </summary>
        int Index { get; set; }
    }
}