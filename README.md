# Conditional Field
Conditional Field is a simple Unity extension that allows you to easily show and hide serialized fields in the Inspector window based on other values on the target object.

## Installation
We recommend you install Conditional Field via [OpenUPM](https://openupm.com/packages/com.jonagill.conditionalfield/). Per OpenUPM's documentation:

1. Open `Edit/Project Settings/Package Manager`
2. Add a new Scoped Registry (or edit the existing OpenUPM entry) to read:
    * Name: `package.openupm.com`
    * URL: `https://package.openupm.com`
    * Scope(s): `com.jonagill.conditionalfield` and `com.jonagill.unityinternalaccess`
3. Click Save (or Apply)
4. Open Window/Package Manager
5. Click the + button
6. Select `Add package by name...` or `Add package from git URL...` 
7. Enter `com.jonagill.unityinternalaccess` and click Add
8. Repeat steps 6 and 7 with `com.jonagill.conditionalfield`

## Basic usage
To mark a field as conditionally displayed, simply add the `[Conditional]` attribute to it, as below:

```
// Displays if the target boolean is true
[SerializeField, Conditional(nameof(_toggleValue))] private int _conditionalField;
[SerializeField] private bool _toggleValue;
```

The `[Conditional]` attribute requires the name of another field, property, or parameterless method on the same object. When deciding whether to display your field or not, we check the current value of the target field and compare it against an expected value or values. Here are some other examples:

```
// Displays if the target enum is a specific value
[SerializeField] private ModeEnum _mode;
[SerializeField, Conditional(nameof(_mode), ModeEnum.Flight)] private string _conditionalField;

```

```
// Displays if the target enum is one of several specific values
[SerializeField] private ModeEnum _mode;
[SerializeField, Conditional(nameof(_mode), new[] { ModeEnum.Attack, ModeEnum.Flight })] private string _conditionalField;
```

```
// Displays if the target property returns true
[SerializeField, Conditional(nameof(TimeElapsed))] private string _conditionalField;
private bool TimeElapsed => Time.realtimeSinceStartup > 1f;
```

## Options

You can pass several options into your `[Conditional]` attribute to modify its behavior.

* `Options.Invert`: If set, your field when be hidden when the target value does NOT match the expected value(s).
* `Options.Chain`: This is set by default. If set, your field will be hidden if its target value is also a `[Conditional]` field and is currently hidden.
* `Options.ShowDisabled`: If set, your field will be drawn disabled instead of completely hidden.


