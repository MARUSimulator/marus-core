using UnityEngine;
using Marus.Ocean;

namespace UnityStandardAssets.Characters.ThirdPerson
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(Animator))]
    public class ThirdPersonSwimmer : MonoBehaviour
    {
        [SerializeField]
        internal Floater floaterBuoy;
        [SerializeField]
        float m_MovingTurnSpeed = 360;
        [SerializeField]
        float m_StationaryTurnSpeed = 180;
        [SerializeField]
        float m_JumpPower = 12f;
        [Range(1f, 4f)]
        [SerializeField]
        float m_GravityMultiplier = 2f;
        [SerializeField]
        float m_RunCycleLegOffset = 0.2f; //specific to the character in sample assets, will need to be modified to work with others
        [SerializeField]
        float m_MoveSpeedMultiplier = 1f;
        [SerializeField]
        float m_AnimSpeedMultiplier = 1f;
        [SerializeField]
        float m_GroundCheckDistance = 0.1f;
        [SerializeField]
        bool m_doGroundCheck = false;
        [SerializeField]
        bool m_UseForceMovement = false;
        [SerializeField]
        float m_VerticalForce = 3f;

        Rigidbody m_Rigidbody;
        Animator m_Animator;
        bool m_IsGrounded = false;
        bool m_IsSwimming = true;

        float m_OrigGroundCheckDistance;
        const float k_Half = 0.5f;
        float m_TurnAmount;
        float m_ForwardAmount;
        Vector3 m_GroundNormal;
        float m_CapsuleHeight;
        Vector3 m_CapsuleCenter;
        CapsuleCollider m_Capsule;
        bool m_Crouching;

        float startingBuoyHeight;

        void Start()
        {
            m_Animator = GetComponent<Animator>();
            m_Rigidbody = GetComponent<Rigidbody>();
            m_Capsule = GetComponent<CapsuleCollider>();
            m_CapsuleHeight = m_Capsule.height;
            m_CapsuleCenter = m_Capsule.center;

            startingBuoyHeight = floaterBuoy.waterLevel;

            m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
            m_OrigGroundCheckDistance = m_GroundCheckDistance;

        }


        public void MoveThirdPerson(Vector3 move, bool crouch, bool jump)
        {

            // convert the world relative moveInput vector into a local-relative
            // turn amount and forward amount required to head in the desired
            // direction.
            if (move.magnitude > 1f) move.Normalize();
            move = transform.InverseTransformDirection(move);
            CheckGroundStatus();
            move = Vector3.ProjectOnPlane(move, m_GroundNormal);
            m_TurnAmount = Mathf.Atan2(move.x, move.z);
            m_ForwardAmount = move.z;

            ApplyExtraTurnRotation();

            // control and velocity handling is different when grounded and airborne:
            if (m_IsGrounded)
            {
                floaterBuoy.buoyUp = true;
                m_Rigidbody.useGravity = true;
                HandleGroundedMovement(crouch, jump);
            }
            else if(m_IsSwimming)
            {
                HandleSwimmingMovement(crouch, jump);
            }
            else
            {
                floaterBuoy.buoyUp = true;
                m_Rigidbody.useGravity = true;
                HandleAirborneMovement();
            }

            ScaleCapsuleForCrouching(crouch);
            PreventStandingInLowHeadroom();

            // send input and other state parameters to the animator
            UpdateAnimator(move);
        }

        public void MoveFirstPerson(Vector3 move, bool crouch, bool jump)
        {

            // convert the world relative moveInput vector into a local-relative
            // turn amount and forward amount required to head in the desired
            // direction.
            if (move.magnitude > 1f) move.Normalize();
            move = transform.InverseTransformDirection(move);
            if (m_doGroundCheck) CheckGroundStatus();
            move = Vector3.ProjectOnPlane(move, m_GroundNormal);
            m_TurnAmount = move.x;
            //m_TurnAmount = Mathf.Atan2(move.x, move.z);
            //Debug.Log("Move :" + move.x + ", " + move.z);
            //Debug.Log("Turn Amount: " + m_TurnAmount);
            m_ForwardAmount = move.z;

            ApplyExtraTurnRotation();

            // control and velocity handling is different when grounded and airborne:
            if (m_IsGrounded)
            {
                floaterBuoy.buoyUp = true;
                m_Rigidbody.useGravity = true;
                HandleGroundedMovement(crouch, jump);
            }
            else if (m_IsSwimming)
            {
                HandleSwimmingMovement(crouch, jump);
            }
            else
            {
                floaterBuoy.buoyUp = true;
                m_Rigidbody.useGravity = true;
                HandleAirborneMovement();
            }

            ScaleCapsuleForCrouching(crouch);
            PreventStandingInLowHeadroom();

            // send input and other state parameters to the animator
            UpdateAnimator(move);
        }

        void ApplyExtraTurnRotation()
        {
            // help the character turn faster (this is in addition to root rotation in the animation)
            float turnSpeed = Mathf.Lerp(m_StationaryTurnSpeed, m_MovingTurnSpeed, m_ForwardAmount);
            transform.Rotate(0, m_TurnAmount * turnSpeed * Time.deltaTime, 0);
        }

        void ScaleCapsuleForCrouching(bool crouch)
        {
            if (m_IsGrounded && crouch)
            {
                if (m_Crouching) return;
                m_Capsule.height = m_Capsule.height / 2f;
                m_Capsule.center = m_Capsule.center / 2f;
                m_Crouching = true;
            }
            else
            {
                Ray crouchRay = new Ray(m_Rigidbody.position + Vector3.up * m_Capsule.radius * k_Half, Vector3.up);
                float crouchRayLength = m_CapsuleHeight - m_Capsule.radius * k_Half;
                if (Physics.SphereCast(crouchRay, m_Capsule.radius * k_Half, crouchRayLength))
                {
                    m_Crouching = true;
                    return;
                }
                m_Capsule.height = m_CapsuleHeight;
                m_Capsule.center = m_CapsuleCenter;
                m_Crouching = false;
            }
        }

        void PreventStandingInLowHeadroom()
        {
            // prevent standing up in crouch-only zones
            if (!m_Crouching)
            {
                Ray crouchRay = new Ray(m_Rigidbody.position + Vector3.up * m_Capsule.radius * k_Half, Vector3.up);
                float crouchRayLength = m_CapsuleHeight - m_Capsule.radius * k_Half;

                if (Physics.SphereCast(crouchRay, m_Capsule.radius * k_Half, crouchRayLength))
                {
                    m_Crouching = true;
                }
            }
        }


        void UpdateAnimator(Vector3 move)
        {
            // update the animator parameters
            m_Animator.SetFloat("Forward", m_ForwardAmount, 0.1f, Time.deltaTime);
            m_Animator.SetFloat("Turn", m_TurnAmount, 0.1f, Time.deltaTime);
            m_Animator.SetBool("Crouch", m_Crouching);
            m_Animator.SetBool("OnGround", m_IsGrounded);
            m_Animator.SetBool("IsSwimming", m_IsSwimming);

            if (!m_IsGrounded && !m_IsSwimming)
            {
                m_Animator.SetFloat("Jump", m_Rigidbody.velocity.y);
            }

            // calculate which leg is behind, so as to leave that leg trailing in the jump animation
            // (This code is reliant on the specific run cycle offset in our animations,
            // and assumes one leg passes the other at the normalized clip times of 0.0 and 0.5)
            float runCycle =
                Mathf.Repeat(
                    m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime + m_RunCycleLegOffset, 1);
            float jumpLeg = (runCycle < k_Half ? 1 : -1) * m_ForwardAmount;
            if (m_IsGrounded)
            {
                m_Animator.SetFloat("JumpLeg", jumpLeg);
            }

            // the anim speed multiplier allows the overall speed of walking/running to be tweaked in the inspector,
            // which affects the movement speed because of the root motion.
            if (m_IsGrounded && move.magnitude > 0)
            {
                m_Animator.speed = m_AnimSpeedMultiplier;
            }
            else
            {
                // don't use that while airborne
                m_Animator.speed = 1;
            }
        }


        void HandleSwimmingMovement(bool crouch, bool jump)
        {
            if (m_UseForceMovement)
            {
                var forcePosition = m_Rigidbody.position;

                // go forward
                m_Rigidbody.AddForceAtPosition(m_Rigidbody.transform.forward * m_ForwardAmount * m_MoveSpeedMultiplier, forcePosition, ForceMode.Acceleration);

                // rotate yaw
                var rotVec = m_Rigidbody.transform.up;
                m_Rigidbody.AddTorque(rotVec * m_MovingTurnSpeed * m_TurnAmount, ForceMode.Acceleration);

                // handle going up and down
                if (crouch)
                {
                    floaterBuoy.buoyUp = false;
                    m_Rigidbody.useGravity = false;
                    m_Rigidbody.AddForceAtPosition(-m_Rigidbody.transform.up * m_VerticalForce, forcePosition, ForceMode.Acceleration);
                }
                else if (jump)
                {

                    floaterBuoy.waterLevel = WaterHeightSampler.Instance.GetWaterLevel(m_Rigidbody.transform.position);

                    if (m_Rigidbody.transform.position.y < floaterBuoy.waterLevel - floaterBuoy.buoyancyCentreOffset.y)
                    {
                        m_Rigidbody.AddForceAtPosition(m_Rigidbody.transform.up * m_VerticalForce, forcePosition, ForceMode.Acceleration);
                    }
                    else
                    {
                        floaterBuoy.buoyUp = true;
                        m_Rigidbody.useGravity = true;
                    }
                }
            }
            else
            {
                if (crouch)
                {
                    floaterBuoy.buoyUp = false;
                    m_Rigidbody.useGravity = false;

                    m_Rigidbody.velocity = (-m_Rigidbody.transform.up * 2);
                }
                else if (jump)
                {
                    floaterBuoy.waterLevel = WaterHeightSampler.Instance.GetWaterLevel(m_Rigidbody.transform.position);

                    if (m_Rigidbody.transform.position.y < floaterBuoy.waterLevel - floaterBuoy.buoyancyCentreOffset.y)
                    {
                        m_Rigidbody.velocity = (m_Rigidbody.transform.up * 2);
                    }
                    else
                    {
                        floaterBuoy.buoyUp = false;
                        m_Rigidbody.useGravity = true;
                    }
                }
                else if (m_Rigidbody.useGravity == false)
                {
                    // Zero out Y-value
                    m_Rigidbody.velocity = new Vector3(m_Rigidbody.velocity.x, 0, m_Rigidbody.velocity.z);
                }

                m_GroundCheckDistance = m_OrigGroundCheckDistance; // This is implemented to fix bug when jumping near water
                m_Rigidbody.velocity = (m_Rigidbody.transform.forward * m_ForwardAmount * 5) + new Vector3(0, m_Rigidbody.velocity.y, 0);
            }
        }

        void HandleAirborneMovement()
        {
            // apply extra gravity from multiplier:
            Vector3 extraGravityForce = (Physics.gravity * m_GravityMultiplier) - Physics.gravity;
            m_Rigidbody.AddForce(extraGravityForce);

            m_GroundCheckDistance = m_Rigidbody.velocity.y < 0 ? m_OrigGroundCheckDistance : 0.01f;
        }


        void HandleGroundedMovement(bool crouch, bool jump)
        {
            // check whether conditions are right to allow a jump:
            if (jump && !crouch && m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Grounded"))
            {
                // jump!
                m_Rigidbody.velocity = new Vector3(m_Rigidbody.velocity.x, m_JumpPower, m_Rigidbody.velocity.z);
                m_IsGrounded = false;
                m_Animator.applyRootMotion = false;
                m_GroundCheckDistance = 0.1f;
            }
        }


        public void OnAnimatorMove()
        {
            // we implement this function to override the default root motion.
            // this allows us to modify the positional speed before it's applied.
            if (m_IsGrounded && Time.deltaTime > 0)
            {
                Vector3 v = (m_Animator.deltaPosition * m_MoveSpeedMultiplier) / Time.deltaTime;

                // we preserve the existing y part of the current velocity.
                v.y = m_Rigidbody.velocity.y;
                m_Rigidbody.velocity = v;
            }
        }


        void CheckGroundStatus()
        {
            RaycastHit[] raycastHitInfo;
            bool hitGround = false;

#if UNITY_EDITOR
            // helper to visualise the ground check ray in the scene view
            Debug.DrawLine(transform.position + (Vector3.up * 0.1f), transform.position + (Vector3.up * 0.1f) + (Vector3.down * m_GroundCheckDistance), Color.cyan);
#endif

            raycastHitInfo = Physics.RaycastAll(transform.position + (Vector3.up * 0.1f), Vector3.down, m_GroundCheckDistance);

            foreach(RaycastHit hitInfo in raycastHitInfo)
            {
                // 0.1f is a small offset to start the ray from inside the character
                // it is also good to note that the transform position in the sample assets is at the base of the character
                if (!hitInfo.collider.CompareTag("Water") && floaterBuoy.forceFactor < floaterBuoy.floatHeight)
                {
                    m_GroundNormal = hitInfo.normal;
                    m_IsGrounded = true;
                    m_Animator.applyRootMotion = true;

                    hitGround = true;
                }
            }

            if(!hitGround)
            {
                m_IsGrounded = false;
                m_GroundNormal = Vector3.up;
                m_Animator.applyRootMotion = false;
            }

        }
/*
        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Water"))
            {
                m_IsSwimming = true;

                floaterBuoy.waterLevel = other.transform.position.y;
            }
        }

        void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Water"))
            {
                m_IsSwimming = true;
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Water"))
            {
                m_IsSwimming = false;

                floaterBuoy.waterLevel = startingBuoyHeight;
            }
        }
        */

    }
}
