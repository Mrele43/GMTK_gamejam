using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/// <summary>
/// 不继承Nono的单例模式类，继承改类需要补充当前类的无参构造函数（私有）
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class BaseMgr<T> where T : class
{
    private static T instance;

    protected static readonly object obj = new object();

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                lock(obj)
                {
                    if (instance == null)
                    {
                        ConstructorInfo info = typeof(T).GetConstructor(
                            BindingFlags.Instance | BindingFlags.NonPublic,
                            null,
                            Type.EmptyTypes,
                            null);

                        if (info != null)
                            instance = info.Invoke(null) as T;
                        else
                            Debug.LogError("没有得到对应的无参构造函数");
                    }
                }
               
            }                      
                return instance;
        }
    }
}



//不继承Mono存在的安全问题

//对于不继承Mono的单例模式，可以在外部进行 new（），破环了单例模式的唯一性
//因此围绕构造函数进行安全优化

//对于不继承Mono的单例模式，在进行多线程的使用的时候可能同时执行单例的初始化，就会发生问题，
//这时候就用lock来保证每次只有一个线程使用这个单例模式

//注意看子类，子类还有多线程处理方法的情况，也是同理用lock来保证每次只有一个线程调用