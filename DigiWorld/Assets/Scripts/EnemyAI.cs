using UnityEngine;
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
        IAstarAI ai;

        private float waitTime;
        public float startWaitTime;

        public Transform[] moveSpots;
        public Transform battleSpot;
        private RectTransform battleBox;
        private int randomSpot;


        private void Start()
        {

            battleBox = GetComponentInChildren<RectTransform>();

            //Stop child objects from updating transforms
            foreach (Transform spot in moveSpots)
            {
                spot.SetParent(null, true);
            }
            battleBox.SetParent(null, true);
            battleSpot.SetParent(null, true);

            waitTime = startWaitTime;
            randomSpot = Random.Range(0, moveSpots.Length);
            ai.destination = moveSpots[randomSpot].position;

            
            
            battleSpot.transform.position = new Vector2(Random.Range(battleBox.rect.xMin, battleBox.rect.yMin),
                                                        Random.Range(battleBox.rect.yMin, battleBox.rect.yMax));


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

            if (target != null && ai != null) ai.destination = moveSpots[randomSpot].position;

            if (Vector2.Distance(transform.position, moveSpots[randomSpot].position) < 0.2f)
            {

                //move into battle state
                battleSpot.transform.position = 
                    new Vector3(Random.Range(battleBox.anchoredPosition.x, battleBox.anchoredPosition.x + battleBox.rect.width),
                                Random.Range(battleBox.anchoredPosition.y, battleBox.anchoredPosition.y + battleBox.rect.height), 0);



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
    }
}
