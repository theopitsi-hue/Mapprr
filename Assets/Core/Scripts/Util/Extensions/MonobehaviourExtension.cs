using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UIElements.Experimental;

public static class MonobehaviourExtension
{
    //Creates a child gameobject with the specified component.
    public static T AddComponentAsObject<T>(this MonoBehaviour parent) where T : MonoBehaviour
    {
        GameObject o = new(typeof(T).Name);
        o.transform.parent = parent.transform;
        return o.AddComponent<T>();
    }

}