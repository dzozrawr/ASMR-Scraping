using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Loader : MonoBehaviour
{
    public static SaveData saveData=null;
    private int levelToLoad = -1;
    private void Awake()
    {
        //TTPCore.Setup();

        saveData = SaveSystem.LoadGameXML();
        if (saveData != null)
        {
            levelToLoad = saveData.level;
            GameController.CoinAmount = saveData.money;
            //Loa.saveData=saveData;
            // ShopController.shopItemInfos=saveData.shopItemInfos;

            /*  WindmillScript.Lvl=saveData.windmillLevel;
             MarketScript.Lvl=saveData.marketLevel; */
        }
        else
        {
            levelToLoad = SceneManager.GetActiveScene().buildIndex + 1;
            // GameController.missionID = 1;
        }

        SceneManager.LoadScene(levelToLoad);
    }
}
