using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public enum State { Patrolling, Chasing }
    public State currentState = State.Patrolling;

    public List<Transform> patrolPoints;
    public float patrolSpeed = 3f;
    public float chaseSpeed = 5f;
    public float reachThreshold = 0.2f;

    public float viewAngle = 90f;
    public float viewDistance = 10f;
    public LayerMask playerLayer;
    public LayerMask obstructionMask;
    public Transform viewOrigin;

    public string sightAnimationName = "Looking";
    public Animator animator;

    public Transform player;
    public float chaseForgetTime = 10f;

    public AudioSource audioSource;
    public AudioClip alertSound;

    public bool isFrozen = false;
    private int currentPointIndex = 0;
    private bool waiting = false;
    private bool hasPlayedAlert = false;
    private float timeSinceLastSeen = 0f;
    private UnityEngine.AI.NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        agent.speed = patrolSpeed;
    }

    void Update()
    {
        if (isFrozen)
        {
            agent.isStopped = true;
            return;
        }

        if (patrolPoints.Count == 0 || waiting)
            return;

        switch (currentState)
        {
            case State.Patrolling:
                if (!waiting)
                {
                    agent.speed = patrolSpeed;
                    Patrol();
                    DetectPlayer();
                }
                break;

            case State.Chasing:
                agent.speed = chaseSpeed;
                ChasePlayer();
                break;

        }
    }

    void Patrol()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            StartCoroutine(WaitBeforeNextPoint());
        }
        else
        {
            if (!agent.hasPath)
                agent.SetDestination(patrolPoints[currentPointIndex].position);
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
        agent.SetDestination(patrolPoints[currentPointIndex].position);
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
                    currentState = State.Chasing;
                    timeSinceLastSeen = 0f;

                    if (!hasPlayedAlert && audioSource && alertSound)
                    {
                        audioSource.PlayOneShot(alertSound);
                        hasPlayedAlert = true;
                        Debug.Log("Player spotted by sight rays!");
                    }
                }
            }
        }
    }

    void ChasePlayer()
    {
        if (player == null) return;

        agent.SetDestination(player.position);

        Vector3 dirToPlayer = (player.position - viewOrigin.position).normalized;
        float distToPlayer = Vector3.Distance(viewOrigin.position, player.position);

        if (Vector3.Angle(viewOrigin.forward, dirToPlayer) < viewAngle / 2 &&
            distToPlayer < viewDistance &&
            !Physics.Raycast(viewOrigin.position, dirToPlayer, distToPlayer, obstructionMask))
        {
            timeSinceLastSeen = 0f;
        }
        else
        {
            timeSinceLastSeen += Time.deltaTime;

            if (timeSinceLastSeen > chaseForgetTime)
            {
                currentState = State.Patrolling;
                agent.SetDestination(patrolPoints[currentPointIndex].position);
                hasPlayedAlert = false;
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
