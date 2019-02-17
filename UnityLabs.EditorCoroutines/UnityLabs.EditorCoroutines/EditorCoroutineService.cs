using System;
using System.Collections;
using System.Reflection;


namespace Andeart.UnityLabs.EditorCoroutines
{

    /// <summary>
    /// Static service to help start EditorCoroutines.
    /// </summary>
    public static class EditorCoroutineService
    {
        /// <summary>
        /// Starts an EditorCoroutine.
        /// The execution of a coroutine can be paused at any point using the yield statement.
        /// When a yield statement is used, the coroutine will pause execution and automatically resume at the next frame.
        /// See the <see cref="UnityEngine.Coroutine" /> documentation for more details.
        /// Coroutines are excellent when modeling behavior over several frames. Coroutines have virtually no performance overhead.
        /// StartCoroutine function always returns immediately, however you can yield the result. Yielding waits until the
        /// coroutine has finished execution.
        /// There is no guarantee that coroutines end in the same order that they were started, even if they finish in the same
        /// frame.
        /// You can <seealso cref="StopCoroutine(object, string)" /> regardless of what approach/overload you used to start
        /// it.
        /// </summary>
        public static EditorCoroutine StartCoroutine (IEnumerator routine)
        {
            EditorCoroutine coroutine = CoroutineFactory.Create (null, routine);
            return CoroutineUpdateService.Instance.StartCoroutine (coroutine);
        }

        /// <summary>
        /// Starts a coroutine with the methodName on the owner object type.
        /// In most cases you want to use the <seealso cref="StartCoroutine(IEnumerator)" /> variation.
        /// You can <seealso cref="StopCoroutine(object, string)" /> regardless of what approach/overload you used to start
        /// it.
        /// </summary>
        /// <param name="owner">The EditorWindow to be set as the owner of this EditorCoroutine.</param>
        /// <param name="methodName">The name of the method on the owner object type.</param>
        /// <param name="methodArgs">The parameters to pass to the method being invoked.</param>
        public static EditorCoroutine StartCoroutine (object owner, string methodName, object[] methodArgs = null)
        {
            if (owner == null)
            {
                throw new ArgumentNullException (nameof(owner), "EditorCoroutine cannot be invoked by name from a null object.");
            }

            MethodInfo methodInfo = owner.GetType ().GetMethod (methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (methodInfo == null)
            {
                throw new ArgumentException ($"Coroutine with name {methodName} does not exist on instance of type {owner.GetType ()}.", nameof(methodName));
            }

            if (methodInfo.ReturnType != typeof(IEnumerator))
            {
                throw new ArgumentException ($"Coroutine with name {methodName} does not return an IEnumerator.", nameof(methodName));
            }

            object returned = methodInfo.Invoke (owner, methodArgs);
            IEnumerator routine = returned as IEnumerator;
            EditorCoroutine coroutine = CoroutineFactory.Create (owner, routine);
            return CoroutineUpdateService.Instance.StartCoroutine (coroutine);
        }

        /// <summary>
        /// Stops all running EditorCoroutine instances that were returned by
        /// <param name="routine" />
        /// .
        /// </summary>
        public static void StopCoroutine (IEnumerator routine)
        {
            string coroutineId = CoroutineFactory.CreateId (null, routine);
            CoroutineUpdateService.Instance.StopCoroutine (coroutineId);
        }

        /// <summary>
        /// Stops an EditorCoroutine instance specified by
        /// <param name="coroutine" />
        /// </summary>
        public static void StopCoroutine (EditorCoroutine coroutine)
        {
            CoroutineUpdateService.Instance.StopCoroutine (coroutine);
        }

        /// <summary>
        /// Stops all running EditorCoroutine instances named
        /// <param name="methodName" />
        /// and belonging to the type of
        /// <param name="owner" />
        /// .
        /// </summary>
        public static void StopCoroutine (object owner, string methodName)
        {
            string coroutineId = CoroutineFactory.CreateId (owner, methodName);
            CoroutineUpdateService.Instance.StopCoroutine (coroutineId);
        }

        /// <summary>
        /// Stops all running EditorCoroutine instances. This is a Global "kill-all" mechanism.
        /// </summary>
        public static void StopAllCoroutines ()
        {
            CoroutineUpdateService.Instance.StopAllCoroutines ();
        }

        /// <summary>
        /// Stops all running EditorCoroutine instances that belong to the type of
        /// <param name="owner" />
        /// .
        /// </summary>
        public static void StopAllCoroutines (object owner)
        {
            int ownerHash = CoroutineFactory.GetOwnerHash (owner);
            CoroutineUpdateService.Instance.StopAllCoroutines (ownerHash);
        }
    }

}