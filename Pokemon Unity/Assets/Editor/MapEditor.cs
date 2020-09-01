using UnityEngine;
using UnityEditor;
using PokemonUnity.Frontend.Overworld.Mapping;
using Leaflet.Frontend.Overworld;

[System.Serializable]
public class Tile
{
    public string name;
    public Material[] materials;
    public AudioClip footstepSound;
    public Color footstepGizmo;
}
class MapEditor : EditorWindow
{
    public bool noPresetTile = false;
    public enum PresetTiles {
        Blank,
        Dirt,
        Grass1,
        Grass2,
        Grass3,
        Carpet,
        Planks,
        Floor1,
        Floor2,
        Floor3,
        Wall1,
        Wall2,
        Industrial,
        Path1,
        Path2,
        Path3,
        Quartz,
        Rock1,
        Rock2,
        Tile,
        UmbraDirt,
        UmbraGrass1,
        UmbraGrass2,
        UmbraGrass3,
        UmbraGrass4,
        UmbraGrass5 //add new preset enums after this
    }
    public string[] castedPresets = new string[]
    {
        "none",
        "blank",
        "dirt",
        "grass1",
        "grass2",
        "grass3",
        "indoorcarpetcenter",
        "indoorplanks",
        "indoortile1",
        "indoortile2",
        "indoortile3",
        "indoorwall1",
        "indoorwall2",
        "industrial",
        "path1",
        "path2",
        "path3",
        "quartz",
        "rock",
        "rock2",
        "tile",
        "umbradirt",
        "umbragrass1",
        "umbragrass2",
        "umbragrass3",
        "umbragrass4",
        "umbragrass5" //add new preset material names after this
    };
    PresetTiles preset;
    int matInt = -1;
    bool firstRun = true;
    [SerializeField]
    GameObject gridPrefab;
    GameObject selectedMap;
    GameObject selectedTile;
    public Tile[] setupTiles;
    private SerializedProperty encountersProp;
    public Tile selTile;
    [MenuItem( "Pokémon Unity/Map Editor %#e" )]
    static void Init()
    {
        MapEditor window = (MapEditor)EditorWindow.GetWindow( typeof( MapEditor ), false, "Map Editor", true );
        window.Show();
    }
    void AddTileItem(GenericMenu menu, string menuPath, Tile tile)
    {
        menu.AddItem(new GUIContent(menuPath), selTile.Equals(tile), OnSelected, tile);
    }
    void OnSelected(object tile)
    {
        selTile = (Tile)tile;
    }
    void OnGUI()
    {
        SerializedObject serializedThis = new SerializedObject(this);
        selectedMap = Selection.activeGameObject;
        selectedTile = Selection.activeGameObject;
        EditorGUILayout.LabelField("Map Editor:", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedThis.FindProperty("setupTiles"),new GUIContent("Tile Setup"),true);
        if(GUILayout.Button("Toggle Grid"))
        {
            if(GameObject.Find("TempMapEditorGrid") != null)
                DestroyImmediate(GameObject.Find("TempMapEditorGrid"));
            else {
                HierarchyProperty prop = new HierarchyProperty(HierarchyType.GameObjects);
                GameObject gridObj = Instantiate(gridPrefab,new Vector3(0,0,0),Quaternion.Euler(90,0,0));
                gridObj.name = "TempMapEditorGrid";
            }
        }
        if(selectedTile != null)
        {
            if(Selection.gameObjects.Length == 1)
            {
                if(selectedTile.GetComponent<Renderer>() != null)
                {
                    if(selectedTile.GetComponent<MeshFilter>() != null)
                    {
                        EditorGUILayout.Separator();
                        EditorGUILayout.LabelField("Tile Editor:", EditorStyles.boldLabel);
                        MeshFilter meshFilter = selectedTile.GetComponent<MeshFilter>();
                        Renderer renderer = selectedTile.GetComponent<Renderer>();
                        Material[] mat = renderer.sharedMaterials;
                        if(meshFilter.sharedMesh.name.Contains("cliff") && mat.Length == 2)
                        {
                            mat[0] = (Material)EditorGUILayout.ObjectField("Cliff Mat",renderer.sharedMaterials[0],typeof(Material),false);
                        }
                        else if(meshFilter.sharedMesh.name.Contains("stair"))
                            mat[0] = (Material)EditorGUILayout.ObjectField("Stair Mat",renderer.sharedMaterials[0],typeof(Material),false);
                        if(selTile != null) {
                            for(int i = mat.Length;i < selTile.materials.Length;i++)
                            {
                                mat[i] = selTile.materials[i];
                                selTile = null;
                            }
                        }
                        bool found = false;
                        for(int i = 0; i < mat.Length; i++)
                        {
                            if(found)
                                break;
                            for(int e = 0; e < castedPresets.Length; e++)
                            {
                                if(mat[i].name == castedPresets[e])
                                {
                                    found = true;
                                    matInt = i;
                                    if(firstRun)
                                        preset = (PresetTiles)e;
                                    break;
                                }
                            }
                        }
                        if(noPresetTile)
                        {
                            if(meshFilter.sharedMesh.name == "tile" && mat.Length == 1)
                                mat[0] = (Material)EditorGUILayout.ObjectField("Tile Mat",renderer.sharedMaterials[0],typeof(Material),false);
                            else if(meshFilter.sharedMesh.name.Contains("cliff") && mat.Length == 2)
                                mat[1] = (Material)EditorGUILayout.ObjectField("Overlay Mat",renderer.sharedMaterials[1],typeof(Material),false);
                            else if(meshFilter.sharedMesh.name == "stairDirt" && mat.Length == 2)
                                mat[1] = (Material)EditorGUILayout.ObjectField("Side Mat",renderer.sharedMaterials[1],typeof(Material),false);
                        }
                        else if(matInt < mat.Length && matInt >= 0)
                        {
                            if(!firstRun)
                                mat[matInt] = (Material)AssetDatabase.LoadAssetAtPath("Assets/MapCreation/Materials/"+castedPresets[(int)preset]+".mat", typeof(Material));
                        }
                        else if(matInt < 0 || matInt >= mat.Length)
                        {
                            matInt = EditorGUILayout.IntField("Mat Element",matInt);
                        }
                        firstRun = true;
                        PresetTiles oldpreset = preset;
                        if(!noPresetTile)
                            preset = (PresetTiles)EditorGUILayout.EnumPopup("Tile Preset",preset);
                        if(oldpreset != preset)
                            firstRun = false;
                        noPresetTile = EditorGUILayout.Toggle("Use Non-Preset",noPresetTile);
                        if(GUILayout.Button("Select Setups"))
                        {
                            GenericMenu menu = new GenericMenu();
                            foreach(Tile tile in setupTiles)
                            {
                                AddTileItem(menu, tile.name, tile);
                            }
                            // display the menu
                            menu.ShowAsContext();
                        }
                        renderer.sharedMaterials = mat;
                    }
                }
                if(selectedTile.GetComponent<Footstep>() != null)
                {
                    Footstep footstep = selectedTile.GetComponent<Footstep>();
                    EditorGUILayout.Separator();
                    EditorGUILayout.LabelField("Footstep Editor:", EditorStyles.boldLabel);
                    footstep.walkClip = (AudioClip)EditorGUILayout.ObjectField("Footstep",footstep.walkClip,typeof(AudioClip),false);
                    footstep.gizmoColor = EditorGUILayout.ColorField("Gizmo Color",footstep.gizmoColor);
                }
                EditorGUILayout.Separator();
                if(selectedMap != null)
                {
                    if(selectedMap.transform.parent != null)
                        selectedMap = selectedMap.transform.parent.gameObject;
                    EditorGUILayout.Separator();
                    if(selectedMap.GetComponent<MapCollider>() != null)
                    {
                        EditorGUILayout.LabelField("Map Collision:", EditorStyles.boldLabel);
                        MapCollider mapCollider = selectedMap.GetComponent<MapCollider>();
                        if(GUILayout.Button("Generate Collision Mesh"))
                        {
                            mapCollider.shorthandCollisionMap = "0x4";
                            mapCollider.width = 2;
                            mapCollider.length = 2;
                            MapCompiler.Compile(selectedMap);
                        }
                        mapCollider.drawWireframe = EditorGUILayout.Toggle("Wireframe Gizmo",mapCollider.drawWireframe);
                        mapCollider.wireframeColor = EditorGUILayout.ColorField("Wireframe Color",mapCollider.wireframeColor);
                    }
                    EditorGUILayout.Separator();
                    if(selectedMap.GetComponent<MapSettings>() != null)
                    {
                        EditorGUILayout.LabelField("Map Settings:", EditorStyles.boldLabel);
                        MapSettings mapSettings = selectedMap.GetComponent<MapSettings>();
                        mapSettings.mapName = EditorGUILayout.TextField(new GUIContent("Map Name"),mapSettings.mapName);
                        mapSettings.mapNameBoxTexture = (Sprite)EditorGUILayout.ObjectField("Map Name Box Sprite",mapSettings.mapNameBoxTexture,typeof(Sprite),false);
                        mapSettings.mapNameColor = EditorGUILayout.ColorField("Map Name Color",mapSettings.mapNameColor);
                        mapSettings.mapBGMClip = (AudioClip)EditorGUILayout.ObjectField("Map BGM",mapSettings.mapBGMClip,typeof(AudioClip),false);
                        mapSettings.mapBGMNightClip = (AudioClip)EditorGUILayout.ObjectField("Map BGM 2",mapSettings.mapBGMNightClip,typeof(AudioClip),false);
                        mapSettings.mapBGMLoopStartSamples = EditorGUILayout.IntField("Map BGM Loop Samples",mapSettings.mapBGMLoopStartSamples);
                        mapSettings.mapBGMNightLoopStartSamples = EditorGUILayout.IntField("Map BGM 2 Loop Samples",mapSettings.mapBGMNightLoopStartSamples);
                        EditorGUILayout.Space();
                        mapSettings.environment = (MapSettings.Environment)EditorGUILayout.EnumPopup("Map Environment",mapSettings.environment);
                        mapSettings.environment2 = (MapSettings.Environment)EditorGUILayout.EnumPopup("Map Environment 2",mapSettings.environment2);
                        mapSettings.pokemonRarity = (MapSettings.PokemonRarity)EditorGUILayout.EnumPopup("Pokémon Rarity",mapSettings.pokemonRarity);
                        SerializedObject serializedObject = new SerializedObject(mapSettings);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("encounters"),new GUIContent("Map Encounters"),true);
                        serializedObject.ApplyModifiedProperties();
                    }
                }
            }
            else
                EditorGUILayout.LabelField("Cannot process multiple selections!", EditorStyles.boldLabel);
        }
        else
            EditorGUILayout.LabelField("No tile or map selected!", EditorStyles.boldLabel);
        serializedThis.ApplyModifiedProperties();
    }
}