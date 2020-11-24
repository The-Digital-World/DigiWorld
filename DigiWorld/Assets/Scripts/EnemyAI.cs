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

        private float waitTime;
        public float startWaitTime;

        public Transform[] moveSpots;
        public Transform enemySpot;
        public Transform playerSpot;
        public Transform petSpot;

        private RectTransform battleBox;


        private int randomSpot;
        private State currentState;

        private GameObject pet;
        private PetAI petScript;

        //Battle Variables
        public bool enemyReady { get; set; }
        public bool petReady { get; set; }

        private enum State
        {
            Patrol = 1,
            BattlePrepare = 2,
            Combat = 3
        }

        private void Start()
        {

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

            currentState = State.Patrol;

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
                default:
                    break;
            }

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

            if (Vector2.Distance(transform.position, enemySpot.transform.position) < 0.2f)
            {
                enemyReady = true;
                petScript.enemyReady = true;

                if (petReady && enemyReady)
                    currentState = State.Combat;
            }
        }

        private void Combat()
        {



        }

        private void OnCollisionEnter2D(Collision2D collision)
        {

            currentState = State.BattlePrepare;
            petScript.SetEnemy(this.gameObject);
            petScript.SetPetSpot(petSpot);


        }
    }
}
