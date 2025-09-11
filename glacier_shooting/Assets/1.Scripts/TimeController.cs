using UnityEngine;

public class TimeController : MonoBehaviour
{


    public void TimeIsStop()
    {
        Time.timeScale = 0;
    }

    public void TimeIsLoad()
    {
        Time.timeScale = 1;
    }
}
