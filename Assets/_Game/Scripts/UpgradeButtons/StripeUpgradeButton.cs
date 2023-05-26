using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class StripeUpgradeButton : UpgradeButton
{
    private static StripeUpgradeButton instance = null;
    public static StripeUpgradeButton Instance { get => instance; }

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private new void Start()
    {
        if ((Loader.saveData == null) || (Loader.saveData.incomeLvl == 1))
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
            SetLvl(Loader.saveData.incomeLvl);
        }
        base.Start();

    }
    protected override void SetLvlSpecial(int lvl)
    {
        if(gameController==null) gameController=GameController.Instance;
        gameController.IncomeAddition = lvl - 1;
    }


    protected override void SpecialEffect()
    {
        gameController.IncomeAddition += 1;//should be a smarter value

        int newPrice = CalculateNewPrice();
        SetPriceAndUpdateUI(newPrice);//there should be a better way of determining the next price
                                      // SetPriceAndUpdateUI(price * 2);//there should be a better way of determining the next price
    }


}
