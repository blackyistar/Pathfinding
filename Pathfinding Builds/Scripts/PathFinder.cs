using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PathFinder : MonoBehaviour
{
    private bool occupied = false;
    public float delay = 0.1f;

    [Header("Scripts")]
    public InterfaceManager interfaceManager;
    public List<GridController> grids;
    public Dictionary<GridController, float> searchtimes = new Dictionary<GridController, float>();

    public bool Occupied { get => occupied; set => occupied = value; }

    private void Awake()
    {
        int num = Random.Range(0, grids[0].possibleMaps.Count);

        foreach (GridController grid in grids)
        {
            grid.possibleMaps[num].SetActive(true);
            grid.Begin();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        foreach (GridController gridController in grids)
        {
            searchtimes.Add(gridController, int.MaxValue);
            gridController.PathFinder = this;

            interfaceManager.gridsByPos.Add(gridController);
        }

        interfaceManager.SetResults();

        /*
         * 
         * This program contains all the pathfinding algorithms
         * If occupied, the algorithms are running
         * If not occupied, the algorithms are not running
         * 
         */
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Return))
        {
            interfaceManager.delaySlider.value = 0;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadSceneAsync("MENU");
        }

        if (Occupied == false && GridsOccupied() == false && interfaceManager.DropDownsOccupied == false)
        {
            if (Input.GetKey(KeyCode.Mouse0))
            {
                bool target = SetTarget();

                if (target)
                {
                    Occupied = true;
                    ClearAllGridResults();

                    foreach (GridController gridController in grids)
                    {
                        StartCoroutine(gridController.currentAlgorithm, gridController);
                    }
                }
            }
        }
    }

    public void ClearAllGridResults()
    {
        foreach (GridController grid in grids)
        {
            grid.ClearResults();
        }
    }


    public bool GridsOccupied()
    {
        bool gridsOccupied = false;

        foreach (GridController gridController in grids)
        {
            if (gridController.Occupied)
                gridsOccupied = true;
        }

        return gridsOccupied;
    }

    public void BubbleSortSpeeds(ref List<GridController> list)
    {
        bool finished = false;

        for (int max = list.Count - 1; max > 0 && !finished; max--)
        {
            bool swapped = false;

            for (int i = 0; i < max; i++)
            {
                GridController left = list[i];
                GridController right = list[i + 1];

                if (searchtimes[left] > searchtimes[right])
                {
                    list[i] = right;
                    list[i + 1] = left;
                    swapped = true;
                }
            }

            if (swapped == false)
            {
                finished = true;
            }
        }
    }


    public bool SetTarget()
    {
        RaycastHit hit;

        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100))
        {
            for (int i = 0; i < grids.Count; i++)
            {
                if (grids[i].GridPos(hit.point) != null && grids[i].GridPos(hit.point) != grids[i].GridPos(grids[i].player.transform.position))
                {
                    grids[i].target = grids[i].GridPos(hit.point);

                    if (grids[i].target.Walkable)
                    {
                        grids[(i + 1) % grids.Count].target = SetSecondaryTarget(hit.point, grids[i], grids[(i + 1) % grids.Count]);
                        grids[(i + 2) % grids.Count].target = SetSecondaryTarget(hit.point, grids[i], grids[(i + 2) % grids.Count]);
                        grids[(i + 3) % grids.Count].target = SetSecondaryTarget(hit.point, grids[i], grids[(i + 3) % grids.Count]);

                        return true;
                    }
                }
            }
        }

        return false;
    }

    public Node SetSecondaryTarget(Vector3 primaryTarget, GridController primaryGrid, GridController secondaryGrid)
    {
        Node secondaryTarget = secondaryGrid.GridPos(new Vector3(primaryTarget.x + (secondaryGrid.transform.position.x - primaryGrid.transform.position.x),
        primaryTarget.y + (secondaryGrid.transform.position.y - primaryGrid.transform.position.y),
        primaryTarget.z + (secondaryGrid.transform.position.z - primaryGrid.transform.position.z)));

        return secondaryTarget;
    }


    void CreatePath(Node startnode, List<Node> closed, GridController grid, float timer)
    {
        List<Node> Path = new List<Node>();

        Node currentnode = grid.target;
        Path.Add(currentnode);

        while (currentnode != startnode)
        {
            if (grid.currentAlgorithm == "AStar" || grid.currentAlgorithm == "BreadthFirst" || grid.currentAlgorithm == "DepthFirst")
            {
                Path.Add(currentnode.parent);
                currentnode = currentnode.parent;
            }
            else if (grid.currentAlgorithm == "Dijkstras")
            {
                Path.Add(currentnode.previous);
                currentnode = currentnode.previous;
            }
        }

        Path.Reverse();

        grid.path = Path;
        grid.distance = Path.Count;
        grid.searched = closed.Count;
        searchtimes[grid] = timer;

        int noOfFinished = 0;

        foreach (GridController gridController in grids)
        {
            if (searchtimes[gridController] != int.MaxValue)
            {
                noOfFinished += 1;
            }
        }

        BubbleSortSpeeds(ref grids);

        if (noOfFinished == grids.Count)
        {
            Occupied = false;
        }

        interfaceManager.SetResults();
        grid.UpdateGrid(null);
    }

    public void PushStack(ref List<Node> stack, ref int pointer, Node item)
    {
        stack.Add(item);
        pointer += 1;
    }

    public Node PopStack(ref List<Node> stack, ref int pointer)
    {
        Node poppedItem;

        if (pointer >= 0)
        {
            poppedItem = stack[pointer];
            stack.Remove(poppedItem);
            pointer -= 1;

            return poppedItem;
        }

        return null;
    }

    public IEnumerator DepthFirst(GridController grid)
    {
        float timer = 0;

        Node startnode = grid.GridPos(grid.player.transform.position);

        if (startnode == null)
            yield break;

        grid.path.Add(grid.target);

        List<Node> visited = new List<Node>();
        List<Node> stack = new List<Node>();
        int stackpointer = -1;

        PushStack(ref stack, ref stackpointer, startnode);

        while (stack.Count != 0)
        {
            timer += Time.deltaTime;

            Node current = PopStack(ref stack, ref stackpointer);

            visited.Add(current);

            grid.UpdateGrid(visited);

            yield return new WaitForSeconds(delay);

            if (current == grid.target)
            {
                CreatePath(startnode, visited, grid, timer);

                yield break;
            }

            List<Node> neighbours = grid.GetNeighbours(current, 10);

            Node top = null, right = null, down = null, left = null;

            foreach (Node node in neighbours)
            {

                if (node.Pos.x == current.Pos.x - 1)
                    top = node;
                else if (node.Pos.z == current.Pos.z + 1)
                    right = node;
                else if (node.Pos.x == current.Pos.x + 1)
                    down = node;
                else if (node.Pos.z == current.Pos.z - 1)
                    left = node;

            }

            List<Node> ordered = new List<Node> { top, right, down, left };

            foreach (Node node in ordered)
            {
                if (node != null && node.Walkable && !visited.Contains(node))
                {
                    int distance = current.distance + grid.CalculateDistance(current, node);

                    if (distance < node.distance || !stack.Contains(node))
                    {
                        node.distance = distance;
                        node.parent = current;

                        if (!stack.Contains(node))
                            PushStack(ref stack, ref stackpointer, node);
                    }
                }
            }
        }
    }

    public void Enqueue(ref List<Node> queue, ref int head, Node item)
    {
        queue.Add(item);
        head += 1;
    }

    public Node Dequeue(ref List<Node> queue, ref int head, ref int tail)
    {
        Node item;

        if (head >= tail)
        {
            item = queue[tail];
            queue.Remove(item);
            head -= 1;

            return item;
        }

        return null;
    }

    public IEnumerator BreadthFirst(GridController grid)
    {
        float timer = 0;

        Node startnode = grid.GridPos(grid.player.transform.position);

        if (startnode == null)
            yield break;

        grid.path.Add(grid.target);

        List<Node> visited = new List<Node>();
        List<Node> queue = new List<Node>();
        int head = -1;
        int tail = 0;

        Enqueue(ref queue, ref head, startnode);

        while (queue.Count != 0)
        {
            timer += Time.deltaTime;

            Node current = Dequeue(ref queue, ref head, ref tail);
            visited.Add(current);

            grid.UpdateGrid(visited);

            yield return new WaitForSeconds(delay);

            if (current == grid.target)
            {
                CreatePath(startnode, visited, grid, timer);

                yield break;
            }

            List<Node> neighbours = grid.GetNeighbours(current, 10);

            foreach (Node neighbour in neighbours)
            {
                if (neighbour.Walkable && !visited.Contains(neighbour))
                {
                    int distance = current.distance + grid.CalculateDistance(current, neighbour);

                    if (distance < neighbour.distance || !queue.Contains(neighbour))
                    {
                        neighbour.distance = distance;
                        neighbour.parent = current;

                        if (!queue.Contains(neighbour))
                            Enqueue(ref queue, ref head, neighbour);
                    }
                }
            }

        }
    }

    public IEnumerator Dijkstras(GridController grid)
    {
        float timer = 0;

        Node startnode = grid.GridPos(grid.player.transform.position);

        if (startnode == null)
            yield break;

        grid.path.Add(grid.target);

        List<Node> nodes = new List<Node>();
        List<Node> visited = new List<Node>();

        foreach (Node node in grid.GetGrid())
        {
            if (node == startnode)
            {
                node.distance = 0;
            }
            else
            {
                node.distance = int.MaxValue;
            }

            nodes.Add(node);
        }

        while (nodes.Count != 0)
        {
            timer += Time.deltaTime;

            Node current = nodes[0];

            foreach (Node node in nodes)
            {
                if (node.distance < current.distance)
                {
                    current = node;
                }
            }

            nodes.Remove(current);
            visited.Add(current);

            grid.UpdateGrid(visited);

            yield return new WaitForSeconds(delay);

            if (current == grid.target)
            {
                CreatePath(startnode, visited, grid, timer);
                yield break;
            }

            if (current.distance == int.MaxValue)
            {
                yield break;
            }

            List<Node> neighbours = grid.GetNeighbours(current, 10);

            foreach (Node neighbour in neighbours)
            {
                if (neighbour.Walkable)
                {
                    int distance = current.distance + grid.CalculateDistance(current, neighbour);

                    if (distance < neighbour.distance)
                    {
                        neighbour.distance = distance;
                        neighbour.previous = current;
                    }
                }
            }
        }
    }

    public IEnumerator AStar(GridController grid)
    {
        float timer = 0;

        Node startnode = grid.GridPos(grid.player.transform.position);

        if (startnode == null)
            yield break;

        grid.path.Add(grid.target);

        Heap<Node> open = new Heap<Node>(50000);
        List<Node> closed = new List<Node>();

        open.Add(startnode);

        while (open.Count > 0)
        {
            timer += Time.deltaTime;

            Node current = open.RemoveRootNode();
            closed.Add(current);

            grid.UpdateGrid(closed);

            yield return new WaitForSeconds(delay);

            if (current == grid.target)
            {
                CreatePath(startnode, closed, grid, timer);
                yield break;
            }

            List<Node> neighbours = grid.GetNeighbours(current, 10);

            foreach (Node neighbour in neighbours)
            {
                if (neighbour.Walkable && !closed.Contains(neighbour))
                {
                    int gcost = current.gcost + grid.CalculateDistance(current, neighbour);

                    if (gcost < neighbour.gcost || !open.Contains(neighbour))
                    {
                        neighbour.gcost = gcost;

                        int hcost = grid.CalculateDistance(neighbour, grid.target);
                        int fcost = hcost + gcost;

                        neighbour.hcost = hcost;
                        neighbour.fcost = fcost;
                        neighbour.parent = current;

                        if (!open.Contains(neighbour))
                            open.Add(neighbour);
                    }
                }
            }
        }
    }
}
