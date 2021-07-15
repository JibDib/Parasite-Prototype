using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{

    public IEnumerator Shake (float duration, float magnitude)      // Co routine that takes in duration and magnitude values. When called will shake the screen for a set duration at a set strength
    {
        Vector3 originalPos = transform.localPosition;      // The camera's rest position

        float elapsed = 0.0f;       // Stores how much time has passed during the shake

        while (elapsed < duration) 
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(x, y, originalPos.z);

            elapsed += Time.deltaTime;

            yield return null;

            // Moves the camera around in an area determined by magnitude
        }

        transform.localPosition = originalPos; // Moves the camera back to it's original position when done

    }

}
