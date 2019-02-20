using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public GameObject scoreText;                // 游戏分数展示
    public GameObject playButton;               // 开始游戏按钮
    public GameObject exitButton;               // 结束游戏按钮
    public GameObject platformPrefab;           // 平台预制体
    public GameObject playerPrefab;             // 玩家预制体

    private GameStatus status;                  // 游戏状态
    private int score;                          // 游戏分数
    private int reward;                         // 奖金，连续踩中平台中心会累计
    private float timer;                        // 计时器，用于计算矩阵变换
    private float power;                        // 跳跃力，用于计算玩家跳跃距离
    private float scale;                        // 按压平台的y轴缩放状态
    private bool isPress = false;               // 是否按压
    private bool isTurn = false;                // 是否转向

    private Vector3 playerObjectPosition;       // 玩家位置
    private Vector3 platformObjectHighPosition; // 平台高位置
    private Vector3 platformObjectLowPosition;  // 平台低位置
    private float playerSpeed = 0.5f;           // 玩家跳跃速度
    private float platformSpeed = 5f;           // 平台下落速度
    private GameObject playerObject;            // 玩家容器，用于位置变换
    private GameObject playerModel;             // 玩家模型，用于旋转变换
    private GameObject prevPlatformObject;      // 上一个平台
    private GameObject nextPlatformObject;      // 下一个平台
    private List<GameObject> platforms;         // 平台列表，用于管理生成的平台
    private List<Color32> colors;               // 颜色列表，用于给平台染色

    void Start()
    {
        status = GameStatus.Menu;
        platforms = new List<GameObject>();
        colors = new List<Color32>();
        colors.Add(Palette.Red);
        colors.Add(Palette.Pink);
        colors.Add(Palette.Purple);
        colors.Add(Palette.DeepPurple);
        colors.Add(Palette.Indigo);
        colors.Add(Palette.Blue);
        colors.Add(Palette.LightBlue);
        colors.Add(Palette.Cyan);
        colors.Add(Palette.Teal);
        colors.Add(Palette.Green);
        colors.Add(Palette.LightGreen);
        colors.Add(Palette.Lime);
        colors.Add(Palette.Yellow);
        colors.Add(Palette.Amber);
        colors.Add(Palette.Orange);
        colors.Add(Palette.DeepOrange);
        playButton.GetComponent<Button>().onClick.AddListener(OnGamePlayClick);
        exitButton.GetComponent<Button>().onClick.AddListener(OnGameExitClick);
    }

    void Update()
    {
        switch (status)
        {
            case GameStatus.Menu:
                // 游戏菜单
                GameMenu();
                break;
            case GameStatus.Init:
                // 游戏初始化
                GameInit();
                break;
            case GameStatus.Generate:
                // 生成平台
                PlatformGenerate();
                break;
            case GameStatus.Fall:
                // 平台坠落
                PlatformFall();
                break;
            case GameStatus.Fold:
                // 按压平台
                PlatformFold();
                break;
            case GameStatus.Free:
                // 释放平台
                PlatformFree();
                break;
            case GameStatus.Jump:
                // 玩家跳跃
                PlayerJump();
                break;
            case GameStatus.Over:
                // 游戏结束
                GameOver();
                break;
            default:
                break;
        }

    }

    private void OnGamePlayClick()
    {
        status = GameStatus.Init;
    }

    private void OnGameExitClick()
    {
        Application.Quit();
    }

    // 游戏菜单
    private void GameMenu()
    {
        // 展示游戏菜单
        scoreText.SetActive(false);
        playButton.SetActive(true);
        exitButton.SetActive(true);
    }

    // 游戏初始化
    private void GameInit()
    {
        // 隐藏游戏菜单
        scoreText.SetActive(true);
        playButton.SetActive(false);
        exitButton.SetActive(false);
        // 分数置0
        score = 0;
        AddScore(0);
        // 销毁所有平台
        foreach (GameObject current in platforms)
        {
            Destroy(current);
        }
        platforms.Clear();
        // 在初始位置重新生成平台
        nextPlatformObject = Instantiate(platformPrefab, new Vector3(0, 0.5f, 0), Quaternion.Euler(Vector3.zero));
        nextPlatformObject.GetComponent<MeshRenderer>().material.color = colors[Random.Range(0, 15)];
        platforms.Add(nextPlatformObject);
        // 销毁玩家
        if (playerObject)
        {
            Destroy(playerObject);
        }
        // 在初始位置重新生成玩家
        playerObject = Instantiate(playerPrefab, new Vector3(0, 1.25f, 0), Quaternion.Euler(Vector3.zero));
        playerModel = playerObject.transform.Find("Model").gameObject;
        // 摄像机归位
        GetComponent<CameraController>().BackToOrigin();
        // 进入生成平台阶段
        status = GameStatus.Generate;
    }

    // 生成平台
    private void PlatformGenerate()
    {
        // 记录上一个平台，以便设置下一个平台位置
        prevPlatformObject = nextPlatformObject;
        // 掷骰子判断下一个平台是否转向
        isTurn = (Random.Range(0, 10) > 5);
        // 在指定位置生成下一个平台
        nextPlatformObject = Instantiate(platformPrefab, prevPlatformObject.transform.position + NextStep(), Quaternion.Euler(Vector3.zero));
        nextPlatformObject.GetComponent<MeshRenderer>().material.color = colors[Random.Range(0, 15)];
        platforms.Insert(platforms.Count, nextPlatformObject);
        // 下一个平台会生成在空中，记录高位置和低位置以便下一个阶段矩阵变换
        platformObjectHighPosition = nextPlatformObject.transform.position;
        platformObjectLowPosition = platformObjectHighPosition;
        platformObjectLowPosition.y = 0.5f;
        // 摄像机移动面向两个平台中间
        GetComponent<CameraController>().MoveByOffset((prevPlatformObject.transform.position + platformObjectLowPosition) / 2);
        timer = 0;
        power = 0;
        // 进入平台坠落阶段
        status = GameStatus.Fall;
    }

    // 平台坠落
    private void PlatformFall()
    {
        timer += Time.deltaTime;
        // 平台坠落的矩阵变换，从高位置插值移动到低位置
        nextPlatformObject.transform.position = Vector3.Lerp(platformObjectHighPosition, platformObjectLowPosition, timer * platformSpeed);
        if (timer * platformSpeed > 1)
        {
            timer = 0;
            // 进入按压平台阶段
            status = GameStatus.Fold;
        }
    }

    // 按压平台
    private void PlatformFold()
    {
        // 按任意按钮进入按压状态
        if (Input.anyKey)
        {
            isPress = true;
        }
        if (isPress)
        {
            if (Input.anyKey)
            {
                timer += Time.deltaTime;
                if (timer < 4)
                {
                    // 蓄力
                    power = timer * 3;
                    // 按压平台的矩阵变换，平台y轴缩放减小、位置降低
                    prevPlatformObject.transform.localScale = new Vector3(1, 1 - 0.2f * timer, 1);
                    prevPlatformObject.transform.Translate(0, -0.1f * Time.deltaTime, 0);
                    // 玩家的矩阵变换，平台y轴位置降低
                    playerObject.transform.Translate(0, -0.2f * Time.deltaTime, 0);
                }
            }
            else
            {
                timer = 0;
                isPress = false;
                // 记录平台y轴缩放状态，用于恢复
                scale = prevPlatformObject.transform.localScale.y;
                // 进入释放平台阶段
                status = GameStatus.Free;
            }
        }
    }

    // 释放平台
    private void PlatformFree()
    {
		timer += Time.deltaTime;
        // 释放平台的矩阵变换，平台y轴缩放增大、位置升高
        prevPlatformObject.transform.localScale = new Vector3(1, scale + 1f * timer, 1);
        prevPlatformObject.transform.Translate(0, 0.5f * Time.deltaTime, 0);
        // 玩家的矩阵变换，平台y轴位置升高
        playerObject.transform.Translate(0, 1f * Time.deltaTime, 0);
        // 如果场景中平台多余5个，则销毁最早生成的
        if (platforms.Count > 5)
        {
            Destroy((GameObject)platforms[0]);
            platforms.RemoveAt(0);
        }
        if (prevPlatformObject.transform.position.y >= 0.5f)
        {
            timer = 0;
            // 设置玩家初始跳跃速度，记录玩家位置
            playerSpeed = 0.3f;
            playerObjectPosition = playerObject.transform.position;
            // 进入玩家跳跃阶段
            status = GameStatus.Jump;
        }
    }

    // 玩家跳跃
    private void PlayerJump()
    {
        playerSpeed -= Time.deltaTime;
        // 玩家跳跃时的矩阵变换
        if (isTurn)
        {
            playerObject.transform.Translate(new Vector3((nextPlatformObject.transform.position.x - playerObjectPosition.x) / 0.6f * Time.deltaTime, playerSpeed / 2, power / 0.6f * Time.deltaTime));
            playerModel.transform.Rotate(new Vector3(600 * Time.deltaTime, 0));
        }
        else
        {
            playerObject.transform.Translate(new Vector3(-power / 0.6f * Time.deltaTime, playerSpeed / 2, (nextPlatformObject.transform.position.z - playerObjectPosition.z) / 0.6f * Time.deltaTime));
            playerModel.transform.Rotate(new Vector3(0, 0, 600 * Time.deltaTime));
        }
        if (playerObject.transform.position.y <= 1)
        {
            playerObject.transform.position = new Vector3(playerObject.transform.position.x, 1.25f, playerObject.transform.position.z);
            playerModel.transform.rotation = Quaternion.Euler(0, 0, 0);
            // 玩家跳跃到上一个平台范围内
            if (Mathf.Abs(playerObject.transform.position.x - prevPlatformObject.transform.position.x) < 0.5 && Mathf.Abs(playerObject.transform.position.z - prevPlatformObject.transform.position.z) < 0.5)
            {
                // 进入按压平台阶段
                status = GameStatus.Fold;
            }
            else
            {
                // 玩家跳跃到下一个平台范围外
                if (Mathf.Abs(playerObject.transform.position.x - nextPlatformObject.transform.position.x) > 0.5 || Mathf.Abs(playerObject.transform.position.z - nextPlatformObject.transform.position.z) > 0.5)
                {
                    timer = 0f;
                    // 进入游戏结束阶段
                    status = GameStatus.Over;
                }
                // 玩家跳跃到下一个平台范围内
                else
                {
                    // 玩家跳跃到下一个平台正中心，累计奖金
                    if (Mathf.Abs(playerObject.transform.position.x - nextPlatformObject.transform.position.x) < 0.1 && Mathf.Abs(playerObject.transform.position.z - nextPlatformObject.transform.position.z) < 0.1)
                    {
                        reward++;
                        AddScore(reward * 2);
                    }
                    else
                    {
                        reward = 0;
                        AddScore(1);
                    }
                    // 进入生成平台阶段
                    status = GameStatus.Generate;
                }
            }
        }
    }

    // 游戏结束
    private void GameOver()
    {
        // 给玩家添加刚体，玩家从平台坠落的效果更真实
        if (System.Math.Abs(timer) == 0f)
        {
            playerModel.AddComponent<Rigidbody>();
        }
        timer += Time.deltaTime;
        if (timer >= 1)
        {
            timer = 0;
            // 进入游戏菜单阶段
            status = GameStatus.Menu;
        }
    }

    // 得分
    private void AddScore(int number = 0)
    {
        scoreText.GetComponent<TextMeshProUGUI>().text = "Score:" + (score += number);
    }

    // 获取下一个平台生成位置
    private Vector3 NextStep()
    {
        if (isTurn)
        {
            return new Vector3(0, 6, Random.Range(1.2f, 4));
        }
        return new Vector3(-Random.Range(1.2f, 4), 6, 0);
    }
}
