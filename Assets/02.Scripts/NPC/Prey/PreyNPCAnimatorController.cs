using UnityEngine;

public class PreyNPCAnimatorController
{
    private Animator animator;

    [SerializeField] private string spawnName = "Spawn";
    [SerializeField] private string attractedName = "Attracted";
    [SerializeField] private string walkName = "Walk";
    [SerializeField] private string sprintName = "Run";
    [SerializeField] private string attackName = "Attack";
    [SerializeField] private string hitName = "Hit";
    [SerializeField] private string deathName = "Death";

    private static bool hashLoad = false;
    private static int hashSpawnTrigger;
    private static int hashAttractedTrigger;
    private static int hashWalkBool;
    private static int hashSprintBool;
    private static int hashAttackTrigger;
    private static int hashHitTrigger;
    private static int hashDeathTrigger;

    public void Initialize(Animator _animator)
    {
        animator = _animator;

        if (hashLoad == false)
        {
            hashLoad = true;

            hashSpawnTrigger = Animator.StringToHash(spawnName);
            hashAttractedTrigger = Animator.StringToHash(attractedName);
            hashWalkBool = Animator.StringToHash(walkName);
            hashSprintBool = Animator.StringToHash(sprintName);
            hashAttackTrigger = Animator.StringToHash(attackName);
            hashHitTrigger = Animator.StringToHash(hitName);
            hashDeathTrigger = Animator.StringToHash(deathName);
        }
    }

    public void SetWalk(bool walk)
    {
        animator.SetBool(hashWalkBool, walk);
    }

    public void SetSprint(bool sprint)
    {
        animator.SetBool(hashSprintBool, sprint);
    }

    public void TriggerAttack()
    {
        animator.SetTrigger(hashAttackTrigger);
    }

    public void TriggerHit()
    {
        animator.SetTrigger(hashHitTrigger);
    }

    public void TriggerDeath()
    {
        animator.SetTrigger(hashDeathTrigger);
    }
}