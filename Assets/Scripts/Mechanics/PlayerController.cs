using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Platformer.Gameplay;
using static Platformer.Core.Simulation;
using Platformer.Model;
using Platformer.Core;

namespace Platformer.Mechanics
{
    public class PlayerController : KinematicObject
    {
        public AudioClip jumpAudio;
        public AudioClip respawnAudio;
        public AudioClip ouchAudio;

        public float maxSpeed = 7;
        public float jumpTakeOffSpeed = 7;

        public JumpState jumpState = JumpState.Grounded;
        private bool stopJump;
        public Collider2D collider2d;
        public AudioSource audioSource;
        public Health health;
        public bool controlEnabled = true;

        bool jump;
        Vector2 move;
        SpriteRenderer spriteRenderer;
        internal Animator animator;
        readonly PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        public Bounds Bounds => collider2d.bounds;

        public GameObject projectilePrefab;
        public Transform firePoint;
        public float projectileSpeed = 10f;

        public int maxAmmo = 8;
        private int currentAmmo;
        public float reloadTime = 2f;
        private bool isReloading = false;

        public float levelTimeLimit = 600f; // 10-minute timer in seconds
        private float currentTime;

        void Awake()
        {
            health = GetComponent<Health>();
            audioSource = GetComponent<AudioSource>();
            collider2d = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
            currentAmmo = maxAmmo;
            currentTime = levelTimeLimit;
        }

        protected override void Update()
        {
            if (controlEnabled)
            {
                move.x = Input.GetAxis("Horizontal");
                if (jumpState == JumpState.Grounded && Input.GetButtonDown("Jump"))
                    jumpState = JumpState.PrepareToJump;
                else if (Input.GetButtonUp("Jump"))
                {
                    stopJump = true;
                    Schedule<PlayerStopJump>().player = this;
                }

                if (Input.GetButtonDown("Fire1"))
                {
                    FireWeapon();
                }

                if (Input.GetKeyDown(KeyCode.R) && currentAmmo < maxAmmo && !isReloading)
                {
                    StartCoroutine(Reload());
                }
            }
            else
            {
                move.x = 0;
            }
            
            UpdateTimer();
            UpdateJumpState();
            base.Update();
        }

        void UpdateTimer()
        {
            currentTime -= Time.deltaTime;
            if (currentTime <= 0)
            {
                LevelFailed();
            }
        }

        void LevelFailed()
        {
            Debug.Log("Time's up! Level failed.");
            // Reload current scene or show Game Over
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        void FireWeapon()
        {
            if (isReloading) return;

            if (currentAmmo > 0)
            {
                if (projectilePrefab != null && firePoint != null)
                {
                    GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
                    
                    if (projectile.GetComponent<Rigidbody2D>() == null)
                    {
                        Rigidbody2D rb = projectile.AddComponent<Rigidbody2D>();
                        rb.gravityScale = 0;
                    }
                    if (projectile.GetComponent<BoxCollider2D>() == null)
                    {
                        projectile.AddComponent<BoxCollider2D>();
                    }
                    if (projectile.GetComponent<Projectile>() == null)
                    {
                        projectile.AddComponent<Projectile>();
                    }

                    Rigidbody2D rb2d = projectile.GetComponent<Rigidbody2D>();
                    rb2d.velocity = new Vector2(spriteRenderer.flipX ? -projectileSpeed : projectileSpeed, 0);

                    currentAmmo--;
                }
            }
            else
            {
                StartCoroutine(Reload());
            }
        }

        IEnumerator Reload()
        {
            isReloading = true;
            Debug.Log("Reloading...");
            yield return new WaitForSeconds(reloadTime);
            currentAmmo = maxAmmo;
            isReloading = false;
            Debug.Log("Reloaded!");
        }

        void UpdateJumpState()
        {
            jump = false;
            switch (jumpState)
            {
                case JumpState.PrepareToJump:
                    jumpState = JumpState.Jumping;
                    jump = true;
                    stopJump = false;
                    break;
                case JumpState.Jumping:
                    if (!IsGrounded)
                    {
                        Schedule<PlayerJumped>().player = this;
                        jumpState = JumpState.InFlight;
                    }
                    break;
                case JumpState.InFlight:
                    if (IsGrounded)
                    {
                        Schedule<PlayerLanded>().player = this;
                        jumpState = JumpState.Landed;
                    }
                    break;
                case JumpState.Landed:
                    jumpState = JumpState.Grounded;
                    break;
            }
        }

        protected override void ComputeVelocity()
        {
            if (jump && IsGrounded)
            {
                velocity.y = jumpTakeOffSpeed * model.jumpModifier;
                jump = false;
            }
            else if (stopJump)
            {
                stopJump = false;
                if (velocity.y > 0)
                {
                    velocity.y = velocity.y * model.jumpDeceleration;
                }
            }

            if (move.x > 0.01f)
                spriteRenderer.flipX = false;
            else if (move.x < -0.01f)
                spriteRenderer.flipX = true;

            animator.SetBool("grounded", IsGrounded);
            animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / maxSpeed);

            targetVelocity = move * maxSpeed;
        }

        public enum JumpState
        {
            Grounded,
            PrepareToJump,
            Jumping,
            InFlight,
            Landed
        }
    }
}
