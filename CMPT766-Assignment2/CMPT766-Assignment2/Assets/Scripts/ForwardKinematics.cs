// Name: Wenhe Wang
// Student ID: 301586596
// Email: wwa118@sfu.ca

using System;
using UnityEngine;

public static class ForwardKinematics
{
    public static void UpdateJointPositions(Actor actor, float[] frameData)
    {
        /*** Please write your code here ***/
        /*** code to be completed by students begins ***/

        // Get the list of joints in the actor
        Joint[] joints = actor.Joints.ToArray();
        int frameIndex = 0;

        // Debug.Log($"Processing {joints.Length} joints with frameData length: {frameData.Length}");

        // Iterate through all joints and update their positions and orientations
        for (int i = 0; i < joints.Length; i++)
        {
            Joint joint = joints[i];
            Joint parentJoint = joint.GetParent();

            // Check if the joint is an end animator or special joint that doesn't have frame/rotation data (e.g., Site for "jumping" animation)
            if (joint.RotateOrder == Joint.RotationOrder.NONE || joint.Name.ToLower().Contains("site"))
            {
                if (parentJoint != null)
                {
                    // Compute global position based on parent
                    joint.GlobalPosition = parentJoint.GlobalPosition + (parentJoint.GlobalQuaternion * joint.LocalPosition);
                    joint.GlobalQuaternion = parentJoint.GlobalQuaternion;
                }
                else
                {
                    // Handle the root joint
                    joint.GlobalPosition = joint.LocalPosition;
                    joint.GlobalQuaternion = Quaternion.identity;
                }
                
                continue;
            }

            // Handle the root joint separately as it has both translation and rotation (6 degrees of freedom)
            if (parentJoint == null)
            {
                joint.LocalPosition = new Vector3(frameData[frameIndex], frameData[frameIndex + 1], frameData[frameIndex + 2]);
                frameIndex += 3;

                joint.LocalQuaternion = ParseRotation(frameData, ref frameIndex, joint.RotateOrder);
                joint.GlobalPosition = joint.LocalPosition;
                joint.GlobalQuaternion = joint.LocalQuaternion;
            }
            // Handle other child joints with rotation only (3 degrees of freedom)
            else
            {
                joint.LocalQuaternion = ParseRotation(frameData, ref frameIndex, joint.RotateOrder);
                joint.GlobalQuaternion = parentJoint.GlobalQuaternion * joint.LocalQuaternion;
                joint.GlobalPosition = parentJoint.GlobalPosition + (parentJoint.GlobalQuaternion * joint.LocalPosition);
            }
        }

        // Debug.Log($"Final frame index: {frameIndex}, FrameData length: {frameData.Length}");

        /*** code to be completed by students ends ***/
    }

    // parse and convert Euler angles based on the rotation order
    // Quaternion.Euler() method in Unity expects the angles in Unity’s default order (which is ZXY)
    // we need to convert our rotation vector positions differently for different bvh files
    // it will be similar to ndarray.permute(1,2,0) in Python
    private static Quaternion ParseRotation(float[] frameData, ref int frameIndex, Joint.RotationOrder rotateOrder)
    {
        float[] rotationValues = new float[3];
        for (int i = 0; i < 3; i++)
        {
            rotationValues[i] = frameData[frameIndex + i];
        }
        frameIndex += 3;

        float x = 0, y = 0, z = 0;

        switch (rotateOrder)
        {
            case Joint.RotationOrder.XYZ:
                x = rotationValues[0];
                y = rotationValues[1];
                z = rotationValues[2];
                break;
            case Joint.RotationOrder.XZY:
                x = rotationValues[0];
                z = rotationValues[1];
                y = rotationValues[2];
                break;
            case Joint.RotationOrder.YXZ:
                y = rotationValues[0];
                x = rotationValues[1];
                z = rotationValues[2];
                break;
            case Joint.RotationOrder.YZX:
                y = rotationValues[0];
                z = rotationValues[1];
                x = rotationValues[2];
                break;
            case Joint.RotationOrder.ZXY:
                z = rotationValues[0];
                x = rotationValues[1];
                y = rotationValues[2];
                break;
            case Joint.RotationOrder.ZYX:
                z = rotationValues[0];
                y = rotationValues[1];
                x = rotationValues[2];
                break;
            default:
                Debug.LogError("unsupported rotation order: " + rotateOrder);
                x = y = z = 0;
                break;
        }

        Quaternion rotX = Quaternion.AngleAxis(x, Vector3.right);
        Quaternion rotY = Quaternion.AngleAxis(y, Vector3.up);
        Quaternion rotZ = Quaternion.AngleAxis(z, Vector3.forward);

        Quaternion rotation;
        switch (rotateOrder)
        {
            case Joint.RotationOrder.XYZ:
                rotation = rotX * rotY * rotZ;
                break;
            case Joint.RotationOrder.XZY:
                rotation = rotX * rotZ * rotY;
                break;
            case Joint.RotationOrder.YXZ:
                rotation = rotY * rotX * rotZ;
                break;
            case Joint.RotationOrder.YZX:
                rotation = rotY * rotZ * rotX;
                break;
            case Joint.RotationOrder.ZXY:
                rotation = rotZ * rotX * rotY;
                break;
            case Joint.RotationOrder.ZYX:
                rotation = rotZ * rotY * rotX;
                break;
            default:
                Debug.LogError("Unsupported rotation order: " + rotateOrder);
                rotation = Quaternion.identity;
                break;
        }

        return rotation;
    }
}
