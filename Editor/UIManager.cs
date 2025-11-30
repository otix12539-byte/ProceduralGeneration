// Assets/Editor/UIManager.cs
using UnityEditor.SceneManagement;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;


public class UIManager : EditorWindow
{
    Generator generator;

    // Serialized copy of fields
    Generator.Algo algorithm;
    int width, height;
    float scale;
    float threshold;
    float fillProbability;
    int caSteps, birthLimit, deathLimit;
    int seed;
    bool randomSeed;
    bool saveSeedToPlayerPrefs;

    Tilemap tilemapRef;

    [MenuItem("Tools/Генератор Рівнів")]
    public static void ShowWindow()
    {
        GetWindow<UIManager>("Генератор Рівнів");
    }

    void OnEnable()
    {
        RefreshFromScene();
    }

    void OnGUI()
    {
        GUILayout.Label("Генератор рівнів (сцена)", EditorStyles.boldLabel);

        if (generator == null)
        {
            EditorGUILayout.HelpBox(
                "Компонент 'Generator' не знайдено на сцені. Додайте його на будь-який GameObject.",
                MessageType.Warning
            );

            if (GUILayout.Button("Знайти Generator на сцені"))
                RefreshFromScene();

            return;
        }

        EditorGUILayout.LabelField("Об'єкт Generator:", generator.gameObject.name);

        algorithm = (Generator.Algo)EditorGUILayout.EnumPopup("Алгоритм", algorithm);

        GUILayout.Space(10);
        GUILayout.Label("Параметри карти", EditorStyles.boldLabel);

        width = EditorGUILayout.IntField("Ширина", width);
        height = EditorGUILayout.IntField("Висота", height);

        GUILayout.Space(10);
        GUILayout.Label("Perlin шум", EditorStyles.boldLabel);

        scale = EditorGUILayout.FloatField("Масштаб", scale);
        threshold = EditorGUILayout.Slider("Поріг", threshold, 0f, 1f);

        GUILayout.Space(10);
        GUILayout.Label("Клітинний автомат", EditorStyles.boldLabel);

        fillProbability = EditorGUILayout.Slider("Ймовірність заповнення", fillProbability, 0f, 1f);
        caSteps = EditorGUILayout.IntField("Кількість ітерацій", caSteps);
        birthLimit = EditorGUILayout.IntField("Поріг народження", birthLimit);
        deathLimit = EditorGUILayout.IntField("Поріг смерті", deathLimit);

        GUILayout.Space(10);
        GUILayout.Label("Seed / Детермінованість", EditorStyles.boldLabel);

        randomSeed = EditorGUILayout.Toggle("Випадковий seed", randomSeed);
        seed = EditorGUILayout.IntField("Seed", seed);
        saveSeedToPlayerPrefs = EditorGUILayout.Toggle("Зберегти seed у PlayerPrefs", saveSeedToPlayerPrefs);

        GUILayout.Space(10);

        if (GUILayout.Button("Застосувати до Generator"))
            ApplyToGenerator();

        if (GUILayout.Button("Згенерувати (Play Mode)"))
        {
            ApplyToGenerator();

            if (Application.isPlaying)
                generator.GenerateOnStart();
            else
                Debug.LogWarning("Не в Play Mode. Скористайтеся кнопкою 'Згенерувати (в редакторі)'.");
        }

        if (GUILayout.Button("Показати об'єкт Generator"))
            Selection.activeObject = generator.gameObject;
    }

    void RefreshFromScene()
    {
        generator = FindObjectOfType<Generator>();
        if (generator == null) return;

        algorithm = generator.algorithm;
        width = generator.width;
        height = generator.height;

        scale = generator.scale;
        threshold = generator.threshold;

        fillProbability = generator.fillProbability;
        caSteps = generator.caSteps;
        birthLimit = generator.birthLimit;
        deathLimit = generator.deathLimit;

        seed = generator.seed;
        randomSeed = generator.randomSeed;
        saveSeedToPlayerPrefs = generator.saveSeedToPlayerPrefs;

        tilemapRef = generator.tilemap;
    }

    void ApplyToGenerator()
    {
        if (!generator)
        {
            Debug.LogWarning("Generator не знайдено.");
            return;
        }

        if (!Application.isPlaying)
            Undo.RecordObject(generator, "Modify Generator");

        generator.algorithm = algorithm;
        generator.width = Mathf.Max(1, width);
        generator.height = Mathf.Max(1, height);

        generator.scale = scale;
        generator.threshold = threshold;

        generator.fillProbability = Mathf.Clamp01(fillProbability);
        generator.caSteps = Mathf.Max(0, caSteps);
        generator.birthLimit = birthLimit;
        generator.deathLimit = deathLimit;

        generator.seed = seed;
        generator.randomSeed = randomSeed;
        generator.saveSeedToPlayerPrefs = saveSeedToPlayerPrefs;

        EditorUtility.SetDirty(generator);

        if (!Application.isPlaying)
            EditorSceneManager.MarkSceneDirty(generator.gameObject.scene);
    }

    void GenerateNow()
    {
        if (!generator)
        {
            Debug.LogWarning("Generator не знайдено.");
            return;
        }

        generator.GenerateMap();
        generator.DrawMap();

        if (generator.tilemap != null)
            generator.tilemap.RefreshAllTiles();

        if (!Application.isPlaying)
            EditorSceneManager.MarkSceneDirty(generator.gameObject.scene);

        Debug.Log("Генерацію виконано в редакторі для об'єкта: " + generator.gameObject.name);
    }
}