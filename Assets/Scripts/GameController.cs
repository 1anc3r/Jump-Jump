using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public GameObject scoreText;
    public GameObject playButton;
    public GameObject exitButton;
    public GameObject platformPrefab;
    public GameObject playerPrefab;

    private GameStatus status;
    private int score;
    private int reward;
    private float timer;
    private float power;
    private float scale;
    private bool isPress = false;
    private bool isTurn = false;

    private Vector3 playerObjectPosition;
    private Vector3 platformObjectHighPosition;
    private Vector3 platformObjectLowPosition;
    private float playerSpeed = 0.5f;
    private float platformSpeed = 5f;
    private GameObject playerObject;
    private GameObject playerModel;
    private GameObject prevPlatformObject;
    private GameObject nextPlatformObject;
    private List<GameObject> platforms;
    private List<Color32> colors;

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
                GameMenu();
                break;
            case GameStatus.Init:
                GameInit();
                break;
            case GameStatus.Generate:
                PlatformGenerate();
                break;
            case GameStatus.Fall:
                PlatformFall();
                break;
            case GameStatus.Fold:
                PlatformFold();
                break;
            case GameStatus.Free:
                PlatformFree();
                break;
            case GameStatus.Bounce:
                PlayerBounce();
                break;
            case GameStatus.Over:
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

    private void GameMenu()
    {
        scoreText.SetActive(false);
        playButton.SetActive(true);
        exitButton.SetActive(true);
    }

    private void GameInit()
    {
        scoreText.SetActive(true);
        playButton.SetActive(false);
        exitButton.SetActive(false);
        score = 0;
        AddScore(0);
        foreach (GameObject current in platforms)
        {
            Destroy(current);
        }
        Destroy(playerObject);
        Vector3 position = new Vector3(0, 0.5f, 0);
        nextPlatformObject = Instantiate(platformPrefab, position, Quaternion.Euler(Vector3.zero));
        nextPlatformObject.GetComponent<MeshRenderer>().material.color = colors[Random.Range(0, 15)];
        platforms.Add(nextPlatformObject);
        playerObject = Instantiate(playerPrefab, new Vector3(0, 1.25f, 0), Quaternion.Euler(Vector3.zero));
        playerModel = playerObject.transform.Find("Model").gameObject;
        GetComponent<CameraController>().BackToOrigin();
        status = GameStatus.Generate;
    }

    private void PlatformGenerate()
    {
        prevPlatformObject = nextPlatformObject;
        isTurn = (Random.Range(0, 10) > 5);
        nextPlatformObject = Instantiate(platformPrefab, prevPlatformObject.transform.position + NextStep(), Quaternion.Euler(Vector3.zero));
        nextPlatformObject.GetComponent<MeshRenderer>().material.color = colors[Random.Range(0, 15)];
        platforms.Insert(platforms.Count, nextPlatformObject);
        platformObjectHighPosition = nextPlatformObject.transform.position;
        platformObjectLowPosition = platformObjectHighPosition;
        platformObjectLowPosition.y = 0.5f;
        GetComponent<CameraController>().MoveByOffset((prevPlatformObject.transform.position + platformObjectLowPosition) / 2);
        timer = 0;
        power = 0;
        status = GameStatus.Fall;
    }

    private void PlatformFall()
    {
        timer += Time.deltaTime;
        nextPlatformObject.transform.position = Vector3.Lerp(platformObjectHighPosition, platformObjectLowPosition, timer * platformSpeed);
        if (timer * platformSpeed > 1)
        {
            timer = 0;
            status = GameStatus.Fold;
        }
    }

    private void PlatformFold()
    {
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
                    power = timer * 3;
                    prevPlatformObject.transform.localScale = new Vector3(1, 1 - 0.2f * timer, 1);
                    prevPlatformObject.transform.Translate(0, -0.1f * Time.deltaTime, 0);
                    playerObject.transform.Translate(0, -0.2f * Time.deltaTime, 0);
                }
            }
            else
            {
                timer = 0;
                isPress = false;
                scale = prevPlatformObject.transform.localScale.y;
                status = GameStatus.Free;
            }
        }
    }

    private void PlatformFree()
    {
		timer += Time.deltaTime;
        prevPlatformObject.transform.localScale = new Vector3(1, scale + 1f * timer, 1);
        prevPlatformObject.transform.Translate(0, 0.5f * Time.deltaTime, 0);
        playerObject.transform.Translate(0, 1f * Time.deltaTime, 0);
        if (prevPlatformObject.transform.position.y >= 0.5f)
        {
            timer = 0;
            playerSpeed = 0.3f;
            playerObjectPosition = playerObject.transform.position;
            status = GameStatus.Bounce;
        }
        if (platforms.Count > 5)
        {
            Destroy((GameObject)platforms[0]);
            platforms.RemoveAt(0);
        }
    }

    private void PlayerBounce()
    {
        playerSpeed -= Time.deltaTime;
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
            if (Mathf.Abs(playerObject.transform.position.x - prevPlatformObject.transform.position.x) < 0.5 && Mathf.Abs(playerObject.transform.position.z - prevPlatformObject.transform.position.z) < 0.5)
            {
                status = GameStatus.Fold;
            }
            else
            {
                if (Mathf.Abs(playerObject.transform.position.x - nextPlatformObject.transform.position.x) > 0.5 || Mathf.Abs(playerObject.transform.position.z - nextPlatformObject.transform.position.z) > 0.5)
                {
                    timer = 0;
                    status = GameStatus.Over;
                }
                else
                {
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
                    status = GameStatus.Generate;
                }
            }
        }
    }

    private void GameOver()
    {
        if (System.Math.Abs(timer) < 0.01)
        {
            playerModel.AddComponent<Rigidbody>();
        }
        timer += Time.deltaTime;
        if (timer >= 1)
        {
            timer = 0;
            status = GameStatus.Menu;
        }
    }

    private void AddScore(int number = 0)
    {
        scoreText.GetComponent<TextMeshProUGUI>().text = "Score:" + (score += number);
    }

    private Vector3 NextStep()
    {
        if (isTurn)
        {
            return new Vector3(0, 6, Random.Range(1.2f, 4));
        }
        return new Vector3(-Random.Range(1.2f, 4), 6, 0);
    }
}
