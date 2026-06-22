using UnityEngine;

namespace GridRun.Runtime
{
    public static class GridRunBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateRuntime()
        {
            if (GameObject.Find("GridRun Runtime") != null)
            {
                return;
            }

            GameObject runtime = new GameObject("GridRun Runtime");
            runtime.AddComponent<GridRunGame>();
        }
    }
}
