using System;
using UnityEngine;

namespace MoreMountains.Tools
{
    [Serializable]
    public class AIActionsList : MMReorderableArray<AIAction>
    {
    }

    [Serializable]
    public class AITransitionsList : MMReorderableArray<AITransition>
    {
    }

    public struct AIStateEvent
    {
        public AIBrain Brain;
        public AIState ExitState;
        public AIState EnterState;

        public AIStateEvent(AIBrain brain, AIState exitState, AIState enterState)
        {
            Brain = brain;
            ExitState = exitState;
            EnterState = enterState;
        }
    }

    /// <summary>
    /// A State is a combination of one or more actions, and one or more transitions. An example of a state could be "_patrolling until an enemy gets in range_".
    /// </summary>
    [Serializable]
    public class AIState
    {
        public string StateName;

        [MMReorderableAttribute(null, "Action", null)]
        public AIActionsList Actions;

        [MMReorderableAttribute(null, "Transition", null)]
        public AITransitionsList Transitions;

        protected AIBrain _brain;

        /// <summary>
        /// Sets this state's brain to the one specified in parameters
        /// </summary>
        /// <param name="brain"></param>
        public virtual void SetBrain(AIBrain brain)
        {
            _brain = brain;
        }

        /// <summary>
        /// On enter state we pass that info to our actions and decisions
        /// </summary>
        public virtual void EnterState()
        {
            for (var i = 0; i < Actions.Count; i++)
            {
                Actions[i].OnEnterState();
            }

            for (var i = 0; i < Transitions.Count; i++)
            {
                Transitions[i].Decision?.OnEnterState();
            }
        }

        /// <summary>
        /// On exit state we pass that info to our actions and decisions
        /// </summary>
        public virtual void ExitState()
        {
            for (var i = 0; i < Actions.Count; i++)
            {
                Actions[i].OnExitState();
            }

            for (var i = 0; i < Transitions.Count; i++)
            {
                Transitions[i].Decision?.OnExitState();
            }
        }

        /// <summary>
        /// Performs this state's actions
        /// </summary>
        public virtual void PerformActions()
        {
            for (var i = 0; i < Actions.Count; i++)
            {
                if (Actions[i])
                    Actions[i].PerformAction();
                else
                    Debug.LogError("An action in " + _brain.gameObject.name + " on state " + StateName + " is null.");
            }
        }

        /// <summary>
        /// Tests this state's transitions
        /// </summary>
        public virtual void EvaluateTransitions()
        {
            for (var i = 0; i < Transitions.Count; i++)
            {
                var t = Transitions[i];
                var decision = t.Decision;
                if (decision == null)
                    continue;

                if (decision.Decide())
                {
                    var trueState = t.TrueState;
                    if (!string.IsNullOrEmpty(trueState))
                    {
                        _brain.TransitionToState(trueState);
                        break;
                    }
                }
                else
                {
                    var falseState = t.FalseState;
                    if (!string.IsNullOrEmpty(falseState))
                    {
                        _brain.TransitionToState(falseState);
                        break;
                    }
                }
            }
        }
    }
}