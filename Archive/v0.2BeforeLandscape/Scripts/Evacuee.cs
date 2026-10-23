using UnityEngine;
using UnityEngine.AI;
namespace FloodSurvival
{
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class Evacuee : MonoBehaviour
    {
        public Vector3 destination;
        public Transform body, leftLeg, rightLeg, leftArm, rightArm;
        public Animator animator;
        public bool collectBelongings;
        public Vector3 homeStop;
        public bool Panicking {get; private set;}
        public bool HasPath => agent && agent.isOnNavMesh && (agent.hasPath || Vector3.Distance(transform.position,destination)<2);
        NavMeshAgent agent;
        NavMeshPath wanderPath;
        Vector3 origin;
        float nextWander;
        float evacuationAt,voiceAt;
        void Awake() { agent=GetComponent<NavMeshAgent>();wanderPath=new NavMeshPath(); origin=transform.position; nextWander=Time.time+Random.Range(1f,4f); }
        void Start()
        {
            // Surfaces register in OnEnable; enable agents after all scene OnEnable calls.
            // Keep the query local: a large radius can select a different floor or roof.
            if(NavMesh.SamplePosition(transform.position,out var hit,2,NavMesh.AllAreas) && Mathf.Abs(hit.position.y-transform.position.y)<.75f)
            {
                transform.position=hit.position;origin=hit.position; agent.enabled=true;
                if(Panicking && agent.isOnNavMesh)agent.SetDestination(destination);
            }
            else Debug.LogWarning("No walkable surface near evacuee "+name,this);
        }
        public void Panic()
        {
            if(Panicking)return;
            Panicking=true; agent.speed=Random.Range(3.5f,4.6f); agent.acceleration=12; evacuationAt=collectBelongings?Time.time+12+transform.GetSiblingIndex():0;
            if(agent.isActiveAndEnabled && agent.isOnNavMesh) agent.SetDestination(collectBelongings?homeStop:destination);
            voiceAt=Time.time+Random.Range(.2f,3f);
        }
        public void EvacuateNow() { evacuationAt=0; if(agent && agent.isActiveAndEnabled && agent.isOnNavMesh)agent.SetDestination(destination); }
        void Update()
        {
            var game=GameManager.Instance; if(!game)return;
            if(!agent || !agent.isActiveAndEnabled || !agent.isOnNavMesh || game.State==GameState.Paused)return;
            if(!Panicking && Time.time>nextWander)
            {
                nextWander=Time.time+Random.Range(5f,9f);
                if(NavMesh.SamplePosition(origin+new Vector3(Random.Range(-3f,3f),0,Random.Range(-3f,3f)),out var hit,2,NavMesh.AllAreas)
                    && Mathf.Abs(hit.position.y-origin.y)<.75f && agent.CalculatePath(hit.position,wanderPath) && wanderPath.status==NavMeshPathStatus.PathComplete)
                    agent.SetDestination(hit.position);
            }
            if(Panicking && evacuationAt>0 && Time.time>=evacuationAt){evacuationAt=0;agent.SetDestination(destination);}
            if(Panicking && voiceAt>0 && Time.time>=voiceAt){voiceAt=0;game.audioSystem.NpcReaction(transform.position,transform.GetSiblingIndex());}
            if(animator){animator.SetFloat("Speed",agent.velocity.magnitude);return;}
            float stride=Mathf.Sin(Time.time*12+transform.GetSiblingIndex())*Mathf.Min(agent.velocity.magnitude*13,40);
            if(leftLeg) leftLeg.localRotation=Quaternion.Euler(stride,0,0);
            if(rightLeg) rightLeg.localRotation=Quaternion.Euler(-stride,0,0);
            if(leftArm) leftArm.localRotation=Quaternion.Euler(-stride,0,Panicking?-25:-5);
            if(rightArm) rightArm.localRotation=Quaternion.Euler(stride,0,Panicking?25:5);
            if(body) body.localPosition=new Vector3(0,1.05f+Mathf.Abs(Mathf.Sin(Time.time*12))*.025f*Mathf.Min(agent.velocity.magnitude,1),0);
        }
    }
}
