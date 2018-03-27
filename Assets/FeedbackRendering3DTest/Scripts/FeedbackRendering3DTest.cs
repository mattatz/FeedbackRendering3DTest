using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent (typeof(MeshFilter), typeof(MeshRenderer))]
public class FeedbackRendering3DTest : MonoBehaviour {

    [SerializeField, Range(16, 256)] protected int width = 128, height = 64, depth = 128;
    [SerializeField] protected ComputeShader compute;
    [SerializeField] protected Transform point;
    [SerializeField, Range(0.01f, 0.75f)] protected float radius = 0.1f;
    [SerializeField, Range(0.9f, 1f)] protected float decay = 0.985f;
    [SerializeField, Range(0.1f, 1f)] protected float displacement = 0.5f;

    PingPong3D velocity, color;

    #region Shader property keys

    protected const string kVelocityReadKey = "_VelocityRead", kVelocityWriteKey = "_VelocityWrite";
    protected const string kColorReadKey = "_ColorRead", kColorWriteKey = "_ColorWrite";

    #endregion

    protected new Renderer renderer;
    protected MaterialPropertyBlock block;

	protected void Start () {
        velocity = new PingPong3D(width, height, depth, RenderTextureFormat.ARGBFloat, FilterMode.Point, TextureWrapMode.Repeat);
        color = new PingPong3D(width, height, depth, RenderTextureFormat.ARGBHalf, FilterMode.Bilinear, TextureWrapMode.Repeat);

        block = new MaterialPropertyBlock();
        renderer = GetComponent<Renderer>();
        renderer.GetPropertyBlock(block);

        var mesh = Build(width, height, depth);
        GetComponent<MeshFilter>().sharedMesh = mesh;

        compute.SetInt("_Width", width);
        compute.SetInt("_Height", height);
        compute.SetInt("_Depth", depth);
        compute.SetVector("_InvSize", new Vector3(1f / width, 1f / height, 1f / depth));
        compute.SetVector("_BoundsMin", mesh.bounds.min);
        compute.SetVector("_BoundsMax", mesh.bounds.max);

        Reset();
	}
	
	protected void Update () {
        compute.SetVector("_Point", transform.InverseTransformPoint(point.position));
        compute.SetFloat("_Radius", radius);
        compute.SetFloat("_Decay", decay);
        compute.SetFloat("_Displacement", displacement);

        int kernel;
        uint tx, ty, tz;

        kernel = compute.FindKernel("Force");
        compute.GetKernelThreadGroupSizes(kernel, out tx, out ty, out tz);

        compute.SetTexture(kernel, kVelocityReadKey, velocity.ReadBuffer); compute.SetTexture(kernel, kVelocityWriteKey, velocity.WriteBuffer);
        compute.Dispatch(kernel, width / (int)tx, height / (int)ty, depth / (int)tz);
        velocity.Swap();

        kernel = compute.FindKernel("Diffuse");
        compute.GetKernelThreadGroupSizes(kernel, out tx, out ty, out tz);

        compute.SetTexture(kernel, kVelocityReadKey, velocity.ReadBuffer); compute.SetTexture(kernel, kVelocityWriteKey, velocity.WriteBuffer);
        compute.Dispatch(kernel, width / (int)tx, height / (int)ty, depth / (int)tz);
        velocity.Swap();

        kernel = compute.FindKernel("Flow");
        compute.GetKernelThreadGroupSizes(kernel, out tx, out ty, out tz);

        compute.SetTexture(kernel, kVelocityReadKey, velocity.ReadBuffer);
        compute.SetTexture(kernel, kColorReadKey, color.ReadBuffer);  compute.SetTexture(kernel, kColorWriteKey, color.WriteBuffer);
        compute.Dispatch(kernel, width / (int)tx, height / (int)ty, depth / (int)tz);
        color.Swap();

        block.SetTexture("_Velocity", velocity.ReadBuffer);
        block.SetTexture("_Color", color.ReadBuffer);
        renderer.SetPropertyBlock(block);
	}

    protected void OnDestroy()
    {
        if(velocity != null)
        {
            velocity.Dispose();
            velocity = null;
        }

        if(color != null)
        {
            color.Dispose();
            color = null;
        }
    }

    public void Reset()
    {
        int kernel;
        uint tx, ty, tz;

        kernel = compute.FindKernel("Reset");
        compute.GetKernelThreadGroupSizes(kernel, out tx, out ty, out tz);
        compute.SetTexture(kernel, kColorWriteKey, color.WriteBuffer);
        compute.Dispatch(kernel, width / (int)tx, height / (int)ty, depth / (int)tz);
        color.Swap();
    }

    protected Mesh Build(int width, int height, int depth)
    {
        var mesh = new Mesh();
        var vertices = new List<Vector3>();
        var indices = new List<int>();
        var inv = new Vector3(1f / width, 1f / height, 1f / depth);
        var offset = -new Vector3(0.5f, 0.5f, 0.5f);
        for(int z = 0; z < depth; z++)
        {
            for(int y = 0; y < height; y++)
            {
                for(int x = 0; x < width; x++)
                {
                    var p = new Vector3(x, y, z);
                    indices.Add(vertices.Count);
                    vertices.Add(Vector3.Scale(p, inv) + offset);
                }
            }
        }
        mesh.SetVertices(vertices);
        mesh.indexFormat = vertices.Count < 65535 ? IndexFormat.UInt16 : IndexFormat.UInt32;
        mesh.SetIndices(indices.ToArray(), MeshTopology.Points, 0);
        mesh.RecalculateBounds();
        return mesh;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }

}
