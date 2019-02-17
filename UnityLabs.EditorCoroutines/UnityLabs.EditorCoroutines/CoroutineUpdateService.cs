using System.Collections.Generic;
using System.Linq;
using UnityEditor;


namespace Andeart.UnityLabs.EditorCoroutines
{

    internal class CoroutineUpdateService
    {
        // Map EditorCoroutine.Id to all running instances of that method.
        private readonly Dictionary<string, List<EditorCoroutine>> _coroutinesToEvaluate;
        private readonly List<string> _stoppedIds;

        private double _previousTimeSinceStartup;

        public static CoroutineUpdateService Instance { get; }

        // Begone, beforefieldinit.
        static CoroutineUpdateService ()
        {
            Instance = new CoroutineUpdateService ();
        }

        private CoroutineUpdateService ()
        {
            _coroutinesToEvaluate = new Dictionary<string, List<EditorCoroutine>> ();
            _stoppedIds = new List<string> ();
        }

        private void Initialize ()
        {
            _previousTimeSinceStartup = EditorApplication.timeSinceStartup;
        }

        [InitializeOnLoadMethod]
        private static void InitializeOnLoad ()
        {
            Instance.Initialize ();
            EditorApplication.update -= Instance.OnUpdate;
            EditorApplication.update += Instance.OnUpdate;
        }


        #region START/STOP

        public EditorCoroutine StartCoroutine (EditorCoroutine coroutine)
        {
            if (!_coroutinesToEvaluate.ContainsKey (coroutine.Id))
            {
                _coroutinesToEvaluate[coroutine.Id] = new List<EditorCoroutine> ();
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

        public void StopCoroutine (EditorCoroutine coroutine)
        {
            _coroutinesToEvaluate.Remove (coroutine.Id);
        }

        public void StopAllCoroutines (int ownerHash)
        {
            _stoppedIds.Clear ();
            foreach (var coroutineInstanceListInfo in _coroutinesToEvaluate)
            {
                var coroutineInstanceList = coroutineInstanceListInfo.Value;
                coroutineInstanceList.RemoveAll (DoesCoroutineOwnerHashMatch);
                if (coroutineInstanceList.Count == 0)
                {
                    _stoppedIds.Add (coroutineInstanceListInfo.Key);
                }
            }

            for (int i = 0; i < _stoppedIds.Count; i++)
            {
                _coroutinesToEvaluate.Remove (_stoppedIds[i]);
            }

            bool DoesCoroutineOwnerHashMatch (EditorCoroutine coroutine)
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

        private void OnUpdate ()
        {
            double deltaTime = EditorApplication.timeSinceStartup - _previousTimeSinceStartup;
            _previousTimeSinceStartup = EditorApplication.timeSinceStartup;
            if (_coroutinesToEvaluate.Count == 0)
            {
                return;
            }

            _stoppedIds.Clear ();
            // The following uses _coroutinesToEvaluate.ToList () and not simply _coroutinesToEvaluate.
            // This is to cache current list of _coroutinesToEvaluate and evaluate only those, in case another EditorCoroutine is started while evaluating the current collection.
            foreach (var coroutineListInfo in _coroutinesToEvaluate.ToList ())
            {
                var coroutineList = coroutineListInfo.Value;
                for (int j = coroutineList.Count - 1; j >= 0; j--) // Go backwards, to allow element removal during iteration.
                {
                    EditorCoroutine coroutine = coroutineList[j];

                    if (!coroutine.CurrentYield.IsReadyToEvaluate (deltaTime, 1))
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
                    _stoppedIds.Add (coroutineListInfo.Key);
                }
            }

            for (int i = 0; i < _stoppedIds.Count; i++)
            {
                _coroutinesToEvaluate.Remove (_stoppedIds[i]);
            }
        }

        #endregion UPDATE


    }

}