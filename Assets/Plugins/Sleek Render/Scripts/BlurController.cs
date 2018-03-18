using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BlurController : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
        timer = 0;
        upBloor = true;
        settings.numberOfPasses = 1;
    }


    public SleekRender.SleekRenderSettings settings;
    public float timeInterval;
    public Text fpsText;

    private float deltaTime;
    private float timer;
    private bool upBloor;

    // Update is called once per frame
    void Update()
    {
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;
        fpsText.text = Mathf.Ceil(fps).ToString();

        timer += Time.deltaTime;
        if (timer > timeInterval)
        {
            if (settings.numberOfPasses == 10 || settings.numberOfPasses == 1)
                upBloor = !upBloor;

            if (upBloor && settings.numberOfPasses < 10)
                settings.numberOfPasses += 1;
            if (!upBloor && settings.numberOfPasses > 1)
                settings.numberOfPasses -= 1;

            timer = 0;
        }
    }
}
