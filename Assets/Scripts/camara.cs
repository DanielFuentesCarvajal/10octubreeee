using UnityEngine;

public class CamaraSeguir : MonoBehaviour
{
    public Transform personaje; // Arrastra tu personaje aquí en el Inspector
    public Vector3 offset; // Ajusta el desplazamiento de la cámara respecto al personaje

    void LateUpdate()
    {
        if (personaje != null)
        {
            transform.position = personaje.position + offset;
        }
    }
}
