using System.Collections;


namespace Andeart.UnityLabs.EditorCoroutines
{

    internal static class CoroutineFactory
    {
        public static EditorCoroutine Create (object owner, IEnumerator routine)
        {
            return new EditorCoroutine (GetOwnerHash (owner), routine);
        }

        public static string CreateId (object owner, IEnumerator routine)
        {
            return CreateId (GetOwnerHash (owner), GetCoreMethodName (routine));
        }

        public static string CreateId (object owner, string methodName)
        {
            return CreateId (GetOwnerHash (owner), methodName);
        }

        public static string CreateId (int ownerHash, IEnumerator routine)
        {
            return CreateId (ownerHash, GetCoreMethodName (routine));
        }

        public static string CreateId (int ownerHash, string methodName)
        {
            return ownerHash + "_" + methodName;
        }

        public static int GetOwnerHash (object owner)
        {
            return owner?.GetHashCode () ?? -1;
        }

        public static string GetCoreMethodName (IEnumerator routine)
        {
            string methodName = routine.ToString (); // Backup name, in case we're unable to parse the real method name.
            var split = routine.ToString ().Split ('<', '>');
            if (split.Length == 3)
            {
                methodName = split[1];
            }

            return methodName;
        }
    }

}