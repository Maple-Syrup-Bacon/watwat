using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private float timerValue = 60f;
    [SerializeField]
    private float timerTickStep = 0.1f;

    private TMP_Text timer;

    // Use this for initialization
    void Start()
    {
        timer = GameObject.Find("Canvas/Timer").GetComponent<TMP_Text>();

        timer.text = timerValue.ToString();

        StartCoroutine(tickTimer());
    }

    // Update is called once per frame
    void Update()
    {

    }

    private IEnumerator tickTimer()
    {
        while (0f < timerValue)
        {
            yield return new WaitForSeconds(timerTickStep);

            timerValue -= timerTickStep;

            timer.text = timerValue.ToString("N1");
        }

        timerValue = 0f;
    }
}
