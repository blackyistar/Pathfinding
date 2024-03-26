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

	public void Add(I item) //Adds a node to the heap
	{
		items[count] = item;
		item.HeapIndex = count;
		SortUp(item);

		count++;
	}

	public I RemoveRootNode() //Returns the node with the highest f cost, the root node, and removes it from the heap
	{
		I root = items[0];
		count--;

		items[0] = items[count];
		items[0].HeapIndex = 0;
		SortDown(items[0]);

		return root;
	}

	public void UpdateItem(I item) //Updates the f cost of a node in the heap
	{
		SortUp(item);
	}

    public bool Contains(I item) //Checks whether a node is in the heap, returns true if so
	{
		if (items[item.HeapIndex] == item)
			return true;
		else
			return false;
	}

	void SortDown(I item) //Traverses down the heap, sorting the nodes
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
				SortDown(item);
			}
			else
			{
				Debug.Log("Sort down complete");
			}

		}
		else
		{
			Debug.Log("Sort down complete");
		}
	}

	void SortUp(I item) //Traverses up the heap, sorting the nodes
	{
		int parentIndex = (item.HeapIndex - 1) / 2;
		I parent = items[parentIndex];

		if (item.CompareTo(parent) < 0)
		{
			Swap(item, parent);
			SortUp(item);
		}
		else
		{
			Debug.Log("Sort up complete");
		}
	}

	void Swap(I Node1, I Node2) //Swaps two nodes in the heap
	{
		int index1 = Node1.HeapIndex;
		int index2 = Node2.HeapIndex;

		items[index1] = Node2;
		items[index2] = Node1;

		Node2.HeapIndex = index1;
		Node1.HeapIndex = index2;
	}
}