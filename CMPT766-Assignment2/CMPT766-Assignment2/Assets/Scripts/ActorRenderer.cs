// Name: Wenhe Wang
// Student ID: 301586596
// Email: wwa118@sfu.ca

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ActorRenderer : MonoBehaviour
{
    private Actor Actor;

    private List<Transform> JointSpheres;
    private List<Transform> Bones;

    [Range(0.0f, 1.0f)]
    public float ActorScale;
    public float BoneWidth = 1f;
    public Color BoneColor = Color.gray;
    public Color JointColor = Color.red;

    private Transform CreateJointObject(Joint joint)
    {
        Transform joint_ball = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
        Destroy(joint_ball.GetComponent<SphereCollider>());
        joint_ball.parent = transform;
        joint_ball.GetComponent<Renderer>().material.color = JointColor;

        return joint_ball;
    }

    private void SetActorScaleAndBoneWidth()
    {
        ActorScale = 1.0f;
        var legJoint = Actor.Joints.Find(x => x.Name.Contains("Leg"));
        if (legJoint is not null)
        {
            ActorScale = 0.5f / Vector3.Magnitude(legJoint.LocalPosition);
            BoneWidth = Mathf.Ceil(Vector3.Magnitude(legJoint.LocalPosition) * 0.2f);
        }
    }

    private Transform CreateBoneObject(Joint joint)
    {
        Transform bone = GameObject.CreatePrimitive(PrimitiveType.Cylinder).transform;
        Destroy(bone.GetComponent<CapsuleCollider>());
        bone.parent = transform;
        bone.GetComponent<Renderer>().material.color = BoneColor;

        return bone;
    }

    private void InitializeSkeleton()
    {
        JointSpheres = new List<Transform>();
        Bones = new List<Transform>();

        JointSpheres.Add(CreateJointObject(Actor.GetRootJoint()));
        Bones.Add(null);

        foreach (Joint joint in Actor.Joints.Skip(1))
        {
            JointSpheres.Add(CreateJointObject(joint));
            Bones.Add(CreateBoneObject(joint));
        }
    }

    private void UpdateSkeletonPosition()
    {
        /*** Please write your code here ***/
        /*** code to be completed by students begins ***/

        for (int i = 0; i < Actor.Joints.Count; i++)
        {
            Joint joint = Actor.Joints[i];
            // Update the sphere position for the joint
            JointSpheres[i].transform.position = joint.GlobalPosition;
            Joint parentJoint = joint.GetParent();

            // ignore the root joint for bones
            if (parentJoint != null)
            {
                // Compute the direction and length between the parent and child joint
                Vector3 direction = joint.GlobalPosition - parentJoint.GlobalPosition;
                float boneLength = direction.magnitude;

                // Set the bone cylinder position half of the distance between parent and child joint
                Bones[i].transform.position = parentJoint.GlobalPosition + direction / 2;

                // Set the bone cylinder rotation to point from the parent to the child joint
                Bones[i].transform.up = direction.normalized;

                // Scale the bone cylinder to match the length between the joints
                Vector3 boneScale = Bones[i].transform.localScale;
                boneScale.y = boneLength / 2; // Assuming the cylinder is along the y-axis
                Bones[i].transform.localScale = boneScale;
            }
        }
        
        /*** code to be completed by students ends ***/

        // DO NOT REMOVE: Necessary to apply the required scaling
        ApplyActorScale();
    }

    private void ApplyActorScale()
    {
        if (JointSpheres.Count > 0)
        {
            JointSpheres[0].localScale = Vector3.one * 2.0f * BoneWidth * ActorScale;
            JointSpheres[0].position *= ActorScale;
        }

        for (int i = 1; i < Actor.Joints.Count; i++)
        {
            var joint = Actor.Joints[i];

            JointSpheres[i].localScale = Vector3.one * 2.0f * BoneWidth * ActorScale;
            JointSpheres[i].position *= ActorScale;

            var boneLength = Vector3.Magnitude(joint.LocalPosition);
            Bones[i].localScale = new Vector3(BoneWidth, boneLength / 2, BoneWidth) * ActorScale;
            Bones[i].position *= ActorScale;
        }
    }

    private void Start()
    {
        Actor = GetComponent<Actor>();
        SetActorScaleAndBoneWidth();
        InitializeSkeleton();
    }

    private void Update()
    {
        UpdateSkeletonPosition();
    }
}
