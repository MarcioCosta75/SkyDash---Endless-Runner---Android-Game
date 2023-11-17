using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShieldTimerController : MonoBehaviour
{
    public Image timer_linear_image;
    public GameObject objectToActivate;

    public float fillDuration = 5.0f; // Duração do preenchimento em segundos

    private float initialFillAmount;
    private float fillSpeed;

    private bool shieldCollision = false;

    private void Start()
    {
        timer_linear_image.fillAmount = 0f;
        objectToActivate.SetActive(false);
        initialFillAmount = timer_linear_image.fillAmount;
        fillSpeed = 1f / fillDuration;
    }

    private void Update()
    {
        if (shieldCollision && !objectToActivate.activeSelf)
        {
            StartCoroutine(StartCountdown());
        }
    }

    private IEnumerator StartCountdown()
    {
        objectToActivate.SetActive(true);

        float elapsedTime = 0f;

        while (elapsedTime < fillDuration)
        {
            elapsedTime += Time.deltaTime;

            float remainingTime = fillDuration - elapsedTime;
            float targetFillAmount = remainingTime / fillDuration;

            timer_linear_image.fillAmount = Mathf.Lerp(targetFillAmount, 0f, elapsedTime / fillDuration);

            yield return null;
        }

        objectToActivate.SetActive(false);
        timer_linear_image.fillAmount = initialFillAmount;
        shieldCollision = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Shield")
        {
            shieldCollision = true;
        }
    }
}
