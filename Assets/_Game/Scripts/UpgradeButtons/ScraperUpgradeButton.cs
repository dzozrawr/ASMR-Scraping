using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScraperUpgradeButton : UpgradeButton
{
    private static ScraperUpgradeButton instance = null;
    public static ScraperUpgradeButton Instance { get => instance; }
    public SpatulaScript scraperScript = null;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    // private Vector3 prevScraperScale;
    new void Start()
    {
        if ((Loader.saveData == null) || (Loader.saveData.scraperLvl == 1))
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
            SetLvl(Loader.saveData.scraperLvl);
        }

        base.Start();

        if (scraperScript == null)
        {
            Debug.LogError("Scraper not assigned to the SpeedUpgradeButton!");
        }
    }

    protected override void SetLvlSpecial(int lvl)
    {
        if(!scraperScript.SetScraperUpgradeToLvl(lvl)){
            ReachedMaxLevel();
        }
    }
    protected override void SpecialEffect()
    {
        //  prevScraperScale=scraperScript.transform.localScale;
        //  scraperScript.transform.localScale=new Vector3(prevScraperScale.x*1.2f,prevScraperScale.y,prevScraperScale.z); //should be a smarter value than this

        // scraperScript.SpatulaWidth*=1.2f;
        if (scraperScript.OnScraperUpgrade())
        {
            int newPrice = CalculateNewPrice();
            SetPriceAndUpdateUI(newPrice);//there should be a better way of determining the next price
                                          // SetPriceAndUpdateUI(price * 2);//there should be a better way of determining the next price
        }
        else
        {
            ReachedMaxLevel();
        }


        //also change the model in the future of the scraper, when it is leveled up enough i guess

    }
}
