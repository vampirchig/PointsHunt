using UnityEngine;

public class MazeConstructor : MonoBehaviour
{
    //свойства для хранения размеров и координат
    public float hallWidth
    {
        get; private set;
    }
    public float hallHeight
    {
        get; private set;
    }

    public int startRow
    {
        get; private set;
    }
    public int startCol
    {
        get; private set;
    }

    public int goalRow
    {
        get; private set;
    }
    public int goalCol
    {
        get; private set;
    }

    //Все эти поля доступны нам в Inspector. showDebug переключает отображение отладки, а различные ссылки Material являются материалами для генерируемых моделей. 
    public bool showDebug;

    [SerializeField] private Material mazeMat1;
    [SerializeField] private Material mazeMat2;
    [SerializeField] private Material startMat;
    [SerializeField] private Material treasureMat;
    [SerializeField] private GameObject plusFive;

    //Свойство data. Объявления доступа (например, объявление свойства как public, но затем назначение его private set) делает его read-only за пределами класса. Таким образом, данные лабиринта невозможно будет изменять извне.
    public int[,] data
    {
        get; private set;
    }

    private MazeDataGenerator dataGenerator;//Частная переменная для хранения генератора данных

    private MazeMeshGenerator meshGenerator;//Частное поле для хранения генератора меша.

    //Функция инициализирует data с массивом 3 x 3 из единиц, окружающих ноль. 1 означает стену, а 0 — пустое пространство, то есть сетка по умолчанию выглядит как окружённая стеной комната.
    void Awake()
    {
        dataGenerator = new MazeDataGenerator();//Создаем экземпляр генератора данных, сохраняя генератор в новую переменную

        meshGenerator = new MazeMeshGenerator();//Создаем экземпляр генератора меша, сохранив генератор в новом поле

        // default to walls surrounding a single empty cell
        data = new int[,]
        {
            {1, 1, 1},
            {1, 0, 1},
            {1, 1, 1}
        };
    }

    public void GenerateNewMaze(int sizeRows, int sizeCols,
    TriggerEventHandler startCallback = null, TriggerEventHandler goalCallback = null)
    {
        if (sizeRows % 2 == 0 && sizeCols % 2 == 0)
        {
            Debug.LogError("Odd numbers work better for dungeon size.");//Для размеров лучше использовать нечётные числа, потому что сгенерированный лабиринт будет окружён стенами.
        }

        DisposeOldMaze();//Вызываем метод DisposeOldMaze() для удаления лабиринта

        data = dataGenerator.FromDimensions(sizeRows, sizeCols);//Вызываем FromDimensions() в GenerateNewMaze() передавая размер сетки и сохраняя получившиеся данные.

        FindStartPosition();//находим стартовую позицию для игрока
        FindGoalPosition();//находим позицию для цели

        // store values used to generate this mesh
        hallWidth = meshGenerator.width;
        hallHeight = meshGenerator.height;

        DisplayMaze();//Вызываем метод DisplayMaze() для отображения лабиринта

        PlaceStartTrigger(startCallback);//устанавливаем стартовые координаты игрока
        PlaceGoalTrigger(goalCallback);//устанавливаем координаты цели
    }

    void OnGUI()
    {
        //Проверяет, включено ли отображение отладки.
        if (!showDebug)
        {
            return;
        }

        //Инициализация нескольких локальных переменных: локальная копия сохранённого лабиринта, максимальная строка и столбец, а также строка.
        int[,] maze = data;
        int rMax = maze.GetUpperBound(0);
        int cMax = maze.GetUpperBound(1);

        string msg = "";

        //Два вложенных цикла проходят по строкам и столбцам двухмерного массива. Для каждой строки/столбца массива код проверят сохранённое значение и добавляет "...." или "==" в зависимости от того, равно ли значение нулю. Также после прохождения по всем столбцам в строке код добавляет новую строку, чтобы каждая строка массива начиналась с новой строки line.
        for (int i = rMax; i >= 0; i--)
        {
            for (int j = 0; j <= cMax; j++)
            {
                if (maze[i, j] == 0)
                {
                    msg += "....";
                }
                else
                {
                    msg += "==";
                }
            }
            msg += "\n";
        }

        //GUI.Label() выводит создаваемую строку.
        GUI.Label(new Rect(20, 20, 500, 500), msg);


    }

    //DisplayMaze() не только вызывает MazeMeshGenerator.FromData(), но и вставляет этот вызов посередине создания экземпляра нового GameObject,
    //задавая тэг Generated, добавляя MeshFilter и сгенерированный меш, 
    //добавляя MeshCollider для коллизий с мешем, и, наконец, добавляя MeshRenderer и материалы.
    private void DisplayMaze()
    {
        GameObject go = new GameObject();
        go.transform.position = Vector3.zero;
        go.name = "Procedural Maze";
        go.tag = "Generated";

        MeshFilter mf = go.AddComponent<MeshFilter>();
        mf.mesh = meshGenerator.FromData(data);

        MeshCollider mc = go.AddComponent<MeshCollider>();
        mc.sharedMesh = mf.mesh;

        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.materials = new Material[2] { mazeMat1, mazeMat2 };
    }

    public void DisposeOldMaze()//Метод удаляет существующий лабиринт: находит все объекты с тэгом Generated и уничтожает их. Используем для генерации нового этажа лабиринта
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag("Generated");
        foreach (GameObject go in objects)
        {
            Destroy(go);
        }
    }

    private void FindStartPosition()//Метод начинает с 0,0 и проходит по всем данным лабиринта, пока не находит открытое пространство. Затем эти координаты сохраняются как начальная позиция лабиринта.
    {
        int[,] maze = data;
        int rMax = maze.GetUpperBound(0);
        int cMax = maze.GetUpperBound(1);

        for (int i = 0; i <= rMax; i++)
        {
            for (int j = 0; j <= cMax; j++)
            {
                if (maze[i, j] == 0)
                {
                    startRow = i;
                    startCol = j;
                    return;
                }
            }
        }
    }

    private void FindGoalPosition()//Аналогично, FindStartPosition() по сути делает то же самое, только начинает с максимальных значений и выполняет обратный отсчёт, задавая позицию цели
    {
        int[,] maze = data;
        int rMax = maze.GetUpperBound(0);
        int cMax = maze.GetUpperBound(1);

        // loop top to bottom, right to left
        for (int i = rMax; i >= 0; i--)
        {
            for (int j = cMax; j >= 0; j--)
            {
                if (maze[i, j] == 0)
                {
                    goalRow = i;
                    goalCol = j;
                    return;
                }
            }
        }
    }

    //PlaceStartTrigger() и PlaceGoalTrigger() размещают объекты в сцене в позициях начала и цели. 
    //Их коллайдер является триггером, применяется соответствующий материал, а затем добавляется TriggerEventRouter (из заготовки проекта). 
    //Этот компонент получает функцию обработки события, которая вызывается, когда что-то входит в объём триггера. Добавим и эти два метода.
    private void PlaceStartTrigger(TriggerEventHandler callback)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.position = new Vector3(startCol * hallWidth, .5f+0.75f, startRow * hallWidth);
        go.name = "Start Trigger";
        go.tag = "Generated";

        go.GetComponent<BoxCollider>().isTrigger = true;
        go.GetComponent<MeshRenderer>().sharedMaterial = startMat;

        TriggerEventRouter tc = go.AddComponent<TriggerEventRouter>();
        tc.callback = callback;
    }
    private void PlaceGoalTrigger(TriggerEventHandler callback)
    {
        GameObject go = Instantiate(plusFive);
        go.transform.position = new Vector3(goalCol * hallWidth, .5f, goalRow * hallWidth);
        go.name = "Treasure";
        go.tag = "Generated";

        go.GetComponent<BoxCollider>().isTrigger = true;
        //go.GetComponent<MeshRenderer>().sharedMaterial = treasureMat;

        TriggerEventRouter tc = go.AddComponent<TriggerEventRouter>();
        tc.callback = callback;
    }
}