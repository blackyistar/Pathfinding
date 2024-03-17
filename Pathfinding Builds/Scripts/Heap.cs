using UnityEngine;
using System.Collections;

public class Heap<I> where I : Node
{
	I[] items;
	int count;

	public int Count { get => count; set => count = value; }

	public Heap(int heapSize)
	{
		items = new I[heapSize];
	}

	public void Add(I item)
	{
		items[count] = item;
		item.HeapIndex = count;
		SortUp(item);

		count++;
	}

	public I RemoveRootNode()
	{
		I root = items[0];
		count--;

		items[0] = items[count];
		items[0].HeapIndex = 0;
		SortDown(items[0]);

		return root;
	}

	public void UpdateItem(I item)
	{
		SortUp(item);
	}

    public bool Contains(I item)
	{
		if (items[item.HeapIndex] == item)
			return true;
		else
			return false;
	}

	void SortDown(I item)
	{
		bool finished = false;

		while (finished == false)
		{
			int left = item.HeapIndex * 2 + 1;
			int right = item.HeapIndex * 2 + 2;
			int swapIndex = 0;

			if (left < count)
			{
				swapIndex = left;

				if (right < count)
				{
					if (items[left].CompareTo(items[right]) > 0)
					{
						swapIndex = right;
					}
				}

				if (item.CompareTo(items[swapIndex]) > 0)
				{
					Swap(item, items[swapIndex]);
				}
				else
				{
					finished = true;
				}

			}
			else
			{
				finished = true;
			}

		}
	}

	void SortUp(I item)
	{
		int parentIndex = (item.HeapIndex - 1) / 2;

		bool finished = false;

		while (finished == false)
		{
			parentIndex = (item.HeapIndex - 1) / 2;

			I parent = items[parentIndex];
			if (item.CompareTo(parent) < 0)
			{
				Swap(item, parent);
			}
			else
			{
				finished = true;
			}
		}
	}

	void Swap(I Node1, I Node2)
	{
		int index1 = Node1.HeapIndex;
		int index2 = Node2.HeapIndex;

		items[index1] = Node2;
		items[index2] = Node1;

		Node2.HeapIndex = index1;
		Node1.HeapIndex = index2;
	}
}