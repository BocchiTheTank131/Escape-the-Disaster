using UnityEngine;
using UnityEngine.InputSystem;
namespace FloodSurvival
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class FirstPersonController : MonoBehaviour
    {
        public Camera view;
        public Light flashlight;
        public InventorySystem inventory;
        public float walkSpeed=4.8f, sprintSpeed=8f, sensitivity=.12f;
        public float Health {get; private set;}=100;
        public float Oxygen {get; private set;}=100;
        public PickupItem FocusedItem {get; private set;}
        public InspectableObject FocusedObject {get; private set;}
        public bool Swimming {get; private set;}
        public float BatteryCharge {get; private set;}=100;
        public bool LampOn => flashlight && flashlight.enabled;
        CharacterController controller;
        Vector3 velocity, planar;
        float pitch, bob, shake, seatedYaw, stepDistance;
        bool seated;
        InputAction move, look, sprint, jump, interact, bag, pause, lamp, medicine,inspect,reload,radio;
        public bool AcceptsInput => GameManager.Instance && GameManager.Instance.IsPlaying && !GameManager.Instance.InventoryOpen;
        void Awake()
        {
            controller=GetComponent<CharacterController>();
            move=new InputAction("Move",InputActionType.Value); move.AddCompositeBinding("2DVector").With("Up","<Keyboard>/w").With("Down","<Keyboard>/s").With("Left","<Keyboard>/a").With("Right","<Keyboard>/d");
            look=new InputAction("Look",InputActionType.Value,"<Mouse>/delta");
            sprint=new InputAction("Sprint",InputActionType.Button,"<Keyboard>/leftShift");
            jump=new InputAction("Jump",InputActionType.Button,"<Keyboard>/space");
            interact=new InputAction("Interact",InputActionType.Button,"<Keyboard>/e");
            bag=new InputAction("Inventory",InputActionType.Button,"<Keyboard>/tab");
            pause=new InputAction("Pause",InputActionType.Button,"<Keyboard>/escape");
            lamp=new InputAction("Flashlight",InputActionType.Button,"<Keyboard>/f");
            medicine=new InputAction("Medicine",InputActionType.Button,"<Keyboard>/h");
            inspect=new InputAction("Inspect",InputActionType.Button,"<Keyboard>/i");
            reload=new InputAction("Batteries",InputActionType.Button,"<Keyboard>/r");
            radio=new InputAction("Radio",InputActionType.Button,"<Keyboard>/v");
            sensitivity=PlayerPrefs.GetFloat("MouseSensitivity",.12f);
        }
        void OnEnable() { foreach(var a in Actions()) a?.Enable(); }
        void OnDisable() { foreach(var a in Actions()) a?.Disable(); }
        InputAction[] Actions() => new[]{move,look,sprint,jump,interact,bag,pause,lamp,medicine,inspect,reload,radio};
        void OnDestroy() { foreach(var a in Actions()) a?.Dispose(); }
        void Update()
        {
            var game=GameManager.Instance; if(!game) return;
            if(pause.WasPressedThisFrame()) game.Escape();
            if(game.IsPlaying && bag.WasPressedThisFrame()) game.ToggleInventory();
            if(game.IsPlaying && LampOn){BatteryCharge=Mathf.Max(0,BatteryCharge-Time.deltaTime*.6f); if(BatteryCharge<=0)flashlight.enabled=false;}
            if(!AcceptsInput) { FocusedItem=null; return; }
            var delta=look.ReadValue<Vector2>()*sensitivity;
            transform.Rotate(0,delta.x,0); pitch=Mathf.Clamp(pitch-delta.y,seated?-55:-82,seated?55:82);
            if(seated)
            {
                float yaw=seatedYaw+Mathf.Clamp(Mathf.DeltaAngle(seatedYaw,transform.eulerAngles.y),-80,80);
                transform.rotation=Quaternion.Euler(0,yaw,0); view.transform.localRotation=Quaternion.Euler(pitch,0,0);
                view.transform.localPosition=new Vector3(0,1.15f,0);
                if(game.EmergencyStarted && interact.WasPressedThisFrame())game.story.StandUp();
                else if(OpeningSequence.CanSkip && jump.WasPressedThisFrame())game.story.Skip();
                return;
            }
            Vector2 input=move.ReadValue<Vector2>();
            float speed=sprint.IsPressed()?sprintSpeed:walkSpeed;
            float water=game.flood.GetLevel(transform.position);
            float depth=water-transform.position.y;
            Swimming=game.flood.IsFlooding && depth>1;
            if(depth>.35f) speed*=Swimming?.55f:.72f;
            Vector3 desired=(transform.right*input.x+transform.forward*input.y)*speed;
            planar=Vector3.Lerp(planar,desired,1-Mathf.Exp(-13*Time.deltaTime));
            if(Swimming)velocity.y=Mathf.Lerp(velocity.y,jump.IsPressed()?2.8f:-.75f,1-Mathf.Exp(-5*Time.deltaTime));
            else
            {
                if(controller.isGrounded && velocity.y<0) velocity.y=-2;
                if(controller.isGrounded && jump.WasPressedThisFrame()) velocity.y=6;
                velocity.y-=19*Time.deltaTime;
            }
            Vector3 current=game.flood.CurrentAt(transform.position);
            controller.Move((planar+velocity+current)*Time.deltaTime);
            if(controller.isGrounded && planar.magnitude>1)
            {
                stepDistance+=planar.magnitude*Time.deltaTime;
                if(stepDistance>2.1f){stepDistance=0; string surface="grass"; if(Physics.Raycast(transform.position+Vector3.up*.3f,Vector3.down,out var floor,.7f,~(1<<2)))surface=floor.collider.name; game.audioSystem.Footstep(surface,depth>.1f);}
            }
            bob+=Time.deltaTime*planar.magnitude*1.9f;
            shake=Mathf.MoveTowards(shake,0,Time.deltaTime*.7f);
            view.transform.localRotation=Quaternion.Euler(pitch+Mathf.Sin(Time.time*37)*shake,0,Mathf.Cos(Time.time*43)*shake*.5f);
            view.transform.localPosition=new Vector3(0,1.64f+(controller.isGrounded?Mathf.Sin(bob)*.022f:0),0);
            FocusedItem=null; FocusedObject=null;
            if(Physics.Raycast(view.transform.position,view.transform.forward,out var hit,3.1f,~(1<<2),QueryTriggerInteraction.Ignore)){FocusedItem=hit.collider.GetComponentInParent<PickupItem>();FocusedObject=hit.collider.GetComponentInParent<InspectableObject>();}
            if(FocusedItem && interact.WasPressedThisFrame())
            {
                if(FocusedItem.Collect(inventory)){game.audioSystem.Pickup();game.objectives.CollectedAt(FocusedItem.transform.position);game.Announce("SUPPLY SECURED",FocusedItem.definition.displayName+" added to your bag.",2);FocusedItem=null;}
                else game.Announce("BAG FULL OR ALREADY EQUIPPED","Use a supply to make space, or find the backpack.",3);
            }
            if(inspect.WasPressedThisFrame())
            {
                if(FocusedItem)game.Announce(FocusedItem.definition.displayName.ToUpperInvariant(),FocusedItem.definition.description,6);
                else if(FocusedObject)game.Announce(FocusedObject.title,FocusedObject.description,6);
            }
            if(lamp.WasPressedThisFrame()) ToggleFlashlight();
            if(medicine.WasPressedThisFrame()) UseMedicine();
            if(reload.WasPressedThisFrame())UseItem(ItemKind.Batteries);
            if(radio.WasPressedThisFrame())UseItem(ItemKind.Radio);
        }
        public void ToggleFlashlight() { if(inventory.Count(ItemKind.Flashlight)>0 && BatteryCharge>0) flashlight.enabled=!flashlight.enabled; }
        public void UseMedicine() { if(Health<100 && inventory.Consume(ItemKind.Medicine)) Health=Mathf.Min(100,Health+40); }
        public void UseItem(ItemKind kind)
        {
            if(kind==ItemKind.Medicine)UseMedicine();
            else if(kind==ItemKind.Flashlight)ToggleFlashlight();
            else if(kind==ItemKind.Batteries && BatteryCharge<99 && inventory.Consume(kind)){BatteryCharge=100;GameManager.Instance.audioSystem.Pickup();}
            else if(kind==ItemKind.Food && Health<100 && inventory.Consume(kind))Health=Mathf.Min(100,Health+12);
            else if(kind==ItemKind.Water && Health<100 && !Swimming && inventory.Consume(kind))Health=Mathf.Min(100,Health+8);
            else if(kind==ItemKind.Radio && inventory.Count(kind)>0){GameManager.Instance.audioSystem.Radio();GameManager.Instance.Announce("EMERGENCY CHANNEL 04","Ridge Shelter remains above the flood. Follow the amber posts east. Stay away from the valley floor.",7);}
        }
        public void Sit(Vector3 position,float yaw) { controller.enabled=false;seated=true;seatedYaw=yaw;transform.SetPositionAndRotation(position,Quaternion.Euler(0,yaw,0));pitch=0;view.transform.localPosition=Vector3.up*1.15f;view.transform.localRotation=Quaternion.identity; }
        public void Stand(Vector3 position) { seated=false;transform.position=position;controller.enabled=true;velocity=planar=Vector3.zero;pitch=0;view.transform.localPosition=Vector3.up*1.64f; }
        public void WaterTick(float level,float dt)
        {
            bool submerged=level>view.transform.position.y;
            Oxygen=Mathf.Clamp(Oxygen+(submerged?-22:35)*dt,0,100);
            if(submerged && Oxygen<=0) Health=Mathf.Max(0,Health-24*dt);
            if(Health<=0) GameManager.Instance.Finish(false);
        }
        public void Shake(float amount) { shake=amount; }
        public void Teleport(Vector3 position) { controller.enabled=false; transform.position=position; controller.enabled=true; velocity=planar=Vector3.zero; }
        public void SetSensitivity(float value) { sensitivity=value; PlayerPrefs.SetFloat("MouseSensitivity",value); }
    }
}
