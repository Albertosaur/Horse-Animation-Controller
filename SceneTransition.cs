using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour {

    public int index;
    
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        SceneManager.LoadScene(index);

        if(Input.GetKeyDown(KeyCode.Return))
        {
            Application.Quit();
        }
    }
}
