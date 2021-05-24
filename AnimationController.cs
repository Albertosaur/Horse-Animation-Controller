using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using System;


public class AnimationController : MonoBehaviour
{
    //lists to store the arrays of quaternions
    List<QuatAngles []> quatF;
    List<QuatAngles[]> quatB;
    List<QuatAngles[]> quatH;
    List<QuatAngles[]> quatT;

    public string [] textFile;

    //varaibles for each joint - could be put into a class for better readability
    public float [] speedFR;
    public int [] counterFR;
    bool [] lerpedFR;

    public float[] speedFL;
    public int[] counterFL;
    bool[] lerpedFL;

    public float[] speedBR;
    public int[] counterBR;
    bool[] lerpedBR;

    public float[] speedBL;
    public int[] counterBL;
    bool[] lerpedBL;

    public float[] speedH;
    public int[] counterH;
    bool[] lerpedH;

    public float[] speedT;
    public int[] counterT;
    bool[] lerpedT;

    //transforms of the parent joint of each limb
    public Transform FrontRClavical;
    public Transform FrontLClavical;
    public Transform BackRClavical;
    public Transform BackLClavical;
    public Transform HeadRoot;
    public Transform TailRoot;

    //lists of transforms in each limb
    List<Transform> FrontRlimb;
    List<Transform> FrontLlimb;
    List<Transform> BackRlimb;
    List<Transform> BackLlimb;
    List<Transform> Head;
    List<Transform> Tail;

    // Start is called before the first frame update
    void Start()
    {

        //initialise variables
        quatF = new List<QuatAngles[]>();
        quatB = new List<QuatAngles[]>();
        quatH = new List<QuatAngles[]>();
        quatT = new List<QuatAngles[]>();

        quatF.Add(new QuatAngles[4]);
        quatB.Add(new QuatAngles[4]);
        quatH.Add(new QuatAngles[1]);
        quatT.Add(new QuatAngles[3]);

        lerpedFR = new bool[4];
        lerpedFL = new bool[4];
        lerpedBR = new bool[4];
        lerpedBL = new bool[4];
        lerpedH = new bool[1];
        lerpedT = new bool[3];

        //read in quaternions from json file
        ReadJson(textFile[0], 0, ref quatF);
        ReadJson(textFile[1], 1, ref quatF);
        ReadJson(textFile[2], 2, ref quatF);
        ReadJson(textFile[3], 3, ref quatF);

        ReadJson(textFile[4], 0, ref quatB);
        ReadJson(textFile[5], 1, ref quatB);
        ReadJson(textFile[6], 2, ref quatB);
        ReadJson(textFile[7], 3, ref quatB);

        ReadJson(textFile[8], 0, ref quatH);

        ReadJson(textFile[9], 0, ref quatT);
        ReadJson(textFile[10], 1, ref quatT);
        ReadJson(textFile[11], 2, ref quatT);

        //get transfroms of joints
        FrontRlimb = new List<Transform>();
        FrontRlimb.Add(FrontRClavical);
        recurisive(FrontRClavical, ref FrontRlimb);

        FrontLlimb = new List<Transform>();
        FrontLlimb.Add(FrontLClavical);
        recurisive(FrontLClavical, ref FrontLlimb);

        BackRlimb = new List<Transform>();
        BackRlimb.Add(BackRClavical);
        recurisive(BackRClavical, ref BackRlimb);

        BackLlimb = new List<Transform>();
        BackLlimb.Add(BackLClavical);
        recurisive(BackLClavical, ref BackLlimb);

        Head = new List<Transform>();
        Head.Add(HeadRoot);
        recurisive(HeadRoot, ref Head);

        Tail = new List<Transform>();
        Tail.Add(TailRoot);
        recurisive(TailRoot, ref Tail);

        //set joints to start position
        for(int i = 0; i < counterBL.Length; i++)
        {
            FrontLlimb[i].localRotation = quatF[0][i].angles[counterFL[i]];
            FrontRlimb[i].localRotation = quatF[0][i].angles[counterFR[i]];
            BackLlimb[i].localRotation = quatB[0][i].angles[counterBL[i]];
            BackRlimb[i].localRotation = quatB[0][i].angles[counterBR[i]];
        }

        for(int i = 0; i < counterT.Length; i++)
        {
            Tail[i].localRotation = quatT[0][i].angles[counterT[i]];
        }

        Head[0].localRotation = quatH[0][0].angles[counterH[0]];
        
    }

    private void ReadJson(string textFile, int i, ref List<QuatAngles[]> quat)//reads and saves quaternions into arrays from json file
    {
        string json = File.ReadAllText("C:/Users/romai/Desktop/Honours Animation/Assets/Resources/" + textFile + ".json");

        quat[0][i] = new QuatAngles();

        quat[0][i] = JsonUtility.FromJson<QuatAngles>(json);
    }

    private void recurisive(Transform i, ref List<Transform> limbs)//recursive function to get all children transform in parent
    {
        foreach (Transform g in i.GetComponentInChildren<Transform>())
        {
            if (g.GetComponent<Transform>())
            {
                limbs.Add(g.GetComponent<Transform>());
                recurisive(g, ref limbs);
            }
        }
    }

    //class for quaternions in json file
    [System.Serializable]
    public class QuatAngles
    {
        public Quaternion[] angles;

        public void setList(int i)
        {
            angles = new Quaternion[i];
        }
    }
    // Update is called once per frame
    void Update()
    {
        //rotate all the limbs
        RotateLimb(ref lerpedFR, ref lerpedFR[2], ref speedFR, ref quatF, ref counterFR, ref FrontRlimb);
        RotateLimb(ref lerpedFL, ref lerpedFL[2], ref speedFL, ref quatF, ref counterFL, ref FrontLlimb);

        RotateLimb(ref lerpedBR, ref lerpedFL[2], ref speedBR, ref quatB, ref counterBR, ref BackRlimb);
        RotateLimb(ref lerpedBL, ref lerpedFR[2], ref speedBL, ref quatB, ref counterBL, ref BackLlimb);

        RotateLimb(ref lerpedH, ref lerpedFL[2], ref speedH, ref quatH, ref counterH, ref Head);
        RotateLimb(ref lerpedT, ref lerpedFR[2], ref speedT, ref quatT, ref counterT, ref Tail);
    }

    private void RotateLimb(ref bool [] lerped, ref bool otherlerped, ref float [] speed, ref List<QuatAngles[]> quat, ref int[] counter, ref List<Transform> limbs)
    {
        //change rotation in synch with other joint
        if (otherlerped == true)
        {

            for (int i = 0; i < quat[0].Length; i++)
            {
                changeRotation(i, ref quat, ref counter, ref lerped);
            }

        }


        for (int i = 0; i < quat[0].Length; i++)
        {
            reachedRotation(i, speed[i], ref limbs, ref quat, ref counter, ref lerped);
        }
    }

    void changeRotation(int i, ref List<QuatAngles[]> quat, ref int[] counter, ref bool[] lerped)
    {
        //resets counter if at the end
        if (counter[i] >= (quat[0][i].angles.Length-1))
        {
            counter[i] = 0;

        }
        else
        {
            //increment
            counter[i]++;
        }
        lerped[i] = false;
        
    }
    void reachedRotation(int i, float speed, ref List<Transform> limbs, ref List<QuatAngles[]> quat, ref int [] counter, ref bool [] lerped)
    {
        //interpolate from cureent rotation to desired rotation
        limbs[i].localRotation = Quaternion.Slerp(limbs[i].localRotation, quat[0][i].angles[counter[i]], Time.deltaTime * speed);

        //signal rotation is done when close enough to desired rotation
        if (Quaternion.Angle(limbs[i].localRotation, quat[0][i].angles[counter[i]]) <= 7.5)
        {
            lerped[i] = true;
        }
    }
}
