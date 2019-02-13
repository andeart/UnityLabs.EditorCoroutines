using System.Collections.Generic;
using UnityEditor;


namespace Andeart.UnityLabs.EditorCoroutines
{

    internal class EditorCoroutineUpdateService
    {
        // Map EditorCoroutine.Id to all running instances of that method.
        private readonly Dictionary<string, List<EditorCoroutine>> _coroutinesToEvaluate;
        private readonly List<string> _stoppedIds;

        private double _previousTimeSinceStartup;

        public static EditorCoroutineUpdateService Instance { get; }

        // Begone, beforefieldinit.
        static EditorCoroutineUpdateService ()
        {
            Instance = new EditorCoroutineUpdateService ();
        }

        private EditorCoroutineUpdateService ()
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
            foreach (var coroutineListInfo in _coroutinesToEvaluate)
            {
                var coroutineList = coroutineListInfo.Value;
                for (int i = coroutineList.Count - 1; i >= 0; i--) // Go backwards, to allow element removal during iteration.
                {
                    EditorCoroutine coroutine = coroutineList[i];

                    if (!coroutine.CurrentYield.IsReadyToEvaluate (deltaTime, 1))
                    {
                        continue;
                    }

                    if (coroutine.Routine.MoveNext ())
                    {
                        coroutine.Evaluate ();
                        continue;
                    }

                    coroutineList.RemoveAt (i);
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