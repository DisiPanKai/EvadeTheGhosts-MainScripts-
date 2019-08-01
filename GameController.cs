using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;


[Serializable]
public class GameController : MonoBehaviour
{
    public Vector3[] ghostSpawnCoordinates; //точки спауна призраков
    public Vector3[] ectoplasmSpawnCoordinates; //точки спауна экто в начале раунда и для тех экто, которые выползают из подарка
    public bool[] PositionState = new bool[25]; //массив для того, чтобы бонус/экто/подарок не появлялись на уже занятой клетке
    //Как работает: образно отображает 25 клеток на поле. Определяет, занята ли клетка бонусом/экто/подарком
    //также, напрямую связан с ectoplasmSpawnCoordinates. Позиции PositionState соответсвуют последовательности координат ectoplasmSpawnCoordinates 
    public Sprite[] backSprites; //спрайты внешнего фона
    public Sprite[] fieldSprites; //спрайты полей
    public Sprite[] ectoSprites; //набор спрайтов для маленьких эктоплазм
    public Sprite[] bigEctoSprites; //набор спрайтов для больших эктоплазм
    public Sprite[] bonusesOnField; //спрайты бонусов на поле
    public AnimationClip[] ghostAnimationClips; //все анимации призраков
    public AnimationClip[] presentAnimationClips2; //анимации подарков
    public AnimationClip[] presentAnimationClips3;
    public AnimationClip[] presentAnimationClips4;
    public AnimationClip[] presentAnimationClips5;
    public AnimationClip[] presentAnimationClips6;
    public AnimationClip[] presentAnimationClips7;
    public GameObject ghostPrefab; //шаблон для создания призраков
    public GameObject parentEctoPrefab; //шаблон для создания эктоплазмы
    public GameObject gameOverPrefab; //вызывающееся меню при столкновении с призраком
    public GameObject presentOnFieldPrefab; //подарок с эктоплазмами и бонусами, если последние имеются
    public GameObject thunderPrefab; //гроза за окном
    public GameObject coolnessPrefab; //холодок при выходе призрака
    public GameObject bonusPrefab; //шаблон для создания бонусов на поле

    public List<Vector3> AllEctoPositions = new List<Vector3>(); //массив для местополежения эктоплазм на поле
    public List<Vector3> AllBonusPositions = new List<Vector3>(); //массив для положений бонусов на поле
    public Vector3 EctoPositionForGhostSpawn; //координата для того, чтобы призрак не появился на одной и той же линии, что и игрок. См. метод PickUpEctoplasm
    public Vector3 LastPlayerPos; //последняя позиция игрока. нужна для того, чтобы на игроке не появились экто или бонусы
    public int GhostScore; //счёт эктоплазм до появления привидения
    public int EctoplasmOnFieldValue; //к-ство экто на поле
    public int RandomGhostSpawn; //рандомизатор для позиций спауна
    public int SaveGhostSpawn; //сохранённая позиция для неповторения появления призрака
    public int GhostAmount; //к-ство призраков на поле. нужна для подсчёта звёздочек на выбранном игроком скине, для записи рекорда и 
    //для удаления в конце раунда всех призраков
    public int HowMuchNeedGhosts; //сколько нужно экто для спауна нового призрака
    public int GhostSpriteNumber; //номер спрайта призрака в массиве, с которым столкнулся игрок и нужен для добавления в лист с купленными призраками
    public int GhostAmountForAlbum; //под конец игры сюда записывается количество призраков на поле для того, чтобы иметь представление, сколько в альбоме показывать призраков
    public int[] Stars; //таблица звёздочек, заработанные на скинах игрока
    private int _carpetStage; //номер стадии для обучающих ковров
    private float _timer; //таймер для появления экто и бонусов из подарка
    public int KeyPrice; //цена ключа
    //private int _notDiscountedKeyPrice; //цена для ключа со скидкой
    public int KeyStorage; //сколько экто собрали для ключа
    public int KeyValue; //количество ключей

    public int EctoLimit; //ограничение количества экто на поле
    public float GhostSpeed; //скорость всех призраков на поле
    public float SpawnZone; //цифра для координат спауна призраков
    public bool KeyBarReset; //условие для уже заполненной полоски ключа
    public bool GhostBarReset; //условие для уже заполненной полоски призраков
    private int _carpetPickedUpEcto; //счётчик подбора экто для анимации корвиков
    private int _counterForGhostPosition; //сколько раз мы можем перенаправить призрака на начале спауна при соприкосновении впритык с другим призраком

    private bool _waitForSpawn; //задержка для спауна всех экто/бонусов/подарков, когда открываем подарок
    public int Record; //рекордное число приведений
    public int GhostOpened; //сколько можно использовать скинов игроку
    private int _randomSkin; //рандомизатор для спрайтов
    bool dontCreate; //запрещает создавать призраков, если игра закончена
    public List<int> GhostSkinIdOnField = new List<int>(); //массив для ID анимаций призраков на поле
    public List<int> GhostAnimationNumberBought = new List<int>(); //массив для купленных призраков 
    public List<List<int>> ActiveBonuses = new List<List<int>>();

    private int _presentNumber; //номер текущего подарка
    private int _ectoCounterFromPresent; //счётчик для эктоплзам. контролирует, сколько экто должно выйти из подарка
    private int _bonusCounterFromPresent; //счётчик для бонусов
    bool _presentSpawned; //заспаунился ли подарок. Команда на спаун подарка, если в этот момент сыпятся бонусы/экто из подарка
    //переменная создана, чтобы подарки не спаунились постоянно, так как, после спауна последнего бонуса  
    int _currentPresentNumber; //переменная для того, чтобы запомнить, какой подарок открываем. Нужна для метода SpawnEctoAndBonusFromPresentAndPresentHimself, как аргумент. 
    //Записывается когда открываем подарок
    public int GhostOpenedByChangeColorSkin = 0; //сколько призраков открыто бонусом изменения цвета. Входит в учёт формулы для открытия новая скина призрака

    public List<int> GhostThatWeSaw = new List<int>();

    public bool Millionare;

    public int StarsForCheats;


    void Start()
    {
        StarsForCheats = 80;
        _timer = 0.5f;
        for (int i = 0; i < 7; i++)
            ActiveBonuses.Add(new List<int>());
    }


    public void FirstGameSet() //первый запуск игры
    {
        GhostSkinIdOnField.Clear();
        _counterForGhostPosition = 10;
        GhostSpeed = 150f;
        SpawnZone = 350f;
        KeyBarReset = false;
        GhostBarReset = false;

        for (int i = 0; i < PositionState.Length; i++)
            PositionState[i] = false;
        KeyStorage = 0;
        KeyPrice = 20;
        //_notDiscountedKeyPrice = 10;
        KeyValue = 0;
        EctoLimit = 7;
        GhostAmount = 0;
        GhostScore = 0;
        HowMuchNeedGhosts = 1;

        EctoplasmOnFieldValue = 0;

        Image backImage = GameObject.Find("Back").GetComponent<Image>();
        Image fielldImage = GameObject.Find("Field").GetComponent<Image>();
        backImage.sprite = backSprites[0];
        fielldImage.sprite = fieldSprites[0];
        UpdateNextGhostValueText();
        UpdateGhostAmountText();
        UpdateKeyText();
        int maxEctoSpawnCoordArrayNumber = ectoplasmSpawnCoordinates.Length - 2;
        int randomSpawn = Random.Range(0, maxEctoSpawnCoordArrayNumber);
        while (ectoplasmSpawnCoordinates[randomSpawn].x == 0 || ectoplasmSpawnCoordinates[randomSpawn].y == 0)
            randomSpawn = Random.Range(0, maxEctoSpawnCoordArrayNumber);
        //AllEctoPositions.Add(ectoplasmSpawnCoordinates[randomEctoplasmSpawn]);
        PositionState[randomSpawn] = true; //записывает в массив, что позиция занята
        int ectoCoordSpawnNumber = randomSpawn; //запоминаем номер позиции, чтобы подарок не появился на том же месте
        Instantiate(parentEctoPrefab, ectoplasmSpawnCoordinates[randomSpawn], Quaternion.identity);
        EctoplasmOnFieldValue++;
        randomSpawn = Random.Range(0, maxEctoSpawnCoordArrayNumber);
        _presentNumber = 1;
        while (ectoplasmSpawnCoordinates[randomSpawn].x == 0 || ectoplasmSpawnCoordinates[randomSpawn].y == 0 || randomSpawn == ectoCoordSpawnNumber)
            randomSpawn = Random.Range(0, maxEctoSpawnCoordArrayNumber);
        PositionState[randomSpawn] = true;
        GameObject present = Instantiate(presentOnFieldPrefab, ectoplasmSpawnCoordinates[randomSpawn], Quaternion.identity) as GameObject;
        InstantiatePresent(present.transform.GetChild(0));
        _presentSpawned = true;
        LastPlayerPos = Vector3.zero;
    }


    void RandomColorForEcto(GameObject ecto) //доделать!!!!!!!!!!
    {
        int rndmColor = Random.Range(1, 10);
        if (rndmColor >= 7)
            rndmColor = Random.Range(1, 10);
        //ecto.GetComponent<Image>()
    }


    public void SlowGhosts() //дебафф с замедлением призраков
    {
        Animator levelUpAnimator = GameObject.Find("LevelUp").GetComponent<Animator>();
        levelUpAnimator.Play("SlowGhosts", -1, 0f);
        GhostSpeed -= 10;
        for (int i = 1; i <= GhostAmount - 1; i++)
        {
            Vector2 ghostVelocity = GameObject.Find("Ghost" + i).GetComponent<Rigidbody2D>().velocity;
            float scaler = GameObject.Find("GameUnits").GetComponent<RectTransform>().localScale.x;
            float ghostSpeedWithScale = GhostSpeed * scaler;
            if (ghostVelocity.x > 0)
                ghostVelocity = new Vector2(ghostSpeedWithScale, 0f);
            else
                ghostVelocity = new Vector2(-ghostSpeedWithScale, 0f);
            if (ghostVelocity.y > 0)
                ghostVelocity = new Vector2(0f, ghostSpeedWithScale);
            else
                ghostVelocity = new Vector2(0f, -ghostSpeedWithScale);
        }
    }


    public void MoreEcto() //метод для бонуса "больше экто на поле" 
    {
        Animator levelUpAnimator = GameObject.Find("LevelUp").GetComponent<Animator>();
        levelUpAnimator.Play("MoreEcto", -1, 0f);
        for (int i = 1; i <= GhostAmount - 1; i++)
        {
            PlacingEctoplasm placingEctoplasmClass = GameObject.Find("Ghost" + i).GetComponentInChildren<PlacingEctoplasm>();
            EctoLimit++;
            placingEctoplasmClass.EctosOnFieldLimit = EctoLimit;
        }

    }


    public void GhostReplace(GameObject gameObject) //перемещение призрака, когда он вышел за поле
    {
        RandomGhostSpawn = Random.Range(0, ghostSpawnCoordinates.Length);
        gameObject.transform.position = ghostSpawnCoordinates[RandomGhostSpawn];
    }


    public void CreateGhost() //создание призрака
    {
        if (dontCreate)
            return;

        //блок с рандомизацией позиции
        do
        {
            RandomGhostSpawn = Random.Range(0, ghostSpawnCoordinates.Length);//ставим рандомное число для позиции
            CheckingPosition();
        } while (ghostSpawnCoordinates[RandomGhostSpawn].x == EctoPositionForGhostSpawn.x || ghostSpawnCoordinates[RandomGhostSpawn].y == EctoPositionForGhostSpawn.y);
        SaveGhostSpawn = RandomGhostSpawn;//сохраняем последнюю позицию, чтобы не повторялась

        int ghostBought = GhostAnimationNumberBought.Count; //к-ство купленных призраков


        GhostAmount += 1;
        if (GhostAmount > Record)
            Record = GhostAmount;

        Image backImage = GameObject.Find("Back").GetComponent<Image>();
        Image fielldImage = GameObject.Find("Field").GetComponent<Image>();

        switch (GhostAmount) //cмена фона
        {
            case 5:
                backImage.sprite = backSprites[1];
                fielldImage.sprite = fieldSprites[1];

                _presentNumber++;
                if (_waitForSpawn) //на 5-м уровне должен появится подарок. 
                    //Если прямо за секунду до этого мы открыли предыдущий подарок и из него появляются бонусы,
                    //то оповещаем, что подарок не заспаунен
                    _presentSpawned = false;
                else
                    CheckingStatePositionAndSpawn(3);
                break;
            case 10:
                backImage.sprite = backSprites[2];
                fielldImage.sprite = fieldSprites[2];

                _presentNumber++;
                if (_waitForSpawn)
                    _presentSpawned = false;
                else
                    CheckingStatePositionAndSpawn(3);
                _presentSpawned = false;
                break;
            case 15:
                backImage.sprite = backSprites[3];
                fielldImage.sprite = fieldSprites[3];

                _presentNumber++;
                if (_waitForSpawn)
                    _presentSpawned = false;
                else
                    CheckingStatePositionAndSpawn(3);
                break;
            case 20:
                backImage.sprite = backSprites[4];
                fielldImage.sprite = fieldSprites[4];

                _presentNumber++;
                if (_waitForSpawn)
                    _presentSpawned = false;
                else
                    CheckingStatePositionAndSpawn(3);
                break;
            case 25:
                backImage.sprite = backSprites[5];
                fielldImage.sprite = fieldSprites[5];

                _presentNumber++;
                if (_waitForSpawn)
                    _presentSpawned = false;
                else
                    CheckingStatePositionAndSpawn(3);
                break;
            case 30:
                backImage.sprite = backSprites[6];
                fielldImage.sprite = fieldSprites[6];

                _presentNumber++;
                if (_waitForSpawn)
                    _presentSpawned = false;
                else
                    CheckingStatePositionAndSpawn(3);
                break;
            case 35:
                backImage.sprite = backSprites[7];
                fielldImage.sprite = fieldSprites[7];

                _presentNumber++;
                if (_waitForSpawn)
                    _presentSpawned = false;
                else
                    CheckingStatePositionAndSpawn(3);
                break;
            case 40:
                backImage.sprite = backSprites[8];
                fielldImage.sprite = fieldSprites[8];
                break;
        }

        //блок с рандомизацией скина
        int maxGhostClips = 71; //максимальное к-ство призраков
        int maxClosedGhosts = Record + ghostBought + GhostOpenedByChangeColorSkin;
        bool skillRandomFailed = true;
        while (skillRandomFailed)
        {
            //RandomizeSkin:
            do
                _randomSkin = Random.Range(0, maxClosedGhosts); //ставим рандомное число для спрайта
            while (_randomSkin >= maxGhostClips); //ошибка геймдизайнера. в итоге, по формуле в переменной maxClosedGhosts, 
            //к-ство скинов, которые можно открыть, станет больше 71. я решил, что, если рандомное число выпадет больше, чем 71, 
            //то рандомизируем пока не получится меньше ровно 71
            int ghostOnFieldValue = GhostSkinIdOnField.Count;
            if (ghostOnFieldValue == 0) //костыль. если не вставить его, то из цикла мы не выйдем в начале игры
                skillRandomFailed = false;

            for (int i = 0; i < ghostOnFieldValue; i++)
                //перебираем существующие скины на поле для того, чтобы спрайты не повторялись
                if (_randomSkin == GhostSkinIdOnField[i])
                //если скин отличается от тех, кто находится на данный момент на поле, то выходим из цикла

                //(старый вариант)если спрайт такой же, как и находящийся на поле, то рандомизируем новый
                {
                    //goto RandomizeSkin;
                    skillRandomFailed = true;
                    break;
                }
                else
                {
                    skillRandomFailed = false;
                }
        }
        //конец блока
        GhostSkinIdOnField.Add(_randomSkin); //добавляем номер скина в коллекцию тех, кто на поле
        GhostThatWeSaw.Add(_randomSkin);

        GameObject ghostClone = Instantiate(ghostPrefab, ghostSpawnCoordinates[RandomGhostSpawn], Quaternion.identity) as GameObject; //создаём новый объект
        ghostClone.name = "Ghost" + GhostAmount;
        UpdateGhostAmountText();
        ChangeGhostClipInRuntimeController(ghostClone, _randomSkin);
        RunGhostRun(ghostClone);

        PlacingEctoplasm placingEctoplasm = ghostClone.transform.GetChild(0).GetChild(0).GetComponent<PlacingEctoplasm>();
        placingEctoplasm.ghostSkinNumber = _randomSkin;
        placingEctoplasm.ghostNumber = GhostAmount - 1; //-1 потому что счёт с 0 начинается

        for (int i = 0; i < ghostBought; i++)
        {
            if (_randomSkin == GhostAnimationNumberBought[i])
                placingEctoplasm.scoreValue = 2;
        }
        placingEctoplasm.colorNumber = PlacingColor(_randomSkin);
        GameObject.Find("GhostIcon").GetComponent<Animator>().Play("GhostIconAnim", -1, 0f);
        if (maxClosedGhosts < maxGhostClips)
            GameObject.Find("CanAppear").GetComponent<Animator>().Play("CanAppear", -1, 0f);
        Instantiate(thunderPrefab);
    }


    public void CheckingPosition() //метод для того, чтобы призраки не выходили с одной и той же стороны. Лимит - 10 раз
    {
    CheckingPosition:
        if (SaveGhostSpawn == RandomGhostSpawn)
        {
            _counterForGhostPosition--;
            RandomGhostSpawn = Random.Range(0, ghostSpawnCoordinates.Length);
            if (_counterForGhostPosition == 0)
            {
                goto SetThePosition;
            }
            goto CheckingPosition;
        }
    SetThePosition:
        _counterForGhostPosition = 10;
        SaveGhostSpawn = RandomGhostSpawn;
    }


    public void ChangeGhostClipInRuntimeController(GameObject gameObject, int skinNumber) //создание индивидуального контроллера и смена анимации для призрака
    {
        RuntimeAnimatorController currentController =
            gameObject.transform.GetChild(0).GetChild(0).GetComponent<Animator>().runtimeAnimatorController;
        String currentAnimationName = currentController.animationClips[0].name;
        AnimatorOverrideController ghostOverrideController = new AnimatorOverrideController();
        ghostOverrideController.runtimeAnimatorController = currentController;
        ghostOverrideController[currentAnimationName] = ghostAnimationClips[skinNumber];
        gameObject.transform.GetChild(0).GetChild(0).GetComponent<Animator>().runtimeAnimatorController = ghostOverrideController;
    }


    void InstantiatePresent(Transform presentTransform)
    {
        presentTransform.GetComponent<PresentOnFieldScript>().presentNumber = _presentNumber;
        RuntimeAnimatorController currentController = presentTransform.GetComponent<Animator>().runtimeAnimatorController;
        String currentAnimationName1 = currentController.animationClips[0].name;
        String currentAnimationName2 = currentController.animationClips[1].name;
        AnimatorOverrideController presentOverrideController = new AnimatorOverrideController();
        presentOverrideController.runtimeAnimatorController = currentController;
        switch (_presentNumber)
        {
            case 2:
                presentOverrideController[currentAnimationName1] = presentAnimationClips2[0];
                presentOverrideController[currentAnimationName2] = presentAnimationClips2[1];
                //смена анимации подарка
                //положить в подарок номер подарка
                break;
            case 3:
                presentOverrideController[currentAnimationName1] = presentAnimationClips3[0];
                presentOverrideController[currentAnimationName2] = presentAnimationClips3[1];
                break;
            case 4:
                presentOverrideController[currentAnimationName1] = presentAnimationClips4[0];
                presentOverrideController[currentAnimationName2] = presentAnimationClips4[1];
                break;
            case 5:
                presentOverrideController[currentAnimationName1] = presentAnimationClips5[0];
                presentOverrideController[currentAnimationName2] = presentAnimationClips5[1];
                break;
            case 6:
                presentOverrideController[currentAnimationName1] = presentAnimationClips6[0];
                presentOverrideController[currentAnimationName2] = presentAnimationClips6[1];
                break;
            case 7:
                presentOverrideController[currentAnimationName1] = presentAnimationClips7[0];
                presentOverrideController[currentAnimationName2] = presentAnimationClips7[1];
                break;
        }
        presentTransform.GetComponent<Animator>().runtimeAnimatorController = presentOverrideController;
    }


    public int PlacingColor(int skinNumber) //метод для внесения цвета относительно номера скина
    {
        int colorNumber = 0;
        if (skinNumber == 0 || skinNumber == 13 || skinNumber == 21 || skinNumber == 32 || skinNumber == 36 || skinNumber == 47 || skinNumber == 54 || skinNumber == 66) //сверяем номера спрайтов и заносим соответсвующий цвет
            colorNumber = 0; //синий
        else
            if (skinNumber == 1 || skinNumber == 10 || skinNumber == 18 || skinNumber == 28 || skinNumber == 42 || skinNumber == 45 || skinNumber == 55 || skinNumber == 68)
                colorNumber = 2; //розовый
            else
                if (skinNumber == 2 || skinNumber == 12 || skinNumber == 25 || skinNumber == 33 || skinNumber == 41 || skinNumber == 49 || skinNumber == 61 || skinNumber == 71)
                    colorNumber = 7; //кориченвый
                else
                    if (skinNumber == 3 || skinNumber == 9 || skinNumber == 20 || skinNumber == 31 || skinNumber == 37 || skinNumber == 53 || skinNumber == 56 || skinNumber == 69)
                        colorNumber = 3; //зелёный
                    else
                        if (skinNumber == 4 || skinNumber == 16 || skinNumber == 23 || skinNumber == 35 || skinNumber == 38 || skinNumber == 50 || skinNumber == 58 || skinNumber == 67)
                            colorNumber = 5; //оранжевый
                        else
                            if (skinNumber == 5 || skinNumber == 11 || skinNumber == 24 || skinNumber == 30 || skinNumber == 40 || skinNumber == 48 || skinNumber == 59 || skinNumber == 65)
                                colorNumber = 1; //фиолетовый
                            else
                                if (skinNumber == 6 || skinNumber == 17 || skinNumber == 22 || skinNumber == 34 || skinNumber == 43 || skinNumber == 51 || skinNumber == 57 || skinNumber == 63)
                                    colorNumber = 6; //красный
                                else
                                    if (skinNumber == 7 || skinNumber == 15 || skinNumber == 26 || skinNumber == 27 || skinNumber == 44 || skinNumber == 52 || skinNumber == 60 || skinNumber == 70)
                                        colorNumber = 8; //чёрный
                                    else
                                        if (skinNumber == 8 || skinNumber == 14 || skinNumber == 19 || skinNumber == 29 || skinNumber == 39 || skinNumber == 46 || skinNumber == 62 || skinNumber == 64)
                                            colorNumber = 4; //жёлтый
        return colorNumber;
    }



    public void RunGhostRun(GameObject gameObject) //направление привидения в нужную сторону после создания или перемещения
    {
        RectTransform recTransform = gameObject.GetComponent<RectTransform>();
        float scaler = GameObject.Find("GameUnits").GetComponent<RectTransform>().localScale.x;
        float ghostSpeedWithScale = GhostSpeed * scaler;
        if (recTransform.anchoredPosition.x <= -SpawnZone)
        {
            gameObject.GetComponentInChildren<Animator>().SetBool("horizontal", true);
            gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.right * ghostSpeedWithScale;
            Vector3 coolnessPosition = new Vector3(-200f, ghostSpawnCoordinates[RandomGhostSpawn].y);
            GameObject coolnesObj = Instantiate(coolnessPrefab, coolnessPosition, Quaternion.identity) as GameObject;
            coolnesObj.GetComponent<RectTransform>().eulerAngles = new Vector3(
                0f,
                0f,
                180f
            );
        }
        else
        {
            if (recTransform.anchoredPosition.y <= -SpawnZone)
            {
                gameObject.GetComponentInChildren<Animator>().SetBool("horizontal", false);
                gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.up * ghostSpeedWithScale;
                Vector3 coolnessPosition = new Vector3(ghostSpawnCoordinates[RandomGhostSpawn].x, -200f);
                GameObject coolnesObj = Instantiate(coolnessPrefab, coolnessPosition, Quaternion.identity) as GameObject;
                coolnesObj.GetComponent<RectTransform>().eulerAngles = new Vector3(
                    0f,
                    0f,
                    270f
                );
            }
            else
            {
                if (recTransform.anchoredPosition.y >= SpawnZone)
                {
                    gameObject.GetComponentInChildren<Animator>().SetBool("horizontal", false);
                    gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.down * ghostSpeedWithScale;
                    Vector3 coolnessPosition = new Vector3(ghostSpawnCoordinates[RandomGhostSpawn].x, 200f);
                    GameObject coolnesObj = Instantiate(coolnessPrefab, coolnessPosition, Quaternion.identity) as GameObject;
                    coolnesObj.GetComponent<RectTransform>().eulerAngles = new Vector3(
                        0f,
                        0f,
                        90f
                    );
                }
                else
                {
                    if (recTransform.anchoredPosition.x >= SpawnZone)
                    {
                        gameObject.GetComponentInChildren<Animator>().SetBool("horizontal", true);
                        gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.left * ghostSpeedWithScale;
                        Vector3 coolnessPosition = new Vector3(200f, ghostSpawnCoordinates[RandomGhostSpawn].y);
                        Instantiate(coolnessPrefab, coolnessPosition, Quaternion.identity);
                    }
                }

            }
        }
    }


    public void GhostScoreReset() //обнуление счётчика эктоплазмы; +1 к экто для вызова приведения
    {
        GhostScore = 0;
        HowMuchNeedGhosts += 1;
    }



    void KeyScoreReset() //обнуление счётчика для получения ключа; +10 к получению следующего ключа
    {
        KeyPrice += 20;
        KeyStorage = 0;

        KeyValue += 1;
        UpdatePhotoAmountText();
    }


    IEnumerator KeyResetBar() //обнуление полоски для ключа
    {
        yield return new WaitForSeconds(0.5f);
        UpdateKeyText();
        FillKeyBar();
    }


    IEnumerator GhostResetBar()//обнуление полоски для призраков
    {
        yield return new WaitForSeconds(0.5f);
        UpdateNextGhostValueText();
        FillGhostBar();
    }


    public void AddToKeyStorage(int newScore) ///последовательность при добавлении очков к приобретению след. ключа
    {
        KeyStorage += newScore;
        if (KeyStorage > KeyPrice) //для больших экто
        {
            KeyStorage = KeyPrice;
            UpdateKeyText();
            FillKeyBar();
            KeyBarReset = true;
            KeyScoreReset();
            PhotocameraAnimationIn();
            KeyStorage = 1;
            StartCoroutine(KeyResetBar());
            return;
        }
        if (KeyStorage == KeyPrice)
        {
            UpdateKeyText();
            FillKeyBar();
            KeyBarReset = true;
            KeyScoreReset();
            PhotocameraAnimationIn();
            StartCoroutine(KeyResetBar());
            return;
        }
        UpdateKeyText();
        if (KeyBarReset)
            return;
        FillKeyBar();
    }


    void PhotocameraAnimationIn() //только, когда фото входит в фотоаппарат
    {
        Animator cameraAnimator = GameObject.Find("CameraIcon").GetComponent<Animator>();
        if (KeyValue == 1)
        {
            cameraAnimator.Play("FristPhotoInCam", -1, 0f);
        }
        else if (KeyValue >= 2)
        {
            cameraAnimator.Play("TwoOrMorePhotos", -1, 0f);
        }
    }


    public void AddToGhostScore(int newGhostScore) //последовательность при добавлении очков к вызову след. призрака
    {
        GhostScore += newGhostScore;
        if (GhostScore > HowMuchNeedGhosts)
        {
            GhostScore = HowMuchNeedGhosts;
            UpdateNextGhostValueText();
            FillGhostBar();
            CreateGhost();

            GhostBarReset = true;
            GhostScoreReset();
            GhostScore = 1;
            StartCoroutine(GhostResetBar());
            return;
        }
        if (GhostScore == HowMuchNeedGhosts)
        {
            UpdateNextGhostValueText();
            CreateGhost(); //обязательно перед заполнением полоски!
            FillGhostBar();

            GhostBarReset = true;
            GhostScoreReset();
            StartCoroutine(GhostResetBar());
            return;
        }

        UpdateNextGhostValueText();
        if (GhostBarReset)
            return;
        FillGhostBar();
    }


    public void FillKeyBar() //заполнение полоски ключа
    {
        KeyFillBar fillBar = GameObject.Find("KeyBar").GetComponent<KeyFillBar>();
        float endPos;
        if (KeyBarReset) //переменная для того, чтобы полоска не двигалась дальше, пока не прекратится анимация обнуления
        {
            endPos = 220f;
            fillBar.endPos = endPos;
            fillBar.doStep = true;
            return;
        }
        endPos = (-210f / KeyPrice) * (KeyPrice - KeyStorage); //формула, которая определяет конечную точку полоски
        fillBar.endPos = endPos;
        fillBar.doStep = true;
    }


    public void FillGhostBar() //заполнение полоски ключа
    {
        GhostFillBar fillBar = GameObject.Find("GhostBar").GetComponent<GhostFillBar>();
        float endPos;
        if (GhostBarReset)
        {
            endPos = 220f;
            fillBar.endPos = endPos;
            fillBar.doStep = true;
            return;
        }
        endPos = (-210f / HowMuchNeedGhosts) * (HowMuchNeedGhosts - GhostScore);
        fillBar.endPos = endPos;
        fillBar.doStep = true;
    }


    public void UpdateNextGhostValueText() //обновление текста с прогрессом вызова след. призрака
    {
        Text nextGhostHowMuchNeedText = GameObject.Find("Next Ghost HowNeed Text").GetComponent<Text>(); //текст для того, чтобы понимать сколько нам нужно очков до следующего призрака
        Text nextGhostValueText = GameObject.Find("Next Ghost Value Text").GetComponent<Text>(); ; //текст счёта очков до следующего призрака
        nextGhostValueText.text = "" + GhostScore;
        nextGhostHowMuchNeedText.text = "" + HowMuchNeedGhosts;
    }


    void UpdateGhostAmountText() //обновление текста с количеством призраков на поле
    {
        Text ghostValueText = GameObject.Find("Ghosts Value Text").GetComponent<Text>(); //текст для подсчёта призраков
        ghostValueText.text = "" + GhostAmount;
    }


    public void UpdateKeyText() //Обновление текста с ключом
    {
        Text keyValueText = GameObject.Find("Key Value Text").GetComponent<Text>(); ; //текст количества собранных экто
        Text keyHowMuchNeedText = GameObject.Find("Key HowNeed Text").GetComponent<Text>(); ; //текст для того, чтобы понимать сколько нам нужно очков до следующего ключа
        keyValueText.text = "" + KeyStorage;
        keyHowMuchNeedText.text = "" + KeyPrice;
    }


    public void UpdatePhotoAmountText() //Обновление текста с фото
    {
        Text photoAmounText = GameObject.Find("Photo Amount Text").GetComponent<Text>(); //текст для подсчёта фото
        photoAmounText.text = "" + KeyValue;
    }


    public void ResearchTheGhost() //открытие призрака
    {
        GameObject.Find("CanAppear").GetComponent<Animator>().Play("CanAppear", -1, 0f); //оповещение, что новых исследуемых призраков стало больше на 1
        GhostAnimationNumberBought.Add(GhostSpriteNumber);
        Animator cameraAnimator = GameObject.Find("CameraIcon").GetComponent<Animator>();
        if (KeyValue == 1)
        {
            cameraAnimator.Play("UsingLastPhoto", -1, 0f);
        }
        else if (KeyValue >= 2)
        {
            cameraAnimator.Play("UsingTwoOrMorePhotos", -1, 0f);
        }
        KeyValue -= 1;
        UpdatePhotoAmountText();
        Restart();
    }


    public void UnpackTheGift(int presentID) //метод, который запускается после поднятия подарка
    {
        //_presentNumber = 1;

        //switch (GhostAmount)
        //{
        //    case 5:
        //        _presentNumber = 2;
        //        break;
        //    case 10:
        //        _presentNumber = 3;
        //        break;
        //    case 15:
        //        _presentNumber = 4;
        //        break;
        //    case 20:
        //        _presentNumber = 5;
        //        break;
        //    case 25:
        //        _presentNumber = 6;
        //        break;
        //    case 30:
        //        _presentNumber = 7;
        //        break;
        //}
        _ectoCounterFromPresent = 0;
        _bonusCounterFromPresent = 0;
        _currentPresentNumber = presentID;
        SpawnEctoAndBonusFromPresentAndPresentHimself(presentID);
        _waitForSpawn = true;
    }


    void SpawnEctoAndBonusFromPresentAndPresentHimself(int presentNumberArg) //проверка на вызов экто или бонуса с подарка
    {

        if (_ectoCounterFromPresent != presentNumberArg) //спауним экто пока количество экто на поле не будет равняться номеру подарка
        {
            CheckingStatePositionAndSpawn(1);
            return;
        }
        int bonusCount = ActiveBonuses[presentNumberArg - 1].Count; //количество бонусов в подарке
        if (_bonusCounterFromPresent != bonusCount)
        {
            CheckingStatePositionAndSpawn(2);
            return;
        }
        if (!_presentSpawned)
            CheckingStatePositionAndSpawn(3);
    }


    /*
     * 1 - спаунится экто
     * 2 - спаунится бонус
     * 3 - спаунится подарок
     */
    void CheckingStatePositionAndSpawn(int option) //метод проверки позиции для спауна бонуса, экто из подарочка или сам подарок
    {
        int ectoSpawnValue = ectoplasmSpawnCoordinates.Length; //общее количество позиций на поле
        int randomPositiom = Random.Range(0, ectoSpawnValue); //рандомизатор позиций для бонуса/экто/подарка

        int positionStatesWithTrue = 0; //сколько клеток занято
        int positionStatesLength = PositionState.Length; //сколько 
        for (int i = 0; i < positionStatesLength; i++)
            if (PositionState[i])
                positionStatesWithTrue++;
        if (positionStatesWithTrue < 25) //если есть места на поле
        {
            //for (int j = 0; j < positionStatesLength; j++)
            //после того, как зарандомили позицию, её нужно проверить на то, чтобы она не была на одной линии с игроком по "x" и "y" и на одной и той же позиции с другой экто уже на поле
            //{
            while (ectoplasmSpawnCoordinates[randomPositiom].x == Mathf.Round(LastPlayerPos.x) ||
                ectoplasmSpawnCoordinates[randomPositiom].y == Mathf.Round(LastPlayerPos.y) ||
                PositionState[randomPositiom])//если позиция занята игроком, экто или бонусом, снова рандомизируем
            {
                randomPositiom = Random.Range(0, ectoSpawnValue);
                //j = 0;
            }
            //}
            PositionState[randomPositiom] = true;
            switch (option)
            {
                case 1://если нужно создать эктоплазму
                    Instantiate(parentEctoPrefab, ectoplasmSpawnCoordinates[randomPositiom], Quaternion.identity);
                    EctoplasmOnFieldValue++;
                    _ectoCounterFromPresent++;
                    break;
                case 2://если бонус
                    GameObject bonusClone = Instantiate(bonusPrefab, ectoplasmSpawnCoordinates[randomPositiom], Quaternion.identity) as GameObject;
                    int bonusID = ActiveBonuses[_currentPresentNumber - 1][_bonusCounterFromPresent]; //номер бонуса, который был помещён в подарок
                    bonusClone.GetComponent<BonusScript>().bonusID = bonusID; //помещаем номер бонуса в объект, чтобы потом узнать, какой эффект нужно сделать
                    bonusClone.GetComponent<Image>().sprite = bonusesOnField[bonusID];
                    _bonusCounterFromPresent++; //переключаемся на следующий бонус
                    break;
                case 3://если подарок
                    GameObject _present = Instantiate(presentOnFieldPrefab, ectoplasmSpawnCoordinates[randomPositiom], Quaternion.identity) as GameObject;
                    InstantiatePresent(_present.transform.GetChild(0));
                    if (_waitForSpawn) //
                        _presentSpawned = true;
                    break;
            }
        }
    }


    public void CarpetTutorial(int ectoScore) //стадии ковра-обучалки
    {
        if (_carpetPickedUpEcto <= 14)
        {
            _carpetPickedUpEcto += ectoScore;
            if (_carpetPickedUpEcto >= 1 && _carpetPickedUpEcto < 3 && _carpetStage == 0)
            {
                _carpetStage++;
                GameObject.Find("Carpet2").GetComponent<Animator>().Play("CollectOff", -1, 0f);
                GameObject.Find("Carpet3").GetComponent<Animator>().Play("AvoidOn", -1, 0f);
            }
            if (_carpetPickedUpEcto >= 3 && _carpetPickedUpEcto < 5 && _carpetStage == 1)
            {
                _carpetStage++;
                GameObject.Find("Carpet3").GetComponent<Animator>().Play("AvoidOff", -1, 0f);
                GameObject.Find("Carpet4").GetComponent<Animator>().Play("MoreMoreOn", -1, 0f);
            }
            if (_carpetPickedUpEcto >= 5 && _carpetPickedUpEcto < 7 && _carpetStage == 2)
            {
                _carpetStage++;
                GameObject.Find("Carpet4").GetComponent<Animator>().Play("MoreMoreOff", -1, 0f);
                GameObject.Find("Carpet5").GetComponent<Animator>().Play("CreateFilmOn", -1, 0f);
            }
            if (_carpetPickedUpEcto >= 7 && _carpetPickedUpEcto < 9 && _carpetStage == 3)
            {
                _carpetStage++;
                GameObject.Find("Carpet5").GetComponent<Animator>().Play("CreateFilmOff", -1, 0f);
                GameObject.Find("Carpet6").GetComponent<Animator>().Play("UseItOn", -1, 0f);
            }
            if (_carpetPickedUpEcto >= 9 && _carpetPickedUpEcto < 13 && _carpetStage == 4)
            {
                _carpetStage++;
                GameObject.Find("Carpet6").GetComponent<Animator>().Play("UseItOff", -1, 0f);
                GameObject.Find("Carpet7").GetComponent<Animator>().Play("SummonGhostsOn", -1, 0f);
            }
            if (_carpetPickedUpEcto >= 13 && _carpetStage == 5)
            {
                _carpetStage++;
                GameObject.Find("Carpet7").GetComponent<Animator>().Play("SummonGhostsOff", -1, 0f);
            }
        }
    }


    public void Restart() //действия при нажатии на кнопку "Restart" и обнуление всех переменных
    {
        Destroy(GameObject.FindGameObjectWithTag("Music"));
        for (int i = 1; i <= GhostAmount; i++)
        {
            Destroy(GameObject.Find("Ghost" + i));
        }

        GameObject[] _allEcto;
        _allEcto = GameObject.FindGameObjectsWithTag("ParentEcto");
        for (int i = 0; i < _allEcto.Length; i++)
        {
            Destroy(_allEcto[i]);
        }

        GameObject[] _allBonuses;
        _allBonuses = GameObject.FindGameObjectsWithTag("BonusOnField");
        foreach (GameObject bonus in _allBonuses)
        {
            Destroy(bonus);
        }

        GameObject[] _allPresents;
        _allPresents = GameObject.FindGameObjectsWithTag("PresentOnField");
        for (int i = 0; i < _allPresents.Length; i++)
        {
            Destroy(_allPresents[i]);
        }
        for (int i = 0; i < PositionState.Length; i++)
            PositionState[i] = false;

        LastPlayerPos = Vector3.zero;
        GameObject _ghostBarObj = GameObject.Find("GhostBar");
        Vector3 ghostBarPosition = _ghostBarObj.GetComponent<RectTransform>().anchoredPosition;
        _ghostBarObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(-210f, ghostBarPosition.y);
        Destroy(GameObject.FindGameObjectWithTag("GameOver"));

        BoardSetup boardSetup = GetComponent<BoardSetup>();
        boardSetup.LevelSetup();

        int randomEctoplasmSpawn = Random.Range(0, ectoplasmSpawnCoordinates.Length - 2);
        while (ectoplasmSpawnCoordinates[randomEctoplasmSpawn].x == 0 || ectoplasmSpawnCoordinates[randomEctoplasmSpawn].y == 0)
            randomEctoplasmSpawn = Random.Range(0, ectoplasmSpawnCoordinates.Length - 2);
        Instantiate(parentEctoPrefab, ectoplasmSpawnCoordinates[randomEctoplasmSpawn], Quaternion.identity);
        PositionState[randomEctoplasmSpawn] = true;
        GhostScore = 0;
        HowMuchNeedGhosts = 1;
        if (GhostAmount > GhostAmountForAlbum)
            GhostAmountForAlbum = GhostAmount;
        GhostAmount = 0;
        EctoplasmOnFieldValue = 1;
        _carpetPickedUpEcto = 0;
        FillKeyBar();
        dontCreate = false;
        UpdateNextGhostValueText();
        UpdateGhostAmountText();
        UpdateKeyText();
        _carpetStage = 0;
        GhostSpeed = 150;
        EctoLimit = 7;
        _presentNumber = 1;
        GhostSkinIdOnField.Clear();
        Image _backImage = GameObject.Find("Back").GetComponent<Image>();
        Image _fielldImage = GameObject.Find("Field").GetComponent<Image>();
        _backImage.sprite = backSprites[0];
        _fielldImage.sprite = fieldSprites[0];

        CheckingStatePositionAndSpawn(3);
    }


    public void GameOver(int newGhostSpriteNumber, int newGhostNumber) //анимация при столкновении с призраком
    {
        GameObject gameOverInGame = GameObject.FindWithTag("GameOver");
        if (gameOverInGame == null)//проверка, есть ли ещё объект GameOver
            Instantiate(gameOverPrefab);
        else
            return;
        dontCreate = true; //запрещает создавать новых призраков, если случайно подобрал экто
        GameObject[] ghostObjects = GameObject.FindGameObjectsWithTag("NPC");
        int ghostOnFieldAmount = ghostObjects.Length;
        for (int i = 0; i < ghostOnFieldAmount; i++)//остановка призрака, в которого врезался, и исчезновение всех остальных
        {
            ghostObjects[i].GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            if (newGhostNumber == i)
            {
                ghostObjects[i].GetComponentInChildren<Animator>().SetBool("horizontal", false);
                ghostObjects[i].transform.GetChild(0).GetChild(0).GetComponent<Animator>().speed = 0;
                continue;
            }
            ghostObjects[i].transform.GetChild(0).GetComponent<Animator>().Play("GhostDisappearing");
        }
        GameObject.Find("GhostsInRoomValue").GetComponent<Text>().text = "" + GhostAmount;
        GameObject.Find("RecordValue").GetComponent<Text>().text = "" + Record;
        GhostSpriteNumber = newGhostSpriteNumber; //спрайт, в который мы врезались
        GameObject ghostOnShowcase = GameObject.Find("Showcase").gameObject;
        ChangeGhostClipInRuntimeController(ghostOnShowcase, GhostSpriteNumber);
        //GameObject.Find("Showcase").transform.GetChild(0).GetChild(0).GetComponent<Animator>().runtimeAnimatorController = ghostControllers[GhostSpriteNumber]; //запись спрайта на обложку показа

        PlayerController playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();

        int starsСount = 0; //запись, сколько звёзд мы собрали
        if (GhostAmount >= 5 && GhostAmount < 10)
            starsСount = 1;
        if (GhostAmount >= 10 && GhostAmount < 15)
            starsСount = 2;
        if (GhostAmount >= 15 && GhostAmount < 20)
            starsСount = 3;
        if (GhostAmount >= 20 && GhostAmount < 25)
            starsСount = 4;
        if (GhostAmount >= 25)
            starsСount = 5;
        if (Stars[playerController.playerSkinNumber - 1] < starsСount) //см. PhotoScript ln 43
            Stars[playerController.playerSkinNumber - 1] = starsСount;


        int ghostBoughtAmount = GhostAnimationNumberBought.Count; //к-ство купленных привидений
        for (int i = 0; i < ghostBoughtAmount; i++) //Берём всё к-ство открытых призраков и делаем проверку: на купленное ли приведение мы наткнулись?
        {
            if (GhostSpriteNumber == GhostAnimationNumberBought[i]) //если номер привидения совпадает с купленным, то идёт анимация, что привидение уже приобретено
            {
                GameObject.Find("Touch Another Text").GetComponent<Animator>().SetBool("anotherGhost", true);
                return;
            }
        }
        if (KeyValue < 1) //если приведение не приобретено, то проверяем, хватит ли денег на покупку. Если не хватает, то обратно посылаем игрока на поле боя за диамантами
            GameObject.Find("Collect More Text").GetComponent<Animator>().SetBool("collectMoar", true);
        else //если хватает, то предлагаем купить
            GameObject.Find("Research Button").GetComponent<Animator>().SetBool("research", true);
    }


    void Update()
    {
        if (_waitForSpawn) //
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0)
            {
                SpawnEctoAndBonusFromPresentAndPresentHimself(_currentPresentNumber);
                _timer = 0.5f;
            }
            if (_ectoCounterFromPresent == _presentNumber && _bonusCounterFromPresent == ActiveBonuses[_presentNumber - 1].Count && _presentSpawned)
            {
                _waitForSpawn = false;
            }


        }


        if (Input.GetKeyDown(KeyCode.R)) //бессмертие
        {
            Restart();
        }


        if (Input.GetKeyDown(KeyCode.F)) //бессмертие
        {
            PlayerController playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
            if (!playerController.GodMode)
                playerController.GodMode = true;
            else
                playerController.GodMode = false;


        }

        if (Input.GetKeyDown(KeyCode.M)) //Millioniere
        {
            int ghostValue = 72;
            for (int i = 0; i < ghostValue; i++)
            {
                GhostAnimationNumberBought.Add(i);
                Stars[i] = 5;
            }
            Stars[72] = 5;
            Millionare = true;
        }


        if (Input.GetKeyDown(KeyCode.G)) //Ghost
        {
            CreateGhost();
        }


        if (Input.GetKey(KeyCode.Escape))
        {
            SaveLoad.Save();
            Application.Quit();
        }


        if (Input.GetKey(KeyCode.L))
            SaveLoad.Load();

        //if (Input.GetKeyDown(KeyCode.K)) //добавить 1 ключ
        //{
        //    AddToKeyStorage((int)(KeyPrice - KeyStorage - 1));
        //}
        //if (Input.GetKeyDown(KeyCode.E)) //заспаунить эктоплазму сверху игрока
        //{
        //    Vector3 spawnEcto = GameObject.FindGameObjectWithTag("Player").GetComponent<RectTransform>().anchoredPosition3D +
        //                        new Vector3(0f, 90f);
        //    Instantiate(parentEctoPrefab, spawnEcto, Quaternion.identity);
        //    EctoplasmOnFieldValue += 1;
        //}
        //if (Input.GetKeyDown(KeyCode.P)) //spawn gift
        //{
        //    Vector3 giftPosition = ectoplasmSpawnCoordinates[16];
        //    Vector3 ectoPosition =
        //        GameObject.FindGameObjectWithTag("ParentEcto").GetComponent<RectTransform>().anchoredPosition;
        //RandomGiftPosition:
        //    int randomGiftPosition = (int)Random.Range(1, 5);
        //    switch (randomGiftPosition)
        //    {
        //        case 1:
        //            giftPosition = ectoplasmSpawnCoordinates[15];
        //            break;
        //        case 2:
        //            giftPosition = ectoplasmSpawnCoordinates[17];
        //            break;
        //        case 3:
        //            giftPosition = ectoplasmSpawnCoordinates[6];
        //            break;
        //        case 4:
        //            giftPosition = ectoplasmSpawnCoordinates[8];
        //            break;
        //    }
        //    if (giftPosition == ectoPosition)
        //        goto RandomGiftPosition;
        //    Instantiate(presentOnFieldPrefab, giftPosition, Quaternion.identity);
        //}
    }
}
