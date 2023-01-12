using UnityEngine;
using static Global;

public class QuadOut : MonoBehaviour
{
    public void Init(Vector3 vx)
    {
        GetComponent<Collider>().material = GR.quadPhyMat;

        Debug.Assert(gameObject.GetComponent<Rigidbody>() == null);
        var rigidBody = gameObject.AddComponent<Rigidbody>();
        rigidBody.velocity = vx;
    }

    private void OnDestroy()
    {
        Destroy(GetComponent<Rigidbody>());
        GetComponent<Collider>().material = null;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.name != "default")
            A.QuadGround(Mathf.Min(collision.impulse.magnitude, 1f));
    }
}
