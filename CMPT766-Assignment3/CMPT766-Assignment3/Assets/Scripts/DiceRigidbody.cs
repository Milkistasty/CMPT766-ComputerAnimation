// Student ID: 301586596
// Student Name: Wenhe Wang

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceRigidbody : MonoBehaviour
{
    [HideInInspector]
    public bool simPaused;

    public Vector3 initialPosition;
    public Vector3 initialRotation;
    public Vector3 initialVelocity;

    public float dt;
    public float mass;
    public float ks;
    public float kd;
    public float mu;

    private Vector3 position;
    private Quaternion rotation;
    private Vector3 linear_velocity;
    private Vector3 angular_velocity;

    private Vector3[] localVertices = {
        new Vector3(0.5f, 0.5f, 0.5f),
        new Vector3(0.5f, 0.5f, -0.5f),
        new Vector3(0.5f, -0.5f, 0.5f),
        new Vector3(0.5f, -0.5f, -0.5f),
        new Vector3(-0.5f, 0.5f, 0.5f),
        new Vector3(-0.5f, 0.5f, -0.5f),
        new Vector3(-0.5f, -0.5f, 0.5f),
        new Vector3(-0.5f, -0.5f, -0.5f)
    };

    private Matrix3x3 GetInertiaRefMatrix()
    {
        return new Matrix3x3(
            mass / 6f * Vector3.right,
            mass / 6f * Vector3.up,
            mass / 6f * Vector3.forward
        );
    }

    private List<Vector3> GetCollidedVertices()
    {
        List<Vector3> CollidedVertices = new List<Vector3>();

        /*** Part 1: Collision Detection ***/
         // Transform local vertices to global positions and check for collision
        foreach (Vector3 localVertex in localVertices)
        {
            // Transform to global position
            Vector3 globalVertex = position + rotation * localVertex;

            // Check if the vertex is below the ground (y < 0)
            if (globalVertex.y < 0f)
            {
                CollidedVertices.Add(globalVertex);
            }
        }
        /*** part 1 coding ends ***/

        return CollidedVertices;
    }

    private (Vector3, Vector3) ComputeForceAndTorque()
    {
        Vector3 netForce = mass * new Vector3(0f, -9.81f, 0f); // Gravity force
        Vector3 netTorque = Vector3.zero;

        /*** Part 2: Calculate Forces and Torques ***/
        // Get the list of collided vertices
        List<Vector3> collidedVertices = GetCollidedVertices();

        foreach (Vector3 collisionPoint in collidedVertices)
        {
            Vector3 normal = Vector3.up; // Ground normal

            // Penetration depth
            float penetrationDepth = -collisionPoint.y;

            // Spring force (penalty)
            Vector3 collisionForce = ks * penetrationDepth * normal;

            // Vector from center of mass to collision point
            Vector3 r = collisionPoint - position;

            // Velocity at the collision point
            Vector3 v_point = linear_velocity + Vector3.Cross(angular_velocity, r);

            // Relative velocity along the normal
            float v_rel = Vector3.Dot(v_point, normal);

            // Damping force
            Vector3 dampingForce = -kd * v_rel * normal;

            // Total normal force
            Vector3 normalForce = collisionForce + dampingForce;

            // Tangential velocity
            Vector3 v_tangent = v_point - v_rel * normal;

            // Friction force
            Vector3 frictionForce = Vector3.zero;
            if (v_tangent.magnitude > 0.0001f)
            {
                Vector3 frictionDir = v_tangent.normalized;
                float frictionMagnitude = mu * normalForce.magnitude;
                frictionForce = -frictionMagnitude * frictionDir;
            }

            // Total collision force
            Vector3 totalForce = normalForce + frictionForce;

            // Aggregate net force and torque
            netForce += totalForce;
            netTorque += Vector3.Cross(r, totalForce);
        }
        /*** part 2 coding ends ***/

        return (netForce, netTorque);
    }

    private void Integrate(float deltaTime)
    {
        var (force, torque) = ComputeForceAndTorque();

        /*** Part 3: Integrate Timestep ***/
        // Update linear velocity
        linear_velocity += (force / mass) * deltaTime;

        // Update position
        position += linear_velocity * deltaTime;

        // inertia matrix in the local body frame
        Matrix3x3 I_body = GetInertiaRefMatrix();

        // Rotation matrix from quaternion
        Matrix3x3 R = new Matrix3x3(rotation);

        // inertia matrix in world frame
        Matrix3x3 I_world = R * I_body * R.transpose;
        Matrix3x3 I_world_inv = I_world.inverse;

        // Angular acceleration
        Vector3 angular_acceleration = I_world_inv * torque;

        // Update angular velocity
        angular_velocity += angular_acceleration * deltaTime;

        // Update rotation using quaternion integration
        Quaternion omegaQuat = new Quaternion(angular_velocity.x, angular_velocity.y, angular_velocity.z, 0f);

        Quaternion q_dot = omegaQuat * rotation;

        q_dot.x *= 0.5f;
        q_dot.y *= 0.5f;
        q_dot.z *= 0.5f;
        q_dot.w *= 0.5f;

        rotation.x += q_dot.x * deltaTime;
        rotation.y += q_dot.y * deltaTime;
        rotation.z += q_dot.z * deltaTime;
        rotation.w += q_dot.w * deltaTime;

        rotation = rotation.normalized;
        /*** part 3 coding ends ***/
    }

    public void AdvanceTimeStep()
    {
        Integrate(dt);
    }

    public void ResetState()
    {
        position = initialPosition;
        linear_velocity = initialVelocity;
        rotation = Quaternion.Euler(initialRotation);
        angular_velocity = Vector3.zero;
    }

    // Start is called before the first frame update
    void Start()
    {
        ResetState();
    }

    // FixedUpdate is called every fixed frame-rate frame
    // Read more here: https://docs.unity3d.com/ScriptReference/MonoBehaviour.FixedUpdate.html
    void FixedUpdate()
    {
        if (!simPaused)
        {
            AdvanceTimeStep();
        }
        transform.position = position;
        transform.rotation = rotation;
        Time.fixedDeltaTime = dt;
    }
}
