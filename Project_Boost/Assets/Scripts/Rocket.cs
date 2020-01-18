using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Rocket : MonoBehaviour
{
    private Rigidbody rb;
    private AudioSource audioSource;

    [SerializeField] float RocketRot = 200.0f;
    [SerializeField] float RocketThrust = 30.0f;
    [SerializeField] float levelLoadDelay = 1f;

    [SerializeField] AudioClip mainThrust;
    [SerializeField] AudioClip deathSound;
    [SerializeField] AudioClip newLevelSound;

    [SerializeField] ParticleSystem mainThrustParticles;
    [SerializeField] ParticleSystem deathParticles;
    [SerializeField] ParticleSystem newLevelParticles;

    private bool enableCollisions = true;
    private bool isTransitioning;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        if(!isTransitioning)
        {
            Thrust();
            Rotate();
        }

    }
    private void Update()
    {
        if (Debug.isDebugBuild) { CheckDebugKeys(); }
    }

    private void CheckDebugKeys()
    {
        if (Input.GetKeyDown(KeyCode.L)) { LoadNextLevel(); }
        else if (Input.GetKeyDown(KeyCode.C)) { enableCollisions = !enableCollisions; }
    }

    private void Thrust()
    {
        
        if (Input.GetKey(KeyCode.Space))
        {
            ApplyThrust();
        }
        else
        {
            StopApplyingThrust();
        }
    }

    private void StopApplyingThrust()
    {
        audioSource.Stop();
        mainThrustParticles.Stop();
    }

    private void ApplyThrust()
    {
        rb.AddRelativeForce(Vector3.up * RocketThrust * Time.deltaTime);
        mainThrustParticles.Play();
        if (!audioSource.isPlaying)
            audioSource.PlayOneShot(mainThrust);
    }

    private void Rotate()
    {
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            RotateManually(Time.deltaTime * RocketRot);
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            RotateManually(-Time.deltaTime * RocketRot);
        }

    }

    private void RotateManually(float rotation)
    {
        rb.freezeRotation = true;
        transform.Rotate(Vector3.forward * rotation);
        rb.freezeRotation = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(isTransitioning || (!enableCollisions)) { return; }
        switch (collision.gameObject.tag)
        {
            case "Friendly":
                print("OK");
                break;
            case "Fuel":
                print("Fuel");
                break;
            case "Finish":
                SuccessSequence();
                break;
            default:
                DeathSequence();
                break;
        }
    }

    private void SuccessSequence()
    {
        isTransitioning = true;
        audioSource.Stop();
        audioSource.PlayOneShot(newLevelSound);
        newLevelParticles.Play();
        Invoke("LoadNextLevel", levelLoadDelay);
    }

    private void DeathSequence()
    {
        isTransitioning = true;
        audioSource.Stop();
        audioSource.PlayOneShot(deathSound);
        deathParticles.Play();
        Invoke("LoadFirstLevel", levelLoadDelay);
    }

    private void LoadFirstLevel()
    {
        SceneManager.LoadScene(0);
    }

    private void LoadNextLevel()
    {
        int sceneCount = SceneManager.sceneCountInBuildSettings;
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = (currentSceneIndex + 1) % sceneCount;
        SceneManager.LoadScene(nextSceneIndex);
    }
}
