using UnityEngine;
using System.Collections;
using UnityEditor;

public static class GenerateScreenSpaceMesh 
{
    [MenuItem("Assets/Create/Screen space mesh")]
    public static void CreateScreeSpaceMesh()
    {
        var mesh = new Mesh();
        var verts = new Vector3[4];
        var tris = new int[6];
        var uvs = new Vector2[4];

        // We will use these raw screenspace coordinates without additional projection matrices
        verts[0] = new Vector3(-1f, -1f, 1f);
        verts[1] = new Vector3(-1f, 1f, 1f);
        verts[2] = new Vector3(1f, 1f, 1f);
        verts[3] = new Vector3(1f, -1f, 1f);

        tris[0] = 0;
        tris[1] = 2;
        tris[2] = 1;

        tris[3] = 0;
        tris[4] = 3;
        tris[5] = 2;

        uvs[0] = new Vector2(0f, 0f);    
        uvs[1] = new Vector2(0f, 1f);    
        uvs[2] = new Vector2(1f, 1f);    
        uvs[3] = new Vector2(1f, 0f);    

        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.uv = uvs;

        AssetDatabase.CreateAsset(mesh, "Assets/screenspaceMesh.asset");
    }
}
