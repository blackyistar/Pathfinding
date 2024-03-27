using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GridController : MonoBehaviour
{
    private bool occupied = false;

    private Grid gridinstance;
    private PathFinder pathFinder;
    public Material nodeMaterial;

    public string currentAlgorithm; //The algorithm currently being run on this grid

    [Header("References")]
    public InterfaceManager interfaceManager;
    public Node start;
    public Node target;

    [Header("Results")]
    public int distance; //Number of nodes in the path
    public int searched; //Number of nodes searched
    public List<Node> path = new List<Node>();

    [Header("Design")]
    public List<GameObject> possibleMaps;
    public LayerMask unwalkableLayer;
    public Color searchedColour;
    public Color walkableColour;
    public Color unwalkableColour;
    public Color pathColour;

    public PathFinder PathFinder { get => pathFinder; set => pathFinder = value; }
    public bool Occupied { get => occupied; set => occupied = value; }

 /*
 * 
 * This class, GridController, is the only class with access to the grid instance
 * 
 */

    public void CreateGrid() //Instantiates the grid and calls for the CreateGrid function of the Grid instance
    {
        Vector3 gridPosition = transform.position;

        if (SceneManager.GetActiveScene().name == "Default")
        {
            gridinstance = new DefaultGrid(gridPosition);
            gridinstance.CreateGrid(transform, nodeMaterial, unwalkableLayer, walkableColour, unwalkableColour);
        }
        else
        {
            gridinstance = new LargeGrid(gridPosition);
            gridinstance.CreateGrid(transform, nodeMaterial, unwalkableLayer, walkableColour, unwalkableColour);
        }
    }


    public Node GridPos(Vector3 Pos)   //Calls for the GridPose function of the Grid instance
    {
        return gridinstance.GridPos(Pos);
    }

    public void UpdateGrid(List<Node> searched) //Calls for the UpdateGrid function of the Grid instance
    {
        if (path.Count == 0)
        {
            gridinstance.UpdateGrid(searched, start, target, pathColour, searchedColour, walkableColour, unwalkableColour);

        }
        else
        {
            gridinstance.UpdateGrid(path, searched, pathColour, searchedColour, walkableColour, unwalkableColour);
        }
    }


    public int CalculateDistance(Node node1, Node node2) //Uses Pythagoras' theorem to calculate the distance between the position vectors of two nodes
    {
        int distance = Mathf.RoundToInt(Mathf.Sqrt(Mathf.Pow(node1.Pos.x - node2.Pos.x, 2) + Mathf.Pow(node1.Pos.z - node2.Pos.z, 2)) * 10);
        return distance;
    }

    public List<Node> GetNeighbours(Node node, int maxdistance) //Returns a list of all nodes adjacent to a node, its neighbours
    {
        List<Node> neighbours = new List<Node>();

        foreach (Node element in GetGrid())
        {
            if (CalculateDistance(node, element) <= maxdistance)
            {
                neighbours.Add(element);
            }
        }

        return neighbours;
    }


    public Node[,] GetGrid() //Returns the grid array, a 2D array of nodes
    {
        return gridinstance.GridProperty;
    }

    public void ClearResults() //Clears the current path on the tilemap, clears the leader board results
    {
        pathFinder.searchtimes[this] = int.MaxValue;
        path.Clear();
        distance = 0;
        searched = 0;
        interfaceManager.SetResults();
        UpdateGrid(null);
    }
}

abstract class Grid
{
    protected Node[,] grid;

    protected int gridwidth, gridheight;
    protected Vector3 gridPosition;
    protected int gridsizex, gridsizey;
    protected Vector3 gridCorner;
    protected float nodeDiameter;

    public Node[,] GridProperty { get => grid; set => grid = value; }

    public Grid(Vector3 gridPosition, int gridwidth, int gridheight, float nodeDiameter) //Constructor which calculates grid dimensions 
    {
        this.gridPosition = gridPosition;
        this.gridwidth = gridwidth;
        this.gridheight = gridheight;
        this.nodeDiameter = nodeDiameter;

        gridsizex = Mathf.RoundToInt(gridwidth / nodeDiameter);
        gridsizey = Mathf.RoundToInt(gridheight / nodeDiameter);
        gridCorner = new Vector3(gridPosition.x - (gridwidth / 2), gridPosition.y, gridPosition.z - (gridheight / 2));

        grid = new Node[gridsizex, gridsizey];
    }

    public Node GridPos(Vector3 Pos) //Returns the node that covers the position of the vector Pos
    {
        int posx = Mathf.RoundToInt(Pos.x - gridCorner.x);
        int posy = Mathf.RoundToInt(Pos.z - gridCorner.z);

        if (posx <= gridsizex && posx >= 0 && posy <= gridsizey && posy >= 0)
            return grid[posx, posy];
        else
            return null;
    }


    public abstract void CreateGrid(Transform parent, Material nodeMaterial, LayerMask unwalkableLayer, Color walkableColour, Color unwalkableColour); //abstract class

    public void UpdateGrid(List<Node> path, List<Node> searched, Color pathColour, Color searchedColour, Color walkableColour, Color unwalkableColour) //Overloaded method: The colours of the tiles on the tile map are updated
    {
        foreach (Node node in grid)
        {
            if (path.Contains(node))
                node.Renderer.material.color = pathColour;
            else if (searched != null && searched.Contains(node))
                node.Renderer.material.color = searchedColour;
            else if (node.Walkable)
                node.Renderer.material.color = walkableColour;
            else
                node.Renderer.material.color = unwalkableColour;
        }
    }

    public void UpdateGrid(List<Node> searched, Node start, Node target, Color pathColour, Color searchedColour, Color walkableColour, Color unwalkableColour) //Overloaded method: The colours of the tiles on the tile map are updated
    {
        foreach (Node node in grid)
        {
            if (node == start || node == target)
                node.Renderer.material.color = pathColour;
            else if (searched != null && searched.Contains(node))
                node.Renderer.material.color = searchedColour;
            else if (node.Walkable)
                node.Renderer.material.color = walkableColour;
            else
                node.Renderer.material.color = unwalkableColour;
        }
    }
}

class DefaultGrid : Grid
{
    public DefaultGrid(Vector3 gridPosition) : base(gridPosition, 20, 20, 1) //Constructor which calculates grid dimensions
    {

    }

    public override void CreateGrid(Transform parent, Material nodeMaterial, LayerMask unwalkableLayer, Color walkableColour, Color unwalkableColour) //Method overriding: Creates the grid of nodes by instantiaing objects of the Node class, and creates the tile map by instatiating cube game objects
    {
        Vector3 currentpos = gridCorner;

        for (int x = 0; x < gridsizex; x++)
        {
            for (int y = 0; y < gridsizey; y++)
            {
                bool unwalkable = Physics.CheckSphere(currentpos, nodeDiameter / 2, unwalkableLayer);

                MeshRenderer renderer = GameObject.CreatePrimitive(PrimitiveType.Cube).GetComponent<MeshRenderer>();
                renderer.transform.localScale = new Vector3(1, 0.1f, 1);
                renderer.transform.position = currentpos;
                renderer.transform.parent = parent;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

                if (unwalkable)
                    renderer.material.color = unwalkableColour;
                else
                    renderer.material.color = walkableColour;

                grid[x, y] = new Node(!unwalkable, currentpos, renderer);

                currentpos.z = currentpos.z + nodeDiameter;
            }

            currentpos.z = gridPosition.z - (gridheight / 2);
            currentpos.x = currentpos.x + nodeDiameter;
        }
    }
}

class LargeGrid : Grid
{
    public LargeGrid(Vector3 gridPosition) : base(gridPosition, 44, 44, 1f) //Constructor which calculates grid dimensions
    {

    }

    public override void CreateGrid(Transform parent, Material nodeMaterial, LayerMask unwalkableLayer, Color walkableColour, Color unwalkableColour) //Method overriding: Creates the grid of nodes by instantiaing objects of the Node class, and creates the tile map by instatiating cube game objects
    {
        Vector3 currentpos = gridCorner;

        for (int x = 0; x < gridsizex; x++)
        {
            for (int y = 0; y < gridsizey; y++)
            {
                bool unwalkable = Physics.CheckSphere(currentpos, nodeDiameter / 2, unwalkableLayer);

                MeshRenderer renderer = GameObject.CreatePrimitive(PrimitiveType.Cube).GetComponent<MeshRenderer>();
                renderer.transform.localScale = new Vector3(1, 0.1f, 1);
                renderer.transform.position = currentpos;
                renderer.transform.parent = parent;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

                renderer.material = nodeMaterial;

                if (unwalkable)
                    renderer.material.color = unwalkableColour;
                else
                    renderer.material.color = walkableColour;

                grid[x, y] = new Node(!unwalkable, currentpos, renderer);

                currentpos.z = currentpos.z + nodeDiameter;
            }

            currentpos.z = gridPosition.z - (gridheight / 2);
            currentpos.x = currentpos.x + nodeDiameter;
        }
    }
}
