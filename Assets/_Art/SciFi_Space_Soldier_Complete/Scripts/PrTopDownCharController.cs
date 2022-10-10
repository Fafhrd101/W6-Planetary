using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Animator))]

public class PrTopDownCharController : MonoBehaviour {
	
    //Inputs
    //[HideInInspector]
    // public string[] playerCtrlMap = {"Yaw", "Pitch", "LookX", "LookY","FireTrigger", "Reload",
    //     "EquipWeapon", "Sprint", "Aim", "ChangeWTrigger", "Roll", "Use", "Crouch", "ChangeWeapon", "Throw"  ,"Fire", "Mouse ScrollWheel"};

    // Hiding most variables, eventually I want to yank them out.
    [Header("Movement")]
    [Range(1f, 4f)] [SerializeField] float m_GravityMultiplier = 2f;
    [HideInInspector]
    [SerializeField] float m_MoveSpeedMultiplier = 1f;
    [HideInInspector]
    public float m_MoveSpeedSpecialModifier = 1f;
    [HideInInspector]
    [SerializeField] float m_AnimSpeedMultiplier = 1f;
    private float _mGroundCheckDistance = 0.25f;

    public bool useRootMotion = true;
    [HideInInspector]
    public float PlayerRunSpeed = 1f;
    [HideInInspector]
    public float PlayerAimSpeed = 1f;
    [HideInInspector]
    public float PlayerSprintSpeed = 1f;
    [HideInInspector]
    public float RunRotationSpeed = 100f;
    [HideInInspector]
    public float AimingRotationSpeed = 25f;
    [HideInInspector]
    public float AnimatorRunDampValue = 0.25f;
    [HideInInspector]
    public float AnimatorSprintDampValue = 0.2f;
    [HideInInspector]
    public float AnimatorAimingDampValue = 0.1f;

    private Rigidbody _mRigidbody;
    private Animator _charAnimator;
    private bool _mIsGrounded;
    private float _mOrigGroundCheckDistance;
    private const float KHalf = 0.5f;
    private float _mTurnAmount;
    private float _mForwardAmount;
    private float _mCapsuleHeight;
    private Vector3 _mCapsuleCenter;
    private CapsuleCollider _mCapsule;

    private bool b_CanRotate = true;
    [HideInInspector] public bool Sprinting = false;

    [HideInInspector] public bool m_isDead = false;
    [HideInInspector] public bool m_CanMove = true;

    [Header("Aiming")]
    public GameObject AimTargetVisual;
    public Transform AimFinalPos;
    public PrTopDownCamera CamScript;
    [SerializeField]
    [Tooltip("An object spawned at the click location by the player")]
    private GameObject pingPrefab = null;
    [SerializeField]
    [Tooltip("Which layers are tested by the click raycast")]
    private LayerMask rayLayerMask = -1;
    
    private Transform m_Cam;                  // A reference to the main camera in the scenes transform
    [HideInInspector]
    public Vector3 m_Move;					  // the world-relative desired move direction, calculated from the camForward and user input.
    private Vector3 smoothMove;

    private PrTopDownCharInventory Inventory;

    public List<GameObject> friends;

    void Start()
    {
        Inventory = GetComponent<PrTopDownCharInventory>();

        // get the transform of the main camera
        if (Camera.main != null)
        {
            m_Cam = CamScript.transform.GetComponentInChildren<Camera>().transform;
        }
        else
        {
            Debug.LogWarning(
                "Warning: no main camera found. Third person character needs a Camera tagged \"MainCamera\", for camera-relative controls.");
            // we use self-relative controls in this case, which probably isn't what the user wants, but hey, we warned them!
        }

        _charAnimator = GetComponent<Animator>();
        _mRigidbody = GetComponent<Rigidbody>();
        _mCapsule = GetComponent<CapsuleCollider>();
        _mCapsuleHeight = _mCapsule.height;
        _mCapsuleCenter = _mCapsule.center;

        _mRigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        _mOrigGroundCheckDistance = _mGroundCheckDistance;
    }

    public void StopMoving(string Case)
    {
        if (Case == "GameOver")
        {
            m_CanMove = false;
            b_CanRotate = false;

            _charAnimator.SetTrigger("GameOver");
            Inventory.isDead = true;
        }
    }

    public void LoadPlayerInfo()
    {
        //Debug.Log("player Info found - Loading Info");
        Inventory.health = PrPlayerInfo.player1.health;
        Inventory.actualHealth = PrPlayerInfo.player1.actualHealth;
        for (int i = 0; i < PrPlayerInfo.player1.weapons.Length; i++)
        {
            Inventory.InitialWeapons[i] = Inventory.WeaponListObject.weapons[PrPlayerInfo.player1.weapons[i]].GetComponent<PrWeapon>();
            Inventory.grenadesCount = PrPlayerInfo.player1.grenades;
        }
    }

    public void SavePlayerInfo()
    {
        //Debug.Log("Saving Player Info");

        PrPlayerInfo playerI = PrPlayerInfo.player1.GetComponent<PrPlayerInfo>();
        playerI.playerName = Inventory.name;
        playerI.health = Inventory.health;
        playerI.actualHealth = Inventory.actualHealth;
        playerI.maxWeaponCount = Inventory.playerWeaponLimit;
        playerI.weapons = new int[Inventory.playerWeaponLimit];
        playerI.grenades = Inventory.grenadesCount;

        for (int i = 0; i < Inventory.playerWeaponLimit; i++)
        {
            //Debug.Log("Weapon " + i + " is " + Inventory.Weapon[i] + " And the Name is " + Inventory.Weapon[i].GetComponent<PrWeapon>().WeaponName + " And the bullets are " + Inventory.Weapon[i].GetComponent<PrWeapon>().ActualBullets);
            playerI.weapons[i] = Inventory.actualWeaponTypes[i];
        }
    }
    

    public void CreatePlayerInfo()
    {
        //Create Player info to be able to save player stats during gameplay
      
        //Debug.Log("player Info NOT found - Saving Info");

        GameObject playerInfo = new GameObject("playerInfo_");
        playerInfo.AddComponent<PrPlayerInfo>();
        PrPlayerInfo playerI = playerInfo.GetComponent<PrPlayerInfo>();
        playerI.playerName = Inventory.name;
        playerI.health = Inventory.health;
        playerI.actualHealth = Inventory.actualHealth;
        playerI.maxWeaponCount = Inventory.playerWeaponLimit;
        playerI.weapons = new int[Inventory.playerWeaponLimit];

        for (int i =0; i < Inventory.playerWeaponLimit; i++)
        {
            //Debug.Log("Weapon " + i + " is " + Inventory.Weapon[i] + " And the Name is " + Inventory.Weapon[i].GetComponent<PrWeapon>().WeaponName + " And the bullets are " + Inventory.Weapon[i].GetComponent<PrWeapon>().ActualBullets);
            playerI.weapons[i] = Inventory.actualWeaponTypes[i];
        }
        
    }
    
    
    public void CantRotate()
    {
        b_CanRotate = false;
    }
    
    private void Update ()
    {
	    if (Player.Instance.inputDisabled)
		    return;
	    
        MouseTargetPos();

        if (!m_isDead && m_CanMove)
		{
            float h = Input.GetAxis("Yaw");
            float v = Input.GetAxis("Pitch");

            if (b_CanRotate)
            {
                 if (Inventory.aiming)
	                 MouseAim(AimFinalPos.position);
                 else
                    RunningLook(new Vector3(h, 0, v));
            }


            m_Move = new Vector3(h, 0, v);
            

            m_Move = m_Move.normalized * m_MoveSpeedSpecialModifier;
            //Rotate move in camera space
            m_Move = Quaternion.Euler(0, 0 - transform.eulerAngles.y + m_Cam.transform.parent.transform.eulerAngles.y, 0) * m_Move;

            //Move Player
            Move(m_Move);

            //Sprint
            if (Input.GetKeyDown(KeyCode.LeftControl) && m_Move.magnitude >= 0.2f && !Inventory.usingObject)
            {
                if (Inventory.actualStamina > 0.0f)
                {
                    Sprinting = true;
                }
                else
                {
                    Sprinting = false;
                }
            }
            else
            {
                Sprinting = false;
            }

            Inventory.usingStamina = Sprinting;
            
        }
        else
        {
            _mForwardAmount = 0.0f;
            _mTurnAmount = 0.0f;
            Inventory.aiming = false;
            UpdateAnimator(Vector3.zero);
        }
        
    }
	
    private void RunningLook(Vector3 Direction)
    {
        if (Direction.magnitude >= 0.25f)
        {
            Direction = Quaternion.Euler(0, 0 + m_Cam.transform.parent.transform.eulerAngles.y, 0) * Direction;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(Direction), Time.deltaTime * (RunRotationSpeed * 0.1f));
            transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);
        }
       
    }

    private void MouseTargetPos()
	{
		if (Camera.main is not null)
		{
			var cursorRay = Camera.main.ScreenPointToRay(Input.mousePosition);
			const float maxDistance = 100f;
			if (Physics.Raycast(cursorRay, out var hit, maxDistance, rayLayerMask, QueryTriggerInteraction.Ignore))
			{
				AimTargetVisual.transform.position = hit.point;
				AimTargetVisual.transform.LookAt(transform.position);
				if (Input.GetMouseButton(0))
					Instantiate(pingPrefab, hit.point, Quaternion.identity);
			}
		}
	}
	
    private void MouseAim(Vector3 FinalPos)
    {
	    transform.LookAt(FinalPos);
	    //transform.rotation = Quaternion.Lerp(transform.rotation, JoystickLookRot.transform.rotation, Time.deltaTime * AimingRotationSpeed);
        transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);
    }

    private void Move(Vector3 move)
	{
        if (!Inventory.usingObject)
        {
            CheckGroundStatus();

            _mTurnAmount = move.x;
            _mForwardAmount = move.z;

            // control and velocity handling is different when grounded and airborne:
            if (_mIsGrounded)
            {
                HandleGroundedMovement(/*crouch, jump*/);
            }
            else
            {
                HandleAirborneMovement();
            }

            // send input and other state parameters to the animator
            UpdateAnimator(move);
        }
		
	}
    
	public void UpdateAnimator(Vector3 move)
	{
		//return;
         // update the animator parameters
        _charAnimator.SetFloat("Y", _mForwardAmount, AnimatorAimingDampValue, Time.deltaTime);
		_charAnimator.SetFloat("X", _mTurnAmount, AnimatorAimingDampValue, Time.deltaTime);
                
        if (!Sprinting)
            _charAnimator.SetFloat("Speed", move.magnitude, AnimatorSprintDampValue, Time.deltaTime);
        else
            _charAnimator.SetFloat("Speed", 2.0f, AnimatorRunDampValue, Time.deltaTime);
        
		_charAnimator.SetBool("OnGround", _mIsGrounded);
			
		// the anim speed multiplier allows the overall speed of walking/running to be tweaked in the inspector,
		// which affects the movement speed because of the root motion.
		if (_mIsGrounded && move.magnitude > 0)
		{
            
            if (Inventory.aiming)
            {
                move *= PlayerAimSpeed;
                transform.Translate(move * Time.deltaTime);
                _charAnimator.applyRootMotion = false;
            }
            else if (Inventory.usingObject)
            {
                move = move * 0.0f;
                transform.Translate(Vector3.zero);
                _charAnimator.applyRootMotion = false;
            }
            else
            {
                if (useRootMotion)
                    _charAnimator.applyRootMotion = true;
                else
                {
 
                    if (Sprinting)
                        move *= PlayerSprintSpeed;
                    else
                        move *= PlayerRunSpeed;

                    transform.Translate(move * Time.deltaTime );
                    _charAnimator.applyRootMotion = false;
                }
            }

            _charAnimator.speed = m_AnimSpeedMultiplier ;
		}
		else
		{
			// don't use that while airborne
			_charAnimator.speed = 1;
		}
	}
	
	
	void HandleAirborneMovement()
	{
		// apply extra gravity from multiplier:
		Vector3 extraGravityForce = (Physics.gravity * m_GravityMultiplier) - Physics.gravity;
		_mRigidbody.AddForce(extraGravityForce);
		
		_mGroundCheckDistance = _mRigidbody.velocity.y < 0 ? _mOrigGroundCheckDistance : 0.01f;
	}
	
	
	void HandleGroundedMovement(/*bool crouch, bool jump*/)
	{
		// check whether conditions are right to allow a jump:
    }
		
	//This function it´s used only for Aiming and Jumping states. Those anims don´t have root motion so we move the player by script
	public void OnAnimatorMove()
	{
		// we implement this function to override the default root motion.
		// this allows us to modify the positional speed before it's applied.
		/*if (_mIsGrounded && Time.deltaTime > 0)
		{
			Vector3 v = (_charAnimator.deltaPosition * m_MoveSpeedMultiplier ) / Time.deltaTime;
			
			// we preserve the existing y part of the current velocity.
			v.y = _mRigidbody.velocity.y;
			_mRigidbody.velocity = v;
		}*/
	}

	void CheckGroundStatus()
	{
		RaycastHit hitInfo;
		#if UNITY_EDITOR
		// helper to visualise the ground check ray in the scene view
		//Debug.DrawLine(transform.position + (Vector3.up * 0.1f), transform.position + (Vector3.up * 0.1f) + (Vector3.down * m_GroundCheckDistance));
		#endif
		// 0.1f is a small offset to start the ray from inside the character
		// it is also good to note that the transform position in the sample assets is at the base of the character
		if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hitInfo, _mGroundCheckDistance))
		{
			_mIsGrounded = true;
			_charAnimator.applyRootMotion = true;
		}
		else
		{
			_mIsGrounded = false;
			_charAnimator.applyRootMotion = false;
		}
	}
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EnvZone"))
        {
            if (CamScript && other.GetComponent<PrEnvironmentZone>() != null)
                CamScript.TargetHeight = other.GetComponent<PrEnvironmentZone>().CameraHeight;
        }
        
    }
}
