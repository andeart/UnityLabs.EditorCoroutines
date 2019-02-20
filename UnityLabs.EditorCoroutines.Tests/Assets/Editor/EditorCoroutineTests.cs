using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.TestTools;


namespace Andeart.UnityLabs.EditorCoroutines.Tests
{

    public class EditorCoroutineTests
    {


        #region STARTCOROUTINE

        [UnityTest]
        public IEnumerator StartCoroutine_IEnumeratorArg_StartsCorrectly ()
        {
            RefBool isTestComplete = new RefBool ();

            EditorCoroutineService.StartCoroutine (SimpleIterator (isTestComplete));
            yield return null;

            Assert.IsTrue (isTestComplete.Value, "EditorCoroutine was not started correctly via IEnumerator arg.");
        }

        [UnityTest]
        public IEnumerator StartCoroutine_MethodNameArg_StartsCorrectly ()
        {
            RefBool isTestComplete = new RefBool ();
            object owner = this;
            const string methodName = "SimpleIterator";

            EditorCoroutineService.StartCoroutine (owner, methodName, new object[] {isTestComplete});
            yield return null;

            Assert.IsTrue (isTestComplete.Value, $"EditorCoroutine was not started correctly via method name {methodName}.");
        }

        private IEnumerator SimpleIterator (RefBool isTestComplete)
        {
            TestContext.WriteLine ("In SimpleIterator routine. Starting.");
            yield return null;
            isTestComplete.Value = true;
            TestContext.WriteLine ("In SimpleIterator routine. Finished.");
        }

        #endregion STARTCOROUTINE


        #region WAIT FOR SECONDS

        [UnityTest]
        public IEnumerator StartCoroutine_YieldWaitForSeconds_SuccessAfterSeconds ()
        {
            RefBool isTestComplete = new RefBool ();
            DateTime then = DateTime.Now;
            const float secondsToWait = 2f;

            EditorCoroutineService.StartCoroutine (ReturnAfterSeconds (secondsToWait, isTestComplete));
            while (!isTestComplete.Value)
            {
                yield return null;
            }

            double deltaSeconds = (DateTime.Now - then).TotalSeconds;

            Assert.AreEqual (secondsToWait,
                             deltaSeconds,
                             0.015,
                             $"WaitForSeconds EditorCoroutine returned in {deltaSeconds}, when expected to return in {secondsToWait} seconds.");
        }

        private IEnumerator ReturnAfterSeconds (float seconds, RefBool isTestComplete)
        {
            TestContext.WriteLine ("In ReturnAfterSeconds routine. Starting.");
            yield return new WaitForSeconds (seconds);
            isTestComplete.Value = true;
            TestContext.WriteLine ($"In ReturnAfterSeconds routine. Finished after waiting for {seconds} seconds.");
        }

        #endregion WAIT FOR SECONDS


        #region WAIT FOR ASYNCOPERATION

        private byte[] _webRequestData;

        [UnityTest]
        public IEnumerator StartCoroutine_YieldWaitForAsyncOperation_ReturnsCorrectly ()
        {
            RefBool isTestComplete = new RefBool ();
            _webRequestData = null;
            const string uri = "http://httpbin.org/get";

            EditorCoroutineService.StartCoroutine (ReturnAfterWebRequest (uri, isTestComplete));
            while (!isTestComplete.Value)
            {
                yield return null;
            }

            Assert.IsNotNull (_webRequestData,
                              $"WebRequest EditorCoroutine did not return any data.\nThis may be an issue with the URI {uri} itself, but for now, this test has failed.");
        }

        private IEnumerator ReturnAfterWebRequest (string uri, RefBool isTestComplete)
        {
            TestContext.WriteLine ("In ReturnAfterWebRequest routine. Starting.");
            UnityWebRequest www = UnityWebRequest.Get (uri);
            yield return www.SendWebRequest ();

            if (www.isNetworkError || www.isHttpError)
            {
                TestContext.WriteLine ($"Request has returned with error:\n{www.error}");
                throw new Exception (www.error);
            }

            TestContext.WriteLine ($"Request has returned successfully with text:\n{www.downloadHandler.text}");
            _webRequestData = www.downloadHandler.data;
            isTestComplete.Value = true;
            TestContext.WriteLine ("In ReturnAfterWebRequest routine. Finished.");
        }

        #endregion WAIT FOR ASYNCOPERATION


        #region WAIT FOR NESTED COROUTINES

        private Stack<int> _nestedRoutinesTracker;

        [UnityTest]
        public IEnumerator StartCoroutine_YieldWaitForNestedCoroutines_ReturnsInCorrectOrder ()
        {
            RefBool isTestComplete = new RefBool ();
            _nestedRoutinesTracker = new Stack<int> ();
            for (int i = 0; i < 3; i++)
            {
                _nestedRoutinesTracker.Push (i);
            }

            EditorCoroutineService.StartCoroutine (ReturnAfterNestedRoutines (isTestComplete));
            while (!isTestComplete.Value)
            {
                yield return null;
            }

            Assert.AreEqual (0, _nestedRoutinesTracker.Count, "Nested EditorCoroutines were not run in the correct order.");
        }

        private IEnumerator ReturnAfterNestedRoutines (RefBool isTestComplete)
        {
            const float secondsToWait = 1f;

            TestContext.WriteLine ("In parent routine. Starting.");
            yield return new WaitForSeconds (secondsToWait);
            yield return EditorCoroutineService.StartCoroutine (ReturnAfterNestedRoutinesDepthOne ());
            if (_nestedRoutinesTracker.Count == 1)
            {
                _nestedRoutinesTracker.Pop ();
            } else
            {
                throw new Exception ("Parent routine is run in wrong order.");
            }

            isTestComplete.Value = true;
            TestContext.WriteLine ("In parent routine. Finished.");
        }

        private IEnumerator ReturnAfterNestedRoutinesDepthOne ()
        {
            const float secondsToWait = 1f;

            TestContext.WriteLine ("In nested routine depth one. Starting.");
            yield return new WaitForSeconds (secondsToWait);
            yield return EditorCoroutineService.StartCoroutine (ReturnAfterNestedRoutinesDepthTwo ());
            if (_nestedRoutinesTracker.Count == 2)
            {
                _nestedRoutinesTracker.Pop ();
            } else
            {
                throw new Exception ("Nested routine depth one is run in wrong order.");
            }

            TestContext.WriteLine ("In nested routine depth one. Finished.");
        }

        private IEnumerator ReturnAfterNestedRoutinesDepthTwo ()
        {
            const float secondsToWait = 1f;

            TestContext.WriteLine ($"In nested routine depth two. Starting. Will automatically finish in {secondsToWait} seconds.");
            yield return new WaitForSeconds (1f);
            if (_nestedRoutinesTracker.Count == 3)
            {
                _nestedRoutinesTracker.Pop ();
            } else
            {
                throw new Exception ("Nested routine depth two is run in wrong order.");
            }

            TestContext.WriteLine ("In nested routine depth two. Finished.");
        }

        #endregion WAIT FOR NESTED COROUTINES


        #region WAIT FOR COROUTINES RUN IN PARALLEL

        private Queue<char> _parallelRoutinesTracker;

        [UnityTest]
        public IEnumerator StartCoroutine_IndependentCoroutinesInParallel_ReturnInCorrectOrder ()
        {
            RefBool isTestComplete = new RefBool ();
            _parallelRoutinesTracker = new Queue<char> ();

            EditorCoroutineService.StartCoroutine (ReturnAfterRoutinesRunInParallel (isTestComplete));
            while (!isTestComplete.Value)
            {
                yield return null;
            }

            Assert.AreEqual (0, _parallelRoutinesTracker.Count, "EditorCoroutines started in parallel were not run in the correct order.");
        }

        private IEnumerator ReturnAfterRoutinesRunInParallel (RefBool isTestComplete)
        {
            const float secondsToWaitInEachRoutine = 2f;
            const float secondRoutineStartDelay = 0.5f;

            TestContext.WriteLine ("In ReturnAfterRoutinesRunInParallel routine. Starting.");
            _parallelRoutinesTracker.Enqueue ('a');
            EditorCoroutineService.StartCoroutine (ReturnAfterSecondsA (secondsToWaitInEachRoutine));
            TestContext.WriteLine ($"In ReturnAfterRoutinesRunInParallel routine. Waiting for {secondRoutineStartDelay} seconds.");
            yield return new WaitForSeconds (secondRoutineStartDelay);
            _parallelRoutinesTracker.Enqueue ('b');
            EditorCoroutineService.StartCoroutine (ReturnAfterSecondsB (secondsToWaitInEachRoutine, isTestComplete));
            TestContext.WriteLine ("In ReturnAfterRoutinesRunInParallel routine. Finished, though an EditorCoroutine started in parallel may still be running.");
        }

        private IEnumerator ReturnAfterSecondsA (float secondsToWait)
        {
            TestContext.WriteLine ($"In ReturnAfterSecondsA routine. Starting. Will automatically finish in {secondsToWait} seconds.");
            yield return new WaitForSeconds (secondsToWait);
            if (_parallelRoutinesTracker.Peek () == 'a')
            {
                _parallelRoutinesTracker.Dequeue ();
            } else
            {
                throw new Exception ("Parallel EditorCoroutine A is run in wrong order.");
            }

            TestContext.WriteLine ("In ReturnAfterSecondsA routine. Finished.");
        }

        private IEnumerator ReturnAfterSecondsB (float secondsToWait, RefBool isTestComplete)
        {
            TestContext.WriteLine ($"In ReturnAfterSecondsB routine. Starting. Will automatically finish in {secondsToWait} seconds.");
            yield return new WaitForSeconds (secondsToWait);
            if (_parallelRoutinesTracker.Peek () == 'b')
            {
                _parallelRoutinesTracker.Dequeue ();
            } else
            {
                throw new Exception ("Parallel EditorCoroutine B is run in wrong order.");
            }

            isTestComplete.Value = true;
            TestContext.WriteLine ("In ReturnAfterSecondsB routine. Finished.");
        }

        #endregion WAIT FOR COROUTINES RUN IN PARALLEL


        #region WAIT FOR CUSTOMYIELDINSTRUCTION

        private int _customIndex;

        [UnityTest]
        public IEnumerator StartCoroutine_CustomYieldInstruction_ReturnsCorrectly ()
        {
            RefBool isTestComplete = new RefBool ();
            _customIndex = 0;

            EditorCoroutineService.StartCoroutine (ReturnAfterCustomYieldInstructionSucceeds (isTestComplete));
            while (!isTestComplete.Value)
            {
                yield return null;
            }

            Assert.AreEqual (1, _customIndex, "EditorCoroutine yielding on a CustomYieldInstruction was not returned correctly.");
        }

        private IEnumerator ReturnAfterCustomYieldInstructionSucceeds (RefBool isTestComplete)
        {
            TestContext.WriteLine ("In ReturnAfterCustomYieldInstructionSucceeds routine. Starting.");
            EditorCoroutineService.StartCoroutine (SetIndexAfterSeconds (2f));
            yield return new CustomWaitWhile (IsIndexStillZero);

            TestContext.WriteLine ("In ReturnAfterCustomYieldInstructionSucceeds routine. Finished.");
            isTestComplete.Value = true;

            bool IsIndexStillZero ()
            {
                return _customIndex == 0;
            }
        }

        private IEnumerator SetIndexAfterSeconds (float secondsToWait)
        {
            TestContext.WriteLine ($"In SetIndexAfterSeconds routine. Starting. Will automatically finish in {secondsToWait} seconds.");
            yield return new WaitForSeconds (secondsToWait);
            _customIndex = 1;
            TestContext.WriteLine ("In SetIndexAfterSeconds routine. Finished.");
        }


        private class CustomWaitWhile : CustomYieldInstruction
        {
            private readonly Func<bool> _predicate;

            public override bool keepWaiting => _predicate ();

            public CustomWaitWhile (Func<bool> predicate)
            {
                _predicate = predicate;
            }
        }

        #endregion WAIT FOR CUSTOMYIELDINSTRUCTION


        #region STOPCOROUTINE

        [UnityTest]
        public IEnumerator StopCoroutine_IEnumeratorArg_StopsCorrectly ()
        {
            RefBool isTestComplete = new RefBool ();
            RefBool wasCancelled = new RefBool ();

            // Test coroutine will be stopped. Check in 1 second for expected results. Test coroutine should finish by then, successfully or not.
            EditorCoroutineService.StartCoroutine (ReturnAfterSecondsToBeStopped (1f, isTestComplete));

            TestContext.WriteLine ("Starting SimpleIteratorToBeStopped routine.\nWill automatically attempt to stop it after a frame.");
            // This coroutine will be attempted to be cancelled after 1 frame, in the line after the following `yield return null`.
            EditorCoroutineService.StartCoroutine (SimpleIteratorToBeStopped (wasCancelled));
            yield return null;
            // StopCoroutine IEnumerator args don't need to be the exact same.
            EditorCoroutineService.StopCoroutine (SimpleIteratorToBeStopped (new RefBool ()));

            while (!isTestComplete.Value)
            {
                yield return null;
            }

            Assert.IsTrue (wasCancelled.Value, "EditorCoroutine was not stopped correctly via IEnumerator arg.");
        }

        [UnityTest]
        public IEnumerator StopCoroutine_EditorCoroutineArg_StopsCorrectly ()
        {
            RefBool isTestComplete = new RefBool ();
            RefBool wasCancelled = new RefBool ();

            // Test coroutine will be stopped. Check in 1 second for expected results. Test coroutine should finish by then, successfully or not.
            EditorCoroutineService.StartCoroutine (ReturnAfterSecondsToBeStopped (1f, isTestComplete));

            TestContext.WriteLine ("Starting SimpleIteratorToBeStopped routine.\nWill automatically attempt to stop it after a frame.");
            // This coroutine will be attempted to be cancelled after 1 frame, in the line after the following `yield return null`.
            EditorCoroutine editorCoroutine = EditorCoroutineService.StartCoroutine (SimpleIteratorToBeStopped (wasCancelled));
            yield return null;
            EditorCoroutineService.StopCoroutine (editorCoroutine);

            while (!isTestComplete.Value)
            {
                yield return null;
            }

            Assert.IsTrue (wasCancelled.Value, "EditorCoroutine was not stopped correctly via Editor Coroutine reference arg.");
        }

        [UnityTest]
        public IEnumerator StopCoroutine_MethodName_StopsCorrectly ()
        {
            RefBool isTestComplete = new RefBool ();
            RefBool wasCancelled = new RefBool ();

            // Test coroutine will be stopped. Check in 1 second for expected results. Test coroutine should finish by then, successfully or not.
            EditorCoroutineService.StartCoroutine (ReturnAfterSecondsToBeStopped (1f, isTestComplete));

            TestContext.WriteLine ("Starting SimpleIteratorToBeStopped routine.\nWill automatically attempt to stop it after a frame.");
            // This coroutine will be attempted to be cancelled after 1 frame, in the line after the following `yield return null`.
            EditorCoroutineService.StartCoroutine (this, "SimpleIteratorToBeStopped", new object[] {wasCancelled});
            yield return null;
            EditorCoroutineService.StopCoroutine (this, "SimpleIteratorToBeStopped");

            while (!isTestComplete.Value)
            {
                yield return null;
            }

            Assert.IsTrue (wasCancelled.Value, "EditorCoroutine was not stopped correctly via Method Name arg.");
        }

        private IEnumerator SimpleIteratorToBeStopped (RefBool wasCancelled)
        {
            TestContext.WriteLine ("In SimpleIteratorToBeStopped routine. Starting.");
            yield return null;
            TestContext.WriteLine ("In SimpleIteratorToBeStopped routine. This routine may be stopped now. If so, this should be the last log from this method.");
            wasCancelled.Value = true;
            yield return null;
            wasCancelled.Value = false;
            TestContext.WriteLine ("In SimpleIteratorToBeStopped routine. Finished. This line should not be logged if the coroutine was stopped successfully.");
        }

        private IEnumerator ReturnAfterSecondsToBeStopped (float secondsToWait, RefBool isTestComplete)
        {
            TestContext.WriteLine ($"In ReturnAfterSecondsToBeStopped routine. Starting. Will automatically finish in {secondsToWait} seconds.");
            yield return new WaitForSeconds (secondsToWait);
            isTestComplete.Value = true;
            TestContext.WriteLine ("In ReturnAfterSecondsToBeStopped routine. Finished.");
        }

        #endregion STOPCOROUTINE


        #region STOPALLCOROUTINES

        [UnityTest]
        public IEnumerator StopAllCoroutines_Global_StopsAllCoroutinesCorrectly ()
        {
            OwnerOfRoutine ownerA = new OwnerOfRoutine ();
            RefBool wasCancelledA = new RefBool ();
            RefBool wasCancelledB = new RefBool ();

            // Test coroutines will be stopped. Check in 1 second for expected results. Test coroutines should finish by then, successfully or not.
            DateTime then = DateTime.Now;
            const double secondsToWait = 1;

            TestContext.WriteLine ("Starting SimpleIteratorToBeStoppedB and ownerA.SimpleIteratorToBeStoppedA routines.\nWill automatically attempt to stop all running routines after a frame.");
            // All the following coroutines will be attempted to be cancelled after 1 frame, in the line after the following `yield return null`.
            EditorCoroutineService.StartCoroutine (ownerA.SimpleIteratorToBeStoppedA (wasCancelledA));
            EditorCoroutineService.StartCoroutine (SimpleIteratorToBeStoppedB (wasCancelledB));
            yield return null;
            EditorCoroutineService.StopAllCoroutines ();

            double deltaSeconds = (DateTime.Now - then).TotalSeconds;
            while (deltaSeconds < secondsToWait)
            {
                yield return null;
                deltaSeconds = (DateTime.Now - then).TotalSeconds;
            }

            Assert.IsTrue (wasCancelledA.Value && wasCancelledB.Value, "Global stop for all EditorCoroutines was not executed correctly.");
        }

        [UnityTest]
        public IEnumerator StopAllCoroutines_SpecificOwnerArg_StopsAllCoroutinesCorrectly ()
        {
            OwnerOfRoutine ownerA = new OwnerOfRoutine ();
            RefBool wasCancelledA = new RefBool ();
            RefBool wasCancelledB = new RefBool ();

            // Test coroutines will be stopped. Check in 1 second for expected results. Test coroutines should finish by then, successfully or not.
            DateTime then = DateTime.Now;
            const double secondsToWait = 1;

            TestContext.WriteLine ("Starting SimpleIteratorToBeStoppedB and ownerA.SimpleIteratorToBeStoppedA routines.\nWill automatically attempt to stop routines belonging to ownerA after a frame.");
            // All the following coroutines will be attempted to be cancelled after 1 frame, in the line after the following `yield return null`.
            EditorCoroutineService.StartCoroutine (ownerA, "SimpleIteratorToBeStoppedA", new object[] {wasCancelledA});
            EditorCoroutineService.StartCoroutine (SimpleIteratorToBeStoppedB (wasCancelledB));
            yield return null;
            EditorCoroutineService.StopAllCoroutines (ownerA);

            double deltaSeconds = (DateTime.Now - then).TotalSeconds;
            while (deltaSeconds < secondsToWait)
            {
                yield return null;
                deltaSeconds = (DateTime.Now - then).TotalSeconds;
            }

            Assert.IsTrue (wasCancelledA.Value && !wasCancelledB.Value, "EditorCoroutines belonging to a specific owner were not stopped correctly.");
        }


        private class OwnerOfRoutine
        {
            public IEnumerator SimpleIteratorToBeStoppedA (RefBool wasCancelled)
            {
                TestContext.WriteLine ("In SimpleIteratorToBeStoppedA routine. Starting.");
                yield return null;
                TestContext.WriteLine ("In SimpleIteratorToBeStoppedA routine. This routine may be stopped now. If so, this should be the last log from this method.");
                wasCancelled.Value = true;
                yield return null;
                wasCancelled.Value = false;
                TestContext.WriteLine ("In SimpleIteratorToBeStoppedA routine. Finished. This line should not be logged if the coroutine was stopped successfully.");
            }
        }


        private IEnumerator SimpleIteratorToBeStoppedB (RefBool wasCancelled)
        {
            TestContext.WriteLine ("In SimpleIteratorToBeStoppedB routine. Starting.");
            yield return null;
            TestContext.WriteLine ("In SimpleIteratorToBeStoppedB routine. This routine may be stopped now. If so, this should be the last log from this method.");
            wasCancelled.Value = true;
            yield return null;
            wasCancelled.Value = false;
            TestContext.WriteLine ("In SimpleIteratorToBeStoppedB routine. Finished. This line should not be logged if the coroutine was stopped successfully.");
        }

        #endregion STOPALLCOROUTINES


        // A reference type to represent bool, since we can't pass a `ref bool` to the test iterator methods.
        private class RefBool
        {
            public bool Value { get; set; }

            public RefBool ()
            {
                Value = false;
            }
        }
    }

}