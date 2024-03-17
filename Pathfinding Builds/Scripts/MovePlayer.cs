using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePlayer : MonoBehaviour
{
    private Camera camera;
    GridController gridController;

    // Start is called before the first frame update
    void Start()
    {
        camera = GetComponentInChildren<Camera>();
        gridController = GetComponentInParent<GridController>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public IEnumerator MoveAlongPath() //Recursion with coroutines
    {
        if (gridController.path.Count == 0) //Base case
        {
            Debug.Log("A completion");

            gridController.ClearResults();

            gridController.Occupied = false;
        }
        else //General case
        {
            transform.position = new Vector3(gridController.path[0].Pos.x, transform.position.y, gridController.path[0].Pos.z);
            gridController.UpdateGrid(null);
            gridController.path = gridController.path.GetRange(1, gridController.path.Count - 1);

            yield return new WaitForSecondsRealtime(0.2f);

            StartCoroutine("MoveAlongPath");
        }
    }
}
