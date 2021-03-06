using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkCollider : MonoBehaviour {
    private float chunkResolution;
    private EdgeCollider2D[] currentColliders;
    private EdgeCollider2D currentCollider;
    private Vector2[] edgePoints;

    public void Generate2DCollider(VoxelChunk chunk, int chunkResolution) {
        this.chunkResolution = chunkResolution;

        currentColliders = chunk.gameObject.GetComponents<EdgeCollider2D>();
        foreach (var t in currentColliders) {
            Destroy(t);
        }

        CalculateMeshOutlines(chunk);

        foreach (var outline in chunk.outlines) {
            currentCollider = chunk.gameObject.AddComponent<EdgeCollider2D>();
            edgePoints = new Vector2[outline.Count];

            for (int i = 0; i < outline.Count; i++) {
                edgePoints[i] = new Vector2(chunk.vertices[outline[i]].x, chunk.vertices[outline[i]].y);
            }

            currentCollider.points = edgePoints;
        }
    }

    private void CalculateMeshOutlines(VoxelChunk chunk) {
        for (var vertexIndex = 0; vertexIndex < chunk.vertices.Length; vertexIndex++) {
            if (!chunk.checkedVertices.Contains(vertexIndex)) {
                var newOutlineVertex = GetConnectedOutlineVertex(vertexIndex, chunk);
                if (newOutlineVertex == -1) continue;
                chunk.checkedVertices.Add(vertexIndex);

                var newOutline = new List<int> {vertexIndex};
                chunk.outlines.Add(newOutline);
                FollowOutline(newOutlineVertex, chunk);
                chunk.outlines[chunk.outlines.Count - 1].Add(vertexIndex);
            }
        }
    }

    private void FollowOutline(int vertexIndex, VoxelChunk chunk) {
        chunk.outlines[chunk.outlines.Count - 1].Add(vertexIndex);
        chunk.checkedVertices.Add(vertexIndex);
        var nextVertexIndex = GetConnectedOutlineVertex(vertexIndex, chunk);

        if (nextVertexIndex != -1) {
            FollowOutline(nextVertexIndex, chunk);
        }
    }

    private int GetConnectedOutlineVertex(int vertexIndex, VoxelChunk chunk) {
        foreach (var triangle in chunk.triangleDictionary[chunk.vertices[vertexIndex]]) {
            for (var i = 0; i < 3; i++) {
                if (triangle[i].x == chunk.vertices[vertexIndex].x / chunkResolution &&
                    triangle[i].y == chunk.vertices[vertexIndex].y / chunkResolution) {
                    Vector3 findVertice = triangle[(i + 1) % 3] * chunkResolution;
                    var nextVertexIndex = chunk.verticeDictionary[findVertice];
                    if (!chunk.checkedVertices.Contains(nextVertexIndex) &&
                        IsOutlineEdge(vertexIndex, nextVertexIndex, chunk)) {
                        return nextVertexIndex;
                    }
                }
            }
        }

        return -1;
    }

    private bool IsOutlineEdge(int vertexA, int vertexB, VoxelChunk chunk) {
        var trianglesContainingVertexA = chunk.triangleDictionary[chunk.vertices[vertexA]];
        var sharedTriangleCount = 0;
        var chunkVertice = new Vector2(chunk.vertices[vertexB].x / chunkResolution,
            chunk.vertices[vertexB].y / chunkResolution);

        for (var i = 0; i < trianglesContainingVertexA.Count; i++) {
            if ((trianglesContainingVertexA[i].A.x == chunkVertice.x &&
                 trianglesContainingVertexA[i].A.y == chunkVertice.y) ||
                (trianglesContainingVertexA[i].B.x == chunkVertice.x &&
                 trianglesContainingVertexA[i].B.y == chunkVertice.y) ||
                (trianglesContainingVertexA[i].C.x == chunkVertice.x &&
                 trianglesContainingVertexA[i].C.y == chunkVertice.y)
            ) {
                sharedTriangleCount++;
                if (sharedTriangleCount > 1) {
                    break;
                }
            }
        }

        return sharedTriangleCount == 1;
    }
}