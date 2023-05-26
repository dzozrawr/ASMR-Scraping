using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


[RequireComponent(typeof(Button))]
public abstract class UpgradeButton : MonoBehaviour
{
    public Button button = null;

    public TMP_Text priceText = null;

    public TMP_Text lvlText = null;


    public Image buttonImg = null;
    public Sprite disabledSprite = null;
    public Sprite enabledSprite = null;

    public List<int> initialPrices = null;

    public float nextPriceMul = 1.5f;


    //public
    protected int price = 2;
    protected GameController gameController = null;
    protected bool isInteractable = true;

    protected int lvl = 1;
    protected bool isForceLocked = false;

    protected int initialPricesInd = 0;

    public int Lvl { get => lvl; set => lvl = value; }

    // Start is called before the first frame update
    protected void Start()
    {
        gameController = GameController.Instance;

        button.onClick.AddListener(Effect);



        OnMoneyAmountChanged();
        gameController.MoneyAmountChanged += OnMoneyAmountChanged;

    }

    private void OnDestroy()
    {
        gameController.MoneyAmountChanged -= OnMoneyAmountChanged;
    }

    public void Effect()
    {
        if (isForceLocked) return;
        if (GameController.CoinAmount >= price) //if enough money do the effect
        {
            lvl++;
            lvlText.text = "LVL  " + lvl;
            gameController.AddMoney(-price);
            SpecialEffect();
            OnMoneyAmountChanged();
        }
    }

    public void SetLvl(int lvl)
    {
        int newPrice = price;


        if (lvl <= initialPrices.Count)
        {
            newPrice = initialPrices[lvl - 1];
            initialPrices.RemoveRange(0, lvl);
        }
        else
        {
            newPrice = initialPrices[initialPrices.Count - 1];
            int pow = lvl - initialPrices.Count;
            initialPrices = null;
            newPrice = (int)(((float)newPrice) * Mathf.Pow(nextPriceMul, pow));
        }

        price = newPrice;
        priceText.text = price + "";
        this.lvl = lvl;
        lvlText.text = "LVL  " + lvl;
        SetPriceAndUpdateUI(price);
        OnMoneyAmountChanged();

        SetLvlSpecial(lvl);
    }

    protected abstract void SpecialEffect();

    protected abstract void SetLvlSpecial(int lvl);

    protected void SetPriceAndUpdateUI(int newPrice)
    {
        price = newPrice;
        priceText.text = price + "";
    }

    public void ShowPrice(bool shouldShow)
    {
        priceText.gameObject.SetActive(shouldShow);
    }

    protected void SetButtonInteractable(bool isInter)
    {
        if (isInter && isForceLocked) return;
        //  button.interactable=isInter;
        if (disabledSprite == null)
            button.gameObject.SetActive(isInter);   //this should be color changing or whatever
        else
        {
            if (isInter) buttonImg.sprite = enabledSprite;
            else buttonImg.sprite = disabledSprite;
        }
        isInteractable = isInter;
    }

    protected void ReachedMaxLevel()
    {
        lvlText.text = "MAX";
        ShowPrice(false);//hides the price
        SetButtonInteractable(false);
        isForceLocked = true;   //this variable shuts down the button interactions completely
    }

    protected void OnMoneyAmountChanged()
    {

        if ((!isInteractable) && (GameController.CoinAmount >= price))
        {
            if (isForceLocked) return;
            SetButtonInteractable(true);
            return;
        }

        if ((isInteractable) && (GameController.CoinAmount < price))
        {
            SetButtonInteractable(false);
            return;
        }
    }

    protected int CalculateNewPrice()
    {
        int newPrice;
        if ((initialPrices != null) && (initialPrices.Count > 0))
        {
            newPrice = initialPrices[0];
            initialPrices.RemoveAt(0);
        }
        else
        {
            newPrice = (int)(((float)price) * nextPriceMul);
        }

        return newPrice;
    }
}
