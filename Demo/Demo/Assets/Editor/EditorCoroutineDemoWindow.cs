using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;


namespace Andeart.UnityLabs.EditorCoroutines.Demos
{

    public class EditorCoroutineDemoWindow : EditorWindow
    {
        [MenuItem ("Window/EditorCoroutine Demos")]
        public static void ShowWindow ()
        {
            GetWindow (typeof(EditorCoroutineDemoWindow));
        }

        private void OnGUI ()
        {
            if (GUILayout.Button ("Wait for two seconds"))
            {
                EditorCoroutineService.StartCoroutine (WaitForTwoSeconds ());
            }

            if (GUILayout.Button ("Wait for 20 frames"))
            {
                EditorCoroutineService.StartCoroutine (WaitForTwentyFrames ());
            }

            if (GUILayout.Button ("Wait for UnityWebRequest"))
            {
                EditorCoroutineService.StartCoroutine (WaitForUnityWebRequest ());
            }

            if (GUILayout.Button ("Wait for nested coroutines"))
            {
                EditorCoroutineService.StartCoroutine (WaitForNestedCoroutines ());
            }

            if (GUILayout.Button ("Stop 'Wait for two seconds' coroutine"))
            {
                EditorCoroutineService.StopCoroutine (WaitForTwoSeconds ());
            }

            if (GUILayout.Button ("Stop all"))
            {
                EditorCoroutineService.StopAllCoroutines ();
            }
        }

        private IEnumerator WaitForTwoSeconds ()
        {
            while (true)
            {
                Debug.Log ("EditorCoroutine Demo. Logging again in 2 seconds...");
                yield return new WaitForSeconds (2f);
            }
        }

        private IEnumerator WaitForTwentyFrames ()
        {
            while (true)
            {
                Debug.Log ("EditorCoroutine Demo. Logging again in 20 frames...");
                for (int i = 0; i < 20; i++)
                {
                    yield return null;
                }
            }
        }

        private IEnumerator WaitForUnityWebRequest ()
        {
            while (true)
            {
                UnityWebRequest www = UnityWebRequest.Get ("https://unity3d.com/");
                yield return www.SendWebRequest ();
                Debug.Log ("EditorCoroutine Demo. " + www.downloadHandler.text);
                yield return new WaitForSeconds (2f);
            }
        }

        private IEnumerator WaitForNestedCoroutines ()
        {
            while (true)
            {
                yield return new WaitForSeconds (2f);
                Debug.Log ("EditorCoroutine Demo. Parent coroutine. Logging again after nested coroutines finish + 2 seconds...");
                yield return EditorCoroutineService.StartCoroutine (WaitForNestedCoroutinesDepthOne ());
            }
        }

        private IEnumerator WaitForNestedCoroutinesDepthOne ()
        {
            yield return new WaitForSeconds (2f);
            Debug.Log ("EditorCoroutine Demo. Nested routine depth 1. Waiting for nested coroutines to finish now.");
            yield return EditorCoroutineService.StartCoroutine (WaitForNestedCoroutinesDepthTwo ());
        }

        private IEnumerator WaitForNestedCoroutinesDepthTwo ()
        {
            yield return new WaitForSeconds (2f);
            Debug.Log ("EditorCoroutine Demo. Nested routine depth 2. Returning control to parent coroutine.");
        }
    }

}