using UnityEngine;
using UnityEngine.UI;
using static Global;

public class GlobalResource : MonoBehaviour
{
    public GameObject iceObj, holeObj, singleQuad, doubleQuad, diagQuad;
    public Material boxMat, quadWoodMat, quadIronMat, quadBlackMat, quadSpriteMat;
    public PhysicMaterial quadPhyMat;
    public Sprite soundOn, soundOff;
    private void Awake()
    {
        GR = this;
    }
}