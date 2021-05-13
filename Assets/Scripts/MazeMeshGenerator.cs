using System.Collections.Generic;
using UnityEngine;

public class MazeMeshGenerator
{
    // generator params
    public float width;     // how wide are hallways
    public float height;    // how tall are hallways

    public MazeMeshGenerator()
    {
        //width и height, аналогичны placementThreshold из MazeDataGenerator: это значения, которые в конструкторе задаются по умолчанию и используемые кодом генерирования меша.
        width = 3.75f;
        height = 3.5f;
    }

        public Mesh FromData(int[,] data)//это метод, который MazeConstructor вызывает для генерирования меша.
        {
            Mesh maze = new Mesh();

            //Создание списков для вершин UV и треугольников. У нас есть два списка треугольников. 
            //Объект Mesh Unity может иметь множество подмешей с различными материалами на каждом, то есть каждый список треугольников является отдельным подмешем. 
            //Мы объявляем два подмеша, чтобы можно было назначить разные материалы полу и стенам.
            List<Vector3> newVertices = new List<Vector3>();
            List<Vector2> newUVs = new List<Vector2>();

            maze.subMeshCount = 2;
            List<int> floorTriangles = new List<int>();
            List<int> wallTriangles = new List<int>();

            int rMax = data.GetUpperBound(0);
            int cMax = data.GetUpperBound(1);
            float halfH = height * .5f;

        //После этого мы проходим по 2D-массиву и создаём четырёхугольники для пола, потолка и стен в каждой ячейке сетки. 
        //Каждой ячейке нужен пол и потолок, кроме того, выполняются проверки соседних ячеек на необходимость стен. 
        //AddQuad() вызывается несколько раз, но каждый раз с другой матрицей преобразований и разными списками треугольников, используемыми для полов и стен. 
        //Также для определения расположения и размеров четырёхугольников используются width и height.
        for (int i = 0; i <= rMax; i++)
            {
                for (int j = 0; j <= cMax; j++)
                {
                    if (data[i, j] != 1)
                    {
                        // floor
                        AddQuad(Matrix4x4.TRS(
                            new Vector3(j * width, 0, i * width),
                            Quaternion.LookRotation(Vector3.up),
                            new Vector3(width, width, 1)
                        ), ref newVertices, ref newUVs, ref floorTriangles);

                        // ceiling
                        AddQuad(Matrix4x4.TRS(
                            new Vector3(j * width, height, i * width),
                            Quaternion.LookRotation(Vector3.down),
                            new Vector3(width, width, 1)
                        ), ref newVertices, ref newUVs, ref floorTriangles);


                        // walls on sides next to blocked grid cells

                        if (i - 1 < 0 || data[i - 1, j] == 1)
                        {
                            AddQuad(Matrix4x4.TRS(
                                new Vector3(j * width, halfH, (i - .5f) * width),
                                Quaternion.LookRotation(Vector3.forward),
                                new Vector3(width, height, 1)
                            ), ref newVertices, ref newUVs, ref wallTriangles);
                        }

                        if (j + 1 > cMax || data[i, j + 1] == 1)
                        {
                            AddQuad(Matrix4x4.TRS(
                                new Vector3((j + .5f) * width, halfH, i * width),
                                Quaternion.LookRotation(Vector3.left),
                                new Vector3(width, height, 1)
                            ), ref newVertices, ref newUVs, ref wallTriangles);
                        }

                        if (j - 1 < 0 || data[i, j - 1] == 1)
                        {
                            AddQuad(Matrix4x4.TRS(
                                new Vector3((j - .5f) * width, halfH, i * width),
                                Quaternion.LookRotation(Vector3.right),
                                new Vector3(width, height, 1)
                            ), ref newVertices, ref newUVs, ref wallTriangles);
                        }

                        if (i + 1 > rMax || data[i + 1, j] == 1)
                        {
                            AddQuad(Matrix4x4.TRS(
                                new Vector3(j * width, halfH, (i + .5f) * width),
                                Quaternion.LookRotation(Vector3.back),
                                new Vector3(width, height, 1)
                            ), ref newVertices, ref newUVs, ref wallTriangles);
                        }
                    }
                }
            }

            maze.vertices = newVertices.ToArray();
            maze.uv = newUVs.ToArray();

            maze.SetTriangles(floorTriangles.ToArray(), 0);
            maze.SetTriangles(wallTriangles.ToArray(), 1);

            //RecalculateNormals() подготавливает меш к освещению
            maze.RecalculateNormals();

            return maze;
        }

    //Код генерирования четырёхугольника для повторного вызова в создании пола, потолка и стен каждой ячейки сетки.

    //Последние три параметра AddQuad() — это список вершин, UV и треугольников.
    //Первая строка метода получает индекс, с которого нужно начинать. При добавлении новых четырёхугольников индекс будет увеличиваться.

    //Первый параметр AddQuad() — это матрица преобразований.
    //Положение/поворот/масштаб может храниться в виде матрицы, а затем применяться к вершинам. Именно это делает вызов MultiplyPoint3x4(). 
    //Таким образом, код генерирования четырёхугольника можно использовать для полов, потолков, стен и т.д. Достаточно лишь изменять используемую матрицу преобразований.
    private void AddQuad(Matrix4x4 matrix, ref List<Vector3> newVertices,
            ref List<Vector2> newUVs, ref List<int> newTriangles)
        {
            int index = newVertices.Count;

            // corners before transforming
            Vector3 vert1 = new Vector3(-.5f, -.5f, 0);
            Vector3 vert2 = new Vector3(-.5f, .5f, 0);
            Vector3 vert3 = new Vector3(.5f, .5f, 0);
            Vector3 vert4 = new Vector3(.5f, -.5f, 0);

            //Список вершин хранит позицию каждой вершины 
            newVertices.Add(matrix.MultiplyPoint3x4(vert1));
            newVertices.Add(matrix.MultiplyPoint3x4(vert2));
            newVertices.Add(matrix.MultiplyPoint3x4(vert3));
            newVertices.Add(matrix.MultiplyPoint3x4(vert4));

            //Перечисленные координаты UV соответствуют вершинам в этом списке
            newUVs.Add(new Vector2(1, 0));
            newUVs.Add(new Vector2(1, 1));
            newUVs.Add(new Vector2(0, 1));
            newUVs.Add(new Vector2(0, 0));

            //А треугольники являются индексами в списке вершин (т.е. «этот треугольник состоит из вершин index, index+1, index+2»)
            newTriangles.Add(index + 2);
            newTriangles.Add(index + 1);
            newTriangles.Add(index);

            //Cоздаются два треугольника; четырёхугольник состоит из двух треугольников
            newTriangles.Add(index + 3);
            newTriangles.Add(index + 2);
            newTriangles.Add(index);
        }
}