using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;
using System.Runtime.Serialization;
using UnityEngine.SceneManagement;

[DataContract]
public class SaveData
{
    [DataMember]
    public int level;
    [DataMember]
    public int money;

    [DataMember]
    public int speedLvl;

    [DataMember]
    public int incomeLvl;

    [DataMember]
    public int scraperLvl;


    public SaveData(int _level, int spLvl, int incLvl, int scraLvl)
    {
        level = _level;
        money = GameController.CoinAmount;   //implicit saving of the coin amount for simplicity of the constructor

        speedLvl=spLvl;
        incomeLvl=incLvl;
        scraperLvl=scraLvl;
    }

    public SaveData()
    {
        level = SceneManager.GetActiveScene().buildIndex;
        money = GameController.CoinAmount;   //implicit saving of the coin amount for simplicity of the constructor

        speedLvl=SpeedUpgradeButton.Instance.Lvl;
        incomeLvl=StripeUpgradeButton.Instance.Lvl;
        scraperLvl=ScraperUpgradeButton.Instance.Lvl;
    }

}
