using UnityEngine;

/// <summary>
/// 【稳定版】继承MonoBehaviour的单例基类
/// 作用：所有管理器（GridMgr、UIMgr、BattleMgr）都可以继承这个
/// 特点：
/// 1. 自动防重复实例
/// 2. 重复对象一创建立刻自杀，不污染数据
/// 3. 优先使用场景里手动摆放、配置好参数的对象
/// 4. 切换场景不销毁
/// </summary>
/// <typeparam name="T">子类的类型</typeparam>
public class BaseMonoMgr<T> : MonoBehaviour where T : MonoBehaviour
{
    // 静态单例实例（全局唯一）
    private static T instance;

    // 标记：是否是代码自动创建的兜底对象（不是场景里手动放的）
    private static bool isManualCreate = false;

    /// <summary>
    /// 全局访问点（ anywhere in code -> XXXMgr.Instance ）
    /// </summary>
    public static T Instance
    {
        get
        {
            // 如果还没有单例实例
            if (instance == null)
            {
                // 第一步：先去场景里找，有没有已经挂好的对象
                instance = FindObjectOfType<T>();

                // 如果场景里也没有
                if (instance == null)
                {
                    // 自动创建一个空GameObject，挂上这个脚本
                    GameObject obj = new GameObject(typeof(T).Name);
                    instance = obj.AddComponent<T>();

                    // 切换场景不销毁
                    DontDestroyOnLoad(obj);

                    // 标记：这是代码自动创建的兜底对象
                    isManualCreate = true;
                }
                else
                {
                    // 场景里找到了，设置为过场景不销毁
                    DontDestroyOnLoad(instance.gameObject);
                }
            }

            // 返回唯一实例
            return instance;
        }
    }

    /// <summary>
    /// 物体唤醒时执行（Unity自带）
    /// 这里只做【单例安全判断】，不做业务逻辑！
    /// </summary>
    protected virtual void Awake()
    {
        // 如果是代码自动创建的兜底单例，不参与竞争，直接跳过
        if (isManualCreate)
            return;

        // ==============================================
        // 【核心稳定逻辑】
        // 如果已经存在单例，并且我不是那个正版单例
        // ==============================================
        if (instance != null && instance != this)
        {
            // 我是重复盗版对象 → 立刻自杀！
            // 重点：自杀前不执行任何业务逻辑，不注册事件、不写字典
            Destroy(gameObject);
            return;
        }

        // 我是正版唯一实例
        instance = this as T;

        // 过场景不销毁
        DontDestroyOnLoad(gameObject);

        // 执行业务初始化（交给子类去写）
        OnInit();
    }

    /// <summary>
    /// 子类重写这个方法来做初始化
    /// 只有【正版单例】会执行这里
    /// 盗版实例根本跑不到这里！
    /// </summary>
    protected virtual void OnInit() { }
}