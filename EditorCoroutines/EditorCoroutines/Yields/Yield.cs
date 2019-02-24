namespace Andeart.EditorCoroutines.Yields
{

    internal static class Yield
    {
        public class Default : IYield
        {
            public bool IsReadyToEvaluate (double deltaTime, int frameCount)
            {
                return true;
            }
        }
    }

}