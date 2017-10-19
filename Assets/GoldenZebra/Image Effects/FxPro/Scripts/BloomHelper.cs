#define FXPRO_EFFECT
//#define BLOOMPRO_EFFECT

#if FXPRO_EFFECT
#define BLOOMPRO_EFFECT
#endif

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using Object = UnityEngine.Object;

#if FXPRO_EFFECT
namespace FxProNS {
#elif BLOOMPRO_EFFECT
namespace BloomProNS {
#endif
	[Serializable]
	public class BloomHelperParams {
	    public EffectsQuality Quality;

	    public Color BloomTint = Color.white;
	
	    [Range( 0f, .99f )]
	    public float BloomThreshold = .8f;
	
	    [Range( 0f, 3f )]
	    public float BloomIntensity = 1.5f;
	
	    [Range( 0.01f, 3f )]
	    public float BloomSoftness = .5f;
	}
	
	public class BloomHelper : Singleton<BloomHelper>, IDisposable
	{
        private static Material _mat;

        public static Material Mat
        {
            get
            {
                if (null == _mat)
                    _mat = new Material( Shader.Find("Hidden/BloomPro") )  {
                        hideFlags = HideFlags.HideAndDontSave
                    };

                return _mat;
            }
        }
	
	    private BloomHelperParams p;
	
	    private int bloomSamples = 5;
	
	    private float bloomBlurRadius = 5f;
	
	    public void SetParams( BloomHelperParams _p ) {
	        p = _p;
	    }
	
	    public void Init() {
	
	        float realBloomIntensity = Mathf.Exp( p.BloomIntensity ) - 1f;
	
	        if (Application.platform == RuntimePlatform.IPhonePlayer)
					p.BloomThreshold *= .75f;

            Mat.SetFloat( "_BloomThreshold", p.BloomThreshold );
            Mat.SetFloat( "_BloomIntensity", realBloomIntensity );

            Mat.SetColor( "_BloomTint", p.BloomTint );

//	        Debug.Log( p.BloomTint );
	
	        //Bloom samples settings
	        if (p.Quality == EffectsQuality.High || p.Quality == EffectsQuality.Normal) {
	            bloomSamples = 5;
                Mat.EnableKeyword( "BLOOM_SAMPLES_5" );
                Mat.DisableKeyword( "BLOOM_SAMPLES_3" );
	        } if (p.Quality == EffectsQuality.Fast || p.Quality == EffectsQuality.Fastest) {
	            bloomSamples = 3;
                Mat.EnableKeyword( "BLOOM_SAMPLES_3" );
                Mat.DisableKeyword( "BLOOM_SAMPLES_5" );
	        }
	
	        //Blur radius settings
	        if (p.Quality == EffectsQuality.High) {
	            bloomBlurRadius = 10f;
                Mat.EnableKeyword( "BLUR_RADIUS_10" );
                Mat.DisableKeyword( "BLUR_RADIUS_5" );
	        } else {
	            bloomBlurRadius = 5f;
                Mat.EnableKeyword( "BLUR_RADIUS_5" );
                Mat.DisableKeyword( "BLUR_RADIUS_10" );
	        }
	
	        float[] bloomTexFactors = CalculateBloomTexFactors( Mathf.Exp( p.BloomSoftness ) - 1f );
	
	        if (bloomTexFactors.Length == 5) {
                Mat.SetVector( "_BloomTexFactors1",
	                new Vector4( bloomTexFactors[0], bloomTexFactors[1], bloomTexFactors[2], bloomTexFactors[3] ) );
                Mat.SetVector( "_BloomTexFactors2", new Vector4( bloomTexFactors[4], 0f, 0f, 0f ) );
	        } else if (bloomTexFactors.Length == 3) {
                Mat.SetVector(
	                "_BloomTexFactors1",
	                new Vector4( bloomTexFactors[0], bloomTexFactors[1], bloomTexFactors[2], 0f ) );
	        } else {
	            Debug.LogError( "Unsupported bloomTexFactors.Length: " + bloomTexFactors.Length );
	        }
	
	        RenderTextureManager.Instance.Dispose();
	    }
	
	    public void RenderBloomTexture( RenderTexture source, RenderTexture dest )
	    {
	        RenderTexture curRenderTex = RenderTextureManager.Instance.RequestRenderTexture( source.width, source.height, source.depth, source.format );
	
	        //Prepare by extracting blooming pixels
            Graphics.Blit( source, curRenderTex, Mat, 0 );  //Is it ok to extract blooming pixels at 1/4 res??????
	
	        //Downsample & blur
	        for (int i = 1; i <= bloomSamples; i++) {
	            float curSpread = Mathf.Lerp( 1f, 2f, (float)(i - 1) / (float)(bloomSamples) );
	
				#if FXPRO_EFFECT
	            RenderTextureManager.Instance.SafeAssign( ref curRenderTex, FxPro.DownsampleTex( curRenderTex, 2f ) );
				#elif BLOOMPRO_EFFECT
				RenderTextureManager.Instance.SafeAssign( ref curRenderTex, BloomPro.DownsampleTex( curRenderTex, 2f ) );
				#elif DOFPRO_EFFECT
				RenderTextureManager.Instance.SafeAssign( ref curRenderTex, DOFPro.DownsampleTex( curRenderTex, 2f ) );
				#endif
	
	            RenderTextureManager.Instance.SafeAssign( ref curRenderTex, BlurTex( curRenderTex, curSpread ) );
	
	            //Set blurred bloom texture
                Mat.SetTexture( "_DsTex" + i, curRenderTex );
	        }
	
	        //Bloom composite pass
            Graphics.Blit( null, dest, Mat, 1 );
	
	        RenderTextureManager.Instance.ReleaseRenderTexture(curRenderTex);
	    }
	
	
	
	    public RenderTexture BlurTex( RenderTexture _input, float _spread ) {
	        //Blur
	        float curBlurSize = _spread * 10f / bloomBlurRadius;     //Normalization - smaller blur = higher step
	
	        RenderTexture tempRenderTex1 = RenderTextureManager.Instance.RequestRenderTexture( _input.width, _input.height, _input.depth, _input.format );
	        RenderTexture tempRenderTex2 = RenderTextureManager.Instance.RequestRenderTexture( _input.width, _input.height, _input.depth, _input.format );
	
	        //Horizontal blur
            Mat.SetVector( "_SeparableBlurOffsets", new Vector4( 1f, 0f, 0f, 0f ) * curBlurSize );
            Graphics.Blit( _input, tempRenderTex1, Mat, 2 );
	
	        //Vertical blur
            Mat.SetVector( "_SeparableBlurOffsets", new Vector4( 0f, 1f, 0f, 0f ) * curBlurSize );
            Graphics.Blit( tempRenderTex1, tempRenderTex2, Mat, 2 );
	
	        tempRenderTex1 = RenderTextureManager.Instance.ReleaseRenderTexture( tempRenderTex1 );
	
	        return tempRenderTex2;
	    }
	
	    private float[] CalculateBloomTexFactors( float softness ) {
	        var bloomTexFactors = new float[bloomSamples];
	        for (int i = 0; i < bloomTexFactors.Length; i++) {
	            float t = (float)(i) / (float)(bloomTexFactors.Length - 1);
	
	            bloomTexFactors[i] = Mathf.Lerp( 1f, softness, t );
	        }
	
	        bloomTexFactors = MakeSumOne( bloomTexFactors );
	
	        return bloomTexFactors;
	    }
	
	    private float[] MakeSumOne( IList<float> _in ) {
	        float sum = _in.Sum();
	
	        var res = new float[_in.Count];
	
	        for (int i = 0; i < _in.Count; i++) {
	            res[i] = _in[i] / sum;
	        }
	
	        return res;
	    }
	
	    public void Dispose() {
            if ( null != Mat )
                Object.DestroyImmediate( Mat );
	
	        RenderTextureManager.Instance.Dispose();
	    }
	}
}