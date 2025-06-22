using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyJumpscareTrigger : MonoBehaviour
{
    public Animator enemyAnimator;
    public string jumpscareAnimationName = "Jumpscare";
    public PlayerMovement playerMovement;
    public GameObject jumpscareUI;
    public Transform playerCamera;
    public float lookSpeed = 5f;

    private bool hasTriggered = false;
    public Transform enemyTransform;
    private Enemy enemyScript;

    void Start()
    {
        enemyScript = enemyTransform.GetComponent<Enemy>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;

        if (other.CompareTag("Player"))
        {
            hasTriggered = true;

            if (playerMovement != null)
                playerMovement.canMove = false;

            if (enemyScript != null)
                enemyScript.isFrozen = true;

            if (enemyAnimator != null && !string.IsNullOrEmpty(jumpscareAnimationName))
                enemyAnimator.Play(jumpscareAnimationName);

            if (jumpscareUI != null)
                jumpscareUI.SetActive(true);

            StartCoroutine(ForceLookAtEnemy());
        }
    }

    System.Collections.IEnumerator ForceLookAtEnemy()
    {
        float duration = 1.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            if (playerCamera != null && enemyTransform != null)
            {
                Vector3 direction = (enemyTransform.position - playerCamera.position).normalized;
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                playerCamera.rotation = Quaternion.Slerp(playerCamera.rotation, targetRotation, Time.deltaTime * lookSpeed);
            }

            yield return null;
        }
    }
}