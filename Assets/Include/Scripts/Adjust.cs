using UnityEngine;

public class Adjust : MonoBehaviour{

    public void AdjustNow(){
        transform.localRotation = Quaternion.Euler(0.0f, transform.localRotation.eulerAngles.y, 0.0f);
        transform.localScale = Vector3.one;
    }
}
