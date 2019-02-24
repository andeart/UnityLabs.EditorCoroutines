namespace Andeart.EditorCoroutines.Yields
{

    internal interface IYield
    {
        bool IsReadyToEvaluate (double deltaTime, int frameCount);
    }

}