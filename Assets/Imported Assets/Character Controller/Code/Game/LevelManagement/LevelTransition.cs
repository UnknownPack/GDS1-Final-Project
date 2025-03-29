using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

//--------------------------------------------------------------------
//Class used on triggers which signify the end of a level. The InSceneLevelSwitcher will move the player to the next level
//--------------------------------------------------------------------
public class LevelTransition : MonoBehaviour {

    [SerializeField, Tooltip("loads the scene that is named")] string SceneName;

   public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene(SceneName);
        }
    }
}
