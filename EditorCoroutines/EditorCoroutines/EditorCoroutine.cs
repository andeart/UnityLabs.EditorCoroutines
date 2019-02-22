using System;
using System.Collections;
using System.Reflection;
using UnityEngine;


namespace Andeart.EditorCoroutines
{

    /// <summary>
    /// <see cref="EditorCoroutineService.StartCoroutine(IEnumerator)"/> returns an EditorCoroutine.
    /// Instances of this class are only used to reference these coroutines, and do not hold any exposed properties or functions.
    /// 
    /// An EditorCoroutine is a function that can suspend its execution (yield) until the given <see cref="YieldInstruction"/> finishes.
    /// </summary>
    public class EditorCoroutine
    {
        internal IYield CurrentYield { get; set; }
        internal bool Finished { get; set; }

        internal IEnumerator Routine { get; }
        internal int OwnerHash { get; }
        internal string Id { get; }

        private EditorCoroutine () { }

        internal EditorCoroutine (int ownerHash, IEnumerator routine)
        {
            CurrentYield = new Yield.Default ();
            Finished = false;

            Routine = routine ?? throw new ArgumentNullException (nameof(routine), "Routine used to start an EditorCoroutine cannot be null.");
            OwnerHash = ownerHash;
            Id = CoroutineFactory.CreateId (OwnerHash, routine);
        }

        internal void Evaluate ()
        {
            object current = Routine.Current;
            switch (current)
            {
                case null:
                case WaitForFixedUpdate _:
                case WaitForEndOfFrame _:
                    CurrentYield = new Yield.WaitForFrames (1);
                    break;
                case WaitForSeconds waitForSeconds:
                    const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
                    FieldInfo field = typeof(WaitForSeconds).GetField ("m_Seconds", bindingFlags);
                    float seconds = float.Parse (field.GetValue (waitForSeconds).ToString ());
                    CurrentYield = new Yield.WaitForSeconds (seconds);
                    break;
                case AsyncOperation asyncOperation:
                    CurrentYield = new Yield.AsyncOperation (asyncOperation);
                    break;
                case CustomYieldInstruction customYieldInstruction:
                    CurrentYield = new Yield.CustomYieldInstruction (customYieldInstruction);
                    break;
                case EditorCoroutine coroutine:
                    CurrentYield = new Yield.NestedCoroutine (coroutine);
                    break;
                default:
                    CurrentYield = new Yield.Default ();
                    break;
            }
        }

        /// <summary>
        /// Determines whether the specified object is the same as this EditorCoroutine.
        /// </summary>
        public override bool Equals (object obj)
        {
            if (obj is EditorCoroutine coroutine)
            {
                return Id == coroutine.Id;
            }

            return false;
        }

        /// <summary>
        /// A hash code for this EditorCoroutine.
        /// </summary>
        public override int GetHashCode ()
        {
            return Id.GetHashCode ();
        }

        /// <summary>
        /// A string that represents this EditorCoroutine.
        /// </summary>
        public override string ToString ()
        {
            return Id;
        }
    }

}