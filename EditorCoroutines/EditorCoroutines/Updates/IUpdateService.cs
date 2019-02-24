using Andeart.EditorCoroutines.Coroutines;


namespace Andeart.EditorCoroutines.Updates
{

    internal interface IUpdateService<T> where T : ICoroutine
    {
        T StartCoroutine (T coroutine);

        void StopCoroutine (string coroutineId);

        void StopCoroutine (T coroutine);

        void StopAllCoroutines (int ownerHash);

        void StopAllCoroutines ();

        void Update (double deltaTime, int deltaFrames);
    }

}