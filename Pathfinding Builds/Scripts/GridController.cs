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
    public MovePlayer player;
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
    [HideInInspector] public Color playerColour;
    [HideInInspector] public Color pathColour;

    public PathFinder PathFinder { get => pathFinder; set => pathFinder = value; }
    public bool Occupied { get => occupied; set => occupied = value; }

    public void Begin()
    {
        Vector3 gridPosition = transform.position;

        if (SceneManager.GetActiveScene().name == "Default")
        {
            gridinstance = new DefaultGrid(gridPosition);
            gridinstance.CreateGrid(transform, nodeMaterial, unwalkableLayer, walkableColour, unwalkableColour);
        }
        else
        {
            gridinstance = new VegetationGrid(gridPosition);
            gridinstance.CreateGrid(transform, nodeMaterial, unwalkableLayer, walkableColour, unwalkableColour);
        }

        playerColour = player.GetComponent<MeshRenderer>().material.color;
        pathColour = playerColour;
    }

    public Node GridPos(Vector3 Pos)
    {
        return gridinstance.GridPos(Pos);
    }

    public void UpdateGrid(List<Node> searched)
    {
        gridinstance.UpdateGrid(path, searched, target, pathColour, searchedColour, walkableColour, unwalkableColour);
    }

    public int CalculateDistance(Node node1, Node node2)
    {
        int distance = Mathf.RoundToInt(Mathf.Sqrt(Mathf.Pow(node1.Pos.x - node2.Pos.x, 2) + Mathf.Pow(node1.Pos.z - node2.Pos.z, 2)) * 10);
        return distance;
    }

    public List<Node> GetNeighbours(Node node, int maxdistance)
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
    public Node[,] GetGrid()
    {
        return gridinstance.GridProperty;
    }

    private void Update()
    {
        if (pathFinder.Occupied == false && occupied == false)
        {
            if (path.Count != 0 && path.Count == distance) //Final path has been received
            {
                if (Input.GetKey(KeyCode.Return))
                {
                    occupied = true;
                    player.StartCoroutine("MoveAlongPath");
                }
            }
        }
    }

    public void ClearResults()
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

    public Grid(Vector3 gridPosition, int gridwidth, int gridheight, float nodeDiameter)
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

    public Node GridPos(Vector3 Pos)
    {
        int posx = Mathf.RoundToInt(Pos.x - gridCorner.x);
        int posy = Mathf.RoundToInt(Pos.z - gridCorner.z);

        if (posx <= gridsizex && posx >= 0 && posy <= gridsizey && posy >= 0)
            return grid[posx, posy];
        else
            return null;
    }

    public abstract void CreateGrid(Transform parent, Material nodeMaterial, LayerMask unwalkableLayer, Color walkableColour, Color unwalkableColour);

    public void UpdateGrid(List<Node> path, List<Node> searched, Node target, Color pathColour, Color searchedColour, Color walkableColour, Color unwalkableColour)
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
}

class DefaultGrid : Grid //Inheritance
{
    public DefaultGrid(Vector3 gridPosition) : base(gridPosition, 20, 20, 1)
    {

    }

    public override void CreateGrid(Transform parent, Material nodeMaterial, LayerMask unwalkableLayer, Color walkableColour, Color unwalkableColour)
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

class VegetationGrid : Grid //Inheritance
{
    public VegetationGrid(Vector3 gridPosition) : base(gridPosition, 44, 44, 1f)
    {

    }

    public override void CreateGrid(Transform parent, Material nodeMaterial, LayerMask unwalkableLayer, Color walkableColour, Color unwalkableColour)
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
