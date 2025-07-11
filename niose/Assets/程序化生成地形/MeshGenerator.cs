using UnityEngine;

public static class MeshGenerator
{
    public static MeshData GenerateTerrainMesh(float[,] heightMap,MeshSettings meshSettings,int levelOfDetail)
    {
        int meshSimplificationIncrement =(levelOfDetail == 0) ? 1 : levelOfDetail * 2;
        
        int borderSize = heightMap.GetLength (0);
        int meshSize = borderSize - 2 * meshSimplificationIncrement;
        int meshSizeUnsimplified = borderSize - 2;
        
        float topLeftX = (meshSizeUnsimplified -1) / -2f;
        float topLeftZ = (meshSizeUnsimplified - 1) / 2f;
        
        int verticesPerLine = ((meshSize - 1) / meshSimplificationIncrement) + 1;
        
        MeshData meshData = new MeshData(verticesPerLine,meshSettings.useFlatShading);

        int[,] vertexIndicesMap = new int[borderSize, borderSize];
        int meshVertexIndex = 0;
        int borderVertexIndex = -1;
        for (int y = 0; y < borderSize; y += meshSimplificationIncrement)
        {
            for (int x = 0; x < borderSize; x += meshSimplificationIncrement)
            {
                bool isBorderVertex = y == 0 || y == borderSize - 1 || x == 0 || x == borderSize - 1;
                if (isBorderVertex)
                {
                    vertexIndicesMap[x, y] = borderVertexIndex;
                    borderVertexIndex--;
                }
                else
                {
                    vertexIndicesMap[x, y] = meshVertexIndex;
                    meshVertexIndex++;
                }
            }
        }

        for (int y = 0; y <borderSize; y += meshSimplificationIncrement)
        {
            for (int x = 0; x < borderSize; x+= meshSimplificationIncrement)
            {
                int vertexIndex = vertexIndicesMap[x, y];
                Vector2 percent = new Vector2 ((x - meshSimplificationIncrement) / (float)meshSize, (y - meshSimplificationIncrement) / (float)meshSize);
                float height = heightMap[x,y];
                Vector3 vertexIndexPosition = new Vector3 ((topLeftX + percent.x * meshSizeUnsimplified) * meshSettings.meshScale, 
                                                           height , 
                                                           (topLeftZ - percent.y * meshSizeUnsimplified) * meshSettings.meshScale);
                
                meshData.AddVertex(vertexIndexPosition, percent, vertexIndex);
                if(x<borderSize-1 && y < borderSize - 1)
                {
                    int a = vertexIndicesMap[x, y];
                    int b = vertexIndicesMap[x + meshSimplificationIncrement, y];
                    int c = vertexIndicesMap[x, y + meshSimplificationIncrement];
                    int d = vertexIndicesMap[x + meshSimplificationIncrement, y + meshSimplificationIncrement];
                    meshData.AddTriangle(a,d,c);
                    meshData.AddTriangle(d,a,b);
                }
                vertexIndex++;
            }
        }
        meshData.Finalize();
        return meshData;
    }
}
public class MeshData
{
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;
    Vector3[] bakedNormals;
    
    Vector3[] borderVertices;
    int[] borderTriangles;
    
    int triangleIndex;
    int borderTriangleIndex;
    
    bool useFlatShading;
    public MeshData(int verticesPerLine,bool useFlatShading)
    {
        this.useFlatShading = useFlatShading;
        
        vertices = new Vector3[verticesPerLine * verticesPerLine];
        triangles = new int[(verticesPerLine - 1) * (verticesPerLine - 1) * 6];
        uvs = new Vector2[verticesPerLine * verticesPerLine];
        
        borderVertices = new Vector3[verticesPerLine * 4 + 4];
        borderTriangles = new int[24 * verticesPerLine];
    }

    public void AddVertex(Vector3 vertexPosition,Vector2 uv,int vertexIndex)
    {
        if (vertexIndex < 0)
        {
            borderVertices[-vertexIndex - 1] = vertexPosition;
        }
        else
        {
            vertices[vertexIndex] = vertexPosition;
            uvs[vertexIndex] = uv;
        }
        
    }
    public void AddTriangle(int a, int b, int c)
    {
        if(a<0 || b < 0 || c < 0)
        {
            borderTriangles[borderTriangleIndex] = a;
            borderTriangles[borderTriangleIndex + 1] = b;
            borderTriangles[borderTriangleIndex + 2] = c;
            borderTriangleIndex += 3;
        }
        else
        {
            triangles[triangleIndex] = a;
            triangles[triangleIndex + 1] = b;
            triangles[triangleIndex + 2] = c;
            triangleIndex += 3;
        }
        
    }

    Vector3[] CalculateNormals()
    {
        Vector3[] vertexNoramls = new Vector3[vertices.Length];
        int triangleCount = triangles.Length / 3;
        for (int i = 0; i < triangleCount; i++)
        {
            int normalTriangleIndex = i * 3;
            int vertexIndexA = triangles[normalTriangleIndex];
            int vertexIndexB = triangles[normalTriangleIndex + 1];
            int vertexIndexC = triangles[normalTriangleIndex + 2];
            
            Vector3 triangleNormal = SurfaceNormalFormIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            vertexNoramls[vertexIndexA] += triangleNormal;
            vertexNoramls[vertexIndexB] += triangleNormal;
            vertexNoramls[vertexIndexC] += triangleNormal;
        }
        int borderTriangleCount = borderTriangles.Length / 3;
        for (int i = 0; i < borderTriangleCount; i++)
        {
            int normalTriangleIndex = i * 3;
            int vertexIndexA = borderTriangles[normalTriangleIndex];
            int vertexIndexB = borderTriangles[normalTriangleIndex + 1];
            int vertexIndexC = borderTriangles[normalTriangleIndex + 2];
            
            Vector3 triangleNormal = SurfaceNormalFormIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            if(vertexIndexA >= 0)
                vertexNoramls[vertexIndexA] += triangleNormal;
            if(vertexIndexB >= 0)
                vertexNoramls[vertexIndexB] += triangleNormal;
            if(vertexIndexC >= 0)
                vertexNoramls[vertexIndexC] += triangleNormal;
        }
        
        for (int i = 0; i < vertexNoramls.Length; i++)
        {
            vertexNoramls[i].Normalize();
        }
        return vertexNoramls;
    }

    Vector3 SurfaceNormalFormIndices(int indexA, int indexB, int indexC)
    {
        Vector3 pointA = (indexA < 0) ? borderVertices[-indexA - 1]:vertices[indexA];
        Vector3 pointB = (indexB < 0) ? borderVertices[-indexB - 1]:vertices[indexB];
        Vector3 pointC = (indexC < 0) ? borderVertices[-indexC - 1]:vertices[indexC];
        
        Vector3 sideAB = pointB - pointA;
        Vector3 sideAC = pointC - pointA;
        Vector3 normal = Vector3.Cross(sideAB, sideAC).normalized;
        return normal;
    }

    public void Finalize()
    {
        if (useFlatShading)
        {
            FlatShading();
        }
        else
        {
            BakedNormals();
        }
    }
     void BakedNormals()
    {
        bakedNormals = CalculateNormals();
    }

    void FlatShading()
    {
        Vector3[] flatShadedVertices = new Vector3[triangles.Length];
        Vector2[] flatShadedUvs = new Vector2[triangles.Length];
        for (int i = 0; i < triangles.Length; i++)
        {
            flatShadedVertices[i] = vertices[triangles[i]];
            flatShadedUvs[i] = uvs[triangles[i]];
            triangles[i] = i;
        }
        vertices = flatShadedVertices;
        uvs = flatShadedUvs;
    }
    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        if (useFlatShading)
            mesh.normals = CalculateNormals();
        else
            mesh.normals = bakedNormals;
        return mesh;
    }
}
