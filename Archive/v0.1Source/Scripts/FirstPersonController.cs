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
        public bool LampOn => flashlight && flashlight.enabled;
        CharacterController controller;
        Vector3 velocity, planar;
        float pitch, bob, shake;
        InputAction move, look, sprint, jump, interact, bag, pause, lamp, medicine;
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
            sensitivity=PlayerPrefs.GetFloat("MouseSensitivity",.12f);
        }
        void OnEnable() { foreach(var a in Actions()) a?.Enable(); }
        void OnDisable() { foreach(var a in Actions()) a?.Disable(); }
        InputAction[] Actions() => new[]{move,look,sprint,jump,interact,bag,pause,lamp,medicine};
        void OnDestroy() { foreach(var a in Actions()) a?.Dispose(); }
        void Update()
        {
            var game=GameManager.Instance; if(!game) return;
            if(pause.WasPressedThisFrame()) game.Escape();
            if(game.IsPlaying && bag.WasPressedThisFrame()) game.ToggleInventory();
            if(!AcceptsInput) { FocusedItem=null; return; }
            var delta=look.ReadValue<Vector2>()*sensitivity;
            transform.Rotate(0,delta.x,0); pitch=Mathf.Clamp(pitch-delta.y,-82,82);
            Vector2 input=move.ReadValue<Vector2>();
            float speed=sprint.IsPressed()?sprintSpeed:walkSpeed;
            if(game.flood.IsFlooding && game.flood.Level>transform.position.y+.5f) speed*=.53f;
            Vector3 desired=(transform.right*input.x+transform.forward*input.y)*speed;
            planar=Vector3.Lerp(planar,desired,1-Mathf.Exp(-13*Time.deltaTime));
            if(controller.isGrounded && velocity.y<0) velocity.y=-2;
            if(controller.isGrounded && jump.WasPressedThisFrame()) velocity.y=6;
            velocity.y-=19*Time.deltaTime;
            controller.Move((planar+velocity)*Time.deltaTime);
            bob+=Time.deltaTime*planar.magnitude*1.9f;
            shake=Mathf.MoveTowards(shake,0,Time.deltaTime*.7f);
            view.transform.localRotation=Quaternion.Euler(pitch+Mathf.Sin(Time.time*37)*shake,0,Mathf.Cos(Time.time*43)*shake*.5f);
            view.transform.localPosition=new Vector3(0,1.64f+(controller.isGrounded?Mathf.Sin(bob)*.022f:0),0);
            FocusedItem=null;
            if(Physics.Raycast(view.transform.position,view.transform.forward,out var hit,3.1f,~(1<<2),QueryTriggerInteraction.Ignore)) FocusedItem=hit.collider.GetComponentInParent<PickupItem>();
            if(FocusedItem && interact.WasPressedThisFrame() && FocusedItem.Collect(inventory)) { game.audioSystem.Pickup(); game.Announce("SUPPLY SECURED",FocusedItem.definition.displayName+" added to your bag.",2); FocusedItem=null; }
            if(lamp.WasPressedThisFrame()) ToggleFlashlight();
            if(medicine.WasPressedThisFrame()) UseMedicine();
        }
        public void ToggleFlashlight() { if(inventory.Count(ItemKind.Flashlight)>0) flashlight.enabled=!flashlight.enabled; }
        public void UseMedicine() { if(Health<100 && inventory.Consume(ItemKind.Medicine)) Health=Mathf.Min(100,Health+40); }
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
