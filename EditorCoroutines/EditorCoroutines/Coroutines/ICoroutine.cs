using Andeart.EditorCoroutines.Yields;
using System.Collections;


namespace Andeart.EditorCoroutines.Coroutines
{

    internal interface ICoroutine
    {
        IYield CurrentYield { get; set; }
        bool Finished { get; set; }

        IEnumerator Routine { get; }
        int OwnerHash { get; }
        string Id { get; }

        void Evaluate ();
    }

}