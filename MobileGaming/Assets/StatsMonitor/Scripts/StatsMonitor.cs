// 
// Copyright © Hexagon Star Softworks. All Rights Reserved.
// http://www.hexagonstar.com/
// 
// StatsMonitor.cs - version 1.0.0 - Created 11/12/17 23:26
//  

using UnityEngine;
using UnityEngine.SceneManagement;


namespace StatsMonitor
{
	// ----------------------------------------------------------------------------------------
	// Public Enums
	// ----------------------------------------------------------------------------------------

	public enum StatsMonitorStatus : byte
	{
		Active,
		Inactive,
		Passive
	}

	public enum StatsMonitorLayout : byte
	{
		Minimal,
		StatsOnly,
		Standard,
		Full
	}

	public enum StatsMonitorStyle : byte
	{
		Default,
		Plain,
		Opaque,
		Oblique
	}


	/// <summary>
	///		A wrapper object into which the actual stats monitor widget is added as a child.
	/// </summary>
	[DisallowMultipleComponent]
	public sealed class StatsMonitor : MonoBehaviour
	{
		// ----------------------------------------------------------------------------------------
		// Constants
		// ----------------------------------------------------------------------------------------

		public const string NAME                = "Stats Monitor";
		public const string VERSION             = "1.3.7";
		public const string WRAPPER_OBJECT_NAME = NAME;
		public const string WIDGET_OBJECT_NAME  = "Stats Monitor Widget";


		// ----------------------------------------------------------------------------------------
		// Properties
		// ----------------------------------------------------------------------------------------

		public static StatsMonitor instance { get; private set; }
		
		private bool _isFirstScene = true;
		private StatsMonitorWidget _widget;
		private Canvas _canvas;


		// ----------------------------------------------------------------------------------------
		// Accessors
		// ----------------------------------------------------------------------------------------

		public static StatsMonitorWidget Widget
		{
			get { return instance != null ? instance._widget : null; }
		}


		// ----------------------------------------------------------------------------------------
		// Public Methods
		// ----------------------------------------------------------------------------------------

		public void SetRenderMode(UIRenderMode renderMode)
		{
			if (renderMode == UIRenderMode.Overlay)
			{
				_canvas.renderMode = RenderMode.ScreenSpaceOverlay;
				_canvas.sortingOrder = short.MaxValue;
			}
			else if (renderMode == UIRenderMode.Camera)
			{
				var cam = Camera.current ? Camera.current : Camera.main;
				_canvas.renderMode = RenderMode.ScreenSpaceCamera;
				_canvas.worldCamera = cam;
				_canvas.planeDistance = cam.nearClipPlane;
				
				/* Set SM to be on the front-most layer since we can't create a sorting layer via script. */
				_canvas.sortingLayerName = SortingLayer.layers[SortingLayer.layers.Length - 1].name;
				_canvas.sortingOrder = short.MaxValue;
			}
		}


		// ----------------------------------------------------------------------------------------
		// Private/Protected Methods
		// ----------------------------------------------------------------------------------------

		private void CreateUI()
		{
			/* Create UI canvas used for all StatsMonitor components. */
			_canvas = gameObject.AddComponent<Canvas>();
			_canvas.pixelPerfect = true;

			/* Adjust StatsMonitor rect transform. */
			var tr = gameObject.GetComponent<RectTransform>();
			tr.pivot = Vector2.up;
			tr.anchorMin = Vector2.up;
			tr.anchorMax = Vector2.up;
			tr.anchoredPosition = new Vector2(0.0f, 0.0f);

			/* Find the widget object to enable it on start. */
			var widgetTransform = transform.Find(WIDGET_OBJECT_NAME);
			if (widgetTransform == null)
			{
				Debug.LogError(WIDGET_OBJECT_NAME + " not found as a child of " + WRAPPER_OBJECT_NAME + ".");
			}
			else
			{
				widgetTransform.gameObject.SetActive(true);

				/* Find StatsMonitor child object. */
				_widget = FindObjectOfType<StatsMonitorWidget>();
				if (_widget != null) _widget.wrapper = this;
				else Debug.LogError(WIDGET_OBJECT_NAME + " object has no '" + WIDGET_OBJECT_NAME + "' component on it.");
			}
		}


		private void DisposeInternal()
		{
			if (_widget != null) _widget.Dispose();
			Destroy(this);
		}


		// ----------------------------------------------------------------------------------------
		// Unity Callbacks
		// ----------------------------------------------------------------------------------------

		private void Awake()
		{
			if (instance == null) instance = this;

			if (transform.parent != null)
			{
				transform.parent = null;
				Debug.LogWarning("Stats Monitor has been moved to root! Please note that it needs to be in root to function properly. To add Stats Monitor to a scene always use the menu 'Game Object/Create Other/Stats Monitor'.");
			}

			if (_widget == null || _widget.KeepAlive) DontDestroyOnLoad(gameObject);
			CreateUI();
			if (_widget != null) SetRenderMode(_widget.RenderMode);
			StatsMonitorUtil.AddToUILayer(gameObject);
			SceneManager.sceneLoaded += OnSceneLoaded;
		}


		private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			if (_widget == null) return;
			if (!_widget.KeepAlive && !_isFirstScene)
			{
				DisposeInternal();
			}
			else
			{
				_isFirstScene = false;
				_widget.ResetMinMaxFPS();
				_widget.ResetAverageFPS();
			}
		}


		private void OnDestroy()
		{
			SceneManager.sceneLoaded -= OnSceneLoaded;
			Destroy(gameObject);
			if (instance == this) instance = null;
		}
	}
}
