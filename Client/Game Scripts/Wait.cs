//this is a simple script that waits for a certain amount of time and allows the win and lose screen to show for a certain amount of seconds before switching back to the menu.
using UnityEngine;
using System.Collections;

public class Wait : MonoBehaviour
{
    public void waitSeconds(int seconds)//wait seconds
    {
        StartCoroutine(WaitFor(seconds));
    }

    IEnumerator WaitFor(int seconds)
    {
        yield return new WaitForSeconds(seconds);
    }
}
