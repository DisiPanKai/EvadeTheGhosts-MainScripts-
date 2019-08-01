using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    bool goUp, goDown, goLeft, goRight; //команда на движение в заданном направлении
    public bool gameOver; //переменная для того, чтобы игрок не двигался после того как умер
    public int playerSkinNumber; //переменная для скина. Раннее, отталкивалась от массива анимаций игрока, т.е., с 0 до 72. Теперь же, переменная отталкивается от имени файлов анимации
    //т.е., 1 - это дефолтный скин, 2 - это первый скин в альбоме, и так до 73
    public float playerSpeed; //скорость игрока
    float endPosX; //конечная позиция игрока при передвижении на 1 клетку для Х
    float endPosY; //конечная позиция игрока при передвижении на 1 клетку для Y
    float step; //расстояние, которое проходит игрок за 1 клетку
    float border; //граница игрового поля
    Vector2 touchOrigin; //для тачскрина
    public int playerColorNumber; //номер цвета для определения дружественных призраков
    Animator animator; //чтобы не создавать новый постоянно
    RectTransform rectTransform; //см. на строку выше
    public bool GodMode; //бессмертие для теста
    public bool ChangeSkins; //триггер на бонус со сменой скинов призраков вокруг
    private List<Collider2D> _ghostChangeSkinsColliders = new List<Collider2D>(); //коллайдеры, которые задел игрок во время взрыва феерверка
    public bool FireworkBoom; //триггер на бонус со сменой скина
    private float _colliderTimer; //время работы коллайдера на момент взрыва феерверка или смены скинов привидений
    public bool MenuIsOpen; //когда меню открыто, то управлять игроком нельзя


    void Start()
    {
        border = 200f;
        step = 90f;
        playerSpeed = 200f;
        GameObject gameUnits = GameObject.Find("GameUnits");
        transform.SetParent(gameUnits.transform, false);
        rectTransform = GetComponent<RectTransform>();
        touchOrigin = -Vector2.one;
        playerSkinNumber = 1;
        playerColorNumber = 9; //нейтральный цвет
        WearTheSkin();
        animator = transform.GetChild(0).GetComponent<Animator>();
    }


    void Update()
    {

        if (FireworkBoom) //
        {
            _colliderTimer -= Time.deltaTime;
            if (_colliderTimer <= 0f)
            {
                FireworkBoom = false;
                transform.GetChild(0).GetComponent<CircleCollider2D>().enabled = false;
            }
        }

        if (ChangeSkins)
        {
            _colliderTimer -= Time.deltaTime;
            if (_colliderTimer <= 0f)
            {
                ChangeSkins = false;
                transform.GetChild(0).GetComponent<CircleCollider2D>().enabled = false;
                if (_ghostChangeSkinsColliders.Count > 0)
                    GhostChangeSkin();
            }
        }

        if (animator
            .GetCurrentAnimatorStateInfo(0)
            .IsName("PlayerAnimation (1)_ 1") || animator
            .GetCurrentAnimatorStateInfo(0)
            .IsName("PlayerAnimation (1)_ 2") || animator
            .GetCurrentAnimatorStateInfo(0)
            .IsName("PlayerAnimation (1)_ 3") || animator
            .GetCurrentAnimatorStateInfo(0)
            .IsName("PlayerAnimation (1)_ 4")) //обнуление всех переменных в аниматоре, для того, чтобы анимация не зацикливалась
        {
            animator.SetBool("up", false);
            animator.SetBool("down", false);
            animator.SetBool("left", false);
            animator.SetBool("right", false);
        }
        if (goUp)
        {
            rectTransform.anchoredPosition = new Vector3(rectTransform.anchoredPosition.x, rectTransform.anchoredPosition.y + (playerSpeed * Time.deltaTime));
            //взял формулу передвижения с инета. до этого была другая формула, но она не подходила
            if (rectTransform.anchoredPosition.y >= endPosY) //когда дошли до конечной точки, то выравниваем позицию игрока. обычно, игрока сдвигается немного дальше
            {
                rectTransform.anchoredPosition = new Vector3(rectTransform.anchoredPosition.x, endPosY);
                goUp = false;
            }
            return;
        }

        if (goDown)
        {
            rectTransform.anchoredPosition = new Vector3(rectTransform.anchoredPosition.x, rectTransform.anchoredPosition.y - (playerSpeed * Time.deltaTime));
            if (rectTransform.anchoredPosition.y <= endPosY)
            {
                rectTransform.anchoredPosition = new Vector3(rectTransform.anchoredPosition.x, endPosY);
                goDown = false;
            }
            return;
        }

        if (goLeft)
        {
            rectTransform.anchoredPosition = new Vector3(rectTransform.anchoredPosition.x - (playerSpeed * Time.deltaTime), rectTransform.anchoredPosition.y);
            if (rectTransform.anchoredPosition.x <= endPosX)
            {
                rectTransform.anchoredPosition = new Vector3(endPosX, rectTransform.anchoredPosition.y);
                goLeft = false;
            }
            return;
        }

        if (goRight)
        {
            rectTransform.anchoredPosition = new Vector3(rectTransform.anchoredPosition.x + (playerSpeed * Time.deltaTime), rectTransform.anchoredPosition.y);
            if (rectTransform.anchoredPosition.x >= endPosX)
            {
                rectTransform.anchoredPosition = new Vector3(endPosX, rectTransform.anchoredPosition.y);
                goRight = false;
            }
            return;
        }

#if UNITY_STANDALONE || UNITY_WEBPLAYER
        GameController gameController =
                GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        if (Input.GetKey(KeyCode.W) && (rectTransform.anchoredPosition.y + step < border) && !gameOver && !MenuIsOpen)
        {
            transform.GetChild(0).GetComponent<Animator>().SetBool("up", true);//включение анимации
            goUp = true; //разрешение на взлёт вверх
            endPosY += step; //выставление конечной позиции
            gameController.LastPlayerPos = new Vector3(rectTransform.anchoredPosition.x, endPosY);
            return;
        }


        if (Input.GetKey(KeyCode.S) && (rectTransform.anchoredPosition.y - step > -border) && !gameOver && !MenuIsOpen)
        {
            transform.GetChild(0).GetComponent<Animator>().SetBool("down", true);
            goDown = true;
            endPosY -= step;
            gameController.LastPlayerPos = new Vector3(rectTransform.anchoredPosition.x, endPosY);
            return;
        }


        if (Input.GetKey(KeyCode.A) && (rectTransform.anchoredPosition.x - step > -border) && !gameOver && !MenuIsOpen)
        {
            transform.GetChild(0).GetComponent<Animator>().SetBool("left", true);
            goLeft = true;
            endPosX -= step;
            gameController.LastPlayerPos = new Vector3(endPosX, rectTransform.anchoredPosition.y);
            return;
        }


        if (Input.GetKey(KeyCode.D) && (rectTransform.anchoredPosition.x + step) < border && !gameOver && !MenuIsOpen)
        {
            transform.GetChild(0).GetComponent<Animator>().SetBool("right", true);
            goRight = true;
            endPosX += step;
            gameController.LastPlayerPos = new Vector3(endPosX, rectTransform.anchoredPosition.y);
        }

#elif UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
        if (Input.touchCount > 0)
        {
            Touch myTouch = Input.touches[0];
            if (myTouch.phase == TouchPhase.Began)
            {
                touchOrigin = myTouch.position;
            }
            else
                if (myTouch.phase == TouchPhase.Ended && touchOrigin.x >= 0)
                {
                    Vector2 touchEnd = myTouch.position;
                    float x = touchEnd.x - touchOrigin.x;
                    float y = touchEnd.y - touchOrigin.y;
                    touchOrigin.x = -1;
                    if (Mathf.Abs(x) < Mathf.Abs(y))
                    {
                        if (y > 0 && (rectTransform.anchoredPosition.y + step < border) && !gameOverPrefab)
                        {
                            goUp = true;
                            endPosX = rectTransform.anchoredPosition.y + step;
                            return;
                        }
                        if (y < 0 && (rectTransform.anchoredPosition.y - step > -border) && !gameOverPrefab)
                        {
                            goDown = true;
                            endPosX = rectTransform.anchoredPosition.y - step;
                            return;
                        }
                    }
                    else
                    {
                        if (x > 0 && (rectTransform.anchoredPosition.x + step < border) && !gameOverPrefab)
                        {
                            goRight = true;
                            endPosX = rectTransform.anchoredPosition.x + step;
                            return;
                        }
                        if (x < 0 && (rectTransform.anchoredPosition.x - step > -border) && !gameOverPrefab)
                        {
                            goLeft = true;
                            endPosX = rectTransform.anchoredPosition.x - step;
                        }
                    }
                }
        }
#endif
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        GameController gameController = GameObject.FindWithTag("GameController").GetComponent<GameController>();
        if (other.tag == "NPC" && !GodMode && !FireworkBoom && !ChangeSkins)//встречаем призрака 
        {
            int ghostColorNumber = other.transform.GetChild(0).GetChild(0).GetComponent<PlacingEctoplasm>().colorNumber;
            if (playerColorNumber == ghostColorNumber) //если того же цвета, то он испаряется
            {
                GhostPoof(other);
                int scoreValue = 1;
                gameController.AddToGhostScore(scoreValue);
                gameController.AddToKeyStorage(scoreValue);
                return;
            }
            ResetPlayerMovement(); //остановка игрока
            gameOver = true; //объявление для конца игры
            int ghostSpriteNumber = other.transform.GetChild(0).GetChild(0).GetComponent<PlacingEctoplasm>().ghostSkinNumber;
            int ghostNumber = other.transform.GetChild(0).GetChild(0).GetComponent<PlacingEctoplasm>().ghostNumber;
            gameController.GameOver(ghostSpriteNumber, ghostNumber);//передача номера спрайта и номера призрака для вывода в меню проигрыша
        }
        if (other.tag == "BonusOnField" && !FireworkBoom && !ChangeSkins) // "!" стоит, чтобы не уничтожались бонусы
        {
            int bonusID = other.GetComponent<BonusScript>().bonusID;
            switch (bonusID)
            {
                case 0: //ускорение игрока
                    SpeedUp();
                    break;
                case 1: //замедление всех призраков
                    gameController.SlowGhosts();
                    break;
                case 2: //макс к-ство экто, которое скидывается призраками на поле становится больше
                    gameController.MoreEcto();
                    break;
                case 3: //заменяет вокруг скины призраков
                    ChangeSkins = true;
                    _colliderTimer = 0.1f;
                    transform.GetChild(0).GetComponent<CircleCollider2D>().enabled = true;
                    //включает ещё один коллайдер побольше (пробовал с размеров одного коллайдера, была какая-то проблема)
                    break;
                case 4: //убирает всех призраков вокруг
                    FireworkBoom = true;
                    _colliderTimer = 0.1f;
                    transform.GetChild(0).GetComponent<CircleCollider2D>().enabled = true;
                    break;
            }
            Destroy(other.gameObject);
        }
        if (other.tag == "NPC" && FireworkBoom)
        {
            GhostPoof(other);
        }
        if (other.tag == "NPC" && ChangeSkins) //добавляем 
        {
            _ghostChangeSkinsColliders.Add(other);
        }
    }


    public void ResetPlayerMovement()//прекращение движения и обнуление конечных позиций игрока
    {
        goUp = false; //если же не нашего цвета, то идёт остановка движения
        goDown = false;
        goLeft = false;
        goRight = false;

        endPosX = 0;
        endPosY = 0;
    }


    public void GhostPoof(Collider2D otherObj)//метод вызывается, когда игрок наталкивается на призрака того же цвета, что и игрок
    {
        otherObj.GetComponent<BoxCollider2D>().enabled = false;
        otherObj.transform.GetChild(0).GetChild(0).GetComponent<CircleCollider2D>().enabled = false;
        otherObj.transform.GetChild(0).GetComponent<Animator>().Play("GhostDisappearing");
        otherObj.transform.GetComponent<GhostScript>().poof = true;
    }


    public void GhostChangeSkin() //вызывается при взрыве бонуса, который меняет скины призраков вокруг игрока
    {
        GameController gameController =
            GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        for (int i = 0; i < _ghostChangeSkinsColliders.Count; i++)
        {
            int ghostBought = gameController.GhostAnimationNumberBought.Count; //к-ство купленных призраков
            int unblockedGhostsValue = gameController.Record + ghostBought + gameController.GhostOpenedByChangeColorSkin;//общее количество всех призраков,
            //которые могут использоваться для спауна
            int ghostOnField = gameController.GhostAmount; //количество призраков на поле
            int maxGhostClips = 71; //максимальное доступное к-ство призраков

            if (ghostOnField == unblockedGhostsValue) //если количество призраков на поле и общее к-ство скинов, которые мы можем использовать для спауна
            //одинаково(то есть, диапазона для рандомных новых скинов нет), то мы расширяем диапазон скинов на 1, открываем его для игрока и выпускаем
            //на поле новый скин
            {
                PlacingEctoplasm ghostScript = _ghostChangeSkinsColliders[i].transform.GetChild(0)
                        .transform.GetChild(0)
                        .GetComponent<PlacingEctoplasm>();
                gameController.GhostSkinIdOnField.Remove(ghostScript.ghostSkinNumber);
                gameController.GhostOpenedByChangeColorSkin++; //теперь закрытых призраков для рандома скинов больше на 1
                int skinNumber = unblockedGhostsValue; //должно быть +1, что означает "открывает следующий закрытый скин", но из-за массивов +1 не нужен
                gameController.GhostSkinIdOnField.Add(skinNumber);
                gameController.GhostThatWeSaw.Add(skinNumber);
                gameController.ChangeGhostClipInRuntimeController(_ghostChangeSkinsColliders[i].gameObject,
                    skinNumber);
                ghostScript.colorNumber = gameController.PlacingColor(skinNumber);
                ghostScript.ghostSkinNumber = skinNumber;
                GameObject.Find("CanAppear").GetComponent<Animator>().Play("CanAppear", -1, 0f);
            }
            else //если же предыдущее условие не подходит, то выбираем рандомом скин из доступных игроку скинов
            {
                int randomSkin = 0;
                //int maxGhostClips = 71; //максимальное к-ство призраков
                PlacingEctoplasm ghostScript = _ghostChangeSkinsColliders[i].transform.GetChild(0)
                        .transform.GetChild(0)
                        .GetComponent<PlacingEctoplasm>();
                gameController.GhostSkinIdOnField.Remove(ghostScript.ghostSkinNumber);
                bool skillRandomFailed = true; //переменная чтобы выйти из цикла ниже
                while (skillRandomFailed) //рандом неповторяющегося скина для призрака
                {
                    //RandomizeSkin:
                    do
                        randomSkin = UnityEngine.Random.Range(0, unblockedGhostsValue);
                    //ставим рандомное число для спрайта
                    while (randomSkin >= maxGhostClips);
                    int ghostOnFieldValue = gameController.GhostSkinIdOnField.Count;
                    if (ghostOnFieldValue == 0) //костыль. если не вставить его, то из цикла мы не выйдем
                        skillRandomFailed = false;
                    for (int j = 0; j < ghostOnFieldValue; j++)
                        //перебираем существующие спрайты на поле для того, чтобы спрайты не повторялись
                        if (randomSkin == gameController.GhostSkinIdOnField[j])
                        //если спрайт такой же, как и находящийся на поле, то рандомизируем новый
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
                ghostScript.colorNumber = gameController.PlacingColor(randomSkin);
                gameController.ChangeGhostClipInRuntimeController(_ghostChangeSkinsColliders[i].gameObject,
                    randomSkin);
                ghostScript.ghostSkinNumber = randomSkin;
                gameController.GhostSkinIdOnField.Add(randomSkin); //добавляем номер спрайта в коллекцию тех, кто на поле

            }

            //if (unblockedGhostsValue < maxGhostClips) //если есть что открывать, то открываем следующий скин по счёту
            //{
            //    if (i == 0) //первый призрак, который встречается, всегда идёт, как вызов нового призрака на поле
            //    {
            //        PlacingEctoplasm ghostScript = _ghostChangeSkinsColliders[i].transform.GetChild(0)
            //            .transform.GetChild(0)
            //            .GetComponent<PlacingEctoplasm>();
            //        gameController.GhostSkinnNumberField.Remove(ghostScript.ghostSkinNumber);
            //        gameController.GhostOpenedByChangeColorSkin++;
            //        int skinNumber = (int)(unblockedGhostsValue + 1); //открывает следующий закрытый скин
            //        GameObject.Find("CanAppear").GetComponent<Animator>().Play("CanAppear", -1, 0f);
            //        gameController.ChangeGhostClipInRuntimeController(_ghostChangeSkinsColliders[i].gameObject,
            //            skinNumber);
            //        ghostScript.ghostSkinNumber = skinNumber;
            //    }
            //    else //если уже не первый эллемент, то заменяем скином, которого нет на поле
            //    {
            //        int ghostFieldValue = gameController.GhostSkinnNumberField.Count;
            //        if (ghostFieldValue < unblockedGhostsValue)
            //        {
            //            int randomSkin;
            //        //int maxGhostClips = 71; //максимальное к-ство призраков
            //        RandomizeSkin:
            //            do
            //                randomSkin = (int)UnityEngine.Random.Range(0, unblockedGhostsValue); //ставим рандомное число для спрайта
            //            while (randomSkin >= maxGhostClips);

            //            for (int j = 0; j < gameController.GhostSkinnNumberField.Count; j++) //перебираем существующие спрайты на поле для того, чтобы спрайты не повторялись
            //                if (randomSkin == gameController.GhostSkinnNumberField[j]) //если спрайт такой же, как и находящийся на поле, то рандомизируем новый
            //                {
            //                    goto RandomizeSkin;
            //                }

            //            gameController.GhostSkinnNumberField.Add(randomSkin); //добавляем номер спрайта в коллекцию тех, кто на поле
            //        }
            //    }
            //}
            //else //если неиспользованных разлоченых скинов призрака нету, то тасуем скины на поле местами для иллюзии действия
            //{
            //кусок, который не нужен. 
            //int ghostFieldValue = gameController.GhostSkinnNumberField.Count; 
            //if (ghostFieldValue < unblockedGhostsValue)
            //{
            //    int randomSkin;
            ////int maxGhostClips = 71; //максимальное к-ство призраков
            //RandomizeSkin:
            //    do
            //        randomSkin = (int)UnityEngine.Random.Range(0, unblockedGhostsValue); //ставим рандомное число для спрайта
            //    while (randomSkin >= maxGhostClips);

            //    for (int j = 0; j < gameController.GhostSkinnNumberField.Count; j++) //перебираем существующие спрайты на поле для того, чтобы спрайты не повторялись
            //        if (randomSkin == gameController.GhostSkinnNumberField[j]) //если спрайт такой же, как и находящийся на поле, то рандомизируем новый
            //        {
            //            goto RandomizeSkin;
            //        }

            //    gameController.GhostSkinnNumberField.Add(randomSkin); //добавляем номер спрайта в коллекцию тех, кто на поле
            //}
            //else
            //{

            //}
            //}
        }

        _ghostChangeSkinsColliders.Clear();
    }



    public void WearTheSkin() //надевание скина
    {
        RuntimeAnimatorController currentController =
            gameObject.transform.GetChild(0).GetComponent<Animator>().runtimeAnimatorController; //берём текущий контроллер
        String currentAnimationName1 = currentController.animationClips[1].name; //копируем имена состояний для того, чтобы поместить в них анимацию, которую нам нужно
        String currentAnimationName2 = currentController.animationClips[2].name;
        String currentAnimationName3 = currentController.animationClips[3].name;
        String currentAnimationName4 = currentController.animationClips[4].name;
        String currentAnimationName5 = currentController.animationClips[5].name;
        String currentAnimationName6 = currentController.animationClips[6].name;
        AnimatorOverrideController playerOverrideController = new AnimatorOverrideController(); //создаём перезаписывающий контроллер как копию текущего контроллера
        playerOverrideController.runtimeAnimatorController = currentController; //копируем имеющийся контроллер в перезаписываемый
        playerOverrideController[currentAnimationName1] =
            Resources.Load("Player Animation/PlayerAnimation (" + playerSkinNumber + ")_") as AnimationClip; //присваеваем анимацию в те состояния, которые нам нужно
        playerOverrideController[currentAnimationName2] =
            Resources.Load("Player Animation/PlayerAnimation (" + playerSkinNumber + ")_ 1") as AnimationClip;
        playerOverrideController[currentAnimationName3] =
            Resources.Load("Player Animation/PlayerAnimation (" + playerSkinNumber + ")_ 2") as AnimationClip;
        playerOverrideController[currentAnimationName4] =
            Resources.Load("Player Animation/PlayerAnimation (" + playerSkinNumber + ")_ 3") as AnimationClip;
        playerOverrideController[currentAnimationName5] =
            Resources.Load("Player Animation/PlayerAnimation (" + playerSkinNumber + ")_ 4") as AnimationClip;
        playerOverrideController[currentAnimationName6] =
            Resources.Load("Player Animation/PlayerAnimation (" + playerSkinNumber + ")_ 5") as AnimationClip;
        gameObject.transform.GetChild(0).GetComponent<Animator>().runtimeAnimatorController = playerOverrideController; //заносим копию в текущий контроллер
    }


    public void SpeedUp() //увеличение скорости игрока
    {
        playerSpeed += 20;
        Animator levelUpAnimator = GameObject.Find("LevelUp").GetComponent<Animator>();
        levelUpAnimator.Play("SpeedUp", -1, 0f);
    }
}
