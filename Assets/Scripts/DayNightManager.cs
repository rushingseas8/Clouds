using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class DayNightManager : MonoBehaviour
{
    [Tooltip("Time of day, float from 0 to 1")]
    [Range(0.0f, 1.0f)]
    public float TimeOfDay;

    public GameObject Sun;
    public GameObject CloudBacklight;
    private Light CloudBacklightLight;

    // Start is called before the first frame update
    void Start()
    {
        if (CloudBacklight != null)
        {
            CloudBacklightLight = CloudBacklight.GetComponent<Light>();
            Debug.Log("Got backlight:" + CloudBacklightLight);
        }
    }

    public bool IsDay() 
    {
        return TimeOfDay > 0f && TimeOfDay < 0.5f;
    }

    public bool IsNight()
    {
        return TimeOfDay > 0.5f && TimeOfDay < 1.0f;
    }

    private float Ease(float x)
    {
        if (x < 0)
        {
            return 0;
        }
        else if (x < 0.25f)
        {
            return 32f * x * x * x;
        }
        else if (x < 0.5f) 
        {
            return 1.0f - (Mathf.Pow(-4.0f * x + 2.0f, 3.0f) / 2.0f);
        }
        else if (x < 0.75f)
        {
            return 1.0f - (Mathf.Pow(-4.0f * (1.0f - x) + 2.0f, 3.0f) / 2.0f);
        }
        else if (x < 1.0f)
        {
            return 32f * Mathf.Pow(1.0f - x, 3.0f);
        }
        else
        {
            return 1;
        }
    }

    // Update is called once per frame
    [ExecuteAlways]
    void Update()
    {
        if (Sun != null)
        {
            Sun.transform.rotation = Quaternion.AngleAxis(360f * TimeOfDay, Vector3.right);
            //Sun.GetComponent<Light>().intensity = 2 * TimeOfDay;
        }

        if (CloudBacklight != null)
        {
            //CloudBacklightLight.intensity = 10 * TimeOfDay;
            if (IsDay())
            {
                Debug.Log("Day!");
                //CloudBacklightLight.intensity = 1.0f;
                //CloudBacklight.transform.rotation = Quaternion.AngleAxis(360f * (TimeOfDay + 0.5f), Vector3.right);
            }
            else
            {
                Debug.Log("Night!");
                // TODO decrease the intensity of the light to simulate starglow/moonglow in a more consistent manner. Maybe add a moon cycle.
                //CloudBacklightLight.intensity = 0.3f;
                //CloudBacklight.transform.rotation = Quaternion.AngleAxis(360f * TimeOfDay, Vector3.right);
            }

            // The power can only be odd, so choose from 1, 3, 5.
            // It affects how quickly the sunlight starts lighting up the cloud box; higher = faster.
            // At 5, the range for max cloud light is [0.1, 0.4].
            // At 3, max cloud light is [0.166, 0.33]
            // At 1, max cloud light is pretty much just 0.25
            float contrib = Mathf.Pow(Mathf.Cos(2.0f * Mathf.PI * TimeOfDay), 5f) + 1.0f; 
            //CloudBacklight.transform.rotation = Quaternion.AngleAxis(-90f * contrib, Vector3.right);
        }

        if (Application.isPlaying)
        {
            TimeOfDay += Time.deltaTime / 60.0f;
            TimeOfDay = TimeOfDay % 1.0f;
        }
    }
}
