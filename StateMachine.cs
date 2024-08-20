using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LWSM
{
    //States can be added and reduced per use case, eg adding OnGetTriggerState();
    public interface IState
    {
        public void EnterState();
        public void ExitState();
        public void UpdateState();
        public void PhysicsState();
        IState GetNextState();
    }

    public interface IStateMachine
    {
        IState CurrentState { get; }
        IDictionary<object, IState> States { get; }
    }

    /// <summary>
    /// Manages the transitions of states and the current state with monobehaviours.
    /// </summary>
    public class StateMachine : MonoBehaviour, IStateMachine
    {
        public IState CurrentState { get; set; }
        public IDictionary<object, IState> States { get; private set; } = new Dictionary<object, IState>();

        protected bool isTransitioning = false;

        private void Start()
        {
            if (States.Count > 0)
            {
                CurrentState = States.Values.FirstOrDefault();
                CurrentState?.EnterState();
            }
            else
            {
                Debug.LogError("No states found in the state machine.");
            }
        }

        private void Update()
        {
            if (CurrentState == null || isTransitioning) return;

            // Declare and initialize the nextState variable
            var nextState = CurrentState.GetNextState();
            if (nextState != null && nextState != CurrentState)
                TransitionToState(nextState);
            else
                CurrentState?.UpdateState();
        }

        private void FixedUpdate()
        {
            CurrentState?.PhysicsState();
        }

        //Manages the transitions of states
        public void TransitionToState(IState nextState)
        {
            if (nextState == null)
            {
                Debug.LogWarning("Attempted to transition to a null state.");
                return;
            }

            isTransitioning = true;
            CurrentState?.ExitState();
            CurrentState = nextState;
            CurrentState?.EnterState();
            isTransitioning = false;
        }
    }
}
