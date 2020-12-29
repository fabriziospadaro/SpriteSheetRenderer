using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

[UpdateInGroup(typeof(PresentationSystemGroup))]
public class SpriteSheetRenderer : ComponentSystem
{
    private Mesh mesh;

    protected override void OnCreate()
    {
        mesh = MeshExtension.Quad();
    }

    protected override void OnDestroy()
    {
        SpriteSheetManager.CleanBuffers();
    }

    protected override void OnUpdate()
    {
        for (int i = 0; i < SpriteSheetManager.renderInformation.Count; i++)
        {
            if (UpdateBuffers(i) > 0)
                Graphics.DrawMeshInstancedIndirect(mesh, 0, SpriteSheetManager.renderInformation[i].material, new Bounds(Vector3.zero, Vector3.one), SpriteSheetManager.renderInformation[i].argsBuffer);

            //this is w.i.p to clean the old buffers
            DynamicBuffer<SpriteIndexBuffer> indexBuffer = EntityManager.GetBuffer<SpriteIndexBuffer>(SpriteSheetManager.renderInformation[i].bufferEntity);
            int start = indexBuffer.Length - 1;
            while (start >= 0 && indexBuffer[start].index == -1)
            {
                start--;
            }

            start++;
            int toRemove = indexBuffer.Length - start;
            if (toRemove > 0)
            {
                EntityManager.GetBuffer<SpriteIndexBuffer>(SpriteSheetManager.renderInformation[i].bufferEntity).RemoveRange(start, toRemove);
                EntityManager.GetBuffer<MatrixBuffer>(SpriteSheetManager.renderInformation[i].bufferEntity).RemoveRange(start, toRemove);
                EntityManager.GetBuffer<SpriteColorBuffer>(SpriteSheetManager.renderInformation[i].bufferEntity).RemoveRange(start, toRemove);
            }
        }
    }

    //we should only update the index of the changed datas for index buffer,matrixbuffer and color buffer inside a burst job to avoid overhead
    int UpdateBuffers(int renderIndex)
    {
        SpriteSheetManager.ReleaseBuffer(renderIndex);

        RenderInformation renderInformation = SpriteSheetManager.renderInformation[renderIndex];
        int instanceCount = EntityManager.GetBuffer<SpriteIndexBuffer>(renderInformation.bufferEntity).Length;
        if (instanceCount > 0)
        {
            int stride = instanceCount >= 16 ? 16 : 16 * SpriteSheetCache.GetLength(renderInformation.material);
            if (renderInformation.updateUvs)
            {
                SpriteSheetManager.ReleaseUvBuffer(renderIndex);
                renderInformation.uvBuffer = new ComputeBuffer(instanceCount, stride);
                renderInformation.uvBuffer.SetData(EntityManager.GetBuffer<UvBuffer>(renderInformation.bufferEntity).Reinterpret<float4>().AsNativeArray());
                renderInformation.material.SetBuffer("uvBuffer", renderInformation.uvBuffer);
                renderInformation.updateUvs = false;
            }

            renderInformation.indexBuffer = new ComputeBuffer(instanceCount, sizeof(int));
            renderInformation.indexBuffer.SetData(EntityManager.GetBuffer<SpriteIndexBuffer>(renderInformation.bufferEntity).Reinterpret<int>().AsNativeArray());
            renderInformation.material.SetBuffer("indexBuffer", renderInformation.indexBuffer);

            renderInformation.matrixBuffer = new ComputeBuffer(instanceCount, 16);
            renderInformation.matrixBuffer.SetData(EntityManager.GetBuffer<MatrixBuffer>(renderInformation.bufferEntity).Reinterpret<float4>().AsNativeArray());
            renderInformation.material.SetBuffer("matrixBuffer", renderInformation.matrixBuffer);

            renderInformation.args[1] = (uint)instanceCount;
            renderInformation.argsBuffer.SetData(renderInformation.args);

            renderInformation.colorsBuffer = new ComputeBuffer(instanceCount, 16);
            renderInformation.colorsBuffer.SetData(EntityManager.GetBuffer<SpriteColorBuffer>(renderInformation.bufferEntity).Reinterpret<float4>().AsNativeArray());
            renderInformation.material.SetBuffer("colorsBuffer", renderInformation.colorsBuffer);
        }
        return instanceCount;
    }
}
