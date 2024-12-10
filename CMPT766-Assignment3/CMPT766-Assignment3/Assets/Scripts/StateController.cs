using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateController : MonoBehaviour
{
    DiceRigidbody WhiteDie;
    Rigidbody YellowDie;
    bool _simPaused;
    bool _advanceFrame;
    bool _advanceFrameDone;

    SimState _YellowDieSavedState;

    struct SimState
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 linear_velocity;
        public Vector3 angular_velocity;
    }

    void Start()
    {
        WhiteDie = GameObject.Find("WhiteDice").GetComponent<DiceRigidbody>();
        YellowDie = GameObject.Find("YellowDice").GetComponent<Rigidbody>();
        _simPaused = false;
        WhiteDie.simPaused = _simPaused;
        ResetSimulation();
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.R))
        {
            ResetSimulation();
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            _simPaused = !_simPaused;
            if (_simPaused)
            {
                PauseSimulation();
            }
            else
            {
                ResumeSimulation();
            }
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) && _simPaused)
        {
            _advanceFrame = true;
            _advanceFrameDone = false;
            WhiteDie.AdvanceTimeStep();
        }
    }

    void ResetSimulation()
    {
        WhiteDie.ResetState();
        YellowDie.position = WhiteDie.initialPosition + 2 * Vector3.right;
        YellowDie.rotation = Quaternion.Euler(WhiteDie.initialRotation);
        YellowDie.velocity = WhiteDie.initialVelocity;
        YellowDie.angularVelocity = Vector3.zero;

        SaveState();
    }

    void PauseSimulation()
    {
        WhiteDie.simPaused = true;
        SaveState();
        YellowDie.isKinematic = true;
    }

    void ResumeSimulation()
    {
        WhiteDie.simPaused = false;
        YellowDie.isKinematic = false;
        LoadState();
    }

    void SaveState()
    {
        _YellowDieSavedState.position = YellowDie.position;
        _YellowDieSavedState.rotation = YellowDie.rotation;
        _YellowDieSavedState.linear_velocity = YellowDie.velocity;
        _YellowDieSavedState.angular_velocity = YellowDie.angularVelocity;
    }

    void LoadState()
    {
        YellowDie.position = _YellowDieSavedState.position;
        YellowDie.rotation = _YellowDieSavedState.rotation;
        YellowDie.velocity = _YellowDieSavedState.linear_velocity;
        YellowDie.angularVelocity = _YellowDieSavedState.angular_velocity;
    }

    void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            WhiteDie = GameObject.Find("WhiteDice").GetComponent<DiceRigidbody>();
            YellowDie = GameObject.Find("YellowDice").GetComponent<Rigidbody>();

            WhiteDie.transform.position = WhiteDie.initialPosition;
            WhiteDie.transform.rotation = Quaternion.Euler(WhiteDie.initialRotation);
            YellowDie.transform.position = WhiteDie.initialPosition + 2 * Vector3.right;
            YellowDie.transform.rotation = Quaternion.Euler(WhiteDie.initialRotation); 
        }
#endif
    }

    void FixedUpdate()
    {
        if (_simPaused && _advanceFrame)
        {
            if (!_advanceFrameDone)
            {
                YellowDie.isKinematic = false;
                LoadState();
                _advanceFrameDone = true;
            }
            else
            {
                SaveState();
                YellowDie.isKinematic = true;
                _advanceFrame = false;
                _advanceFrameDone = false;
            }
        }
    }
}
