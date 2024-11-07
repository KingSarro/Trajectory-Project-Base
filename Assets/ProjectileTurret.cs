using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileTurret : MonoBehaviour{
    [SerializeField] float projectileSpeed = 1;
    [SerializeField] Vector3 gravity = new Vector3(0, -9.8f, 0);
    [SerializeField] LayerMask targetLayer;
    [SerializeField] GameObject crosshair;
    [SerializeField] float baseTurnSpeed = 3;
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] GameObject gun;
    [SerializeField] Transform turretBase;
    [SerializeField] Transform barrelEnd;
    [SerializeField] LineRenderer line;
    [SerializeField] bool useLowAngle;

    List<Vector3> points = new List<Vector3>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        TrackMouse();
        TurnBase();
        RotateGun();

        if (Input.GetButtonDown("Fire1"))
            Fire();
    }

    void Fire()
    {
        GameObject projectile = Instantiate(projectilePrefab, barrelEnd.position, gun.transform.rotation);
        projectile.GetComponent<Rigidbody>().velocity = projectileSpeed * barrelEnd.transform.forward;
    }

    void TrackMouse()
    {
        Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
        if(Physics.Raycast(cameraRay, out hit, 1000, targetLayer))
        {
            crosshair.transform.forward = hit.normal;
            crosshair.transform.position = hit.point + hit.normal * 0.1f;
            //Debug.Log("hit ground");
        }
    }

    void TurnBase()
    {
        Vector3 directionToTarget = (crosshair.transform.position - turretBase.transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToTarget.x, 0, directionToTarget.z));
        turretBase.transform.rotation = Quaternion.Slerp(turretBase.transform.rotation, lookRotation, Time.deltaTime * baseTurnSpeed);
    }

    void RotateGun()
    {
        float? angle = CalculateTrajectory(crosshair.transform.position, useLowAngle);
        if (angle != null)
            gun.transform.localEulerAngles = new Vector3(360f - (float)angle, 0, 0);
    }

    float? CalculateTrajectory(Vector3 target, bool useLow)
    {
        Vector3 targetDir = target - barrelEnd.position;
        
        float y = targetDir.y;
        targetDir.y = 0;

        float x = targetDir.magnitude;

        float v = projectileSpeed;
        float v2 = Mathf.Pow(v, 2);
        float v4 = Mathf.Pow(v, 4);
        float g = gravity.y;
        float x2 = Mathf.Pow(x, 2);

        float underRoot = v4 - g * ((g * x2) + (2 * y * v2));

        if (underRoot >= 0)
        {
            float root = Mathf.Sqrt(underRoot);
            float highAngle = v2 + root;
            float lowAngle = v2 - root;

            //!DrawLine(); //S.S

            if (useLow){
                float theta = (Mathf.Atan2(lowAngle, g * x) * Mathf.Rad2Deg);
                CalculateLine(lowAngle);
                return theta;
            }
            else{
                float theta = (Mathf.Atan2(highAngle, g * x) * Mathf.Rad2Deg);
                CalculateLine(highAngle);
                return theta;
            }
        }
        else
            return null;
    }

        private void DrawLine(){
        Vector3 iPos = barrelEnd.position;
        Vector3 tarPos = crosshair.transform.position;
        float midPoint;

        //Get the mid point // When speed is 0, the is at the highest point
        //midPoint = (float)((iPos.y * projectileSpeed/2) + (.5 * (0 + gravity.y) * (projectileSpeed/2 * projectileSpeed/2)));
        midPoint = (((0 + gravity.y) + 0)/2) * 2;

        

        float x = (iPos.x + tarPos.x) /2;
        //float y = midPoint/2;
        float y = barrelEnd.position.y + (projectileSpeed * Time.deltaTime) + (0.5f * gravity.y * (Time.deltaTime * Time.deltaTime));
        float z = (iPos.z + tarPos.z) / 2;
        

        //float dis = Mathf.Sqrt( ((tarPos.x - iPos.x)*(tarPos.x - iPos.x) + (tarPos.z - iPos.z)*(tarPos.z - iPos.z)) );

        Vector3 midPosition= new Vector3(x, Mathf.Abs(y), z);
        line.SetPosition(0, iPos);
        line.SetPosition(1, midPosition);
        line.SetPosition(2,tarPos);
    }

    private void CalculateLine(float theta){
        float d;
        float v_i;
        float v_f;
        float a;
        float t;

        //Find y
        //!float y = barrelEnd.position.y + (projectileSpeed * Time.deltaTime) + (0.5f * -gravity.y * (Time.deltaTime * Time.deltaTime));
        float V_oy = projectileSpeed * Mathf.Sin(theta);
        float V_y = V_oy - -gravity.y * Time.deltaTime;
        float d_y = barrelEnd.position.y + (V_oy * Time.deltaTime) + (.5f * -gravity.y * (Time.deltaTime *Time.deltaTime));

        //Find x
        //Find z
        float x = (barrelEnd.position.x + crosshair.transform.position.x) / 2;
        float z = (barrelEnd.position.z + crosshair.transform.position.z) / 2;

        Vector3 midPosition = new Vector3(x, d_y, z);

        line.SetPosition(0, barrelEnd.position);
        line.SetPosition(1, midPosition);
        line.SetPosition(2, crosshair.transform.position);



    }//End Method

}
