using UnityEngine;

namespace SleekRender
{
    public class PassRenderer
    {
        private Mesh _fullscreenQuadMesh;   

        public PassRenderer()
        {
            _fullscreenQuadMesh = CreateScreenSpaceQuadMesh();
        }

		public void Blit(Texture source, RenderTexture destination, Material material, int materialPass = 0)
        {
            SetActiveRenderTextureAndClear(destination);
            DrawFullscreenQuad(source, material, materialPass);
        }

		public void DrawFullscreenQuad(Texture source, Material material, int materialPass = 0)
        {
            material.SetTexture(Uniforms._MainTex, source);
            material.SetPass(materialPass);
            Graphics.DrawMeshNow(_fullscreenQuadMesh, Matrix4x4.identity);
        }
		
        public void SetActiveRenderTextureAndClear(RenderTexture destination)
        {
            RenderTexture.active = destination;
            GL.Clear(true, true, new Color(1f, 0.75f, 0.5f, 0.8f));
        }
        private Mesh CreateScreenSpaceQuadMesh()
        {
            var mesh = new Mesh();

            var vertices = new[]
            {
                new Vector3 (-1f, -1f, 0f), // BL
                new Vector3 (-1f, 1f, 0f), // TL
                new Vector3 (1f, 1f, 0f), // TR
                new Vector3 (1f, -1f, 0f) // BR
            };

            var uvs = new[]
            {
                new Vector2 (0f, 0f),
                new Vector2 (0f, 1f),
                new Vector2 (1f, 1f),
                new Vector2 (1f, 0f)
            };

            var colors = new[]
            {
                new Color (0f, 0f, 1f),
                new Color (0f, 1f, 1f),
                new Color (1f, 1f, 1f),
                new Color (1f, 0f, 1f),
            };

            var triangles = new[]
            {
                0,
                2,
                1,
                0,
                3,
                2
            };

            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.colors = colors;
            mesh.UploadMeshData(true);

            return mesh;
        }
    }
}
