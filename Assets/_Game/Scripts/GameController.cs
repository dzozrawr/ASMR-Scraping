//using System.Collections;
using System.Collections.Generic;
using BzKovSoft.ObjectSlicer;
using UnityEngine;
using BzKovSoft.ObjectSlicerSamples;
using System.Collections;
using RayFire;
using DG.Tweening;
using CrazyLabsExtension;
using Lofelt.NiceVibrations;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    private static GameController instance = null;
    public static GameController Instance { get => instance; }

    private static int coinAmount;
    public static int CoinAmount { get => coinAmount; set => coinAmount = value; }
    public bool IsCutMade { get => isCutMade; set => isCutMade = value; }
    public BzSliceTryResult CurCutLayer { get => curCutLayer; set => curCutLayer = value; }

    // public static SaveData saveData=null;



    public Transform curSliceableObject;

    public Transform scraper = null;

    public GameObject arrayOfSlicablesGOParent = null;

    public List<GameObject> arrayOfSliceablePrefabs = null;

    public CoinUIEarnScript coinEarnTweenPrefab = null;

    public float instantiatedSlicableYOffset = 0f;

    public delegate void GameControllerEvent();

    public GameControllerEvent MoneyAmountChanged;

    private List<GameObject> arrayOfSlicablesGO = null;

    private int curSlicableInd = 0;

    private int curPrefabSliceableInd = 0;

    private GameObject _slice;
    private Material[] _materials;

    private bool _inProgress = false;

    private float _pointY;

    private bool shouldCutNewLayer = true;

    private BzSliceTryResult curCutLayer = null;
    private bool isLastLayer = false;

    private int sliceId = 0;

    private bool isCutMade = false;

    private bool isLastStripe = false;

    private float centerX;

    private float prevLayerHeight = 0f;

    private SpatulaScript scraperScript;

    private int stripeMoneyWorth = 1;

    private bool shouldDestroySliceableParent = false;

    private bool didStartRolling = false;

    private SliceableObjectScript curSliceableObjectScript = null;

    private float incomeAddition = 0f;

    private float zRange, zStart;

    private float startingRadius, endRadius;

    private SliceableObjectScript specialNextTile = null;

    private float rollingProgress = 0f;

    private SoundManager soundManager = null;

    public float IncomeAddition { get => incomeAddition; set => incomeAddition = value; }
    public SliceableObjectScript CurSliceableObjectScript { get => curSliceableObjectScript; set => curSliceableObjectScript = value; }
    public bool InProgress { get => _inProgress; set => _inProgress = value; }

    //public int StripeMoneyWorth { get => stripeMoneyWorth; set => stripeMoneyWorth = value; }

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        scraperScript = scraper.GetComponent<SpatulaScript>();

        if (arrayOfSlicablesGO == null)
        {
            if (arrayOfSlicablesGOParent.transform.childCount > 0)
            {
                arrayOfSlicablesGO = new List<GameObject>();
                for (int i = 0; i < arrayOfSlicablesGOParent.transform.childCount; i++)
                {
                    arrayOfSlicablesGO.Add(arrayOfSlicablesGOParent.transform.GetChild(i).gameObject);
                }
            }
        }

        if (curSliceableObject != null)
        {
            curSliceableObjectScript = curSliceableObject.GetComponent<SliceableObjectScript>();
        }

        // Application.targetFrameRate = 60;
        //Debug.Log("Frame rate limited to 60!");
    }

    private void Start()
    {
        soundManager = SoundManager.Instance;
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKey(KeyCode.E))
        {
            AddMoney(5);
        }
    }
#endif

    public void AddMoney(int moneyAmountToAdd)
    {
        coinAmount += moneyAmountToAdd;
        //if ((moneyEarnedInLevel + moneyAmountToAdd) < 0) moneyEarnedInLevel = 0; else moneyEarnedInLevel += moneyAmountToAdd;
        MoneyAmountChanged?.Invoke();
    }

    public void CutWSpatula(GameObject target)
    {
        if (isCutMade) return;
        if (_inProgress) return;


        Collider col = scraper.GetComponent<Collider>();

        Plane planeSide = new Plane(Vector3.left, new Vector3(col.bounds.center.x - col.bounds.extents.x, col.bounds.center.y, col.bounds.center.z));

        if (curCutLayer == null)
        {
            float remainingLayerHeight = curSliceableObject.transform.GetChild(0).GetComponent<Collider>().bounds.size.y;
            if (remainingLayerHeight <= (scraperScript.SpatulaHeight * scraperScript.layerHeightMultiplier))    //this should be checked on the first cut of the new sliceable
            {
                isLastLayer = true;
            }
        }

        if ((curCutLayer == null) && (!isLastLayer))    //here we are cutting new layer (if its not the last one) with cutting a new stripe at the same time
        {
            var sliceable = target.GetComponent<IBzSliceableAsync>();
            if (sliceable == null)
            {
                return;
            }

            Plane plane = new Plane(Vector3.up, col.bounds.center);

            sliceable.Slice(plane, ++sliceId, r =>
            {
                if (!r.sliced)
                {
                    // GameObject planeGO= GameObject.CreatePrimitive(PrimitiveType.Plane);
                    // planeGO.transform.position=col.bounds.center;

                    Debug.LogError("Failed to slice new layer.");
                    return;
                }
                isCutMade = true;

                curCutLayer = r;

                _slice = r.outObjectPos;

                _slice.transform.SetParent(r.outObjectNeg.transform); //setting the slice to be the child of the sliced object

                // Debug.Log("Sliced the whole layer.");

                IBzSliceableAsync newSlicable = r.outObjectPos.GetComponent<IBzSliceableAsync>();

                StartCoroutine(CutStripeAfterNewLayerAfterDelay(newSlicable, planeSide, 0.05f));   //there needs to be a delay for a second slice for some reason
            });

        }
        else if (!isLastLayer)      //here we are just cutting the stripes, with a special case of rolling the whole layer if no stripes were cut
        {
            IBzSliceableAsync newSlicable;
            newSlicable = curCutLayer.outObjectPos.GetComponent<IBzSliceableAsync>();


            newSlicable.Slice(planeSide, ++sliceId, r1 =>
                   {
                       // Debug.Log("Sliced a stripe");
                       if (!r1.sliced)  //if we didn't manage to slice the stripe, it's the last stripe in the layer and we just roll it without cutting
                       {
                           //float remainingStripeWidth = curCutLayer.outObjectPos.GetComponent<Collider>().bounds.size.x;
                           // if (remainingStripeWidth < spatulaScript.SpatulaWidth)
                           //  {

                           SetRollingVariables(curCutLayer.outObjectPos);

                           // prevLayerHeight += _slice.GetComponent<Collider>().bounds.size.y;
                           float remainingLayerHeight = curSliceableObject.transform.GetChild(0).GetComponent<Collider>().bounds.size.y;

                           //if (remainingLayerHeight <= scraperScript.SpatulaHeight)
                           if (remainingLayerHeight <= (scraperScript.SpatulaHeight * scraperScript.layerHeightMultiplier))
                           {
                               isLastLayer = true;
                           }
                           curCutLayer = null;
                           scraperScript.ShouldCarriageReturn = true;
                           // }
                           return;
                       }
                       SetRollingVariables(r1.outObjectNeg);

                       curCutLayer = r1;
                   });
        }
        else  //if isLastLayer==true, here we are cutting the last layer with a special case of rolling the whole layer if no stripes were cut
        {
            IBzSliceableAsync newSlicable;
            if (curCutLayer != null) newSlicable = curCutLayer.outObjectPos.GetComponent<IBzSliceableAsync>();
            else
                newSlicable = curSliceableObject.GetComponentInChildren<IBzSliceableAsync>();


            newSlicable.Slice(planeSide, ++sliceId, r1 =>
                   {
                       //Debug.Log("Sliced a stripe");
                       if (!r1.sliced)  //if we didn't manage to slice the stripe, it's the last stripe in the layer and we just roll it without cutting
                       {
                           // float remainingStripeWidth = curCutLayer.outObjectPos.GetComponent<Collider>().bounds.size.x;
                           //  if (remainingStripeWidth < spatulaScript.SpatulaWidth)
                           // {
                           if (curCutLayer == null) //this is true when the last layer wasn't cut previously, because it is narrower than the scraper and this is the first cut
                           {
                               // SetRollingVariables(curSliceableObject.GetChild(0).gameObject);
                               if (!curSliceableObjectScript.shouldCrumble)
                               {
                                   SetRollingVariables(curSliceableObject.GetChild(0).gameObject);
                               }
                               else
                               {
                                   SetCrumbleVariables(curSliceableObject.GetChild(0).gameObject);
                               }
                           }
                           else
                           {
                               //SetRollingVariables(curCutLayer.outObjectPos);
                               if (!curSliceableObjectScript.shouldCrumble)
                               {
                                   SetRollingVariables(curCutLayer.outObjectPos);
                               }
                               else
                               {
                                   SetCrumbleVariables(curCutLayer.outObjectPos);
                               }
                           }


                           isLastLayer = false;
                           prevLayerHeight = 0f;
                           curCutLayer = null;

                           if ((arrayOfSliceablePrefabs != null) && (arrayOfSliceablePrefabs.Count > 0))
                               InstantiateANewSlicable();

                           curSlicableInd = (curSlicableInd + 1) % arrayOfSlicablesGO.Count;
                           /*                            curSliceableObject = arrayOfSlicablesGO[curSlicableInd].transform;
                                                      curSliceableObjectScript = curSliceableObject.GetComponent<SliceableObjectScript>(); */

                           scraperScript.ShouldCarriageReturn = true;

                           shouldDestroySliceableParent = true;
                           //  }
                           return;
                       }
                       if (!curSliceableObjectScript.shouldCrumble)
                       {
                           SetRollingVariables(r1.outObjectNeg);
                       }
                       else
                       {
                           SetCrumbleVariables(r1.outObjectNeg);
                       }


                       curCutLayer = r1;
                   });
        }
    }

    private void SetCrumbleVariables(GameObject slice)
    {
        _slice = slice;   //this is the gameObject that is rolled
        isCutMade = true;   //this variable prevents the cutting function to be called again
        //_inProgress = true; 

        _slice.tag = "Fragment";
        _slice.layer = 7;

        RayfireRigid rayfireRigidOfSlice = _slice.AddComponent<RayfireRigid>();
        //rayfireRigidOfSlice.simulationType=SimType.Sleeping;
        rayfireRigidOfSlice.simulationType = SimType.Kinematic;
        rayfireRigidOfSlice.physics.materialType = MaterialType.Brick;
        rayfireRigidOfSlice.physics.colliderType = RFColliderType.Box;
        rayfireRigidOfSlice.physics.planarCheck = false;
        rayfireRigidOfSlice.demolitionType = DemolitionType.Runtime;
        rayfireRigidOfSlice.meshDemolition.amount = 15;

        rayfireRigidOfSlice.fading.fadeType = FadeType.ScaleDown;
        rayfireRigidOfSlice.fading.fadeTime = 1f;
        rayfireRigidOfSlice.fading.lifeTime = 5f;
        rayfireRigidOfSlice.fading.lifeVariation = 0f;

        rayfireRigidOfSlice.Demolish();
    }

    private void InstantiateANewSlicable()
    {
        GameObject newSlicableInstance = null;
        if (specialNextTile == null)
        {
            curPrefabSliceableInd = Random.Range(0, arrayOfSliceablePrefabs.Count - 1);
            newSlicableInstance = Instantiate(arrayOfSliceablePrefabs[curPrefabSliceableInd]);
            specialNextTile = newSlicableInstance.GetComponent<SliceableObjectScript>().specialNextTile;
        }
        else
        {
            newSlicableInstance = Instantiate(specialNextTile.gameObject);
            specialNextTile = specialNextTile.specialNextTile;    //getting the next tile of the newly created tile, will be null if there is no special tile
        }
        /*         curPrefabSliceableInd = Random.Range(0, arrayOfSliceablePrefabs.Count - 1);
                GameObject newSlicableInstance = Instantiate(arrayOfSliceablePrefabs[curPrefabSliceableInd]); */
        //set the appropriate position for it
        Transform lastObjectTransform = arrayOfSlicablesGO[(curSlicableInd + arrayOfSlicablesGO.Count - 1) % arrayOfSlicablesGO.Count].transform;
        newSlicableInstance.transform.SetParent(arrayOfSlicablesGOParent.transform);
        newSlicableInstance.transform.SetAsLastSibling();
        float lastObjectMinY = lastObjectTransform.GetChild(0).GetComponent<Collider>().bounds.min.y;
        float newSlicableExtentY = newSlicableInstance.transform.GetChild(0).GetComponent<Collider>().bounds.extents.y;
        newSlicableInstance.transform.position = new Vector3(lastObjectTransform.position.x, lastObjectMinY - newSlicableExtentY - instantiatedSlicableYOffset, lastObjectTransform.position.z); //hard code 0.05f to give objects some space


        // curPrefabSliceableInd = (curPrefabSliceableInd + 1) % arrayOfSliceablePrefabs.Count;
        arrayOfSlicablesGO[curSlicableInd] = newSlicableInstance;

        newSlicableInstance.transform.DOPunchScale(new Vector3(0.25f, 0.25f, 0.25f), 0.33f, 0, 0.1f);
    }

    private void SetRollingVariables(GameObject sliceGO)
    {
        _slice = sliceGO;   //this is the gameObject that is rolled
        isCutMade = true;   //this variable prevents the cutting function to be called again
                            //   _inProgress = true; //this variable prevents the cutting function to be called again x2
        _pointY = float.MaxValue;

        SetScrapingStart();


        if (curSliceableObjectScript != null)
            scraperScript.ObjectResistance = curSliceableObjectScript.scrapingSpeedReductionMul;  //this slows down the scraping if the "material" is more rough

        var meshFilter = _slice.GetComponent<MeshFilter>();
        centerX = meshFilter.sharedMesh.bounds.center.y;

        _materials = _slice.GetComponent<MeshRenderer>().materials;

        foreach (var material in _materials)
        {
            material.SetFloat("_PointX", centerX);
        }
    }

    public void SetScrapingStart() //sets the inProgress to true and sets the appropriate scraping sound, regardless of the scraping mechanic
    {
        _inProgress = true;
        if (soundManager != null)
        {
            if ((curSliceableObjectScript != null) && (CurSliceableObjectScript.specialScrapingSound != null))
            {
                soundManager.PlaySound(CurSliceableObjectScript.specialScrapingSound);
            }
            else
            {
                soundManager.PlaySound("scrapeSound");
            }
        }
    }

    IEnumerator CutStripeAfterNewLayerAfterDelay(IBzSliceableAsync sliceable, Plane cuttingPlane, float delay)
    {
        yield return new WaitForSeconds(delay);
        sliceable.Slice(cuttingPlane, ++sliceId, r1 =>
                    {
                        //  Debug.Log("Sliced a stripe");
                        if (!r1.sliced) //if it wasn't cut then the stripe is narrower than the scraper
                        {
                            // Debug.Log("New layer cut without stripes.");
                            SetRollingVariables(curCutLayer.outObjectPos);

                            float remainingLayerHeight = curSliceableObject.transform.GetChild(0).GetComponent<Collider>().bounds.size.y;

                            if (remainingLayerHeight <= scraperScript.SpatulaHeight)
                            {
                                isLastLayer = true;
                            }
                            curCutLayer = null;
                            scraperScript.ShouldCarriageReturn = true;

                            return;
                        }
                        SetRollingVariables(r1.outObjectNeg);

                        curCutLayer = r1;
                    });
    }



    public void SendScraperCoordinates(SpatulaScript s)  //it might not be spatula, might abstract it to ScraperScript 
    {
        if (!_inProgress) return;


        var pos = s.transform.position;
        pos.z = s.transform.position.z;

        float z = pos.z;

        //float progress=0f;

#if !UNITY_EDITOR
VibrateBasedOnResistance(curSliceableObjectScript.scrapingSpeedReductionMul);
#endif

        if (!didStartRolling)   //this marks the beginning of the rolling, happens only once in the beginning
        {
            zStart = z;
            zRange = -0.5f - zStart;

            /*             if ((_materials != null) && (_materials.Length > 0))
                        {
                            startingRadius = _materials[0].GetFloat("_Radius");
                        } */
            didStartRolling = true;

            // endRadius = startingRadius * 3f;
        }
        // spatula.position = pos;

        rollingProgress = (z - zStart) / zRange;

        if (rollingProgress > 1f) rollingProgress = 0.99f;

        if (soundManager != null)
            SoundManager.audioSrc.time = rollingProgress * SoundManager.audioSrc.clip.length;

        if (curSliceableObjectScript.shouldCrumble)
        {
            if (z <= -0.5f)
            {
                if (soundManager != null)
                {
                    SoundManager.audioSrc.time = 0f;
                    SoundManager.audioSrc.clip = null;
                }
                rollingProgress = 0f;
                //                _slice.GetComponent<Rigidbody>().isKinematic = false;
                _inProgress = false;

                // Debug.Log("if (z <= -0.5f)");

                // _slice.GetComponent<Rigidbody>().AddForce(-Vector3.forward * 5, ForceMode.Impulse);

                CoinUIEarnScript c = Instantiate(coinEarnTweenPrefab, s.transform.position, Quaternion.identity);
                if ((curSliceableObjectScript != null) && (curSliceableObjectScript.stripeCoinWorth != -1))
                {
                    // c.PlayCoinEarnAnimation((int)(curSliceableObjectScript.stripeCoinWorth * incomeAddition));
                    c.PlayCoinEarnAnimation((int)(curSliceableObjectScript.stripeCoinWorth + incomeAddition));
                }
                else
                {
                    //c.PlayCoinEarnAnimation((int)(stripeMoneyWorth * incomeAddition)); //plays the tween, the money is added after the move tween is finished
                    c.PlayCoinEarnAnimation((int)(stripeMoneyWorth + incomeAddition)); //plays the tween, the money is added after the move tween is finished
                }


                Destroy(c.gameObject, 1.5f);

                scraperScript.ObjectResistance = 1f;  //returns the scraper to normal speed

                if (!shouldDestroySliceableParent)
                {
                    if (_slice != null)
                    {
                        _slice.transform.SetParent(null);

                        Destroy(_slice.gameObject, 1f);
                    }

                }
                else
                {

                    shouldDestroySliceableParent = false;

                    CoinUIEarnScript layerCoinTween = null;// = Instantiate(coinEarnTweenPrefab, _slice.transform.parent.position, Quaternion.identity);   //finishing the whole layer reward
                    if (_slice != null)
                    {
                        layerCoinTween = Instantiate(coinEarnTweenPrefab, _slice.transform.parent.position, Quaternion.identity);   //finishing the whole layer reward
                    }
                    else
                    {
                        layerCoinTween = Instantiate(coinEarnTweenPrefab, curSliceableObject.transform.position, Quaternion.identity);   //finishing the whole layer reward
                    }
                    // layerCoinTween.PlayCoinEarnAnimation((int)(curSliceableObjectScript.layerCoinWorth * incomeAddition));
                    layerCoinTween.PlayCoinEarnAnimation((int)(curSliceableObjectScript.layerCoinWorth + incomeAddition));
                    //HapticFeedbackController.
                    if (_slice != null)
                        Destroy(_slice.transform.parent.gameObject, 6f);
                    else
                    {
                        Destroy(curSliceableObject.gameObject,6f);
                    }

                    curSliceableObject = arrayOfSlicablesGO[curSlicableInd].transform;  //switching to the next object in the array AFTER rolling is done COMPLETELY
                    curSliceableObjectScript = curSliceableObject.GetComponent<SliceableObjectScript>();

                    Destroy(layerCoinTween.gameObject, 1.5f);
                    // ja sam homoseksualac, volim da drkam kite, isti sam kao ensar moram priznati...
/*                    if (_slice != null)
                        Destroy(_slice.transform.parent.gameObject, 1f);*/


                    scraperScript.EnableColliders(false);
                }

                /*             curSliceableObject = arrayOfSlicablesGO[curSlicableInd].transform;  //switching to the next object in the array AFTER rolling is done COMPLETELY
                            curSliceableObjectScript = curSliceableObject.GetComponent<SliceableObjectScript>(); */
                //c.SetMoneyAmount(2);
                didStartRolling = false;

            }
            return;
        }






        float pointZ = _slice.transform.InverseTransformPoint(pos).z;

        //float zRange=-0.5f-

        if (pointZ < _pointY)
        {
            _pointY = pointZ;
        }
        if ((_materials != null) && (_materials.Length > 0))
        {
            foreach (var material in _materials)
            {
                material.SetFloat("_PointY", _pointY);
                // material.SetFloat("_Radius",startingRadius+ (endRadius-startingRadius)*progress);
            }
        }

        if (z <= -0.5f)
        {
            //SoundManager.audioSrc.Stop();
            if (soundManager != null)
            {
                SoundManager.audioSrc.time = 0f;
                SoundManager.audioSrc.clip = null;
                rollingProgress = 0f;
            }


            _slice.GetComponent<Rigidbody>().isKinematic = false;
            _inProgress = false;

            // Debug.Log("if (z <= -0.5f)");

            _slice.GetComponent<Rigidbody>().AddForce(-Vector3.forward * 5, ForceMode.Impulse);

            CoinUIEarnScript c = Instantiate(coinEarnTweenPrefab, s.transform.position, Quaternion.identity);
            if ((curSliceableObjectScript != null) && (curSliceableObjectScript.stripeCoinWorth != -1))
            {
                //c.PlayCoinEarnAnimation((int)(curSliceableObjectScript.stripeCoinWorth * incomeAddition));
                c.PlayCoinEarnAnimation((int)(curSliceableObjectScript.stripeCoinWorth + incomeAddition));
            }
            else
            {
                //c.PlayCoinEarnAnimation((int)(stripeMoneyWorth * incomeAddition)); //plays the tween, the money is added after the move tween is finished
                c.PlayCoinEarnAnimation((int)(stripeMoneyWorth + incomeAddition)); //plays the tween, the money is added after the move tween is finished
            }


            Destroy(c.gameObject, 1.5f);

            scraperScript.ObjectResistance = 1f;  //returns the scraper to normal speed

            if (!shouldDestroySliceableParent)
            {
                _slice.transform.SetParent(null);




                Destroy(_slice.gameObject, 1f);
            }
            else
            {

                shouldDestroySliceableParent = false;

                CoinUIEarnScript layerCoinTween = Instantiate(coinEarnTweenPrefab, _slice.transform.parent.position, Quaternion.identity);   //finishing the whole layer reward
                //layerCoinTween.PlayCoinEarnAnimation((int)(curSliceableObjectScript.layerCoinWorth * incomeAddition));
                layerCoinTween.PlayCoinEarnAnimation((int)(curSliceableObjectScript.layerCoinWorth + incomeAddition));

                curSliceableObject = arrayOfSlicablesGO[curSlicableInd].transform;  //switching to the next object in the array AFTER rolling is done COMPLETELY
                curSliceableObjectScript = curSliceableObject.GetComponent<SliceableObjectScript>();

                Destroy(layerCoinTween.gameObject, 1.5f);
                Destroy(_slice.transform.parent.gameObject, 1f);

                scraperScript.EnableColliders(false);
            }

            /*             curSliceableObject = arrayOfSlicablesGO[curSlicableInd].transform;  //switching to the next object in the array AFTER rolling is done COMPLETELY
                        curSliceableObjectScript = curSliceableObject.GetComponent<SliceableObjectScript>(); */
            //c.SetMoneyAmount(2);
            didStartRolling = false;

        }
    }

    private void VibrateBasedOnResistance(float resistance)
    {    //the most resistant is 0 and the least resistant is 1

        if (resistance > 0.66f)
        {
            HapticFeedbackController._hapticMinimumDelay = 0.1f;
            HapticFeedbackController.TriggerHaptics(HapticPatterns.PresetType.LightImpact);
            return;
        }

        if (resistance > 0.33f)
        {
            HapticFeedbackController._hapticMinimumDelay = 0.05f;
            HapticFeedbackController.TriggerHaptics(HapticPatterns.PresetType.MediumImpact);
            return;
        }

        if (resistance > 0f)
        {
            HapticFeedbackController._hapticMinimumDelay = 0.05f;
            HapticFeedbackController.TriggerHaptics(HapticPatterns.PresetType.HeavyImpact);
            return;
        }

    }

    public static bool IsOverRaycastBlockingUI()
    {
        int id = 0;
#if UNITY_EDITOR
        id = -1;
#endif
        //  bool isOverUI = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(id);       //this checks if the pointer is over UI (through EventSystem) and if it is then it blocks raycasts
        bool isOverBlockingUI =
                                UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(id) &&
                                UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject != null &&
                                UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.CompareTag("UIRayBlock");       //this checks if the pointer is over UI (through EventSystem) and if it is then it blocks raycasts

        return isOverBlockingUI;
    }
#if UNITY_EDITOR
    private void OnApplicationQuit()
    {
        SaveData saveData = new SaveData();
        SaveSystem.SaveGameXML(saveData);
    }
#endif

#if !UNITY_EDITOR
    private void OnApplicationFocus(bool focusStatus)
    {
        if (!focusStatus)
        {
            SaveData saveData = new SaveData();
            SaveSystem.SaveGameXML(saveData);
        }
    }
#endif
}
