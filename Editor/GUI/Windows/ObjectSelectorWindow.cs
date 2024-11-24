using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MMDarkness.Editor
{
    /// <summary>
    ///     对象选择器窗口
    /// </summary>
    public class ObjectSelectorWindow : EditorWindow
    {
        private static ObjectSelectorWindow _sharedObjectSelector;
        private BuiltinRes[] _activeBuiltinList;
        private SerializedProperty _cacheProperty;
        private BuiltinRes[] _currentBuiltinResources;
        private EditorWrapperCache _editorWrapperCache;
        private bool _focusSearchFilter;
        private string _folderPath;
        private Action<Object> _itemSelectedCallback;
        private int _lastSelectedIdx;
        private Object _lastSelectedObject;
        private Texture2D _lastSelectedObjectIcon;
        private BuiltinRes _noneBuiltinRes;
        private readonly PreviewResizer _previewResizer = new();
        private float _previewSize = 101f;
        private Vector2 _scrollPosition;
        private string _searchFilter;
        private bool _showNoneItem;

        private Styles _styles;
        private readonly float _toolbarHeight = 44f;
        private float _topSize;

        public static ObjectSelectorWindow get
        {
            get
            {
                if (_sharedObjectSelector == null)
                {
                    var array = Resources.FindObjectsOfTypeAll(typeof(ObjectSelectorWindow));
                    if (array != null && array.Length > 0) _sharedObjectSelector = (ObjectSelectorWindow)array[0];

                    if (_sharedObjectSelector == null) _sharedObjectSelector = CreateInstance<ObjectSelectorWindow>();
                }

                return _sharedObjectSelector;
            }
        }

        /// <summary>
        ///     列表项绘制区域
        /// </summary>
        private Rect listPosition =>
            new(0f, _toolbarHeight, position.width, Mathf.Max(0f, _topSize - _toolbarHeight));

        /// <summary>
        ///     完全文件夹路径
        /// </summary>
        private string folderFullPath =>
            Path.Combine(Application.dataPath, _folderPath.Length > 6 ? _folderPath.Substring(7) : string.Empty);

        /// <summary>
        ///     显示的列表项数量
        /// </summary>
        private int itemCount
        {
            get
            {
                var num2 = _activeBuiltinList.Length;
                var num3 = !_showNoneItem ? 0 : 1;
                return num2 + num3;
            }
        }

        private void OnEnable()
        {
            _previewResizer.Init("ObjectSelectorWindow");
            _previewSize = _previewResizer.GetPreviewSize();
        }

        private void OnDisable()
        {
            _itemSelectedCallback = null;
            _currentBuiltinResources = null;
            _activeBuiltinList = null;
            _lastSelectedObject = null;
            _lastSelectedObjectIcon = null;
            if (_sharedObjectSelector == this) _sharedObjectSelector = null;

            if (_editorWrapperCache != null) _editorWrapperCache.Dispose();
        }

        private void OnGUI()
        {
            OnObjectListGUI();
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape) Cancel();
        }

        private static string SearchField(Rect position, string text)
        {
            var position2 = position;
            position2.width -= 15f;
            text = EditorGUI.TextField(position2, text, new GUIStyle("SearchTextField"));
            var position3 = position;
            position3.x += position.width - 15f;
            position3.width = 15f;
            if (GUI.Button(position3, GUIContent.none,
                    string.IsNullOrEmpty(text) ? "SearchCancelButtonEmpty" : "SearchCancelButton"))
            {
                text = string.Empty;
                GUIUtility.keyboardControl = 0;
            }

            return text;
        }

        /// <summary>
        ///     显示对象选择器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">初始的对象</param>
        /// <param name="itemSelectedCallback">列表项选中回调</param>
        /// <param name="folderPath">所属的文件夹路径</param>
        /// <param name="allowedInstanceIDs"></param>
        public static void ShowObjectPicker<T>(Object obj, Action<Object> itemSelectedCallback,
            string folderPath = "Assets", List<int> allowedInstanceIDs = null) where T : Object
        {
            var typeFromHandle = typeof(T);
            get.Show(obj, typeFromHandle, null, itemSelectedCallback, folderPath, allowedInstanceIDs);
        }

        /// <summary>
        ///     显示对象选择器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">初始的对象</param>
        /// <param name="requiredTypeName"></param>
        /// <param name="itemSelectedCallback">列表项选中回调</param>
        /// <param name="folderPath">所属的文件夹路径</param>
        /// <param name="allowedInstanceIDs"></param>
        public static void ShowObjectPicker<T>(Object obj, string requiredTypeName,
            Action<Object> itemSelectedCallback,
            string folderPath = "Assets", List<int> allowedInstanceIDs = null) where T : Object
        {
            get.Show(obj, requiredTypeName, null, itemSelectedCallback, folderPath, allowedInstanceIDs);
        }

        public static void ShowObjectPicker<T>(Object obj, Action<Object> itemSelectedCallback,
            string folderPath, Object[] allowedInstanceObjects) where T : Object
        {
            List<int> allowedInstanceIDs = null;
            if (allowedInstanceObjects != null)
            {
                allowedInstanceIDs = new List<int>(allowedInstanceObjects.Length);
                foreach (var allowedInstanceObject in allowedInstanceObjects)
                    allowedInstanceIDs.Add(allowedInstanceObject.GetInstanceID());
            }

            ShowObjectPicker<T>(obj, itemSelectedCallback, folderPath, allowedInstanceIDs);
        }

        public static void ShowObjectPicker(SerializedProperty property,
            Action<Object> itemSelectedCallback, string folderPath = "Assets")
        {
            get.Show(null, typeof(Object), property, itemSelectedCallback, folderPath);
        }

        public void Show(Object obj, Type requiredType, SerializedProperty property,
            Action<Object> itemSelectedCallback, string folderPath, List<int> allowedInstanceIDs = null)
        {
            var requiredTypeName = string.Empty;
            if (property != null)
            {
                obj = property.objectReferenceValue;
                requiredTypeName = property.objectReferenceValue.GetType().Name;
            }
            else
            {
                requiredTypeName = requiredType.Name;
            }

            Show(obj, requiredTypeName, property, itemSelectedCallback, folderPath, allowedInstanceIDs);
        }

        public void Show(Object obj, string requiredType, SerializedProperty property,
            Action<Object> itemSelectedCallback, string folderPath, List<int> allowedInstanceIDs = null)
        {
            _folderPath = folderPath;
            if (!Directory.Exists(folderFullPath))
            {
                Debug.LogError(folderPath + " is not a Directory!");
                return;
            }

            _cacheProperty = property;
            _itemSelectedCallback = itemSelectedCallback;
            InitIfNeeded();


            InitBuiltinList(requiredType, allowedInstanceIDs);
            titleContent = new GUIContent("Select " + requiredType);
            _focusSearchFilter = true;
            _showNoneItem = true;
            _searchFilter = string.Empty;
            ListItemFrame(obj, true);
            ShowAuxWindow();
        }

        /// <summary>
        ///     初始化所指定的文件夹路径里的对象列表
        /// </summary>
        /// <param name="requiredTypeName"></param>
        /// <param name="allowedInstanceIDs"></param>
        private void InitBuiltinList(string requiredTypeName, List<int> allowedInstanceIDs)
        {
            var lenFolderPath = _folderPath.Length; // + 1;
            var builtinResList = new List<BuiltinRes>();

            if (allowedInstanceIDs == null)
            {
                var guids = AssetDatabase.FindAssets("t:" + requiredTypeName, new[] { _folderPath });
                foreach (var guid in guids)
                {
                    var builtinRes = new BuiltinRes();
                    var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    builtinRes.name = assetPath.Substring(lenFolderPath, assetPath.LastIndexOf('.') - lenFolderPath);
                    builtinRes.icon = AssetDatabase.GetCachedIcon(assetPath);
                    builtinRes.path = assetPath;
                    builtinResList.Add(builtinRes);
                }
            }
            else
            {
                foreach (var allowedInstanceID in allowedInstanceIDs)
                {
                    var assetPath = AssetDatabase.GetAssetPath(allowedInstanceID);
                    var obj = EditorUtility.InstanceIDToObject(allowedInstanceID);
                    var isSub = AssetDatabase.IsSubAsset(allowedInstanceID);
                    var assetName = isSub
                        ? obj.name
                        : assetPath.Substring(lenFolderPath, assetPath.LastIndexOf('.') - lenFolderPath);
                    var builtinRes = new BuiltinRes();
                    builtinRes.name = assetName;
                    builtinRes.icon =
                        isSub ? AssetPreview.GetMiniThumbnail(obj) : AssetDatabase.GetCachedIcon(assetPath);
                    builtinRes.path = assetPath;
                    builtinRes.id = allowedInstanceID;
                    builtinResList.Add(builtinRes);
                }
            }

            _currentBuiltinResources = _activeBuiltinList = builtinResList.ToArray();
        }

        private void InitIfNeeded()
        {
            if (_styles == null) _styles = new Styles();

            if (_noneBuiltinRes == null)
            {
                _noneBuiltinRes = new BuiltinRes();
                _noneBuiltinRes.name = "None";
            }

            _topSize = position.height - _previewSize;
        }

        private void Cancel()
        {
            Close();
            GUI.changed = true;
            GUIUtility.ExitGUI();
        }

        private void OnObjectListGUI()
        {
            InitIfNeeded();
            ResizeBottomPartOfWindow();
            HandleKeyboard();
            GUI.BeginGroup(new Rect(0f, 0f, position.width, position.height), GUIContent.none);
            SearchArea();
            GridListArea();
            PreviewArea();
            GUI.EndGroup();

            GUI.Label(new Rect(position.width * 0.5f - 16f,
                    position.height - _previewSize + 2f, 32f,
                    _styles.bottomResize.fixedHeight),
                GUIContent.none, _styles.bottomResize);
        }

        private void SearchArea()
        {
            GUI.Label(new Rect(0f, 0f, position.width, _toolbarHeight), GUIContent.none, _styles.toolbarBack);
            var flag = Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape;
            GUI.SetNextControlName("SearchFilter");
            var text = SearchField(new Rect(5f, 5f, position.width - 10f, 15f), _searchFilter);
            if (flag && Event.current.type == EventType.Used)
            {
                if (_searchFilter == string.Empty) Cancel();

                _focusSearchFilter = true;
            }

            if (text != _searchFilter || _focusSearchFilter)
            {
                _searchFilter = text;
                FilterSettingsChanged();
                Repaint();
            }

            if (_focusSearchFilter)
            {
                EditorGUI.FocusTextInControl("SearchFilter");
                _focusSearchFilter = false;
            }

            GUILayout.BeginArea(new Rect(0f, 26f, position.width, _toolbarHeight - 26f));
            GUILayout.BeginHorizontal();
            GUILayout.Toggle(true, _folderPath, _styles.tab);
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void GridListArea()
        {
            var totalRect = listPosition;
            var itemHeight = itemCount * 16f;
            var viewRect = new Rect(0f, 0f, 1f, itemHeight);
            GUI.Label(totalRect, GUIContent.none, _styles.iconAreaBg);
            _scrollPosition = GUI.BeginScrollView(totalRect, _scrollPosition, viewRect);

            var num = FirstVisibleRow(0f, _scrollPosition);
            if (num >= 0 && num < itemCount)
            {
                var num3 = num;
                var num4 = Math.Min(itemCount, 2147483647);
                var num5 = 16f;
                var num6 = (int)Math.Ceiling(position.height / num5);
                num4 = Math.Min(num4, num3 + num6 * 1 + 1);
                DrawListInternal(num3, num4);
            }

            GUI.EndScrollView();

            if (_lastSelectedObject && !_lastSelectedObjectIcon &&
                AssetPreview.IsLoadingAssetPreview(_lastSelectedObject.GetInstanceID()))
            {
                _lastSelectedObjectIcon = AssetPreview.GetAssetPreview(_lastSelectedObject);
                Repaint();
            }
        }

        private void DrawListInternal(int beginIndex, int endIndex)
        {
            var num = beginIndex;
            var num2 = 0;
            if (_showNoneItem)
            {
                if (beginIndex < 1)
                {
                    DrawListItemInternal(ListItemCalcRect(num), _noneBuiltinRes, num);
                    num++;
                }

                num2++;
            }

            if (_activeBuiltinList.Length > 0)
            {
                var num4 = beginIndex - num2;
                num4 = Math.Max(num4, 0);
                var num5 = num4;
                while (num5 < _activeBuiltinList.Length && num <= endIndex)
                {
                    DrawListItemInternal(ListItemCalcRect(num), _activeBuiltinList[num5], num);
                    num++;
                    num5++;
                }
            }
        }

        private void DrawListItemInternal(Rect rect4, BuiltinRes builtinResource, int itemIdx)
        {
            var current = Event.current;
            var num5 = 18f;
            var rect5 = new Rect(num5, rect4.y, rect4.width - num5, rect4.height);
            var selected = false;
            var focus = true;

            if (current.type == EventType.MouseDown)
            {
                if (current.button == 0 && rect4.Contains(current.mousePosition))
                {
                    if (current.clickCount == 1)
                    {
                        SetSelectedAssetByIdx(itemIdx);
                        current.Use();
                    }
                    else if (current.clickCount == 2)
                    {
                        current.Use();
                        Close();
                        GUIUtility.ExitGUI();
                    }
                }
            }
            else if (current.type == EventType.Repaint)
            {
                if (itemIdx == _lastSelectedIdx)
                    _styles.resultsLabel.Draw(rect4, GUIContent.none, false, false, true, focus);

                _styles.resultsLabel.Draw(rect5, builtinResource.name, false, false, selected, focus);
                var rect6 = rect5;
                rect6.width = 16f;
                rect6.x = 16f;
                if (builtinResource.icon != null) GUI.DrawTexture(rect6, builtinResource.icon);
            }
        }

        /// <summary>
        ///     每个项的矩形区域
        /// </summary>
        /// <param name="itemIdx"></param>
        /// <returns></returns>
        private Rect ListItemCalcRect(int itemIdx)
        {
            return new Rect(0f, itemIdx * 16f, listPosition.width, 16f);
        }

        private void PreviewArea()
        {
            GUI.Box(new Rect(0f, _topSize, position.width, _previewSize), string.Empty, _styles.previewBackground);

            if (_editorWrapperCache == null) _editorWrapperCache = new EditorWrapperCache(EditorFeatures.PreviewGUI);

            var currentObject = _lastSelectedObject;
            EditorWrapper editorWrapper = null;

            if (_previewSize < 75f)
            {
                string text;
                if (currentObject != null)
                {
                    editorWrapper = _editorWrapperCache[currentObject];
                    var str = ObjectNames.NicifyVariableName(currentObject.GetType().Name);
                    if (editorWrapper != null)
                        text = editorWrapper.name + " (" + str + ")";
                    else
                        text = currentObject.name + " (" + str + ")";

                    text = text + "      " + AssetDatabase.GetAssetPath(currentObject);
                }
                else
                {
                    text = "None";
                }

                LinePreview(text, currentObject, editorWrapper);
            }
            else
            {
                string text3;
                if (currentObject != null)
                {
                    editorWrapper = _editorWrapperCache[currentObject];
                    var text2 = ObjectNames.NicifyVariableName(currentObject.GetType().Name);
                    if (editorWrapper != null)
                    {
                        text3 = editorWrapper.GetInfoString().Replace("\n", "  ");
                        if (text3 != string.Empty)
                            text3 = string.Concat(editorWrapper.name, "\n", text2, "\n", text3);
                        else
                            text3 = editorWrapper.name + "\n" + text2;
                    }
                    else
                    {
                        text3 = currentObject.name + "\n" + text2;
                    }

                    text3 = text3 + "\n" + AssetDatabase.GetAssetPath(currentObject);
                }
                else
                {
                    text3 = "None";
                }

                WidePreview(_previewSize, text3, currentObject, editorWrapper);
                _editorWrapperCache.CleanupUntouchedEditors();
            }
        }

        private void LinePreview(string s, Object o, EditorWrapper p)
        {
            if (_lastSelectedObjectIcon != null)
                GUI.DrawTexture(new Rect(2f, (int)(_topSize + 2f), 16f, 16f), _lastSelectedObjectIcon,
                    ScaleMode.StretchToFill);

            var pos = new Rect(20f, _topSize + 1f, position.width - 22f, 18f);
            if (EditorGUIUtility.isProSkin)
                EditorGUI.DropShadowLabel(pos, s, _styles.smallStatus);
            else
                GUI.Label(pos, s, _styles.smallStatus);
        }

        private void WidePreview(float actualSize, string s, Object o, EditorWrapper p)
        {
            var num = 5f;
            var pos = new Rect(num, _topSize + num, actualSize - num * 2f, actualSize - num * 2f);
            var position2 = new Rect(_previewSize + 3f, _topSize + (_previewSize - 75f) * 0.5f,
                position.width - _previewSize - 3f - num, 75f);
            if (p != null && p.HasPreviewGUI())
            {
                p.OnInteractivePreviewGUI(pos, _styles.previewTextureBackground);

                var rect = new Rect(_previewSize + 3f, position.height - 22f, position.width, 16f);
                GUI.BeginGroup(rect);
                EditorGUILayout.BeginHorizontal(GUIStyle.none, GUILayout.Height(17f));
                p.OnPreviewSettings();
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                GUI.EndGroup();
            }
            else
            {
                if (o != null) DrawObjectIcon(pos, _lastSelectedObjectIcon);
            }

            if (EditorGUIUtility.isProSkin)
                EditorGUI.DropShadowLabel(position2, s, _styles.smallStatus);
            else
                GUI.Label(position2, s, _styles.smallStatus);
        }

        private void ResizeBottomPartOfWindow()
        {
            GUI.changed = false;
            var x = 5f + _previewSize - 5f * 2f;
            var dragRect = new Rect(x, 0, position.width - x, 0f);
            _previewSize = _previewResizer.ResizeHandle(position, 65f, 270f, 20f, dragRect) + 20f;
            _topSize = position.height - _previewSize;
        }

        private void HandleKeyboard()
        {
            if (!GUI.enabled || Event.current.type != EventType.KeyDown) return;

            switch (Event.current.keyCode)
            {
                case KeyCode.UpArrow:
                    if (_lastSelectedIdx > 0)
                    {
                        _lastSelectedIdx--;
                        SetSelectedAssetByIdx(_lastSelectedIdx);
                    }

                    break;
                case KeyCode.DownArrow:
                    if (_lastSelectedIdx < itemCount - 1)
                    {
                        _lastSelectedIdx++;
                        SetSelectedAssetByIdx(_lastSelectedIdx);
                    }

                    break;
            }
        }

        /// <summary>
        ///     设置选中的索引
        /// </summary>
        /// <param name="selectedIdx"></param>
        /// <param name="callback"></param>
        private void SetSelectedAssetByIdx(int selectedIdx, bool callback = true)
        {
            _lastSelectedIdx = selectedIdx;

            if (_showNoneItem && selectedIdx == 0)
            {
                _lastSelectedObject = null;
                _lastSelectedObjectIcon = null;
            }
            else
            {
                if (_showNoneItem) selectedIdx--;

                if (_activeBuiltinList[selectedIdx].id > 0)
                    _lastSelectedObject = EditorUtility.InstanceIDToObject(_activeBuiltinList[selectedIdx].id);
                else
                    _lastSelectedObject =
                        AssetDatabase.LoadAssetAtPath<Object>(_activeBuiltinList[selectedIdx].path);

                _lastSelectedObjectIcon = AssetPreview.GetAssetPreview(_lastSelectedObject);

                if (_editorWrapperCache != null && _lastSelectedObject)
                {
                    _editorWrapperCache.CleanupUntouchedEditors();
                    var editorWrapper = _editorWrapperCache[_lastSelectedObject];
                    if (editorWrapper != null)
                    {
                    }
                }
            }

            var r = ListItemCalcRect(selectedIdx);
            ScrollToPosition(AdjustRectForFraming(r));
            Repaint();

            if (callback && _itemSelectedCallback != null)
            {
                _itemSelectedCallback(_lastSelectedObject);
            }
            else if (callback && _cacheProperty != null)
            {
                _cacheProperty.objectReferenceValue = _lastSelectedObject;
                _cacheProperty.serializedObject.ApplyModifiedProperties();
            }
        }

        /// <summary>
        ///     绘制对象的预览图标
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="icon"></param>
        private void DrawObjectIcon(Rect rect, Texture icon)
        {
            if (icon == null) return;

            var num = Mathf.Min((int)rect.width, (int)rect.height);
            if (num >= icon.width * 2) num = icon.width * 2;

            var filterMode = icon.filterMode;
            icon.filterMode = FilterMode.Point;
            GUI.DrawTexture(
                new Rect(rect.x + ((int)rect.width - num) / 2, rect.y + ((int)rect.height - num) / 2, num, num), icon,
                ScaleMode.ScaleToFit);
            icon.filterMode = filterMode;
        }

        /// <summary>
        ///     找到第一个可见的列表项
        /// </summary>
        /// <param name="yOffset"></param>
        /// <param name="scrollPos"></param>
        /// <returns></returns>
        private int FirstVisibleRow(float yOffset, Vector2 scrollPos)
        {
            var num = scrollPos.y - yOffset;
            var result = 0;
            if (num > 0f)
            {
                var num2 = 16f; // 列表项高度
                result = (int)Mathf.Max(0f, Mathf.Floor(num / num2));
            }

            return result;
        }

        /// <summary>
        ///     搜索字符串变化
        /// </summary>
        private void FilterSettingsChanged()
        {
            var array = _currentBuiltinResources;
            if (array != null && array.Length > 0 && !string.IsNullOrEmpty(_searchFilter))
            {
                var list3 = new List<BuiltinRes>();
                var value = _searchFilter.ToLower();
                var array2 = array;
                for (var j = 0; j < array2.Length; j++)
                {
                    var builtinResource = array2[j];
                    if (builtinResource.name.ToLower().Contains(value)) list3.Add(builtinResource);
                }

                array = list3.ToArray();
            }

            _activeBuiltinList = array;

            if (_lastSelectedObject) ListItemFrame(_lastSelectedObject, true);
        }

        /// <summary>
        ///     列表项滚动到指定矩形区域
        /// </summary>
        /// <param name="r"></param>
        private void ScrollToPosition(Rect r)
        {
            var y = r.y;
            var yMax = r.yMax;
            var height = listPosition.height;
            if (yMax > height + _scrollPosition.y) _scrollPosition.y = yMax - height;

            if (y < _scrollPosition.y) _scrollPosition.y = y;

            _scrollPosition.y = Mathf.Max(_scrollPosition.y, 0f);
        }

        /// <summary>
        ///     指定矩形定位后的区域
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        private static Rect AdjustRectForFraming(Rect r)
        {
            r.height += _sharedObjectSelector._styles.resultsGridLabel.fixedHeight * 2f;
            r.y -= _sharedObjectSelector._styles.resultsGridLabel.fixedHeight;
            return r;
        }

        /// <summary>
        ///     列表项定位
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="frame"></param>
        /// <returns></returns>
        private bool ListItemFrame(string assetPath, bool frame)
        {
            var num = ListItemIndexOf(assetPath);
            if (num != -1)
            {
                if (frame) SetSelectedAssetByIdx(num, false);

                return true;
            }

            return false;
        }

        /// <summary>
        ///     列表项定位
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="frame"></param>
        /// <returns></returns>
        private bool ListItemFrame(Object obj, bool frame)
        {
            if (obj == null || !AssetDatabase.Contains(obj)) return false;

            return ListItemFrame(AssetDatabase.GetAssetPath(obj), frame);
        }

        /// <summary>
        ///     根据路径查找所在的索引
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        private int ListItemIndexOf(string assetPath)
        {
            var num = 0;
            if (_showNoneItem)
            {
                if (string.IsNullOrEmpty(assetPath)) return 0;

                num++;
            }
            else
            {
                if (string.IsNullOrEmpty(assetPath)) return -1;
            }

            var activeBuiltinList = _activeBuiltinList;
            for (var j = 0; j < activeBuiltinList.Length; j++)
            {
                var builtinResource = activeBuiltinList[j];
                if (assetPath == builtinResource.path) return num;

                num++;
            }

            return -1;
        }

        private class Styles
        {
            public GUIStyle background = "ObjectPickerBackground";
            public readonly GUIStyle bottomResize = "WindowBottomResize";
            public GUIStyle dragHandle = "RL DragHandle";
            public readonly GUIStyle iconAreaBg = "ProjectBrowserIconAreaBg";
            public GUIStyle largeStatus = "ObjectPickerLargeStatus";

            public GUIStyle preButton = "preButton";
            public GUIStyle preToolbar = "preToolbar";
            public readonly GUIStyle previewBackground = "PopupCurveSwatchBackground";
            public GUIStyle previewBg = "ProjectBrowserPreviewBg";
            public readonly GUIStyle previewTextureBackground = "ObjectPickerPreviewBackground";

            public readonly GUIStyle resultsGridLabel = "ProjectBrowserGridLabel";
            public readonly GUIStyle resultsLabel = "PR Label";
            public readonly GUIStyle smallStatus = "ObjectPickerSmallStatus";
            public readonly GUIStyle tab = "ObjectPickerTab";
            public readonly GUIStyle toolbarBack = "ObjectPickerToolbar";
        }

        private class BuiltinRes
        {
            public Texture icon;
            public int id;
            public string name;
            public string path;
        }
    }


    internal class EditorWrapper : IDisposable
    {
        public delegate void VoidDelegate(SceneView sceneView);

        private UnityEditor.Editor editor;

        private EditorWrapper()
        {
        }

        public string name => editor.target.name;

        public void Dispose()
        {
            if (editor != null)
            {
                Object.DestroyImmediate(editor);
                editor = null;
            }

            GC.SuppressFinalize(this);
        }

        public void OnEnable()
        {
            var method = editor.GetType().GetMethod("OnEnable",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (method != null) method.Invoke(editor, null);
        }

        public void OnDisable()
        {
            var method = editor.GetType().GetMethod("OnDisable",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (method != null) method.Invoke(editor, null);
        }

        public bool HasPreviewGUI()
        {
            return editor.HasPreviewGUI();
        }

        public void OnPreviewSettings()
        {
            editor.OnPreviewSettings();
        }

        public void OnPreviewGUI(Rect position, GUIStyle background)
        {
            editor.OnPreviewGUI(position, background);
        }

        public void OnInteractivePreviewGUI(Rect r, GUIStyle background)
        {
            if (editor != null) editor.OnInteractivePreviewGUI(r, background);
        }

        public string GetInfoString()
        {
            return editor.GetInfoString();
        }

        public static EditorWrapper Make(Object obj, EditorFeatures requirements)
        {
            var editorWrapper = new EditorWrapper();
            if (editorWrapper.Init(obj, requirements)) return editorWrapper;

            editorWrapper.Dispose();
            return null;
        }

        private bool Init(Object obj, EditorFeatures requirements)
        {
            editor = UnityEditor.Editor.CreateEditor(obj);
            if (editor == null) return false;

            if ((requirements & EditorFeatures.PreviewGUI) > EditorFeatures.None && !editor.HasPreviewGUI())
                return false;

            return true;
        }
    }


    internal enum EditorFeatures
    {
        None = 0,
        PreviewGUI = 1,
        OnSceneDrag = 4
    }

    internal class EditorWrapperCache : IDisposable
    {
        private readonly Dictionary<Object, EditorWrapper> _editorCache;
        private readonly EditorFeatures _requirements;
        private readonly Dictionary<Object, bool> _usedEditors;

        public EditorWrapperCache(EditorFeatures requirements)
        {
            _requirements = requirements;
            _editorCache = new Dictionary<Object, EditorWrapper>();
            _usedEditors = new Dictionary<Object, bool>();
        }

        public EditorWrapper this[Object o]
        {
            get
            {
                _usedEditors[o] = true;
                if (_editorCache.TryGetValue(o, out var item)) return item;

                var editorWrapper = EditorWrapper.Make(o, _requirements);
                var editorWrapper2 = editorWrapper;
                _editorCache[o] = editorWrapper2;
                return editorWrapper2;
            }
        }

        public void Dispose()
        {
            CleanupAllEditors();
            GC.SuppressFinalize(this);
        }

        public void CleanupUntouchedEditors()
        {
            var list = new List<Object>();
            foreach (var current in _editorCache.Keys)
                if (!_usedEditors.ContainsKey(current))
                    list.Add(current);

            if (_editorCache != null)
                foreach (var current2 in list)
                {
                    var editorWrapper = _editorCache[current2];
                    _editorCache.Remove(current2);
                    if (editorWrapper != null) editorWrapper.Dispose();
                }

            _usedEditors.Clear();
        }

        public void CleanupAllEditors()
        {
            _usedEditors.Clear();
            CleanupUntouchedEditors();
        }
    }
}