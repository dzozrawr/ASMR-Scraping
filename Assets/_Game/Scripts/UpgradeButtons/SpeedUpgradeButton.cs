using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class SpeedUpgradeButton : UpgradeButton
{

    private static SpeedUpgradeButton instance = null;
    public static SpeedUpgradeButton Instance { get => instance; }
    private float minSpeed = 0.33f;
    private float maxSpeed = 1.3f;

    private int maxLvl = 10;

    private float speedUpgradeStep;
    public SpatulaScript scraperScript = null;    //specific to this button


    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    // Start is called before the first frame update
    new void Start()
    {

        if (scraperScript == null)
        {
            Debug.LogError("Scraper not assigned to the SpeedUpgradeButton!");
        }

        speedUpgradeStep = (maxSpeed - minSpeed) / ((float)(maxLvl - 1));

        if ((Loader.saveData == null) || (Loader.saveData.speedLvl == 1))
        {
            if ((initialPrices != null) && (initialPrices.Count > 0))
            {
                price = initialPrices[0];
                initialPrices.RemoveAt(0);
            }
            priceText.text = price + "";
        }
        else if (Loader.saveData != null)
        {
            SetLvl(Loader.saveData.speedLvl);
        }





        base.Start();


    }

    protected override void SetLvlSpecial(int lvl)
    {
        //Debug.Log("lvl="+lvl);
        //Debug.Log("maxLvl="+maxLvl);
        if (lvl < maxLvl)
            scraperScript.OnSpeedUpgrade((lvl - 1) * speedUpgradeStep);
           // if(lvl==maxLvl) ReachedMaxLevel();
        else
        {
            scraperScript.OnSpeedUpgrade((maxLvl - 1) * speedUpgradeStep);
            ReachedMaxLevel();
        }
    }

    protected override void SpecialEffect()
    {
        //  scraperScript.scrapeSpeed+=speedUpgradeStep;
        scraperScript.OnSpeedUpgrade(speedUpgradeStep);
        //   scraperScript.scrapeSpeed*=1.2f;//should be a better value than this
        int newPrice = CalculateNewPrice();
        SetPriceAndUpdateUI(newPrice);//there should be a better way of determining the next price

        if (lvl == maxLvl)
        {
            ReachedMaxLevel();
        }
    }

}
