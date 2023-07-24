using System.Data;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ExcelDataReader;
using System.IO;
using System.Collections.Generic;
using System;
using System.Collections;
using RenderHeads.Media.AVProVideo;
using InfiniteWheel;
using Vuplex.WebView;
public class MenuManager : MonoBehaviour
{
    // 菜单数据
    private DataTable menuData;
    // 按钮预制体
    public GameObject buttonPrefab,clickButtonPrefab,ItemButtonPrefab;
    // 按钮容器
    public Transform ThirdbuttonContainer,FourthlybuttonContainer,PublicShowButtonContainer,ItemContainer;
    // 省份按钮容器
    private Transform provinceTransform;
    // 城市按钮容器
    private Transform cityTransform;
    // 广东省按钮
    private Button guangDong_Button;
    // 省外按钮
    private Button outsideGuangdongProvince_Button;
    // 城市按钮字典
    private Dictionary<string, Button> cityButtons;
    // 当前显示的菜单项
    private MenuItem currentDisplayItem;
    // 当前显示的内容按钮
    private GameObject currentScenicSpotButton;
    //UI层级
    public GameObject[] UIElements;
    //UI标题
    public GameObject Title_UI;
    //全景视频播放
    public GameObject Player;
    //弹窗
    public GameObject propUpWindow;
    //当前类型
    private PlayType playType;
    //弹窗素材
    public GameObject[] PropElements;
    //用于展示Gltf的组件
    private GLTFUtilityScript m_GLTFUtilityScript;
    //三级菜单控制器
    // private InfiniteWheelController infiniteWheelController;
    private FScrollPage.FScrollPage infiniteWheelController;
    // 按钮 返回首页按钮 全景按钮 返回上一页按钮 详情了解按钮
    private Button returnButton,panoramicButton,BackButton,EnterButton;
    //图片浏览器
    private Image imagePlayer;
    //视频播放器
    private MediaPlayer mediaPlayer;
    //网页浏览器
    private CanvasWebViewPrefab propWebViewPrefab;
    //全景网页播放器
    private CanvasWebViewPrefab panoramicWebView;
    private MediaPlayer panoramicMediaPlayer;
    //弹窗父物体
    public GameObject propUp;
    public Button quitButton;
    private bool isFirstEnter;
    private Button PlayPauseButton,CloseButton;

    private Canvas canvas;
    private delegate void avproPlayerAction();
    enum PlayType
    {
        Image,
        Video,
        Gltf,
        Web,
        Null
    }
    void Awake()
    {
        m_GLTFUtilityScript =  GLTFUtilityScript.Instance;
        cityButtons = new Dictionary<string, Button>();
        provinceTransform = UIElements[0].transform;
        cityTransform =UIElements[1].transform.GetChild(0);
        returnButton = GameObject.Find("ReturnButton").GetComponent<Button>();
        panoramicButton = GameObject.Find("PanoramicButton").GetComponent<Button>();
        EnterButton = GameObject.Find("EnterButton").GetComponent<Button>();
        BackButton =GameObject.Find("BackButton").GetComponent<Button>();
        panoramicWebView =Player.GetComponentInChildren<CanvasWebViewPrefab>();
        panoramicMediaPlayer =Player.GetComponentInChildren<MediaPlayer>();
        imagePlayer =propUpWindow.GetComponentInChildren<Image>();
        propWebViewPrefab = propUpWindow.GetComponentInChildren<CanvasWebViewPrefab>();
        mediaPlayer = propUpWindow.GetComponentInChildren<MediaPlayer>();
        canvas = quitButton.transform.parent.GetComponent<Canvas>();
        for (int i = 0; i < provinceTransform.childCount; i++)
        {
            if (provinceTransform.GetChild(i).name.Contains("GuangDong"))
            {
                guangDong_Button = provinceTransform.GetChild(i).GetComponent<Button>();
                guangDong_Button.GetComponentInChildren<Text>().text = "0";
                guangDong_Button.onClick.AddListener(() => OnProvinceButtonClick(guangDong_Button.transform.GetChild(0).name));
            }
            else
            {
                outsideGuangdongProvince_Button = provinceTransform.GetChild(i).GetComponent<Button>();
                outsideGuangdongProvince_Button.onClick.AddListener(() =>OnCityButtonClick(outsideGuangdongProvince_Button.transform.GetChild(0).name,ItemContainer,true));
            }
        }
        for (int i = 0; i < cityTransform.childCount; i++)
        {
            Button button = cityTransform.GetChild(i).GetComponent<Button>();
            string cityName = button.GetComponentInChildren<Text>().name;
            button.GetComponentInChildren<Text>().text = "0";
            cityButtons.Add(cityName, button);
            button.gameObject.SetActive(false);
        }
        // infiniteWheelController = GameObject.Find("Menu").GetComponent<InfiniteWheelController>();
        infiniteWheelController = ThirdbuttonContainer.transform.parent.GetComponent<FScrollPage.FScrollPage>();
        EnterButton.onClick.AddListener(() => OnEntetButtonCilck());
        BackButton.onClick.AddListener(() => OnBackButtonClick());
    }

    void Start()
    {
        // 读取Excel文件并获取数据
        menuData = ReadExcelFile(Application.streamingAssetsPath + "/menuData.xlsx");
        ChangeElements(-1);
        canvas.gameObject.SetActive(true);
        // 获取省份的所有内容数量
        GetProvinceContentCounts();

    }

// 读取Excel文件并获取数据
    private DataTable ReadExcelFile(string filePath)
    {
        using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
        {
            IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
            var config = new ExcelDataSetConfiguration
            {
                ConfigureDataTable = _ => new ExcelDataTableConfiguration
                {
                    UseHeaderRow = true,
                }
            };
            DataSet result = excelReader.AsDataSet(config);
            DataTable dataTable = result.Tables["Test"];
            excelReader.Close();
            return dataTable;
        }
    }
    public void ChangeElements(int index)
    {
        if(index == -1)
        {
            foreach(var temp in UIElements)
            {
                temp.SetActive(false);
            }  
            index = 0;
        }
        propUp.SetActive(false);
        switch(index)
        {
            case 0:
            provinceTransform.gameObject.SetActive(true);
            cityTransform.gameObject.SetActive(false);
            PublicShowButtonContainer.transform.parent.parent.gameObject.SetActive(true);
            ThirdbuttonContainer.parent.parent.gameObject.SetActive(false);
            FourthlybuttonContainer.parent.gameObject.SetActive(false);
            infiniteWheelController.gameObject.SetActive(false);
            Title_UI.SetActive(true);
            panoramicMediaPlayer.gameObject.SetActive(false);
            panoramicWebView.gameObject.SetActive(false);
            quitButton.gameObject.SetActive(true);
            OnCityButtonClick("样品",PublicShowButtonContainer,false);
            break;
            case 1:
            OnCityButtonClick("广东样品",PublicShowButtonContainer,false);
            provinceTransform.gameObject.SetActive(false);
            cityTransform.gameObject.SetActive(true);
            PublicShowButtonContainer.transform.parent.parent.gameObject.SetActive(true);
            ThirdbuttonContainer.parent.gameObject.SetActive(false);
            FourthlybuttonContainer.parent.parent.gameObject.SetActive(false);
            infiniteWheelController.gameObject.SetActive(false);
            Title_UI.SetActive(true);
            panoramicMediaPlayer.gameObject.SetActive(false);
            panoramicWebView.gameObject.SetActive(false);
            quitButton.gameObject.SetActive(false);
            break;
            case 2:
            provinceTransform.gameObject.SetActive(false);
            cityTransform.gameObject.SetActive(false);
            PublicShowButtonContainer.transform.parent.parent.gameObject.SetActive(false);
            ThirdbuttonContainer.parent.parent.gameObject.SetActive(true);
            FourthlybuttonContainer.parent.parent.gameObject.SetActive(false);
            infiniteWheelController.gameObject.SetActive(true);
            Title_UI.SetActive(false);
            panoramicMediaPlayer.gameObject.SetActive(false);
            panoramicWebView.gameObject.SetActive(false);
            quitButton.gameObject.SetActive(false);
            break;
            case 3:
            provinceTransform.gameObject.SetActive(false);
            cityTransform.gameObject.SetActive(false);
            PublicShowButtonContainer.transform.parent.parent.gameObject.SetActive(false);
            ThirdbuttonContainer.parent.parent.gameObject.SetActive(false);
            infiniteWheelController.gameObject.SetActive(false);
            FourthlybuttonContainer.parent.parent.gameObject.SetActive(true);
            Title_UI.SetActive(false);
            quitButton.gameObject.SetActive(false);
            break;
        }
        for(int i = 0;i<UIElements.Length;i++)
            {
                UIElements[i].SetActive(false);
            }
        UIElements[index].SetActive(true);
        Debug.Log("当前场景+"+index);
    }
    private void GetProvinceContentCounts()
    {
        var provinceContentCounts = menuData.AsEnumerable()
            .GroupBy(row => row.Field<string>("Province"))
            .Select(group => new
            {
                Province = group.Key,
                RowCount = group.Count(),
                UniqueCities = group.Select(row => row.Field<string>("ScenicSpotName")).Distinct().Count()
            });

        foreach (var item in provinceContentCounts)
        {
            Debug.Log(item.Province + ": " + item.RowCount + " rows, " + item.UniqueCities + " unique cities");

            if (item.Province.Contains("广东"))
            {
                guangDong_Button.GetComponentInChildren<Text>().text = item.UniqueCities.ToString();
            }
            else
            {
                outsideGuangdongProvince_Button.GetComponentInChildren<Text>().text = item.UniqueCities.ToString();
            }
        }
    }
    // 省份按钮点击事件处理函数
    private void OnProvinceButtonClick(string province)
    {

        var cityContentCounts = menuData.AsEnumerable()
            .Where(row => row.Field<string>("Province") == province)
            .GroupBy(row => row.Field<string>("City"))
            .Select(group => new
            {
                City = group.Key,
                ContentCount = group.Count(),
                UniqueScenicSpot = group.Select(row => row.Field<string>("ScenicSpotName")).Distinct().Count()
            });
        ChangeElements(1);
        foreach (var item in cityContentCounts)
        {
            Debug.Log(item.City + ": " + item.ContentCount);
            if (cityButtons.ContainsKey(item.City))
            {
                Button button = cityButtons[item.City];
                button.GetComponentInChildren<Text>().text = item.UniqueScenicSpot.ToString();
                button.gameObject.SetActive(true);
                button.onClick.AddListener(() => OnCityButtonClick(item.City,ItemContainer,true));
                // button.onClick.AddListener(() => OnCityButtonClick(item.City,ThirdbuttonContainer));
            }
        }
    }

    public IEnumerable<CityContentCount> GetCityContentCounts(string city)
    {
        if (city != "样品")
        {
            var cityContentCounts = menuData.AsEnumerable()
                .Where(row => row.Field<string>("City") == city)
                .GroupBy(row => row.Field<string>("ScenicSpotName"))
                .Select(group => new CityContentCount
                {
                    ScenicSpotName = group.Key,
                    ContentCount = group.Count()
                });
            return cityContentCounts;
        }
        else
        {
            var cityContentCounts = menuData.AsEnumerable()
                .Where(row => row.Field<string>("City").Contains("样品"))
                .GroupBy(row => row.Field<string>("ScenicSpotName"))
                .Select(group => new CityContentCount
                {
                    ScenicSpotName = group.Key,
                    ContentCount = group.Count()
                });
            return cityContentCounts;
        }
    }




    // 城市按钮点击事件处理函数
    private void OnCityButtonClick(string city,Transform container,bool isThird)
    {


        if(container.name != PublicShowButtonContainer.name)
        {
            ChangeElements(2);
            Title_UI.SetActive(false);
            Debug.Log(container.name+"="+PublicShowButtonContainer);
        }
        int childCount = container.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Destroy(container.GetChild(i).gameObject);
        }
        // List<InfiniteWheelItem> items = new List<InfiniteWheelItem>();
        foreach (var item in GetCityContentCounts(city))
        {
            // Debug.Log(item.ScenicSpotName + "生成按钮 " + item.ContentCount);
            if(isThird)
            {
                // items.Add(AddInfinItemWheelItemButtons(item.ScenicSpotName,container));
                AddScenicSpotButtons(item.ScenicSpotName,container);
                returnButton.onClick.RemoveAllListeners();
                returnButton.onClick.AddListener(() => OnContentReturn(2));
            }else
            {
                AddScenicSpotButtons(item.ScenicSpotName,container);
            }
        }
        if (isThird)
        {
            infiniteWheelController.gameObject.SetActive(true);
            // infiniteWheelController.init(items.ToArray(),items.Count);
            // infiniteWheelController.items = items.ToArray();
            infiniteWheelController.Init();
            MouseSimulater.LeftClick(1000, 1000);
            PublicShowButtonContainer.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, PublicShowButtonContainer.GetComponent<RectTransform>().anchoredPosition.y);
        }
        else
        {
            StartCoroutine(CalculateContentWidth(container.GetComponent<RectTransform>(), 50));
        }
        if(city == "省外")
        {
            isFirstEnter = true;
        }else
        {
            isFirstEnter = false;
        }
    }
    ///三级菜单按钮生成
    private InfiniteWheelItem AddInfinItemWheelItemButtons(string scenicSpot,Transform container)
    {
        var scenicSpotCount = menuData.AsEnumerable()
                  .FirstOrDefault(row => row.Field<string>("ScenicSpotName") == scenicSpot);
        ContentItem contentItem = new ContentItem(
            scenicSpotCount["ScenicSpotName"].ToString(),
            scenicSpotCount["CoverImage"].ToString(),
            scenicSpotCount["CoverText"].ToString(),
            scenicSpotCount["TriggerEvent"].ToString(),
            scenicSpotCount["DisplayImage"].ToString(),
            scenicSpotCount["ShowImage"].ToString(),
            scenicSpotCount["ShowElement"].ToString()
        );
        GameObject infiniteWheelItem_Obj = Instantiate(ItemButtonPrefab);
        infiniteWheelItem_Obj.transform.SetParent(container);
        var SSData = infiniteWheelItem_Obj.AddComponent<ScenicSpotData>();
        SSData.SetScenicSpotData(LoadSpriteFromPath(Application.streamingAssetsPath+"/内容/DisplayImage/"+contentItem.DisplayImage),LoadSpriteFromPath(Application.streamingAssetsPath+"/内容/CoverImage/"+contentItem.CoverImage));
        infiniteWheelItem_Obj.name = contentItem.ScenicSpotName;
        var infiniteWheelItem = infiniteWheelItem_Obj.GetComponent<InfiniteWheelItem>();
        return infiniteWheelItem;
    }
    //三级按钮生成
    private void AddScenicSpotButtons(string scenicSpot,Transform container)
    {
        var scenicSpotCount = menuData.AsEnumerable()
                  .FirstOrDefault(row => row.Field<string>("ScenicSpotName") == scenicSpot);
        ContentItem contentItem = new ContentItem(
            scenicSpotCount["ScenicSpotName"].ToString(),
            scenicSpotCount["CoverImage"].ToString(),
            scenicSpotCount["CoverText"].ToString(),
            scenicSpotCount["TriggerEvent"].ToString(),
            scenicSpotCount["DisplayImage"].ToString(),
            scenicSpotCount["ShowImage"].ToString(),
            scenicSpotCount["ShowElement"].ToString()
        );
        GameObject buttonObj = Instantiate(buttonPrefab);
        buttonObj.transform.SetParent(container);
        var SSData = buttonObj.AddComponent<ScenicSpotData>();
        SSData.SetScenicSpotData(LoadSpriteFromPath(Application.streamingAssetsPath+"/内容/DisplayImage/"+contentItem.DisplayImage),LoadSpriteFromPath(Application.streamingAssetsPath+"/内容/CoverImage/"+contentItem.CoverImage));
        Button button = buttonObj.GetComponent<Button>();
        button.GetComponentInChildren<RawImage>().texture = SSData.m_CoverImage.texture;
        button.transform.localScale = Vector3.one;
        if(container.name != PublicShowButtonContainer.name)
        {
            // button.onClick.AddListener(() => OnScenicSpotClick(contentItem));
            returnButton.onClick.RemoveAllListeners();
            returnButton.onClick.AddListener(() => OnContentReturn(2));
        }else
        {
            button.onClick.AddListener(() =>OnPublicButtonClick(contentItem.ScenicSpotName));
        }
    }
    // 创建按钮
    private IEnumerator CreateButton(Transform parent, string text, UnityEngine.Events.UnityAction onClick,int dir)
    {
        GameObject buttonObj = Instantiate(clickButtonPrefab);
        buttonObj.transform.SetParent(parent);
        // 设置生成位置和大小等属性以确保生成的按钮位于内容按钮的右下角,并且不会超出内容按钮的范围.
        RectTransform m_Button = buttonObj.GetComponent<RectTransform>();
        m_Button.anchorMax = new Vector2(0.5f,0.5f);
        m_Button.anchorMin = new Vector2(0.5f,0.5f);
        if(dir == 0)
        {
            m_Button.localPosition = new Vector2(m_Button.rect.width,-120f);
        }
        else
        {
            m_Button.localPosition = new Vector2(-m_Button.rect.width,-120f);
        }
        buttonObj.GetComponentInChildren<Text>().text = text;
        buttonObj.GetComponent<Button>().onClick.AddListener(onClick);
        return null;
    }
    private void switchPlayer(PlayType playType)
    {
        propUp.SetActive(true);
        for(int i = 0 ; i<PropElements.Length;i++)
        {
            if(i == (int)playType)
            {
                PropElements[i].SetActive(true);
            }else
            {
                PropElements[i].SetActive(false);
            }
        }
    }
    private void OnPublicButtonClick(string scenicSpotName)
    {
        nextToConnectButton(scenicSpotName,3);
        returnButton.onClick.RemoveAllListeners();
        returnButton.onClick.AddListener(() => OnContentReturn(0));
    }
    private void nextToConnectButton(string scenicSpotName,int returnLevel)
    {
        ChangeElements(returnLevel);
        var scenicSpotCount = menuData.AsEnumerable()
            .Where(row => row.Field<string>("ScenicSpotName") == scenicSpotName);
        foreach(Transform child in FourthlybuttonContainer.transform)
         {
             if(child.name != "TextPrefab")
             {
                 Destroy(child.gameObject);
             }
         }
        foreach(var scenicSpot in scenicSpotCount)
        {
            ContentItem contentItem = new ContentItem(
                scenicSpot["ScenicSpotName"].ToString(),
                scenicSpot["CoverImage"].ToString(),
                scenicSpot["CoverText"].ToString(),
                scenicSpot["TriggerEvent"].ToString(),
                scenicSpot["DisplayImage"].ToString(),
                scenicSpot["ShowImage"].ToString(),
                scenicSpot["ShowElement"].ToString()
            );
            AddContentButtons(contentItem);
        }
        string path = string.Format("/内容/DefaultCover/{0}{1}",scenicSpotName,".png");
        Player.GetComponent<RawImage>().texture = LoadSpriteFromPath(Application.streamingAssetsPath + path).texture;
        StartCoroutine(CalculateContentWidth(FourthlybuttonContainer.GetComponent<RectTransform>(),50)); 
        // infiniteScrollView.EnableScrolling();
    }
    // 添加内容按钮到菜单中
    private void AddContentButtons(ContentItem contentItem)
    {
        if(contentItem.TriggerEvent != "Panoramic" && contentItem.TriggerEvent != "Fixed")
        {
            GameObject buttonObj = Instantiate(buttonPrefab);
            buttonObj.transform.SetParent(FourthlybuttonContainer);
            Button button = buttonObj.GetComponent<Button>();
            string path = string.Format("/内容/ShowImage/{0}/{1}",contentItem.ScenicSpotName,contentItem.ShowImage);
            button.GetComponentInChildren<RawImage>().texture = LoadSpriteFromPath(Application.streamingAssetsPath+path).texture;
            button.onClick.AddListener(() => OnContentButtonClick(contentItem,propUpWindow));
            button.transform.localScale = Vector3.one;
        }
        else if(contentItem.TriggerEvent == "Panoramic" )
        {
            panoramicButton.onClick.RemoveAllListeners();
            panoramicButton.onClick.AddListener( () => OnPanoramicButtonClick(contentItem));
            Debug.Log("全景按钮绑定"+contentItem.ShowElement);
        }
    }
    private void OnContentButtonClick(ContentItem contentItem, GameObject player)
    {
        string extension = Path.GetExtension(contentItem.ShowElement).ToLower();
        PlayType playType = GetPlayType(extension);
        // 停止当前的播放
        StopPanoramic();
        StopCurrentPlayback();
        // 切换播放器和显示内容
        switchProp(playType, contentItem);
    }

    private PlayType GetPlayType(string extension)
    {
        switch (extension)
        {
            case ".jpg":
            case ".png":
            case ".bmp":
                return PlayType.Image;
            case ".mp4":
            case ".avi":
            case ".mov":
                return PlayType.Video;
            case ".gltf":
            case ".glb":
                return PlayType.Gltf;
            default:
                return PlayType.Web;
        }
    }

    private void switchProp(PlayType playType, ContentItem contentItem)
    {
        propUp.SetActive(true);

        // 遍历所有的UI元素
        for (int i = 0; i < PropElements.Length; i++)
        {
            // 判断当前UI元素是否对应播放类型
            bool isActive = i == (int)playType;

            // 如果是当前播放类型,显示该UI元素;否则隐藏
            PropElements[i].SetActive(isActive);
        }
        StopPanoramic();
        // 停止Web加载
        StopWebPage(propWebViewPrefab);
        string path = string.Format("/内容/ShowElement/{0}/{1}",contentItem.ScenicSpotName,contentItem.ShowElement);
        // 根据播放类型进行相应的处理
        switch (playType)
        {
            case PlayType.Image:
                StartLoadImage(Application.streamingAssetsPath +path);
                break;
            case PlayType.Video:
                mediaPlayer.OpenMedia(MediaPathType.AbsolutePathOrURL, Application.streamingAssetsPath + path, false);
                mediaPlayer.Loop = true;
                mediaPlayer.Play();
                break;
            case PlayType.Gltf:
                Debug.Log("Display gltf: " + contentItem.ShowElement);
                transform.GetComponent<GLTFModelRenderer>().LoadAndRenderModel(Application.streamingAssetsPath + path);
                break;
            case PlayType.Web:
                OpenWebPage(contentItem.ShowElement,propWebViewPrefab);
                break;
        }
    }

    public void StopCurrentPlayback()
    {
        // 停止图片、视频、Web等的播放
        // 根据具体的播放器类型和逻辑进行停止操作
        // 例如停止视频播放:mediaPlayer.Stop();
        // 停止Web加载:StopWebPage();
        imagePlayer.sprite = null;
        mediaPlayer.Stop();
        mediaPlayer.GetComponentInChildren<DisplayUGUI>().material.mainTexture = null;
        mediaPlayer.CloseMedia();
        transform.GetComponent<GLTFModelRenderer>().HideModel();
        StopWebPage(propWebViewPrefab);
    }
    private void StartLoadImage(string path)
    {
        Image m_Image = propUpWindow.GetComponentInChildren<Image>();
        m_Image.sprite = LoadSpriteFromPath(path);
        m_Image.transform.GetComponent<Zoom>().SetTouch(true);
    }
    private void StopLoadImage()
    {
        Image m_Image = propUpWindow.GetComponentInChildren<Image>();
        m_Image.sprite = null;
        m_Image.transform.GetComponent<Zoom>().SetTouch(false);
    }

    private void OpenWebPage(string url,CanvasWebViewPrefab browser)
    {
        // 在此处处理打开Web页面的逻辑
        // 例如使用浏览器打开URL:Application.OpenURL(url);
        // 或者使用WebView组件打开URL:browser.WebView.LoadUrl(url);
        browser.gameObject.SetActive(true);
        browser.WebView.LoadUrl(url);
    }

    private void StopWebPage(CanvasWebViewPrefab browser)
    {
        // 停止Web加载的逻辑
        // 请根据具体的WebView组件或插件提供的API进行停止操作
        // 例如:browser.WebView.StopLoad();
        if(browser != null)
        { 
            if(browser.isActiveAndEnabled)
            {
                browser.WebView.Reload();
                browser.gameObject.SetActive(false);
            }
        }
    }

    private void StopPanoramic()
    {
        StopWebPage(panoramicWebView);
        if(panoramicMediaPlayer.isActiveAndEnabled)
        {
            panoramicMediaPlayer.Stop();
            panoramicMediaPlayer.GetComponentInChildren<DisplayUGUI>().material.mainTexture = null;
            panoramicMediaPlayer.CloseMedia();
            panoramicMediaPlayer.gameObject.SetActive(false);
        }
    }

    // 返回按钮点击事件处理函数
    public void OnBackButtonClick()
    {
        if(isFirstEnter)
        {
            ChangeElements(0);
        }else
        {
            ChangeElements(1);
        }
        for(int i = 0;i<infiniteWheelController.transform.childCount;i++)
        {
            Destroy(infiniteWheelController.transform.GetChild(i).gameObject);
        }
        // infiniteWheelController.items = null;
        infiniteWheelController.gameObject.SetActive(false);
    }
    // 加载图片
    private Sprite LoadSpriteFromPath(string path)
    {
        // Debug.Log(path);
        // 检查文件是否存在
        if (File.Exists(path))
        {
            // 从文件读取字节数据
            byte[] imageData = File.ReadAllBytes(path);

            // 创建新的Texture2D
            Texture2D texture = new Texture2D(2048, 2048,TextureFormat.RGBA32,false);
            // 加载图片数据到Texture2D
            if (texture.LoadImage(imageData))
            {
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
                return sprite;
            }
            else
            {
                Debug.LogError("无法加载图片数据到Texture2D.");
                return null;
            }
        }
        else
        {
            Debug.LogError("指定的图片路径不存在: " + path);
            return null;
        }
    }

    // 重置菜单项
    private void ResetMenuItem(MenuItem menuItem)
    {
    }
    ///
    private IEnumerator CalculateContentWidth(RectTransform contentTransform, int spacing)
    {
        yield return new WaitForSeconds(0f);
        float totalWidth = 0f;
        for (int i = 0; i < contentTransform.childCount; i++)
        {
            RectTransform child = contentTransform.GetChild(i) as RectTransform;
            totalWidth += child.rect.width;
            if (i < contentTransform.childCount - 1)
            {
                totalWidth += spacing;
            }
        }
        Debug.Log("totalWidth = "+ totalWidth+"||||||contentChild ="+contentTransform.childCount);
        float contentWidth = totalWidth+50f;
        contentTransform.sizeDelta = new Vector2(contentWidth, contentTransform.sizeDelta.y);
        
        // Reset content position to the left
        contentTransform.anchoredPosition = new Vector2(0f, contentTransform.anchoredPosition.y);
        var CenteredScrollView = contentTransform.GetComponent<CenteredScrollView>();
        if(CenteredScrollView)
        {
            CenteredScrollView.InitScrollView();
            Debug.Log("CenteredScrollView is Ready for init");
        }
    }





    private void OnContentReturn(int returnLevel)
    {
        if(returnLevel == 2)
        {
            MouseSimulater.LeftClick(1000, 1000);
        }
        StopCurrentPlayback();
        StopPanoramic();
        ChangeElements(returnLevel);
    }
    private void OnPanoramicButtonClick(ContentItem contentItem)
    {
        string extension = Path.GetExtension(contentItem.ShowElement).ToLower();
        switch(extension)
        {
            case ".mp4":
            case ".avi":
            case ".mov":
            string path = string.Format("/内容/ShowElement/{0}/{1}",contentItem.ScenicSpotName,contentItem.ShowElement);
            panoramicMediaPlayer.gameObject.SetActive(true);
            panoramicMediaPlayer.OpenMedia(MediaPathType.AbsolutePathOrURL, Application.streamingAssetsPath + path, false);
            panoramicMediaPlayer.Loop = true;
            panoramicMediaPlayer.Play();
            break;
            default:
            Debug.Log("LoadUrl"+contentItem.ShowElement);
            panoramicWebView.gameObject.SetActive(true);
            if(panoramicWebView.gameObject.activeInHierarchy)
                panoramicWebView.WebView.LoadUrl(contentItem.ShowElement);
            break;
        }
    }
    private void OnEntetButtonCilck()
    {
        if(infiniteWheelController.transform.childCount != 0)
        {
            // string scenicSpotName = infiniteWheelController.transform.GetChild(infiniteWheelController.Index).name;
            string scenicSpotName = infiniteWheelController.transform.GetChild(infiniteWheelController.OpenID).name;
            nextToConnectButton(scenicSpotName,3);
        }
    }
    public void ClosePropButtonClick()
    {
        propUp.SetActive(false);
        StopCurrentPlayback();
    }
    public void QuitGame()
    {
        Debug.Log("Quit the Program");
        Application.Quit();
    }
}


//菜单项类
public class MenuItem
{
    public string Province { get; private set; }            // 省份名称
    public string City { get; private set; }                // 城市名称
    public Dictionary<string, ContentItem> CityContents { get; private set; } // 城市所包含的内容
    public MenuItem(string province, string city, Dictionary<string, ContentItem> cityContents)
    {
        Province = province;
        City = city;
        CityContents = cityContents;
    }
}

//内容项类
public class ContentItem
{
    public string ScenicSpotName   {get;    private set; }        // 内容名称
    public string CoverImage    {get;    private set; }        // 封面图片
    public string CoverText     {get;    private set; }        // 封面文字
    public string TriggerEvent  {get;    private set; }        // 触发事件
    public string DisplayImage  {get;    private set; }        // 展示图片
    public string ShowImage  {get;    private set; }        // 展示图片
    public string ShowElement  {get;    private set; }        // 展示图片
    public ContentItem(string scenicSpotName, string coverImage, string coverText, string triggerEvent, string displayImage,string showImage,string showElement)
    {
        ScenicSpotName = scenicSpotName;
        CoverImage = coverImage;
        CoverText = coverText;
        TriggerEvent = triggerEvent;
        DisplayImage = displayImage;
        ShowImage = showImage;
        ShowElement = showElement;
    }
}

public class CityContentCount
{
    public string ScenicSpotName { get; set; }
    public int ContentCount { get; set; }
}
