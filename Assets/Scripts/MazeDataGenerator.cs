using System.Collections.Generic;
using UnityEngine;

public class MazeDataGenerator //Класс не будет использоваться непосредственно как компонент, а только внутри MazeConstructor
{
    public float placementThreshold;    // chance of empty space

    public MazeDataGenerator()
    {
        placementThreshold = .1f;                               
        //placementThreshold будет использоваться алгоритмом генерирования данных для определения того, пусто ли пространство. 
        //В конструкторе класса этой переменной назначается значение по умолчанию, но она сделана public, чтобы другой код мог управлять настройкой генерируемого лабиринта.
    }

    public int[,] FromDimensions(int sizeRows, int sizeCols)    // 2
    {
        int[,] maze = new int[sizeRows, sizeCols];
        int rMax = maze.GetUpperBound(0);
        int cMax = maze.GetUpperBound(1);

        for (int i = 0; i <= rMax; i++)
        {
            for (int j = 0; j <= cMax; j++)
            {
                //Для каждой ячейки сетки код сначала проверяет, выходит ли текущая ячейка за пределы сетки (то есть находится ли какой-то из индексов на границе массива). Если это так, то он ставит стену, присваивая 1.
                if (i == 0 || j == 0 || i == rMax || j == cMax)
                {
                    maze[i, j] = 1;
                }

                //Далее код проверяет, делятся ли координаты на 2 нацело, чтобы выполнять действия в каждой второй ячейке. 
                //Также здесь есть дополнительная проверка на описанное выше значение placementThreshold для пропуска случайным образом этой ячейки и продолжения обхода массива.
                else if (i % 2 == 0 && j % 2 == 0)
                {
                    if (Random.value > placementThreshold)
                    {
                        //Наконец, код присваивает значение 1 текущей ячейке и случайно выбранной соседней ячейке. 
                        //Код использует несколько тернарных операций для прибавления к индексу массива 0, 1 или -1, получая таким образом индекс соседней ячейки.
                        maze[i, j] = 1;

                        int a = Random.value < .5 ? 0 : (Random.value < .5 ? -1 : 1);
                        int b = a != 0 ? 0 : (Random.value < .5 ? -1 : 1);
                        maze[i + a, j + b] = 1;
                    }
                }
            }
        }
        return maze;
    }
}