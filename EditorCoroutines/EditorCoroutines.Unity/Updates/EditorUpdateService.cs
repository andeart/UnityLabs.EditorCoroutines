using Andeart.EditorCoroutines.Unity.Coroutines;
using Andeart.EditorCoroutines.Updates;
using UnityEditor;


namespace Andeart.EditorCoroutines.Unity.Updates
{

    internal class EditorUpdateService : IUpdateService<EditorCoroutine>
    {
        private readonly IUpdateService<EditorCoroutine> _coreUpdateService;
        private double _previousTimeSinceStartup;

        public EditorUpdateService ()
        {
            _coreUpdateService = new CoroutineUpdateService<EditorCoroutine> ();

            _previousTimeSinceStartup = EditorApplication.timeSinceStartup;
            EditorApplication.update -= OnUpdate;
            EditorApplication.update += OnUpdate;
        }

        private void OnUpdate ()
        {
            double deltaTime = EditorApplication.timeSinceStartup - _previousTimeSinceStartup;
            _previousTimeSinceStartup = EditorApplication.timeSinceStartup;
            Update (deltaTime, 1);
        }

        public EditorCoroutine StartCoroutine (EditorCoroutine coroutine)
        {
            return _coreUpdateService.StartCoroutine (coroutine);
        }

        public void StopCoroutine (string coroutineId)
        {
            _coreUpdateService.StopCoroutine (coroutineId);
        }

        public void StopCoroutine (EditorCoroutine coroutine)
        {
            _coreUpdateService.StopCoroutine (coroutine);
        }

        public void StopAllCoroutines (int ownerHash)
        {
            _coreUpdateService.StopAllCoroutines (ownerHash);
        }

        public void StopAllCoroutines ()
        {
            _coreUpdateService.StopAllCoroutines ();
        }

        public void Update (double deltaTime, int deltaFrames)
        {
            _coreUpdateService.Update (deltaTime, deltaFrames);
        }
    }

}