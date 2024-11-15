using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MMDarkness.Editor.Draws;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MMDarkness.Editor
{
    public class ActionEditorWindow : EditorWindow
    {
        [NonSerialized]
        private float editorPreviousTime;

        [NonSerialized]
        internal bool WillRepaint;

        [NonSerialized]
        internal bool ShowDragDropInfo;

        public TimelineGraphAsset TimelineGraphAsset => App.GraphAsset;
        public TimelineGraphPreviewProcessor player => App.Player;

        int UID(int g, int t, int a)
        {
            var A = g.ToString("D3");
            var B = t.ToString("D3");
            var C = a.ToString("D4");
            return int.Parse(A + B + C);
        }


        #region 静态接口

        public static ActionEditorWindow current;

        public static void ShowWindow()
        {
            var window = GetWindow(typeof(ActionEditorWindow)) as ActionEditorWindow;
            if (window == null) return;
            window.InitializeAll();
            window.Show();
        }

        public static void CloseWindow()
        {
            var window = GetWindow(typeof(ActionEditorWindow)) as ActionEditorWindow;
            if (window == null) return;
            window.Close();
        }

        static bool Quit()
        {
            CloseWindow();
            return true;
        }

        #endregion

        #region 生命周期接口

        void OnEnable()
        {
            current = this;
            Styles.Load();

            EditorSceneManager.sceneSaving -= OnWillSaveScene;
            EditorSceneManager.sceneSaving += OnWillSaveScene;
#pragma warning disable 618
            EditorApplication.playmodeStateChanged -= InitializeAll;
            EditorApplication.playmodeStateChanged += InitializeAll;
#pragma warning restore

            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.update += OnEditorUpdate;

            EditorApplication.wantsToQuit -= Quit;
            EditorApplication.wantsToQuit += Quit;

            App.OnPlay -= OnPlay;
            App.OnPlay += OnPlay;

            App.OnStop -= OnStop;
            App.OnStop += OnStop;

            Tools.hidden = false;
            titleContent = new GUIContent(Lan.Title, Styles.CutsceneIconOpen);
            minSize = new Vector2(500, 250);

            WillRepaint = true;
            ShowDragDropInfo = true;

            pendingGuides = new List<GuideLine>();

            InitializeAll();
        }


        void OnDisable()
        {
            EditorSceneManager.sceneSaving -= OnWillSaveScene;

#pragma warning disable 618
            EditorApplication.playmodeStateChanged -= InitializeAll;
#pragma warning restore

            EditorApplication.update -= OnEditorUpdate;
            App.OnPlay -= OnPlay;

            Tools.hidden = false;
            if (App.GraphAsset != null && !Application.isPlaying)
            {
                App.Stop(true);
            }

            EditorApplication.wantsToQuit -= Quit;

            App.OnDisable?.Invoke();
        }


        void OnPlay()
        {
            editorPreviousTime = Time.realtimeSinceStartup;
        }

        void OnStop()
        {
            WillRepaint = true;
        }

        void OnEditorUpdate()
        {
            if (App.GraphAsset == null)
            {
                return;
            }

            if (EditorApplication.isCompiling)
            {
                App.Stop(true);
                return;
            }

            App.TryAutoSave();

            var delta = (Time.realtimeSinceStartup - editorPreviousTime) * Time.timeScale;

            editorPreviousTime = Time.realtimeSinceStartup;

            player.Sample();

            if (App.EditorPlaybackState == EditorPlaybackState.Stopped)
            {
                return;
            }

            if (player.CurrentTime >= App.GraphAsset.Length && App.EditorPlaybackState == EditorPlaybackState.PlayingForwards)
            {
                if (App.EditorPlaybackWrapMode == WrapMode.Once)
                {
                    App.Stop(true);
                    return;
                }

                if (App.EditorPlaybackWrapMode == WrapMode.Loop)
                {
                    player.Sample(0);
                    player.Sample(delta);
                    return;
                }
            }

            if (player.CurrentTime <= 0 && App.EditorPlaybackState == EditorPlaybackState.PlayingBackwards)
            {
                App.Stop(true);
                return;
            }

            player.CurrentTime += App.EditorPlaybackState == EditorPlaybackState.PlayingForwards ? delta : -delta;
        }

        void Update()
        {
            if (WillRepaint)
            {
                WillRepaint = false;
                Repaint(); //重新绘制窗口
            }

            player?.Sample();
        }

        void OnGUI()
        {
            G.Reset();
            GUI.skin.label.richText = true;
            GUI.skin.label.alignment = TextAnchor.UpperLeft;
            EditorStyles.label.richText = true;
            EditorStyles.textField.wordWrap = true;
            EditorStyles.foldout.richText = true;
            var e = Event.current;

            if (App.GraphAsset == null)
            {
                DrawUtils.Draw<WelcomeGUI>();
                return;
            }

            //避免编译时编辑
            if (EditorApplication.isCompiling)
            {
                App.Stop(true);
                ShowNotification(new GUIContent(Lan.CompilingTips));
                return;
            }


            if (e.type == EventType.MouseDown)
            {
                RemoveNotification();
            }


            //记录撤消和脏污？这是一个过度的回退。某些操作也会注册撤消。
            // var doRecordUndo = e.rawType == EventType.MouseDown && (e.button == 0 || e.button == 1);
            // doRecordUndo |= e.type == EventType.DragPerform;
            // if (doRecordUndo)
            // {
            //     Undo.RegisterFullObjectHierarchyUndo(cutscene.groupsRoot.gameObject, "Cutscene Change");
            //     Undo.RecordObject(cutscene, "Cutscene Change");
            //     willDirty = true;
            // }


            DrawUtils.Draw<HeaderGUI>();
            if (App.GraphAsset == null) return;
            DrawUtils.Draw<HeaderTimeInfoGUI>();
            DrawTimelineInfo();
            DrawUtils.Draw<BottomGUI>();
            DoKeyboardShortcuts();
        }


        void OnWillSaveScene(Scene scene, string path)
        {
            if (App.GraphAsset != null && player.CurrentTime > 0)
            {
                App.Stop(true);
                Debug.LogWarning(
                    "Scene Saved while a cutscene was in preview mode. Cutscene was reverted before saving the scene along with changes it affected.");
            }
        }

        #endregion

        #region Init

        void InitializeAll()
        {
            Lan.Load();
            Prefs.InitializeAssetTypes();
            App.OnInitialize?.Invoke();
            //停止播放
            if (App.GraphAsset != null)
            {
                if (!Application.isPlaying)
                {
                    App.Stop(true);
                }
            }

            WillRepaint = true;
        }

        #endregion

        #region 快捷键

        /// <summary>
        /// 监听执行鼠标快捷键
        /// </summary>
        void DoKeyboardShortcuts()
        {
            var e = Event.current;
            var isCtrl = e.control;
            var isShift = e.shift;
            var isAlt = e.alt;
            if (e.type == EventType.KeyDown && GUIUtility.keyboardControl == 0)
            {
                //Ctrl +
                if (isCtrl)
                {
                    if (e.keyCode == KeyCode.S)
                    {
                        App.AutoSave();
                    }
                }

                //play
                if (e.keyCode == KeyCode.Space && !e.shift)
                {
                    if (App.EditorPlaybackState != EditorPlaybackState.Stopped)
                    {
                        App.Pause();
                    }
                    else
                    {
                        App.Play();
                    }

                    e.Use();
                }

                if (e.keyCode == KeyCode.Escape)
                {
                    App.Stop(false);
                }

                //下一帧
                if (e.keyCode == KeyCode.Period || e.keyCode == KeyCode.RightArrow)
                {
                    App.StepForward();
                    e.Use();
                }

                //上一帧
                if (e.keyCode == KeyCode.Comma || e.keyCode == KeyCode.LeftArrow)
                {
                    App.StepBackward();
                    e.Use();
                }
            }
        }

        #endregion

        #region 时间轴

        private static bool isProSkin => EditorGUIUtility.isProSkin;


        private Action postWindowsGUI;

        private Vector2 scrollPos;
        internal Vector2 mousePosition;

        private Dictionary<int, ActionClipWrapper> clipWrappers = new();

        private Dictionary<ClipAsset, ActionClipWrapper> clipWrappersMap = new();

        ActionClipWrapper interactingClip;


        [NonSerialized]
        private Vector2? multiSelectStartPos;

        [NonSerialized]
        private Rect preMultiSelectionRetimeMinMax;

        [NonSerialized]
        private int multiSelectionScaleDirection;

        List<ActionClipWrapper> multiSelection;

        internal List<GuideLine> pendingGuides;

        internal float[] magnetSnapTimesCache;

        private float MAGNET_SNAP_INTERVAL => TimelineGraphAsset.ViewTime * 0.01f;

        private Section draggedSection;

        private bool isMovingScrubCarret;
        private bool isMovingEndCarret;
        private bool isMouseButton2Down;

        private Rect CenterRect;

        internal void SafeDoAction(Action call)
        {
            var time = player.CurrentTime;
            App.Stop(true);
            call();
            player.CurrentTime = time;
        }

        public void DrawTimelineInfo()
        {
            var e = Event.current;

            if (e.button == 2 && e.type == EventType.MouseDown)
            {
                isMouseButton2Down = true;
            }

            if (e.button == 2 && e.rawType == EventType.MouseUp)
            {
                isMouseButton2Down = false;
            }

            mousePosition = e.mousePosition;
            if (interactingClip == null && e.type == EventType.Layout)
            {
                foreach (var group in TimelineGraphAsset.groupAssets)
                {
                    foreach (var track in group.Tracks)
                    {
                        track.Clips = track.Clips.OrderBy(a => a.StartTime).ToList();
                    }
                }
            }

            CenterRect = G.CenterRect;

            DoScrubControls();
            DoZoomAndPan();

            var scrollRect1 = Rect.MinMaxRect(0, CenterRect.yMin, G.ScreenWidth, G.ScreenHeight - 5);
            var scrollRect2 = Rect.MinMaxRect(0, CenterRect.yMin, G.ScreenWidth, G.TotalHeight + 150);
            G.ScrollPos = GUI.BeginScrollView(scrollRect1, G.ScrollPos, scrollRect2);
            DrawUtils.Draw<GroupAndTrackListGUI>();
            ShowTimeLines(CenterRect);
            GUI.EndScrollView();

            DrawGuides();

            AcceptDrops();

            if (e.type == EventType.MouseDown && e.button == 0 && GUIUtility.hotControl == 0)
            {
                if (CenterRect.Contains(mousePosition))
                {
                    DirectorUtility.SelectedObject = null;
                    multiSelection = null;
                }

                GUIUtility.keyboardControl = 0;
                ShowDragDropInfo = false;
            }

            if (ShowDragDropInfo && TimelineGraphAsset.groupAssets.Find(g => g.GetType() == typeof(GroupAsset)) == null)
            {
                var label = "Drag & Drop GameObjects or Prefabs in this window to create Actor Groups";
                var size = new GUIStyle("label").CalcSize(new GUIContent(label));
                var notificationRect = new Rect(0, 0, size.x, size.y);
                notificationRect.center = new Vector2((G.ScreenWidth / 2) + (Styles.LeftMargin / 2), (G.ScreenHeight / 2) + Styles.TopMargin);
                GUI.Label(notificationRect, label);
            }

            if (e.type == EventType.MouseDrag || e.type == EventType.MouseUp || GUI.changed)
            {
                WillRepaint = true;
            }

            Handles.color = Color.black;
            Handles.DrawLine(new Vector2(0, CenterRect.y + 1), new Vector2(CenterRect.xMax, CenterRect.y + 1));
            Handles.DrawLine(new Vector2(CenterRect.x, CenterRect.y + 1), new Vector2(CenterRect.x, CenterRect.yMax));
            Handles.color = Color.white;

            GUI.color = Color.white;
            GUI.backgroundColor = Color.white;
            GUI.skin = null;
        }

        void AcceptDrops()
        {
            if (player.CurrentTime > 0)
            {
                return;
            }

            var e = Event.current;
            if (e.type == EventType.DragUpdated)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Link;
            }
        }

        #region Magnet Snap

        void CacheMagnetSnapTimes(ClipAsset clipAsset = null)
        {
            var result = new List<float>();
            result.Add(0);
            result.Add(TimelineGraphAsset.Length);
            result.Add(player.CurrentTime);

            foreach (var cw in clipWrappers)
            {
                var action = cw.Value.action;
                if (clipAsset == null || (action != clipAsset && action.Parent.Parent == clipAsset.Parent.Parent))
                {
                    result.Add(action.StartTime);
                    result.Add(action.EndTime);
                }
            }

            magnetSnapTimesCache = result.Distinct().ToArray();
        }

        float? MagnetSnapTime(float time, float[] snapTimes)
        {
            if (snapTimes == null)
            {
                return null;
            }

            var bestDistance = float.PositiveInfinity;
            var bestTime = float.PositiveInfinity;
            for (var i = 0; i < snapTimes.Length; i++)
            {
                var snapTime = snapTimes[i];
                var distance = Mathf.Abs(snapTime - time);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestTime = snapTime;
                }
            }

            if (Mathf.Abs(bestTime - time) <= MAGNET_SNAP_INTERVAL)
            {
                return bestTime;
            }

            return null;
        }

        #endregion

        #region 多选

        /// <summary>
        /// 多选
        /// </summary>
        void DoMultiSelection()
        {
            var e = Event.current;

            var r = new Rect();
            var bigEnough = false;
            if (multiSelectStartPos != null)
            {
                var start = (Vector2)multiSelectStartPos;
                if ((start - e.mousePosition).magnitude > 10)
                {
                    bigEnough = true;
                    r.xMin = Mathf.Max(Mathf.Min(start.x, e.mousePosition.x), 0);
                    r.xMax = Mathf.Min(Mathf.Max(start.x, e.mousePosition.x), G.ScreenWidth);
                    r.yMin = Mathf.Min(start.y, e.mousePosition.y);
                    r.yMax = Mathf.Max(start.y, e.mousePosition.y);
                    GUI.color = isProSkin ? Color.white : Color.white.WithAlpha(0.3f);
                    GUI.Box(r, string.Empty, Styles.HollowFrameStyle);
                    GUI.color = Color.white.WithAlpha(0.05f);
                    GUI.DrawTexture(r, Styles.WhiteTexture);
                    GUI.color = Color.white;
                    foreach (var wrapper in clipWrappers.Values.Where(b => r.Encapsulates(b.rect) && !b.action.IsLocked))
                    {
                        GUI.color = new Color(0.5f, 0.5f, 1, 0.5f);
                        GUI.Box(wrapper.rect, string.Empty, Styles.ClipBoxStyle);
                        GUI.color = Color.white;
                    }
                }
            }

            if (e.rawType == EventType.MouseUp)
            {
                if (bigEnough)
                {
                    multiSelection = clipWrappers.Values.Where(b => r.Encapsulates(b.rect) && !b.action.IsLocked).ToList();
                    if (multiSelection.Count == 1)
                    {
                        DirectorUtility.SelectedObject = multiSelection[0].action;
                        multiSelection = null;
                    }
                }

                multiSelectStartPos = null;
            }

            if (multiSelection != null)
            {
                var boundRect = RectUtility.GetBoundRect(multiSelection.Select(b => b.rect).ToArray()).ExpandBy(4);

                var leftDragRect = new Rect(boundRect.xMin - 6, boundRect.yMin, 4, boundRect.height);
                var rightDragRect = new Rect(boundRect.xMax + 2, boundRect.yMin, 4, boundRect.height);
                AddCursorRect(leftDragRect, MouseCursor.ResizeHorizontal);
                AddCursorRect(rightDragRect, MouseCursor.ResizeHorizontal);
                GUI.color = isProSkin ? new Color(0.7f, 0.7f, 0.7f) : Color.grey;
                GUI.DrawTexture(leftDragRect, Styles.WhiteTexture);
                GUI.DrawTexture(rightDragRect, Styles.WhiteTexture);
                GUI.color = Color.white;

                if (e.type == EventType.MouseDown && (leftDragRect.Contains(e.mousePosition) || rightDragRect.Contains(e.mousePosition)))
                {
                    multiSelectionScaleDirection = leftDragRect.Contains(e.mousePosition) ? -1 : 1;
                    var minTime = Mathf.Min(multiSelection.Select(b => b.action.StartTime).ToArray());
                    var maxTime = Mathf.Max(multiSelection.Select(b => b.action.EndTime).ToArray());
                    preMultiSelectionRetimeMinMax = Rect.MinMaxRect(minTime, 0, maxTime, 0);
                    foreach (var wrapper in multiSelection)
                    {
                        wrapper.BeginClipAdjust();
                    }

                    e.Use();
                }

                if (e.type == EventType.MouseDrag && multiSelectionScaleDirection != 0)
                {
                    foreach (var clipWrapper in multiSelection)
                    {
                        var preTimeMin = preMultiSelectionRetimeMinMax.xMin;
                        var preTimeMax = preMultiSelectionRetimeMinMax.xMax;
                        var pointerTime = G.SnapTime(TimelineGraphAsset.PosToTime(mousePosition.x));

                        var lerpMin = multiSelectionScaleDirection == -1 ? Mathf.Clamp(pointerTime, 0, preTimeMax) : preTimeMin;
                        var lerpMax = multiSelectionScaleDirection == 1 ? Mathf.Max(pointerTime, preTimeMin) : preTimeMax;

                        var normIn = Mathf.InverseLerp(preTimeMin, preTimeMax, clipWrapper.preScaleStartTime);
                        clipWrapper.action.StartTime = Mathf.Lerp(lerpMin, lerpMax, normIn);

                        var normOut = Mathf.InverseLerp(preTimeMin, preTimeMax, clipWrapper.preScaleEndTime);
                        clipWrapper.action.EndTime = Mathf.Lerp(lerpMin, lerpMax, normOut);
                    }

                    e.Use();
                }

                if (e.rawType == EventType.MouseUp)
                {
                    multiSelectionScaleDirection = 0;
                    foreach (var clipWrapper in multiSelection)
                    {
                        clipWrapper.EndClipAdjust();
                    }
                }
            }

            if (e.type == EventType.MouseDown && e.button == 0 && GUIUtility.hotControl == 0)
            {
                multiSelection = null;
                multiSelectStartPos = e.mousePosition;
            }

            GUI.color = Color.white;
        }

        #endregion

        #region 辅助标线

        /// <summary>
        /// 绘制时间参考线
        /// </summary>
        void DrawGuides()
        {
            //区间开始时间参考线
            DrawGuideLine(0, isProSkin ? Color.gray : Color.black);

            //区间结束时间参考线
            DrawGuideLine(TimelineGraphAsset.Length, isProSkin ? Color.white : Color.black);

            //当前播放帧的实际参考线
            if (player.CurrentTime > 0)
            {
                DrawGuideLine(player.CurrentTime, player.GetScriberColor());
            }

            // 开始和结束时间标线
            if (interactingClip != null)
            {
                if (interactingClip.isDragging || interactingClip.isScalingStart)
                {
                    DrawGuideLine(interactingClip.action.StartTime, Color.yellow.WithAlpha(0.2f));
                }

                if (interactingClip.isDragging || interactingClip.isScalingEnd)
                {
                    DrawGuideLine(interactingClip.action.EndTime, Color.yellow.WithAlpha(0.2f));
                }
            }

            if (draggedSection != null)
            {
                DrawGuideLine(draggedSection.time, draggedSection.color);
            }


            for (var i = 0; i < pendingGuides.Count; i++)
            {
                DrawGuideLine(pendingGuides[i].Time, pendingGuides[i].Color);
            }

            pendingGuides.Clear();
        }

        /// <summary>
        /// 绘制垂直参考线
        /// </summary>
        /// <param name="time"></param>
        /// <param name="color"></param>
        void DrawGuideLine(float time, Color color)
        {
            if (time >= TimelineGraphAsset.ViewTimeMin && time <= TimelineGraphAsset.ViewTimeMax)
            {
                var xPos = TimelineGraphAsset.TimeToPos(time);
                var guideRect = new Rect(xPos + CenterRect.x - 1, CenterRect.y, 1, CenterRect.height);
                GUI.color = color;
                GUI.DrawTexture(guideRect, Styles.WhiteTexture);
                GUI.color = Color.white;
            }
        }

        /// <summary>
        /// 添加拖动游标
        /// </summary>
        internal void AddCursorRect(Rect rect, MouseCursor type)
        {
            EditorGUIUtility.AddCursorRect(rect, type);
            WillRepaint = true;
        }

        #endregion

        #region 刻度控制

        /// <summary>
        /// 时间轴刻度控制
        /// </summary>
        void DoScrubControls()
        {
            if (player.IsActive)
            {
                return;
            }

            var e = Event.current;
            if (e.type == EventType.MouseDown && G.TopMiddleRect.Contains(mousePosition))
            {
                var carretPos = TimelineGraphAsset.TimeToPos(TimelineGraphAsset.Length) + G.LeftRect.width;
                var isEndCarret = Mathf.Abs(mousePosition.x - carretPos) < 10 || e.control;

                if (e.button == 0)
                {
                    isMovingEndCarret = isEndCarret;
                    isMovingScrubCarret = !isMovingEndCarret;
                    App.Pause();
                }

                if (e.button == 1 && isEndCarret && TimelineGraphAsset.Directables != null)
                {
                    var menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Set To Last Clip Time"), false, () =>
                    {
                        var lastClip = TimelineGraphAsset.Directables.Where(d => d is ClipAsset).OrderBy(d => d.EndTime).LastOrDefault();
                        if (lastClip != null)
                        {
                            TimelineGraphAsset.Length = lastClip.EndTime;
                        }
                    });
                    menu.ShowAsContext();
                }

                e.Use();
            }

            if (e.button == 0 && e.rawType == EventType.MouseUp)
            {
                isMovingScrubCarret = false;
                isMovingEndCarret = false;
            }

            var pointerTime = TimelineGraphAsset.PosToTime(mousePosition.x);
            if (isMovingScrubCarret)
            {
                player.CurrentTime = G.SnapTime(pointerTime);
                player.CurrentTime = Mathf.Clamp(player.CurrentTime, Mathf.Max(TimelineGraphAsset.ViewTimeMin, 0) + float.Epsilon,
                    TimelineGraphAsset.ViewTimeMax - float.Epsilon);
            }

            if (isMovingEndCarret)
            {
                TimelineGraphAsset.Length = G.SnapTime(pointerTime);
                var magnetSnap = MagnetSnapTime(TimelineGraphAsset.Length, magnetSnapTimesCache);
                TimelineGraphAsset.Length = magnetSnap != null ? magnetSnap.Value : TimelineGraphAsset.Length;
                TimelineGraphAsset.Length = Mathf.Clamp(TimelineGraphAsset.Length, TimelineGraphAsset.ViewTimeMin + float.Epsilon,
                    TimelineGraphAsset.ViewTimeMax - float.Epsilon);
            }
        }

        /// <summary>
        /// 时间轴缩放和平移操作
        /// </summary>
        void DoZoomAndPan()
        {
            if (!CenterRect.Contains(mousePosition))
            {
                return;
            }

            var e = Event.current;
            //缩放或向下/向上滚动，如果prefs设置为滚轮
            if ((e.type == EventType.ScrollWheel && Prefs.scrollWheelZooms) || (e.alt && !e.shift && e.button == 1))
            {
                AddCursorRect(CenterRect, MouseCursor.Zoom);
                if (e.type == EventType.MouseDrag || e.type == EventType.MouseDown || e.type == EventType.MouseUp || e.type == EventType.ScrollWheel)
                {
                    var pointerTimeA = TimelineGraphAsset.PosToTime(mousePosition.x);
                    var delta = e.alt ? -e.delta.x * 0.1f : e.delta.y;
                    var t = (Mathf.Abs(delta * 25) / CenterRect.width) * TimelineGraphAsset.ViewTime;
                    TimelineGraphAsset.ViewTimeMin += delta > 0 ? -t : t;
                    TimelineGraphAsset.ViewTimeMax += delta > 0 ? t : -t;
                    var pointerTimeB = TimelineGraphAsset.PosToTime(mousePosition.x + e.delta.x);
                    var diff = pointerTimeA - pointerTimeB;
                    TimelineGraphAsset.ViewTimeMin += diff;
                    TimelineGraphAsset.ViewTimeMax += diff;
                    e.Use();
                }
            }

            //左/右，上/下
            if (isMouseButton2Down || (e.alt && !e.shift && e.button == 0))
            {
                AddCursorRect(CenterRect, MouseCursor.Pan);
                if (e.type == EventType.MouseDrag || e.type == EventType.MouseDown || e.type == EventType.MouseUp)
                {
                    var t = (Mathf.Abs(e.delta.x) / CenterRect.width) * TimelineGraphAsset.ViewTime;
                    TimelineGraphAsset.ViewTimeMin += e.delta.x > 0 ? -t : t;
                    TimelineGraphAsset.ViewTimeMax += e.delta.x > 0 ? -t : t;
                    G.ScrollPos.y -= e.delta.y;
                    e.Use();
                }
            }
        }

        #endregion

        #region 时间轴

        //初始化动作剪辑包装器
        internal void InitClipWrappers()
        {
            if (TimelineGraphAsset == null)
            {
                return;
            }

            var lastTime = player.CurrentTime;

            if (!Application.isPlaying)
            {
                App.Stop(true);
            }

            clipWrappers = new Dictionary<int, ActionClipWrapper>();
            clipWrappersMap = new Dictionary<ClipAsset, ActionClipWrapper>();
            for (int g = 0; g < TimelineGraphAsset.groupAssets.Count; g++)
            {
                for (int t = 0; t < TimelineGraphAsset.groupAssets[g].Tracks.Count; t++)
                {
                    for (int a = 0; a < TimelineGraphAsset.groupAssets[g].Tracks[t].Clips.Count; a++)
                    {
                        var id = UID(g, t, a);
                        if (clipWrappers.ContainsKey(id))
                        {
                            Debug.LogError("Collided UIDs. This should really not happen but it did!");
                            continue;
                        }

                        var clip = TimelineGraphAsset.groupAssets[g].Tracks[t].Clips[a];
                        var wrapper = new ActionClipWrapper(clip);
                        clipWrappers[id] = wrapper;
                        clipWrappersMap[clip] = wrapper;
                    }
                }
            }

            player.InitializePreviewPointers();

            if (lastTime > 0)
            {
                player.CurrentTime = lastTime;
            }
        }

        /// <summary>
        /// 中间区域-时间轴显示区域
        /// </summary>
        /// <param name="centerRect"></param>
        void ShowTimeLines(Rect centerRect)
        {
            var e = Event.current;
            var bgRect = Rect.MinMaxRect(centerRect.xMin, Styles.TopMargin + Styles.ToolbarHeight + G.ScrollPos.y, centerRect.xMax,
                G.ScreenHeight - Styles.ToolbarHeight + G.ScrollPos.y);
            GUI.color = Color.black.WithAlpha(0.1f);
            GUI.DrawTexture(bgRect, Styles.WhiteTexture);
            GUI.color = Color.black.WithAlpha(0.03f);
            GUI.DrawTextureWithTexCoords(bgRect, Styles.Stripes, new Rect(0, 0, bgRect.width / -7, bgRect.height / -7));
            GUI.color = Color.white;

            // 绘制时间参考线
            for (var _i = G.timeInfoStart; _i <= G.timeInfoEnd; _i += G.timeInfoInterval)
            {
                var i = Mathf.Round(_i * 10) / 10;
                DrawGuideLine(i, Color.black.WithAlpha(0.05f));
                if (i % G.timeInfoHighMod == 0)
                {
                    DrawGuideLine(i, Color.black.WithAlpha(0.05f));
                }
            }

            GUI.BeginGroup(centerRect);

            var nextYPos = Styles.FirstGroupTopMargin;

            BeginWindows();

            for (int g = 0; g < TimelineGraphAsset.groupAssets.Count; g++)
            {
                var group = TimelineGraphAsset.groupAssets[g];
                ShowGroupArea(group, g, e, ref nextYPos);
            }

            EndWindows();

            if (postWindowsGUI != null)
            {
                postWindowsGUI();
                postWindowsGUI = null;
            }

            //片段多选
            DoMultiSelection();

            GUI.EndGroup();

            GUI.color = Color.white.WithAlpha(0.2f);
            GUI.Box(bgRect, string.Empty, Styles.ShadowBorderStyle);
            GUI.color = Color.white;

            //超出范围的变暗
            if (TimelineGraphAsset.ViewTimeMax > TimelineGraphAsset.Length)
            {
                var endPos = Mathf.Max(TimelineGraphAsset.TimeToPos(TimelineGraphAsset.Length) + G.LeftRect.width, centerRect.xMin);
                var darkRect = Rect.MinMaxRect(endPos, centerRect.yMin, centerRect.xMax, centerRect.yMax);
                GUI.color = Color.black.WithAlpha(0.3f);
                GUI.Box(darkRect, string.Empty, "TextField");
                GUI.color = Color.white;
            }

            //超出范围的变暗
            if (TimelineGraphAsset.ViewTimeMin < 0)
            {
                var startPos = Mathf.Min(TimelineGraphAsset.TimeToPos(0) + G.LeftRect.width, centerRect.xMax);
                var darkRect = Rect.MinMaxRect(centerRect.xMin, centerRect.yMin, startPos, centerRect.yMax);
                GUI.color = Color.black.WithAlpha(0.3f);
                GUI.Box(darkRect, string.Empty, "TextField");
                GUI.color = Color.white;
            }

            //确保剪辑不交叉
            if (e.rawType == EventType.MouseUp)
            {
                if (interactingClip != null)
                {
                    interactingClip.ResetInteraction();
                    interactingClip.EndClipAdjust();
                    interactingClip = null;
                }
            }
        }

        void ShowGroupArea(GroupAsset groupAsset, int groupIndex, Event e, ref float nextYPos)
        {
            if (G.IsFilteredOutBySearch(groupAsset))
            {
                groupAsset.IsCollapsed = true;
                return;
            }

            var groupRect = Rect.MinMaxRect(Mathf.Max(TimelineGraphAsset.TimeToPos(TimelineGraphAsset.ViewTimeMin), TimelineGraphAsset.TimeToPos(0)),
                nextYPos, TimelineGraphAsset.TimeToPos(TimelineGraphAsset.ViewTimeMax), nextYPos + Styles.GroupHeight);
            nextYPos += Styles.GroupHeight;

            if (groupAsset.IsCollapsed)
            {
                GUI.color = Color.black.WithAlpha(0.15f);
                var collapseRect = Rect.MinMaxRect(groupRect.xMin + 2, groupRect.yMin + 2, groupRect.xMax, groupRect.yMax - 4);
                GUI.DrawTexture(collapseRect, Styles.WhiteTexture);
                GUI.color = Color.grey.WithAlpha(0.5f);
                foreach (var track in groupAsset.Tracks)
                {
                    foreach (var clip in track.Clips)
                    {
                        var start = TimelineGraphAsset.TimeToPos(clip.StartTime);
                        var end = TimelineGraphAsset.TimeToPos(clip.EndTime);
                        GUI.DrawTexture(Rect.MinMaxRect(start + 0.5f, collapseRect.y + 2, end - 0.5f, collapseRect.yMax - 2), Styles.WhiteTexture);
                    }
                }

                GUI.color = Color.white;
                return;
            }

            for (int t = 0; t < groupAsset.Tracks.Count; t++)
            {
                var track = groupAsset.Tracks[t];
                ShowTrackArea(track, groupIndex, t, e, ref nextYPos);
            }

            if (ReferenceEquals(DirectorUtility.SelectedObject, groupAsset))
            {
                var r = Rect.MinMaxRect(groupRect.xMin, groupRect.yMin, groupRect.xMax, nextYPos);
                GUI.color = Color.grey;
                GUI.Box(r, string.Empty, Styles.HollowFrameHorizontalStyle);
                GUI.color = Color.white;
            }
        }

        void ShowTrackArea(TrackAsset trackAsset, int groupIndex, int trackIndex, Event e, ref float nextYPos)
        {
            var yPos = nextYPos;

            var trackPosRect =
                Rect.MinMaxRect(
                    Mathf.Max(TimelineGraphAsset.TimeToPos(TimelineGraphAsset.ViewTimeMin), TimelineGraphAsset.TimeToPos(trackAsset.StartTime)), yPos,
                    TimelineGraphAsset.TimeToPos(TimelineGraphAsset.ViewTimeMax), yPos + trackAsset.ShowHeight);

            nextYPos += trackAsset.ShowHeight + Styles.TrackMargins;

            //GRAPHICS
            GUI.color = Color.black.WithAlpha(isProSkin ? 0.06f : 0.1f);
            GUI.DrawTexture(trackPosRect, Styles.WhiteTexture);
            Handles.color = ColorUtility.Grey(isProSkin ? 0.15f : 0.4f);
            Handles.DrawLine(new Vector2(TimelineGraphAsset.TimeToPos(TimelineGraphAsset.ViewTimeMin), trackPosRect.y + 1),
                new Vector2(trackPosRect.xMax, trackPosRect.y + 1));
            Handles.DrawLine(new Vector2(TimelineGraphAsset.TimeToPos(TimelineGraphAsset.ViewTimeMin), trackPosRect.yMax),
                new Vector2(trackPosRect.xMax, trackPosRect.yMax));

            Handles.color = Color.white;
            if (TimelineGraphAsset.ViewTimeMin < 0)
            {
                //just visual clarity
                GUI.Box(
                    Rect.MinMaxRect(TimelineGraphAsset.TimeToPos(TimelineGraphAsset.ViewTimeMin), trackPosRect.yMin, TimelineGraphAsset.TimeToPos(0),
                        trackPosRect.yMax), string.Empty);
            }

            // Debug.Log($"track.parent={track.parent}");
            if (trackAsset.StartTime > trackAsset.Parent.StartTime || trackAsset.EndTime < trackAsset.Parent.EndTime)
            {
                Handles.color = Color.white;
                GUI.color = Color.black.WithAlpha(0.2f);
                if (trackAsset.StartTime > trackAsset.Parent.StartTime)
                {
                    var tStart = TimelineGraphAsset.TimeToPos(trackAsset.StartTime);
                    var r = Rect.MinMaxRect(TimelineGraphAsset.TimeToPos(0), yPos, tStart, yPos + trackAsset.ShowHeight);
                    GUI.DrawTexture(r, Styles.WhiteTexture);
                    GUI.DrawTextureWithTexCoords(r, Styles.Stripes, new Rect(0, 0, r.width / 7, r.height / 7));
                    var a = new Vector2(tStart, trackPosRect.yMin);
                    var b = new Vector2(a.x, trackPosRect.yMax);
                    Handles.DrawLine(a, b);
                }

                if (trackAsset.EndTime < trackAsset.Parent.EndTime)
                {
                    var tEnd = TimelineGraphAsset.TimeToPos(trackAsset.EndTime);
                    var r = Rect.MinMaxRect(tEnd, yPos, TimelineGraphAsset.TimeToPos(TimelineGraphAsset.Length), yPos + trackAsset.ShowHeight);
                    GUI.DrawTexture(r, Styles.WhiteTexture);
                    GUI.DrawTextureWithTexCoords(r, Styles.Stripes, new Rect(0, 0, r.width / 7, r.height / 7));
                    var a = new Vector2(tEnd, trackPosRect.yMin);
                    var b = new Vector2(a.x, trackPosRect.yMax);
                    Handles.DrawLine(a, b);
                }

                GUI.color = Color.white;
                Handles.color = Color.white;
            }

            GUI.backgroundColor = Color.white;

            //highlight selected track
            if (ReferenceEquals(DirectorUtility.SelectedObject, trackAsset))
            {
                GUI.color = Color.grey;
                GUI.Box(trackPosRect.ExpandBy(0, 2), string.Empty, Styles.HollowFrameHorizontalStyle);
                GUI.color = Color.white;
            }

            if (trackAsset.IsLocked)
            {
                if (e.isMouse && trackPosRect.Contains(e.mousePosition))
                {
                    e.Use();
                }
            }


            if (!trackAsset.IsActive || trackAsset.IsLocked)
            {
                postWindowsGUI += () =>
                {
                    //overlay dark stripes for disabled tracks
                    if (!trackAsset.IsActive)
                    {
                        GUI.color = Color.black.WithAlpha(0.2f);
                        GUI.DrawTexture(trackPosRect, Styles.WhiteTexture);
                        GUI.DrawTextureWithTexCoords(trackPosRect, Styles.Stripes,
                            new Rect(0, 0, (trackPosRect.width / 5), (trackPosRect.height / 5)));
                        GUI.color = Color.white;
                    }

                    //overlay light stripes for locked tracks
                    if (trackAsset.IsLocked)
                    {
                        GUI.color = Color.black.WithAlpha(0.15f);
                        GUI.DrawTextureWithTexCoords(trackPosRect, Styles.Stripes, new Rect(0, 0, trackPosRect.width / 20, trackPosRect.height / 20));
                        GUI.color = Color.white;
                    }

                    if (isProSkin)
                    {
                        string overlayLabel = null;
                        if (!trackAsset.IsActive && trackAsset.IsLocked)
                        {
                            overlayLabel = $"{Lan.Disable} & {Lan.Locked}";
                        }
                        else
                        {
                            if (!trackAsset.IsActive)
                            {
                                overlayLabel = Lan.Disable;
                            }

                            if (trackAsset.IsLocked)
                            {
                                overlayLabel = Lan.Locked;
                            }
                        }

                        var size = Styles.CenterLabel.CalcSize(new GUIContent(overlayLabel));
                        var bgLabelRect = new Rect(0, 0, size.x, size.y);
                        bgLabelRect.center = trackPosRect.center;
                        GUI.Label(trackPosRect, $"<b>{overlayLabel}</b>", Styles.CenterLabel);
                        GUI.color = Color.white;
                    }
                };
            }

            //绘制轨道片段
            for (int a = 0; a < trackAsset.Clips.Count; a++)
            {
                ShowClipArea(trackAsset, yPos, groupIndex, trackIndex, a, e);
            }

            var cursorTime = G.SnapTime(TimelineGraphAsset.PosToTime(mousePosition.x));

            TrackDraw.DrawTrackContextMenu(trackAsset, e, trackPosRect, cursorTime);
        }

        void ShowClipArea(TrackAsset trackAsset, float nextYPos, int groupIndex, int trackIndex, int clipIndex, Event e)
        {
            var action = trackAsset.Clips[clipIndex];
            var ID = UID(groupIndex, trackIndex, clipIndex);

            if (!clipWrappers.TryGetValue(ID, out var clipWrapper) || clipWrapper.action != action)
            {
                InitClipWrappers();
                clipWrapper = clipWrappers[ID];
            }

            //查找并存储下一个/上一个剪辑到包装器
            var nextClip = clipIndex < trackAsset.Clips.Count - 1 ? trackAsset.Clips[clipIndex + 1] : null;
            var previousClip = clipIndex != 0 ? trackAsset.Clips[clipIndex - 1] : null;
            clipWrapper.NextClipAsset = nextClip;
            clipWrapper.PreviousClipAsset = previousClip;
            var clipRect = clipWrapper.rect;
            clipRect.y = nextYPos;
            clipRect.width = Mathf.Max(action.Length / TimelineGraphAsset.ViewTime * CenterRect.width, 6);
            clipRect.height = trackAsset.ShowHeight;
            //获取动作时间和位置
            var xTime = action.StartTime;
            if (interactingClip != null && ReferenceEquals(interactingClip.action, action) && interactingClip.isDragging)
            {
                var mx = e.mousePosition.x - interactingClip.dragOffset;

                var lastTime = xTime;
                xTime = TimelineGraphAsset.PosToTime(mx + G.LeftRect.width);
                xTime = G.SnapTime(xTime);
                xTime = Mathf.Clamp(xTime, 0, TimelineGraphAsset.MaxTime - 0.1f);

                // 处理multisection。限制xmin和xmax的边界
                if (multiSelection != null && multiSelection.Count > 1)
                {
                    var delta = xTime - lastTime;
                    var boundMin = Mathf.Min(multiSelection.Select(b => b.action.StartTime).ToArray());
                    if (boundMin + delta < 0)
                    {
                        xTime -= delta;
                        delta = 0;
                    }

                    foreach (var cw in multiSelection)
                    {
                        if (cw.action != action)
                        {
                            cw.action.StartTime += delta;
                        }
                    }
                }

                //夹紧和交叉混合之间的其他附近的剪辑
                if (multiSelection == null || multiSelection.Count < 1)
                {
                    var cursorTime = G.SnapTime(TimelineGraphAsset.PosToTime(mousePosition.x));

                    var preCursorClip = trackAsset.Clips.LastOrDefault(x => x != action && x.StartTime < cursorTime);
                    var postCursorClip = trackAsset.Clips.FirstOrDefault(x => x != action && x.EndTime > cursorTime);

                    //转变/涟漪剪辑
                    //当移动轨道夹子总是夹到以前的夹子，不需要夹到下一个
                    if (e.shift)
                    {
                        preCursorClip = previousClip;
                        postCursorClip = null;
                    }

                    var preTime = preCursorClip != null ? preCursorClip.EndTime : 0;
                    var postTime = postCursorClip != null ? postCursorClip.StartTime : TimelineGraphAsset.MaxTime + action.Length;

                    //拖拽夹子时磁铁会断裂
                    if (Prefs.magnetSnapping && !e.control)
                    {
                        var snapStart = MagnetSnapTime(xTime, magnetSnapTimesCache);
                        var snapEnd = MagnetSnapTime(xTime + action.Length, magnetSnapTimesCache);
                        if (snapStart != null && snapEnd != null)
                        {
                            var distStart = Mathf.Abs(snapStart.Value - xTime);
                            var distEnd = Mathf.Abs(snapEnd.Value - (xTime + action.Length));
                            var bestTime = distEnd < distStart ? snapEnd.Value : snapStart.Value;
                            pendingGuides.Add(new GuideLine(bestTime, Color.white));
                            xTime = distEnd < distStart ? snapEnd.Value - action.Length : snapStart.Value;
                        }
                        else
                        {
                            if (snapEnd != null)
                            {
                                pendingGuides.Add(new GuideLine(snapEnd.Value, Color.white));
                                xTime = snapEnd.Value - action.Length;
                            }

                            if (snapStart != null)
                            {
                                pendingGuides.Add(new GuideLine(snapStart.Value, Color.white));
                                xTime = snapStart.Value;
                            }
                        }
                    }


                    //如果可混合，则延长可能的时间
                    if (action.CanCrossBlend(preCursorClip))
                    {
                        preTime -= Mathf.Min(action.Length / 2, preCursorClip.Length / 2);
                    }

                    if (action.CanCrossBlend(postCursorClip))
                    {
                        postTime += Mathf.Min(action.Length / 2, postCursorClip.Length / 2);
                    }

                    //合身吗?
                    if (action.Length > postTime - preTime)
                    {
                        xTime = lastTime;
                    }

                    if (xTime != lastTime)
                    {
                        xTime = Mathf.Clamp(xTime, preTime, postTime - action.Length);
                        //Shift all the next clips along with this one if shift is down
                        if (e.shift)
                        {
                            foreach (var cw in clipWrappers.Values.Where(c =>
                                         c.action.Parent == action.Parent && c.action != action && c.action.StartTime > lastTime))
                            {
                                cw.action.StartTime += xTime - lastTime;
                            }
                        }
                    }
                }

                //Apply xTime
                action.StartTime = xTime;
            }

            //apply xPos
            clipRect.x = TimelineGraphAsset.TimeToPos(xTime);

            //dont draw if outside of view range and not selected
            var isSelected = ReferenceEquals(DirectorUtility.SelectedObject, action) ||
                             (multiSelection != null && multiSelection.Select(b => b.action).Contains(action));
            var isVisible = Rect.MinMaxRect(0, G.ScrollPos.y, CenterRect.width, CenterRect.height).Overlaps(clipRect);
            if (!isSelected && !isVisible)
            {
                //we basicaly "nullify" the rect. Too much trouble to work with nullable rect.
                clipWrapper.rect = default(Rect);
                return;
            }

            //draw selection graphics rect
            if (isSelected)
            {
                var selRect = clipRect.ExpandBy(2);
                GUI.color = Styles.HighlightColor;
                GUI.DrawTexture(selRect, Styles.WhiteTexture);
                GUI.color = Color.white;
            }

            //determine color and draw clip
            var color = Color.white;
            color = action.IsValid ? color : new Color(1, 0.3f, 0.3f);
            color = trackAsset.IsActive ? color : Color.grey;
            GUI.color = color;
            GUI.Box(clipRect, string.Empty, Styles.ClipBoxHorizontalStyle);
            clipWrapper.rect = clipRect;
            ShowActionClipWindow(ID, clipRect);

            //右键菜单
            if (e.type == EventType.ContextClick && clipRect.Contains(e.mousePosition))
            {
                var menu = new GenericMenu();
                if (multiSelection != null && multiSelection.FindIndex(a => a.action == action) != -1)
                {
                    menu.AddItem(new GUIContent(Lan.ClipDelete), false, () =>
                    {
                        SafeDoAction(() =>
                        {
                            foreach (var act in multiSelection.Select(b => b.action).ToArray())
                            {
                                if (act.Parent is TrackAsset parent)
                                    parent.DeleteClip(act);
                            }

                            InitClipWrappers();
                            multiSelection = null;
                        });
                    });

                    menu.ShowAsContext();
                    e.Use();
                    return;
                }

                menu.AddItem(new GUIContent(Lan.ClipCopy), false, () => { DirectorUtility.CopyClipAsset = action; });
                menu.AddItem(new GUIContent(Lan.ClipCut), false, () => { DirectorUtility.CutClip(action); });

                if (action is ISubClipContainable subContainable && subContainable.SubClipLength > 0)
                {
                    menu.AddSeparator("");
                    menu.AddItem(new GUIContent(Lan.MatchPreviousLoop), false, () => { action.TryMatchPreviousSubClipLoop(); });
                    menu.AddItem(new GUIContent(Lan.MatchClipLength), false, () => { action.TryMatchSubClipLength(); });
                    menu.AddItem(new GUIContent(Lan.MatchNextLoop), false, () => { action.TryMatchNexSubClipLoop(); });
                }

                menu.AddSeparator("/");

                menu.AddItem(new GUIContent(Lan.ClipDelete), false, () =>
                {
                    SafeDoAction(() =>
                    {
                        if (action.Parent is TrackAsset parent)
                            parent.DeleteClip(action);
                        InitClipWrappers();
                    });
                });

                menu.ShowAsContext();
                e.Use();
            }

            if (!isProSkin)
            {
                GUI.color = Color.white.WithAlpha(0.5f);
                GUI.Box(clipRect, string.Empty);
                GUI.color = Color.white;
            }

            GUI.color = Color.white;

            var nextPosX = TimelineGraphAsset.TimeToPos(nextClip != null ? nextClip.StartTime : TimelineGraphAsset.ViewTimeMax);
            var prevPosX = TimelineGraphAsset.TimeToPos(previousClip != null ? previousClip.EndTime : TimelineGraphAsset.ViewTimeMin);
            var extRectLeft = Rect.MinMaxRect(prevPosX, clipRect.yMin, clipRect.xMin, clipRect.yMax);
            var extRectRight = Rect.MinMaxRect(clipRect.xMax, clipRect.yMin, nextPosX, clipRect.yMax);
            action.ShowClipGUIExternal(extRectLeft, extRectRight);

            //如果剪辑太小，则把说明信息绘制在外部
            if (clipRect.width <= 20)
            {
                GUI.Label(extRectRight, $"<size=9>{action.Info}</size>");
            }
        }

        void ShowActionClipWindow(int id, Rect rect)
        {
            if (clipWrappers.TryGetValue(id, out ActionClipWrapper wrapper))
            {
                GUI.BeginClip(rect);
                wrapper.OnClipGUI(id);
                GUI.EndClip();
            }
        }

        /// <summary>
        /// 轨道片段包装器
        /// </summary>
        sealed class ActionClipWrapper
        {
            const float CLIP_BLOCK_COLOR_HEIGHT = 4f;
            const float SCALE_RECT_WIDTH = 5;

            public float dragOffset;
            public ClipAsset action;
            public bool isDragging;
            public bool isScalingStart;
            public bool isScalingEnd;
            public bool isControlingBlendIn;
            public bool isControlingBlendOut;

            public float preScaleStartTime;
            public float preScaleEndTime;

            public ClipAsset PreviousClipAsset;
            public ClipAsset NextClipAsset;

            private Event e;
            private int clipID;
            private bool isWaitingMouseDrag;
            private float overlapIn;
            private float overlapOut;
            private float blendInPosX;

            private float blendOutPosX;

            private float pointerTime;
            private float snapedPointerTime;
            private bool allowScale;

            private Rect dragRect;
            private Rect controlRectIn;
            private Rect controlRectOut;

            private ActionEditorWindow editor => current;

            private List<ActionClipWrapper> multiSelection
            {
                get => editor.multiSelection;
                set => editor.multiSelection = value;
            }

            private Rect _rect;

            public Rect rect
            {
                get => action.IsCollapsed ? default : _rect;
                set => _rect = value;
            }

            public ActionClipWrapper(ClipAsset action)
            {
                this.action = action;
            }

            public void ResetInteraction()
            {
                isWaitingMouseDrag = false;
                isDragging = false;
                isControlingBlendIn = false;
                isControlingBlendOut = false;
                isScalingStart = false;
                isScalingEnd = false;
            }

            public void OnClipGUI(int clipId)
            {
                clipID = clipId;
                e = Event.current;

                overlapIn = PreviousClipAsset != null ? Mathf.Max(PreviousClipAsset.EndTime - action.StartTime, 0) : 0;
                overlapOut = NextClipAsset != null ? Mathf.Max(action.EndTime - NextClipAsset.StartTime, 0) : 0;
                blendInPosX = (action.BlendIn / action.Length) * rect.width;
                blendOutPosX = ((action.Length - action.BlendOut) / action.Length) * rect.width;

                pointerTime = editor.TimelineGraphAsset.PosToTime(editor.mousePosition.x);
                snapedPointerTime = G.SnapTime(pointerTime);

                allowScale = action.CanScale() && action.Length > 0 && rect.width > SCALE_RECT_WIDTH * 2;

                var minusHeight = rect.height;

                dragRect = new Rect(0, 0, rect.width, minusHeight).ExpandBy(allowScale ? -SCALE_RECT_WIDTH : 0, 0);
                controlRectIn = new Rect(0, 0, SCALE_RECT_WIDTH, minusHeight);
                controlRectOut = new Rect(rect.width - SCALE_RECT_WIDTH, 0, SCALE_RECT_WIDTH, minusHeight);

                editor.AddCursorRect(dragRect, MouseCursor.Link);
                if (allowScale) //增加拖动游标
                {
                    editor.AddCursorRect(controlRectIn, MouseCursor.ResizeHorizontal);
                    editor.AddCursorRect(controlRectOut, MouseCursor.ResizeHorizontal);
                }

                var wholeRect = new Rect(0, 0, rect.width, rect.height);
                if (action.IsLocked && e.isMouse && wholeRect.Contains(e.mousePosition))
                {
                    e.Use();
                }

                action.ShowClipGUI(wholeRect);
                if (action is ISubClipContainable subClip)
                {
                    DrawUtils.DrawLoopedLines(wholeRect, subClip.SubClipLength / subClip.SubClipSpeed, action.Length, subClip.SubClipOffset);
                }

                ShowClipBlockColor(wholeRect);

                //设置交叉混合的重叠属性。当没有剪辑互动或没有剪辑拖拽时做这个
                //这种方式避免了在另一边移动剪辑时的问题，但至少在缩放剪辑时保持重叠交互。
                if (editor.interactingClip == null || !editor.interactingClip.isDragging)
                {
                    var overlap = PreviousClipAsset != null ? Mathf.Max(PreviousClipAsset.EndTime - action.StartTime, 0) : 0;
                    if (overlap > 0)
                    {
                        action.BlendIn = overlap;
                        PreviousClipAsset.BlendOut = overlap;
                    }
                }

                if (e.type == EventType.MouseDown)
                {
                    if (e.button == 0)
                    {
                        if (dragRect.Contains(e.mousePosition))
                        {
                            isWaitingMouseDrag = true;
                        }

                        editor.interactingClip = this;
                        editor.CacheMagnetSnapTimes(action);
                    }

                    if (e.control && dragRect.Contains(e.mousePosition))
                    {
                        if (multiSelection == null)
                        {
                            multiSelection = new List<ActionClipWrapper> { this };
                        }

                        if (multiSelection.Contains(this))
                        {
                            multiSelection.Remove(this);
                        }
                        else
                        {
                            multiSelection.Add(this);
                        }
                    }
                    else
                    {
                        DirectorUtility.SelectedObject = action;
                        if (multiSelection != null && !multiSelection.Select(cw => cw.action).Contains(action))
                        {
                            multiSelection = null;
                        }
                    }
                }

                if (e.type == EventType.MouseDrag && isWaitingMouseDrag)
                {
                    if (!isDragging) dragOffset = e.mousePosition.x;
                    isDragging = true;
                }

                DrawBlendGraphics();
                DoEdgeControls();

                if (e.rawType == EventType.MouseUp)
                {
                    if (editor.interactingClip != null)
                    {
                        editor.interactingClip.EndClipAdjust();
                        editor.interactingClip.ResetInteraction();
                        editor.interactingClip = null;
                    }
                }
                else if (e.type == EventType.MouseDown)
                {
                    if (DirectorUtility.SelectedObject == action)
                    {
                        e.Use();
                    }
                }


                //宽度足够，显示文本信息
                if (rect.width > 20)
                {
                    var r = new Rect(0, 0, rect.width, rect.height);
                    if (overlapIn > 0)
                    {
                        r.xMin = blendInPosX;
                    }

                    if (overlapOut > 0)
                    {
                        r.xMax = blendOutPosX;
                    }

                    var label = $"<size=10>{action.Info}</size>";
                    GUI.color = Color.black;
                    GUI.Label(r, label);
                    GUI.color = Color.white;
                }
            }

            /// <summary>
            /// 绘制片段混合图形
            /// </summary>
            void DrawBlendGraphics()
            {
                if (action.BlendIn > 0)
                {
                    Handles.color = Color.black.WithAlpha(0.5f);
                    Handles.DrawAAPolyLine(2, new Vector2(0, rect.height), new Vector2(blendInPosX, 0));
                    Handles.color = Color.black.WithAlpha(0.3f);
                    Handles.DrawAAConvexPolygon(new Vector3(0, 0), new Vector3(0, rect.height), new Vector3(blendInPosX, 0));
                }

                if (action.BlendOut > 0 && overlapOut == 0)
                {
                    Handles.color = Color.black.WithAlpha(0.5f);
                    Handles.DrawAAPolyLine(2, new Vector2(blendOutPosX, 0), new Vector2(rect.width, rect.height));
                    Handles.color = Color.black.WithAlpha(0.3f);
                    Handles.DrawAAConvexPolygon(new Vector3(rect.width, 0), new Vector2(blendOutPosX, 0), new Vector2(rect.width, rect.height));
                }

                if (overlapIn > 0)
                {
                    Handles.color = Color.black;
                    Handles.DrawAAPolyLine(2, new Vector2(blendInPosX, 0), new Vector2(blendInPosX, rect.height));
                }

                Handles.color = Color.white;
            }

            /// <summary>
            /// 片段长度控制器
            /// </summary>
            void DoEdgeControls()
            {
                var canBlendIn = action.CanBlendIn() && action.Length > 0;
                var canBlendOut = action.CanBlendOut() && action.Length > 0;
                if (!isScalingStart && !isScalingEnd && !isControlingBlendIn && !isControlingBlendOut)
                {
                    if (allowScale || canBlendIn)
                    {
                        if (controlRectIn.Contains(e.mousePosition))
                        {
                            GUI.DrawTexture(controlRectIn.ExpandBy(0, -2), Styles.WhiteTexture);
                            if (e.type == EventType.MouseDown && e.button == 0)
                            {
                                if (allowScale && !e.control)
                                {
                                    isScalingStart = true;
                                }

                                if (canBlendIn && e.control)
                                {
                                    isControlingBlendIn = true;
                                }

                                BeginClipAdjust();
                                e.Use();
                            }
                        }
                    }

                    if (allowScale || canBlendOut)
                    {
                        if (controlRectOut.Contains(e.mousePosition))
                        {
                            GUI.DrawTexture(controlRectOut.ExpandBy(0, -2), Styles.WhiteTexture);
                            if (e.type == EventType.MouseDown && e.button == 0)
                            {
                                if (allowScale && !e.control)
                                {
                                    isScalingEnd = true;
                                }

                                if (canBlendOut && e.control)
                                {
                                    isControlingBlendOut = true;
                                }

                                BeginClipAdjust();
                                e.Use();
                            }
                        }
                    }
                }

                if (isControlingBlendIn)
                {
                    action.BlendIn = Mathf.Clamp(pointerTime - action.StartTime, 0, action.Length - action.BlendOut);
                }

                if (isControlingBlendOut)
                {
                    action.BlendOut = Mathf.Clamp(action.EndTime - pointerTime, 0, action.Length - action.BlendIn);
                }

                if (isScalingStart)
                {
                    var prevTime = PreviousClipAsset?.EndTime ?? 0;
                    //magnet snap
                    if (Prefs.magnetSnapping && !e.control)
                    {
                        var snapStart = editor.MagnetSnapTime(snapedPointerTime, editor.magnetSnapTimesCache);
                        if (snapStart != null)
                        {
                            snapedPointerTime = snapStart.Value;
                            editor.pendingGuides.Add(new GuideLine(snapedPointerTime, Color.white));
                        }
                    }

                    if (action.CanCrossBlend(PreviousClipAsset))
                    {
                        prevTime -= Mathf.Min(action.Length / 2, PreviousClipAsset.Length / 2);
                    }

                    action.StartTime = snapedPointerTime;
                    action.StartTime = Mathf.Clamp(action.StartTime, prevTime, preScaleEndTime);
                    action.EndTime = preScaleEndTime;
                }

                if (isScalingEnd)
                {
                    var nextTime = NextClipAsset != null ? NextClipAsset.StartTime : editor.TimelineGraphAsset.MaxTime;
                    //magnet snap
                    if (Prefs.magnetSnapping && !e.control)
                    {
                        var snapEnd = editor.MagnetSnapTime(snapedPointerTime, editor.magnetSnapTimesCache);
                        if (snapEnd != null)
                        {
                            snapedPointerTime = snapEnd.Value;
                            editor.pendingGuides.Add(new GuideLine(snapedPointerTime, Color.white));
                        }
                    }

                    if (action.CanCrossBlend(NextClipAsset))
                    {
                        nextTime += Mathf.Min(action.Length / 2, NextClipAsset.Length / 2);
                    }

                    action.EndTime = snapedPointerTime;
                    action.EndTime = Mathf.Clamp(action.EndTime, 0, nextTime);
                }
            }


            public void BeginClipAdjust()
            {
                preScaleStartTime = action.StartTime;
                preScaleEndTime = action.EndTime;
                // // preScaleKeys = action.GetCurvesAll().ToDictionary(k => k, k => k.keys);
                // if (action is ISubClipContainable containable)
                // {
                //     preScaleSubclipOffset = containable.subClipOffset;
                //     preScaleSubclipSpeed = containable.subClipSpeed;
                // }

                editor.CacheMagnetSnapTimes(action);
            }

            public void EndClipAdjust()
            {
                // preScaleKeys = null;
                // if (Prefs.autoCleanKeysOffRange)
                // {
                //     // CleanKeysOffRange();
                // }
            }

            /// <summary>
            /// 显示剪辑的底部色块
            /// </summary>
            /// <param name="rect"></param>
            void ShowClipBlockColor(Rect rect)
            {
                var colorAttribute = action.GetType().GetCustomAttribute<ColorAttribute>();
                if (colorAttribute != null)
                {
                    var dopeRect = new Rect(0, rect.height - CLIP_BLOCK_COLOR_HEIGHT, rect.width, CLIP_BLOCK_COLOR_HEIGHT);
                    GUI.color = colorAttribute.Color;
                    GUI.Box(dopeRect, string.Empty, Styles.HeaderBoxStyle);
                    GUI.color = Color.white;
                }
            }
        }

        #endregion

        #endregion
    }
}