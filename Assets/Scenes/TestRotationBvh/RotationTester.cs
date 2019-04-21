using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class RotationTester : MonoBehaviour
{
    private Transform t;
    // Start is called before the first frame update
    void Awake()
    {
        t = GetComponent<Transform>();
    }

    private Vector3 _rotAroundGlobal;
    private Vector3 _rotAroundLocal;
    public Vector3 RotAroundGlobal
    {
        get { return _rotAroundGlobal; }
        set
        {
            if(!t)
                t = GetComponent<Transform>();
            
            _rotAroundGlobal = value;

            t.rotation = Quaternion.Euler(value);
            
            _rotAroundLocal = t.localRotation.eulerAngles;
            
        }
    }

    [SerializeField]
    public Vector3 RotAroundLocal
    {
        get { return _rotAroundLocal; }
        set
        {
            if (!t)
                t = GetComponent<Transform>();
            
            _rotAroundLocal = value;
            t.localRotation = Quaternion.Euler(value);

            _rotAroundGlobal = t.rotation.eulerAngles;

        }
    }
}

[CustomEditor(typeof(RotationTester))]
public class letthatshitEditor : Editor
{
    private  RotationTester tester => target as RotationTester;

    public override void OnInspectorGUI()
    {
        tester.RotAroundLocal = EditorGUILayout.Vector3Field("Rotation Around Local", tester.RotAroundLocal);
        tester.RotAroundGlobal = EditorGUILayout.Vector3Field("Rotation Around Global", tester.RotAroundGlobal);
    }
}

