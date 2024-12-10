// Student name: Wenhe Wang
// Student ID: 301586596
// Student email: wwa118@sfu.ca

// reference: https://github.com/miyamotok0105/unity-ml-agents/tree/master
// https://www.gocoder.one/blog/training-agents-using-ppo-with-unity-ml-agents/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgentsUtils;

[RequireComponent(typeof(JointDriveController))] // Required to set joint forces
public class MarathonManAgent : Agent
{
    [Header("Body Parts")] 
    [Space(10)]
    public Rigidbody body;
    public Rigidbody left_thigh;
    public Rigidbody left_shin;
    public Rigidbody left_foot;
    public Rigidbody right_thigh;
    public Rigidbody right_shin;
    public Rigidbody right_foot;

    JointDriveController m_JdController;
    Dictionary<Rigidbody, BodyPart> bodyPartsDict;

    public override void Initialize()
    {
        m_JdController = GetComponent<JointDriveController>();
        bodyPartsDict = m_JdController.bodyPartsDict;

        //Setup each body part
        m_JdController.SetupBodyPart(body);
        m_JdController.SetupBodyPart(left_thigh);
        m_JdController.SetupBodyPart(left_shin);
        m_JdController.SetupBodyPart(left_foot);
        m_JdController.SetupBodyPart(right_thigh);
        m_JdController.SetupBodyPart(right_shin);
        m_JdController.SetupBodyPart(right_foot);
    }

    /// <summary>
    /// Loop over body parts and reset them to initial conditions.
    /// </summary>
    public override void OnEpisodeBegin()
    {
        foreach (var bodyPart in m_JdController.bodyPartsDict.Values)
        {
            bodyPart.Reset(bodyPart);
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // body's orientation
        sensor.AddObservation(body.rotation.eulerAngles);

        // joints angles
        sensor.AddObservation(bodyPartsDict[left_thigh].getJointEulerAngle().x);
        sensor.AddObservation(bodyPartsDict[left_thigh].getJointEulerAngle().z);
        sensor.AddObservation(bodyPartsDict[left_shin].getJointEulerAngle().x);
        sensor.AddObservation(bodyPartsDict[left_foot].getJointEulerAngle().x);
        sensor.AddObservation(bodyPartsDict[right_thigh].getJointEulerAngle().x);
        sensor.AddObservation(bodyPartsDict[right_shin].getJointEulerAngle().x);
        sensor.AddObservation(bodyPartsDict[right_foot].getJointEulerAngle().x);

        // joints velocities
        sensor.AddObservation(bodyPartsDict[left_thigh].getJointAngleVelocity());
        sensor.AddObservation(bodyPartsDict[left_shin].getJointAngleVelocity());
        sensor.AddObservation(bodyPartsDict[left_foot].getJointAngleVelocity());
        sensor.AddObservation(bodyPartsDict[right_thigh].getJointAngleVelocity());
        sensor.AddObservation(bodyPartsDict[right_shin].getJointAngleVelocity());
        sensor.AddObservation(bodyPartsDict[right_foot].getJointAngleVelocity());

        // body's height and velocity
        sensor.AddObservation(body.position.y);
        sensor.AddObservation(body.velocity);

        // left and right foot's relative yz-position to body
        var left_foot_relPos = left_foot.position - body.position;
        var right_foot_relPos = right_foot.position - body.position;
        sensor.AddObservation(left_foot_relPos.y);
        sensor.AddObservation(left_foot_relPos.z);
        sensor.AddObservation(right_foot_relPos.y);
        sensor.AddObservation(right_foot_relPos.z);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        var continuousActions = actionBuffers.ContinuousActions;
        var i = 0;

        bodyPartsDict[left_thigh].SetJointTargetRotation(continuousActions[i++], 0, continuousActions[i++]);
        bodyPartsDict[left_shin].SetJointTargetRotation(continuousActions[i++], 0, 0);
        bodyPartsDict[left_foot].SetJointTargetRotation(continuousActions[i++], 0, 0);
        bodyPartsDict[right_thigh].SetJointTargetRotation(continuousActions[i++], 0, continuousActions[i++]);
        bodyPartsDict[right_shin].SetJointTargetRotation(continuousActions[i++], 0, 0);
        bodyPartsDict[right_foot].SetJointTargetRotation(continuousActions[i++], 0, 0);

        // Make sure to call the correct reward function while training your standard/variation models
        // AddReward(computeRewardStandard());
        AddReward(computeRewardVariation());
    }

    public float computeRewardStandard()
    {
        /*** Please write your code for the standard reward here ***/

        // expected forward speed in the +z direction
        float desiredSpeed = 1.0f;

        // agent's actual forward velocity
        float forwardVelocity = Vector3.Dot(body.velocity, Vector3.forward);

        //  the ratio of actual speed to expected forward speed
        float speedReward = Mathf.Clamp(forwardVelocity / desiredSpeed, 0f, 1f);

        // up-rightness by calculating the body's up vector with the global up vector
        float uprightness = Vector3.Dot(body.transform.up, Vector3.up);

        // Clamp the uprightness between 0 and 1
        float uprightnessReward = Mathf.Clamp01(uprightness);

        // Weighted sum of speed and uprightness rewards
        float totalReward = 0.7f * speedReward + 0.3f * uprightnessReward;

        // check if the agent has fallen over
        if (body.position.y < 0.5f)
        {
            // penalty for falling and end the episode
            totalReward = -1f;
            EndEpisode();
        }

        // clamp the total reward between -1 and 1
        totalReward = Mathf.Clamp(totalReward, -1f, 1f);

        return totalReward;

        /*** Coding ends here ***/
    }

    public float computeRewardVariation()
    {   
        // in this variation, I just tested the agent to move in +x direction (sideways)

        /*** Please write your code for the variation reward here ***/

        // expected speed in the +x direction
        float desiredSpeed = 1.0f;

        // actual velocity
        float sidewaysVelocity = Vector3.Dot(body.velocity, Vector3.right);

        // the ratio of actual speed to expected speed, clamped between 0 and 1
        float speedReward = Mathf.Clamp(sidewaysVelocity / desiredSpeed, 0f, 1f);

        // up-rightness by calculating the body's up vector with the global up vector
        float uprightness = Vector3.Dot(body.transform.up, Vector3.up);

        // clamp uprightness between 0 and 1
        float uprightnessReward = Mathf.Clamp01(uprightness);

        // Weighted sum of speed and uprightness rewards
        float totalReward = 0.7f * speedReward + 0.3f * uprightnessReward;

        // Check if the agent has fallen over
        if (body.position.y < 0.5f)
        {
            // penalty for falling and end the episode
            totalReward = -1f;
            EndEpisode();
        }

        // clamp the total reward between -1 and 1
        totalReward = Mathf.Clamp(totalReward, -1f, 1f);

        return totalReward;

        /*** Coding ends here ***/
    }
}
