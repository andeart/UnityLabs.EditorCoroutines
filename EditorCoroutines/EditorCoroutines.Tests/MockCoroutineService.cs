using Andeart.EditorCoroutines.Coroutines;
using Andeart.EditorCoroutines.Updates;
using System;
using System.Collections;
using System.Reflection;


namespace Andeart.EditorCoroutines.Tests
{

    internal class MockCoroutineService
    {
        private readonly ICoroutineFactory<Coroutine> _coroutineFactory;
        internal IUpdateService<Coroutine> UpdateService { get; }

        public MockCoroutineService ()
        {
            UpdateService = new CoroutineUpdateService<Coroutine> ();
            _coroutineFactory = new CoroutineFactory ();
        }

        public Coroutine StartCoroutine (IEnumerator routine)
        {
            Coroutine coroutine = _coroutineFactory.Create (null, routine);
            return UpdateService.StartCoroutine (coroutine);
        }

        public Coroutine StartCoroutine (object owner, string methodName, object[] methodArgs = null)
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
            Coroutine coroutine = _coroutineFactory.Create (owner, routine);
            return UpdateService.StartCoroutine (coroutine);
        }

        public void StopCoroutine (IEnumerator routine)
        {
            string coroutineId = _coroutineFactory.CreateId (null, routine);
            UpdateService.StopCoroutine (coroutineId);
        }

        public void StopCoroutine (Coroutine coroutine)
        {
            UpdateService.StopCoroutine (coroutine);
        }

        public void StopCoroutine (object owner, string methodName)
        {
            string coroutineId = _coroutineFactory.CreateId (owner, methodName);
            UpdateService.StopCoroutine (coroutineId);
        }

        public void StopAllCoroutines ()
        {
            UpdateService.StopAllCoroutines ();
        }

        public void StopAllCoroutines (object owner)
        {
            int ownerHash = _coroutineFactory.GetOwnerHash (owner);
            UpdateService.StopAllCoroutines (ownerHash);
        }
    }

}