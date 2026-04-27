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
    private readonly IActionBus _eventBus;

    public Scene1EntryPoint(ILabelManager labelManager, IActionBus eventBus)
    {
        _labelManager = labelManager;
        _eventBus = eventBus;
    }

    public void Start()
    {
        Debug.Log("[ExampleScene1] 初始化完成");
    }
}