using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public Image mapImage;
    public List<Sprite> maps = new List<Sprite>();
    public List<string> scenes = new List<string>();
    public int currentMap;

    [Header("Buttons")]
    public Button mapButton;
    public Button backButton;
    public Text startText;

    [Header("Animators")]
    public Animator title;
    public Animator background;
    
    // Start is called before the first frame update
    void Start()
    {
        mapButton.enabled = true;
        backButton.enabled = false;
    }

    public void LoadScene() //Loads the selected scene
    {
        SceneManager.LoadSceneAsync(scenes[currentMap]);
    }

    public void SetStart() //Triggers the animation that hides the scene selection
    {
        mapButton.enabled = true;
        backButton.enabled = false;

        background.SetTrigger("Close");
    }

    public void SetMapSelection() //Triggers the animation which displays the scene selection
    {
        mapButton.enabled = false;
        backButton.enabled = true;

        background.SetTrigger("Open");
    }
    
    public void NextMap() //Toggles between the large scene and default scene
    {
        currentMap += 1;

        if (currentMap == maps.Count)
        {
            currentMap = 0;
        }

        startText.text = "Load " + scenes[currentMap] + " Scene";

        mapImage.sprite = maps[currentMap];
    }
}
