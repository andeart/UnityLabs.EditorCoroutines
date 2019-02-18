using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.TestTools;


namespace Andeart.UnityLabs.EditorCoroutines.Unity.Tests
{

    public class EditorCoroutineTestRunner
    {
        private bool _isTestCoroutineComplete;


        #region WAIT FOR SECONDS

        [UnityTest]
        public IEnumerator StartCoroutine_YieldWaitForSeconds_SuccessAfterSeconds ()
        {
            _isTestCoroutineComplete = false;
            DateTime then = DateTime.Now;
            const float secondsToWait = 2;

            EditorCoroutineService.StartCoroutine (ReturnAfterSeconds (secondsToWait));
            while (!_isTestCoroutineComplete)
            {
                yield return null;
            }

            double deltaSeconds = (DateTime.Now - then).TotalSeconds;

            Assert.AreEqual (secondsToWait,
                             deltaSeconds,
                             0.015,
                             $"WaitForSeconds EditorCoroutine returned in {deltaSeconds}, when expected to return in {secondsToWait} seconds.");
        }

        private IEnumerator ReturnAfterSeconds (float seconds)
        {
            yield return new WaitForSeconds (seconds);
            TestContext.WriteLine ("Inside coroutine. Finished 2 seconds.");
            _isTestCoroutineComplete = true;
        }

        #endregion WAIT FOR SECONDS


        #region WAIT FOR ASYNCOPERATION

        private byte[] _webRequestData;

        [UnityTest]
        public IEnumerator StartCoroutine_YieldWaitForAsyncOperation_ReturnsCorrectly ()
        {
            _isTestCoroutineComplete = false;
            _webRequestData = null;

            EditorCoroutineService.StartCoroutine (ReturnAfterWebRequest ("http://httpbin.org/get"));
            while (!_isTestCoroutineComplete)
            {
                yield return null;
            }

            Assert.IsNotNull (_webRequestData, "WebRequest EditorCoroutine returned null data.");
        }

        private IEnumerator ReturnAfterWebRequest (string uri)
        {
            UnityWebRequest www = UnityWebRequest.Get (uri);
            yield return www.SendWebRequest ();

            if (www.isNetworkError || www.isHttpError)
            {
                TestContext.WriteLine (www.error);
                throw new Exception (www.error);
            }

            TestContext.WriteLine (www.downloadHandler.text);
            _webRequestData = www.downloadHandler.data;
            _isTestCoroutineComplete = true;
        }

        #endregion WAIT FOR ASYNCOPERATION


        #region WAIT FOR NESTED COROUTINES

        private Stack<int> _nestedCoroutinesTracker;

        [UnityTest]
        public IEnumerator StartCoroutine_YieldWaitForNestedCoroutines_ReturnsInCorrectOrder ()
        {
            _isTestCoroutineComplete = false;
            _nestedCoroutinesTracker = new Stack<int> ();
            for (int i = 0; i < 3; i++)
            {
                _nestedCoroutinesTracker.Push (i);
            }

            EditorCoroutineService.StartCoroutine (ReturnAfterNestedCoroutines ());
            while (!_isTestCoroutineComplete)
            {
                yield return null;
            }

            Assert.AreEqual (0, _nestedCoroutinesTracker.Count, "Nested EditorCoroutines were not run in the right order.");
        }

        private IEnumerator ReturnAfterNestedCoroutines ()
        {
            TestContext.WriteLine ("Starting parent coroutine...");
            yield return new WaitForSeconds (1f);
            yield return EditorCoroutineService.StartCoroutine (ReturnAfterNestedCoroutinesDepthOne ());
            TestContext.WriteLine ("Finished parent coroutine and nested coroutines.");
            if (_nestedCoroutinesTracker.Count == 1)
            {
                _nestedCoroutinesTracker.Pop ();
            } else
            {
                throw new Exception ("Parent EditorCoroutine is run in wrong order.");
            }

            _isTestCoroutineComplete = true;
        }

        private IEnumerator ReturnAfterNestedCoroutinesDepthOne ()
        {
            TestContext.WriteLine ("Starting nested coroutine depth one...");
            yield return new WaitForSeconds (1f);
            yield return EditorCoroutineService.StartCoroutine (ReturnAfterNestedCoroutinesDepthTwo ());
            TestContext.WriteLine ("Finished nested coroutine depth one.");
            if (_nestedCoroutinesTracker.Count == 2)
            {
                _nestedCoroutinesTracker.Pop ();
            } else
            {
                throw new Exception ("Nested EditorCoroutine of depth one is run in wrong order.");
            }
        }

        private IEnumerator ReturnAfterNestedCoroutinesDepthTwo ()
        {
            TestContext.WriteLine ("Starting nested coroutine depth two. Waiting 1 second to finish...");
            yield return new WaitForSeconds (1f);
            TestContext.WriteLine ("Finished nested coroutine depth two.");
            if (_nestedCoroutinesTracker.Count == 3)
            {
                _nestedCoroutinesTracker.Pop ();
            } else
            {
                throw new Exception ("Nested EditorCoroutine of depth two is run in wrong order.");
            }
        }

        #endregion WAIT FOR NESTED COROUTINES


        #region WAIT FOR COROUTINES RUN IN PARALLEL

        private Queue<char> _parallelCoroutinesTracker;

        [UnityTest]
        public IEnumerator StartCoroutine_IndependentCoroutinesInParallel_ReturnInCorrectOrder ()
        {
            _isTestCoroutineComplete = false;
            _parallelCoroutinesTracker = new Queue<char> ();

            EditorCoroutineService.StartCoroutine (ReturnAfterCoroutinesInParallel ());
            while (!_isTestCoroutineComplete)
            {
                yield return null;
            }

            Assert.AreEqual (0, _parallelCoroutinesTracker.Count, "EditorCoroutines started in parallel were not run in the right order.");
        }

        private IEnumerator ReturnAfterCoroutinesInParallel ()
        {
            _parallelCoroutinesTracker.Enqueue ('a');
            EditorCoroutineService.StartCoroutine (ReturnAfterSecondsA ());
            yield return new WaitForSeconds (0.5f);
            _parallelCoroutinesTracker.Enqueue ('b');
            EditorCoroutineService.StartCoroutine (ReturnAfterSecondsB ());
        }

        private IEnumerator ReturnAfterSecondsA ()
        {
            TestContext.WriteLine ("Starting parallel EditorCoroutine A. Waiting 2 seconds to finish...");
            yield return new WaitForSeconds (2f);
            TestContext.WriteLine ("Finished parallel EditorCoroutine A.");
            if (_parallelCoroutinesTracker.Peek () == 'a')
            {
                _parallelCoroutinesTracker.Dequeue ();
            } else
            {
                throw new Exception ("Parallel EditorCoroutine A is run in wrong order.");
            }
        }

        private IEnumerator ReturnAfterSecondsB ()
        {
            TestContext.WriteLine ("Starting parallel EditorCoroutine B. Waiting 2 seconds to finish...");
            yield return new WaitForSeconds (2f);
            TestContext.WriteLine ("Finished parallel EditorCoroutine B.");
            if (_parallelCoroutinesTracker.Peek () == 'b')
            {
                _parallelCoroutinesTracker.Dequeue ();
            } else
            {
                throw new Exception ("Parallel EditorCoroutine B is run in wrong order.");
            }

            _isTestCoroutineComplete = true;
        }

        #endregion WAIT FOR COROUTINES RUN IN PARALLEL


        #region WAIT FOR CUSTOMYIELDINSTRUCTION

        private int _customIndex;

        [UnityTest]
        public IEnumerator StartCoroutine_CustomYieldInstruction_ReturnsCorrectly ()
        {
            _isTestCoroutineComplete = false;
            _customIndex = 0;

            EditorCoroutineService.StartCoroutine (ReturnAfterCustomYieldInstructionSucceeds ());
            while (!_isTestCoroutineComplete)
            {
                yield return null;
            }

            Assert.AreEqual (1, _customIndex, "CustomYieldInstruction EditorCoroutine was not returned correctly.");
        }

        private IEnumerator ReturnAfterCustomYieldInstructionSucceeds ()
        {
            EditorCoroutineService.StartCoroutine (SetIndexAfterSeconds ());
            yield return new CustomWaitWhile (IsIndexStillZero);

            TestContext.WriteLine ("Custom WaitWhile finished.");
            _isTestCoroutineComplete = true;

            bool IsIndexStillZero ()
            {
                return _customIndex == 0;
            }
        }

        private IEnumerator SetIndexAfterSeconds ()
        {
            yield return new WaitForSeconds (2f);
            _customIndex = 1;
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


    }

}