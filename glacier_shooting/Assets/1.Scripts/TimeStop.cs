using UnityEngine;

public class TimeStop : MonoBehaviour
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
