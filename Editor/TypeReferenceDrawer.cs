using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace TypeInspector.Editor
{
    [CustomPropertyDrawer(typeof(TypeReference))]
    public class TypeReferenceDrawer : PropertyDrawer
    {
        private TypeFindPopup typeSelector;

        public static SerializedProperty FilterObjectProperty;
        public static Func<Type, bool> Filter; 
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (FilterObjectProperty == null)
            {
                FilterObjectProperty = property;
            }
            
            var fullNameSP = property.FindPropertyRelative("FullName");
            position.height = GetPropertyHeight(fullNameSP, new GUIContent(""));

            EditorGUI.BeginProperty(position, label, property);
            
            var prefixRect = EditorGUI.PrefixLabel(position, new GUIContent(FilterObjectProperty.displayName));
            var buttonRect = position;
            buttonRect.position -= new Vector2(buttonRect.position.x - prefixRect.width, buttonRect.position.y);
            buttonRect.width -= prefixRect.width;

            var filter = ReflectionHelpers.CreateFilter<Type, TypeFilterAttribute>(FilterObjectProperty);
            if (Filter != null)
            {
                filter = t => Filter(t) && filter(t);
            }
            
            if (GUI.Button(prefixRect, fullNameSP.stringValue, EditorStyles.popup))
            {
                PopupWindow.Show(prefixRect, new TypeFindPopup(type =>
                {
                    fullNameSP.stringValue = type.FullName + ", " + type.Assembly.GetName().Name;
                    property.serializedObject.ApplyModifiedProperties();
                }, filter));
            }
            
            EditorGUI.EndProperty();

            FilterObjectProperty = null;
        }
    }

    public class TypeFindPopup : PopupWindowContent
    {
        private string searchText = "";
        private string searchHistory = "";
        private SearchField searchField;
        private Action<Type> selectAction;
        private TypeFilter filter;

        private string currentNamespace;
        private bool isNamespaceSelected;
        private IEnumerable<string> namespaces;
        private IEnumerable<Type> types;
        private bool isDirty;
        private Vector2 scrollPosition;

        private static GUIStyle itemStyle = new GUIStyle("CN Box");
        private static GUIStyle searchBox = new GUIStyle("CN Box");
        private static GUIStyle toolBox = new GUIStyle("CN Box");
        
        static TypeFindPopup()
        {
            itemStyle.clipping = TextClipping.Clip;
          
            searchBox.padding.left = 7;
            searchBox.padding.right = 7;
            searchBox.padding.top = 7;
            searchBox.padding.bottom = 7;
            
            toolBox.padding.left = 7;
            toolBox.padding.right = 7;
            toolBox.padding.top = 4;
            toolBox.padding.bottom = 4;
            toolBox.normal.background = Texture2D.whiteTexture;
        }
        
        public TypeFindPopup(Action<Type> selectAction, Func<Type, bool> filter) : base()
        {
            this.selectAction = selectAction;
            this.filter = new TypeFilter(filter);
        }
        
        public override void OnOpen()
        {
            searchField = new SearchField();
        }

        public override void OnGUI(Rect rect)
        {
            DrawSearchBox();
            
            GUILayout.Space(1);

            DrawToolbar();
            
            GUILayout.Space(1);
            
            if (isNamespaceSelected)
            {
                DrawSelectClass();
            }
            else
            {
                DrawSelectNamespace();
            }
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(toolBox, GUILayout.Height(20));
            
            if (isNamespaceSelected)
            {
                DrawToolbarNamespace();
            }
            else
            {
                GUILayout.Button("Search namespace", EditorStyles.boldLabel);
            }
            
            EditorGUILayout.EndHorizontal();
        }

        private void DrawToolbarNamespace()
        {
            GUILayout.Button("<", EditorStyles.boldLabel, GUILayout.Width(14));
            var isClick = GUILayout.Button(currentNamespace, EditorStyles.boldLabel);
            if (isClick)
            {
                BackHistory();
            }
        }

        private void BackHistory()
        {
            isNamespaceSelected = false;
            searchText = searchHistory;
            isDirty = true;
        }

        private void DrawSelectNamespace()
        {
            if (isDirty || namespaces == null)
            {
                namespaces = filter.FilterNamespaces(searchText);
                isDirty = false;
            }

            EditorGUILayout.BeginHorizontal(itemStyle);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, false, true);
            foreach (var ns in namespaces)
            {
                if (DrawButton(ns, true))
                {
                    SelectNamespace(ns);
                }
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndHorizontal();
        }

        private void SelectNamespace(string ns)
        {
            isNamespaceSelected = true;
            currentNamespace = ns;
            searchHistory = searchText;
            searchText = "";
            isDirty = true;
        }

        private void DrawSelectClass()
        {
            if (isDirty || types == null)
            {
                types = filter.FilterTypes(currentNamespace, searchText);
                isDirty = false;
            }

            foreach (var type in types)
            {
                var isClick = DrawButton(type.Name);
                
                if (!isClick)
                {
                    continue;
                }
                
                selectAction(type);
                editorWindow.Close();
            }
        }

        private bool DrawButton(string text, bool needArrow = false)
        {
            EditorGUILayout.BeginHorizontal(itemStyle, GUILayout.Height(20));

            var isClick = GUILayout.Button(text, EditorStyles.label, GUILayout.ExpandWidth(false));
            if (needArrow)
            {
                EditorGUILayout.LabelField(">", GUILayout.Width(14));
            }
            
            EditorGUILayout.EndHorizontal();

            return isClick;
        }

        private void DrawSearchBox()
        {
            EditorGUILayout.BeginHorizontal(searchBox, GUILayout.Height(30));
            var currentSearchText = searchField.OnGUI(searchText);
            EditorGUILayout.EndHorizontal();

            isDirty |= currentSearchText != searchText;

            searchText = currentSearchText;
        }
    }

    public class TypeFilter
    {
        public readonly Type[] Types;
        public readonly string[] Namespaces;
        public readonly ReadOnlyDictionary<string, Type[]> NamespaceToTypes;
        
        public TypeFilter(Func<Type, bool> filter)
        {
            Types = AppDomain.CurrentDomain.GetAssemblies()
                    .OrderBy(o => o.FullName)
                    .SelectMany(o => o.GetTypes())
                    .Where(o => o.IsPublic)
                    .OrderBy(o => o.Name)
                    .Where(filter)
                .ToArray();

            var namespaceGrouping = Types.GroupBy(t => t.Namespace).ToList();
            
            Namespaces = namespaceGrouping
                .Select(n => string.IsNullOrEmpty(n.Key) ? "global" : n.Key)
                .OrderBy(n => n)
                .ToArray();
            
            NamespaceToTypes = new ReadOnlyDictionary<string, Type[]>(namespaceGrouping.ToDictionary(g => string.IsNullOrEmpty(g.Key) ? "global" : g.Key, g => g.ToArray()));
        }

        public IEnumerable<string> FilterNamespaces(string substring)
        {
            var result = new ConcurrentBag<string>();
            
            if (string.IsNullOrEmpty(substring))
            {
                return Namespaces;
            }
            
            Parallel.For(0, Namespaces.Length, (i, state) =>
            {
                if (Namespaces[i].Contains(substring))
                {
                    result.Add(Namespaces[i]);
                }
            });
            
            return result.OrderBy(ns => ns);
        }
        
        public IEnumerable<Type> FilterTypes(string ns, string substring)
        {
            var result = new ConcurrentBag<Type>();
            var types = NamespaceToTypes[ns];
            
            if (string.IsNullOrEmpty(substring))
            {
                return types;
            }
            
            Parallel.For(0, types.Length, (i, state) =>
            {
                if (types[i].Name.Contains(substring))
                {
                    result.Add(types[i]);
                }
            });

            return result.OrderBy(t => t.Name);
        }
    }
}