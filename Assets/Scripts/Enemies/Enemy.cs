using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public List<Transform> patrolPoints;
    public float moveSpeed = 3f;
    public float reachThreshold = 0.2f;

    public float viewAngle = 90f;
    public float viewDistance = 10f;
    public LayerMask playerLayer;
    public LayerMask obstructionMask;
    public Transform viewOrigin;
    public string sightAnimationName = "Looking";
    public Animator animator;

    public bool isFrozen = false;
    private int currentPointIndex = 0;
    private bool waiting = false;

    void Update()
    {
        if (patrolPoints.Count == 0 || waiting)
            return;

        Patrol();
        DetectPlayer();
    }

    void Patrol()
    {
        Transform targetPoint = patrolPoints[currentPointIndex];
        Vector3 direction = (targetPoint.position - transform.position).normalized;
        Vector3 move = direction * moveSpeed * Time.deltaTime;

        if (isFrozen)
        {
            return;
        }

        transform.position += move;

        if (move != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }

        if (Vector3.Distance(transform.position, targetPoint.position) <= reachThreshold)
        {
            StartCoroutine(WaitBeforeNextPoint());
        }
    }

    IEnumerator WaitBeforeNextPoint()
    {
        waiting = true;

        float waitDuration = 1f;
        string animationToPlay = sightAnimationName;
        bool playSight = false;

        var patrolPoint = patrolPoints[currentPointIndex].GetComponent<PatrolPoint>();
        if (patrolPoint != null)
        {
            waitDuration = patrolPoint.waitTime;
            playSight = patrolPoint.doSight;
        }

        yield return new WaitForSeconds(waitDuration);

        if (playSight && animator != null && !string.IsNullOrEmpty(animationToPlay))
        {
            animator.Play(animationToPlay);

            yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f &&
                                             animator.GetCurrentAnimatorStateInfo(0).IsName(animationToPlay));
        }

        currentPointIndex = (currentPointIndex + 1) % patrolPoints.Count;
        waiting = false;
    }

    void DetectPlayer()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, viewDistance, playerLayer);

        foreach (var hit in hits)
        {
            Transform player = hit.transform;

            Vector3 dirToPlayer = (player.position - (viewOrigin != null ? viewOrigin.position : transform.position)).normalized;

            if (Vector3.Angle(transform.forward, dirToPlayer) < viewAngle / 2)
            {
                float distToPlayer = Vector3.Distance(transform.position, player.position);

                if (!Physics.Raycast(transform.position, dirToPlayer, distToPlayer, obstructionMask))
                {
                    Debug.Log("Player spotted by sight rays!");
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (viewOrigin == null) viewOrigin = transform;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(viewOrigin.position, viewDistance);

        Vector3 rightBoundary = Quaternion.Euler(0, viewAngle / 2, 0) * viewOrigin.forward;
        Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle / 2, 0) * viewOrigin.forward;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(viewOrigin.position, viewOrigin.position + rightBoundary * viewDistance);
        Gizmos.DrawLine(viewOrigin.position, viewOrigin.position + leftBoundary * viewDistance);
    }
}
