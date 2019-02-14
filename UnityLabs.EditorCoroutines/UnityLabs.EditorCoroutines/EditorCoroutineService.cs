using System;
using System.Collections;
using System.Reflection;
using UnityEditor;


namespace Andeart.UnityLabs.EditorCoroutines
{

    /// <summary>
    /// Static service to help start EditorCoroutines.
    /// </summary>
    public static class EditorCoroutineService
    {
        /// <summary>
        /// Starts a coroutine named
        /// <param name="methodName" />
        /// .
        /// In most cases you want to use the <seealso cref="StartCoroutine(IEnumerator)" /> variation.
        /// You can <seealso cref="StopCoroutine(EditorWindow, string)" /> regardless of what approach/overload you used to start
        /// it.
        /// </summary>
        /// <param name="owner">The EditorWindow to be set as the owner of this EditorCoroutine.</param>
        public static EditorCoroutine StartCoroutine (this EditorWindow owner, string methodName)
        {
            return StartCoroutine (owner, methodName, null);
        }

        /// <summary>
        /// Starts a coroutine named
        /// <param name="methodName" />
        /// .
        /// In most cases you want to use the <seealso cref="StartCoroutine(IEnumerator)" /> variation.
        /// You can <seealso cref="StopCoroutine(EditorWindow, string)" /> regardless of what approach/overload you used to start
        /// it.
        /// </summary>
        /// <param name="owner">The EditorWindow to be set as the owner of this EditorCoroutine.</param>
        /// <param name="methodArgs">The parameters to pass to the method being invoked.</param>
        public static EditorCoroutine StartCoroutine (this EditorWindow owner, string methodName, params object[] methodArgs)
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
            return StartCoroutine (owner, routine);
        }

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
        /// You can <seealso cref="StopCoroutine(EditorWindow, string)" /> regardless of what approach/overload you used to start
        /// it.
        /// </summary>
        public static EditorCoroutine StartCoroutine (IEnumerator routine)
        {
            return StartCoroutine (null, routine);
        }

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
        /// You can <seealso cref="StopCoroutine(EditorWindow, string)" /> regardless of what approach/overload you used to start
        /// it.
        /// </summary>
        public static EditorCoroutine StartCoroutine (this EditorWindow owner, IEnumerator routine)
        {
            EditorCoroutine coroutine = EditorCoroutineFactory.Create (owner, routine);
            return EditorCoroutineUpdateService.Instance.StartCoroutine (coroutine);
        }

        /// <summary>
        /// Stops all running EditorCoroutine instances named
        /// <param name="methodName" />
        /// and belonging to the type of
        /// <param name="owner" />
        /// .
        /// </summary>
        public static void StopCoroutine (this EditorWindow owner, string methodName)
        {
            string coroutineId = EditorCoroutineFactory.GetId (owner, methodName);
            EditorCoroutineUpdateService.Instance.StopCoroutine (coroutineId);
        }

        /// <summary>
        /// Stops all running EditorCoroutine instances that were returned by
        /// <param name="routine" />
        /// .
        /// </summary>
        public static void StopCoroutine (IEnumerator routine)
        {
            StopCoroutine (null, routine);
        }

        /// <summary>
        /// Stops an EditorCoroutine instance specified by <param name="coroutine" />
        /// </summary>
        public static void StopCoroutine (EditorCoroutine coroutine)
        {
            EditorCoroutineUpdateService.Instance.StopCoroutine (coroutine);
        }

        /// <summary>
        /// Stops all running EditorCoroutine instances that were returned by
        /// <param name="routine" />
        /// and belonging to the type of
        /// <param name="owner" />
        /// .
        /// </summary>
        public static void StopCoroutine (this EditorWindow owner, IEnumerator routine)
        {
            string coroutineId = EditorCoroutineFactory.GetId (owner, routine);
            EditorCoroutineUpdateService.Instance.StopCoroutine (coroutineId);
        }

        /// <summary>
        /// Stops all running EditorCoroutine instances that belong to the type of
        /// <param name="owner" />
        /// .
        /// </summary>
        public static void StopAllCoroutines (this EditorWindow owner)
        {
            int ownerHash = EditorCoroutineFactory.GetOwnerHash (owner);
            EditorCoroutineUpdateService.Instance.StopAllCoroutines (ownerHash);
        }

        /// <summary>
        /// Stops all running EditorCoroutine instances. This is a Global "kill-all" mechanism.
        /// </summary>
        public static void StopAllCoroutines ()
        {
            EditorCoroutineUpdateService.Instance.StopAllCoroutines ();
        }
    }

}