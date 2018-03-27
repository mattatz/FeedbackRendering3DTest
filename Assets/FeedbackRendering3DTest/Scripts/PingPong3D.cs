using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Rendering;

public class PingPong3D : System.IDisposable {

    public RenderTexture ReadBuffer { get { return buffers[iRead]; } }
    public RenderTexture WriteBuffer { get { return buffers[iWrite]; } }

    protected RenderTexture[] buffers;
    protected int iRead = 0, iWrite = 1;

    public PingPong3D(int width, int height, int depth, RenderTextureFormat format = RenderTextureFormat.ARGBFloat, FilterMode filter = FilterMode.Point, TextureWrapMode wrap = TextureWrapMode.Repeat)
    {
        buffers = new RenderTexture[2]
        {
            Create3DBuffer(width, height, depth, format, filter, wrap),
            Create3DBuffer(width, height, depth, format, filter, wrap)
        };
    }

    protected RenderTexture Create3DBuffer(int width, int height, int depth, RenderTextureFormat format, FilterMode filter, TextureWrapMode wrap)
    {
        var buffer = new RenderTexture(width, height, 0, format);
        buffer.dimension = TextureDimension.Tex3D;
        buffer.volumeDepth = depth;
        buffer.filterMode = filter;
        buffer.wrapMode = wrap;
        buffer.enableRandomWrite = true;
        buffer.Create();
        return buffer;
    }

    public void Swap()
    {
        var tmp = iRead;
        iRead = iWrite;
        iWrite = tmp;
    }

    public void Dispose() {
        if(buffers != null)
        {
            buffers[0].Release();
            buffers[1].Release();
            buffers = null;
        }
    }

}
