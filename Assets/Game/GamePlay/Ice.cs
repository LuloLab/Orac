using UnityEngine;

public class Ice : MonoBehaviour
{
    public int id, pos;
    const float shrinkSpeed = 5;
    float size = 1;
    bool shrinking = false;
    public void Init()
    {
        size = 1;
        shrinking = false;
        transform.localScale = Vector3.one;
    }
    public void Shrink() => shrinking = true;
    void Update()
    {
        if (!shrinking || size == 0)
            return;
        if ((size -= Time.deltaTime * shrinkSpeed) < 0)
            size = 0;
        transform.localScale = size * Vector3.one;
    }
}
