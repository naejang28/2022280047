using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public string sceneName;   // 넘어갈 씬 이름

    public void Load()
    {
        SceneManager.LoadScene(sceneName);
    }
}
