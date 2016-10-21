using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LSystem : MonoBehaviour {
	List<LSElement> str;

    public float scale = 1f;

    private int length0 = 100;
    private float r1 = 0.57f;
    private float r2 = 0.95f;
    private float alpha1 = -70;
    private float alpha2 = 55;
    private float phi1 = 7;
    private float phi2 = 17;
    private float w0 = 4;
    private float q = 0.40f;
    private float e = 0.10f;
    private float smin = 5.0f;
    private float iter = 12;
    
	List<Vector3> vertices = new List<Vector3>();
	List<int> indices = new List<int>();
	int count = 0;
	void Start(){
		str = new List<LSElement>();
        ExpandRules();
        var mesh = Interpret();
        mesh.RecalculateNormals();
        mesh.uv = new Vector2[mesh.vertexCount];
        GetComponent<MeshFilter>().mesh = mesh;
        transform.localScale = new Vector3(1f /1000f * scale, 1f / 1000f * scale, 1f / 1000f * scale);
    }
    
	void ExpandRules(){
		str.Clear();
		str.Add(new LSElement(LSElement.LSSymbol.A, length0, w0));
        
		Rule r = new Rule(alpha1, alpha2, phi1, phi2, r1, r2, q, e, smin);
		for (int i=0;i<iter;i++){
			List<LSElement> outList = new List<LSElement>();
			foreach (var s in str){
				r.Apply(s, outList);
			}
			str = outList;
		}
	}

	void AddCone(Matrix4x4 m, float l, float w0, float w1){
		const int N = 5;
		for (int i=0;i<=N;i++){
			float alpha = 2.0f*Mathf.PI*i/(float)N;
			Vector3 p0 = m.MultiplyPoint(new Vector3(w0 * Mathf.Cos(alpha), w0 * Mathf.Sin(alpha), 0));
			Vector3 p1 = m.MultiplyPoint(new Vector3(w1 * Mathf.Cos(alpha), w1 * Mathf.Sin(alpha), l));

			alpha = 2.0f*Mathf.PI*(i+1)/(float)N;
			Vector3 p2 = m.MultiplyPoint(new Vector3(w0 * Mathf.Cos(alpha), w0 * Mathf.Sin(alpha), 0));
			Vector3 p3 = m.MultiplyPoint(new Vector3(w1 * Mathf.Cos(alpha), w1 * Mathf.Sin(alpha), l));

			vertices.Add(p0);
			vertices.Add(p2);
			vertices.Add(p1);

			vertices.Add(p1);
			vertices.Add(p2);
			vertices.Add(p3);
			for (int j=0;j<6;j++){
				indices.Add(indices.Count);
			}
		}
	}

	public Mesh Interpret(){
		Turtle turtle = new Turtle(w0);

		foreach (var elem in str){
			switch (elem.symbol){
			case LSElement.LSSymbol.DRAW:
				AddCone(turtle.Peek().M, elem.data[0], turtle.GetWidth(), turtle.GetWidth() * elem.data[1]);
				turtle.Move(elem.data[0]);
				break;
			case LSElement.LSSymbol.TURN:
				turtle.Turn(elem.data[0]);
				break;
			case LSElement.LSSymbol.ROLL:
				turtle.Roll(elem.data[0]);
				break;
			case LSElement.LSSymbol.LEFT_BRACKET:
				turtle.Push();
				break;
			case LSElement.LSSymbol.RIGHT_BRACKET:
				turtle.Pop();
				break;
			case LSElement.LSSymbol.WIDTH:
				turtle.SetWidth(elem.data[0]);
				break;
			}
		}
		Mesh mesh = new Mesh();
		mesh.vertices = vertices.ToArray();
		mesh.triangles = indices.ToArray();
		vertices.Clear();
		indices.Clear();
		return mesh;
	}
}
