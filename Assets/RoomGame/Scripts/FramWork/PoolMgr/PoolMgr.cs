using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine;


public abstract class BaseDataDrawer
{

}
/// <summary>
/// 存放数据结构类、逻辑类的抽屉，用于不继承Mono的对象
/// </summary>
/// <typeparam name="T">装载对象的类型</typeparam>
public class DataDrawer<T>: BaseDataDrawer where T : class 
{
    //用队列装载抽屉里面的类
    public Queue<T> dataQueue = new Queue<T>();  
}
/// <summary>
/// 想用对象池存储数据结构类，逻辑类必须要继承该接口
/// 该结构用于实现数据重置的方法，避免其他引用的冲突
/// </summary>
public interface I_DataDrawer
{
    void Reset();
}



//封装为抽屉类的数据结构，主要是有一个抽屉根来存放GameObject对象
public class Drawer
{

    //抽屉的固定容量
    private int maxNum;

    //是否获取抽屉固定容量属性
    public bool IsDrawerCapacityFull => useObjList.Count > maxNum;


    //抽屉根
    private GameObject drawerRoot;

    //储存未使用的单个对象容器 
    private Stack<GameObject> objStack = new Stack<GameObject>();

    //储存正在使用的单个对象容器
    private List<GameObject> useObjList = new List<GameObject>();

    //获取抽屉容量
    public int DarwerCount => objStack.Count;

    //获取正在使用的对象容器容量
    public int UseObjListCount => useObjList.Count;



    /// <summary>
    /// 抽屉构造函数，初始化抽屉的父对象位置和抽屉的名字
    /// </summary>
    /// <param name="root">对象池对象</param>
    /// <param name="name">抽屉对象的名字，即物体名字</param>
    public Drawer(GameObject root,string name,GameObject obj)
    {
        PoolObj poolObj = obj.GetComponent<PoolObj>();

        if (PoolMgr.isOpenLayout)
        {
            //设置抽屉根的父对象为对象池根
            drawerRoot = new GameObject();
            drawerRoot.transform.SetParent(root.transform);
            drawerRoot.name = name;
        }

        //实例化一个抽屉的时候，说明有对象要使用，那就直接存放在正在使用的容器当中
        PushInUseObjList(obj);


        if (poolObj == null)
        {
            Debug.LogError($"挂载PoolObj脚本，并为{name}这个对象的对应抽屉设置最大容量");
            return;
        }
        else
        {
            maxNum = poolObj.maxNum;
        }
    }

    //弹栈
    //public GameObject Pop()
    //{
    //    GameObject obj;

    //    // 只有空闲池有对象才复用
    //    if (DarwerCount > 0)
    //    {
    //        obj = objStack.Pop();
    //        useObjList.Add(obj);

    //        obj.SetActive(true);
    //        if (PoolMgr.isOpenLayout)
    //            obj.transform.SetParent(null);

    //        return obj;
    //    }
    //    // 空闲空了 → 返回 null
    //    else
    //    {
    //        return null;
    //    }
    //}
    //弹栈
    public GameObject Pop()
    {
        // 循环检查，跳过已被销毁的对象
        while (objStack.Count > 0)
        {
            GameObject obj = objStack.Pop();
            if (obj == null) continue; // 对象已被销毁，继续取下一个

            useObjList.Add(obj);
            obj.SetActive(true);
            if (PoolMgr.isOpenLayout)
                obj.transform.SetParent(null);
            return obj;
        }
        // 空闲池空了或全是无效对象
        return null;
    }

    //压栈
    public void Push(GameObject obj)
    {
        obj.SetActive(false);

        if(PoolMgr.isOpenLayout)
        obj.transform.SetParent(drawerRoot.transform);
        objStack.Push(obj);

        //使用完后应当在记录使用单个对象容器中进行删除
        useObjList.Remove(obj);
    }

    //存放正在使用的容器
    public void PushInUseObjList(GameObject obj)
    {
        useObjList.Add(obj);
    }
}

/// <summary>
/// 使用该管理器，存、放 ->物体<- 的方法时必须要对象挂载PoolObj脚本限制抽屉最大容量
/// </summary>
public class PoolMgr : BaseMgr<PoolMgr>
{

    //是否开启布局设置，在游戏正式发行的时候设置为false，减少性能消耗
    public static bool isOpenLayout = true;

    private PoolMgr() { }

    //存放GameObject的抽屉字典
    Dictionary<string,Drawer> poolDic = new Dictionary<string,Drawer>();

    //存放数据结构类，逻辑类的抽屉字典
    Dictionary<string, BaseDataDrawer> dataPoolDic = new Dictionary<string, BaseDataDrawer>();

    #region poolDic
    //对象池对象作为所有抽屉的根
    GameObject pool;
    /// <summary>
    /// 存放物体
    /// </summary>
    /// <param name="obj">存放的物体</param>
    //public void PushObj(GameObject obj)
    //{
    //    //失活对象
    //    obj.SetActive(false);
    //    //如果没有对象池根，再创建根，因为过场景时，对象池根会被删除，这时候就要重新创建根，每次检测到根为空就创建才合理
    //    if ((pool == null) && (isOpenLayout == true))
    //    {
    //        Debug.Log("创建了pool");
    //        pool = new GameObject("pool");
    //    }

    //    poolDic[obj.name].Push(obj);

    //}
    public void PushObj(GameObject obj)
    {
        //失活对象
        obj.SetActive(false);
        //如果没有对象池根，再创建根，因为过场景时，对象池根会被删除，这时候就要重新创建根，每次检测到根为空就创建才合理
        if ((pool == null) && (isOpenLayout == true))
        {
            Debug.Log("创建了pool");
            pool = new GameObject("pool");
        }

        // 增加安全检查：如果抽屉不存在，直接销毁对象，避免报错
        if (poolDic.ContainsKey(obj.name))
        {
            poolDic[obj.name].Push(obj);
        }
        else
        {
            Debug.LogWarning($"对象池中没有 {obj.name} 的抽屉，直接销毁对象");
            GameObject.Destroy(obj);
        }
    }

    /// <summary>
    /// 获取物体
    /// </summary>
    /// <param name="name">抽屉的名字</param>
    /// <returns>抽屉中存放的对象</returns>
    /// <summary>
    /// 获取物体
    /// </summary>
    /// <param name="name">抽屉的名字</param>
    /// <returns>抽屉中存放的对象</returns>
    public GameObject GetObj(string name, int maxNum = 5)
    {
        GameObject obj;

        // 如果没有对象池根，再创建根
        if ((pool == null) && (isOpenLayout == true))
        {
            Debug.Log("创建了pool");
            pool = new GameObject("pool");
        }

        // 没有抽屉，创建一个抽屉
        if (!poolDic.ContainsKey(name))
        {
            obj = GameObject.Instantiate(Resources.Load<GameObject>(name));
            if (obj == null)
            {
                Debug.LogError($"实例化{name}失败,获取的obj对象为空");
                return null;
            }
            obj.name = name;
            poolDic.Add(name, new Drawer(pool, name, obj));
            return obj;
        }
        // 核心修复：先取对象，判断是否为 null
        obj = poolDic[name].Pop();

        // 如果对象池返回了 null（无空闲对象）
        if (obj == null)
        {
            // 直接创建新的
            obj = GameObject.Instantiate(Resources.Load<GameObject>(name));
            obj.name = name;
            poolDic[name].PushInUseObjList(obj);
        }
        return obj;
    }

    #endregion

    #region dataPoolDic
    /// <summary>
    /// 放数据结构类、逻辑类对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="nameSpace">传入命名空间，可用于不同命名空间相同名字的类</param>
    /// <returns></returns>
    public void PushObj<T>(T obj, string nameSpace = "") where T : class, I_DataDrawer
    {
        //池子的名字
        string drawerName = nameSpace + "_" + typeof(T).Name;

        DataDrawer<T> dataDrawer;
        //如果有池子
        if (dataPoolDic.ContainsKey(drawerName))
        {
            dataDrawer = dataPoolDic[drawerName] as DataDrawer<T>;
        }
        else//没有池子
        {
            //新建抽屉
            dataDrawer = new DataDrawer<T>();
            //添加到字典
            dataPoolDic.Add(drawerName, dataDrawer);
        }
        //重置数据
        obj.Reset();
        //放入抽屉
        dataDrawer.dataQueue.Enqueue(obj);
    }

    /// <summary>
    /// 取数据结构类、逻辑类对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="nameSpace">传入命名空间，可用于不同命名空间相同名字的类</param>
    /// <returns></returns>
    public T GetObj<T>(string nameSpace = "") where T : class, I_DataDrawer, new()
    {
        //池子的名字
        string drawerName = nameSpace + "_" + typeof(T).Name;
        DataDrawer<T> dataDrawer;
        //如果有池子
        if (dataPoolDic.ContainsKey(drawerName))
        {
            dataDrawer = dataPoolDic[drawerName] as DataDrawer<T>;
            //如果池子中有对象，从池子中取出
            if (dataDrawer.dataQueue.Count > 0)
                return dataDrawer.dataQueue.Dequeue();
            else//没有对象直接创建一个新对象
                return new T();
        }
        else//没有池子,同样是new一个对象返回出去
        {
            return new T();
        }
    }
    #endregion

    public void Clear()
    {
        // 清空所有抽屉中的对象（销毁实际物体）
        foreach (var drawer in poolDic.Values)
        {
            // 简单清理：把空闲栈中的对象销毁
            while (drawer.DarwerCount > 0)
            {
                GameObject obj = drawer.Pop();
                if (obj != null) GameObject.Destroy(obj);
            }
            // 注意：正在使用的对象无法在此处销毁，因为它们可能还在场景中，但场景切换时会自动销毁
        }
        poolDic.Clear();
        // 不再设置 pool = null，保留根节点
        // pool = null;  // 注释掉这一行

        dataPoolDic.Clear();
    }
}
