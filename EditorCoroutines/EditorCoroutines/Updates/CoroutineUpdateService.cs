using Andeart.EditorCoroutines.Coroutines;
using System.Collections.Generic;
using System.Linq;


namespace Andeart.EditorCoroutines.Updates
{

    internal class CoroutineUpdateService<T> : IUpdateService<T> where T : ICoroutine
    {
        // Map ICoroutine.Id to all running instances of that method.
        private readonly Dictionary<string, List<T>> _coroutinesToEvaluate;

        public CoroutineUpdateService ()
        {
            _coroutinesToEvaluate = new Dictionary<string, List<T>> ();
        }


        #region START/STOP

        public T StartCoroutine (T coroutine)
        {
            if (!_coroutinesToEvaluate.ContainsKey (coroutine.Id))
            {
                _coroutinesToEvaluate[coroutine.Id] = new List<T> ();
            }

            _coroutinesToEvaluate[coroutine.Id].Add (coroutine);

            if (coroutine.Routine.MoveNext ())
            {
                coroutine.Evaluate ();
            }

            return coroutine;
        }

        public void StopCoroutine (string coroutineId)
        {
            _coroutinesToEvaluate.Remove (coroutineId);
        }

        public void StopCoroutine (T coroutine)
        {
            _coroutinesToEvaluate.Remove (coroutine.Id);
        }

        public void StopAllCoroutines (int ownerHash)
        {
            foreach (KeyValuePair<string, List<T>> coroutineInstanceListInfo in _coroutinesToEvaluate.ToList ())
            {
                List<T> coroutineInstanceList = coroutineInstanceListInfo.Value;
                coroutineInstanceList.RemoveAll (DoesCoroutineOwnerHashMatch);
                if (coroutineInstanceList.Count == 0)
                {
                    _coroutinesToEvaluate.Remove (coroutineInstanceListInfo.Key);
                }
            }

            bool DoesCoroutineOwnerHashMatch (T coroutine)
            {
                return coroutine.OwnerHash == ownerHash;
            }
        }

        public void StopAllCoroutines ()
        {
            _coroutinesToEvaluate.Clear ();
        }

        #endregion START/STOP


        #region UPDATE

        public void Update (double deltaTime, int deltaFrames)
        {
            if (_coroutinesToEvaluate.Count == 0)
            {
                return;
            }

            // The following uses _coroutinesToEvaluate.ToList () and not simply _coroutinesToEvaluate.
            // This is to cache current list of _coroutinesToEvaluate and evaluate only those, in case another ICoroutine is started while evaluating the current collection.
            foreach (KeyValuePair<string, List<T>> coroutineListInfo in _coroutinesToEvaluate.ToList ())
            {
                List<T> coroutineList = coroutineListInfo.Value;
                for (int j = coroutineList.Count - 1; j >= 0; j--) // Go backwards, to allow element removal during iteration.
                {
                    T coroutine = coroutineList[j];

                    if (!coroutine.CurrentYield.IsReadyToEvaluate (deltaTime, deltaFrames))
                    {
                        continue;
                    }

                    if (coroutine.Routine.MoveNext ())
                    {
                        coroutine.Evaluate ();
                        continue;
                    }

                    coroutineList.RemoveAt (j);
                    coroutine.CurrentYield = null;
                    coroutine.Finished = true;
                }

                if (coroutineList.Count == 0)
                {
                    _coroutinesToEvaluate.Remove (coroutineListInfo.Key);
                }
            }
        }

        #endregion UPDATE
    }

}