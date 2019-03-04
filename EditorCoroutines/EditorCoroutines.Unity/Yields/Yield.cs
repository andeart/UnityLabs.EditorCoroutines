using Andeart.EditorCoroutines.Coroutines;
using Andeart.EditorCoroutines.Yields;


namespace Andeart.EditorCoroutines.Unity.Yields
{

    internal static class Yield
    {
        public class WaitForFrames : IYield
        {
            private int _framesLeft;

            public WaitForFrames (int framesLeft)
            {
                _framesLeft = framesLeft;
            }

            public bool IsReadyToEvaluate (double deltaTime, int frameCount)
            {
                _framesLeft -= frameCount;
                return _framesLeft <= 0;
            }
        }


        public class WaitForSeconds : IYield
        {
            private double _remainingDuration;

            public WaitForSeconds (float remainingDuration)
            {
                _remainingDuration = remainingDuration;
            }

            public bool IsReadyToEvaluate (double deltaTime, int frameCount)
            {
                _remainingDuration -= deltaTime;
                return _remainingDuration < 0;
            }
        }


        public class AsyncOperation : IYield
        {
            private readonly UnityEngine.AsyncOperation _asyncOperation;

            public AsyncOperation (UnityEngine.AsyncOperation asyncOperation)
            {
                _asyncOperation = asyncOperation;
            }

            public bool IsReadyToEvaluate (double deltaTime, int frameCount)
            {
                return _asyncOperation.isDone;
            }
        }


        public class CustomYieldInstruction : IYield
        {
            private readonly UnityEngine.CustomYieldInstruction _customYieldInstruction;

            public CustomYieldInstruction (UnityEngine.CustomYieldInstruction customYieldInstruction)
            {
                _customYieldInstruction = customYieldInstruction;
            }

            public bool IsReadyToEvaluate (double deltaTime, int frameCount)
            {
                return !_customYieldInstruction.keepWaiting;
            }
        }


        public class NestedCoroutine : IYield
        {
            private readonly ICoroutine _coroutine;

            public NestedCoroutine (ICoroutine coroutine)
            {
                _coroutine = coroutine;
            }

            public bool IsReadyToEvaluate (double deltaTime, int frameCount)
            {
                return _coroutine.Finished;
            }
        }
    }

}