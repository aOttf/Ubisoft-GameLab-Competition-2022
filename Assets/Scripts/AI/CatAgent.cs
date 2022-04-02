using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DecisionMaking.StateMachine;
using Mirror;

[RequireComponent(typeof(FSM), typeof(FSMStateBehaviour))]
public class CatAgent : NetworkBehaviour
{
    //Caches
    private FSM m_fsm;
    private BlackboardManager m_manager;
    private CatInteractable m_petInteract;
    private MeshRenderer m_renderer;

    [SerializeField] private int energy;

    // Movement
    //private float speed = 1f;
    private float[] x_limits = new float[] { 18f, 2f };
    private float[] z_limits = new float[] { -18f, -3.5f };

    private Vector3 destination;
    private UnityEngine.AI.NavMeshAgent NMAgent;
    private UnityEngine.AI.NavMeshPath path;
    private float elapsed = 0.0f;

    [Header("Cat Moving Speed Params")]
    [SerializeField] private float m_maxSpeed = 2f;
    [SerializeField] private float m_speedLerpFactor = .5f;

    [Header("Cat Sitting State Params")]
    [SerializeField] private int m_energyIncreasingAmount = 3;

    private bool m_arrivedCurrentPath;
    private bool m_reachedMaxSpeed;

    [Header("Cat Spawning Socks Params")]
    [SerializeField] private GameObject m_sockPrefab;
    [SerializeField] private SpawnPointsManager m_spawnManager;
    [SerializeField] private float m_spawnRadius = 3f;
    [SerializeField] [Range(0, 50)] private int m_spawnLimit;
    [SerializeField] [Range(0, 1)] private float m_spawnProbility = .1f;
    private int m_curSpawnNum = 0;

    [Header("Cat Petting Params")]
    [Tooltip("When the cat  is being pet, the rendering color will be changed.")]
    [SerializeField] private Color m_petColor;

    [Tooltip("When the cat turns around to face you")]
    [SerializeField] private float m_petAngularSpeed;

    [Tooltip("When the cat approches you")]
    [SerializeField] private float m_petLinearSpeed;

    [SerializeField] private float m_petStoppingDistance;

    private Color m_normalColor;
    private Player m_petter;

    private void Awake()
    {
        NMAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        NMAgent.speed = 0;
        m_fsm = GetComponent<FSM>();
        m_manager = BlackboardManager.Instance;
        m_petInteract = GetComponent<CatInteractable>();
        m_renderer = GetComponent<MeshRenderer>();
        if (m_spawnManager == null)
            m_spawnManager = GameObject.Find("Waypoints").GetComponent<SpawnPointsManager>();
        destination = this.transform.position;

        EventManager.AddListener("PetCat", OnUpdatePetStatus);
    }

    private void OnDestroy()
    {
        EventManager.RemoveListener("PetCat", OnUpdatePetStatus);
    }

    // Start is called before the first frame update
    private void Start()
    {
        m_fsm.TurnOn();

        energy = 5;

        //path = new UnityEngine.AI.NavMeshPath();
        destination = new Vector3(10f, 1.77f, -4f);
        //elapsed = 0.0f;

        m_normalColor = m_renderer.material.color;
    }

    private void FixedUpdate()
    {
        m_manager.SetInteger("Energy", energy);
    }

    public void OnUpdatePetStatus()
    {
        // Only the local player could have ever caused this method to be called.

        m_petter = Player.Instance;
        m_manager.SetTrigger("OnPet");
        print("OnPet: " + m_manager.GetTrigger("OnPet"));
    }

    // Called when we first enter the 'Walking' state
    public void EnterWalk()
    {
        //transform.position += new Vector3(0,0,0.5f) * Time.deltaTime * speed;

        NMAgent.enabled = true;
        m_reachedMaxSpeed = false;
        m_arrivedCurrentPath = false;

        //NMAgent.speed = 2;

        PickRandomPos();

        /*
        elapsed += Time.deltaTime;
        if (elapsed > 1.0f)
        {
            elapsed -= 1.0f;
            UnityEngine.AI.NavMesh.CalculatePath(transform.position, destination, UnityEngine.AI.NavMesh.AllAreas, path);
        }*/
    }

    public void EnterSit()
    {
        //NMAgent.enabled = false;
    }

    public void Walk()
    {
        energy -= 2;

        m_arrivedCurrentPath = NMAgent.remainingDistance <= NMAgent.stoppingDistance;
        m_reachedMaxSpeed = (m_maxSpeed - NMAgent.speed) <= float.Epsilon;
        if (!m_reachedMaxSpeed && !m_arrivedCurrentPath)
        {
            NMAgent.speed = Mathf.Lerp(NMAgent.speed, m_maxSpeed, m_speedLerpFactor);
        }

        // Reached destination, needs to find new one
        if (NMAgent.remainingDistance <= NMAgent.stoppingDistance)
        {
            if (!NMAgent.hasPath || NMAgent.velocity.sqrMagnitude == 0f)
            {
                print("This is actually been called");
                PickRandomPos();
            }
        }
    }

    public void Sit()
    {
        energy += m_energyIncreasingAmount;
        if ((NMAgent.speed = Mathf.Lerp(NMAgent.speed, 0f, m_speedLerpFactor)) < .001f)
        {
            SetStillState();
        }
    }

    public void ExitSit()
    {
        ResetStillState();
    }

    public void PickRandomPos()
    {
        int walk_radius = 20;
        Vector3 random_dir = Random.insideUnitSphere * walk_radius;
        random_dir += this.transform.position;
        UnityEngine.AI.NavMeshHit hit;
        UnityEngine.AI.NavMesh.SamplePosition(random_dir, out hit, walk_radius, 1);
        NMAgent.destination = hit.position;
    }

    public void SpawnSock()
    {
        if (m_curSpawnNum < m_spawnLimit)
        {
            if (Random.Range(0, 1f) > m_spawnProbility)
                return;

            m_curSpawnNum++;
            //Vector2 spawnOffset = Random.insideUnitCircle * m_spawnRadius;
            //Vector3 spawnPos = transform.position + new Vector3(spawnOffset.x, 0, spawnOffset.y);

            Vector3 spawnPos;
            if (m_spawnManager.TryQueryNeighbour(transform.position, out spawnPos))
                NetworkServer.Spawn(Instantiate(m_sockPrefab, spawnPos, m_sockPrefab.transform.rotation));
        }
    }

    public void EnterPet()
    {
        m_arrivedCurrentPath = false;
        //NMAgent.stoppingDistance = m_petStoppingDistance;
    }

    public void Pet()
    {
        if (!m_arrivedCurrentPath)
        {
            NMAgent.speed = Mathf.Lerp(NMAgent.speed, m_petLinearSpeed, m_speedLerpFactor);
            NMAgent.destination = m_petter.transform.position;
            if (m_arrivedCurrentPath = (NMAgent.remainingDistance < m_petStoppingDistance)) //Should modify in the future
            {
                NMAgent.speed = 0;
                SetStillState();
                StartCoroutine(nameof(FacePlayer));
            }
        }
        //else
        //{
        //    transform.forward = Vector3.Slerp(transform.forward, m_petter.transform.position - transform.position, .1f);
        //}
    }

    public void ExitPet()
    {
        m_petter = null;
        ResetStillState();
        NMAgent.stoppingDistance = 0f;
        StopCoroutine(nameof(FacePlayer));
    }

    private IEnumerator FacePlayer()
    {
        while (true)
        {
            transform.forward = Vector3.RotateTowards(transform.forward, m_petter.transform.position - transform.position, NMAgent.angularSpeed * Time.deltaTime * Mathf.PI / 180f, 1f);
            yield return new WaitForEndOfFrame();
        }
    }

    public void SetStillState()
    {
        m_renderer.material.color = m_petColor;
    }

    public void ResetStillState()
    {
        m_renderer.material.color = m_normalColor;
    }
}