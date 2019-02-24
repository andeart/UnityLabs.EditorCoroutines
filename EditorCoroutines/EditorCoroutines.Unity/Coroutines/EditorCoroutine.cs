using Andeart.EditorCoroutines.Coroutines;
using Andeart.EditorCoroutines.Yields;
using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using Yield = Andeart.EditorCoroutines.Unity.Yields.Yield;


namespace Andeart.EditorCoroutines.Unity.Coroutines
{

    /// <summary>
    /// <see cref="EditorCoroutineService.StartCoroutine(IEnumerator)" /> returns an EditorCoroutine.
    /// Instances of this class are only used to reference these coroutines, and do not hold any exposed properties or
    /// functions.
    /// An EditorCoroutine is a function that can suspend its execution (yield) until the given <see cref="YieldInstruction" />
    /// finishes.
    /// </summary>
    public class EditorCoroutine : ICoroutine
    {
        private IYield _currentYield;
        private bool _finished;
        private readonly IEnumerator _routine;
        private readonly int _ownerHash;
        private readonly string _id;

        // Make otherwise-public properties as EIIs, so they're not visible to client code.
        // Simplicity.
        IYield ICoroutine.CurrentYield
        {
            get => _currentYield;
            set => _currentYield = value;
        }
        bool ICoroutine.Finished
        {
            get => _finished;
            set => _finished = value;
        }
        IEnumerator ICoroutine.Routine => _routine;
        int ICoroutine.OwnerHash => _ownerHash;
        string ICoroutine.Id => _id;

        internal EditorCoroutine (int ownerHash, IEnumerator routine, string id)
        {
            _currentYield = new EditorCoroutines.Yields.Yield.Default ();
            _finished = false;

            _routine = routine ?? throw new ArgumentNullException (nameof(routine), "Routine used to start an EditorCoroutine cannot be null.");
            _ownerHash = ownerHash;
            _id = id;
        }

        void ICoroutine.Evaluate ()
        {
            object current = _routine.Current;
            switch (current)
            {
                case null:
                case WaitForFixedUpdate _:
                case WaitForEndOfFrame _:
                    _currentYield = new Yield.WaitForFrames (1);
                    break;
                case WaitForSeconds waitForSeconds:
                    const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
                    FieldInfo field = typeof(WaitForSeconds).GetField ("m_Seconds", bindingFlags);
                    // This check should always pass...
                    if (field != null)
                    {
                        float seconds = float.Parse (field.GetValue (waitForSeconds).ToString ());
                        _currentYield = new Yield.WaitForSeconds (seconds);
                    }
                    // ... unless Unity has changed their internal variable naming in WaitForSeconds, in which case we use Yield.Default.
                    else
                    {
                        _currentYield = new EditorCoroutines.Yields.Yield.Default ();
                    }

                    break;
                case AsyncOperation asyncOperation:
                    _currentYield = new Yield.AsyncOperation (asyncOperation);
                    break;
                case CustomYieldInstruction customYieldInstruction:
                    _currentYield = new Yield.CustomYieldInstruction (customYieldInstruction);
                    break;
                case EditorCoroutine coroutine:
                    _currentYield = new Yield.NestedCoroutine (coroutine);
                    break;
                default:
                    _currentYield = new EditorCoroutines.Yields.Yield.Default ();
                    break;
            }
        }

        /// <summary>
        /// Determines whether the specified object is the same as this EditorCoroutine.
        /// </summary>
        public override bool Equals (object obj)
        {
            if (obj is ICoroutine coroutine)
            {
                return _id == coroutine.Id;
            }

            return false;
        }

        /// <summary>
        /// A hash code for this EditorCoroutine.
        /// </summary>
        public override int GetHashCode ()
        {
            return _id.GetHashCode ();
        }

        /// <summary>
        /// A string that represents this EditorCoroutine.
        /// </summary>
        public override string ToString ()
        {
            return _id;
        }
    }

}