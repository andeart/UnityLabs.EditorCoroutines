﻿using System.Collections;


namespace Andeart.UnityLabs.EditorCoroutines
{

    internal static class EditorCoroutineFactory
    {
        public static EditorCoroutine Create (object owner, IEnumerator routine)
        {
            return new EditorCoroutine (GetOwnerHash (owner), routine);
        }

        public static string GetId (object owner, IEnumerator routine)
        {
            return GetId (GetOwnerHash (owner), GetCoreMethodName (routine));
        }

        public static string GetId (object owner, string methodName)
        {
            return GetId (GetOwnerHash (owner), methodName);
        }

        public static string GetId (int ownerHash, IEnumerator routine)
        {
            return GetId (ownerHash, GetCoreMethodName (routine));
        }

        public static string GetId (int ownerHash, string methodName)
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