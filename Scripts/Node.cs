using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node //Composition
{
    private bool walkable;
    private Vector3 pos;
    private MeshRenderer renderer;

    public bool Walkable { get => walkable; }
    public Vector3 Pos { get => pos; }
    public MeshRenderer Renderer { get => renderer; }

    [Header("A Star")]
    public Node parent;
    public int gcost = 0;
    public int hcost = 0;
    public int fcost = 0;
    private int heapIndex;
    public int HeapIndex { get => heapIndex; set => heapIndex = value; }

    [Header("Dijkstra's")]
    public Node previous;
    public int distance = 0;

    public Node (bool walkable, Vector3 pos, MeshRenderer renderer)
    {
        this.walkable = walkable;
        this.pos = pos;
        this.renderer = renderer;
    }

    public int CompareTo(Node nodeToCompare) //Returns 1 if the current instance has a greater cost than the nodeToCompare instance, -1 if it has a smaller cost than nodeToCompare and 0 if they both have the same cost
    {
        int compare = fcost.CompareTo(nodeToCompare.fcost);

        if (compare == 0)
        {
            compare = hcost.CompareTo(nodeToCompare.hcost);
        }

        return compare;
    }
}
