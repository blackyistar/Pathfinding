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
    public Button gameButton;
    public Button standardButton;
    public Text startText;

    [Header("Animators")]
    public Animator title;
    public Animator background;
    
    // Start is called before the first frame update
    void Start()
    {
        Open();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            title.SetTrigger("Exit");
            background.SetTrigger("Exit");
        }
    }

    void Open()
    {
        gameButton.enabled = true;
        standardButton.enabled = false;
    }

    public void LoadScene()
    {
        SceneManager.LoadSceneAsync(scenes[currentMap]);
    }

    public void SetGameMode()
    {
        gameButton.enabled = false;
        standardButton.enabled = true;

        background.SetTrigger("Open");
    }

    public void SetStandardMode()
    {
        gameButton.enabled = true;
        standardButton.enabled = false;

        background.SetTrigger("Close");
    }
    
    public void NextMap()
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
