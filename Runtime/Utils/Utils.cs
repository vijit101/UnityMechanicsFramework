using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameplayMechanicsUMFOSS.Core
{
    public static class Utils
    {
        public static void ReloadLvl()
        {
            int indx = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(indx);
        }
        public static void LoadAnyLvl(int lvl)
        {
            SceneManager.LoadScene(lvl);
        }
    }


}

