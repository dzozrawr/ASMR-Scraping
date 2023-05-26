using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BzKovSoft.ObjectSlicerSamples;

public class SliceableObjectScript : MonoBehaviour
{
/*     public Renderer objRenderer=null;
    public ObjectSlicerSample slicerScript=null;
    public float rollingRadius=0.4f;

    private Material mat=null; */
    public float scrapingSpeedReductionMul=1f;
    public float stripeCoinWorth=-1f;
    public float layerCoinWorth = 3f;

    public bool shouldCrumble=false;

    public List<Transform>  carriageReturnList=null;

    public SliceableObjectScript specialNextTile=null;

    public AudioClip specialScrapingSound=null;

    private void Awake() {
/*         mat=objRenderer.material;
        mat.SetFloat("_Radius", rollingRadius);

        slicerScript.DefaultSliceMaterial=mat; */
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

}
