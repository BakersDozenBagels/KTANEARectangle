using System.Collections;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Rectangularity : MonoBehaviour
{
    [SerializeField, RummageNoRemove, RummageNoRename]
    private Color[] _colors;
    [SerializeField, RummageNoRemove, RummageNoRename]
    private Renderer _rectangle;
    [SerializeField, RummageNoRemove, RummageNoRename]
    private KMAudio _audio;
    [SerializeField, RummageNoRemove, RummageNoRename]
    private KMBombInfo _info;
    [SerializeField, RummageNoRemove, RummageNoRename]
    private TextMesh _text;

    private bool _initted, _isSolved;
    private Rectangularity[] _rects;
    private MonoBehaviour[] _squares;
    private int[][] _assignments;
    private int _correctRect;
    private int _correctTime;
    private int _press;
    private int _pressTime;
    private readonly int _id = ++_idc;
    private bool[] _solved;
    private static int _idc;

    private static readonly string[] _colorNames = { "ORANGE", "PINK", "CYAN", "YELLOW", "LAVENDER", "BROWN", "TAN", "BLUE", "JADE", "INDIGO", "WHITE" };

    [RummageNoRemove, RummageNoRename]
    private void Start()
    {
        _text.gameObject.SetActive(GetComponent<KMColorblindMode>().ColorblindModeActive);

        if(_initted)
            return;

        _rects = transform.root.GetComponentsInChildren<Rectangularity>();
        _squares = transform.root.GetComponentsInChildren<MonoBehaviour>().Where(b => b.GetType().Name == "ASquareScript").ToArray();
        _assignments = new int[_rects.Length + _squares.Length][];
        _solved = new bool[_rects.Length];
        int showsCorrectRect = Random.Range(0, _assignments.Length);
        _correctRect = Random.Range(0, _assignments.Length);
        _correctTime = Random.Range(0, 10);
        for(int i = 0; i < _rects.Length; ++i)
        {
            Rectangularity rect = _rects[i];
            int j = i;
            rect._initted = true;
            KMSelectable sel = rect.GetComponent<KMSelectable>();
            sel.Children[0].OnHighlight += () =>
            {
                if(_solved.All(b => b))
                    return;
                _audio.PlaySoundAtTransform("HL", sel.transform);
                Highlight(j);
            };
            sel.Children[0].OnHighlightEnded += () =>
            {
                if(_solved.All(b => b))
                    return;
                _audio.PlaySoundAtTransform("UHL", sel.transform);
                UnHighlight();
            };
            sel.Children[0].OnInteract += () =>
            {
                sel.Children[0].AddInteractionPunch(0.2f);
                _audio.PlaySoundAtTransform("Press", sel.Children[0].transform);
                Press(j);
                return false;
            };
            sel.Children[0].OnInteractEnded += () =>
            {
                _audio.PlaySoundAtTransform("Release", sel.Children[0].transform);
                Release();
            };
        }
        for(int i = _rects.Length; i < _assignments.Length; ++i)
        {
            MonoBehaviour rect = _squares[i - _rects.Length];
            int j = i;
            KMSelectable sel = rect.GetComponent<KMSelectable>();
            sel.Children[0].OnHighlight += () =>
            {
                if(_solved.All(b => b))
                    return;
                _audio.PlaySoundAtTransform("HL", sel.transform);
                Highlight(j);
            };
            sel.Children[0].OnHighlightEnded += () =>
            {
                if(_solved.All(b => b))
                    return;
                _audio.PlaySoundAtTransform("UHL", sel.transform);
                UnHighlight();
            };
            sel.Children[0].OnInteract += () =>
            {
                sel.Children[0].AddInteractionPunch(0.2f);
                _audio.PlaySoundAtTransform("Press", sel.Children[0].transform);
                Press(j);
                return false;
            };
            sel.Children[0].OnInteractEnded += () =>
            {
                _audio.PlaySoundAtTransform("Release", sel.Children[0].transform);
                Release();
            };
        }

        Generate();
    }

    private void Generate()
    {
        int showsCorrectRect = Random.Range(0, _assignments.Length);
        _correctRect = Random.Range(0, _assignments.Length);
        _correctTime = Random.Range(0, 10);
        for(int i = 0; i < _assignments.Length; ++i)
        {
            _assignments[i] = new int[10];
            for(int t = 0; t < 10; ++t)
            {
                if(i == showsCorrectRect && t == _correctTime)
                    _assignments[i][t] = _correctRect * 10 + t;
                else
                {
                    int r = Random.Range(0, 9);
                    if(r >= t)
                        ++r;
                    int rt = Random.Range(0, _assignments.Length);
                    _assignments[i][t] = rt * 10 + r;
                }
            }
        }

        if(_assignments.Length == 1)
            Log("There is 1 rectangle.");
        else
            Log("There are " + _assignments.Length + " rectangles.");
        if(_correctRect > _rects.Length)
            Log("Solution is square #" + _squares[_correctRect - _rects.Length].GetType().GetField("_moduleId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(_squares[_correctRect - _rects.Length]) + " at time " + _correctTime + ".");
        else
            Log("Solution is module #" + _rects[_correctRect]._id + " at time " + _correctTime + ".");
    }

    private void Release()
    {
        if(_solved.All(b => b))
            return;
        if(_pressTime == (int)_info.GetTime())
        {
            if(_press == _correctRect * 10 + _correctTime)
                Solve();
            else
                StartCoroutine(Strike());
        }
    }

    private IEnumerator Strike()
    {
        Rectangularity r = _rects[Random.Range(0, _rects.Length)];
        r.Log("Strike.");
        r.GetComponent<KMBombModule>().HandleStrike();
        r._rectangle.material.color = Color.red;
        r._text.text = "RED";
        yield return new WaitForSeconds(1f);
        r._rectangle.material.color = r._isSolved ? Color.green : new Color(221f / 255f, 221f / 255f, 221f / 255f);
        r._text.text = r._isSolved ? "GREEN" : _colorNames[10];
    }

    private void Solve()
    {
        int ix = _solved.IndexOf(b => !b);
        _solved[ix] = true;
        Rectangularity r = _rects[ix];
        r.Log("Solved.");
        r.GetComponent<KMBombModule>().HandlePass();
        r._isSolved = true;
        r._rectangle.material.color = Color.green;
        r._text.text = "GREEN";
        Generate();
    }

    private void Log(string s)
    {
        Debug.Log("[A Rectangle #" + _id + "] " + s);
    }

    private void Press(int i)
    {
        if(_solved.All(b => b))
            return;
        _pressTime = (int)_info.GetTime();
        _press = 10 * i + _pressTime % 10;
    }

    private void UnHighlight()
    {
        foreach(Rectangularity rect in _rects)
        {
            rect._rectangle.material.color = rect._isSolved ? Color.green : new Color(221f / 255f, 221f / 255f, 221f / 255f);
            rect._text.text = rect._isSolved ? "GREEN" : _colorNames[10];
        }
        foreach(MonoBehaviour sq in _squares)
        {
            TextMesh text = sq.GetType().GetField("ColorblindText", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).GetValue(sq) as TextMesh;
            text.text = _colorNames[10];
            Material[] mats = sq.GetType().GetField("SquareColors", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).GetValue(sq) as Material[];
            Renderer square = sq.GetType().GetField("SquareObj", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).GetValue(sq) as Renderer;
            square.material = mats[10];
        }
    }

    private void Highlight(int i)
    {
        int ix = _assignments[i][(int)_info.GetTime() % 10];
        if(ix / 10 > _rects.Length)
        {
            MonoBehaviour sq = _squares[ix / 10 - _rects.Length];
            TextMesh text = sq.GetType().GetField("ColorblindText", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).GetValue(sq) as TextMesh;
            text.text = _colorNames[ix % 10];
            Material[] mats = sq.GetType().GetField("SquareColors", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).GetValue(sq) as Material[];
            Renderer square = sq.GetType().GetField("SquareObj", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).GetValue(sq) as Renderer;
            square.material = mats[ix % 10];
        }
        else
            _rects[ix / 10].SetColor(ix % 10);
    }

    private void SetColor(int c)
    {
        _rectangle.material.color = _colors[c];
        _text.text = _colorNames[c];
    }
}
