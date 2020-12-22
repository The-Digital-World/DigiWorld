using UnityEngine;
using System.Linq;
using System.Collections;

namespace Pathfinding
{
    /// <summary>
    /// Sets the destination of an AI to the position of a specified object.
    /// This component should be attached to a GameObject together with a movement script such as AIPath, RichAI or AILerp.
    /// This component will then make the AI move towards the <see cref="target"/> set on this component.
    ///
    /// See: <see cref="Pathfinding.IAstarAI.destination"/>
    ///
    /// [Open online documentation to see images]
    /// </summary>
    [UniqueComponent(tag = "ai.destination")]
    [HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_a_i_destination_setter.php")]
    public class EnemyAI : VersionedMonoBehaviour
    {
        /// <summary>The object that the AI should move to</summary>
        public Transform target;
        public Transform enemeyTarget;
        IAstarAI ai;

        public Animator animator;
        Vector2 direction;

        private float waitTime;
        private float speed;
        public float startWaitTime;

        public Transform[] moveSpots;
        public Transform enemySpot;
        public Transform playerSpot;
        public Transform petSpot;

        private RectTransform battleBox;


        private int randomSpot;
        public State currentState; //move back to private

        private GameObject pet;
        private PetAI petScript;

        //Battle Variables
        public float health;
        public float attack;
        public float evasion;
        public float defence;
        public bool enemyReady { get; set; }
        public bool petReady { get; set; }

        public enum State
        {
            Patrol = 1,
            BattlePrepare = 2,
            Combat = 3,
            ResetCombat = 4
        }

        private void Start()
        {
            //animator = GetComponentInChildren<Animator>();
            //rigidbody = GetComponentInChildren<Rigidbody2D>();
            battleBox = GetComponentInChildren<RectTransform>();
            pet = GameObject.FindGameObjectsWithTag("Pet").FirstOrDefault();
            petScript = (PetAI)pet.GetComponent(typeof(PetAI));

            //Stop child objects from updating transforms
            foreach (Transform spot in moveSpots)
            {
                spot.SetParent(null, true);
            }

            battleBox.SetParent(null, true);
            enemySpot.SetParent(null, true);
            petSpot.SetParent(null, true);
            playerSpot.SetParent(null, true);

            waitTime = startWaitTime;
            randomSpot = Random.Range(0, moveSpots.Length);
            ai.destination = moveSpots[randomSpot].position;

            //Animation
            UpdateAnimations();
            animator.SetFloat("Speed", ai.destination.sqrMagnitude - this.transform.position.sqrMagnitude);

            currentState = State.Patrol;
            health = 100;

            petSpot.transform.position =
                new Vector3(Random.Range(battleBox.anchoredPosition.x, battleBox.anchoredPosition.x + battleBox.rect.width),
                            Random.Range(battleBox.anchoredPosition.y, battleBox.anchoredPosition.y + battleBox.rect.height), 0);

            enemySpot.transform.position =
                new Vector3(Random.Range(battleBox.anchoredPosition.x, battleBox.anchoredPosition.x + battleBox.rect.width),
                            Random.Range(battleBox.anchoredPosition.y, battleBox.anchoredPosition.y + battleBox.rect.height), 0);

        }

        void OnEnable()
        {
            ai = GetComponent<IAstarAI>();
            // Update the destination right before searching for a path as well.
            // This is enough in theory, but this script will also update the destination every
            // frame as the destination is used for debugging and may be used for other things by other
            // scripts as well. So it makes sense that it is up to date every frame.
            if (ai != null) ai.onSearchPath += Update;

        }

        void OnDisable()
        {
            if (ai != null) ai.onSearchPath -= Update;
        }

        /// <summary>Updates the AI's destination every frame</summary>
        void Update()
        {
            speed = ai.destination.sqrMagnitude - this.transform.position.sqrMagnitude;
            animator.SetFloat("Speed", (speed * speed));

            switch (currentState)
            {
                case State.Patrol:
                    Patrol();
                    break;
                case State.BattlePrepare:
                    BattlePrepare();
                    break;
                case State.Combat:
                    Combat();
                    break;
                case State.ResetCombat:
                    ResetCombat();
                    break;
                default:
                    break;
            }

            if (health <= 0)
                Destroy(this.gameObject);
        }

        private void UpdateAnimations()
        {
            animator.SetFloat("Horizontal", ai.destination.normalized.x - this.transform.position.normalized.x);
            animator.SetFloat("Vertical", ai.destination.normalized.y - this.transform.position.normalized.y);
        }

        public void UpdatePetSpot()
        {
            petSpot.transform.position =
                new Vector3(Random.Range(battleBox.anchoredPosition.x, battleBox.anchoredPosition.x + battleBox.rect.width),
                            Random.Range(battleBox.anchoredPosition.y, battleBox.anchoredPosition.y + battleBox.rect.height), 0);
        }

        private void Patrol()
        {
            if (target != null && ai != null) ai.destination = moveSpots[randomSpot].position;


            if (Vector2.Distance(transform.position, moveSpots[randomSpot].position) < 0.2f)
            {

                if (waitTime <= 0)
                {
                    randomSpot = Random.Range(0, moveSpots.Length);
                    waitTime = startWaitTime;
                    ai.destination = moveSpots[randomSpot].position;
                    UpdateAnimations();
                    
                }
                else
                {
                    waitTime -= Time.deltaTime;
                }
            }
        }

        private void BattlePrepare()
        {
            
            
            ai.destination = enemySpot.transform.position;
            UpdateAnimations();

            if (Vector2.Distance(transform.position, enemySpot.transform.position) < 0.2f)
            {
                
                enemyReady = true;
                petScript.enemyReady = true;

                if (petReady && enemyReady)
                {
                    //ai.destination = petScript.gameObject.transform.position;
                    //UpdateAnimations();
                    currentState = State.Combat;
                    
                }
                    
            }
        }

        private void Combat()
        {

            ai.destination = petScript.gameObject.transform.position;
            UpdateAnimations();

            if (Vector2.Distance(transform.position, petScript.gameObject.transform.position) < 0.2f)
            {

                enemySpot.transform.position =
                     new Vector3(Random.Range(battleBox.anchoredPosition.x, battleBox.anchoredPosition.x + battleBox.rect.width),
                            Random.Range(battleBox.anchoredPosition.y, battleBox.anchoredPosition.y + battleBox.rect.height), 0);

                ai.destination = enemySpot.transform.position;
                UpdateAnimations();
                petScript.TakeDamage(attack);
                currentState = State.ResetCombat;
            }


        }

        public void TakeDamage(float attackAmount)
        {
            health -= attackAmount;
        }

        private void ResetCombat()
        {

            if (Vector2.Distance(transform.position, enemySpot.transform.position) < 0.2f)
            {
                ai.destination = petScript.transform.position;
                UpdateAnimations();
                currentState = State.Combat;
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {

            switch (currentState)
            {
                case State.Patrol:
                    currentState = State.BattlePrepare;
                    petScript.SetEnemy(this.gameObject);
                    petScript.SetPetSpot(petSpot);
                    break;
                case State.BattlePrepare:
                    break;
                case State.Combat:
                    break;
                default:
                    break;
            }         



        }
    }
}
