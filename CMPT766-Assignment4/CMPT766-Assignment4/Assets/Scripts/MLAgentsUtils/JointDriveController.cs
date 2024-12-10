using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Unity.MLAgents;

namespace Unity.MLAgentsUtils
{
    /// <summary>
    /// Used to store relevant information for acting and learning for each body part in agent.
    /// </summary>
    [System.Serializable]
    public class BodyPart
    {
        [Header("Body Part Info")]
        [Space(10)]
        public ConfigurableJoint joint;
        public Rigidbody rb;
        [HideInInspector] public Vector3 startingPos;
        [HideInInspector] public Quaternion startingRot;

        [Header("Ground Contact")]
        [Space(10)]
        public GroundContact groundContact;

        [FormerlySerializedAs("thisJDController")]
        [HideInInspector] public JointDriveController thisJdController;

        /// <summary>
        /// Reset body part to initial configuration.
        /// </summary>
        public void Reset(BodyPart bp)
        {
            bp.rb.transform.position = bp.startingPos;
            bp.rb.transform.rotation = bp.startingRot;
            bp.rb.velocity = Vector3.zero;
            bp.rb.angularVelocity = Vector3.zero;
            if (bp.groundContact)
            {
                bp.groundContact.touchingGround = false;
            }
        }

        /// <summary>
        /// Apply torque according to defined goal `x, y, z` angle and force `strength`.
        /// Assuming `x, y, z` are normalized thus they're within the range of [-1, 1].
        /// </summary>
        public void SetJointTargetRotation(float x, float y, float z)
        {
            // shift `x, y, z` to the range [0, 1]
            x = (x + 1f) * 0.5f;
            y = (y + 1f) * 0.5f;
            z = (z + 1f) * 0.5f;

            // unormalize `x, y, z`
            var xRot = Mathf.Lerp(joint.lowAngularXLimit.limit, joint.highAngularXLimit.limit, x);
            var yRot = Mathf.Lerp(-joint.angularYLimit.limit, joint.angularYLimit.limit, y);
            var zRot = Mathf.Lerp(-joint.angularZLimit.limit, joint.angularZLimit.limit, z);

            joint.targetRotation = Quaternion.Euler(xRot, yRot, zRot);
        }

        /// <summary>
        /// Get the current joint angle.
        /// </summary>
        public Vector3 getJointEulerAngle()
        {
            var jointRotation = Quaternion.Inverse(joint.connectedBody.transform.rotation) * joint.transform.rotation;
            return jointRotation.eulerAngles;
        }

        /// <summary>
        /// Get the current joint angle velocity.
        /// </summary>
        public float getJointAngleVelocity()
        {
            var parentAngularVelocity = joint.connectedBody.GetComponent<Rigidbody>().angularVelocity;
            var childAngularVelocity = rb.angularVelocity;
            return (childAngularVelocity - parentAngularVelocity).magnitude;
        }
    }

    public class JointDriveController : MonoBehaviour
    {
        [Header("Joint Drive Settings")]
        [Space(10)]
        public float ks;
        public float kd;
        public float maxJointForceLimit;

        [HideInInspector] public Dictionary<Rigidbody, BodyPart> bodyPartsDict = new Dictionary<Rigidbody, BodyPart>();
        const float k_MaxAngularVelocity = 50.0f;

        /// <summary>
        /// Create BodyPart object and add it to dictionary.
        /// </summary>
        public void SetupBodyPart(Rigidbody rb)
        {
            var bp = new BodyPart
            {
                rb = rb,
                joint = rb.GetComponent<ConfigurableJoint>(),
                startingPos = rb.position,
                startingRot = rb.rotation
            };
            bp.rb.maxAngularVelocity = k_MaxAngularVelocity;

            // Add & setup the ground contact script
            bp.groundContact = rb.GetComponent<GroundContact>();
            if (!bp.groundContact)
            {
                bp.groundContact = rb.gameObject.AddComponent<GroundContact>();
                bp.groundContact.agent = gameObject.GetComponent<Agent>();
            }
            else
            {
                bp.groundContact.agent = gameObject.GetComponent<Agent>();
            }

            if (bp.joint)
            {
                var jd = new JointDrive
                {
                    positionSpring = ks,
                    positionDamper = kd,
                    maximumForce = maxJointForceLimit
                };
                bp.joint.slerpDrive = jd;
            }

            bp.thisJdController = this;
            bodyPartsDict.Add(rb, bp);
        }
    }
}
