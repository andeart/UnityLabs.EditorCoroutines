using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;


namespace Andeart.EditorCoroutines.Tests
{

    [TestClass]
    public class CoreTests
    {
        private readonly MockCoroutineService _mockCoroutineService;

        // Gets or sets the test context which provides information about and functionality for the current test run.
        public TestContext TestContext { get; set; }

        public CoreTests ()
        {
            _mockCoroutineService = new MockCoroutineService ();
        }

        /// <summary>
        /// Mock the Unity EditorApplication.Update.
        /// A very simplistic Iterator evaluation.
        /// </summary>
        private void MockApplicationUpdate (IEnumerator routine)
        {
            DateTime then = DateTime.Now;
            while (routine.MoveNext ())
            {
                double deltaTime = (DateTime.Now - then).TotalSeconds;
                then = DateTime.Now;
                _mockCoroutineService.UpdateService.Update (deltaTime, 1);
            }
        }


        #region STARTCOROUTINE

        [TestMethod]
        public void StartCoroutine_IEnumeratorArg_StartsCorrectly ()
        {
            MockApplicationUpdate (Routine ());

            IEnumerator Routine ()
            {
                RefBool isTestComplete = new RefBool ();

                _mockCoroutineService.StartCoroutine (SimpleIterator (isTestComplete));
                yield return null;

                Assert.IsTrue (isTestComplete.Value, "EditorCoroutine was not started correctly via IEnumerator arg.");
            }
        }

        [TestMethod]
        public void StartCoroutine_MethodNameArg_StartsCorrectly ()
        {
            MockApplicationUpdate (Routine ());

            IEnumerator Routine ()
            {
                RefBool isTestComplete = new RefBool ();
                object owner = this;
                const string methodName = "SimpleIterator";

                _mockCoroutineService.StartCoroutine (owner, methodName, new object[] {isTestComplete});
                yield return null;

                Assert.IsTrue (isTestComplete.Value, $"EditorCoroutine was not started correctly via method name {methodName}.");
            }
        }

        private IEnumerator SimpleIterator (RefBool isTestComplete)
        {
            TestContext.WriteLine ("In SimpleIterator routine. Starting.");
            yield return null;
            isTestComplete.Value = true;
            TestContext.WriteLine ("In SimpleIterator routine. Finished.");
        }

        #endregion STARTCOROUTINE


        #region WAIT FOR NESTED COROUTINES

        private Stack<int> _nestedRoutinesTracker;

        [TestMethod]
        public void StartCoroutine_YieldWaitForNestedCoroutines_ReturnsInCorrectOrder ()
        {
            MockApplicationUpdate (Routine ());

            IEnumerator Routine ()
            {
                RefBool isTestComplete = new RefBool ();
                _nestedRoutinesTracker = new Stack<int> ();
                for (int i = 0; i < 3; i++)
                {
                    _nestedRoutinesTracker.Push (i);
                }

                _mockCoroutineService.StartCoroutine (ReturnAfterNestedRoutines (isTestComplete));
                while (!isTestComplete.Value)
                {
                    yield return null;
                }

                Assert.AreEqual (0, _nestedRoutinesTracker.Count, "Nested EditorCoroutines were not run in the correct order.");
            }
        }

        private IEnumerator ReturnAfterNestedRoutines (RefBool isTestComplete)
        {
            const float secondsToWait = 1f;

            TestContext.WriteLine ("In parent routine. Starting.");
            yield return _mockCoroutineService.StartCoroutine (ReturnAfterNestedRoutinesDepthOne ());
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
            yield return _mockCoroutineService.StartCoroutine (ReturnAfterNestedRoutinesDepthTwo ());
            if (_nestedRoutinesTracker.Count == 2)
            {
                _nestedRoutinesTracker.Pop ();
            } else
            {
                throw new Exception ("Nested routine depth one is run in wrong order.");
            }

            TestContext.WriteLine ("In nested routine depth one. Finished.");
            yield return null;
        }

        private IEnumerator ReturnAfterNestedRoutinesDepthTwo ()
        {
            const float secondsToWait = 1f;

            TestContext.WriteLine ($"In nested routine depth two. Starting. Will automatically finish in {secondsToWait} seconds.");

            if (_nestedRoutinesTracker.Count == 3)
            {
                _nestedRoutinesTracker.Pop ();
            } else
            {
                throw new Exception ("Nested routine depth two is run in wrong order.");
            }

            TestContext.WriteLine ("In nested routine depth two. Finished.");
            yield return null;
        }

        #endregion WAIT FOR NESTED COROUTINES


        //
        //
        // #region WAIT FOR COROUTINES RUN IN PARALLEL
        //
        // private Queue<char> _parallelRoutinesTracker;
        //
        // [TestMethod]
        // public void StartCoroutine_IndependentCoroutinesInParallel_ReturnInCorrectOrder()
        // {
        //     RefBool isTestComplete = new RefBool();
        //     _parallelRoutinesTracker = new Queue<char>();
        //
        //     EditorCoroutineService.StartCoroutine(ReturnAfterRoutinesRunInParallel(isTestComplete));
        //     while (!isTestComplete.Value)
        //     {
        //         yield return null;
        //     }
        //
        //     Assert.AreEqual(0, _parallelRoutinesTracker.Count, "EditorCoroutines started in parallel were not run in the correct order.");
        // }
        //
        // private IEnumerator ReturnAfterRoutinesRunInParallel(RefBool isTestComplete)
        // {
        //     const float secondsToWaitInEachRoutine = 2f;
        //     const float secondRoutineStartDelay = 0.5f;
        //
        //     TestContext.WriteLine("In ReturnAfterRoutinesRunInParallel routine. Starting.");
        //     _parallelRoutinesTracker.Enqueue('a');
        //     EditorCoroutineService.StartCoroutine(ReturnAfterSecondsA(secondsToWaitInEachRoutine));
        //     TestContext.WriteLine($"In ReturnAfterRoutinesRunInParallel routine. Waiting for {secondRoutineStartDelay} seconds.");
        //     yield return new WaitForSeconds(secondRoutineStartDelay);
        //     _parallelRoutinesTracker.Enqueue('b');
        //     EditorCoroutineService.StartCoroutine(ReturnAfterSecondsB(secondsToWaitInEachRoutine, isTestComplete));
        //     TestContext.WriteLine("In ReturnAfterRoutinesRunInParallel routine. Finished, though an EditorCoroutine started in parallel may still be running.");
        // }
        //
        // private IEnumerator ReturnAfterSecondsA(float secondsToWait)
        // {
        //     TestContext.WriteLine($"In ReturnAfterSecondsA routine. Starting. Will automatically finish in {secondsToWait} seconds.");
        //     yield return new WaitForSeconds(secondsToWait);
        //     if (_parallelRoutinesTracker.Peek() == 'a')
        //     {
        //         _parallelRoutinesTracker.Dequeue();
        //     }
        //     else
        //     {
        //         throw new Exception("Parallel EditorCoroutine A is run in wrong order.");
        //     }
        //
        //     TestContext.WriteLine("In ReturnAfterSecondsA routine. Finished.");
        // }
        //
        // private IEnumerator ReturnAfterSecondsB(float secondsToWait, RefBool isTestComplete)
        // {
        //     TestContext.WriteLine($"In ReturnAfterSecondsB routine. Starting. Will automatically finish in {secondsToWait} seconds.");
        //     yield return new WaitForSeconds(secondsToWait);
        //     if (_parallelRoutinesTracker.Peek() == 'b')
        //     {
        //         _parallelRoutinesTracker.Dequeue();
        //     }
        //     else
        //     {
        //         throw new Exception("Parallel EditorCoroutine B is run in wrong order.");
        //     }
        //
        //     isTestComplete.Value = true;
        //     TestContext.WriteLine("In ReturnAfterSecondsB routine. Finished.");
        // }
        //
        // #endregion WAIT FOR COROUTINES RUN IN PARALLEL
        //
        //
        // #region WAIT FOR CUSTOMYIELDINSTRUCTION
        //
        // private int _customIndex;
        //
        // [TestMethod]
        // public void StartCoroutine_CustomYieldInstruction_ReturnsCorrectly()
        // {
        //     RefBool isTestComplete = new RefBool();
        //     _customIndex = 0;
        //
        //     EditorCoroutineService.StartCoroutine(ReturnAfterCustomYieldInstructionSucceeds(isTestComplete));
        //     while (!isTestComplete.Value)
        //     {
        //         yield return null;
        //     }
        //
        //     Assert.AreEqual(1, _customIndex, "EditorCoroutine yielding on a CustomYieldInstruction was not returned correctly.");
        // }
        //
        // private IEnumerator ReturnAfterCustomYieldInstructionSucceeds(RefBool isTestComplete)
        // {
        //     TestContext.WriteLine("In ReturnAfterCustomYieldInstructionSucceeds routine. Starting.");
        //     EditorCoroutineService.StartCoroutine(SetIndexAfterSeconds(2f));
        //     yield return new CustomWaitWhile(IsIndexStillZero);
        //
        //     TestContext.WriteLine("In ReturnAfterCustomYieldInstructionSucceeds routine. Finished.");
        //     isTestComplete.Value = true;
        //
        //     bool IsIndexStillZero()
        //     {
        //         return _customIndex == 0;
        //     }
        // }
        //
        // private IEnumerator SetIndexAfterSeconds(float secondsToWait)
        // {
        //     TestContext.WriteLine($"In SetIndexAfterSeconds routine. Starting. Will automatically finish in {secondsToWait} seconds.");
        //     yield return new WaitForSeconds(secondsToWait);
        //     _customIndex = 1;
        //     TestContext.WriteLine("In SetIndexAfterSeconds routine. Finished.");
        // }
        //
        //
        // private class CustomWaitWhile : CustomYieldInstruction
        // {
        //     private readonly Func<bool> _predicate;
        //     public override bool keepWaiting => _predicate();
        //
        //     public CustomWaitWhile(Func<bool> predicate)
        //     {
        //         _predicate = predicate;
        //     }
        // }
        //
        // #endregion WAIT FOR CUSTOMYIELDINSTRUCTION
        //
        //
        // #region STOPCOROUTINE
        //
        // [TestMethod]
        // public void StopCoroutine_IEnumeratorArg_StopsCorrectly()
        // {
        //     RefBool isTestComplete = new RefBool();
        //     RefBool wasCancelled = new RefBool();
        //
        //     // Test coroutine will be stopped. Check in 1 second for expected results. Test coroutine should finish by then, successfully or not.
        //     EditorCoroutineService.StartCoroutine(ReturnAfterSecondsToBeStopped(1f, isTestComplete));
        //
        //     TestContext.WriteLine("Starting SimpleIteratorToBeStopped routine.\nWill automatically attempt to stop it after a frame.");
        //     // This coroutine will be attempted to be cancelled after 1 frame, in the line after the following `yield return null`.
        //     EditorCoroutineService.StartCoroutine(SimpleIteratorToBeStopped(wasCancelled));
        //     yield return null;
        //     // StopCoroutine IEnumerator args don't need to be the exact same.
        //     EditorCoroutineService.StopCoroutine(SimpleIteratorToBeStopped(new RefBool()));
        //
        //     while (!isTestComplete.Value)
        //     {
        //         yield return null;
        //     }
        //
        //     Assert.IsTrue(wasCancelled.Value, "EditorCoroutine was not stopped correctly via IEnumerator arg.");
        // }
        //
        // [TestMethod]
        // public void StopCoroutine_EditorCoroutineArg_StopsCorrectly()
        // {
        //     RefBool isTestComplete = new RefBool();
        //     RefBool wasCancelled = new RefBool();
        //
        //     // Test coroutine will be stopped. Check in 1 second for expected results. Test coroutine should finish by then, successfully or not.
        //     EditorCoroutineService.StartCoroutine(ReturnAfterSecondsToBeStopped(1f, isTestComplete));
        //
        //     TestContext.WriteLine("Starting SimpleIteratorToBeStopped routine.\nWill automatically attempt to stop it after a frame.");
        //     // This coroutine will be attempted to be cancelled after 1 frame, in the line after the following `yield return null`.
        //     EditorCoroutine editorCoroutine = EditorCoroutineService.StartCoroutine(SimpleIteratorToBeStopped(wasCancelled));
        //     yield return null;
        //     EditorCoroutineService.StopCoroutine(editorCoroutine);
        //
        //     while (!isTestComplete.Value)
        //     {
        //         yield return null;
        //     }
        //
        //     Assert.IsTrue(wasCancelled.Value, "EditorCoroutine was not stopped correctly via Editor Coroutine reference arg.");
        // }
        //
        // [TestMethod]
        // public void StopCoroutine_MethodName_StopsCorrectly()
        // {
        //     RefBool isTestComplete = new RefBool();
        //     RefBool wasCancelled = new RefBool();
        //
        //     // Test coroutine will be stopped. Check in 1 second for expected results. Test coroutine should finish by then, successfully or not.
        //     EditorCoroutineService.StartCoroutine(ReturnAfterSecondsToBeStopped(1f, isTestComplete));
        //
        //     TestContext.WriteLine("Starting SimpleIteratorToBeStopped routine.\nWill automatically attempt to stop it after a frame.");
        //     // This coroutine will be attempted to be cancelled after 1 frame, in the line after the following `yield return null`.
        //     EditorCoroutineService.StartCoroutine(this, "SimpleIteratorToBeStopped", new object[] { wasCancelled });
        //     yield return null;
        //     EditorCoroutineService.StopCoroutine(this, "SimpleIteratorToBeStopped");
        //
        //     while (!isTestComplete.Value)
        //     {
        //         yield return null;
        //     }
        //
        //     Assert.IsTrue(wasCancelled.Value, "EditorCoroutine was not stopped correctly via Method Name arg.");
        // }
        //
        // private IEnumerator SimpleIteratorToBeStopped(RefBool wasCancelled)
        // {
        //     TestContext.WriteLine("In SimpleIteratorToBeStopped routine. Starting.");
        //     yield return null;
        //     TestContext.WriteLine("In SimpleIteratorToBeStopped routine. This routine may be stopped now. If so, this should be the last log from this method.");
        //     wasCancelled.Value = true;
        //     yield return null;
        //     wasCancelled.Value = false;
        //     TestContext.WriteLine("In SimpleIteratorToBeStopped routine. Finished. This line should not be logged if the coroutine was stopped successfully.");
        // }
        //
        // private IEnumerator ReturnAfterSecondsToBeStopped(float secondsToWait, RefBool isTestComplete)
        // {
        //     TestContext.WriteLine($"In ReturnAfterSecondsToBeStopped routine. Starting. Will automatically finish in {secondsToWait} seconds.");
        //     yield return new WaitForSeconds(secondsToWait);
        //     isTestComplete.Value = true;
        //     TestContext.WriteLine("In ReturnAfterSecondsToBeStopped routine. Finished.");
        // }
        //
        // #endregion STOPCOROUTINE
        //
        //
        // #region STOPALLCOROUTINES
        //
        // [TestMethod]
        // public void StopAllCoroutines_Global_StopsAllCoroutinesCorrectly()
        // {
        //     OwnerOfRoutine ownerA = new OwnerOfRoutine();
        //     RefBool wasCancelledA = new RefBool();
        //     RefBool wasCancelledB = new RefBool();
        //
        //     // Test coroutines will be stopped. Check in 1 second for expected results. Test coroutines should finish by then, successfully or not.
        //     DateTime then = DateTime.Now;
        //     const double secondsToWait = 1;
        //
        //     TestContext.WriteLine("Starting SimpleIteratorToBeStoppedB and ownerA.SimpleIteratorToBeStoppedA routines.\nWill automatically attempt to stop all running routines after a frame.");
        //     // All the following coroutines will be attempted to be cancelled after 1 frame, in the line after the following `yield return null`.
        //     EditorCoroutineService.StartCoroutine(ownerA.SimpleIteratorToBeStoppedA(wasCancelledA));
        //     EditorCoroutineService.StartCoroutine(SimpleIteratorToBeStoppedB(wasCancelledB));
        //     yield return null;
        //     EditorCoroutineService.StopAllCoroutines();
        //
        //     double deltaSeconds = (DateTime.Now - then).TotalSeconds;
        //     while (deltaSeconds < secondsToWait)
        //     {
        //         yield return null;
        //         deltaSeconds = (DateTime.Now - then).TotalSeconds;
        //     }
        //
        //     Assert.IsTrue(wasCancelledA.Value && wasCancelledB.Value, "Global stop for all EditorCoroutines was not executed correctly.");
        // }
        //
        // [TestMethod]
        // public void StopAllCoroutines_SpecificOwnerArg_StopsAllCoroutinesCorrectly()
        // {
        //     OwnerOfRoutine ownerA = new OwnerOfRoutine();
        //     RefBool wasCancelledA = new RefBool();
        //     RefBool wasCancelledB = new RefBool();
        //
        //     // Test coroutines will be stopped. Check in 1 second for expected results. Test coroutines should finish by then, successfully or not.
        //     DateTime then = DateTime.Now;
        //     const double secondsToWait = 1;
        //
        //     TestContext.WriteLine("Starting SimpleIteratorToBeStoppedB and ownerA.SimpleIteratorToBeStoppedA routines.\nWill automatically attempt to stop routines belonging to ownerA after a frame.");
        //     // All the following coroutines will be attempted to be cancelled after 1 frame, in the line after the following `yield return null`.
        //     EditorCoroutineService.StartCoroutine(ownerA, "SimpleIteratorToBeStoppedA", new object[] { wasCancelledA });
        //     EditorCoroutineService.StartCoroutine(SimpleIteratorToBeStoppedB(wasCancelledB));
        //     yield return null;
        //     EditorCoroutineService.StopAllCoroutines(ownerA);
        //
        //     double deltaSeconds = (DateTime.Now - then).TotalSeconds;
        //     while (deltaSeconds < secondsToWait)
        //     {
        //         yield return null;
        //         deltaSeconds = (DateTime.Now - then).TotalSeconds;
        //     }
        //
        //     Assert.IsTrue(wasCancelledA.Value && !wasCancelledB.Value, "EditorCoroutines belonging to a specific owner were not stopped correctly.");
        // }
        //
        //
        // private class OwnerOfRoutine
        // {
        //     public IEnumerator SimpleIteratorToBeStoppedA(RefBool wasCancelled)
        //     {
        //         TestContext.WriteLine("In SimpleIteratorToBeStoppedA routine. Starting.");
        //         yield return null;
        //         TestContext.WriteLine("In SimpleIteratorToBeStoppedA routine. This routine may be stopped now. If so, this should be the last log from this method.");
        //         wasCancelled.Value = true;
        //         yield return null;
        //         wasCancelled.Value = false;
        //         TestContext.WriteLine("In SimpleIteratorToBeStoppedA routine. Finished. This line should not be logged if the coroutine was stopped successfully.");
        //     }
        // }
        //
        //
        // private IEnumerator SimpleIteratorToBeStoppedB(RefBool wasCancelled)
        // {
        //     TestContext.WriteLine("In SimpleIteratorToBeStoppedB routine. Starting.");
        //     yield return null;
        //     TestContext.WriteLine("In SimpleIteratorToBeStoppedB routine. This routine may be stopped now. If so, this should be the last log from this method.");
        //     wasCancelled.Value = true;
        //     yield return null;
        //     wasCancelled.Value = false;
        //     TestContext.WriteLine("In SimpleIteratorToBeStoppedB routine. Finished. This line should not be logged if the coroutine was stopped successfully.");
        // }
        //
        // #endregion STOPALLCOROUTINES
        //


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