using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameCanvasController : MonoBehaviour
{
    public TMP_Text coinAmountTxt = null;

    // public AnimationCurve moneyEarnEase;


    private GameController gameController = null;
    //    private Coroutine addMoneyInSequenceCoroutine = null;
    // Start is called before the first frame update
    void Start()
    {
        gameController = GameController.Instance;

        coinAmountTxt.text = GameController.CoinAmount + "";
        gameController.MoneyAmountChanged += OnMoneyAmountChanged;
        //        gameController.MoneyAmountChangedInc += OnMoneyAmountChangedIncrementally;



    }
    public void OnMoneyAmountChanged()
    {
        coinAmountTxt.text = GameController.CoinAmount + "";
        //  SoundManager.Instance.PlaySound("coinClaim");   //or maybe without sound?
    }

    /*     public void OnMoneyAmountChangedIncrementally()
        {
            // coinAmountTxt.text= GameController.CoinAmount+"";
            if (addMoneyInSequenceCoroutine == null)
                addMoneyInSequenceCoroutine = StartCoroutine(AddMoneyInSequence(int.Parse(coinAmountTxt.text), GameController.CoinAmount, 1f));
            else
            {
                StopCoroutine(addMoneyInSequenceCoroutine);
                addMoneyInSequenceCoroutine = null;
                addMoneyInSequenceCoroutine = StartCoroutine(AddMoneyInSequence(int.Parse(coinAmountTxt.text), GameController.CoinAmount, 1f));
            }
        } */

    /*     IEnumerator AddMoneyInSequence(float curCoinAmount, float targetCoinAmount, float duration)
        {
            float startCoinAmount = curCoinAmount;

            int prevCoinAmountInt = (int)curCoinAmount;
            int curCoinAmountInt = (int)curCoinAmount;
            for (float t = 0f, counter = 0f; t < 1f; counter += Time.deltaTime, t = counter / duration)
            {
                prevCoinAmountInt = (int)curCoinAmount;
                curCoinAmount = Mathf.Lerp(startCoinAmount, targetCoinAmount, moneyEarnEase.Evaluate(t));
                curCoinAmountInt = (int)curCoinAmount;

                if (curCoinAmountInt != prevCoinAmountInt)
                {
                    coinAmountTxt.text = (int)curCoinAmount + "";
                  //  SoundManager.Instance.PlaySound("coinClaim");
                }

                yield return null;
            }

            if (curCoinAmount != targetCoinAmount)
            {
                coinAmountTxt.text = (int)targetCoinAmount + "";
             //   SoundManager.Instance.PlaySound("coinClaim");
            }

            addMoneyInSequenceCoroutine = null;
        } */

}

