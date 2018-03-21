#define FXPRO_EFFECT
//#define BLOOMPRO_EFFECT
//#define DOFPRO_EFFECT

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if FXPRO_EFFECT
namespace FxProNS {
#elif BLOOMPRO_EFFECT
namespace BloomProNS {
#elif DOFPRO_EFFECT
namespace DOFProNS {
#endif
	public enum EffectsQuality
	{
	    High,
	    Normal,
	    Fast,
	    Fastest
	}

    public abstract class Singleton<T>
        				where T : class, new() {

        private static bool Compare<U>( U x, U y ) where U : class {
            return x == y;
        }

        #region Singleton

        private static T instance = default( T );

        public static T Instance {
            get {
                if (Compare<T>( default( T ), instance )) {
                    instance = new T();
                }

                return instance;
            }
        }

        #endregion
    }

	public class RenderTextureManager : IDisposable
	{
	    private static RenderTextureManager instance;
	    public static RenderTextureManager Instance
	    {
	        get
	        {
	            return instance ?? (instance = new RenderTextureManager());
	        }
	    }
	
		private List<RenderTexture> allRenderTextures = null;
		private List<RenderTexture> availableRenderTextures = null;
	
	    //	public RenderTexture RequestRenderTexture(int _width, int _height, int _depth, RenderTextureFormat _format) {
	//		return RenderTexture.GetTemporary( _width, _height, _depth, _format );
	//	}
	//	
	//	public RenderTexture ReleaseRenderTexture( RenderTexture _tex ) {
	//		RenderTexture.ReleaseTemporary( _tex );
	//		
	//		return null;
	//	}
		
		public RenderTexture RequestRenderTexture(int _width, int _height, int _depth, RenderTextureFormat _format) {
			if (null == allRenderTextures)
				allRenderTextures = new List<RenderTexture>();
		
			if (null == availableRenderTextures)
				availableRenderTextures = new List<RenderTexture>();
		
			//First look for an available RenderTexture
			RenderTexture tempTex = null;
			
			foreach (RenderTexture rt in availableRenderTextures) {
				if (null == rt)
					continue;
			
				if (rt.width == _width && rt.height == _height && rt.depth == _depth && rt.format == _format) {
					tempTex = rt;
				}
			}
			
			if (null != tempTex) {
				MakeRenderTextureNonAvailable( tempTex );
				
	//			PrintRenderTextureStats();
				tempTex.DiscardContents();
				return tempTex;
			}
			
			//Create a new texture if it was not found.
			tempTex = CreateNewTexture( _width, _height, _depth, _format );
			MakeRenderTextureNonAvailable( tempTex );
	//		PrintRenderTextureStats();
			
			return tempTex;
		}
		
		public RenderTexture ReleaseRenderTexture( RenderTexture _tex ) {
	//		Debug.Log( "<color=cyan>ReleaseRenderTexture: " + RenderTexToString(_tex) + "</color>" );
		
			if (null == _tex || null == availableRenderTextures)
				return null;
		
			if ( availableRenderTextures.Contains( _tex ) ) {
	//			Debug.Log( "<color=red>Already available</color>" );
				return null;
			}
			
			availableRenderTextures.Add( _tex );
			
			return null;
		}
		
	    /// <summary>
	    /// Releases a, and then assigns b to a (a = b).
	    /// </summary>
	    /// <param name="a"></param>
	    /// <param name="b"></param>
	    public void SafeAssign( ref RenderTexture a, RenderTexture b )
	    {
	        if ( a == b )
	            return;

            ReleaseRenderTexture( a );
		    a = b;
	
	        //return b;
	    }
		
		public void MakeRenderTextureNonAvailable ( RenderTexture _tex ) {
	//		Debug.Log("<color=blue>MakeRenderTextureNonAvailable</color>: " + RenderTexToString( _tex ) );
			if ( availableRenderTextures.Contains (_tex ) )
				availableRenderTextures.Remove( _tex );
		}
		
		
		private RenderTexture CreateNewTexture( int _width, int _height, int _depth, RenderTextureFormat _format ) {
			RenderTexture newTexture = new RenderTexture( _width, _height, _depth, _format );
			newTexture.Create();
			
			allRenderTextures.Add( newTexture );
			availableRenderTextures.Add( newTexture );
			
	//		Debug.Log("<color=green>CreateNewTexture: " + RenderTexToString(newTexture) + "</color>");
			
			return newTexture;
		}
		
		public void PrintRenderTextureStats() {
		    string resString = "<color=blue>availableRenderTextures: </color>" + availableRenderTextures.Count + "\n";
			foreach (RenderTexture rt in availableRenderTextures) {
				resString += "\t" + RenderTexToString( rt ) + "\n";
			}
			
			Debug.Log(resString);
		
			resString = "<color=green>allRenderTextures:</color>" + allRenderTextures.Count + "\n";
			foreach (RenderTexture rt in allRenderTextures) {
				resString += "\t" + RenderTexToString( rt ) + "\n";
			}
			
			Debug.Log(resString);
		}
		
		private string RenderTexToString( RenderTexture _rt ) {
			if (null == _rt)
				return "null";
				
			return _rt.width + " x " + _rt.height + "\t" + _rt.depth + "\t" + _rt.format;
		}
		
		private void PrintRenderTexturesCount(string _prefix = "") {
			Debug.Log(_prefix + ": " + (allRenderTextures.Count - availableRenderTextures.Count) + "/" + allRenderTextures.Count);
		}
		
	    //Should be called every frame to make sure that we don't hold on to render textures that are no longer used.
		public void ReleaseAllRenderTextures() {
			if (null == allRenderTextures)
				return;
		
			foreach (RenderTexture rt in allRenderTextures) {
				if ( !availableRenderTextures.Contains( rt ) ) {
	//				Debug.Log("<color=red>RT not released: " + RenderTexToString(rt) + "</color>" );
					ReleaseRenderTexture( rt );
				}
			}
		}
	
	    public void PrintBalance()
	    {
	        Debug.Log( "RenderTextures balance: " + (allRenderTextures.Count - availableRenderTextures.Count) + "/" + allRenderTextures.Count );
	    }
		
		public void Dispose() {
	//        Debug.Log("<color=red>Dispose</color>");
			
			if (null != allRenderTextures) {
				foreach (RenderTexture rt in allRenderTextures) {
	//				Debug.Log("Releasing " + RenderTexToString(rt) );
					rt.Release();
				}
			
				allRenderTextures.Clear();
			}
			
			if (null != availableRenderTextures) {
				availableRenderTextures.Clear();
			}
		}
	}
}