using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(BoxCollider)), RequireComponent(typeof(AudioSource))]
public class StandardizedProjectile : MonoBehaviour
{
    #region Private Values
    // Bow communication variables
    [HideInInspector]
    public Rigidbody rigid;
    [HideInInspector]
    public StandardizedBow bowScript;
    [HideInInspector]
    public Transform quiver;
    // Inside private variables
    private BoxCollider boxCollider;
    private AudioSource audioSource;
    private float currentTime = 0;
    private bool collisionHappened = false;
    #endregion

    // Public variables
    [Tooltip("Amount of time to deactivate and de-pool the projectile.")]
    public float timeLimitToDePool = 5f;
    [Range(0, 1f), Tooltip("This value is multiplied with the emission amount of the particle. Higher the value, more particle emission.")]
    public float effectOfVelocityOnParticleEmission = 0.5f;
    [Header("     Projectile On Hit Particles")]
    public GameObject projectileHitParticleFruit;


    [Header("     Projectile On Hit Sounds")]
    public AudioClip hitSoundFruit;


    [Header("     Projectile On Hit Detection Tags")]
    public string fruitDetectionTagToHash = "Fruit";


    // Start is called before the first frame update
    void Start()
    {
        // Reset timer
        currentTime = 0;
        // Cache rigidbody
        if (rigid == null)
        {
            // Just added as a precaution
            Debug.Log("Rigidbody is found through an expensive method. This is not good. There might be an error in pooling. Consider checking what you changed in script.");
            rigid = GetComponent<Rigidbody>();
        }
        rigid.useGravity = false;  // Disable gravity so projectile doesn't fall while being drawn
        if (bowScript == null)
        {
            // Just added as a precaution
            Debug.Log("StandardizeBow script is found through an expensive method. This is not good. Consider checking what you changed in script.");
            bowScript = GetComponentInParent<StandardizedBow>();
        }
        // Just ensuring that your colliders are trigger instead of actual physics colliders.
        boxCollider = GetComponent<BoxCollider>();
        if (boxCollider != null)
        {
            boxCollider.isTrigger = true;
        }
        // Hashing is here to prevent using strings for comparison - The projectile pooling is called from the bow.
        // 
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    // Update is called once per frame
    void Update()
    {
        // If it is released, meaning that projectile is not parented to string, start procedure
        if (transform.parent == null)
        {
            currentTime += Time.deltaTime;
            // If the given time limit is passed, start with dequeuing and prepare for re-pooling.
            if (currentTime > timeLimitToDePool)
            {
                rigid.velocity = Vector3.zero;
                rigid.useGravity = false;
                collisionHappened = false;
                rigid.constraints = RigidbodyConstraints.None;
                currentTime = 0;

                // Deactivate the last used particle
                // Re-add the projectile to the bow quiver
                transform.position = Vector3.zero;
                gameObject.SetActive(false);
                transform.parent = quiver;
                bowScript.projectilePool.Enqueue(gameObject); // Pooling
            }
            else
            {
                if (!collisionHappened)
                {
                    // Start from 90 vert and rotate depending on the launch angle and velocity
                    transform.rotation = Quaternion.LookRotation(rigid.velocity);
                }
            }
        }
    }

    // On Contact With Collider
    private void OnTriggerEnter(Collider other)
    {

        if (other.tag == "Fruit")
        {
            GameObject particleInstance = Instantiate(projectileHitParticleFruit, transform.position, Quaternion.identity);
            ParticleSystem ps = particleInstance.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var emission = ps.emission;
                emission.SetBurst(0, new ParticleSystem.Burst(0.0f, 10)); // replace 10 with your desired burst amount
                ps.Play();
            }
            Destroy(other.gameObject);
            audioSource.PlayOneShot(hitSoundFruit);
        }
    }
}


