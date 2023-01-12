
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Global;
using static Quad;

public class Game : MonoBehaviour
{
    // Resources
    public string[] patternNames;

    // Constants
    public float quadOutSpeed, quadColorExp;
    private readonly Color[] quadColors =
    {
        Color.red,
        Color.green,
        Color.blue,
        Color.cyan,
        Color.magenta,
        Color.yellow,
        new Color(1, 0.1f, 0),
        new Color(1, 0, 0.1f),
        new Color(0.1f, 1, 0),
        new Color(0, 1, 0.1f),
        new Color(0.1f, 0, 1),
        new Color(0, 0.1f, 1),
    };
    private readonly Color offTargColor = new(0.02f, 0.02f, 0.02f);
    private const int Empty = 0, Wall = 1, Hole = 2;
    private const int CanMove = 0, HitWall = 1, StuckHole = 2, 
        HitIce = 3, WoodHitWood = 4, WoodHitIron = 5,
        IronHitWood = 6, IronHitIron = 7, GetOut = 8;

    // Level Info
    public int chapter, level;
    [HideInInspector] public LevelInfo info;
    [HideInInspector] public bool isNextButtonActive;
    private bool IsOpenScene => info.env == 3;

    // Game Variables
    private int nMove, nQuadOut, nextPosCache;
    private int[] groundObjs, surfaceObjs, targetObjs;
    private readonly List<Quad> quads = new();
    private readonly List<Ice> ices = new();
    private readonly Stack<int> history = new();
    private Transform box;


    #region Enter
    private void Awake()
    {
        G = this;
    }
    public void NewGame()
    {
        M.gamePlayUI.Init();
        isNextButtonActive = false;
        info = JsonUtility.FromJson<LevelInfo>(
            Resources.Load<TextAsset>($"Level{chapter}-{level:d2}").text);


        #region Basic Setup
        groundObjs = new int[info.xx * info.yy];
        surfaceObjs = new int[info.xx * info.yy];
        targetObjs = new int[info.xx * info.yy];
        for (int i = 0; i < info.xx; i++)
        {
            for (int j = 0; j < info.yy; j++)
            {
                groundObjs[i + j * info.xx] = Empty;
                if (info.env == 1 
                    && (i == 0 || i == info.xx - 1 || j == 0 || j == info.yy - 1)
                    || info.env == 2)
                    groundObjs[i + j * info.xx] = Wall;
            }
        }
        #endregion


        #region Model Setup
        box = Instantiate(Resources.Load<GameObject>($"box-{info.modelIdx:d2}")).transform;
        box.localPosition = 0.2f * Vector3.up;
        Vector2 boxRange = new Vector4(IsOpenScene ? info.xx : info.xx - 2,
            IsOpenScene ? info.yy : info.yy - 2);
        if ((info.modelBias & 1) == 1)
        {
            box.position += 0.42f * Vector3.left;
            box.GetChild(0).transform.localPosition += 0.5f * Vector3.right;
            --boxRange.x;
        }
        if (((info.modelBias >> 1) & 1) == 1)
        {
            box.position += 0.42f * Vector3.right;
            box.GetChild(0).transform.localPosition += 0.5f * Vector3.left;
            --boxRange.x;
        }
        if (((info.modelBias >> 2) & 1) == 1)
        {
            box.position += 0.42f * Vector3.back;
            box.GetChild(0).transform.localPosition += 0.5f * Vector3.forward;
            --boxRange.y;
        }
        if (((info.modelBias >> 3) & 1) == 1)
        {
            box.position += 0.42f * Vector3.forward;
            box.GetChild(0).transform.localPosition += 0.5f * Vector3.back;
            --boxRange.y;
        }
        box.localScale = (0.5f + 2.0f / boxRange.y) * Vector3.one;

        var boxMat = new Material(GR.boxMat);
        boxMat.SetVector("Bound", boxRange);
        var boxObj = box.GetChild(0).gameObject;
        boxObj.GetComponent<MeshRenderer>().material = boxMat;

        var boxCollider = boxObj.AddComponent<BoxCollider>();
        boxCollider.center = new Vector3(0, -0.08f, 0);
        boxCollider.size = new Vector3(boxRange.x, 0.16f, boxRange.y);
        boxCollider.material = GR.quadPhyMat;
        #endregion


        #region Read Content
        var decalScr = new RandomSequence(patternNames.Length);
        var colorScr = new RandomSequence(quadColors.Length);
        int decalIdx = -1, thePattern = 0;
        Color quadColor = default;
        foreach (string s in info.content.Split(' '))
        {
            if (s[0] == 'q' || s[0] == 'Q')
            {
                if (s[^1] != '*')
                {
                    ++thePattern;
                    decalIdx = decalScr.Get();
                    quadColor = quadColorExp * quadColors[colorScr.Get()];
                }

                bool hasTarget = s.Length >= 5;
                char quadShape = @"-|\/".Contains(s[^1]) ? s[^1] : '.';
                var g = Instantiate(quadShape switch
                {
                    '.' => GR.singleQuad,
                    '-' => GR.doubleQuad,
                    '|' => GR.doubleQuad,
                    _   => GR.diagQuad,
                }, box);
                g.name = patternNames[decalIdx];

                var quad = g.AddComponent<Quad>();
                quad.id = quads.Count;
                quad.pattern = thePattern;
                quad.hasTarget = hasTarget;
                quad.iron = s[0] == 'Q';
                if (quad.iron && quadShape != '.')
                    Debug.LogError("Invalid iron quad");
                if (quad.iron)
                    g.GetComponent<MeshRenderer>().material = GR.quadIronMat;
                quad.atHome = true;
                quad.color = quadColor;
                quad.shape = quadShape;
                quad.initFirstPos = ToNum(s[1]) + ToNum(s[2]) * info.xx;
                quad.subquads = quadShape switch
                {
                    '.' => new SubQuad[1] {
                        new SubQuad { pos = quad.initFirstPos }
                    },
                    '-' => new SubQuad[2] {
                        new SubQuad { pos = quad.initFirstPos },
                        new SubQuad { pos = quad.initFirstPos + 1 }
                    },
                    '|' => new SubQuad[2] {
                        new SubQuad { pos = quad.initFirstPos },
                        new SubQuad { pos = quad.initFirstPos + info.xx }
                    },
                    '\\' => new SubQuad[2] {
                        new SubQuad { pos = quad.initFirstPos },
                        new SubQuad { pos = quad.initFirstPos - info.xx + 1 }
                    },
                    _ => new SubQuad[2] {
                        new SubQuad { pos = quad.initFirstPos },
                        new SubQuad { pos = quad.initFirstPos + info.xx + 1 }
                    },
                };
                quad.transform.rotation = quad.InitRotation;
                if (hasTarget)
                {
                    int targPos = ToNum(s[3]) + ToNum(s[4]) * info.xx;
                    Sprite sprite = Resources.Load<Sprite>($"targ{g.name}");
                    for (int i = 0; i < quad.subquads.Length; i++)
                    {
                        // target
                        int thisPos = targPos - quad.initFirstPos + quad.subquads[i].pos;
                        var targ = new GameObject($"targ{g.name}");
                        targ.transform.SetParent(box);
                        targ.transform.localPosition = GetPos(thisPos, 0.001f);
                        targ.transform.rotation = Quaternion.Euler(90, 0, 0);
                        targ.transform.localScale = new Vector3(0.75f, 0.75f, 1);
                        var targSpriteRenderer = targ.AddComponent<SpriteRenderer>();
                        targSpriteRenderer.sprite = sprite;
                        targetObjs[thisPos] = quad.pattern;

                        // decal
                        quad.subquads[i].decalMat = quad.GetComponent<MeshRenderer>().materials[i + 1];

                        // sprite
                        var quadSprite = new GameObject("sprite");
                        quadSprite.transform.SetParent(quad.transform);
                        quadSprite.transform.localPosition = new(
                            quad.subquads[i].pos % info.xx - quad.initFirstPos % info.xx,
                            0.201f,
                            quad.subquads[i].pos / info.xx - quad.initFirstPos / info.xx);
                        quadSprite.transform.rotation = Quaternion.Euler(90, 0, 0);
                        quadSprite.transform.localScale = new Vector3(0.5f, 0.5f, 1);
                        quad.subquads[i].sprite = quadSprite.AddComponent<SpriteRenderer>();
                        quad.subquads[i].sprite.material = new Material(GR.quadSpriteMat);
                        quad.subquads[i].sprite.color = Color.white;
                        quad.subquads[i].sprite.sprite = sprite;
                    }
                }
                quads.Add(quad);
            }
            else if (s[0] == 'w' || s[0] == 'g')
            {
                var obj = s[0] == 'w' ? Wall : Empty;
                if (s.Length == 3)
                    groundObjs[ToNum(s[2]) * info.xx + ToNum(s[1])] = obj;
                else
                {
                    int x1 = ToNum(s[1]), x2 = ToNum(s[2]),
                        y1 = ToNum(s[3]), y2 = ToNum(s[4]);
                    for (int i = x1; i <= x2; i++)
                        for (int j = y1; j <= y2; j++)
                            groundObjs[j * info.xx + i] = (groundObjs[j * info.xx + i] >> 8) << 8 | obj;
                }
            }
            else if (s[0] == 'h')
            {
                int pos = ToNum(s[2]) * info.xx + ToNum(s[1]);
                groundObjs[pos] = Hole;
                var hole = Instantiate(GR.holeObj, box).transform;
                hole.localPosition = GetPos(pos, 0);
            }
            else if (s[0] == 'i')
            {
                int pos = ToNum(s[2]) * info.xx + ToNum(s[1]);
                var g = Instantiate(GR.iceObj, box);
                g.transform.localPosition = GetPos(pos, 0);
                g.transform.localRotation = Quaternion.identity;
                g.transform.localScale = Vector3.one;
                var ice = g.AddComponent<Ice>();
                ice.id = ices.Count;
                ice.pos = pos;
                ices.Add(ice);
            }
        }
        #endregion


        Restart();
    }
    #endregion


    #region Moving
    public void InputMove(Quad quad, Vector3 v) => InputMove(quad, ParseDirection(v));
    public void InputMove(Quad quad, int dir)
    {
        if (!GLOBAL_INTERACTIVE || !quad.moveable)
            return;
        SetQuadToArray(quad, false);
        if (GetCode(quad, dir) is CanMove or GetOut)
        {
            M.gamePlayUI.SetMove(++nMove);
            history.Push(0);
            StartMove(quad, dir, true);
        }
        else
        {
            SetQuadToArray(quad, true);
        }
    }
    private int GetSubCode(int pos, int dir)
    {
        if (IsGettingOut(pos, dir)) return GetOut;
        nextPosCache = NextPos(pos, dir);
        if ((groundObjs[pos] & 255) != Empty)
            return StuckHole;
        if ((groundObjs[nextPosCache] & 255) == Wall)
            return HitWall;
        if (surfaceObjs[nextPosCache] != 0)
        {
            if ((surfaceObjs[nextPosCache] >> 8) == 1)
            {
                return quads[surfaceObjs[nextPosCache] & 0xff].iron ? WoodHitIron : WoodHitWood;
            }
            else return HitIce;
        }
        return CanMove;
    }
    private int GetCode(Quad quad, int dir)
    {
        foreach (var subquad in quad.subquads)
        {
            int code = GetSubCode(subquad.pos, dir);
            if (code == GetOut) return GetOut;
            else if (code != CanMove)
            {
                if (quad.iron)
                {
                    if (code == WoodHitWood)
                        return IronHitWood;
                    if (code == WoodHitIron)
                        return IronHitIron;
                }
                return code;
            }
        }
        return CanMove;
    }
    private int Simulate(Quad quad, int dir, out int step)
    {
        step = 0;
        while (true)
        {
            ++step;
            for (int i = 0; i < quad.subquads.Length; ++i)
                quad.subquads[i].pos = NextPos(quad.subquads[i].pos, dir);
            int code = GetCode(quad, dir);
            if (code != CanMove)
                return code;
        }
    }
    private void StartMove(Quad quad, int dir, bool powerful)
    {
        GLOBAL_INTERACTIVE = false;
        history.Push(1 << 20 | quad.id << 16 | quad.FirstPos);
        foreach (var subquad in quad.subquads)
        {
            surfaceObjs[subquad.pos] = 0;
            if (quad.hasTarget)
                SetSubquadColor(subquad, offTargColor);
        }

        var timer = quad.gameObject.AddComponent<Timer>();
        var initPos = quad.transform.localPosition;
        var dirVec = GetDirection(dir);
        if (powerful)
        { 
            int code = Simulate(quad, dir, out int step);
            if (code == GetOut)
            {
                timer.period = step / quadOutSpeed;
                timer.confirmUpdate = false;
                timer.onUpdate = x =>
                    quad.transform.localPosition = initPos + x * step * dirVec;
                timer.onStop = () =>
                {
                    GLOBAL_INTERACTIVE = true;
                    InitQuadOut(quad, dir);
                };
            }
            else
            {
                timer.period = step / quadOutSpeed;
                timer.onUpdate = x =>
                    quad.transform.localPosition = initPos + x * step * dirVec;
                timer.onStop = () => EndMove(quad, dir, code, true);
            }
        }
        else // !powerful
        {
            if(IsGettingOut(quad.FirstPos, dir))
            {
                GLOBAL_INTERACTIVE = true;
                InitQuadOut(quad, dir);
            }
            else
            {
                quad.FirstPos = NextPos(quad.FirstPos, dir);
                timer.period = 2.0f / quadOutSpeed;
                timer.onUpdate = x => quad.transform.localPosition = initPos + x * dirVec;
                timer.timeMap = Timer.Accelaration(0, 1, 0.5f);
                timer.onStop = () => EndMove(quad, dir, CanMove, false);
            }
        }
    }
    private void EndMove(Quad quad, int dir, int code, bool powerful)
    {
        GLOBAL_INTERACTIVE = true;
        CheckPos(quad, true);
        foreach (var subquad in quad.subquads)
        {
            surfaceObjs[subquad.pos] = 1 << 8 | quad.id;
        }

        if (powerful)
        {
            if (code is HitIce or WoodHitIron or IronHitWood or IronHitIron)
            {
                Debug.Assert(quad.subquads.Length == 1);
                if (code == HitIce)
                {
                    if ((surfaceObjs[nextPosCache] >> 8) == 2)
                    {
                        ices[surfaceObjs[nextPosCache] & 0xff].Shrink();
                        history.Push(2 << 8 | surfaceObjs[nextPosCache] & 0xff);
                        surfaceObjs[nextPosCache] = 0;
                    }
                    A.QuadIce();
                }
                else if (code == WoodHitIron)
                {
                    StartMove(quad, dir ^ 1, false);
                    A.QuadIron();
                }
                else
                {
                    Quad nextQuad = quads[surfaceObjs[nextPosCache] & 0xff];
                    int nextCode = GetCode(nextQuad, dir);
                    if (nextCode is CanMove or GetOut)
                    {
                        StartMove(nextQuad, dir, code == IronHitIron);
                    }
                    if (code == IronHitIron)
                        A.IronIron();
                    else
                        A.QuadIron();
                }
            }
            else if (code == StuckHole)
                A.QuadStuck();
            else if (code == HitWall)
                A.QuadWall();
            else if (code == WoodHitWood)
                A.QuadQuad();
        }

        if (GLOBAL_INTERACTIVE)
        {
            if (quads.All(q => q.atHome))
                GameOver();
        }
    }
    private void CheckPos(Quad quad, bool playAudio = false)
    {
        if (quad.hasTarget)
        {
            quad.atHome = true;
            foreach (var subquad in quad.subquads)
            {
                bool subquadAtHome = targetObjs[subquad.pos] == quad.pattern;
                quad.atHome &= subquadAtHome;
                SetSubquadColor(subquad, subquadAtHome ? quad.color : offTargColor);
            }
            if (playAudio && quad.atHome)
                A.QuadHome();
        }
    }
    private void SetSubquadColor(SubQuad subquad, Color color)
    {
        subquad.sprite.material.SetColor("_EmissionColor", color);
        subquad.decalMat.SetColor("_EmissionColor", color);
    }
    private bool IsGettingOut(int pos, int dir) => dir switch
    {
        0 => pos % info.xx == info.xx - 1,
        1 => pos % info.xx == 0,
        2 => pos / info.xx == info.yy - 1,
        _ => pos / info.xx == 0
    };
    private void InitQuadOut(Quad quad, int dir)
    {
        quad.moveable = false;
        quad.gameObject.AddComponent<QuadOut>().Init(
            box.localScale.x * quadOutSpeed * GetDirection(dir));
        ++nQuadOut;
    }
    private void TryRemoveQuadOut(Quad quad)
    {
        if (quad.TryGetComponent<QuadOut>(out var quadOut))
        {
            Destroy(quadOut);
            --nQuadOut;
        }
    }

    #endregion


    #region Quiting
    public void GameOver()
    {
        M.gamePlayUI.GameOver();
        A.LevelComplete();
        isNextButtonActive = true;
        PlayerPrefs.SetInt($"data{chapter}",
            PlayerPrefs.GetInt($"data{chapter}") | 1 << (level - 1));
        PlayerPrefs.Save();
    }
    public void QuitGame()
    {
        history.Clear();
        quads.Clear();
        ices.Clear();
        Destroy(box.gameObject);

        var nextButton = M.gamePlayUI.transform.Find("next").gameObject;
        if(nextButton.TryGetComponent<Timer>(out var timer))
            Destroy(timer);
        nextButton.SetActive(false);
    }
    #endregion


    #region GeneralTools
    public static int ToNum(char c) => c >= 'a' ? c - 'a' + 10 : c - '0';
    public int NextPos(int pos, int dir)
    {
        if (dir == 0) return pos + 1;
        else if (dir == 1) return pos - 1;
        else if (dir == 2) return pos + info.xx;
        else return pos - info.xx;
    }
    public Vector3 GetPos(int x, int z, float y)
    {
        return new Vector3(0.5f - info.xx * 0.5f + x, y, 0.5f - info.yy * 0.5f + z);
    }
    public Vector3 GetPos(int pos, float y)
    {
        return GetPos(pos % info.xx, pos / info.xx, y);
    }
    public Vector3 GetPosF(float x, float z)
    {
        return new Vector3(0.5f - info.xx * 0.5f + x, 0, 0.5f - info.yy * 0.5f + z);
    }
    public Vector3 GetDirection(int dir)
    {
        return dir switch
        {
            0 => Vector3.right,
            1 => Vector3.left,
            2 => Vector3.forward,
            _ => Vector3.back
        };
    }
    public int ParseDirection(Vector3 v)
    {
        return (v.z + v.x > 0 ? 0 : 3) ^ (v.z - v.x > 0 ? 2 : 0);
    }
    #endregion


    #region Buttons
    public void Restart()
    {
        M.gamePlayUI.SetMove(nMove = 0);
        history.Clear();
        if (nQuadOut > 0)
        {
            foreach (var quad in quads)
            {
                if (quad.TryGetComponent<QuadOut>(out var quadOut))
                    Destroy(quadOut);
                if (quad.TryGetComponent<Timer>(out var timer))
                    Destroy(timer);
            }
            nQuadOut = 0;
        }
        System.Array.Fill(surfaceObjs, 0);
        foreach (Quad quad in quads)
        {
            quad.moveable = true;
            quad.FirstPos = quad.initFirstPos;
            quad.transform.localPosition = GetPos(quad.FirstPos, 0);
            quad.transform.rotation = quad.InitRotation;
            SetQuadToArray(quad, true);
            CheckPos(quad);
        }
        foreach (Ice ice in ices)
        {
            ice.Init();
            surfaceObjs[ice.pos] = 2 << 8 | ice.id;
        }

        GLOBAL_INTERACTIVE = true;
    }
    public void Undo()
    {
        if (nMove == 0) return;
        M.gamePlayUI.SetMove(--nMove);

        List<Quad> quadTodo = new();
        List<Ice> iceTodo = new();
        int j = history.Pop();
        while (j != 0)
        {
            if ((j >> 20) == 1)
            {
                var quad = quads[(j >> 16) & 0xf];
                if (surfaceObjs[quad.FirstPos] != 0)
                    SetQuadToArray(quad, false);
                quad.moveable = true;
                quad.FirstPos = j & 0xffff;
                quad.transform.localPosition = GetPos(quad.FirstPos, 0);
                quad.transform.rotation = quad.InitRotation;
                CheckPos(quad);
                quadTodo.Add(quad);
                if (nQuadOut > 0)
                    TryRemoveQuadOut(quad);
            }
            else
            {
                var ice = ices[j & 0xf];
                ice.Init();
                iceTodo.Add(ice);
            }
            j = history.Pop();
        }
        foreach (var quad in quadTodo)
            SetQuadToArray(quad, true);
        foreach (var ice in iceTodo)
            surfaceObjs[ice.pos] = 2 << 8 | ice.id;
        
        GLOBAL_INTERACTIVE = true;
    }
    private void SetQuadToArray(Quad quad, bool addOrRemove)
    {
        foreach (var subquad in quad.subquads)
            surfaceObjs[subquad.pos] = addOrRemove ? 1 << 8 | quad.id : 0;
    }
    #endregion

}
