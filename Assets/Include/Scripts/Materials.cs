using UnityEngine;

public class Materials : MonoBehaviour{

    private Component[] children;

    public void changeChildrenMaterial(Material mat){
        children = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer mesh in children)
            mesh.material = mat;
    }
}
