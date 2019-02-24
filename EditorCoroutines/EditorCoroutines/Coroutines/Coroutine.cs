using Andeart.EditorCoroutines.Yields;
using System;
using System.Collections;


namespace Andeart.EditorCoroutines.Coroutines
{

    internal class Coroutine : ICoroutine
    {
        public IYield CurrentYield { get; set; }
        public bool Finished { get; set; }
        public IEnumerator Routine { get; }
        public int OwnerHash { get; }
        public string Id { get; }

        internal Coroutine (int ownerHash, IEnumerator routine, string id)
        {
            CurrentYield = new Yield.Default ();
            Finished = false;

            Routine = routine ?? throw new ArgumentNullException (nameof(routine), "Routine used to start a Coroutine cannot be null.");
            OwnerHash = ownerHash;
            Id = id;
        }

        public void Evaluate ()
        {
            CurrentYield = new Yield.Default ();
        }

        public override bool Equals (object obj)
        {
            if (obj is ICoroutine coroutine)
            {
                return Id == coroutine.Id;
            }

            return false;
        }

        public override int GetHashCode ()
        {
            return Id.GetHashCode ();
        }

        public override string ToString ()
        {
            return Id;
        }
    }

}