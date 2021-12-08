using UnityEngine;
using System.Collections;

/*
public class DiveOffBoat : MonoBehaviour {

    public RuntimeAnimatorController movemenetController;

    Rigidbody m_Rigidbody;
    Animator m_Animator;
    CapsuleCollider m_Capsule;

    private bool hasDived = false;
    private bool finishedDiving = false;

    void Start()
    {
        m_Animator = GetComponent<Animator>();
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Capsule = GetComponent<CapsuleCollider>();
    }

    void Update()
    {
        if(Input.anyKeyDown && !hasDived && !Input.GetKeyDown(KeyCode.V))
        {
            hasDived = true;
            m_Animator.SetBool("ShouldDive?", true);
            m_Animator.applyRootMotion = false;
        }

        if (m_Animator.GetCurrentAnimatorStateInfo(0).IsName("FinishedDiving") && hasDived && !finishedDiving)
        {
            finishedDiving = true;
            FinishDiving();
        }
    }

    public void OnAnimatorMove()
    {
        // we implement this function to override the default root motion.
        // this allows us to modify the positional speed before it's applied.
        if (Time.deltaTime > 0)
        {
            Vector3 v = (m_Animator.deltaPosition) / Time.deltaTime;

            // we preserve the existing y part of the current velocity.
            //v.y = m_Rigidbody.velocity.y;
            m_Rigidbody.velocity = v;
        }
    }

    public void FinishDiving()
    {
        m_Rigidbody.useGravity = true;
        m_Capsule.enabled = true;
        m_Animator.runtimeAnimatorController = movemenetController;
        GetComponent<UnityStandardAssets.Characters.ThirdPerson.ThirdPersonSwimmer>().enabled = true;
        GetComponent<UnityStandardAssets.Characters.ThirdPerson.ThirdPersonSwimmerController>().enabled = true;
        enabled = false;
    }

}
*/
