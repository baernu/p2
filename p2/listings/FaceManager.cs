using System.Collections.Generic;
using UnityEngine;
using System;

using NuitrackSDK.Tutorials.RGBandSkeletons;



namespace NuitrackSDK.Tutorials.FaceTracker
{
    [AddComponentMenu("NuitrackSDK/Tutorials/Face Tracker/Face Manager")]
    public class FaceManager : MonoBehaviour
    {
        [SerializeField] Canvas canvas;
        [SerializeField] GameObject faceController;
        [SerializeField] SkeletonController skeletonController;
        List<FaceController> faceControllers = new List<FaceController>();
        string message = "";
        [SerializeField] private Transform transform_Camera;
        private bool beingHandled = false;
        private float lastYaw;
        private float lastPitch;
        


        void Start()
        {


            for (int i = 0; i < skeletonController.skeletonCount; i++)
            {
                faceControllers.Add(Instantiate(faceController, canvas.transform).GetComponent<FaceController>());
            }

            message = "Starting ...";
            
        }

        void Update()
        {

            for (int i = 0; i < faceControllers.Count; i++)
            {
                int id = i + 1;
                UserData user = NuitrackManager.Users.GetUser(id);

                if (user != null && user.Skeleton != null && user.Face != null)
                {
                    // Pass the face to FaceController
                    faceControllers[i].SetFace(user.Face);
                    faceControllers[i].gameObject.SetActive(true);

                    UserData.SkeletonData.Joint head = user.Skeleton.GetJoint(nuitrack.JointType.Head);

                    faceControllers[i].transform.position = head.RelativePosition(Screen.width, Screen.height);
                    //stretch the face to fit the rectangle

                    faceControllers[i].transform.localScale = user.Face.ScreenRect(Screen.width, Screen.height).size;

                    message = "Position" + head.Position.z;
                    Vector3 _position = head.Position;
                    _position.z = -_position.z;
                    //_position.y = 0;
                   
                    transform_Camera.position = _position;

                    /*

                    string json = nuitrack.Nuitrack.GetInstancesJson();
                    //message = "json" + json;


                    // Deserialize JSON string into RootObject using Unity's JsonUtility
                    RootObject data = JsonUtility.FromJson<RootObject>(json);


                    // Accessing angles from the first instance (assuming there is at least one instance)
                    if (data.Instances.Count > 0 && !beingHandled)
                    {
                        
                        StartCoroutine(HandleIt());
                        

                        float yaw = data.Instances[0].face.angles.yaw;
                        float pitch = data.Instances[0].face.angles.pitch;
                        float roll = data.Instances[0].face.angles.roll;
                        //message = "Angle" + yaw + " Pitch" + pitch + " Roll" + roll;
                        yaw = (yaw + 4*lastYaw) / 5;
                        pitch = (pitch + 4 * lastPitch) / 5;
                        Quaternion relativePos = new Quaternion(- pitch/70, - yaw/80, 0.0f, 1.0f);
                        //transform_Camera.rotation = relativePos;
                        lastYaw = yaw;
                        lastPitch = pitch;
                    } 
                    */
                        

                }
                else
                {
                    faceControllers[i].gameObject.SetActive(false);
                }
            }
        }


        /*
        private System.Collections.IEnumerator HandleIt()
        {
            beingHandled = true;
            // process pre-yield
            yield return new WaitForSeconds(0.02f);
            // process post-yield
            beingHandled = false;
        }

       

        [Serializable]
        public class Angles
        {
            public float yaw;
            public float pitch;
            public float roll;
        }

        [Serializable]
        public class Face
        {
            public Angles angles;
        }

        [Serializable]
        public class Instance
        {
            public Face face;
        }

        [Serializable]
        public class RootObject
        {
            public List<Instance> Instances;
        }
        */

        void OnGUI()
        {
            GUI.color = Color.red;
            GUI.skin.label.fontSize = 30;
            GUILayout.Label(message);
        }
    }
}