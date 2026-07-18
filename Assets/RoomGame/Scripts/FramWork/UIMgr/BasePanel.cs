using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 继承这个类的面板，一定要给需要使用的控件重新命名（该控件有逻辑），否则将会忽略该控件
/// </summary>
public abstract class BasePanel : MonoBehaviour
{
    //存储该面板的所有需要使用到的组件
    protected Dictionary<string,UIBehaviour> controlDic = new Dictionary<string,UIBehaviour>();

    /// <summary>
    /// 控件默认名字 如果得到的控件名字存在于这个容器 意味着不会通过代码去使用它 它只会是起到显示作用的控件
    /// </summary>
    private static List<string> defaultNameList = new List<string>() { "Image",
                                                                   "Text (TMP)",
                                                                   "RawImage",
                                                                   "Background",
                                                                   "Checkmark",
                                                                   "Label",
                                                                   "Text (Legacy)",
                                                                   "Arrow",
                                                                   "Placeholder",
                                                                   "Fill",
                                                                   "Handle",
                                                                   "Viewport",
                                                                   "Scrollbar Horizontal",
                                                                   "Scrollbar Vertical"};


    //可以重写获取额外的组件
    protected virtual void Awake()
    {
        //为了避免 某一个对象上存在两种控件的情况
        //我们应该优先查找重要的组件
        FindChildrenControl<Button>();
        FindChildrenControl<Toggle>();
        FindChildrenControl<Slider>();
        FindChildrenControl<InputField>();
        FindChildrenControl<ScrollRect>();
        FindChildrenControl<Dropdown>();
        //即使对象上挂在了多个组件 只要优先找到了重要组件
        //之后也可以通过重要组件得到身上其他挂载的内容
        FindChildrenControl<Text>();
        FindChildrenControl<TextMeshPro>();
        FindChildrenControl<Image>();
    }

    //添加各组件的监听事件，如果遇到有相同类型的组件，通过组件名字区分
    //使用方法：子类重写对应方法时，用switch（name）来判断，name是你自己命名的所以可以知道，对应添加方法即可
    protected virtual void ButtonClick(string name)
    {

    }
   
    protected virtual void SliderChange(string name,float value)
    {

    }
    protected virtual void ToggleClick(string name,bool value)
    {

    }

    /// <summary>
    /// 隐藏面板
    /// </summary>
    public virtual void HideMe()
    {
        this.gameObject.SetActive(false);
    }


    /// <summary>
    /// 显示面板
    /// </summary>
    public virtual void ShowMe()
    {
        this.gameObject.SetActive(true);
    }


    protected virtual void FindChildrenControl<T>() where T : UIBehaviour
    {
        T []controls = this.GetComponentsInChildren<T>(true);
        for(int i = 0; i < controls.Length; i++)
        {
            //由于闭包，内部值会返回最终值，也就意味着每一个controls[i].name实际上i都为controls.Length
            //所以下面这一行代码来避免闭包
            string temName = controls[i].name;
            if (!controlDic.ContainsKey(controls[i].gameObject.name) && !defaultNameList.Contains(controls[i].gameObject.name))
            {
                controlDic.Add(controls[i].gameObject.name, controls[i]);
                //添加组件的监听事件，如果有其他的组件事件需要添加，就继续往后面加
                if (controls[i] is Button)
                {
                    (controls[i] as Button).onClick.AddListener(() =>
                    {
                        ButtonClick(temName);
                    });
                }
                else if (controls[i] is Slider)
                {
                    (controls[i] as Slider).onValueChanged.AddListener((value) =>
                    {
                        SliderChange(temName, value);
                    });
                }
                else if (controls[i] is Toggle)
                {
                    (controls[i] as Toggle).onValueChanged.AddListener((value) =>
                    {
                        ToggleClick(temName, value);
                    });
                }

            }
        }
    }

    /// <summary>
    /// 获取对应的组件
    /// </summary>
    /// <typeparam name="T">组件类型</typeparam>
    /// <param name="name">组件名字</param>
    /// <returns></returns>

    protected T GetControl<T>(string name) where T : UIBehaviour
    {
        if (controlDic.ContainsKey(name))
        {
            T control = controlDic[name] as T;
            if (controlDic[name] == null)
            Debug.LogError($"没有找到名字为{name},类型为{typeof(T)}的组件");
            return control;
        }
        else
        {
            Debug.LogError($"没有找到名字为{name},类型为{typeof(T)}的组件");
            return null;
        }
           
    }
}
