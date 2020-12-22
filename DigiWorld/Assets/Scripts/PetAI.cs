using UnityEngine;
using System.Linq;
using System.Collections;

namespace Pathfinding
{

    [UniqueComponent(tag = "ai.destination")]
    [HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_a_i_destination_setter.php")]
    public class PetAI : VersionedMonoBehaviour
    {
        /// <summary>The object that the AI should move to</summary>
        private Transform target;
        private Transform petSpot;
        public GameObject enemyTarget;
        private AIPath petAiPath;
        private EnemyAI enemyScript;
        IAstarAI ai;

        public State currentState;

        //Battle variables

        public float health;
        public float attack;
        public float evasion;
        public float defence;

        public bool enemyReady { get; set; }
        public bool petReady { get; set; }

        public enum State
        {
            Following = 1,
            BattlePrepare = 2,
            Combat = 3,
            ResetCombat = 4
        }

        private void Start()
        {
            health = 100;
            target = GameObject.FindGameObjectWithTag("Player").transform;
            petAiPath = (AIPath)GetComponent(typeof(AIPath));            
            currentState = State.Following;

        }

        void OnEnable()
        {
            ai = GetComponent<IAstarAI>();
            if (ai != null) ai.onSearchPath += Update;

        }

        void OnDisable()
        {
            if (ai != null) ai.onSearchPath -= Update;
        }

        /// <summary>Updates the AI's destination every frame</summary>
        void Update()
        {
            switch (currentState)
            {
                case State.Following:
                    Following();
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
            
            if(Input.GetKeyDown("space"))
            {
                Debug.Log(enemyScript.transform.position);
            }

            if (enemyScript != null)
            {
                if(enemyScript.health <= 0)
                    currentState = State.Following;
            }
               
        }


        private void Following()
        {
            if (target != null && ai != null) ai.destination = target.position;
            petAiPath.endReachedDistance = 1.91f;
        }

        private void BattlePrepare()
        {
            ai.destination = petSpot.position;
            //UpdateAnimations

            if (Vector2.Distance(transform.position, petSpot.position) < 0.2f)
            {
                petReady = true;
                enemyScript.petReady = true;

                if (petReady && enemyReady)
                {
                    //enemyScript.UpdatePetSpot();
                    //ai.destination = enemyScript.transform.position;
                    currentState = State.Combat;
                }
                    

            }

        }

        private void ResetCombat()
        {
            if (Vector2.Distance(transform.position, enemyScript.petSpot.transform.position) < 0.2f)
            {
                ai.destination = enemyScript.transform.position;
                //UpdateAnimations
                currentState = State.Combat;
            }
        }

        private void Combat()
        {

            ai.destination = enemyScript.transform.position;
            //UpdateAnimations

            if (Vector2.Distance(transform.position, enemyScript.gameObject.transform.position) < 0.2f)
            {
                enemyScript.UpdatePetSpot();
                ai.destination = petSpot.transform.position;
                //UpdateAnimations
                enemyScript.TakeDamage(attack);
                currentState = State.ResetCombat;
            }
        }

        public void TakeDamage(float attackAmount)
        {
            //increase logic here
            health -= attackAmount;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            switch (currentState)
            {
                case State.Following:
                    break;
                case State.BattlePrepare:
                    break;
                case State.Combat:
                    break;
                default:
                    break;
            }
        }

        public void SetEnemy(GameObject enemy)
        {
            enemyTarget = enemy.gameObject;
            enemyScript = (EnemyAI)enemyTarget.GetComponent(typeof(EnemyAI));
            currentState = State.BattlePrepare;
            petAiPath.endReachedDistance = 0.2f;
        }

        public void SetPetSpot(Transform ps)
        {
            petSpot = ps;
        }
    }
}
