using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SpatulaScript : MonoBehaviour
{
    public enum MoveState { Scrape, Return, Tab, CarriageReturn }

    public bool shouldSetInitPos = true;

    public float scrapeSpeed = 1f;

    public float returnSpeedMul = 1f;
    public float tabSpeedMul = 1f;
    public float carriageReturnSpeedMul = 1f;

    public float speedUpMulVal = 1.5f;

    private float speedUpMul = 1f;


    public Rigidbody rb = null;
    public Collider collider = null;
    public Collider fragmentsCollider = null;
    public List<TrailRenderer> trails = null;
    public float layerHeightMultiplier = 1f;

    public float preferredSpatulaHeight = 0f;

    public List<ScraperInfo> scrapers = null;
    private ScraperInfo curScraper = null;
    private int curScraperInd = 0;


    private float speed = 1f;
    private Vector3 startingPos = Vector3.zero;
    private GameController gameController = null;
    private float spatulaWidth;
    private float spatulaHeight;

    private float targetZ = -1f;  //hard coded



    private float defaultSpeed = 1f;

    private float doubledSpeed;

    private Vector3 targetPos;
    private Vector3 moveVector;

    private float curTargetDistance = 10f;
    private float prevTargetDistance = 10f;

    private MoveState moveState = MoveState.Scrape;

    private Coroutine speedUpCoroutine = null;

    private Vector3 gizmosSpherePos = Vector3.zero;

    private Vector3 gizmosPlaneCutPosition = Vector3.zero;

    private float speedUpCdTimer = 0f;
    private float speedUpCdDuration = 0.5f;

    private bool isSpeedUpActive = false;

    private bool shouldCarriageReturn = false;

    private float gameStartDelayTimer = 0f;
    private float gameStartDelayValue = 0.75f;

    private Vector3 gizmosBoxCenter = Vector3.negativeInfinity, gizmosBoxSize;

    private float objectResistance = 1f;    //value should be between 0 and 1, used to modify the speed of scraping

    public float SpatulaWidth { get => spatulaWidth; set => spatulaWidth = value; }
    public float SpatulaHeight { get => spatulaHeight; set => spatulaHeight = value; }
    public bool ShouldCarriageReturn { get => shouldCarriageReturn; set => shouldCarriageReturn = value; }
    public float ObjectResistance { get => objectResistance; set => objectResistance = value; }

    private void Awake()
    {


        spatulaWidth = collider.bounds.size.x;
        //        Debug.Log("spatulaWidth=" + spatulaWidth);

        if (preferredSpatulaHeight == 0)
            spatulaHeight = collider.bounds.size.y;
        else spatulaHeight = preferredSpatulaHeight;



        speed = scrapeSpeed;
        doubledSpeed = speed * 2;



        SetSpedUpEffect(false);

        curScraper = scrapers[curScraperInd];   //the initial value of curScraper (needed for upgrades)
    }

    void Start()
    {

        gameController = GameController.Instance;

        //setting the spatula init pos
        if (shouldSetInitPos)
        {
            if ((gameController.CurSliceableObjectScript != null) && (gameController.CurSliceableObjectScript.carriageReturnList != null) && (gameController.CurSliceableObjectScript.carriageReturnList.Count > 0))
            {
                Debug.Log("Carriager return list not null");
                Transform scrapePos = gameController.CurSliceableObjectScript.carriageReturnList[0];
                gameController.CurSliceableObjectScript.carriageReturnList.RemoveAt(0);

                transform.position = new Vector3(scrapePos.position.x, scrapePos.position.y, transform.position.z);
            }
            else
            {
                Transform slicableObjectTransform = gameController.curSliceableObject.GetChild(0);
                Collider slicableObjectCollider = slicableObjectTransform.GetComponent<Collider>();
                Vector3 v = FindScrapePointBasedOnY(slicableObjectTransform.GetComponent<MeshFilter>().sharedMesh, slicableObjectTransform, slicableObjectCollider.bounds.max.y - spatulaHeight * layerHeightMultiplier);
                transform.position = new Vector3(v.x - spatulaWidth / 2, v.y + spatulaHeight / 2, transform.position.z);

                gizmosBoxCenter = slicableObjectCollider.bounds.center;
                gizmosBoxSize = slicableObjectCollider.bounds.size;
            }




        }

        startingPos = transform.position;

        targetPos = new Vector3(transform.position.x, transform.position.y, targetZ);// this should be set once
        moveVector = targetPos - transform.position;

        if (gameController.CurSliceableObjectScript.shouldCrumble)
        {
            Invoke(nameof(CutWSpatulaAfterDelay), 0.1f);
            fragmentsCollider.enabled = true;
        }
    }
    private void CutWSpatulaAfterDelay()
    {
        gameController.CutWSpatula(gameController.curSliceableObject.transform.GetChild(0).gameObject);
    }
    private void OnTriggerEnter(Collider other)
    {
        // Debug.Log("OnTriggerEnter");
        if (!gameController.CurSliceableObjectScript.shouldCrumble)
        {
            gameController.CutWSpatula(other.gameObject);
        }

        if (moveState==MoveState.Scrape && other.gameObject.tag.Equals("Fragment"))
        {
            other.gameObject.GetComponent<Rigidbody>().isKinematic = false;


            objectResistance = gameController.CurSliceableObjectScript.scrapingSpeedReductionMul;
            gameController.SetScrapingStart();
           // gameController.InProgress = true;
        }
    }

    /*     private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.tag.Equals("Fragment"))
            {
                other.gameObject.GetComponent<Rigidbody>().isKinematic = false;
            }
        } */

    private void Update()
    {
        if (gameStartDelayTimer < gameStartDelayValue)//doing this for fixing the problem where in the beginning of the game the spatula doesn't register a collision sometimes
        {
            gameStartDelayTimer += Time.deltaTime;
            return;
        }

        if (IsInStartingPos())
        {
            gameController.IsCutMade = false;
        }
        if (Input.GetMouseButtonDown(0))
        {
            if (GameController.IsOverRaycastBlockingUI()) return;
            SpeedUp();
        }
        /*         Move();

                if (isSpeedUpActive)
                {
                    speedUpCdTimer += Time.deltaTime;
                    if (speedUpCdTimer >= speedUpCdDuration)
                    {
                        //speed = defaultSpeed;
                        speed /= speedUpMul;
                        speedUpMul = 1f;
                        isSpeedUpActive = false;
                        speedUpCdTimer = 0f;
                        //speedUpCoroutine = null;
                        SetSpedUpEffect(false);
                    }
                } */
    }

    private void FixedUpdate()
    {
        Move();

        if (isSpeedUpActive)
        {
            speedUpCdTimer += Time.deltaTime;
            if (speedUpCdTimer >= speedUpCdDuration)
            {
                //speed = defaultSpeed;
                speed /= speedUpMul;
                speedUpMul = 1f;
                isSpeedUpActive = false;
                speedUpCdTimer = 0f;
                //speedUpCoroutine = null;
                SetSpedUpEffect(false);
            }
        }
    }

    private Vector3 FindMeshTopRightVert(Mesh m, Transform transformOfMeshGO)
    {
        Vector3[] verts = m.vertices;
        Vector3 topVertex = new Vector3(0, float.NegativeInfinity, 0);
        List<Vector3> topVertices = new List<Vector3>();

        Debug.Log("Vertex count:" + verts.Length);

        for (int i = 0; i < verts.Length; i++)
        {
            Vector3 vert = transformOfMeshGO.TransformPoint(verts[i]);
            if (vert.y > topVertex.y)
            {
                topVertex = vert;
                topVertices.Clear();
                topVertices.Add(vert);
            }
            else if (vert.y == topVertex.y)
            {
                topVertices.Add(vert);
            }
        }
        if (topVertices.Count == 1) return topVertex;

        Vector3 rightMostVertex = new Vector3(float.NegativeInfinity, topVertex.y, 0);
        for (int j = 0; j < topVertices.Count; j++)
        {
            if (topVertices[j].x >= rightMostVertex.x)
            {
                rightMostVertex = topVertices[j];
            }
        }

        return rightMostVertex;

        //return Vector3.negativeInfinity;
    }
    private Vector3 FindScrapePointBasedOnY(Mesh m, Transform transformOfMeshGO, float y)
    {
        // Vector3 worldUp = transformOfMeshGO.InverseTransformDirection(Vector3.up).normalized;
        Plane p = new Plane(Vector3.up, new Vector3(0, y, 0));
        Vector3[] verts = m.vertices;
        Vector3 closestVertex = new Vector3(0, float.NegativeInfinity, 0);
        float minDist = float.PositiveInfinity;
        List<Vector3> closestVertices = new List<Vector3>();

        for (int i = 0; i < verts.Length; i++)
        {
            Vector3 verWS = transformOfMeshGO.TransformPoint(verts[i]);
            float dist = Mathf.Abs(p.GetDistanceToPoint(verWS));
            if (dist < minDist)
            {
                minDist = dist;
                closestVertex = verWS;
                closestVertices.Clear();
                closestVertices.Add(closestVertex);
            }
            else
            if (Mathf.Approximately(dist, minDist))
            {
                closestVertices.Add(verWS);
            }
        }
        //        Debug.Log("Num of closest verts=" + closestVertices.Count);


        if (closestVertices.Count == 1) return closestVertex; //return new Vector3(closestVertex.x,y,closestVertex.z);

        Vector3 rightMostVertex = new Vector3(float.NegativeInfinity, closestVertex.y, 0);
        for (int j = 0; j < closestVertices.Count; j++)
        {
            if (closestVertices[j].x >= rightMostVertex.x)
            {
                rightMostVertex = closestVertices[j];
            }
        }

        gizmosSpherePos = rightMostVertex;
        // return new Vector3(rightMostVertex.x, y, rightMostVertex.z);
        return rightMostVertex;
    }
    /* private Vector3 FindScrapePointBasedOnY(Mesh m, Transform transformOfMeshGO, float y)
     {
         Plane p = new Plane(Vector3.up, new Vector3(0, y, 0));


         Vector3[] verts = m.vertices;
         Vector3 closestVertex = new Vector3(0, float.NegativeInfinity, 0);
         float minDist = float.PositiveInfinity;
         List<Vector3> closestVertices = new List<Vector3>();

         gizmosPlaneCutPosition = new Vector3(0, y, 0);

         for (int i = 0; i < verts.Length; i++)
         {
             Vector3 verWS = transformOfMeshGO.TransformPoint(verts[i]);
             float dist = Mathf.Abs(p.GetDistanceToPoint(verWS));
             if (dist < minDist)
             {
                 minDist = dist;
                 closestVertex = verWS;
                 closestVertices.Clear();
                 closestVertices.Add(closestVertex);

             }
             else if (Mathf.Approximately(dist, minDist))
             {
                 closestVertices.Add(verWS);
             }


         }

         if (closestVertices.Count == 1)
         {
             Debug.Log("Only one closest vertext");
             gizmosSpherePos = closestVertices[0];
             return closestVertex; 
         }

         Vector3 rightMostVertex = new Vector3(float.NegativeInfinity, closestVertex.y, 0);
         List<Vector3> rightMostVertices = new List<Vector3>();
         for (int j = 0; j < closestVertices.Count; j++)
         {
             if (closestVertices[j].x > rightMostVertex.x)
             {
                 rightMostVertex = closestVertices[j];
                 rightMostVertices.Clear();
                 rightMostVertices.Add(rightMostVertex);
             } else if (Mathf.Approximately(closestVertices[j].x, rightMostVertex.x)){
                 rightMostVertices.Add(closestVertices[j]);
             }
         }

         if(rightMostVertices.Count>1){


             float yAvg=0f;

             for (int k = 0; k < rightMostVertices.Count; k++)
             {
                 yAvg+=rightMostVertices[k].y;
             }
             yAvg=yAvg/rightMostVertices.Count;

             rightMostVertex=new Vector3(rightMostVertex.x,yAvg,rightMostVertex.z);
         }

         gizmosSpherePos = rightMostVertex;
         return rightMostVertex;
     }*/

    private void SpeedUp()
    {

        if (!isSpeedUpActive)
        {
            speedUpMul = speedUpMulVal;
            speed *= speedUpMul;

            SetSpedUpEffect(true);

            isSpeedUpActive = true;
            speedUpCdTimer = 0f;
        }
        else
        {
            speedUpCdTimer = 0f;
        }
    }

    private void SetSpedUpEffect(bool shouldEnable)
    {
        foreach (TrailRenderer trail in trails)
        {
            trail.enabled = shouldEnable;
        }
    }

    public void EnableColliders(bool shouldEnable)
    {
        //        Debug.Log("EnableColliders");
        collider.enabled = shouldEnable;
        fragmentsCollider.enabled = shouldEnable;
    }

    private void Move()
    {
        curTargetDistance = Vector3.Distance(transform.position, targetPos);
        if ((curTargetDistance < 0.01f) || (curTargetDistance > prevTargetDistance))   //if the spatula overstepped the target and started going away
        {
            transform.position = targetPos;
            ChangeMoveState();
            return;
        }
        prevTargetDistance = curTargetDistance;

        rb.MovePosition(transform.position + moveVector * speed * objectResistance * Time.fixedDeltaTime);
        // rb.MovePosition(transform.position + moveVector * speed * Time.deltaTime);
        //transform.position += moveVector * speed * objectResistance * Time.deltaTime;

        gameController.SendScraperCoordinates(this);
    }

    private void ChangeMoveState()
    {
        switch (moveState)
        {
            case MoveState.Scrape:

                targetPos = new Vector3(transform.position.x, transform.position.y, startingPos.z);
                moveVector = targetPos - transform.position;
                moveState = MoveState.Return;

                speed = scrapeSpeed * returnSpeedMul;
                break;
            case MoveState.Return:

                if (!shouldCarriageReturn)
                {
                    Collider stripeCollider = gameController.CurCutLayer.outObjectPos.GetComponent<Collider>();
                    float stripeWidth = stripeCollider.bounds.size.x;
                    if (stripeWidth <= spatulaWidth) targetPos = new Vector3(stripeCollider.bounds.center.x, stripeCollider.bounds.min.y, transform.position.z);
                    else
                        targetPos = new Vector3(transform.position.x - spatulaWidth, transform.position.y, transform.position.z);    //can be a value other than spatula width
                    moveVector = targetPos - transform.position;
                    moveState = MoveState.Tab;

                    speed = scrapeSpeed * tabSpeedMul;
                }
                else
                {
                    // Debug.Log("Carriage return");    

                    Transform sliceableObjectTransform = gameController.curSliceableObject.GetChild(0);
                    Collider slicableObjectCollider = sliceableObjectTransform.GetComponent<Collider>();
                    float heightOfLayer = slicableObjectCollider.bounds.size.y;

                    if ((gameController.CurSliceableObjectScript.carriageReturnList != null) && (gameController.CurSliceableObjectScript.carriageReturnList.Count > 0))
                    {
                        targetPos = gameController.CurSliceableObjectScript.carriageReturnList[0].position;
                    }
                    else
                    {
                        if (heightOfLayer <= (spatulaHeight * layerHeightMultiplier))
                        {
                            Vector3 v = FindScrapePointBasedOnY(sliceableObjectTransform.GetComponent<MeshFilter>().mesh, sliceableObjectTransform, slicableObjectCollider.bounds.min.y);

                            targetPos = new Vector3(v.x - spatulaWidth / 2, v.y + spatulaHeight / 2, transform.position.z);
                        }
                        else
                        {
                            Vector3 v = FindScrapePointBasedOnY(sliceableObjectTransform.GetComponent<MeshFilter>().mesh, sliceableObjectTransform, slicableObjectCollider.bounds.max.y - spatulaHeight * layerHeightMultiplier);

                            targetPos = new Vector3(v.x - spatulaWidth / 2, v.y + spatulaHeight / 2, transform.position.z);
                        }
                    }

                    moveVector = targetPos - transform.position;
                    moveState = MoveState.CarriageReturn;
                    shouldCarriageReturn = false;

                    speed = scrapeSpeed * carriageReturnSpeedMul;
                }
                /*                else
                                {   
                                    Transform sliceableObjectTransform = gameManager.curSliceableObject.GetChild(0);
                                    Collider slicableObjectCollider = sliceableObjectTransform.GetComponent<Collider>();
                                    float heightOfLayer = slicableObjectCollider.bounds.size.y;
                                    if (heightOfLayer <= (spatulaHeight * layerHeightMultiplier))
                                    {
                                        Vector3 v = FindScrapePointBasedOnY(sliceableObjectTransform.GetComponent<MeshFilter>().sharedMesh, sliceableObjectTransform, slicableObjectCollider.bounds.min.y);
                                         targetPos = new Vector3(v.x, v.y + spatulaHeight , transform.position.z);
                                    }
                                    else
                                    {
                                        Vector3 v = FindScrapePointBasedOnY(sliceableObjectTransform.GetComponent<MeshFilter>().sharedMesh, sliceableObjectTransform, slicableObjectCollider.bounds.max.y - spatulaHeight * layerHeightMultiplier);
                                        if(Mathf.Approximately(v.y,sliceableObjectTransform.TransformPoint(sliceableObjectTransform.GetComponent<MeshFilter>().sharedMesh.bounds.max).y )){
                                            targetPos = new Vector3(v.x, v.y-spatulaHeight*1.5f, transform.position.z);
                                        } else if (Mathf.Approximately(v.y,sliceableObjectTransform.TransformPoint(sliceableObjectTransform.GetComponent<MeshFilter>().sharedMesh.bounds.min).y)){
                                            targetPos = new Vector3(v.x, v.y+spatulaHeight/2, transform.position.z);
                                        } else
                                        targetPos = new Vector3(v.x, v.y, transform.position.z);
                                    }

                                    moveVector = targetPos - transform.position;
                                    moveState = MoveState.CarriageReturn;
                                    shouldCarriageReturn = false;

                                    speed = scrapeSpeed * carriageReturnSpeedMul;
                                }*/
                break;
            case MoveState.Tab:
                if (gameController.CurSliceableObjectScript.shouldCrumble)
                {
                    gameController.CutWSpatula(gameController.curSliceableObject.transform.GetChild(0).gameObject);
                }

                targetPos = new Vector3(transform.position.x, transform.position.y, targetZ);
                moveVector = targetPos - transform.position;
                moveState = MoveState.Scrape;

                speed = scrapeSpeed;
                break;
            case MoveState.CarriageReturn:
                EnableColliders(true);
                fragmentsCollider.enabled = gameController.CurSliceableObjectScript.shouldCrumble;
                if (gameController.CurSliceableObjectScript.shouldCrumble)
                    if (gameController.CurSliceableObjectScript.shouldCrumble)
                    {
                        gameController.CutWSpatula(gameController.curSliceableObject.transform.GetChild(0).gameObject);
                    }

                targetPos = new Vector3(transform.position.x, transform.position.y, targetZ);
                moveVector = targetPos - transform.position;
                moveState = MoveState.Scrape;

                speed = scrapeSpeed;
                break;
        }
        speed *= speedUpMul;

        curTargetDistance = prevTargetDistance = 10f;
    }

    public void OnSpeedUpgrade(float speedUpgradeStep)
    {
        scrapeSpeed += speedUpgradeStep;
        RecalculateSpeed();
    }

    private void RecalculateSpeed()
    {
        switch (moveState)
        {
            case MoveState.Scrape:
                speed = scrapeSpeed;
                break;
            case MoveState.Return:
                speed = scrapeSpeed * returnSpeedMul;
                break;
            case MoveState.Tab:
                speed = scrapeSpeed * tabSpeedMul;
                break;
            case MoveState.CarriageReturn:
                speed = scrapeSpeed * carriageReturnSpeedMul;
                break;
        }
        speed *= speedUpMul;

    }

    public bool SetScraperUpgradeToLvl(int lvl)
    {
        if (lvl <= scrapers.Count)
        {
            curScraperInd = lvl - 1;
        }
        else
        {
            curScraperInd = scrapers.Count - 1;
        }
        ScraperInfo prevScraper = curScraper;
        curScraper = scrapers[curScraperInd];//enable the next scraper

        prevScraper.gameObject.SetActive(false);
        curScraper.gameObject.SetActive(true);

        //  curScraper.scraperModel.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0.1f), 0.2f, 0, 0.1f);

        BoxCollider newScraperCollider = curScraper.scraperCollider;
        BoxCollider parentCollider = (BoxCollider)collider;

        parentCollider.center = newScraperCollider.center;//copy the collider values to the parent collider
        parentCollider.size = newScraperCollider.size;

        //maybe activate trails or special effects for the new scraper

        spatulaWidth = newScraperCollider.size.x;

        ScraperInfo sI = curScraper.GetComponent<ScraperInfo>();

        if (sI != null)
        {
            if ((sI.trailTransforms != null) && (sI.trailTransforms.Count > 0))
            {
                for (int i = 0; i < trails.Count; i++)
                {
                    trails[i].transform.position = sI.trailTransforms[i].position;
                }
            }
        }

        if (lvl <= scrapers.Count)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public bool OnScraperUpgrade()  //returns true if upgrade was succesful, it is not succesful when the max upgrade is reached
    {
        if ((curScraperInd + 1) < scrapers.Count)
        {
            curScraperInd++;
        }
        else return false;



        //   curScraper.gameObject.SetActive(false);//disable the current scraper


        ScraperInfo prevScraper = curScraper;
        curScraper = scrapers[curScraperInd];//enable the next scraper

        prevScraper.gameObject.SetActive(false);
        curScraper.gameObject.SetActive(true);

        curScraper.scraperModel.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0.1f), 0.2f, 0, 0.1f);


        //   curScraper.gameObject.SetActive(true);

        BoxCollider newScraperCollider = curScraper.scraperCollider;
        BoxCollider parentCollider = (BoxCollider)collider;

        parentCollider.center = newScraperCollider.center;//copy the collider values to the parent collider
        parentCollider.size = newScraperCollider.size;

        //maybe activate trails or special effects for the new scraper

        spatulaWidth = newScraperCollider.size.x;

        ScraperInfo sI = curScraper.GetComponent<ScraperInfo>();

        if (sI != null)
        {
            if ((sI.trailTransforms != null) && (sI.trailTransforms.Count > 0))
            {
                for (int i = 0; i < trails.Count; i++)
                {
                    trails[i].transform.position = sI.trailTransforms[i].position;
                }
            }
        }



        //        Debug.Log("spatulaWidth=" + spatulaWidth);
        return true;
    }

    /*    private Vector3 FindScrapePointBasedOnMinY(Collider col)
        {
            Vector3 closestPoint = col.bounds.ClosestPoint(new Vector3(float.MaxValue, col.bounds.min.y, col.bounds.center.z));

            // gizmosSpherePos = closestPoint;
            if (!col.bounds.Contains(closestPoint))
            // if (!col.GetComponent<Renderer>().bounds.Contains(col.transform.InverseTransformPoint(closestPoint)))
            {
                Ray ray = new Ray(new Vector3(20f, col.bounds.min.y, col.bounds.center.z), Vector3.left);//hard coded left direction
                RaycastHit raycastHit;
                // Physics.Raycast()
                if (Physics.Raycast(ray, out raycastHit, Mathf.Infinity))
                {
                    Debug.Log("Raycast hit.");
                    gizmosSpherePos = raycastHit.point;
                    return raycastHit.point;
                }
                else
                {
                    Debug.Log("Raycast didn't hit.");
                    return Vector3.zero;
                };
            }
            else
            {
                Debug.Log("Raycast wasn't cast. Returning closest point.");
                return closestPoint;
            }
        } */

    private void OnDrawGizmos()
    {
        if (gizmosSpherePos != Vector3.zero)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(gizmosSpherePos, 0.05f);
        }
        if (gizmosBoxCenter != Vector3.negativeInfinity)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(gizmosBoxCenter, gizmosBoxSize);
        }

        if (gizmosPlaneCutPosition != Vector3.zero)
        {
            Gizmos.color = new Color(0, 0, 1, 0.5f);
            Gizmos.DrawCube(gizmosPlaneCutPosition, new Vector3(2, 0, 2));
        }

    }

    public bool IsInStartingPos()
    {
        if (Mathf.Abs(startingPos.z - transform.position.z) < 0.1f)
        {
            return true;
        }
        return false;
    }
}
