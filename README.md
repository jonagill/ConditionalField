# Conditional Field
Conditional Field is a simple Unity extension that allows you to easily show and hide serialized fields in the Inspector window based on other values on the target object.

## Installation
Conditional Field relies on my [Unity Internal Access](https://github.com/jonagill/UnityInternalAccess) library for certain functionality.

### Install via Git
1. Open Window/Package Manager
2. Click the + button
3. Select Add Package From Git URL
4. Paste `https://github.com/jonagill/UnityInternalAccess.git?path=Packages/com.jonagill.unityinternalaccess` into the URL field
5. Click Install
6. Repeat, pasting `https://github.com/jonagill/ConditionalField.git?path=Packages/com.jonagill.conditionalfield` instead

### Installation via OpenUPM
To install  via [OpenUPM](https://openupm.com/packages/com.jonagill.autofill/):

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
To mark a field as conditionally displayed, simply add the `[ConditionalField]` attribute to it, as below:

```
// Displays if the target boolean is true
[SerializeField, ConditionalField(nameof(_toggleValue))] private int _conditionalField;
[SerializeField] private bool _toggleValue;
```

The `[ConditionalField]` attribute requires the name of another field, property, or parameterless method on the same object. When deciding whether to display your field or not, we check the current value of the target field and compare it against an expected value or values. Here are some other examples:

```
// Displays if the target enum is a specific value
[SerializeField] private ModeEnum _mode;
[SerializeField, ConditionalField(nameof(_mode), ModeEnum.Flight)] private string _conditionalField;

```

```
// Displays if the target enum is one of several specific values
[SerializeField] private ModeEnum _mode;
[SerializeField, ConditionalField(nameof(_mode), new[] { ModeEnum.Attack, ModeEnum.Flight })] private string _conditionalField;
```

```
// Displays if the target property returns true
[SerializeField, ConditionalField(nameof(TimeElapsed))] private string _conditionalField;
private bool TimeElapsed => Time.realtimeSinceStartup > 1f;
```

## Options

You can pass several options into your `[ConditionaField]` attribute to modify its behavior.

* `Options.Invert`: If set, your field when be hidden when the target value does NOT match the expected value(s).
* `Options.Chain`: This is set by default. If set, your field will be hidden if its target value is also a `[ConditionalField]` field and is currently hidden.
* `Options.ShowDisabled`: If set, your field will be drawn disabled instead of completely hidden.


