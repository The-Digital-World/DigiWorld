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

        private State currentState;

        //Battle variables
        public bool enemyReady { get; set; }
        public bool petReady { get; set; }

        private enum State
        {
            Following = 1,
            BattlePrepare = 2,
            Combat = 3
        }

        private void Start()
        {
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
                    break;
                default:
                    break;
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


            if (Vector2.Distance(transform.position, petSpot.position) < 0.2f)
            {
                petReady = true;
                enemyScript.petReady = true;

                if (petReady && enemyReady)
                    currentState = State.Combat;

            }

        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            
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
