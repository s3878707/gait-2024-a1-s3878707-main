using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DrawGUI : MonoBehaviour
{
    public Sprite HeartSprite;
    public Sprite FlySprite;

    private int _iconSize = 20;
    private int _iconSeparation = 10;

    private Texture2D _heartTex;
    private Texture2D _flyTex;

    public Frog Frog;

    [SerializeField]
    private GameObject gameOverPanel;

    [SerializeField]
    private GameObject restartText;
    public TMPro.TextMeshProUGUI textDisplay;

    public bool isGameOver = false;

    void Start()
    {
        _heartTex = SpriteToTexture(HeartSprite);
        _flyTex = SpriteToTexture(FlySprite);
        gameOverPanel.SetActive(false);
        restartText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (Frog.health == 0)
        {   
            Time.timeScale = 0;
            isGameOver = true;
            textDisplay.text = "You died!";
            StartCoroutine(GameOverSequence());
        }
        if (Frog.fliesEaten == 10)
        {   
            Time.timeScale = 0;
            isGameOver = true;
            textDisplay.text = "You won!";
            StartCoroutine(GameOverSequence());
        }

        if (isGameOver)
        {
            //If R is hit, restart the current scene
            if (Input.GetKeyDown(KeyCode.R))
            {
                Time.timeScale = 1;
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }

    void OnGUI()
    {
        int maxFlies = 10;

        // Draw the background for the heath & flies overlay.
        GUI.Box(new Rect(10, 10, 30 * maxFlies + 10, 60), "");

        // At the moment, the GUI is hardcoded to display 3 health and 5 flies.
        // You will need to edit the below to display the correct information.
        for (int i = 0; i < Frog.health; i++)
        {
            GUI.DrawTexture(
                new Rect(20 + (_iconSize + _iconSeparation) * i, 20, _iconSize, _iconSize),
                _heartTex,
                ScaleMode.ScaleToFit,
                true,
                0.0f
            );
        }

        for (int i = 0; i < Frog.fliesEaten; i++)
        {
            GUI.DrawTexture(
                new Rect(20 + (_iconSize + _iconSeparation) * i, 45, _iconSize, _iconSize),
                _flyTex,
                ScaleMode.ScaleToFit,
                true,
                0.0f
            );
        }
    }

    // Helper function to convert sprites to textures.
    // Follows the code from http://answers.unity3d.com/questions/651984/convert-sprite-image-to-texture.html
    private Texture2D SpriteToTexture(Sprite sprite)
    {
        if (sprite.rect.width != sprite.texture.width)
        {
            Texture2D texture = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
            Color[] pixels = sprite.texture.GetPixels(
                (int)sprite.textureRect.x,
                (int)sprite.textureRect.y,
                (int)sprite.textureRect.width,
                (int)sprite.textureRect.height
            );
            texture.SetPixels(pixels);
            texture.Apply();

            return texture;
        }
        else
        {
            return sprite.texture;
        }
    }

    private IEnumerator GameOverSequence()
    {
        gameOverPanel.SetActive(true);
        yield return new WaitForSeconds(0f);
        restartText.SetActive(true);
    }
}
