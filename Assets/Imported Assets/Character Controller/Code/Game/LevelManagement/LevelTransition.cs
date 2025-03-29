using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

//--------------------------------------------------------------------
//Class used on triggers which signify the end of a level. The InSceneLevelSwitcher will move the player to the next level
//--------------------------------------------------------------------
public class LevelTransition : MonoBehaviour {

    [SerializeField] int m_Index = 0;

   public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene(m_Index);
        }
    }
}
