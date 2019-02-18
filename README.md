# UnityLabs.EditorCoroutines

`EditorCoroutines` allows you to start and stop Unity coroutines in Editor scripts.

This library is a modernisation of @marijnz's [unity-editor-coroutines](https://github.com/marijnz/unity-editor-coroutines), built from the ground up keeping newer C# features, API architecture, and Unity performance in mind.

An example:
```csharp
// In Editor script
private void RunLoop ()
{
    EditorCoroutineService.StartCoroutine (LoopEveryTwoSeconds ());
}

private IEnumerator LoopEveryTwoSeconds ()
{
    while (true)
    {
        Debug.Log ("EditorCoroutine Demo. Logging again in 2 seconds...");
        yield return new WaitForSeconds (2f);
    }
}

```
results in this...

![Andeart.EditorCoroutines.gif](https://user-images.githubusercontent.com/6226493/52686751-f8609f80-2f03-11e9-8144-207171ecc2ed.gif)

Note that the Editor does not need to be playing (as can be seen in the image above).
 
## API
 
`EditorCoroutines` currently supports starting and stopping coroutines via the following methods:
```csharp
EditorCoroutine StartCoroutine (IEnumerator routine);

EditorCoroutine StartCoroutine (object owner, string methodName, object[] methodArgs = null);

void StopCoroutine (IEnumerator routine);

void StopCoroutine (EditorCoroutine routine);

void StopCoroutine (object owner, string methodName);

void StopAllCoroutines ();

void StopAllCoroutines (object owner);
```

#### Supported YieldInstruction types
- `WaitForSeconds`
- `WaitForFixedUpdate`
- `WaitForEndOfFrame`
- `null` (behaves similarly to `WaitForFixedUpdate`)
- `AsyncOperation`
- ~~`WWW`~~: `WWW` is now obsolete from Unity. Use `UnityWebRequest` instead, which is supported as an `AsyncOperation`.
- `CustomYieldInstruction`
- Nested `EditorCoroutines`

## Tests

Because `EditorCoroutines` is purely a Unity concept (and implementation), the tests for this project are written in Unity's TestRunner.
You can find the currently implemented tests in the [Tests.cs](https://github.com/andeart/UnityLabs.EditorCoroutines/blob/master/UnityLabs.EditorCoroutines.Tests/Assets/Editor/EditorCoroutineTests.cs) file.

## Installation and Usage
* Drop the `UnityLabs.EditorCoroutines.dll` file (from the [Releases tab](https://github.com/andeart/UnityLabs.EditorCoroutines/releases)) anywhere in your Unity project. Any sub-directory under `Assets` will work- **it does not need to be under an `Editor` folder**.
* Optional: Also drop the `UnityLabs.EditorCoroutines.pdb` and `UnityLabs.EditorCoroutines.xml` files in the same location if you're interested in debugging.
* You can now use `EditorCoroutines` in your Editor scripts.
* Refer to the [Demo C#](https://github.com/andeart/UnityLabs.EditorCoroutines/blob/master/Demo/Assets/Editor/EditorCoroutineDemoWindow.cs) file for more examples

## Feedback
Please feel free to send in a Pull Request, or drop me an email. Cheers!