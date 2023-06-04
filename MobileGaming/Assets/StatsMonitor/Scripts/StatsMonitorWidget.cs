// 
// Copyright © Hexagon Star Softworks. All Rights Reserved.
// http://www.hexagonstar.com/
// 
// StatsMonitorWidget.cs - version 1.0.0 - Created 2017/11/14 12:13
//  

using System;
using System.Collections;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;
using Object = UnityEngine.Object;


namespace StatsMonitor
{
	/// <summary>
	///		Provides the main functionailty for the stats monitor.
	/// </summary>
	[DisallowMultipleComponent]
	public sealed class StatsMonitorWidget : MonoBehaviour
	{
		// ----------------------------------------------------------------------------------------
		// Enums
		// ----------------------------------------------------------------------------------------

		internal enum StatsMonitorInvalidationFlag : byte
		{
			Any,
			Text,
			Graph,
			Background,
			Scale
		}
		
		private enum StatsMonitorViewType : byte
		{
			Background,
			FPSView,
			StatsView,
			GraphView,
			SysInfoView
		}


		// ----------------------------------------------------------------------------------------
		// Constants
		// ----------------------------------------------------------------------------------------

		private const float MEMORY_DIVIDER = 1048576.0f; // 1024^2
		private const int MINMAX_SKIP_INTERVALS = 3;


		// ----------------------------------------------------------------------------------------
		// Properties
		// ----------------------------------------------------------------------------------------

		/* General parameters */
		[SerializeField] private StatsMonitorStatus _status                        = StatsMonitorStatus.Active;
		[SerializeField] private UIRenderMode _renderMode                          = UIRenderMode.Overlay;
		[SerializeField] private StatsMonitorLayout _layout                        = StatsMonitorLayout.Standard;
		[SerializeField] private StatsMonitorStyle _style                          = StatsMonitorStyle.Default;
		[SerializeField] private UIAlignment _alignment                            = UIAlignment.UpperRight;
		[SerializeField] private bool _keepAlive                                   = true;
		[SerializeField] [Range(0.1f, 10.0f)] private float _statsUpdateInterval   = 1.0f;
		[SerializeField] [Range(0.01f, 10.0f)] private float _graphUpdateInterval  = 0.05f;
		[SerializeField] [Range(0, 10)] internal int _objectsCountInterval;

		public bool gcMonitoringEnabled;
		
		/* Hot keys */
		public bool inputEnabled = true;

		public KeyCode modKeyToggle    = KeyCode.LeftShift;
		public KeyCode hotKeyToggle    = KeyCode.BackQuote;
		public KeyCode modKeyAlignment = KeyCode.LeftControl;
		public KeyCode hotKeyAlignment = KeyCode.BackQuote;
		public KeyCode modKeyStyle     = KeyCode.LeftAlt;
		public KeyCode hotKeyStyle     = KeyCode.BackQuote;

		/* Touch Control */
		[Range(0, 5)] public int toggleTouchCount        = 3;
		[Range(0, 5)] public int switchAlignmentTapCount = 3;
		[Range(0, 5)] public int switchStyleTapCount     = 3;

		/* Look and feel */
		[SerializeField] [Range(8, 128)] internal int fontSizeLarge = 32;
		[SerializeField] [Range(8, 128)] internal int fontSizeSmall = 16;
		[SerializeField] [Range(0, 100)] internal int padding       = 4;
		[SerializeField] [Range(0, 100)] internal int spacing       = 2;
		[SerializeField] [Range(10, 400)] internal int graphHeight  = 40;
		[SerializeField] [Range(1, 10)] internal int scale          = 1;

		[SerializeField] internal Font fontFace;
		[SerializeField] internal bool autoScale;
		
		[SerializeField] internal Color colorBGLower     = StatsMonitorUtil.HexToColor32("002525C8");
		[SerializeField] internal Color colorBGUpper     = StatsMonitorUtil.HexToColor32("00314ABE");
		[SerializeField] internal Color colorFPS         = Color.white;
		[SerializeField] internal Color colorFPSAvg      = StatsMonitorUtil.HexToColor32("00C8DCFF");
		[SerializeField] internal Color colorFPSCritical = StatsMonitorUtil.HexToColor32("FF0000FF");
		[SerializeField] internal Color colorFPSMax      = StatsMonitorUtil.HexToColor32("CCCCCCFF");
		[SerializeField] internal Color colorFPSMin      = StatsMonitorUtil.HexToColor32("999999FF");
		[SerializeField] internal Color colorFPSWarning  = StatsMonitorUtil.HexToColor32("FFA000FF");
		[SerializeField] internal Color colorFXD         = StatsMonitorUtil.HexToColor32("C68D00FF");
		[SerializeField] internal Color colorGCBlip      = StatsMonitorUtil.HexToColor32("00FF00FF");
		[SerializeField] internal Color colorGraphBG     = StatsMonitorUtil.HexToColor32("00800010");
		[SerializeField] internal Color colorMemAlloc    = StatsMonitorUtil.HexToColor32("B480FFFF");
		[SerializeField] internal Color colorMemMono     = StatsMonitorUtil.HexToColor32("FF66D1FF");
		[SerializeField] internal Color colorMemTotal    = StatsMonitorUtil.HexToColor32("4080FFFF");
		[SerializeField] internal Color colorMS          = StatsMonitorUtil.HexToColor32("C8C820FF");
		[SerializeField] internal Color colorObjCount    = StatsMonitorUtil.HexToColor32("00B270FF");
		[SerializeField] internal Color colorOutline     = StatsMonitorUtil.HexToColor32("00000000");
		[SerializeField] internal Color colorSysInfoEven = StatsMonitorUtil.HexToColor32("A5D6FFFF");
		[SerializeField] internal Color colorSysInfoOdd  = StatsMonitorUtil.HexToColor32("D2EBFFFF");

		/* FPS-specific */
		[SerializeField] private bool _throttleFrameRate;

		[SerializeField] [Range(-1, 200)] private int _throttledFrameRate = -1;
		[SerializeField] [Range(0,  100)] private int _avgSamples         = 50;
		[SerializeField] [Range(1,  200)] private int _warningThreshold   = 40;
		[SerializeField] [Range(1,  200)] private int _criticalThreshold  = 20;

		/* Calculated values */
		public float fixedUpdateRate { get; private set; }

		public float memAlloc            { get; private set; }
		public float memMono             { get; private set; }
		public float memTotal            { get; private set; }
		public float ms                  { get; private set; }
		public int   fps                 { get; private set; }
		public int   fpsAvg              { get; private set; }
		public int   fpsMax              { get; private set; }
		public int   fpsMin              { get; private set; }
		public int   objectCount         { get; private set; }
		public int   renderedObjectCount { get; private set; }
		public int   renderObjectCount   { get; private set; }

		internal StatsMonitor wrapper;
		internal int fpsLevel;
		
		private bool    _isDisposed;
		private bool    _isInitialized;
		private float   _actualUpdateInterval;
		private float   _currentAVGRaw;
		private float   _fpsNew;
		private float   _intervalTimeCount2;
		private float   _intervalTimeCount;
		private float[] _accAVGSamples;
		private int     _cachedFrameRate         = -1;
		private int     _cachedVSync             = -1;
		private int     _currentAVGSamples;
		private int     _minMaxIntervalsSkipped;
		private int     _totalHeight;
		private int     _totalWidth;

		private readonly UIAnchors _anchors = new UIAnchors();
		private UIAnchor    _anchor;
		private RawImage    _background;
		private Texture2D   _gradient;
		private FPSView     _fpsView;
		private StatsView   _statsView;
		private GraphView   _graphView;
		private SysInfoView _sysInfoView;

#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
		// ReSharper disable once UnusedMember.Local
		private Vector2 _touchOrigin = -Vector2.one;
		// ReSharper disable once NotAccessedField.Local
		private Rect _touchArea;
#endif

#if UNITY_EDITOR
		[HideInInspector] [SerializeField] internal bool hotKeyGroupToggle;
		[HideInInspector] [SerializeField] internal bool touchControlGroupToggle;
		[HideInInspector] [SerializeField] internal bool layoutAndStylingToggle = true;
		[HideInInspector] [SerializeField] internal bool fpsGroupToggle = true;
#endif


		// ----------------------------------------------------------------------------------------
		// Accessors
		// ----------------------------------------------------------------------------------------

		/// <summary>
		///		Determines the status that Stats Monitor is running in.
		/// 
		///		Active: (Default) The stats monitor is drawn and operates in the
		///		foreground.
		///		Inactive: Disables the stats monitor completely except for hotkeys
		///		checking.
		///		Passive: Doesn't draw the stats monitor but still polls stats in the
		///		background. Useful for hidden performance monitoring.
		/// </summary>
		public StatsMonitorStatus Status
		{
			get { return _status; }
			set
			{
				if (_status == value || !Application.isPlaying) return;
				_status = value;
				if (!enabled) return;
				if (_status != StatsMonitorStatus.Inactive)
				{
					OnEnable();
					UpdateData();
					UpdateView();
				}
				else
				{
					OnDisable();
				}
			}
		}

		/// <summary>
		///		Determines the mode that Stats Monitor is rendered in.
		/// </summary>
		public UIRenderMode RenderMode
		{
			get { return _renderMode; }
			set
			{
				if (_renderMode == value || !Application.isPlaying) return;
				_renderMode = value;
				if (!enabled) return;
				wrapper.SetRenderMode(_renderMode);
			}
		}

		/// <summary>
		///		The layout style of Stats Monitor.
		/// 
		///		Minimal: Displays only the FPS counter.
		///		StatsOnly: Displays only the textual stats section.
		///		Standard: (Default) Displays the textual stats section and the graph
		///		section.
		///		Full: Displays the textual stats section, the graph section, and the
		///		sysinfo section.
		/// </summary>
		public StatsMonitorLayout Layout
		{
			get { return _layout; }
			set
			{
				if (_layout == value || !Application.isPlaying) return;
				_layout = value;
				if (!enabled) return;
				CreateChildren();
				UpdateData();
				UpdateView();
			}
		}

		/// <summary>
		///		The visual style preset of Stats Monitor. Allows to select several default styles that
		///		affect the background color alpha and text/graph effects.
		/// </summary>
		public StatsMonitorStyle Style
		{
			get { return _style; }
			set
			{
				if (_style == value) return;
				_style = value;
				switch (_style)
				{
					case StatsMonitorStyle.Default:
						colorBGUpper.a = StatsMonitorUtil.Normalize(190.0f);
						colorBGLower.a = StatsMonitorUtil.Normalize(200.0f);
						colorGraphBG.a = StatsMonitorUtil.Normalize(16.0f);
						colorOutline.a = StatsMonitorUtil.Normalize(0.0f);
						break;
					case StatsMonitorStyle.Plain:
						colorBGUpper.a = StatsMonitorUtil.Normalize(0.0f);
						colorBGLower.a = StatsMonitorUtil.Normalize(0.0f);
						colorGraphBG.a = StatsMonitorUtil.Normalize(0.0f);
						colorOutline.a = StatsMonitorUtil.Normalize(0.0f);
						break;
					case StatsMonitorStyle.Opaque:
						colorBGUpper.a = StatsMonitorUtil.Normalize(255.0f);
						colorBGLower.a = StatsMonitorUtil.Normalize(255.0f);
						colorGraphBG.a = StatsMonitorUtil.Normalize(255.0f);
						colorOutline.a = StatsMonitorUtil.Normalize(0.0f);
						break;
					case StatsMonitorStyle.Oblique:
						colorBGUpper.a = StatsMonitorUtil.Normalize(0.0f);
						colorBGLower.a = StatsMonitorUtil.Normalize(0.0f);
						colorGraphBG.a = StatsMonitorUtil.Normalize(0.0f);
						colorOutline.a = StatsMonitorUtil.Normalize(190.0f);
						break;
				}

				if (_isDisposed || !Application.isPlaying || !enabled) return;
				Invalidate(UIInvalidationType.All);
			}
		}

		/// <summary>
		///		Determines the position of Stats Monitor.
		/// </summary>
		public UIAlignment Alignment
		{
			get { return _alignment; }
			set
			{
				if (_alignment == value || !Application.isPlaying) return;
				_alignment = value;
				if (!enabled) return;
				Align(_alignment);
			}
		}

		/// <summary>
		///		If checked prevents Stats Monitor from being destroyed on level (scene)
		///		load.
		/// </summary>
		public bool KeepAlive
		{
			get { return _keepAlive; }
			set { _keepAlive = value; }
		}

		/// <summary>
		///		The time, in seconds at which the text displays are updated.
		/// </summary>
		public float StatsUpdateInterval
		{
			get { return _statsUpdateInterval; }
			set
			{
				if (Mathf.Abs(_statsUpdateInterval - value) < 0.001f || !Application.isPlaying) return;
				_statsUpdateInterval = value;
				if (!enabled) return;
				DetermineActualUpdateInterval();
				RestartCoroutine();
			}
		}

		/// <summary>
		///		The time, in seconds at which the graph is updated.
		/// </summary>
		public float GraphUpdateInterval
		{
			get { return _graphUpdateInterval; }
			set
			{
				if (Mathf.Abs(_graphUpdateInterval - value) < 0.001f || !Application.isPlaying) return;
				_graphUpdateInterval = value;
				if (!enabled) return;
				DetermineActualUpdateInterval();
				RestartCoroutine();
			}
		}

		/// <summary>
		///		The time, in seconds at which objects are counted.
		/// </summary>
		public int ObjectsCountInterval
		{
			get { return _objectsCountInterval; }
			set
			{
				if (_objectsCountInterval < 0 || !Application.isPlaying) return;
				_objectsCountInterval = value;
				if (!enabled) return;
				DetermineActualUpdateInterval();
				RestartCoroutine();
			}
		}

		/// <summary>
		///		The font used for all text fields. Set to null to use the included font.
		/// </summary>
		public Font FontFace
		{
			get { return fontFace; }
			set
			{
				if (fontFace == value || !Application.isPlaying) return;
				fontFace = value;
				if (!enabled) return;
				Invalidate(UIInvalidationType.All, StatsMonitorInvalidationFlag.Text);
			}
		}

		/// <summary>
		///		Font size for all small text.
		/// </summary>
		public int FontSizeSmall
		{
			get { return fontSizeSmall; }
			set
			{
				if (fontSizeSmall == value || !Application.isPlaying) return;
				fontSizeSmall = value;
				if (!enabled) return;
				Invalidate(UIInvalidationType.All, StatsMonitorInvalidationFlag.Text);
			}
		}

		/// <summary>
		///		Font size for the FPS counter.
		/// </summary>
		public int FontSizeLarge
		{
			get { return fontSizeLarge; }
			set
			{
				if (fontSizeLarge == value || !Application.isPlaying) return;
				fontSizeLarge = value;
				if (!enabled) return;
				Invalidate(UIInvalidationType.All, StatsMonitorInvalidationFlag.Text);
			}
		}

		/// <summary>
		///		The padding between the outer edges and text fields.
		/// </summary>
		public int Padding
		{
			get { return padding; }
			set
			{
				if (padding == value || !Application.isPlaying) return;
				padding = value;
				if (!enabled) return;
				Invalidate(UIInvalidationType.All, StatsMonitorInvalidationFlag.Text);
			}
		}

		/// <summary>
		///		The spacing between text fields.
		/// </summary>
		public int Spacing
		{
			get { return spacing; }
			set
			{
				if (spacing == value || !Application.isPlaying) return;
				spacing = value;
				if (!enabled) return;
				Invalidate(UIInvalidationType.All, StatsMonitorInvalidationFlag.Text);
			}
		}

		/// <summary>
		///		Determines the height of the graph.
		/// </summary>
		public int GraphHeight
		{
			get { return graphHeight; }
			set
			{
				if (graphHeight == value || !Application.isPlaying) return;
				graphHeight = value;
				if (!enabled) return;
				Invalidate(UIInvalidationType.All, StatsMonitorInvalidationFlag.Graph);
			}
		}

		/// <summary>
		///		The scaling multiplier.
		/// </summary>
		public int Scale
		{
			get { return scale; }
			set
			{
				if (scale == value || !Application.isPlaying) return;
				scale = value;
				if (!enabled || _status != StatsMonitorStatus.Active) return;
				Invalidate(UIInvalidationType.All, StatsMonitorInvalidationFlag.Scale);
			}
		}

		/// <summary>
		///		If true, scale will be determined according to current screen DPI.
		///		Recommended to leave on for testing on various supported devices that
		///		have large differences in screen resolution.
		/// </summary>
		public bool AutoScale
		{
			get { return autoScale; }
			set
			{
				if (autoScale == value || !Application.isPlaying) return;
				autoScale = value;
				if (!enabled || _status != StatsMonitorStatus.Active) return;
				Invalidate(UIInvalidationType.All, StatsMonitorInvalidationFlag.Scale);
			}
		}

		/// <summary>
		///		The color and transparency of the background panel top.
		/// </summary>
		public Color ColorBgUpper
		{
			get { return colorBGUpper; }
			set
			{
				if (colorBGUpper == value || !Application.isPlaying) return;
				colorBGUpper = value;
				if (!enabled) return;
				Invalidate(UIInvalidationType.Style, StatsMonitorInvalidationFlag.Background);
			}
		}

		/// <summary>
		///		The color and transparency of the background panel bottom.
		/// </summary>
		public Color ColorBgLower
		{
			get { return colorBGLower; }
			set
			{
				if (colorBGLower == value || !Application.isPlaying) return;
				colorBGLower = value;
				if (!enabled) return;
				Invalidate(UIInvalidationType.Style, StatsMonitorInvalidationFlag.Background);
			}
		}

		/// <summary>
		///		The background color and transparency of the graph area.
		/// </summary>
		public Color ColorGraphBG
		{
			get { return colorGraphBG; }
			set
			{
				if (colorGraphBG == value || !Application.isPlaying) return;
				colorGraphBG = value;
				if (!enabled) return;
				Invalidate(UIInvalidationType.Style);
			}
		}

		/// <summary>
		///		Color used for the FPS value.
		/// </summary>
		public Color ColorFPS
		{
			get { return colorFPS; }
			set
			{
				if (colorFPS == value || !Application.isPlaying) return;
				colorFPS = value;
				if (!enabled) return;
				Invalidate(UIInvalidationType.Style);
			}
		}

		/// <summary>
		///		The FPS counter will use this color if FPS falls below the FPS warning
		///		threshold.
		/// </summary>
		public Color ColorFPSWarning
		{
			get { return colorFPSWarning; }
			set
			{
				if (colorFPSWarning == value || !Application.isPlaying) return;
				colorFPSWarning = value;
				if (!enabled) return;
				Invalidate(UIInvalidationType.Style);
			}
		}

		/// <summary>
		///		The FPS counter will use this color if FPS falls below the FPS critical
		///		threshold.
		/// </summary>
		public Color ColorFPSCritical
		{
			get { return colorFPSCritical; }
			set
			{
				if (colorFPSCritical == value || !Application.isPlaying) return;
				colorFPSCritical = value;
				if (!enabled) return;
				Invalidate(UIInvalidationType.Style);
			}
		}

		/// <summary>
		///		Color used for the FPS minimum value.
		/// </summary>
		public Color ColorFPSMin
		{
			get { return colorFPSMin; }
			set
			{
				if (colorFPSMin == value || !Application.isPlaying) return;
				colorFPSMin = value;
				if (!enabled) return;
				Invalidate(UIInvalidationType.Style);
			}
		}

		/// <summary>
		///		Color used for the FPS maximum value.
		/// </summary>
		public Color ColorFPSMax
		{
			get { return colorFPSMax; }
			set
			{
				if (colorFPSMax == value || !Application.isPlaying) return;
				colorFPSMax = value;
				if (!enabled) return;
				Invalidate(UIInvalidationType.Style);
			}
		}

		/// <summary>
		///		Color used for the FPS average value.
		/// </summary>
		public Color ColorFPSAvg
		{
			get { return colorFPSAvg; }
			set
			{
				if (colorFPSAvg == value || !Application.isPlaying) return;
				colorFPSAvg = value;
				if (!enabled) return;
				Invalidate(UIInvalidationType.Style);
			}
		}

		/// <summary>
		///		Color used for Fixed Update framerate value.
		/// </summary>
		public Color ColorFxd
		{
			get { return colorFXD; }
			set
			{
				if (colorFXD == value || !Application.isPlaying) return;
				colorFXD = value;
				if (!enabled) return;
				Invalidate(UIInvalidationType.Style);
			}
		}

		/// <summary>
		///		Color used for the milliseconds value.
		/// </summary>
		public Color ColorMS
		{
			get { return colorMS; }
			set
			{
				if (colorMS == value || !Application.isPlaying) return;
				colorMS = value;
				if (!enabled) return;
				Invalidate(UIInvalidationType.Style);
			}
		}

		/// <summary>
		///		Color used for the garbage collection graph blip.
		/// </summary>
		public Color ColorGCBlip
		{
			get { return colorGCBlip; }
			set
			{
				if (colorGCBlip == value || !Application.isPlaying) return;
				colorGCBlip = value;
				if (!enabled) return;
				Invalidate(UIInvalidationType.Style);
			}
		}

		/// <summary>
		///		Color used for visible renderers and total objects values.
		/// </summary>
		public Color ColorObjectCount
		{
			get { return colorObjCount; }
			set
			{
				if (colorObjCount == value || !Application.isPlaying) return;
				colorObjCount = value;
				if (!enabled) return;
				Invalidate(UIInvalidationType.Style);
			}
		}

		/// <summary>
		///		Color used for the total memory value.
		/// </summary>
		public Color ColorMemTotal
		{
			get { return colorMemTotal; }
			set
			{
				if (colorMemTotal == value || !Application.isPlaying) return;
				colorMemTotal = value;
				if (!enabled) return;
				Invalidate(UIInvalidationType.Style);
			}
		}

		/// <summary>
		///		Color used for the allocated memory value.
		/// </summary>
		public Color ColorMemAlloc
		{
			get { return colorMemAlloc; }
			set
			{
				if (colorMemAlloc == value || !Application.isPlaying) return;
				colorMemAlloc = value;
				if (!enabled) return;
				Invalidate(UIInvalidationType.Style);
			}
		}

		/// <summary>
		///		Color used for the Mono memory value.
		/// </summary>
		public Color ColorMemMono
		{
			get { return colorMemMono; }
			set
			{
				if (colorMemMono == value || !Application.isPlaying) return;
				colorMemMono = value;
				if (!enabled) return;
				Invalidate(UIInvalidationType.Style);
			}
		}

		/// <summary>
		///		Color used for odd sysinfo rows.
		/// </summary>
		public Color ColorSysInfoOdd
		{
			get { return colorSysInfoOdd; }
			set
			{
				if (colorSysInfoOdd == value || !Application.isPlaying) return;
				colorSysInfoOdd = value;
				if (!enabled) return;
				Invalidate(UIInvalidationType.Style);
			}
		}

		/// <summary>
		///		Color used for even sysinfo rows.
		/// </summary>
		public Color ColorSysInfoEven
		{
			get { return colorSysInfoEven; }
			set
			{
				if (colorSysInfoEven == value || !Application.isPlaying) return;
				colorSysInfoEven = value;
				if (!enabled) return;
				Invalidate(UIInvalidationType.Style);
			}
		}

		/// <summary>
		///		Color used for text and graph outline. Setting alpha to 0 will remove
		///		the effect components.
		/// </summary>
		public Color ColorOutline
		{
			get { return colorOutline; }
			set
			{
				if (colorOutline == value || !Application.isPlaying) return;
				colorOutline = value;
				if (!enabled) return;
				Invalidate(UIInvalidationType.All);
			}
		}

		/// <summary>
		///		Enables or disables FPS throttling. This can be used to see how the game
		///		performs under a specific framerate. Note that this option doesn't guarantee
		///		the selected framerate. Set this value to -1 to render as fast as the system
		///		allows under current conditions. IMPORTANT: this option disables VSync while
		///		enabled!
		/// </summary>
		public bool ThrottleFrameRate
		{
			get { return _throttleFrameRate; }
			set
			{
				if (_throttleFrameRate == value || !Application.isPlaying) return;
				_throttleFrameRate = value;
				if (!enabled || _status == StatsMonitorStatus.Inactive) return;
				RefreshThrottledFrameRate();
			}
		}

		/// <summary>
		///		The maximum framerate at which the game may run if Framerate Throttling
		///		is enabled. When setting this value to -1 the game will run at a framerate
		///		as fast as the system's performance allows.
		/// </summary>
		public int ThrottledFrameRate
		{
			get { return _throttledFrameRate; }
			set
			{
				if (_throttledFrameRate == value || !Application.isPlaying) return;
				_throttledFrameRate = value;
				if (!enabled || _status == StatsMonitorStatus.Inactive) return;
				RefreshThrottledFrameRate();
			}
		}

		/// <summary>
		///		The amount of samples collected to calculate the average FPS value from.
		///		Setting this to 0 will result in an average FPS value calculated from
		///		all samples since startup or level load.
		/// </summary>
		public int AverageSamples
		{
			get { return _avgSamples; }
			set
			{
				if (_avgSamples == value || !Application.isPlaying) return;
				_avgSamples = value;
				if (!enabled) return;
				if (_avgSamples > 0)
				{
					if (_accAVGSamples == null)
						_accAVGSamples = new float[_avgSamples];
					else if (_accAVGSamples.Length != _avgSamples)
						Array.Resize(ref _accAVGSamples, _avgSamples);
				}
				else
				{
					_accAVGSamples = null;
				}
				ResetAverageFPS();
				UpdateData();
				UpdateView();
			}
		}

		/// <summary>
		///		The threshold below which the FPS will be marked with warning color.
		/// </summary>
		public int WarningThreshold
		{
			get { return _warningThreshold; }
			set { _warningThreshold = value; }
		}

		/// <summary>
		///		The threshold below which the FPS will be marked with critical color.
		/// </summary>
		public int CriticalThreshold
		{
			get { return _criticalThreshold; }
			set { _criticalThreshold = value; }
		}


		// ----------------------------------------------------------------------------------------
		// Public Methods
		// ----------------------------------------------------------------------------------------

		/// <summary>
		///		Toggles the mode of Stats Monitor between Active and Inactive.
		/// </summary>
		public void Toggle()
		{
			if (_status == StatsMonitorStatus.Inactive) Status = StatsMonitorStatus.Active;
			else if (_status == StatsMonitorStatus.Active) Status = StatsMonitorStatus.Inactive;
		}


		/// <summary>
		///		Switches to the next layout style.
		/// </summary>
		public void NextStyle()
		{
			if (_layout == StatsMonitorLayout.Minimal) Layout = StatsMonitorLayout.StatsOnly;
			else if (_layout == StatsMonitorLayout.StatsOnly) Layout = StatsMonitorLayout.Standard;
			else if (_layout == StatsMonitorLayout.Standard) Layout = StatsMonitorLayout.Full;
			else if (_layout == StatsMonitorLayout.Full) Layout = StatsMonitorLayout.Minimal;
		}


		/// <summary>
		///		Switches to the next alignment, in a clock-wise order.
		/// </summary>
		public void NextAlignment()
		{
			if (_alignment == UIAlignment.UpperLeft) Align(UIAlignment.UpperCenter);
			else if (_alignment == UIAlignment.UpperCenter) Align(UIAlignment.UpperRight);
			else if (_alignment == UIAlignment.UpperRight) Align(UIAlignment.MiddleRight);
			else if (_alignment == UIAlignment.MiddleRight) Align(UIAlignment.LowerRight);
			else if (_alignment == UIAlignment.LowerRight) Align(UIAlignment.LowerCenter);
			else if (_alignment == UIAlignment.LowerCenter) Align(UIAlignment.LowerLeft);
			else if (_alignment == UIAlignment.LowerLeft) Align(UIAlignment.MiddleLeft);
			else if (_alignment == UIAlignment.MiddleLeft) Align(UIAlignment.UpperLeft);
		}


		/// <summary>
		/// 	Allows to set the alignment of Stats Monitor to a specific value.
		/// </summary>
		/// <param name="alignment">The aligment value.</param>
		/// <param name="newScale">Allows for specifying a new scale value. This is
		/// used by the script internally to provide a correctly updated scale for
		/// touch areas.</param>
		public void Align(UIAlignment alignment, int newScale = -1)
		{
			_alignment = alignment;
			switch (alignment)
			{
				case UIAlignment.UpperLeft:
					_anchor = _anchors.upperLeft;
					break;
				case UIAlignment.UpperCenter:
					_anchor = _anchors.upperCenter;
					break;
				case UIAlignment.UpperRight:
					_anchor = _anchors.upperRight;
					break;
				case UIAlignment.MiddleRight:
					_anchor = _anchors.middleRight;
					break;
				case UIAlignment.LowerRight:
					_anchor = _anchors.lowerRight;
					break;
				case UIAlignment.LowerCenter:
					_anchor = _anchors.lowerCenter;
					break;
				case UIAlignment.LowerLeft:
					_anchor = _anchors.lowerLeft;
					break;
				case UIAlignment.MiddleLeft:
					_anchor = _anchors.middleLeft;
					break;
			}
			
			var t = gameObject.GetComponent<RectTransform>();
			t.anchoredPosition = _anchor.position;
			t.anchorMin = _anchor.min;
			t.anchorMax = _anchor.max;
			t.pivot = _anchor.pivot;

#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
			StartCoroutine("UpdateTouchAreaLater", newScale);
#endif
		}


		/// <summary>
		///		Resets the FPS minimum and maximum values.
		/// </summary>
		public void ResetMinMaxFPS()
		{
			fpsMin = -1;
			fpsMax = -1;
			_minMaxIntervalsSkipped = 0;
			if (!Application.isPlaying) return;
			UpdateData(true);
		}


		/// <summary>
		///		Resets the average FPS value.
		/// </summary>
		public void ResetAverageFPS()
		{
			if (!Application.isPlaying) return;
			fpsAvg = 0;
			_currentAVGSamples = 0;
			_currentAVGRaw = 0;
			if (_avgSamples > 0 && _accAVGSamples != null)
			{
				Array.Clear(_accAVGSamples, 0, _accAVGSamples.Length);
			}
		}


		/// <summary>
		///		Completely disposes Stats Monitor.
		/// </summary>
		public void Dispose()
		{
			StopCoroutine("Interval");
			DisposeChildren();
			Destroy(this);
			_isDisposed = true;
		}

		// ----------------------------------------------------------------------------------------
		// Internal Methods
		// ----------------------------------------------------------------------------------------

		internal void Invalidate(UIInvalidationType type, StatsMonitorInvalidationFlag flag = StatsMonitorInvalidationFlag.Any, bool invalidateChildren = true)
		{
			UpdateFont();

			var totalWidth = 0.0f;
			var totalHeight = 0.0f;

			if (_fpsView != null)
			{
				if (invalidateChildren && (flag == StatsMonitorInvalidationFlag.Any || flag == StatsMonitorInvalidationFlag.Text))
				{
					_fpsView.Invalidate(type);
				}
				if (_fpsView.Width > totalWidth)
				{
					totalWidth = _fpsView.Width;
				}
				totalHeight += _fpsView.Height;
			}
			if (_statsView != null)
			{
				if (invalidateChildren && (flag == StatsMonitorInvalidationFlag.Any || flag == StatsMonitorInvalidationFlag.Text))
				{
					_statsView.Invalidate(type);
				}
				if (_statsView.Width > totalWidth)
				{
					totalWidth = _statsView.Width;
				}
				totalHeight += _statsView.Height;
			}
			if (_graphView != null)
			{
				_graphView.SetWidth(totalWidth);
				if (invalidateChildren &&
					(flag == StatsMonitorInvalidationFlag.Any || flag == StatsMonitorInvalidationFlag.Graph || flag == StatsMonitorInvalidationFlag.Text))
				{
					_graphView.Invalidate();
				}
				_graphView.Y = -totalHeight;
				if (_graphView.Width > totalWidth)
				{
					totalWidth = _graphView.Width;
				}
				totalHeight += _graphView.Height;
			}
			if (_sysInfoView != null)
			{
				_sysInfoView.SetWidth(totalWidth);
				if (invalidateChildren && (flag == StatsMonitorInvalidationFlag.Any || flag == StatsMonitorInvalidationFlag.Text))
				{
					_sysInfoView.Invalidate(type);
				}
				_sysInfoView.Y = -totalHeight;
				if (_sysInfoView.Width > totalWidth)
				{
					totalWidth = _sysInfoView.Width;
				}
				totalHeight += _sysInfoView.Height;
			}

			if (_layout != StatsMonitorLayout.Minimal && (type == UIInvalidationType.All ||
				(type == UIInvalidationType.Style && flag == StatsMonitorInvalidationFlag.Background)))
			{
				CreateBackground();
			}

			if (totalWidth > 0.0f || totalHeight > 0.0f)
			{
				_totalWidth = (int) totalWidth;
				_totalHeight = (int) totalHeight;
				gameObject.transform.localScale = Vector3.one;
				StatsMonitorUtil.RTransform(gameObject, Vector2.one, 0, 0, _totalWidth, _totalHeight);

				/* Re-apply current scale factor. */
				if (autoScale)
				{
					var scaleFactor = (int) StatsMonitorUtil.DPIScaleFactor(true);
					if (scaleFactor > -1) scale = scaleFactor;
					if (scale > 10) scale = 10; // Unlikely, but just to be safe.

					/* Prevent scaled size to be larger than fullscreen resolution! Mainly
					 * useful for small devices like iPhone to prevent the view extend
					 * outside the screen. */
					if (_totalWidth * scale > Screen.currentResolution.width) scale--;
				}

				gameObject.transform.localScale = new Vector3(scale, scale, 1.0f);
				Align(_alignment, scale);
			}
		}


		// ----------------------------------------------------------------------------------------
		// Private/Protected Methods
		// ----------------------------------------------------------------------------------------

		private void CreateChildren()
		{
			UpdateFont();

			/* Reset any applied scaling. */
			gameObject.transform.localScale = Vector3.one;

			switch (_layout)
			{
				case StatsMonitorLayout.Minimal:
					DisposeChild(StatsMonitorViewType.Background);
					DisposeChild(StatsMonitorViewType.StatsView);
					DisposeChild(StatsMonitorViewType.GraphView);
					DisposeChild(StatsMonitorViewType.SysInfoView);
					if (_fpsView == null) _fpsView = new FPSView(this);
					break;
				case StatsMonitorLayout.StatsOnly:
					CreateBackground();
					DisposeChild(StatsMonitorViewType.FPSView);
					DisposeChild(StatsMonitorViewType.GraphView);
					DisposeChild(StatsMonitorViewType.SysInfoView);
					if (_statsView == null) _statsView = new StatsView(this);
					break;
				case StatsMonitorLayout.Standard:
					CreateBackground();
					DisposeChild(StatsMonitorViewType.FPSView);
					DisposeChild(StatsMonitorViewType.SysInfoView);
					if (_statsView == null) _statsView = new StatsView(this);
					if (_graphView == null) _graphView = new GraphView(this);
					break;
				case StatsMonitorLayout.Full:
					CreateBackground();
					DisposeChild(StatsMonitorViewType.FPSView);
					if (_statsView == null) _statsView = new StatsView(this);
					if (_graphView == null) _graphView = new GraphView(this);
					if (_sysInfoView == null) _sysInfoView = new SysInfoView(this);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			/* Add all child objects to UI layer. */
			foreach (Transform t1 in transform)
			{
				StatsMonitorUtil.AddToUILayer(t1.gameObject);
				foreach (Transform t2 in t1.transform)
				{
					StatsMonitorUtil.AddToUILayer(t2.gameObject);
				}
			}

			Invalidate(UIInvalidationType.All);
		}


		private void DisposeChildren()
		{
			DisposeChild(StatsMonitorViewType.Background);
			DisposeChild(StatsMonitorViewType.FPSView);
			DisposeChild(StatsMonitorViewType.StatsView);
			DisposeChild(StatsMonitorViewType.GraphView);
			DisposeChild(StatsMonitorViewType.SysInfoView);
		}


		private void DisposeChild(StatsMonitorViewType viewType)
		{
			switch (viewType)
			{
				case StatsMonitorViewType.Background:
					if (_background != null)
					{
						Destroy(_gradient);
						Destroy(_background);
						_gradient = null;
						_background = null;
					}
					break;
				case StatsMonitorViewType.FPSView:
					if (_fpsView != null)
					{
						_fpsView.Dispose();
					}
					_fpsView = null;
					break;
				case StatsMonitorViewType.StatsView:
					if (_statsView != null)
					{
						_statsView.Dispose();
					}
					_statsView = null;
					break;
				case StatsMonitorViewType.GraphView:
					if (_graphView != null)
					{
						_graphView.Dispose();
					}
					_graphView = null;
					break;
				case StatsMonitorViewType.SysInfoView:
					if (_sysInfoView != null)
					{
						_sysInfoView.Dispose();
					}
					_sysInfoView = null;
					break;
			}
		}


		private void UpdateData(bool forceUpdate = false, float timeElapsed = -1.0f)
		{
			/* Update FPS value. */
			var roundedFPS = (int) _fpsNew;
			ms = (1000.0f / _fpsNew);
			if (fps != roundedFPS || forceUpdate) fps = roundedFPS;

			if (fps <= _criticalThreshold) fpsLevel = 2;
			else if (fps <= _warningThreshold) fpsLevel = 1;
			else
			{
				if (fps > 999) fps = 999;
				fpsLevel = 0;
			}

			if (_layout == StatsMonitorLayout.Minimal) return;

			/* Calculate FPS MIN/MAX FPS values. */
			if (_minMaxIntervalsSkipped < MINMAX_SKIP_INTERVALS)
			{
				if (!forceUpdate) _minMaxIntervalsSkipped++;
			}
			else
			{
				if (fpsMin == -1) fpsMin = fps;
				else if (fps < fpsMin) fpsMin = fps;
				if (fpsMax == -1) fpsMax = fps;
				else if (fps > fpsMax) fpsMax = fps;
			}

			/* Calculate Average FPS value. */
			if (_avgSamples == 0)
			{
				_currentAVGSamples++;
				_currentAVGRaw += (fps - _currentAVGRaw) / _currentAVGSamples;
			}
			else
			{
				_accAVGSamples[_currentAVGSamples % _avgSamples] = fps;
				_currentAVGSamples++;
				_currentAVGRaw = GetAccumulatedAVGSamples();
			}
			var rounded = Mathf.RoundToInt(_currentAVGRaw);
			if (fpsAvg != rounded || forceUpdate) fpsAvg = rounded;

			/* Calculate fixed update rate. */
			fixedUpdateRate = 1.0f / Time.fixedDeltaTime;

			/* Calculate MEM values. */
			memTotal = Profiler.GetTotalReservedMemoryLong() / MEMORY_DIVIDER;
			memAlloc = Profiler.GetTotalAllocatedMemoryLong() / MEMORY_DIVIDER;
			memMono = GC.GetTotalMemory(false) / MEMORY_DIVIDER;

			/* Calculate objects count/rendered objects count. */
			if (_objectsCountInterval > 0)
			{
				_intervalTimeCount2 += timeElapsed;
				if (_intervalTimeCount2 >= _objectsCountInterval || timeElapsed < 0.0f || forceUpdate)
				{
					var allObjects = FindObjectsOfType<GameObject>();
					objectCount = allObjects.Length;
					renderObjectCount = renderedObjectCount = 0;

					for (var i = 0; i < allObjects.Length; i++)
					{
						var r = allObjects[i].GetComponent<Renderer>();
						if (r == null) continue;
						++renderObjectCount;
						if (r.isVisible) ++renderedObjectCount;
					}

					_intervalTimeCount2 = 0.0f;
				}
			}
		}


		private void UpdateView(float timeElapsed = -1.0f)
		{
			/* Add up interval time count to update text/graph views at their own intervals. */
			_intervalTimeCount += timeElapsed;

			/* Update textual views. */
			if (_intervalTimeCount >= _statsUpdateInterval || timeElapsed < 0.0f)
			{
				if (_fpsView != null) _fpsView.Update();
				if (_statsView != null) _statsView.Update();
				if (_sysInfoView != null) _sysInfoView.Update();
				if (_statsUpdateInterval > _graphUpdateInterval) _intervalTimeCount = 0.0f;
			}

			/* Update graph views. */
			if (!(_intervalTimeCount >= _graphUpdateInterval) && !(timeElapsed < 0.0f)) return;
			if (_graphView != null) _graphView.Update();
			if (_graphUpdateInterval >= _statsUpdateInterval) _intervalTimeCount = 0.0f;
		}


		private float GetAccumulatedAVGSamples()
		{
			float totalFPS = 0;
			for (var i = 0; i < _avgSamples; i++) totalFPS += _accAVGSamples[i];
			return _currentAVGSamples < _avgSamples ? totalFPS / _currentAVGSamples : totalFPS / _avgSamples;
		}


		private void RefreshThrottledFrameRate()
		{
			// ReSharper disable once IntroduceOptionalParameters.Local
			RefreshThrottledFrameRate(false);
		}


		private void RefreshThrottledFrameRate(bool disable)
		{
			if (_throttleFrameRate && !disable)
			{
				if (_cachedVSync == -1)
				{
					_cachedVSync = QualitySettings.vSyncCount;
					_cachedFrameRate = Application.targetFrameRate;
					QualitySettings.vSyncCount = 0;
				}
				Application.targetFrameRate = _throttledFrameRate;
			}
			else
			{
				if (_cachedVSync == -1) return;
				QualitySettings.vSyncCount = _cachedVSync;
				Application.targetFrameRate = _cachedFrameRate;
				_cachedVSync = -1;
			}
		}


		private void DetermineActualUpdateInterval()
		{
			/* We need to run the interval at whichever update interval value is smaller. */
			_actualUpdateInterval = _graphUpdateInterval < _statsUpdateInterval ? _graphUpdateInterval : _statsUpdateInterval;
		}


		private void RestartCoroutine()
		{
			StopCoroutine("Interval");
			StartCoroutine("Interval");
		}


		private void UpdateFont()
		{
			/* Load default font used for stats text display. */
			if (fontFace != null) return;
			fontFace = (Font) Resources.Load("Fonts/terminalstats", typeof(Font));
			if (fontFace == null) fontFace = (Font) Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
		}


		private void CreateBackground()
		{
			if (Math.Abs(colorBGUpper.a) < 0.01f && Math.Abs(colorBGLower.a) < 0.01f)
			{
				DisposeChild(StatsMonitorViewType.Background);
			}
			else if (_background == null)
			{
				_gradient = new Texture2D(2, 2)
				{
					filterMode = FilterMode.Bilinear,
					wrapMode = TextureWrapMode.Clamp
				};
				_background = gameObject.GetComponent<RawImage>();
				if (_background == null) _background = gameObject.AddComponent<RawImage>();
				_background.color = Color.white;
				_background.texture = _gradient;
			}

			if (_background != null)
			{
				_gradient.SetPixel(0, 0, colorBGLower);
				_gradient.SetPixel(1, 0, colorBGLower);
				_gradient.SetPixel(0, 1, colorBGUpper);
				_gradient.SetPixel(1, 1, colorBGUpper);
				_gradient.Apply();
			}
		}


#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
		private void UpdateTouchArea(int newScale = -1)
		{
			/* If new scale is given by AlignLater, take it over! */
			if (newScale > -1) scale = newScale;
			var scaledW = _totalWidth * scale;
			var scaledH = _totalHeight * scale;

			switch (_alignment)
			{
				case UIAlignment.UpperLeft:
					_touchArea = new Rect(0, Screen.height - scaledH, scaledW, scaledH);
					break;
				case UIAlignment.UpperCenter:
					_touchArea = new Rect((Screen.width * .5f) - (scaledW * .5f), Screen.height - scaledH, scaledW, scaledH); 
					break;
				case UIAlignment.UpperRight:
					_touchArea = new Rect(Screen.width - scaledW, Screen.height - scaledH, scaledW, scaledH);
					break;
				case UIAlignment.LowerRight:
					_touchArea = new Rect(Screen.width - scaledW, 0, scaledW, scaledH);
					break;
				case UIAlignment.LowerCenter:
					_touchArea = new Rect((Screen.width * .5f) - (scaledW * .5f), 0, scaledW, scaledH);
					break;
				case UIAlignment.LowerLeft:
					_touchArea = new Rect(0, 0, scaledW, scaledH);
					break;
			}
		}
#endif


		// ----------------------------------------------------------------------------------------
		// Coroutines
		// ----------------------------------------------------------------------------------------

		private IEnumerator Interval()
		{
			while (true)
			{
				/* Calculate new FPS value. */
				var previousUpdateTime = Time.unscaledTime;
				var previousUpdateFrames = Time.frameCount;
				yield return new WaitForSeconds(_actualUpdateInterval);
				var timeElapsed = Time.unscaledTime - previousUpdateTime;
				var framesChanged = Time.frameCount - previousUpdateFrames;
				_fpsNew = (framesChanged / timeElapsed);
				UpdateData(false, timeElapsed);
				UpdateView(timeElapsed);
			}
			// ReSharper disable once FunctionNeverReturns
			// ReSharper disable once IteratorNeverReturns
		}


#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
		private IEnumerator UpdateTouchAreaLater(int newScale)
		{
			/* Calls UpdateTouchArea() one frame later. We need this to have correct screen
			 * width/height in UpdateTouchArea() after calling Invalidate(). */
			yield return 0;
			UpdateTouchArea(newScale);
		}
#endif


		// ----------------------------------------------------------------------------------------
		// Unity Callbacks
		// ----------------------------------------------------------------------------------------

		private void Awake()
		{
			fpsMin = fpsMax = -1;
			_accAVGSamples = new float[_avgSamples];
			_isInitialized = true;
			_isDisposed = false;
		}


		private void Update()
		{
			if (!_isInitialized || !inputEnabled) return;

#if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_WEBGL || UNITY_EDITOR
			if (Input.anyKeyDown)
			{
				/* Check for toggle key combinations. */
				if (((modKeyToggle != KeyCode.None && hotKeyToggle != KeyCode.None) &&
						(Input.GetKey(modKeyToggle) && Input.GetKeyDown(hotKeyToggle))) ||
					((modKeyToggle == KeyCode.None && hotKeyToggle != KeyCode.None) && Input.GetKeyDown(hotKeyToggle)))
				{
					Toggle();
				}
				/* Aligment- and Style toggle should only work if statsmonitor is visible. */
				/* Check for _alignment key combinations. */
				else if (_status == StatsMonitorStatus.Active &&
					(((modKeyAlignment != KeyCode.None && hotKeyAlignment != KeyCode.None) &&
							(Input.GetKey(modKeyAlignment) && Input.GetKeyDown(hotKeyAlignment))) ||
						((modKeyAlignment == KeyCode.None && hotKeyAlignment != KeyCode.None) && Input.GetKeyDown(hotKeyAlignment))))
				{
					NextAlignment();
				}
				/* Check for _style key combinations. */
				else if (_status == StatsMonitorStatus.Active &&
					(((modKeyStyle != KeyCode.None && hotKeyStyle != KeyCode.None) &&
							(Input.GetKey(modKeyStyle) && Input.GetKeyDown(hotKeyStyle))) ||
						((modKeyStyle == KeyCode.None && hotKeyStyle != KeyCode.None) && Input.GetKeyDown(hotKeyStyle))))
				{
					NextStyle();
				}
			}
#elif UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
			if (toggleTouchCount > 0 && Input.touchCount == toggleTouchCount)
			{
				var touch = Input.GetTouch(toggleTouchCount - 1);
				if (touch.phase == TouchPhase.Began)
				{
					_touchOrigin = touch.position;
				}
				else if (touch.phase == TouchPhase.Ended && _touchOrigin.x >= 0)
				{
					_touchOrigin = -Vector2.one;
					Toggle();
				}
			}
			else if (switchStyleTapCount > 0 && (_status == StatsMonitorStatus.Active && Input.touchCount == 2))
			{
				var touchA = Input.GetTouch(0);
				var touchB = Input.GetTouch(1);
				/* First finger must be inside area, second finger must be outside. */
				if (_touchArea.Contains(touchA.position) && !_touchArea.Contains(touchB.position))
				{
					if (touchB.phase == TouchPhase.Began && touchB.tapCount == switchStyleTapCount)
					{
						_touchOrigin = touchB.position;
					}
					else if (touchB.phase == TouchPhase.Ended && _touchOrigin.x >= 0)
					{
						_touchOrigin = -Vector2.one;
						NextStyle();
					}
				}
			}
			else if (switchAlignmentTapCount > 0 && (_status == StatsMonitorStatus.Active && Input.touchCount == 1))
			{
				var touch = Input.GetTouch(0);
				if (_touchArea.Contains(touch.position))
				{
					if (touch.phase == TouchPhase.Began && touch.tapCount == switchAlignmentTapCount)
					{
						_touchOrigin = touch.position;
					}
					else if (touch.phase == TouchPhase.Ended && _touchOrigin.x >= 0)
					{
						_touchOrigin = -Vector2.one;
						NextAlignment();
					}
				}
			}
#endif
		}


		private void OnEnable()
		{
			if (!_isInitialized || _status == StatsMonitorStatus.Inactive) return;

			fps = 0;
			memTotal = 0.0f;
			memAlloc = 0.0f;
			memMono = 0.0f;
			_intervalTimeCount = 0.0f;
			_intervalTimeCount2 = 0.0f;

			ResetMinMaxFPS();
			ResetAverageFPS();
			DetermineActualUpdateInterval();

			if (_status == StatsMonitorStatus.Active) CreateChildren();

			StartCoroutine("Interval");
			UpdateView();
			Invoke("RefreshThrottledFrameRate", 0.5f);
		}


		private void OnDisable()
		{
			if (!_isInitialized) return;
			StopCoroutine("Interval");
			if (IsInvoking("RefreshThrottledFrameRate")) CancelInvoke("RefreshThrottledFrameRate");
			RefreshThrottledFrameRate(true);
			DisposeChildren();
		}


		private void OnDestroy()
		{
			if (_isInitialized)
			{
				DisposeChildren();
				_isInitialized = false;
			}
			Destroy(gameObject);
		}
	}


	// ============================================================================================

	/// <summary>
	///		A wrapper class for Texture2D that makes it easier to work with a
	///		Texture2D used in UI.
	/// </summary>
	internal sealed class StatsMonitorBitmap
	{
		internal readonly Texture2D texture;
		internal Color color;
		private readonly Rect _rect;

		
		internal StatsMonitorBitmap(int width, int height, Color? color = null)
		{
			texture = new Texture2D(width, height, TextureFormat.ARGB32, false) {filterMode = FilterMode.Point};
			_rect = new Rect(0, 0, width, height);
			this.color = color ?? Color.black;
			Clear();
		}

		
		internal StatsMonitorBitmap(float width, float height, Color? color = null)
		{
			texture = new Texture2D((int) width, (int) height, TextureFormat.ARGB32, false) {filterMode = FilterMode.Point};
			this.color = color ?? Color.black;
			Clear();
		}

		
		internal void Resize(int width, int height)
		{
			texture.Reinitialize(width, height);
			texture.Apply();
		}

		
		/// <summary>
		///		Clears the Bitmap2D by filling it with a given color. If color is null
		///		the default color is being used.
		/// </summary>
		internal void Clear(Color? col = null)
		{
			var c = col ?? color;
			var a = texture.GetPixels();
			var i = 0;
			while (i < a.Length) a[i++] = c;
			texture.SetPixels(a);
			texture.Apply();
		}

		
		/// <summary>
		///		Fills an area in the Bitmap2D with a given color. If rect is null the
		///		whole bitmap is filled. If color is null the default color is used.
		/// </summary>
		internal void FillRect(Rect? rect = null, Color? col = null)
		{
			var r = rect ?? _rect;
			var c = col ?? color;
			var a = new Color[(int) (r.width * r.height)];
			var i = 0;
			while (i < a.Length) a[i++] = c;
			texture.SetPixels((int) r.x, (int) r.y, (int) r.width, (int) r.height, a);
		}

		
		/// <summary>
		///		Fills an area in the Bitmap2D with a given color. If rect is null the
		///		whole bitmap is filled. If color is null the default color is used.
		/// </summary>
		internal void FillRect(int x, int y, int w, int h, Color? col = null)
		{
			var c = col ?? color;
			var a = new Color[w * h];
			var i = 0;
			while (i < a.Length) a[i++] = c;
			texture.SetPixels(x, y, w, h, a);
		}

		
		/// <summary>
		///		Fills a one pixel column in the bitmap.
		/// </summary>
		internal void FillColumn(int x, Color? col = null)
		{
			FillRect(new Rect(x, 0, 1, texture.height), col);
		}

		
		internal void FillColumn(int x, int y, int height, Color? col = null)
		{
			FillRect(new Rect(x, y, 1, height), col);
		}

		
		/// <summary>
		///		Fills a one pixel row in the bitmap.
		/// </summary>
		internal void FillRow(int y, Color? col = null)
		{
			FillRect(new Rect(0, y, texture.width, 1), col);
		}

		
		/// <summary>
		///		Sets a pixel at x, y.
		/// </summary>
		internal void SetPixel(int x, int y, Color col)
		{
			texture.SetPixel(x, y, col);
		}

		
		/// <summary>
		///		Sets a pixel at x, y.
		/// </summary>
		internal void SetPixel(float x, float y, Color col)
		{
			texture.SetPixel((int) x, (int) y, col);
		}

		
		///  <summary>
		/// 		Scrolls the bitmap by a certain amount of pixels.
		///  </summary>
		internal void Scroll(int x, Color? fillColor = null)
		{
			x = ~x + 1;
			/* GC fix introduced in 1.3.7 */
			for (var i = 0; i < texture.width - x; i++)
			{
				for (var j = 0; j < texture.height; j++)
				{
					texture.SetPixel(i, j, texture.GetPixel(x + i, j));
				}
			}
			FillRect(texture.width - x, 0, x, texture.height, fillColor);
		}

		
		/// <summary>
		///		Applies changes to the bitmap.
		/// </summary>
		internal void Apply()
		{
			texture.Apply();
		}

		
		internal void Dispose()
		{
			Object.Destroy(texture);
		}
	}
	
	
	// ============================================================================================

	/// <summary>
	///		Base class for StatsMonitor views.
	/// </summary>
	internal abstract class StatsMonitorView
	{
		/// <summary>
		///		The game object of this view. All child objects should be added
		///		inside this game object.
		/// </summary>
		private GameObject _gameObject;

		private RectTransform _rectTransform;
		protected StatsMonitorWidget _widget;

		/// <summary>
		///		Returns the RectTransform for the view. If the views doesn't have a
		///		RectTransform compoents yet, one is added automatically.
		/// </summary>
		internal RectTransform RTransform
		{
			get
			{
				if (_rectTransform != null) return _rectTransform;
				_rectTransform = _gameObject.GetComponent<RectTransform>();
				if (_rectTransform == null) _rectTransform = _gameObject.AddComponent<RectTransform>();
				return _rectTransform;
			}
		}

		/// <summary>
		///		The width of the view.
		/// </summary>
		internal float Width
		{
			get { return RTransform.rect.width; }
			set { RTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, value); }
		}

		/// <summary>
		///		The height of the view.
		/// </summary>
		internal float Height
		{
			get { return RTransform.rect.height; }
			set { RTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, value); }
		}

		/// <summary>
		///		The X position of the view.
		/// </summary>
		internal float X
		{
			get { return RTransform.anchoredPosition.x; }
			set { RTransform.anchoredPosition = new Vector2(value, Y); }
		}

		/// <summary>
		///		The Y position of the view.
		/// </summary>
		internal float Y
		{
			get { return RTransform.anchoredPosition.y; }
			set { RTransform.anchoredPosition = new Vector2(X, value); }
		}

		/// <summary>
		///		The pivot vector of the view.
		/// </summary>
		internal Vector2 Pivot
		{
			get { return RTransform.pivot; }
			set { RTransform.pivot = value; }
		}

		/// <summary>
		///		The min anchor vector of the view.
		/// </summary>
		internal Vector2 AnchorMin
		{
			get { return RTransform.anchorMin; }
			set { RTransform.anchorMin = value; }
		}

		/// <summary>
		///		The max anchor vector of the view.
		/// </summary>
		internal Vector2 AnchorMax
		{
			get { return RTransform.anchorMax; }
			set { RTransform.anchorMax = value; }
		}

		
		/// <summary>
		///		Sets the scale of the view.
		/// </summary>
		internal void SetScale(float h = 1.0f, float v = 1.0f)
		{
			RTransform.localScale = new Vector3(h, v, 1.0f);
		}

		
		/// <summary>
		///		Allows to set all rect transform values at once.
		/// </summary>
		internal void SetRTransformValues(float x, float y, float width, float height, Vector2 pivotAndAnchor)
		{
			RTransform.anchoredPosition = new Vector2(x, y);
			RTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
			RTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
			Pivot = AnchorMin = AnchorMax = pivotAndAnchor;
		}

		
		/// <summary>
		///		Invalidates the view. This methods takes care of that all child
		///		objects have been created and then takes care that the style and layout
		///		is updated. This method needs to be called from a sub class constructor!
		/// </summary>
		internal void Invalidate(UIInvalidationType type = UIInvalidationType.All)
		{
			if (_gameObject == null) _gameObject = CreateChildren();
			/* Reset Z pos! */
			RTransform.anchoredPosition3D = new Vector3(RTransform.anchoredPosition.x, RTransform.anchoredPosition.y, 0.0f);
			SetScale();
			if (type == UIInvalidationType.Style || type == UIInvalidationType.All) UpdateStyle();
			if (type == UIInvalidationType.Layout || type == UIInvalidationType.All) UpdateLayout();
		}

		
		/// <summary>
		///		Resets the view and all its child objects.
		/// </summary>
		internal virtual void Reset()
		{
		}

		
		/// <summary>
		///		Updates the view.
		/// </summary>
		internal virtual void Update()
		{
		}

		
		/// <summary>
		///		Disposes the view and all its children.
		/// </summary>
		internal virtual void Dispose()
		{
			Destroy(_gameObject);
			_gameObject = null;
		}

		
		/// <summary>
		///		Static helper method to destroy child objects.
		/// </summary>
		internal static void Destroy(Object obj)
		{
			Object.Destroy(obj);
		}

		
		/// <summary>
		///		Used to create any child objects for the view. Should only be called
		///		once per object lifetime. This method must return the game object of
		///		the view!
		/// </summary>
		protected virtual GameObject CreateChildren()
		{
			return null;
		}

		
		/// <summary>
		///		Used to update the style of child objects. This can be used to make
		///		visual changes that don't require the layout to be updated, for example
		///		color changes.
		/// </summary>
		protected virtual void UpdateStyle()
		{
		}

		
		/// <summary>
		///		Used to layout all child objects in the view. This updates the
		///		transformations of all children and should be called to change the
		///		size, position, etc. of child objects.
		/// </summary>
		protected virtual void UpdateLayout()
		{
		}
	}


	// ============================================================================================

	/// <summary>
	///		View class that displays only an FPS counter.
	/// </summary>
	internal sealed class FPSView : StatsMonitorView
	{
		private Text _text;
		private string[] _fpsTemplates;

		
		internal FPSView(StatsMonitorWidget widget)
		{
			_widget = widget;
			Invalidate();
		}

		
		internal override void Reset()
		{
			_text.text = "";
		}

		
		internal override void Update()
		{
			_text.text = StatsMonitorUtil.Concat(_fpsTemplates[_widget.fpsLevel], _widget.fps.ToString(), "FPS</color>");
		}

		
		internal override void Dispose()
		{
			Destroy(_text);
			_text = null;
			base.Dispose();
		}

		
		protected override GameObject CreateChildren()
		{
			_fpsTemplates = new string[3];

			var container = new GameObject { name = "FPSView" };
			container.transform.parent = _widget.transform;

			var g = new StatsMonitorGraphics(container, _widget.colorFPS, _widget.fontFace, _widget.fontSizeSmall);
			_text = g.CreateText("Text", "000FPS");
			_text.alignment = TextAnchor.MiddleCenter;

			return container;
		}

		
		protected override void UpdateStyle()
		{
			_text.font = _widget.fontFace;
			_text.fontSize = _widget.FontSizeLarge;
			_text.color = _widget.colorFPS;

			if (_widget.colorOutline.a > 0.0f)
				StatsMonitorGraphics.AddOutlineAndShadow(_text.gameObject, _widget.colorOutline);
			else
				StatsMonitorGraphics.RemoveEffects(_text.gameObject);

			_fpsTemplates[0] = StatsMonitorUtil.Concat("<color=#", StatsMonitorUtil.Color32ToHex(_widget.colorFPS), ">");
			_fpsTemplates[1] = StatsMonitorUtil.Concat("<color=#", StatsMonitorUtil.Color32ToHex(_widget.colorFPSWarning), ">");
			_fpsTemplates[2] = StatsMonitorUtil.Concat("<color=#", StatsMonitorUtil.Color32ToHex(_widget.colorFPSCritical), ">");
		}

		
		protected override void UpdateLayout()
		{
			var padding = _widget.padding;
			_text.rectTransform.anchoredPosition = new Vector2(padding, -padding);

			/* Center the text object */
			_text.rectTransform.anchoredPosition = Vector2.zero;
			_text.rectTransform.anchorMin = _text.rectTransform.anchorMax = _text.rectTransform.pivot = new Vector2(0.5f, 0.5f);

			/* Update panel size with calculated dimensions. */
			var w = padding + (int) _text.preferredWidth + padding;
			var h = padding + (int) _text.preferredHeight + padding;
			/* Normalize width to even number to prevent texture glitches. */
			w = w % 2 == 0 ? w : w + 1;

			SetRTransformValues(0, 0, w, h, Vector2.one);
		}
	}


	// ============================================================================================

	/// <summary>
	///		View class that displays the textual stats information.
	/// </summary>
	internal sealed class StatsView : StatsMonitorView
	{
		private Text _text1;
		private Text _text2;
		private Text _text3;
		private Text _text4;

		private string[] _fpsTemplates;
		private string   _fpsMinTemplate;
		private string   _fpsMaxTemplate;
		private string   _fpsAvgTemplate;
		private string   _fxuTemplate;
		private string   _msTemplate;
		private string   _objTemplate;
		private string   _memTotalTemplate;
		private string   _memAllocTemplate;
		private string   _memMonoTemplate;

		
		internal StatsView(StatsMonitorWidget widget)
		{
			_widget = widget;
			Invalidate();
		}

		
		internal override void Reset()
		{
			/* Clear all text fields. */
			_text1.text = _text2.text = _text3.text = _text4.text = "";
		}

		
		internal override void Update()
		{
			_text1.text = StatsMonitorUtil.Concat(_fpsTemplates[_widget.fpsLevel], _widget.fps.ToString(), "</color>");

			_text2.text = StatsMonitorUtil.Concat(_fpsMinTemplate, _widget.fpsMin > -1 ? _widget.fpsMin.ToString() : "0", "</color>\n", _fpsMaxTemplate, _widget.fpsMax > -1 ? _widget.fpsMax.ToString() : "0", "</color>");

			var text3Sub = _widget._objectsCountInterval > 0 ? StatsMonitorUtil.Concat(_widget.renderedObjectCount.ToString(), "/", _widget.renderObjectCount.ToString(), "/", _widget.objectCount.ToString()) : "--/--/--";
			_text3.text = StatsMonitorUtil.Concat(_fpsAvgTemplate, _widget.fpsAvg.ToString(), "</color> ", _msTemplate, _widget.ms.ToString("F1"), "MS</color> ", _fxuTemplate, _widget.fixedUpdateRate.ToString("F0"), " </color>\n", _objTemplate, "OBJ:", text3Sub, "</color>");

			_text4.text = StatsMonitorUtil.Concat(_memTotalTemplate, _widget.memTotal.ToString("F1"), "MB</color> ", _memAllocTemplate, _widget.memAlloc.ToString("F1"), "MB</color> ", _memMonoTemplate, _widget.memMono.ToString("F1"), "MB</color>");
		}

		
		internal override void Dispose()
		{
			Destroy(_text1);
			Destroy(_text2);
			Destroy(_text3);
			Destroy(_text4);
			_text1 = _text2 = _text3 = _text4 = null;
			base.Dispose();
		}
		

		protected override GameObject CreateChildren()
		{
			_fpsTemplates = new string[3];

			var container = new GameObject { name = "StatsView" };
			container.transform.parent = _widget.transform;

			var g = new StatsMonitorGraphics(container, _widget.colorFPS, _widget.fontFace, _widget.fontSizeSmall);
			_text1 = g.CreateText("Text1", "FPS:000");
			_text2 = g.CreateText("Text2", "MIN:000\nMAX:000");
			_text3 = g.CreateText("Text3", "AVG:000\n[000.0 MS]");
			_text4 = g.CreateText("Text4", "TOTAL:000.0MB ALLOC:000.0MB MONO:00.0MB");

			return container;
		}

		
		protected override void UpdateStyle()
		{
			_text1.font = _widget.fontFace;
			_text1.fontSize = _widget.FontSizeLarge;
			_text2.font = _widget.fontFace;
			_text2.fontSize = _widget.FontSizeSmall;
			_text3.font = _widget.fontFace;
			_text3.fontSize = _widget.FontSizeSmall;
			_text4.font = _widget.fontFace;
			_text4.fontSize = _widget.FontSizeSmall;

			if (_widget.colorOutline.a > 0.0f)
			{
				StatsMonitorGraphics.AddOutlineAndShadow(_text1.gameObject, _widget.colorOutline);
				StatsMonitorGraphics.AddOutlineAndShadow(_text2.gameObject, _widget.colorOutline);
				StatsMonitorGraphics.AddOutlineAndShadow(_text3.gameObject, _widget.colorOutline);
				StatsMonitorGraphics.AddOutlineAndShadow(_text4.gameObject, _widget.colorOutline);
			}
			else
			{
				StatsMonitorGraphics.RemoveEffects(_text1.gameObject);
				StatsMonitorGraphics.RemoveEffects(_text2.gameObject);
				StatsMonitorGraphics.RemoveEffects(_text3.gameObject);
				StatsMonitorGraphics.RemoveEffects(_text4.gameObject);
			}

			const string ts = "<color=#";
			_fpsTemplates[0]  = StatsMonitorUtil.Concat(ts, StatsMonitorUtil.Color32ToHex(_widget.colorFPS), ">FPS:");
			_fpsTemplates[1]  = StatsMonitorUtil.Concat(ts, StatsMonitorUtil.Color32ToHex(_widget.colorFPSWarning), ">FPS:");
			_fpsTemplates[2]  = StatsMonitorUtil.Concat(ts, StatsMonitorUtil.Color32ToHex(_widget.colorFPSCritical), ">FPS:");
			_fpsMinTemplate   = StatsMonitorUtil.Concat(ts, StatsMonitorUtil.Color32ToHex(_widget.colorFPSMin), ">MIN:");
			_fpsMaxTemplate   = StatsMonitorUtil.Concat(ts, StatsMonitorUtil.Color32ToHex(_widget.colorFPSMax), ">MAX:");
			_fpsAvgTemplate   = StatsMonitorUtil.Concat(ts, StatsMonitorUtil.Color32ToHex(_widget.colorFPSAvg), ">AVG:");
			_fxuTemplate      = StatsMonitorUtil.Concat(ts, StatsMonitorUtil.Color32ToHex(_widget.colorFXD), ">FXD:");
			_msTemplate       = StatsMonitorUtil.Concat(ts, StatsMonitorUtil.Color32ToHex(_widget.colorMS), ">");
			_objTemplate      = StatsMonitorUtil.Concat(ts, StatsMonitorUtil.Color32ToHex(_widget.colorObjCount), ">");
			_memTotalTemplate = StatsMonitorUtil.Concat(ts, StatsMonitorUtil.Color32ToHex(_widget.colorMemTotal), ">TOTAL:");
			_memAllocTemplate = StatsMonitorUtil.Concat(ts, StatsMonitorUtil.Color32ToHex(_widget.colorMemAlloc), ">ALLOC:");
			_memMonoTemplate  = StatsMonitorUtil.Concat(ts, StatsMonitorUtil.Color32ToHex(_widget.colorMemMono), ">MONO:");
		}

		
		protected override void UpdateLayout()
		{
			var padding = _widget.padding;
			var hSpacing = _widget.spacing;
			var vSpacing = (_widget.spacing / 4);

			/* Make sure that string lengths keep initial set length before resizing text. */
			_text1.text = PadString(_text1.text, 7);
			_text2.text = PadString(_text2.text.Split('\n')[0], 7);
			_text3.text = PadString(_text3.text.Split('\n')[0], 20);
			_text4.text = PadString(_text4.text, 39);

			_text1.rectTransform.anchoredPosition = new Vector2(padding, -padding);
			var x = padding + (int) _text1.preferredWidth + hSpacing;
			_text2.rectTransform.anchoredPosition = new Vector2(x, -padding);
			x += (int) _text2.preferredWidth + hSpacing;
			_text3.rectTransform.anchoredPosition = new Vector2(x, -padding);
			x = padding;

			/* Workaround for correct preferredHeight which we'd have to wait for the next frame. */
			var text2DoubleHeight = (int) _text2.preferredHeight * 2;
			var y = padding + ((int) _text1.preferredHeight >= text2DoubleHeight ? (int) _text1.preferredHeight : text2DoubleHeight) + vSpacing;
			_text4.rectTransform.anchoredPosition = new Vector2(x, -y);
			y += (int) _text4.preferredHeight + padding;

			/* Update container size. */
			var row1Width = padding + _text1.preferredWidth + hSpacing + _text2.preferredWidth + hSpacing + _text3.preferredWidth + padding;
			var row2Width = padding + _text4.preferredWidth + padding;

			/* Pick larger width & normalize to even number to prevent texture glitches. */
			var w = row1Width > row2Width ? (int) row1Width : (int) row2Width;
			w = w % 2 == 0 ? w : w + 1;

			SetRTransformValues(0, 0, w, y, Vector2.one);
		}

		
		private static string PadString(string s, int minChars)
		{
			s = StatsMonitorUtil.StripHTMLTags(s);
			if (s.Length >= minChars) return s;
			var len = minChars - s.Length;
			for (var i = 0; i < len; i++)
			{
				s += "_";
			}
			return s;
		}
	}


	// ============================================================================================

	/// <summary>
	///		View class that displays the stats graph.
	/// </summary>
	internal sealed class GraphView : StatsMonitorView
	{
		private RawImage _image;
		private StatsMonitorBitmap _graph;
		private int      _oldWidth;
		private int      _width;
		private int      _height;
		private int      _graphStartX;
		private int      _graphMaxY;
		private int      _memCeiling;
		private int      _lastGCCollectionCount = -1;
		private Color?[] _fpsColors;
		

		public GraphView(StatsMonitorWidget widget)
		{
			_widget = widget;
			Invalidate();
		}

		
		internal override void Reset()
		{
			if (_graph != null) _graph.Clear();
		}

		
		internal override void Update()
		{
			if (_graph == null) return;

			/* Total Mem */
			_graph.SetPixel(_graphStartX, Mathf.Min(_graphMaxY, (int) Mathf.Ceil((_widget.memTotal / _memCeiling) * _height)), _widget.colorMemTotal);
			/* Alloc Mem */
			_graph.SetPixel(_graphStartX, Mathf.Min(_graphMaxY, (int) Mathf.Ceil((_widget.memAlloc / _memCeiling) * _height)), _widget.colorMemAlloc);
			/* Mono Mem */
			var monoMem = (int) Mathf.Ceil((_widget.memMono / _memCeiling) * _height);
			_graph.SetPixel(_graphStartX, Mathf.Min(_graphMaxY, monoMem), _widget.colorMemMono);
			/* MS */
			var ms = (int) _widget.ms >> 1;
			if (ms == monoMem) ms += 1; // Don't overlay mono mem as they are often in the same range.
			_graph.SetPixel(_graphStartX, Mathf.Min(_graphMaxY, ms), _widget.colorMS);
			/* FPS */
			/* FPS graph issue fixed in 1.3.7 */
			_graph.SetPixel(_graphStartX, Mathf.Min(_graphMaxY, ((float)_widget.fps / Mathf.Max(_widget.fpsMax, 60)) * _graphMaxY), _widget.colorFPS);
			/* GC. */
			if (_widget.gcMonitoringEnabled && _lastGCCollectionCount != GC.CollectionCount(0))
			{
				_lastGCCollectionCount = GC.CollectionCount(0);
				_graph.FillColumn(_graphStartX, 0, 5, _widget.colorGCBlip);
			}

			_graph.Scroll(-1, _fpsColors[_widget.fpsLevel]);
			_graph.Apply();
		}

		
		internal override void Dispose()
		{
			if (_graph != null) _graph.Dispose();
			_graph = null;
			Destroy(_image);
			_image = null;
			base.Dispose();
		}

		
		internal void SetWidth(float width)
		{
			_width = (int) width;
		}

		
		protected override GameObject CreateChildren()
		{
			_fpsColors = new Color?[3];

			var container = new GameObject { name = "GraphView" };
			container.transform.parent = _widget.transform;

			_graph = new StatsMonitorBitmap(10, 10, _widget.colorGraphBG);

			_image = container.AddComponent<RawImage>();
			_image.rectTransform.sizeDelta = new Vector2(10, 10);
			_image.color = Color.white;
			_image.texture = _graph.texture;

			/* Calculate estimated memory ceiling for application. */
			var sysMem = SystemInfo.systemMemorySize;
			if (sysMem <= 1024) _memCeiling = 512;
			else if (sysMem > 1024 && sysMem <= 2048) _memCeiling = 1024;
			else _memCeiling = 2048;

			return container;
		}

		
		protected override void UpdateStyle()
		{
			if (_graph != null) _graph.color = _widget.colorGraphBG;
			if (_widget.colorOutline.a > 0.0f)
				StatsMonitorGraphics.AddOutlineAndShadow(_image.gameObject, _widget.colorOutline);
			else
				StatsMonitorGraphics.RemoveEffects(_image.gameObject);
			_fpsColors[0] = null;
			_fpsColors[1] = new Color(_widget.colorFPSWarning.r, _widget.colorFPSWarning.g,
				_widget.colorFPSWarning.b, _widget.colorFPSWarning.a / 4);
			_fpsColors[2] = new Color(_widget.ColorFPSCritical.r, _widget.ColorFPSCritical.g,
				_widget.ColorFPSCritical.b, _widget.ColorFPSCritical.a / 4);
		}

		
		protected override void UpdateLayout()
		{
			/* Make sure that dimensions for text size are valid! */
			if ((_width > 0 && _widget.graphHeight > 0) && (_widget.graphHeight != _height || _oldWidth != _width))
			{
				_oldWidth = _width;

				_height = _widget.graphHeight;
				_height = _height % 2 == 0 ? _height : _height + 1;

				/* The X position in the graph for pixels to be drawn. */
				_graphStartX = _width - 1;
				_graphMaxY = _height - 1;

				_image.rectTransform.sizeDelta = new Vector2(_width, _height);
				_graph.Resize(_width, _height);
				_graph.Clear();

				SetRTransformValues(0, 0, _width, _height, Vector2.one);
			}
		}
	}


	// ============================================================================================

	internal sealed class SysInfoView : StatsMonitorView
	{
		private int  _width;
		private int  _height;
		private Text _text;
		private bool _isDirty;

		
		internal SysInfoView(StatsMonitorWidget widget)
		{
			_widget = widget;
			Invalidate();
		}

		
		internal override void Reset()
		{
			_text.text = "";
		}

		
		internal override void Update()
		{
			if (!_isDirty) return;

			_text.text = StatsMonitorUtil.Concat("<color=#", StatsMonitorUtil.Color32ToHex(_widget.colorSysInfoOdd)
				, ">OS:", SystemInfo.operatingSystem
				, "</color>\n<color=#", StatsMonitorUtil.Color32ToHex(_widget.colorSysInfoEven)
				, ">CPU:", SystemInfo.processorType
				, " [", SystemInfo.processorCount.ToString(), " cores]"
				, "</color>\n<color=#", StatsMonitorUtil.Color32ToHex(_widget.colorSysInfoOdd)
				, ">GRAPHICS:", SystemInfo.graphicsDeviceName
				, "\nAPI:", SystemInfo.graphicsDeviceVersion
				, "\nShader Level:", SystemInfo.graphicsShaderLevel.ToString()
				, ", Video RAM:", SystemInfo.graphicsMemorySize.ToString(), " MB"
				, "</color>\n<color=#", StatsMonitorUtil.Color32ToHex(_widget.colorSysInfoEven)
				, ">SYSTEM RAM:", SystemInfo.systemMemorySize.ToString(), " MB"
				, "</color>\n<color=#", StatsMonitorUtil.Color32ToHex(_widget.colorSysInfoOdd)
				, ">SCREEN:", Screen.currentResolution.width.ToString(), " x "
				, Screen.currentResolution.height.ToString()
				, " @", Screen.currentResolution.refreshRate.ToString(), "Hz"
				, ",\nwindow size:", Screen.width.ToString(), " x ", Screen.height.ToString()
				, " ", Screen.dpi.ToString("F0"), "dpi</color>");

			_height = _widget.padding + (int) _text.preferredHeight + _widget.padding;
			Invalidate(UIInvalidationType.Layout);

			/* Invalidate stats monitor once more to update correct height but don't
			 * reinvalidate children or we'd be stuck in a loop! */
			_widget.Invalidate(UIInvalidationType.Layout, StatsMonitorWidget.StatsMonitorInvalidationFlag.Text, false);

			_isDirty = false;
		}

		
		internal override void Dispose()
		{
			Destroy(_text);
			_text = null;
			base.Dispose();
		}

		
		internal void SetWidth(float width)
		{
			_width = (int) width;
		}

		
		protected override GameObject CreateChildren()
		{
			var container = new GameObject { name = "SysInfoView" };
			container.transform.parent = _widget.transform;

			var g = new StatsMonitorGraphics(container, _widget.colorFPS, _widget.fontFace, _widget.fontSizeSmall);
			_text = g.CreateText("Text", "", null, 0, null, false);

			return container;
		}

		
		protected override void UpdateStyle()
		{
			_text.font = _widget.fontFace;
			_text.fontSize = _widget.FontSizeSmall;
			if (_widget.colorOutline.a > 0.0f)
				StatsMonitorGraphics.AddOutlineAndShadow(_text.gameObject, _widget.colorOutline);
			else
				StatsMonitorGraphics.RemoveEffects(_text.gameObject);
			_isDirty = true;
		}

		
		protected override void UpdateLayout()
		{
			var padding = _widget.padding;
			_text.rectTransform.anchoredPosition = new Vector2(padding, -padding);
			_text.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _width - (padding * 2));
			_height = padding + (int) _text.preferredHeight + padding;
			SetRTransformValues(0, 0, _width, _height, Vector2.one);
			_isDirty = true;
		}
	}
	
	
	// ----------------------------------------------------------------------------------------
	// Utils
	// ----------------------------------------------------------------------------------------
	
	public enum UIRenderMode : byte
	{
		Overlay,
		Camera
	}
	
	public enum UIAlignment : byte
	{
		UpperLeft,
		UpperCenter,
		UpperRight,
		MiddleRight,
		LowerRight,
		LowerCenter,
		LowerLeft,
		MiddleLeft
	}

	public enum UIInvalidationType : byte
	{
		All,
		Style,
		Layout
	}
	
	
	internal sealed class UIAnchor
	{
		internal Vector2 position;
		internal Vector2 min;
		internal Vector2 max;
		internal Vector2 pivot;

		internal UIAnchor(float x, float y, float minX, float minY, float maxX, float maxY, float pivotX, float pivotY)
		{
			position = new Vector2(x, y);
			min = new Vector2(minX, minY);
			max = new Vector2(maxX, maxY);
			pivot = new Vector2(pivotX, pivotY);
		}
	}


	internal sealed class UIAnchors
	{
		public readonly UIAnchor upperLeft = new UIAnchor(0, 0, 0, 1, 0, 1, 0, 1);
		public readonly UIAnchor upperCenter = new UIAnchor(0, 0, .5f, 1, .5f, 1, .5f, 1);
		public readonly UIAnchor upperRight = new UIAnchor(0, 0, 1, 1, 1, 1, 1, 1);
		public readonly UIAnchor middleRight = new UIAnchor(0, 0, 1, .5f, 1, .5f, 1, .5f);
		public readonly UIAnchor lowerRight = new UIAnchor(0, 0, 1, 0, 1, 0, 1, 0);
		public readonly UIAnchor lowerCenter = new UIAnchor(0, 0, .5f, 0, .5f, 0, .5f, 0);
		public readonly UIAnchor lowerLeft = new UIAnchor(0, 0, 0, 0, 0, 0, 0, 0);
		public readonly UIAnchor middleLeft = new UIAnchor(0, 0, 0, .5f, 0, .5f, 0, .5f);
		public readonly UIAnchor middleCenter = new UIAnchor(0, 0, .5f, .5f, .5f, .5f, .5f, .5f);
	}
	
	
	internal static class StatsMonitorUtil
	{
		private static readonly StringBuilder _stringBuilder = new StringBuilder(256);
		
		
		internal static string Concat(params string[] strings)
		{
			_stringBuilder.Length = 0;
			if (strings.Length < 1) return string.Empty;
			if (strings.Length == 1) return strings[0];
			for (var i = 0; i < strings.Length; i++) _stringBuilder.Append(strings[i]);
			return _stringBuilder.ToString();
		}
		
		
		internal static bool AddToUILayer(GameObject obj)
		{
			var uiLayerID = LayerMask.NameToLayer("UI");
			if (uiLayerID > -1)
			{
				obj.layer = uiLayerID;
				return true;
			}
			return false;
		}
		
		
		internal static float DPIScaleFactor(bool round = false)
		{
			var dpi = Screen.dpi;
			if (dpi <= 0) return -1.0f;
			var factor = dpi / 96.0f;
			if (factor < 1.0f) return 1.0f;
			return round ? Mathf.Round(factor) : factor;
		}
		
		
		internal static string Color32ToHex(Color32 color)
		{
			return color.r.ToString("x2") + color.g.ToString("x2") + color.b.ToString("x2") + color.a.ToString("x2");
		}
		
		
		internal static Color HexToColor32(string hex)
		{
			if (hex.Length < 1) return Color.black;
			return new Color32(byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber), byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber), byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber), byte.Parse(hex.Substring(6, 2), NumberStyles.HexNumber));
		}
		
		
		internal static float Normalize(float a, float min = 0.0f, float max = 255.0f)
		{
			if (max <= min) return 1.0f;
			return (Mathf.Clamp(a, min, max) - min) / (max - min);
		}
		
		
		internal static void RTransform(GameObject obj, Vector2 anchor, float x = 0, float y = 0, float w = 0, float h = 0)
		{
			var t = obj.GetComponent<RectTransform>();
			if (t == null) t = obj.AddComponent<RectTransform>();
			t.pivot = t.anchorMin = t.anchorMax = anchor;
			t.anchoredPosition = new Vector2(x, y);
			if (w > 0.0f) t.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
			if (h > 0.0f) t.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
		}
		
		
		internal static string StripHTMLTags(string s)
		{
			return Regex.Replace(s, "<.*?>", string.Empty);
		}
	}
	
	
	internal sealed class StatsMonitorGraphics
	{
		private readonly GameObject parent;
		private readonly Color defaultColor;
		private readonly Font defaultFontFace;
		private readonly int defaultFontSize;
		private static readonly Vector2 defaultEffectDistance = new Vector2(1, -1);

		
		internal StatsMonitorGraphics(GameObject parent, Color defaultColor, Font defaultFontFace = null, int defaultFontSize = 16)
		{
			this.parent = parent;
			this.defaultFontFace = defaultFontFace;
			this.defaultFontSize = defaultFontSize;
			this.defaultColor = defaultColor;
		}

		
		internal Graphic CreateGraphic(string name, Type type, float x = 0, float y = 0, float w = 0, float h = 0, Color? color = null)
		{
			var wrapper = new GameObject();
			wrapper.name = name;
			wrapper.transform.parent = parent.transform;
			var g = (Graphic) wrapper.AddComponent(type);
			g.color = color ?? defaultColor;
			var tr = wrapper.GetComponent<RectTransform>();
			if (tr == null) tr = wrapper.AddComponent<RectTransform>();
			tr.pivot = Vector2.up;
			tr.anchorMin = Vector2.up;
			tr.anchorMax = Vector2.up;
			tr.anchoredPosition = new Vector2(x, y);
			if (w > 0 && h > 0)
			{
				tr.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
				tr.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
			}
			return g;
		}


		internal Text CreateText(string name, string text = "", Color? color = null, int fontSize = 0, Font fontFace = null, bool fitH = true, bool fitV = true)
		{
			var t = (Text) CreateGraphic(name, typeof(Text), 0, 0, 0, 0, color);
			t.font = fontFace ? fontFace : defaultFontFace;
			t.fontSize = fontSize < 1 ? defaultFontSize : fontSize;
			if (fitH) t.horizontalOverflow = HorizontalWrapMode.Overflow;
			if (fitV) t.verticalOverflow = VerticalWrapMode.Overflow;
			t.text = text;
			if (fitH || fitV) FitText(t, fitH, fitV);
			return t;
		}

		
		internal static void FitText(Text text, bool h, bool v)
		{
			var csf = text.gameObject.GetComponent<ContentSizeFitter>();
			if (csf == null) csf = text.gameObject.AddComponent<ContentSizeFitter>();
			if (h) csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
			if (v) csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
		}


		internal static void AddOutlineAndShadow(GameObject obj, Color color, Vector2? distance = null)
		{
			var shadow = obj.GetComponent<Shadow>();
			if (shadow == null) shadow = obj.AddComponent<Shadow>();
			shadow.effectColor = color;
			shadow.effectDistance = distance ?? defaultEffectDistance;
			var outline = obj.GetComponent<Outline>();
			if (outline == null) outline = obj.AddComponent<Outline>();
			outline.effectColor = color;
			outline.effectDistance = distance ?? defaultEffectDistance;
		}

		
		internal static void RemoveEffects(GameObject obj)
		{
			var shadow = obj.GetComponent<Shadow>();
			if (shadow != null) Object.Destroy(shadow);
			var outline = obj.GetComponent<Outline>();
			if (outline != null) Object.Destroy(outline);
		}
	}
}
