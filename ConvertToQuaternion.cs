using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class ConvertToQuaternion : MonoBehaviour
{
    //global variables 
    public TextAsset textFile;
    string text;
    string[] line;
    float[] angles;
    public string filename;
    
    int counter = 0;
    float target = 0;
    public bool start = true;
    bool skip = true;
    
    QuatAngles g;

    public Vector3 axis = Vector3.up;

    // Start is called before the first frame update
    void Start()
    {
        
        //read in angles from txt file
        text = textFile.ToString();
        line = text.Split('\n');
        angles = new float[line.Length];
        g = new QuatAngles();
        g.setList(line.Length);

        for (int i = 0; i < line.Length; i++)
        {
            //convert from radians to degrees
            angles[i] = float.Parse(line[i]);
            angles[i] = angles[i] * Mathf.Rad2Deg;
        }
       
       
    }

    // Update is called once per frame
    void Update()
    {
        if (start)
        {
            if (counter > (angles.Length - 0.9))
            {
             //write to file when reached end of the array  
                Debug.Log("conversion done");
                if (skip)
                {
                    writeToFile();
                    skip = false;
                }
            }
            else
            {
                //set target angle to rotate by
                if (counter == 0)
                {
                    target = angles[counter];
                }
                else
                {
                    //set target to be the distance between the previous and current angle 
                    target = angles[counter] - angles[counter - 1];
                }

                //rotate to target angle round local axis
                transform.Rotate(axis, target, Space.Self);
               
                //save local quaternion to an array
                g.angles[counter] = transform.localRotation;

                //pause rotation
                start = false;
            }
        }
        else
        {   
            //move on to next target angle
            counter++;
            start = true;
        }
       
    }

    void writeToFile()
    {
        //write to file
        string json = JsonUtility.ToJson(g);

        File.WriteAllText("C:/Users/romai/Desktop/Honours Animation/Assets/Resources/" + filename, json);
    }

    //class for json file to save quaternions
    [System.Serializable]
    public class QuatAngles
    {
        public Quaternion[] angles;

        public void setList(int i)
        {
            angles = new Quaternion[i];
        }
    }

}
