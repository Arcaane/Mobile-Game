using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabLightmapData : MonoBehaviour
{
    [System.Serializable]
    	struct RendererInfo
    	{
    		public Renderer 	renderer;
    		public int 			lightmapIndex;
    		public Vector4 		lightmapOffsetScale;
    	}
    
    	[SerializeField]
    	RendererInfo[]	m_RendererInfo;
    	[SerializeField]
    	Texture2D[] 	m_Lightmaps;
    
    	void Awake ()
    	{
    		if (m_RendererInfo == null || m_RendererInfo.Length == 0)
    			return;
    
    		var lightmaps = LightmapSettings.lightmaps;
    		List<LightmapData> combinedLightmaps = new List<LightmapData>();
    		for(int i = 0; i < lightmaps.Length; i++)
    		{
    			combinedLightmaps.Add(lightmaps[i]);
    		}
    
    		for (int i = 0; i < m_Lightmaps.Length;i++)
    		{
    			int index = -1;
    			for(int j = 0; j < lightmaps.Length; j++)
    			{
    				if(lightmaps[j].lightmapColor.GetHashCode() == m_Lightmaps[i].GetHashCode())
    				{
    					index = j;
    					break;
    				}
    			}
    			
    			if(index == -1)
    			{
    				LightmapData newLMD = new LightmapData();
    				newLMD.lightmapColor = m_Lightmaps[i];
    				combinedLightmaps.Add(newLMD);
    			}
    			else{
    				m_RendererInfo[i].lightmapIndex = index;
    			}
    		}
    
    		ApplyRendererInfo(m_RendererInfo, 0);
    		LightmapSettings.lightmaps = combinedLightmaps.ToArray();
    
    	}
    
    	
    	static void ApplyRendererInfo (RendererInfo[] infos, int lightmapOffsetIndex)
    	{
    		for (int i=0;i<infos.Length;i++)
    		{
    			var info = infos[i];
    			info.renderer.lightmapIndex = info.lightmapIndex + lightmapOffsetIndex;
    			info.renderer.lightmapScaleOffset = info.lightmapOffsetScale;
    		}
    	}
    
    #if UNITY_EDITOR
    	[UnityEditor.MenuItem("Assets/Bake Prefab Lightmaps")]
    	static void GenerateLightmapInfo ()
    	{
    		if (UnityEditor.Lightmapping.giWorkflowMode != UnityEditor.Lightmapping.GIWorkflowMode.OnDemand)
    		{
    			Debug.LogError("ExtractLightmapData requires that you have baked you lightmaps and Auto mode is disabled.");
    			return;
    		}
    		UnityEditor.Lightmapping.Bake();
    
    		PrefabLightmapData[] prefabs = FindObjectsOfType<PrefabLightmapData>();
    
    		foreach (var instance in prefabs)
    		{
    			var gameObject = instance.gameObject;
    			var rendererInfos = new List<RendererInfo>();
    			var lightmaps = new List<Texture2D>();
    			
    			GenerateLightmapInfo(gameObject, rendererInfos, lightmaps);
    			
    			instance.m_RendererInfo = rendererInfos.ToArray();
    			instance.m_Lightmaps = lightmaps.ToArray();
    
    			var targetPrefab = UnityEditor.PrefabUtility.GetPrefabParent(gameObject) as GameObject;
    			if (targetPrefab != null)
    			{
    				//UnityEditor.Prefab
    				UnityEditor.PrefabUtility.ReplacePrefab(gameObject, targetPrefab);
    			}
    		}
    	}
    
    	static void GenerateLightmapInfo (GameObject root, List<RendererInfo> rendererInfos, List<Texture2D> lightmaps)
    	{
    		var renderers = root.GetComponentsInChildren<MeshRenderer>();
    		foreach (MeshRenderer renderer in renderers)
    		{
    			if (renderer.lightmapIndex != -1)
    			{
    				RendererInfo info = new RendererInfo();
    				info.renderer = renderer;
    				info.lightmapOffsetScale = renderer.lightmapScaleOffset;
    
    				Texture2D lightmap = LightmapSettings.lightmaps[renderer.lightmapIndex].lightmapColor;
    
    				info.lightmapIndex = lightmaps.IndexOf(lightmap);
    				if (info.lightmapIndex == -1)
    				{
    					info.lightmapIndex = lightmaps.Count;
    					lightmaps.Add(lightmap);
    				}
    
    				rendererInfos.Add(info);
    			}
    		}
    	}
    #endif
    
    }
