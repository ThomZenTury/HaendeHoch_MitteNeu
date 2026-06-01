using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // <- required

public class GearRotations : MonoBehaviour
{

    public Transform gearSet;
    public Transform drivingGear;
    public Transform targetGear1;
    public Transform targetGear2;
    public Transform targetGear3;
    public Transform planetaryGear;

    public double radians; // angle between each gear in radians 
    public Vector3 midPoint;

    [Header("OSC Rotation")]
    public float moveToDegree = 360f;    // desired angle to move to
    public float duration = 5f;          // seconds

    Quaternion startLeftRotation; // temporary state of left main gears rotation
    Quaternion startRightRotation; // temporary state of right main gears rotation
    float elapsed; // for time calculation
    // bool running; // replaced with autorotate

    [Header("Gears Settings")]
    [field: SerializeField]
    public bool debug { get; set; } = false; // debug mode, when true, the rotation of the main gear can be manually adjusted in the Inspector for testing and debugging purposes, can be removed later
    [field: SerializeField]
    public bool autoRotate { get; set; } = false; // debug mode, when true, the rotation of the main gear can be manually adjusted in the Inspector for testing and debugging purposes, can be removed later

    [field: SerializeField]
    public bool driveGearStill { get; set; } = false; // debug mode, when true, the rotation of the main gear can be manually adjusted in the Inspector for testing and debugging purposes, can be removed later

    [field: SerializeField]
    public float rotationSpeed { get; set; } = 50.0f; // debug mode, when true, the rotation of the main gear can be manually adjusted in the Inspector for testing and debugging purposes, can be removed later

    [field: SerializeField] public float masterRotation { get; set; } = 0.0f; // to rotate the whole gear set on y-axis
    [field: SerializeField] public float circleRot { get; set; } = 0.0f; // 
    // [field: SerializeField] public float ausgleichIn { get; set; } = 0.0f; // 

    [field: SerializeField]
    public int num { get; set; } = 3; // number of gears, used to calculate the angle between each gear, can be adjusted in the Inspector for testing and debugging purposes, can be removed later

    // you have to use [field: SerializeField] to make the variable work in Inspector
    // [field: SerializeField]
    // public float mainRot { get; set; } = 0.0f; // rotation of the main gear, updated in real time in the Inspector, can be used for debugging and testing

    // [field: SerializeField]
    // public float rotOff1 { get; set; } = 180f; // teeth align value

    // [field: SerializeField]
    // public float rotOff2 { get; set; } = 0.0f; // teeth align value, 264.5f it is in still, but offset is calculated based on the current angle of the main gear, so it will change in real time as the main gear rotates

    // [field: SerializeField]
    // public float rotOff3 { get; set; } = 0.0f; // teeth align value

    // private double initR = 0.05; // initial radius for two turn spiral, calculated from the 3D model in Blender
    [field: SerializeField]
    // private double initR { get; set; } = 0.146415; // initial radius for two turn spiral, calculated from the 3D model in Blender, updated on 2024-06-17 after adjusting the 3D model in Blender to better fit the exponential function
    private double initR { get; set; } = 0.1575; // initial radius for two turn spiral, calculated from the 3D model in Blender, updated on 2024-06-17 after adjusting the 3D model in Blender to better fit the exponential function

    [field: SerializeField]
    private double expansion { get; set; } = 0.1641; // original value measured was 0.171
    private double distAB = 0.598689; // distance between centers of the two gears, calculated from the 3D model in Blender
    float fixedRotAngle;
    float gear2Angle;



    public double CalculateR(double a, double b, double theta)
    {
        double radians = theta * Mathf.Deg2Rad; // convert degrees to radians
        return a * Math.Exp(b * radians); // calculate the radius based on the exponential function
    }

    public double CalculateTheta(double r, double c, double d)
    {
        // double radians = ((Math.Log(r) - Math.Log(a)) / b) * Math.PI * 2; // calculate the angle in radians based on the inverse of the exponential function
        double radians = ((Math.Log(r) - Math.Log(c)) / d); // calculate the angle in radians based on the inverse of the exponential function
        return radians * Mathf.Rad2Deg; // convert radians back to degrees
        // return (Math.Log(r) - Math.Log(a)) / b;
    }

    // public float CalculateR(float a, float b, float theta)
    // {
    //     return a * Math.Exp(b * theta);
    // }

    // float CalculateTheta(float r, float a, float b)
    // {
    //     return (Math.Log(r) - Math.Log(a)) / b;
    // }

    // 
    public double calculateNewAngle(double a, double b, double theta, double c, float offset)
    {
        //2. calculate current radius of main gear:
        // double rA = CalculateR(initR, expansion, currAngle); // initR=a, expansion=b, currAngle=theta
        double rA = CalculateR(a, b, theta);

        // 3. distAB defined on top in variables

        // 4. get radius for second gear
        double rB = distAB - rA;

        //5. calculate angle for second gear, covert double to float using Convert.ToSingle()) 
        // float newAngle = Convert.ToSingle(CalculateTheta(rB, initR, expansion)); // calculate the angle for the second gear based on its radius using the inverse of the exponential function
        float newAngle = Convert.ToSingle(CalculateTheta(rB, c, b)); // r a b

        // double newAngle = CalculateTheta(rB, initR, expansion); // calculate the angle for the second gear based on its radius using the inverse of the exponential function
        float newPlusOffset = newAngle + offset; // calculate the angle for the second gear based on the angle of the first gear, but in the opposite direction and with an offset
                                                 // float newAngOff = newAngle + rotOffset; // calculate the angle for the second gear based on the angle of the first gear, but in the opposite direction and with an offset
        if (debug)
        {
            // Debug.Log("currAngle: " + currAngle);
            // Debug.Log("rA: " + rA);
            // Debug.Log("rB: " + rB);
            // Debug.Log("newAngle: " + newAngle);
            // Debug.Log("rotOffset1: " + rotOff1);
            // Debug.Log("newPlusOffset1: " + newPlusOffset);
            // Debug.Log("test: " + test);
        }

        return newPlusOffset;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // initialAngle = drivingGear.rotation.eulerAngles; // get the initial rotation of the driving gear

        // masterRotation = -transform.rotation.eulerAngles.y;
        if (gearSet.name == "GearSetLeft") masterRotation = -SceneTransition.crossMasterRotationLeft; // set the initial master rotation for the left gear set based on the value from SceneTransition,
        if (gearSet.name == "GearSetRight") masterRotation = -SceneTransition.crossMasterRotationRight; // set the initial master rotation for the right gear set based on the value from SceneTransition,

        // for positioning the gears
        radians = 2 * Math.PI / num; // angle between each gear in radians 
        midPoint = drivingGear.position; // get the position of the driving gear, which is the center point for placing the target gears

        if (gearSet.name == "GearSetLeft") gearSet.rotation = Quaternion.Euler(0, -masterRotation, 0); // set the initial rotation of the whole gear set to 0, can be adjusted in the Inspector for testing and debugging purposes,
        // rotateLeftGearSet(masterRotation, radians, midPoint, new Quaternion(0, 0, 0, 0)); // initialize the position and rotation of the target gears based on the initial rotation of the main gear, can be adjusted in the Inspector for testing and debugging purposes
        rotateLeftGearSet(masterRotation, radians, midPoint, Quaternion.Euler(0, -masterRotation, 0)); // initialize the position and rotation of the target gears based on the initial rotation of the main gear, can be adjusted in the Inspector for testing and debugging purposes

        // flip the right gear to make it look inwards
        if (gearSet.name == "GearSetRight") gearSet.rotation = Quaternion.Euler(0, -masterRotation, 0); // set the initial rotation of the whole gear set to 0, can be adjusted in the Inspector for testing and debugging purposes,
        rotateRightGearSet(masterRotation, radians, midPoint, Quaternion.Euler(0, -masterRotation, 0)); // initialize the position and rotation of the target gears based on the initial rotation of the main gear, can be adjusted in the Inspector for testing and debugging purposes

        // Debug.Log("masterRot= " + masterRotation);
        // Debug.Log("radians= " + Mathf.Rad2Deg * radians);
        // Debug.Log("midPoint= " + midPoint);



    }

    // Update is called once per frame
    void Update()
    {
        // masterRotation = -transform.rotation.eulerAngles.y;
        if (gearSet.name == "GearSetLeft") masterRotation = -SceneTransition.crossMasterRotationLeft; // set the initial master rotation for the left gear set based on the value from SceneTransition,
        if (gearSet.name == "GearSetRight") masterRotation = -SceneTransition.crossMasterRotationRight; // set the initial master rotation for the right gear set based on the value from SceneTransition,

        //transform.Rotate(0, -masterRotation, 0, Space.World);

        // for positioning the gears
        double radians = 2 * Math.PI / num; // angle between each gear in radians 
        Vector3 midPoint = drivingGear.position; // get the position of the driving gear, which is the center point for placing the target gears
        // float masterRotation = gearSet.rotation.eulerAngles.y;
        // float masterRotation = masterRotation;

        //////////////////////////////////////////////// auto rotate or manual rotation ////////////////////////////////////////////////
        if (autoRotate)
        {
            //////////////////////////////////////////////// drive gear rotates -> LEFT gear set
            if (!driveGearStill)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                // current angle in degrees (handles values > 360)
                float angleNow = Mathf.Lerp(0f, moveToDegree, t);

                // apply rotation relative to the start rotation
                // Quaternion angle2apply = startLeftRotation * Quaternion.AngleAxis(angleNow, Vector3.back.normalized);

                // rotate everything
                rotateLeftGearSet(masterRotation, radians, midPoint, startLeftRotation * Quaternion.AngleAxis(angleNow, Vector3.back.normalized));

                if (t >= 1f)
                {
                    autoRotate = false;
                    GameObject.Find("EventSystem").GetComponent<UIKeyboard>().toggleLeftGears = false;
                }

                // float angleNow = drivingGear.rotation.y;
                // Debug.Log(transform.rotation.eulerAngles.z);
                // Debug.Log(rotationSpeed * Time.deltaTime);
                // if (angleNow - startAngle <= moveToAngle)
                // rotateLeftGearSet(masterRotation, radians, midPoint, rotationSpeed * Time.deltaTime);
            }
            //////////////////////////////////////////////// drive gear does not rotate -> RIGHT gear set
            else // drive gear does not rotate, target gears rotate around drive gear
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                // current angle in degrees (handles values > 360)
                float angleNow = Mathf.Lerp(0f, moveToDegree, t);

                // rotate the planetary gear accordingly but add/subtract its on rotation to match the drillholes
                planetaryGear.rotation = (startRightRotation * Quaternion.Euler(0, 0, 30f)) * Quaternion.AngleAxis(-angleNow, Vector3.back.normalized);
                // apply rotation relative to the start rotation
                // Quaternion angle2apply = startRightRotation * Quaternion.AngleAxis(angleNow, Vector3.back.normalized);

                // rotate everything
                rotateRightGearSet(masterRotation, radians, midPoint, startRightRotation * Quaternion.AngleAxis(angleNow, Vector3.back.normalized));

                if (t >= 1f)
                {
                    autoRotate = false;
                    GameObject.Find("EventSystem").GetComponent<UIKeyboard>().toggleRightGears = false;
                }

                // rotateRightGearSet(masterRotation, radians, midPoint, rotationSpeed * Time.deltaTime);

            }
        }
        //////////////////////////////////////////////// manual rotation ////////////////////////////////////////////////
        else
        {

            //////////////////////////////////////////////// drive gear rotates -> LEFT gear set
            if (!driveGearStill)
            {

                // set all necessary vars for autorotation of left gear here (see above in autorotation code)
                if (startLeftRotation != transform.rotation) startLeftRotation = transform.rotation;
                elapsed = 0f;

                // drivingGear.rotation = Quaternion.Euler(0, 0, mainRot);

                //     double add = circleRot * Mathf.Deg2Rad; // add the rotation of the circle to the angle between each gear, so that the gears can be rotated as a group by adjusting the circleRot variable in the Inspector for testing and debugging purposes, can be removed later
                //     // position the gears on a circle with radius equal to the distance between the centers of the two gears, so that they are touching each other at the start, and set their initial rotation based on the initial angle of the main gear and the offset values
                //     Vector3 pos1 = new Vector3(Convert.ToSingle(distAB * Math.Cos(radians + add)) + midPoint.x, Convert.ToSingle(distAB * Math.Sin(radians + add)) + midPoint.y, 0); // place target gear 1 at the correct position based on the distance between the centers of the two gears and the angle of the main gear
                //     Vector3 pos2 = new Vector3(Convert.ToSingle(distAB * Math.Cos(radians * 2 + add)) + midPoint.x, Convert.ToSingle(distAB * Math.Sin(radians * 2 + add)) + midPoint.y, 0); // place target gear 2 at the correct position based on the distance between the centers of the two gears and the angle of the main gear
                //     Vector3 pos3 = new Vector3(Convert.ToSingle(distAB * Math.Cos(radians * 3 + add)) + midPoint.x, Convert.ToSingle(distAB * Math.Sin(radians * 3 + add)) + midPoint.y, 0); // place target gear 3 at the correct position based on the distance between the centers of the two gears and the angle of the main gear

                //     // Apply y-rotation to the whole gear set
                //     float masterRotRad = masterRotation * Mathf.Deg2Rad;
                //     float cosTheta = Mathf.Cos(masterRotRad);
                //     float sinTheta = Mathf.Sin(masterRotRad);
                //     Vector3 offset1 = pos1 - midPoint;
                //     Vector3 rotatedOffset1 = new Vector3(offset1.x * cosTheta - offset1.z * sinTheta, offset1.y, offset1.x * sinTheta + offset1.z * cosTheta);
                //     targetGear1.position = midPoint + rotatedOffset1;

                //     Vector3 offset2 = pos2 - midPoint;
                //     Vector3 rotatedOffset2 = new Vector3(offset2.x * cosTheta - offset2.z * sinTheta, offset2.y, offset2.x * sinTheta + offset2.z * cosTheta);
                //     targetGear2.position = midPoint + rotatedOffset2;

                //     Vector3 offset3 = pos3 - midPoint;
                //     Vector3 rotatedOffset3 = new Vector3(offset3.x * cosTheta - offset3.z * sinTheta, offset3.y, offset3.x * sinTheta + offset3.z * cosTheta);
                //     targetGear3.position = midPoint + rotatedOffset3;

                //     //1. get CurrentAngle of main gear
                //     double currAngle1 = Convert.ToDouble(transform.rotation.eulerAngles.z); // get the rotation of the driving gear
                //     double currAngle2 = Convert.ToDouble(transform.rotation.eulerAngles.z); // get the rotation of the driving gear
                //     double currAngle3 = Convert.ToDouble(transform.rotation.eulerAngles.z); // get the rotation of the driving gear

                //     float off1 = Convert.ToSingle(radians) * Mathf.Rad2Deg;
                //     float off2 = Convert.ToSingle(radians * 2) * Mathf.Rad2Deg;
                //     float off3 = Convert.ToSingle(radians * 3) * Mathf.Rad2Deg;

                //     // gearAngles for speed calc !!! I don't understand these calcs, has to do sth. with the initial rotation of 180° I think
                //     float gear1Angle = 180f - Convert.ToSingle(radians) * Mathf.Rad2Deg; // angle for gear 1 based on the number of gears, can be adjusted in the Inspector for testing and debugging purposes, can be removed later
                //     if (num == 2)
                //     {
                //         gear2Angle = 180f; // angle for gear 3 based on the number of gears, can be adjusted in the Inspector for testing and
                //     }
                //     else if (num == 3)
                //     {
                //         gear2Angle = 180f + Convert.ToSingle(radians) * Mathf.Rad2Deg; // angle for gear 2 based on the number of gears, can be adjusted in the Inspector for testing and debugging purposes, can be removed later
                //     }

                //     float gear3Angle = Convert.ToSingle(radians * 3 / 2) * Mathf.Rad2Deg; // angle for gear 3 based on the number of gears, can be adjusted in the Inspector for testing and

                //     // Debug.Log(gear1Angle);
                //     currAngle1 = (currAngle1 + gear1Angle) % 360; // adjust the current angle of the main gear by adding the offset value, and take the modulus to keep it within 0-360 degrees
                //     currAngle2 = (currAngle2 + gear2Angle) % 360; // adjust the current angle of the main gear by adding the offset value, and take the modulus to keep it within 0-360 degrees
                //     currAngle3 = (currAngle3 + gear3Angle) % 360; // adjust the current angle of the main gear by adding the offset value, and take the modulus to keep it within 0-360 degrees


                //     // steps 2-5 are all done in the calculateNewAngle function, which takes in the current angle of the main gear, the initial radius, and the expansion rates, and returns the new angle for the target gear
                //     // float targetAngle1 = Convert.ToSingle(calculateNewAngle(initR, expansion, currAngle1, initR, Convert.ToSingle(2 * Math.PI / num))); // calculate the new angle for the target gear based on the current angle of the main gear, the initial radius, and the expansion rates
                //     float targetAngle1 = Convert.ToSingle(calculateNewAngle(initR, expansion, currAngle1, initR, off1)); // calculate the new angle for the target gear based on the current angle of the main gear, the initial radius, and the expansion rates
                //     float targetAngle2 = Convert.ToSingle(calculateNewAngle(initR, expansion, currAngle2, initR, off2)); // calculate the new angle for the target gear based on the current angle of the main gear, the initial radius, and the expansion rates
                //     float targetAngle3 = Convert.ToSingle(calculateNewAngle(initR, expansion, currAngle3, initR, off3)); // calculate the new angle for the target gear based on the current angle of the main gear, the initial radius, and the expansion rates

                //     // 6. set target gear's rotation to calculated angle
                //     targetGear1.rotation = Quaternion.Euler(0, -masterRotation, targetAngle1); // set rotation of target gear 1 to the calculated angle + offset angle
                //     targetGear2.rotation = Quaternion.Euler(0, -masterRotation, targetAngle2); // set rotation of target gear 1 to the calculated angle + offset angle
                //     targetGear3.rotation = Quaternion.Euler(0, -masterRotation, targetAngle3); // set rotation of target gear 1 to the calculated angle + offset angle

                // }
                // else
                // {
                //     // position target gears around driveGear midpoint based on manual circle rotaion angle
                //     double add = circleRot * Mathf.Deg2Rad; // add the rotation of the circle to the angle between each gear, so that the gears can be rotated as a group by adjusting the circleRot variable in the Inspector for testing and debugging purposes, can be removed later
                //     // position the gears on a circle with radius equal to the distance between the centers of the two gears, so that they are touching each other at the start, and set their initial rotation based on the initial angle of the main gear and the offset values
                //     Vector3 pos1 = new Vector3(Convert.ToSingle(distAB * Math.Cos(radians + add)) + midPoint.x, Convert.ToSingle(distAB * Math.Sin(radians + add)) + midPoint.y, 0); // place target gear 1 at the correct position based on the distance between the centers of the two gears and the angle of the main gear
                //     Vector3 pos2 = new Vector3(Convert.ToSingle(distAB * Math.Cos(radians * 2 + add)) + midPoint.x, Convert.ToSingle(distAB * Math.Sin(radians * 2 + add)) + midPoint.y, 0); // place target gear 2 at the correct position based on the distance between the centers of the two gears and the angle of the main gear
                //     Vector3 pos3 = new Vector3(Convert.ToSingle(distAB * Math.Cos(radians * 3 + add)) + midPoint.x, Convert.ToSingle(distAB * Math.Sin(radians * 3 + add)) + midPoint.y, 0); // place target gear 3 at the correct position based on the distance between the centers of the two gears and the angle of the main gear

                //     // Apply y-rotation to the whole gear set
                //     float masterRotRad = masterRotation * Mathf.Deg2Rad;
                //     float cosTheta = Mathf.Cos(masterRotRad);
                //     float sinTheta = Mathf.Sin(masterRotRad);
                //     Vector3 offset1 = pos1 - midPoint;
                //     Vector3 rotatedOffset1 = new Vector3(offset1.x * cosTheta - offset1.z * sinTheta, offset1.y, offset1.x * sinTheta + offset1.z * cosTheta);
                //     targetGear1.position = midPoint + rotatedOffset1;

                //     Vector3 offset2 = pos2 - midPoint;
                //     Vector3 rotatedOffset2 = new Vector3(offset2.x * cosTheta - offset2.z * sinTheta, offset2.y, offset2.x * sinTheta + offset2.z * cosTheta);
                //     targetGear2.position = midPoint + rotatedOffset2;

                //     Vector3 offset3 = pos3 - midPoint;
                //     Vector3 rotatedOffset3 = new Vector3(offset3.x * cosTheta - offset3.z * sinTheta, offset3.y, offset3.x * sinTheta + offset3.z * cosTheta);
                //     targetGear3.position = midPoint + rotatedOffset3;
            }
            else
            {
                if (startRightRotation != transform.rotation) startRightRotation = transform.rotation;
                elapsed = 0f;
            }
        }




        // Debug.Log("Gear1 z-rot: " + targetGear1.rotation.eulerAngles.z);
    }


    ///////////////////////// function to rotate the left gear set based on the rotation of the drive gear, the angle between each gear, and the position of the drive gear, can be called in the Update function for auto rotation, or can be called in a separate function for manual rotation, not finished yet
    public void rotateLeftGearSet(float masterRotation, double radians, Vector3 midPoint, Quaternion currentAngle)
    {

        // rotate drive Gear
        // transform.Rotate(Vector3.back, currentAngle); // next line for use with duration and move2degree variables, needs Quaternion input
        // transform.rotation = startLeftRotation * Quaternion.AngleAxis(currentAngle, Vector3.back.normalized);
        transform.rotation = currentAngle;
        // position target gears around driveGear midpoint based on manual circle rotaion angle
        double add = circleRot * Mathf.Deg2Rad; // add the rotation of the circle to the angle between each gear, so that the gears can be rotated as a group by adjusting the circleRot variable in the Inspector for testing and debugging purposes, can be removed later
                                                // double ausgleich = ausgleichIn * Mathf.Deg2Rad; // add the rotation of the circle to the angle between each gear, so that the gears can be rotated as a group by adjusting the circleRot variable in the Inspector for testing and debugging purposes, can be removed later
        float masterRotRad = masterRotation * Mathf.Deg2Rad; // convert master rotation to radians for use in positioning the target gears, can be adjusted in the Inspector for testing and debugging purposes, can be removed later
                                                             // position the gears on a circle with radius equal to the distance between the centers of the two gears, so that they are touching each other at the start, and set their initial rotation based on the initial angle of the main gear and the offset values
        Vector3 pos1 = new Vector3(Convert.ToSingle(distAB * Math.Cos(radians + add)) + midPoint.x, Convert.ToSingle(distAB * Math.Sin(radians + add)) + midPoint.y, 0); // place target gear 1 at the correct position based on the distance between the centers of the two gears and the angle of the main gear
        Vector3 pos2 = new Vector3(Convert.ToSingle(distAB * Math.Cos(radians * 2 + add)) + midPoint.x, Convert.ToSingle(distAB * Math.Sin(radians * 2 + add)) + midPoint.y, 0); // place target gear 2 at the correct position based on the distance between the centers of the two gears and the angle of the main gear
        Vector3 pos3 = new Vector3(Convert.ToSingle(distAB * Math.Cos(radians * 3 + add)) + midPoint.x, Convert.ToSingle(distAB * Math.Sin(radians * 3 + add)) + midPoint.y, 0); // place target gear 3 at the correct position based on the distance between the centers of the two gears and the angle of the main gear

        // Apply y-rotation to the whole gear set
        float cosTheta = Mathf.Cos(masterRotRad);
        float sinTheta = Mathf.Sin(masterRotRad);
        Vector3 offset1 = pos1 - midPoint;
        Vector3 rotatedOffset1 = new Vector3(offset1.x * cosTheta - offset1.z * sinTheta, offset1.y, offset1.x * sinTheta + offset1.z * cosTheta);
        targetGear1.position = midPoint + rotatedOffset1;

        Vector3 offset2 = pos2 - midPoint;
        Vector3 rotatedOffset2 = new Vector3(offset2.x * cosTheta - offset2.z * sinTheta, offset2.y, offset2.x * sinTheta + offset2.z * cosTheta);
        targetGear2.position = midPoint + rotatedOffset2;

        Vector3 offset3 = pos3 - midPoint;
        Vector3 rotatedOffset3 = new Vector3(offset3.x * cosTheta - offset3.z * sinTheta, offset3.y, offset3.x * sinTheta + offset3.z * cosTheta);
        targetGear3.position = midPoint + rotatedOffset3;

        //1. get CurrentAngle of main gear
        double currAngle1 = Convert.ToDouble(transform.rotation.eulerAngles.z); // get the rotation of the driving gear
        double currAngle2 = Convert.ToDouble(transform.rotation.eulerAngles.z); // get the rotation of the driving gear
        double currAngle3 = Convert.ToDouble(transform.rotation.eulerAngles.z); // get the rotation of the driving gear

        float off1 = Convert.ToSingle(radians) * Mathf.Rad2Deg;
        float off2 = Convert.ToSingle(radians * 2) * Mathf.Rad2Deg;
        float off3 = Convert.ToSingle(radians * 3) * Mathf.Rad2Deg;

        // gearAngles for speed calc !!! I don't understand these calcs, has to do sth. with the initial rotation of 180° I think
        float gear1Angle = 180f - Convert.ToSingle(radians) * Mathf.Rad2Deg; // angle for gear 1 based on the number of gears, can be adjusted in the Inspector for testing and debugging purposes, can be removed later
        if (num == 2)
        {
            gear2Angle = 180f; // angle for gear 3 based on the number of gears, can be adjusted in the Inspector for testing and
        }
        else if (num == 3)
        {
            gear2Angle = 180f + Convert.ToSingle(radians) * Mathf.Rad2Deg; // angle for gear 2 based on the number of gears, can be adjusted in the Inspector for testing and debugging purposes, can be removed later
        }

        float gear3Angle = Convert.ToSingle(radians * 3 / 2) * Mathf.Rad2Deg; // angle for gear 3 based on the number of gears, can be adjusted in the Inspector for testing and

        // Debug.Log(gear1Angle);
        currAngle1 = (currAngle1 + gear1Angle) % 360; // adjust the current angle of the main gear by adding the offset value, and take the modulus to keep it within 0-360 degrees
        currAngle2 = (currAngle2 + gear2Angle) % 360; // adjust the current angle of the main gear by adding the offset value, and take the modulus to keep it within 0-360 degrees
        currAngle3 = (currAngle3 + gear3Angle) % 360; // adjust the current angle of the main gear by adding the offset value, and take the modulus to keep it within 0-360 degrees


        // steps 2-5 are all done in the calculateNewAngle function, which takes in the current angle of the main gear, the initial radius, and the expansion rates, and returns the new angle for the target gear
        // float targetAngle1 = Convert.ToSingle(calculateNewAngle(initR, expansion, currAngle1, initR, Convert.ToSingle(2 * Math.PI / num))); // calculate the new angle for the target gear based on the current angle of the main gear, the initial radius, and the expansion rates
        float targetAngle1 = Convert.ToSingle(calculateNewAngle(initR, expansion, currAngle1, initR, off1)); // calculate the new angle for the target gear based on the current angle of the main gear, the initial radius, and the expansion rates
        float targetAngle2 = Convert.ToSingle(calculateNewAngle(initR, expansion, currAngle2, initR, off2)); // calculate the new angle for the target gear based on the current angle of the main gear, the initial radius, and the expansion rates
        float targetAngle3 = Convert.ToSingle(calculateNewAngle(initR, expansion, currAngle3, initR, off3)); // calculate the new angle for the target gear based on the current angle of the main gear, the initial radius, and the expansion rates

        // 6. set target gear's rotation to calculated angle
        // targetGear1.rotation = Quaternion.Euler(0, 0, newAngle); // set rotation of target gear 1 to the calculated angle + offset angle
        // targetGear1.rotation = Quaternion.Euler(0, 0, targetAngle1); // set rotation of target gear 1 to the calculated angle + offset angle
        // targetGear2.rotation = Quaternion.Euler(0, 0, targetAngle2); // set rotation of target gear 1 to the calculated angle + offset angle
        // targetGear3.rotation = Quaternion.Euler(0, 0, targetAngle3); // set rotation of target gear 1 to the calculated angle + offset angle

        // variant for turning whole gear set around, not finished yet
        targetGear1.rotation = Quaternion.Euler(0, -masterRotation, targetAngle1); // set rotation of target gear 1 to the calculated angle + offset angle
        targetGear2.rotation = Quaternion.Euler(0, -masterRotation, targetAngle2); // set rotation of target gear 1 to the calculated angle + offset angle
        targetGear3.rotation = Quaternion.Euler(0, -masterRotation, targetAngle3); // set rotation of target gear 1 to the calculated angle + offset angle

    }

    public void rotateRightGearSet(float masterRotation, double radians, Vector3 midPoint, Quaternion currentAngle)
    {

        // let target gears  auto rotate around driveGear midpoint, hold the drive gear fixed in place
        float currentAngleRad = currentAngle.eulerAngles.z * Mathf.Deg2Rad;
        fixedRotAngle = currentAngleRad; // use the absolute current angle rather than accumulating it, so the orbit speed stays constant
                                         // position the gears on a circle with radius equal to the distance between the centers of the two gears, so that they are touching each other at the start, and set their initial rotation based on the initial angle of the main gear and the offset values
        Vector3 pos1 = new Vector3(Convert.ToSingle(distAB * Math.Cos(radians - fixedRotAngle)) + midPoint.x, Convert.ToSingle(distAB * Math.Sin(radians - fixedRotAngle)) + midPoint.y, 0); // place target gear 1 at the correct position based on the distance between the centers of the two gears and the angle of the main gear
        Vector3 pos2 = new Vector3(Convert.ToSingle(distAB * Math.Cos(radians * 2 - fixedRotAngle)) + midPoint.x, Convert.ToSingle(distAB * Math.Sin(radians * 2 - fixedRotAngle)) + midPoint.y, 0); // place target gear 2 at the correct position based on the distance between the centers of the two gears and the angle of the main gear
        Vector3 pos3 = new Vector3(Convert.ToSingle(distAB * Math.Cos(radians * 3 - fixedRotAngle)) + midPoint.x, Convert.ToSingle(distAB * Math.Sin(radians * 3 - fixedRotAngle)) + midPoint.y, 0); // place target gear 3 at the correct position based on the distance between the centers of the two gears and the angle of the main gear

        // Apply y-rotation to the whole gear set
        float masterRotRad = masterRotation * Mathf.Deg2Rad; // convert master rotation to radians for use in positioning the target gears, can be adjusted in the Inspector for testing and debugging purposes, can be removed later
        float cosTheta = Mathf.Cos(masterRotRad);
        float sinTheta = Mathf.Sin(masterRotRad);
        Vector3 offset1 = pos1 - midPoint;
        Vector3 rotatedOffset1 = new Vector3(offset1.x * cosTheta - offset1.z * sinTheta, offset1.y, offset1.x * sinTheta + offset1.z * cosTheta);
        targetGear1.position = midPoint + rotatedOffset1;

        Vector3 offset2 = pos2 - midPoint;
        Vector3 rotatedOffset2 = new Vector3(offset2.x * cosTheta - offset2.z * sinTheta, offset2.y, offset2.x * sinTheta + offset2.z * cosTheta);
        targetGear2.position = midPoint + rotatedOffset2;

        Vector3 offset3 = pos3 - midPoint;
        Vector3 rotatedOffset3 = new Vector3(offset3.x * cosTheta - offset3.z * sinTheta, offset3.y, offset3.x * sinTheta + offset3.z * cosTheta);
        targetGear3.position = midPoint + rotatedOffset3;

        // Calculate self-rotation based on the orbiting angle (treating it as the drive gear's effective rotation)
        // double currAngle1 = (radians + fixedRotAngle) * Mathf.Rad2Deg; // get the effective rotation angle for gear 1
        double currAngle1 = fixedRotAngle * Mathf.Rad2Deg; // get the effective rotation angle for gear 1
        double currAngle2 = fixedRotAngle * Mathf.Rad2Deg; // get the effective rotation angle for gear 2
        double currAngle3 = (radians * 3 + fixedRotAngle) * Mathf.Rad2Deg; // get the effective rotation angle for gear 3

        // float off1 = Convert.ToSingle(radians) * Mathf.Rad2Deg - fixedRotAngle;
        float off1 = fixedRotAngle * Mathf.Rad2Deg - Convert.ToSingle(radians) * Mathf.Rad2Deg; // offset for gear 1 based on the effective rotation angle, can be adjusted in the Inspector for testing and debugging purposes, can be removed later
                                                                                                // Debug.Log("off1: " + off1);
        float off2 = fixedRotAngle * Mathf.Rad2Deg - Convert.ToSingle(radians * 2) * Mathf.Rad2Deg;
        // float off2 = rotOff2;
        float off3 = fixedRotAngle * Mathf.Rad2Deg - Convert.ToSingle(radians * 3) * Mathf.Rad2Deg;

        // gearAngles for speed calc !!! I don't understand these calcs, has to do sth. with the initial rotation of 180° I think
        float gear1Angle = 180f - Convert.ToSingle(radians) * Mathf.Rad2Deg; // angle for gear 1 based on the number of gears, can be adjusted in the Inspector for testing and debugging purposes, can be removed later
                                                                             // gear2Angle = 180f; // angle for gear 3 based on the number of gears, can be adjusted in the Inspector for testing and

        if (num == 2)
        {
            gear2Angle = 180f; // angle for gear 3 based on the number of gears, can be adjusted in the Inspector for testing and
        }
        else if (num == 3)
        {
            gear2Angle = 180f + Convert.ToSingle(radians) * Mathf.Rad2Deg; // angle for gear 2 based on the number of gears, can be adjusted in the Inspector for testing and debugging purposes, can be removed later
        }

        float gear3Angle = Convert.ToSingle(radians * 3 / 2) * Mathf.Rad2Deg; // angle for gear 3 based on the number of gears, can be adjusted in the Inspector for testing and

        // Debug.Log(gear1Angle);
        currAngle1 = (currAngle1 + gear1Angle) % 360; // adjust the current angle of the main gear by adding the offset value, and take the modulus to keep it within 0-360 degrees
        currAngle2 = (currAngle2 + gear2Angle) % 360; // adjust the current angle of the main gear by adding the offset value, and take the modulus to keep it within 0-360 degrees
        currAngle3 = (currAngle3 + gear3Angle) % 360; // adjust the current angle of the main gear by adding the offset value, and take the modulus to keep it within 0-360 degrees

        // steps 2-5 are all done in the calculateNewAngle function, which takes in the current angle of the main gear, the initial radius, and the expansion rates, and returns the new angle for the target gear
        float targetAngle1 = Convert.ToSingle(calculateNewAngle(initR, expansion, currAngle1, initR, -off1)); // calculate the new angle for the target gear based on the current angle of the main gear, the initial radius, and the expansion rates
        float targetAngle2 = Convert.ToSingle(calculateNewAngle(initR, expansion, currAngle2, initR, -off2)); // calculate the new angle for the target gear based on the current angle of the main gear, the initial radius, and the expansion rates
        float targetAngle3 = Convert.ToSingle(calculateNewAngle(initR, expansion, currAngle3, initR, -off3)); // calculate the new angle for the target gear based on the current angle of the main gear, the initial radius, and the expansion rates

        // 6. set target gear's rotation to calculated angle
        targetGear1.rotation = Quaternion.Euler(0, -masterRotation, targetAngle1); // set rotation of target gear 1 to the calculated angle + offset angle
        targetGear2.rotation = Quaternion.Euler(0, -masterRotation, targetAngle2); // set rotation of target gear 2 to the calculated angle + offset angle
        targetGear3.rotation = Quaternion.Euler(0, -masterRotation, targetAngle3); // set rotation of target gear 3 to the calculated angle + offset angle

    }

    public void resetGearSet()
    {
        // masterRotation = -transform.rotation.eulerAngles.y;

        // // for positioning the gears
        // double radians = 2 * Math.PI / num; // angle between each gear in radians 
        // Vector3 midPoint = drivingGear.position;

        // rotateLeftGearSet(masterRotation, radians, midPoint, 0.0f); // initialize the position and rotation of the target gears based on the initial rotation of the main gear, can be adjusted in the Inspector for testing and debugging purposes, can be removed later
        // rotateRightGearSet(masterRotation, radians, midPoint, 0.0f); // initialize the position and rotation of the target gears based on the initial rotation of the main gear, can be adjusted in the Inspector for testing and debugging purposes, can be removed later
        // drivingGear.rotation = Quaternion.Euler(initialAngle); // reset the rotation of the driving gear to its initial angle


        // just trying to reload the whole scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        // if PlayerPrefs save and load method is going to be used, also reload the values here
    }
}
