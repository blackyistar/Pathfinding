using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InterfaceManager : MonoBehaviour
{
    PathFinder pathfinder;
    public Slider delaySlider;
    public Text inputKeys;
    private bool dropDownsOccupied;

    [Header("Grids")]
    [HideInInspector] public List<GridController> gridsByPos;

    [Header("Dropdowns")]
    public Dropdown dropdown1;
    public Dropdown dropdown2;
    public Dropdown dropdown3;
    public Dropdown dropdown4;

    [Header("Names")]
    public List<Text> names1;
    public List<Text> names2;
    public List<Text> names3;
    public List<Text> names4;

    [Header("Distance of the final path")]
    public Text distance1;
    public Text distance2;
    public Text distance3;
    public Text distance4;

    [Header("Number of tiles searched")]
    public Text searched1;
    public Text searched2;
    public Text searched3;
    public Text searched4;

    [Header("Finish order of the algorithms")]
    public Text place1;
    public Text place2;
    public Text place3;
    public Text place4;

    public bool DropDownsOccupied { get => dropDownsOccupied; set => dropDownsOccupied = value; }

    void Awake()
    {
        pathfinder = GameObject.FindWithTag("Controller").GetComponent<PathFinder>();

        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("1"))
        {
            names1.Add(obj.GetComponent<Text>());
        }

        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("2"))
        {
            names2.Add(obj.GetComponent<Text>());
        }

        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("3"))
        {
            names3.Add(obj.GetComponent<Text>());
        }

        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("4"))
        {
            names4.Add(obj.GetComponent<Text>());
        }
    }



    // Update is called once per frame
    void Update()
    {
        pathfinder.delay = delaySlider.value;


        if (pathfinder.Occupied == false)
        {
            dropdown1.interactable = true;
            dropdown2.interactable = true;
            dropdown3.interactable = true;
            dropdown4.interactable = true;
        }
        else
        {
            dropdown1.interactable = false;
            dropdown2.interactable = false;
            dropdown3.interactable = false;
            dropdown4.interactable = false;
        }

        if (pathfinder.Occupied)
        {
            inputKeys.text = "Cancel current pathfinding - RETURN";
        }
        else if (pathfinder.selectedNodeCount == 1)
        {
            inputKeys.text = "Cancel current start node - RETURN";
        }
        else if (pathfinder.selectedNodeCount == 2)
        {
            inputKeys.text = "Clear completed paths - RETURN";
        }
        else
        {
            inputKeys.text = "Return to start menu - ESCAPE";
        }
    }


    public void UpdateNames() //Updates the names of the algorithms in the leaderboard when one of the selected algorithms is changed using a dropdown
    {
        gridsByPos[0].currentAlgorithm = dropdown1.options[dropdown1.value].text;

        gridsByPos[1].currentAlgorithm = dropdown2.options[dropdown2.value].text;

        gridsByPos[2].currentAlgorithm = dropdown3.options[dropdown3.value].text;

        gridsByPos[3].currentAlgorithm = dropdown4.options[dropdown4.value].text;

        foreach (Text name in names1)
        {
            name.text = pathfinder.grids[0].currentAlgorithm;
        }

        foreach (Text name in names2)
        {
            name.text = pathfinder.grids[1].currentAlgorithm;
        }

        foreach (Text name in names3)
        {
            name.text = pathfinder.grids[2].currentAlgorithm;
        }

        foreach (Text name in names4)
        {
            name.text = pathfinder.grids[3].currentAlgorithm;
        }
    }

    public void SetResults() //Displays the result of a pathfininding on the leader board when an algorithm finds a path
    {
        UpdateNames();

        //Distance
        Debug.Log("");
        distance1.text = pathfinder.grids[0].distance.ToString();
        distance2.text = pathfinder.grids[1].distance.ToString();
        distance3.text = pathfinder.grids[2].distance.ToString();
        distance4.text = pathfinder.grids[3].distance.ToString();

        //Searched
        searched1.text = pathfinder.grids[0].searched.ToString();
        searched2.text = pathfinder.grids[1].searched.ToString();
        searched3.text = pathfinder.grids[2].searched.ToString();
        searched4.text = pathfinder.grids[3].searched.ToString();

        //Place
        if (pathfinder.searchtimes[pathfinder.grids[0]] == int.MaxValue)
            place1.text = "0";
        else
            place1.text = "1";

        if (pathfinder.searchtimes[pathfinder.grids[1]] == int.MaxValue)
            place2.text = "0";
        else
            place2.text = "2";

        if (pathfinder.searchtimes[pathfinder.grids[2]] == int.MaxValue)
            place3.text = "0";
        else
            place3.text = "3";

        if (pathfinder.searchtimes[pathfinder.grids[3]] == int.MaxValue)
            place4.text = "0";
        else
            place4.text = "4";
    }
    
}
