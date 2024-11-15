using System.Linq;
using MMDarkness.Editor.Draws;
using UnityEditor;
using UnityEngine;

namespace MMDarkness.Editor
{
    public class GroupAndTrackListGUI : ICustomized
    {
        private static readonly Color s_listSelectionColor = new(0.5f, 0.5f, 1, 0.3f);
        private static readonly Color s_groupColor = new(0f, 0f, 0f, 0.25f);
        private TrackAsset m_copyTrackAsset;
        private bool m_isResizingLeftMargin;

        private Rect m_leftRect;
        private GroupAsset m_pickedGroupAsset;
        private TrackAsset m_pickedTrackAsset;

        private TimelineGraphPreviewProcessor Player => App.Player;
        public TimelineGraphAsset TimelineGraphAsset => App.GraphAsset;

        /// <summary>
        ///     轨道/轨道组 列表
        /// </summary>
        public void OnGUI()
        {
            m_leftRect = G.LeftRect;
            var e = Event.current;

            var scaleRect = new Rect(m_leftRect.xMax - 4, m_leftRect.yMin, 4, m_leftRect.height);
            ActionEditorWindow.current.AddCursorRect(scaleRect, MouseCursor.ResizeHorizontal);
            if (e.type == EventType.MouseDown && e.button == 0 && scaleRect.Contains(e.mousePosition))
            {
                m_isResizingLeftMargin = true;
                e.Use();
            }

            if (m_isResizingLeftMargin) Styles.LeftMargin = e.mousePosition.x + 2;

            if (e.rawType == EventType.MouseUp) m_isResizingLeftMargin = false;

            GUI.enabled = Player.CurrentTime <= 0;

            var nextYPos = Styles.FirstGroupTopMargin;
            var wasEnabled = GUI.enabled;
            GUI.enabled = true;
            var collapseAllRect = Rect.MinMaxRect(m_leftRect.x + 5, m_leftRect.y + 4, 20, m_leftRect.y + 20 - 1);
            var searchRect = Rect.MinMaxRect(m_leftRect.x + 20, m_leftRect.y + 4, m_leftRect.xMax - 18,
                m_leftRect.y + 20 - 1);
            var searchCancelRect = Rect.MinMaxRect(searchRect.xMax, searchRect.y, m_leftRect.xMax - 4, searchRect.yMax);
            var anyExpanded = TimelineGraphAsset.groupAssets.Any(g => !g.IsCollapsed);
            ActionEditorWindow.current.AddCursorRect(collapseAllRect, MouseCursor.Link);
            GUI.color = Color.white.WithAlpha(0.5f);
            if (GUI.Button(collapseAllRect, anyExpanded ? "▼" : "►", "label"))
                foreach (var group in TimelineGraphAsset.groupAssets)
                    group.IsCollapsed = anyExpanded;

            GUI.color = Color.white;
            G.SearchString = EditorGUI.TextField(searchRect, G.SearchString, (GUIStyle)"ToolbarSearchTextField");
            if (GUI.Button(searchCancelRect, string.Empty, "ToolbarSearchCancelButton"))
            {
                G.SearchString = string.Empty;
                GUIUtility.keyboardControl = 0;
            }

            GUI.enabled = wasEnabled;

            GUI.BeginGroup(m_leftRect);
            ShowListGroups(e, ref nextYPos);
            GUI.EndGroup();

            G.TotalHeight = nextYPos;

            var addButtonY = G.TotalHeight + Styles.TopMargin + Styles.ToolbarHeight + 20;
            var addRect = Rect.MinMaxRect(m_leftRect.xMin + 10, addButtonY, m_leftRect.xMax - 10, addButtonY + 20);
            GUI.color = Color.white.WithAlpha(0.5f);
            if (GUI.Button(addRect, Lan.GroupAdd))
            {
                var newGroup = TimelineGraphAsset.AddGroup<GroupAsset>();
                DirectorUtility.SelectedObject = newGroup;
            }

            //clear picks
            if (e.rawType == EventType.MouseUp)
            {
                m_pickedGroupAsset = null;
                m_pickedTrackAsset = null;
            }

            GUI.enabled = true;
            GUI.color = Color.white;
        }


        private void ShowListGroups(Event e, ref float nextYPos)
        {
            for (var g = 0; g < TimelineGraphAsset.groupAssets.Count; g++)
            {
                var group = TimelineGraphAsset.groupAssets[g];

                if (G.IsFilteredOutBySearch(group))
                {
                    group.IsCollapsed = true;
                    continue;
                }

                var groupRect = new Rect(4, nextYPos, m_leftRect.width - Styles.GroupRightMargin - 4,
                    Styles.GroupHeight - 3);
                ActionEditorWindow.current?.AddCursorRect(groupRect,
                    m_pickedGroupAsset == null ? MouseCursor.Link : MouseCursor.MoveArrow);
                nextYPos += Styles.GroupHeight;

                var groupSelected = ReferenceEquals(group, DirectorUtility.SelectedObject) ||
                                    group == m_pickedGroupAsset;
                GUI.color = groupSelected ? s_listSelectionColor : s_groupColor;
                GUI.Box(groupRect, string.Empty, Styles.HeaderBoxStyle);
                GUI.color = Color.white;


                var plusClicked = false;
                GUI.color = EditorGUIUtility.isProSkin ? Color.white.WithAlpha(0.5f) : new Color(0.2f, 0.2f, 0.2f);
                var plusRect = new Rect(groupRect.xMax - 20, groupRect.y + 6, 16, 16);

                if (GUI.Button(plusRect, Styles.MenuIcon, GUIStyle.none)) plusClicked = true;

                if (!group.IsActive)
                {
                    var disableIconRect = new Rect(plusRect.xMin - 20, groupRect.y + 6, 16, 16);
                    if (GUI.Button(disableIconRect, Styles.HiddenIcon, GUIStyle.none)) group.IsActive = true;
                }

                if (group.IsLocked)
                {
                    var lockIconRect = new Rect(plusRect.xMin - (group.IsActive ? 20 : 36), groupRect.y + 6, 16, 16);
                    if (GUI.Button(lockIconRect, Styles.LockIcon, GUIStyle.none)) group.IsLocked = false;
                }

                GUI.color = EditorGUIUtility.isProSkin ? Color.yellow : Color.white;
                GUI.color = group.IsActive ? GUI.color : Color.grey;
                var foldRect = new Rect(groupRect.x + 2, groupRect.y + 1, 20, groupRect.height);
                group.IsCollapsed = !EditorGUI.Foldout(foldRect, !group.IsCollapsed, $"<b>{group.Name}</b>");
                GUI.color = Color.white;

                //右键菜单
                if ((e.type == EventType.ContextClick && groupRect.Contains(e.mousePosition)) || plusClicked)
                {
                    var menu = new GenericMenu();
                    foreach (var metaInfo in EditorTools.GetTypeMetaDerivedFrom(typeof(Track)))
                    {
                        var info = metaInfo;
                        if (info.AttachableTypes == null || !info.AttachableTypes.Contains(group.groupModel.GetType()))
                            continue;

                        var canAdd = !info.IsUnique || group.Tracks.Find(track => track.GetType() == info.Type) == null;
                        if (group.IsLocked) canAdd = false;

                        var finalPath = string.IsNullOrEmpty(info.Category)
                            ? info.Name
                            : info.Category + "/" + info.Name;
                        if (canAdd)
                            menu.AddItem(new GUIContent($"{Lan.MenuAddTrack}/" + finalPath), false,
                                () => { group.AddTrack(info.Type); });
                        else
                            menu.AddDisabledItem(new GUIContent($"{Lan.MenuAddTrack}/" + finalPath));
                    }

                    menu.AddSeparator("");
                    if (group.CanAddTrack(m_copyTrackAsset))
                        menu.AddItem(new GUIContent(Lan.MenuPasteTrack), false, () =>
                        {
                            var t = group.PasteTrack(m_copyTrackAsset);
                            DirectorUtility.SelectedObject = t;
                            ActionEditorWindow.current.InitClipWrappers();
                        });
                    else
                        menu.AddDisabledItem(new GUIContent(Lan.MenuPasteTrack));

                    menu.AddItem(new GUIContent(Lan.GroupDisable), !group.IsActive,
                        () => { group.IsActive = !group.IsActive; });
                    menu.AddItem(new GUIContent(Lan.GroupLocked), group.IsLocked,
                        () => { group.IsLocked = !group.IsLocked; });

                    menu.AddSeparator("");

                    menu.AddItem(new GUIContent(Lan.GroupReplica), false, () =>
                    {
                        TimelineGraphAsset.PasteGroup(group);
                        ActionEditorWindow.current.InitClipWrappers();
                    });

                    menu.AddSeparator("");
                    if (group.IsLocked)
                        menu.AddDisabledItem(new GUIContent(Lan.GroupDelete));
                    else
                        menu.AddItem(new GUIContent(Lan.GroupDelete), false, () =>
                        {
                            if (EditorUtility.DisplayDialog(Lan.GroupDelete, Lan.GroupDeleteTips, Lan.TipsConfirm,
                                    Lan.TipsCancel))
                            {
                                TimelineGraphAsset.DeleteGroup(group);
                                ActionEditorWindow.current.InitClipWrappers();
                            }
                        });

                    menu.ShowAsContext();
                    e.Use();
                }


                if (e.type == EventType.MouseDown && e.button == 0 && groupRect.Contains(e.mousePosition))
                {
                    DirectorUtility.SelectedObject = group;

                    m_pickedGroupAsset = group;

                    e.Use();
                }

                if (m_pickedGroupAsset != null && m_pickedGroupAsset != group) // && !(group is DirectorGroup))
                {
                    if (groupRect.Contains(e.mousePosition))
                    {
                        var markRect = new Rect(groupRect.x,
                            TimelineGraphAsset.groupAssets.IndexOf(m_pickedGroupAsset) < g
                                ? groupRect.yMax - 2
                                : groupRect.y, groupRect.width, 2);
                        GUI.color = Color.grey;
                        GUI.DrawTexture(markRect, Styles.WhiteTexture);
                        GUI.color = Color.white;
                    }

                    if (e.rawType == EventType.MouseUp && e.button == 0 && groupRect.Contains(e.mousePosition))
                    {
                        TimelineGraphAsset.groupAssets.Remove(m_pickedGroupAsset);
                        TimelineGraphAsset.groupAssets.Insert(g, m_pickedGroupAsset);
                        m_pickedGroupAsset = null;
                        e.Use();
                    }
                }

                if (!group.IsCollapsed)
                {
                    ShowListTracks(e, group, ref nextYPos);
                    GUI.color = groupSelected ? s_listSelectionColor : s_groupColor;
                    var verticalRect = Rect.MinMaxRect(groupRect.x, groupRect.yMax, groupRect.x + 3, nextYPos - 2);
                    GUI.DrawTexture(verticalRect, Styles.WhiteTexture);
                    GUI.color = Color.white;
                }
            }
        }

        /// <summary>
        ///     显示轨道列表
        /// </summary>
        private void ShowListTracks(Event e, GroupAsset groupAsset, ref float nextYPos)
        {
            for (var t = 0; t < groupAsset.Tracks.Count; t++)
            {
                var track = groupAsset.Tracks[t];
                var yPos = nextYPos;

                var trackRect = new Rect(10, yPos, m_leftRect.width - Styles.TrackRightMargin - 10, track.ShowHeight);
                nextYPos += track.ShowHeight + Styles.TrackMargins;

                GUI.color = ColorUtility.Grey(EditorGUIUtility.isProSkin ? track.IsActive ? 0.25f : 0.2f :
                    track.IsActive ? 0.9f : 0.8f);
                GUI.DrawTexture(trackRect, Styles.WhiteTexture);
                GUI.color = Color.white.WithAlpha(0.25f);
                GUI.Box(trackRect, string.Empty, "flow node 0");
                if (ReferenceEquals(track, DirectorUtility.SelectedObject) || track == m_pickedTrackAsset)
                {
                    GUI.color = s_listSelectionColor;
                    GUI.DrawTexture(trackRect, Styles.WhiteTexture);
                }

                if (track.IsActive && track.Color != Color.white && track.Color.a > 0.2f)
                {
                    GUI.color = track.Color;
                    var colorRect = new Rect(trackRect.xMax + 1, trackRect.yMin, 2, track.ShowHeight);
                    GUI.DrawTexture(colorRect, Styles.WhiteTexture);
                }

                GUI.color = Color.white;

                GUI.BeginGroup(trackRect);
                TrackDraw.Draw(track, trackRect);
                GUI.EndGroup();

                ActionEditorWindow.current.AddCursorRect(trackRect,
                    m_pickedTrackAsset == null ? MouseCursor.Link : MouseCursor.MoveArrow);

                //右键菜单
                if (e.type == EventType.ContextClick && trackRect.Contains(e.mousePosition))
                {
                    var menu = new GenericMenu();
                    menu.AddItem(new GUIContent(Lan.TrackDisable), !track.IsActive,
                        () => { track.IsActive = !track.IsActive; });
                    menu.AddItem(new GUIContent(Lan.TrackLocked), track.IsLocked,
                        () => { track.IsLocked = !track.IsLocked; });

                    menu.AddSeparator("");

                    menu.AddItem(new GUIContent(Lan.TrackCopy), false, () => { m_copyTrackAsset = track; });

                    if (track.GetType().RTGetAttribute<UniqueAttribute>(true) == null)
                        menu.AddItem(new GUIContent(Lan.TrackReplica), false, () =>
                        {
                            var t1 = groupAsset.PasteTrack(track);
                            ActionEditorWindow.current.InitClipWrappers();
                            DirectorUtility.SelectedObject = t1;
                        });
                    else
                        menu.AddDisabledItem(new GUIContent(Lan.TrackReplica));

                    menu.AddSeparator("/");
                    if (track.IsLocked)
                        menu.AddDisabledItem(new GUIContent(Lan.TrackDelete));
                    else
                        menu.AddItem(new GUIContent(Lan.TrackDelete), false, () =>
                        {
                            if (EditorUtility.DisplayDialog(Lan.TrackDelete, Lan.TrackDeleteTips, Lan.TipsConfirm,
                                    Lan.TipsCancel))
                            {
                                groupAsset.DeleteTrack(track);
                                ActionEditorWindow.current.InitClipWrappers();
                            }
                        });

                    menu.ShowAsContext();
                    e.Use();
                }

                //选中
                if (e.type == EventType.MouseDown && e.button == 0 && trackRect.Contains(e.mousePosition))
                {
                    DirectorUtility.SelectedObject = track;
                    m_pickedTrackAsset = track;
                    e.Use();
                }

                if (m_pickedTrackAsset != null && m_pickedTrackAsset != track &&
                    ReferenceEquals(m_pickedTrackAsset.Parent, groupAsset))
                {
                    if (trackRect.Contains(e.mousePosition))
                    {
                        var markRect = new Rect(trackRect.x,
                            groupAsset.Tracks.IndexOf(m_pickedTrackAsset) < t ? trackRect.yMax - 2 : trackRect.y,
                            trackRect.width, 2);
                        GUI.color = Color.grey;
                        GUI.DrawTexture(markRect, Styles.WhiteTexture);
                        GUI.color = Color.white;
                    }

                    if (e.rawType == EventType.MouseUp && e.button == 0 && trackRect.Contains(e.mousePosition))
                    {
                        groupAsset.Tracks.Remove(m_pickedTrackAsset);
                        groupAsset.Tracks.Insert(t, m_pickedTrackAsset);
                        m_pickedTrackAsset = null;
                        e.Use();
                    }
                }
            }
        }
    }
}