using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


[RequireComponent(typeof(MazeConstructor))]//Атрибут RequireComponent обеспечивает добавление компонента MazeConstructor при добавлении этого скрипта к GameObject

public class GameController : MonoBehaviour
{
    //сериализованные поля для объектов в сцене
    //Сериализация в Юнити используется для отображения приватных членов в инспекторе. Т.е. Юнити в инспекторе отображает члены класса, которые она может сериализовать.
    [SerializeField] private FpsMovement player;
    [SerializeField] private Text timeLabel;
    [SerializeField] private Text scoreLabel;
    [SerializeField] private Text startOverLabel;

    private MazeConstructor generator;

    //Частные переменные для отслеживания таймера и очков игры, а также того, найдена ли цель в лабиринте.
    private DateTime startTime;
    private int timeLimit;
    private int reduceLimitBy;

    private int score;
    private bool goalReached;

    void Start()//На СтАрТ!
    {
        generator = GetComponent<MazeConstructor>(); //Частная переменная хранит ссылку, возвращаемую GetComponent()
        StartNewGame();
    }

    //StartNewGame() используется для запуска всей игры сначала, а не для переключения уровней внутри игры. Таймеру присваиваются исходные значения, очки сбрасываются, после чего создаётся лабиринт.
    private void StartNewGame()
    {
        timeLimit = 80;//начальный лимит времени - 80-5(условие вычета времени срабатывает и при первом применении)=75
        reduceLimitBy = 5;//с каждым новым лабиринтом лимит времени уменьшается на 5
        startTime = DateTime.Now;

        score = 0; //начальный счёт = 0
        scoreLabel.text = score.ToString();
        startOverLabel.text = "";

        StartNewMaze();
    }

    //StartNewMaze() переходит к новому уровню, не перезапуская заново всю игру. Кроме создания нового лабиринта, этот метод располагает игрока в начальной точке, сбрасывает цель и снижает лимит времени.
    private void StartNewMaze()
    {
        generator.GenerateNewMaze(13, 15, OnStartTrigger, OnGoalTrigger); //Вызывает метод MazeConstructor.GenerateNewMaze()
        //Числа 13 и 15 — это параметры метода, определяющие размеры лабиринта. Эти параметры размера задают количество строк и столбцов сетки.

        float x = generator.startCol * generator.hallWidth;
        float y = 1;
        float z = generator.startRow * generator.hallWidth;
        player.transform.position = new Vector3(x, y, z);//обязательно включить auto sync transformation в Edit>>Project settings>>physics, 
        //иначе перемещение не будет корректно работать и координаты иногда будут возвращаться к предыдущему значению, оставляя игрока на месте

        goalReached = false;
        player.enabled = true;

        // restart timer
        timeLimit -= reduceLimitBy;
        startTime = DateTime.Now;
    }

    //Update() проверяет, активен ли игрок, а затем обновляет время, оставшееся на прохождение уровня. После завершения времени игрок деактивируется и начинается новая игра.
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && (!player.enabled))//если нажать пробел, когда время вышло
        {
            Invoke("StartNewGame", 0);//начинаем игру заново
        }
        if (Input.GetKeyDown(KeyCode.Escape))  // если нажат escape
        {
            SceneManager.LoadScene("Menu");// выйти в меню
            Cursor.visible = true;
        }
        if (!player.enabled)
        {
            return;
        }

        int timeUsed = (int)(DateTime.Now - startTime).TotalSeconds;//таймер
        int timeLeft = timeLimit - timeUsed;

        if (timeLeft > 0)//пока время есть
        {
            timeLabel.text = timeLeft.ToString();
        }
        else//время вышло
        {
            timeLabel.text = "ВРЕМЯ ВЫШЛО\nТы заработал ";
            timeLabel.text += score;
            timeLabel.text += " баллов";
            player.enabled = false;//выключаем игрока когда заканчивается время
            startOverLabel.text = "ЧТОБЫ НАЧАТЬ НОВУЮ ИГРУ НАЖМИ ПРОБЕЛ";

            //Invoke("StartNewGame", 0);//начинаем игру заново
        }
    }

    //OnGoalTrigger() и OnStartTrigger() — это функции обработки событий, передаваемые TriggerEventRouter в MazeConstructor. 
    //OnGoalTrigger() записывает, что цель была найдена, а затем увеличивает количество очков. 
    //OnStartTrigger() проверяет, найдена ли цель, и если это так, то деактивирует игрока и запускает новый лабиринт.
    private void OnGoalTrigger(GameObject trigger, GameObject other)
    {
        Debug.Log("Goal!");
        goalReached = true;

        //score += 5; //Здесь выполняется условие на набор очков при подборе цели
        //scoreLabel.text = score.ToString();

        Destroy(trigger);
    }
    private void OnStartTrigger(GameObject trigger, GameObject other)
    {
        if (goalReached)
        {
            score += 5;//Здесь выполняется условие при подборе цели и возвращении на стартовую точку(как бы вынос сокровища)
            scoreLabel.text = score.ToString();

            Debug.Log("Finish!");
            player.enabled = false;

            Invoke("StartNewMaze", 3);
        }
    }
}