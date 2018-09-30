/*
* Created by Daniel Mak
*/

using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour {
    public void Load(int sceneIDToLoad) {
        SceneManager.LoadScene(sceneIDToLoad);
    }
}