using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class Scene1_LifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {


        builder.RegisterEntryPoint<Scene1EntryPoint>();
    }
}

public class Scene1EntryPoint : IStartable
{
    private readonly ILabelManager _labelManager;
    private readonly IActionBus _actionBus;

    public Scene1EntryPoint(ILabelManager labelManager, IActionBus actionBus)
    {
        _labelManager = labelManager;
        _actionBus = actionBus;
    }

    public void Start()
    {
        Debug.Log("[ExampleScene1] 初始化完成");
    }
}
