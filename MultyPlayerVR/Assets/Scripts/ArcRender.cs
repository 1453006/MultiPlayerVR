using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ArcRender : MonoBehaviour {

    LineRenderer lr;

    public float velocity;
    public float angle;
    public int resolution;

    float g; // gravity
    float radianAngle;
    public  Transform handDisplay;
    public Transform teleportTarget;
    public Transform avatar;
    Transform player;
    Vector3 destination;
    bool isTransitioning;
    float transitionSpeed = 10f;
    // Use this for initialization
    void Start () {
        lr = this.GetComponent<LineRenderer>();
        g = Mathf.Abs(Physics2D.gravity.y);

        teleportTarget = GameObject.Find("Sphere").transform;
        player = GameObject.Find("Player").transform;
        isTransitioning = false;
    }

    

    // Update is called once per frame
    void Update () {
        if (!avatar.GetComponent<PhotonView>().isMine)
        {
           
            return;
        }
        if (GvrController.IsTouching)
        {
            lr.enabled = true;
            teleportTarget.gameObject.SetActive(true);
            //this.transform.rotation = Quaternion.Euler(0, 0, 0);
            this.transform.rotation = Quaternion.Euler(0, 0, 0);
            this.transform.localRotation = Quaternion.Euler(-(handDisplay.localRotation.eulerAngles.x + avatar.rotation.eulerAngles.x), 0, 0);
         
            angle =  -handDisplay.rotation.eulerAngles.x;
            RenderArc();

            if(GvrController.ClickButtonDown)
            {
                destination = teleportTarget.transform.position; 
                isTransitioning = true;
            }

        }
        else
        {
            lr.enabled = false;
            teleportTarget.gameObject.SetActive(false);
        }

        if(isTransitioning)
        {
            lr.enabled = false;
            Vector3 targetPosition = new Vector3(destination.x, player.position.y, destination.z);
          
            // Animate player to position with linear steps

            player.position = Vector3.MoveTowards(
              avatar.position,
              targetPosition,
              transitionSpeed * Time.deltaTime);

            // Check if transition is finished.
            if (Vector3.Distance(targetPosition,player.transform.position) <= 0.01f)
            {
                isTransitioning = false;
            }
        }
    }

    void RenderArc()
    {
        lr.SetVertexCount(resolution + 1);
        lr.SetPositions(CalcArcArray());
    }

    Vector3[] CalcArcArray()
    {
        Vector3[] arcArray = new Vector3[resolution + 1];

        radianAngle = Mathf.Deg2Rad * angle;
         float maxDistance = (velocity * velocity * Mathf.Sin(2 * radianAngle)) / g;
      
        for( int i = 0; i <= resolution; i++)
        {
            float t = (float)i / resolution;
            arcArray[i] = CalcArcPoint(t,maxDistance);

        }
        //arcArray[resolution] = new Vector3(0, -1, arcArray[resolution - 1].z);
    
        teleportTarget.localPosition = new Vector3(0, arcArray[resolution].y, arcArray[resolution].z-1);

        return arcArray;
    }

    void CheckRayCast(Vector3[] arcArray)
    {
        RaycastHit hit;
        Vector3 fromPosition = arcArray[0];
        Vector3 toPosition = arcArray[resolution];
        Vector3 direction = toPosition - fromPosition;


        if (Physics.Raycast(arcArray[0], direction, out hit))
        {
            print("ray just hit the gameobject: " + hit.collider.gameObject.name);
            Debug.DrawLine(arcArray[0], toPosition,Color.red);
        }
    }
    Vector3 CalcArcPoint(float t, float maxDist)
    {
        float z = t * maxDist;
        float y = -1 * t + z * Mathf.Tan(radianAngle) - ((g * z * z) / (2 * velocity * velocity * Mathf.Cos(radianAngle) * Mathf.Cos(radianAngle)));

        return new Vector3(0,y,z);
    }
}
