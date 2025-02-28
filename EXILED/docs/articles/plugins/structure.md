---
title: Plugin Structure
---

This tutorial assumes that you are familiar with C#.

### Plugin Structure
In order to be loaded onto the framework, *every* plugin must follow a certain structure and inherit from certain members. If this is not achieved, the plugin will not execute. This tutorial will explain the proper setup for a plugin on the EXILED framework.

## Plugin Core
Every plugin must have a .cs file that consists of the plugin class itself. This file (and the class itself) are typically simply named "Plugin"; however, any name is appropriate for the main plugin class. This example will use "Plugin" as the name of the class.

After the main file is created, the Plugin class must be declared as a plugin, so that the EXILED framework loads it. This can be done by inheriting the `Plugin<IConfig>` class, provided in the `Exiled.API.Features` namespace.

The following example shows how to properly inherit the class. However, notice the `Config` class inside of the angled brackets. This class must be created and must inherit from `IConfig`, which is part of the `Exiled.API.Interfaces` namespace. Upon the creation of the Config class, the interface will require you to add an `IsEnabled` property.
```cs
namespace MyPluginNamespace
{
    using Exiled.API.Features;
    public class Plugin : Plugin<Config>
    {
        // This plugin will now be recognized by the EXILED framework!
    }
    // It is strongly encouraged to create a separate file for your Config class.
    using Exiled.API.Interfaces;
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; }
    }
}
```
By creating the `Config` class and including it in the angled brackets, the rest of the plugin's code, as well as the EXILED framework, will recognize that the class resembles configuration for server owners. For more information about setting up configuration, see the Configuration section below.

## OnEnabled and OnDisabled
The plugin is now successfully loaded onto the framework. However, it doesn't actually do anything; no functionality has been assigned. The `Plugin<IConfig>` class provides two overridable methods in order to give the plugin functionality: `OnEnabled` and `OnDisabled`. These two methods do exactly as they sound: Execute when the plugin is enabled/loaded, and when it is disabled.

The following example shows how to utilize these methods to send a message to the console.
```cs
namespace MyPluginNamespace
{
    using Exiled.API.Features;
    public class Plugin : Plugin<Config>
    {
        public override void OnEnabled()
        {
            Log.Info("My plugin has been enabled!");
        }
        public override void OnDisabled()
        {
            Log.Info("My plugin has been disabled!");
        }
    }
    // Config.cs file
    using Exiled.API.Interfaces;
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; }
    }
}
```
All of the code for the plugin *must* be enabled in the OnEnabled method, and *must* be disabled on the OnDisabled method. It is important that these two methods execute as expected, because server hosts can enable and disable plugins as much as they'd like, and the plugin *must* be able to respond to these changes appropriately.

## Plugin Data
In order for a plugin to be submitted for public use, the plugin must override three properties: `Name`, `Author`, and `Version`. The first two are strings, whereas the last one is a `Version` class (`using System;` is required).

The following example shows how to properly override this data.
```cs
namespace MyPluginNamespace
{
    using System;
    using Exiled.API.Features;
    public class Plugin : Plugin<Config>
    {
        public override string Name => "My Awesome Plugin";
        public override string Author => "MyName";
        public override Version Version => new Version(1, 0, 0);
    }
    // ...
}
```

## Configuration
This section is related to creating and reading the value of configuration.

### Creating Configs
A lot of plugins provide configuration to allow the server hosts to change various features of the plugin. Luckily, creating configuration is very simple.

To start, take a look at your Config.cs file.
```cs
namespace MyPluginNamespace
{
    using Exiled.API.Interfaces;
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; }
        public bool Debug { get; set; }
    }
}
```
There is currently one config, called `IsEnabled`. As stated above, this config is required and cannot be removed. However, more config can be added. The YAML serialization allows almost any type to be added and still work, including bools, ints, arrays of anything, enums, and even whole classes!

In the following example, a config file with three configs is created.
```cs
namespace MyPluginNamespace
{
    using Exiled.API.Interfaces;
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; }
        public bool Debug { get; set; }
        public bool MyBoolConfig { get; set; }
        public string MyStringConfig { get; set; }
        public int MyIntConfig { get; set; } = 5; // Set to 5 by default.
    }
}
```
To server hosts, the functionality of these configs might be confusing at first. So, the `System.ComponentModel.DescriptionAttribute` can be used to provide a description for each config!
```cs
namespace MyPluginNamespace
{
    using System.ComponentModel;
    using Exiled.API.Interfaces;
    public class Config : IConfig
    {
        [Description("Whether the plugin is enabled.")]
        public bool IsEnabled { get; set; }
        [Description("Whether debug messages should be shown in the console.")]
        bool Debug { get; set; }
        [Description("Config that must be true or false!")]
        public bool MyBoolConfig { get; set; }
        [Description("Config that must be a string!")]
        public string MyStringConfig { get; set; }
        [Description("Config that must be a number! Defaults to 5.")]
        public int MyIntConfig { get; set; } = 5;
    }
}
```

### Reading Configs
> [!NOTE]
> You do not need to read the value of the `IsEnabled` config; EXILED will automatically prevent your plugin from executing if its `IsEnabled` config is set to false.


Reading configuration is more simple than creating it. The base `Plugin<IConfig>` class provides a property, called `Config`, which can be used to access these values.

In the following example, our config from the previous class is displayed when the plugin starts.
```cs
namespace MyPluginNamespace
{
    using Exiled.API.Features;
    public class Plugin : Plugin<Config>
    {
        public override void OnEnabled()
        {
            Log.Info("Boolean config: " + Config.MyBoolConfig);
            Log.Info("String config: " + Config.MyStringConfig);
            Log.Info("Int config: " + Config.MyIntConfig);
        }
    }
    // Config.cs file
    using System.ComponentModel;
    using Exiled.API.Interfaces;
    public class Config : IConfig
    {
        [Description("Whether the plugin is enabled.")]
        public bool IsEnabled { get; set; }
        [Description("Whether debug messages should be shown in the console.")]
        bool Debug { get; set; }
        [Description("Config that must be true or false!")]
        public bool MyBoolConfig { get; set; }
        [Description("Config that must be a string!")]
        public string MyStringConfig { get; set; }
        [Description("Config that must be a number! Defaults to 5.")]
        public int MyIntConfig { get; set; } = 5;
    }
}
```

