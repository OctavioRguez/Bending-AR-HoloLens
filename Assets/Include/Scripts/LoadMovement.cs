using UnityEngine;

public class LoadMovement : MonoBehaviour{

    [SerializeField] private GameObject beam;

    private float leftLimit;
    private float rightLimit;

    void Start(){
        leftLimit = beam.transform.GetChild(0).transform.localPosition.x;
        rightLimit = beam.transform.GetChild(beam.transform.childCount - 1).transform.localPosition.x;
    }

    void Update(){
        if (transform.localPosition.x <= leftLimit){
            transform.localPosition = new Vector3(leftLimit, transform.localPosition.y, transform.localPosition.z);
        }
        else if (transform.localPosition.x >= rightLimit){
            transform.localPosition = new Vector3(rightLimit, transform.localPosition.y, transform.localPosition.z);
        }
    }
}
