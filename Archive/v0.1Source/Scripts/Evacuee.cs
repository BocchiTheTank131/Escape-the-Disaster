using UnityEngine;
using UnityEngine.AI;
namespace FloodSurvival
{
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class Evacuee : MonoBehaviour
    {
        public Vector3 destination;
        public Transform body, leftLeg, rightLeg, leftArm, rightArm;
        public bool Panicking {get; private set;}
        public bool HasPath => agent && agent.isOnNavMesh && (agent.hasPath || Vector3.Distance(transform.position,destination)<2);
        NavMeshAgent agent;
        Vector3 origin;
        float nextWander;
        void Awake() { agent=GetComponent<NavMeshAgent>(); origin=transform.position; nextWander=Time.time+Random.Range(1f,4f); }
        void Start()
        {
            // Surfaces register in OnEnable; enable agents after all scene OnEnable calls.
            if(NavMesh.SamplePosition(transform.position,out var hit,5,NavMesh.AllAreas))
            {
                transform.position=hit.position; agent.enabled=true;
                if(Panicking && agent.isOnNavMesh)agent.SetDestination(destination);
            }
            else Debug.LogWarning("No walkable surface near evacuee "+name,this);
        }
        public void Panic()
        {
            Panicking=true; agent.speed=Random.Range(3.5f,4.6f); agent.acceleration=12;
            if(agent.isActiveAndEnabled && agent.isOnNavMesh) agent.SetDestination(destination);
        }
        void Update()
        {
            var game=GameManager.Instance; if(!game)return;
            if(game.State==GameState.Menu && Time.time>nextWander && agent.isOnNavMesh)
            {
                nextWander=Time.time+Random.Range(5f,9f);
                if(NavMesh.SamplePosition(origin+new Vector3(Random.Range(-3f,3f),0,Random.Range(-3f,3f)),out var hit,5,NavMesh.AllAreas)) agent.SetDestination(hit.position);
            }
            float stride=Mathf.Sin(Time.time*12+transform.GetSiblingIndex())*Mathf.Min(agent.velocity.magnitude*13,40);
            if(leftLeg) leftLeg.localRotation=Quaternion.Euler(stride,0,0);
            if(rightLeg) rightLeg.localRotation=Quaternion.Euler(-stride,0,0);
            if(leftArm) leftArm.localRotation=Quaternion.Euler(-stride,0,Panicking?-25:-5);
            if(rightArm) rightArm.localRotation=Quaternion.Euler(stride,0,Panicking?25:5);
            if(body) body.localPosition=new Vector3(0,1.05f+Mathf.Abs(Mathf.Sin(Time.time*12))*.025f*Mathf.Min(agent.velocity.magnitude,1),0);
        }
    }
}
