using UnityEngine;

namespace SleekRender
{
	public static class HelperExtensions
	{
		public static void DestroyImmediateIfNotNull(this Object obj)
        {
            if (obj != null)
            {
                Object.DestroyImmediate(obj);
            }
        }

		public static RenderTexture CreateTransientRenderTexture(string textureName, int width, int height)
        {
            var renderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
            renderTexture.name = textureName;
            renderTexture.filterMode = FilterMode.Bilinear;
            renderTexture.wrapMode = TextureWrapMode.Clamp;
            return renderTexture;
        }

        public static Material CreateMaterialFromShader(string shaderName)
        {
            var shader = Shader.Find(shaderName);
            return new Material(shader);
        }
	}
}