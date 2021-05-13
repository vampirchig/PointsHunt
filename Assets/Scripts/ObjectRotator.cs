using UnityEngine;
using System.Collections;

public class ObjectRotator : MonoBehaviour
{
    void Update()
    {
        transform.Rotate(new Vector3(0, 45, 0) * Time.deltaTime);//задаем вращение объекта вокруг оси Y
    }
}