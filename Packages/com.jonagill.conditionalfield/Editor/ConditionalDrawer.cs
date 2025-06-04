using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityInternalAccess.Editor;

namespace ConditionalField
{
    [CustomPropertyDrawer(typeof(ConditionalFieldAttribute), true)]
    public class ConditionalHidePropertyDrawer : PropertyDrawer
    {
        private static readonly BindingFlags BINDING_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        
        private Dictionary<Type, Dictionary<string, ConditionalFieldAttribute>> cachedAttributeDict = 
            new Dictionary<Type, Dictionary<string, ConditionalFieldAttribute>>();

        private float HelpBoxHeight => EditorGUIUtility.singleLineHeight + 15f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (ShouldRenderField(property, out string warning))
            {
                if (!string.IsNullOrEmpty(warning))
                {
                    position.height = HelpBoxHeight;
                    EditorGUI.HelpBox(
                        position,
                        warning,
                        MessageType.Warning);
                    position.y += HelpBoxHeight;
                    position.height = EditorGUI.GetPropertyHeight(property, label);
                }

                DefaultPropertyDrawer.PropertyField(position, property, label, true);
            }
        }
        
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var rootElement = new VisualElement();

            var helpBox = new HelpBox(string.Empty, HelpBoxMessageType.Warning);
            var field = DefaultPropertyDrawer.CreatePropertyField( property );

            void UpdateVisibility()
            {
                if ( field == null )
                {
                    return;
                }

                ConditionalFieldAttribute conditionalFieldAttribute = (ConditionalFieldAttribute) attribute;
                bool enabled = ShouldRenderField( property, out string warning );

                if (!string.IsNullOrEmpty(warning))
                {
                    helpBox.text = warning;
                    helpBox.style.display = DisplayStyle.Flex;
                }
                else
                {
                    helpBox.style.display = DisplayStyle.None;
                }
                
                rootElement.style.display =
                    enabled || ((conditionalFieldAttribute.options & Conditional.Options.ShowDisabled) != 0 ) ?
                        DisplayStyle.Flex :
                        DisplayStyle.None;
                
                field.SetEnabled( enabled );
            }

            // Subscribe to any changes in our serialized object
            var cachedPropertyPath = property.propertyPath;
            field.TrackSerializedObjectValue( property.serializedObject, serializedObject =>
            {
                if ( serializedObject.FindProperty( cachedPropertyPath ) != null )
                {
                    UpdateVisibility();
                }
            } );
            
            UpdateVisibility();
            
            rootElement.Add(helpBox);
            rootElement.Add(field);
            
            return rootElement;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (ShouldRenderField(property, out string warning))
            {
                var height = DefaultPropertyDrawer.GetPropertyHeight(property, label);
                if (!string.IsNullOrEmpty(warning))
                {
                    height += HelpBoxHeight;
                }

                return height;
            }
            else
            {
                return -EditorGUIUtility.standardVerticalSpacing;
            }
        }
        
        private bool ShouldRenderField(SerializedProperty property, out string warning)
        {
            var conditional = (ConditionalFieldAttribute)attribute;
            string propertyPath = property.propertyPath;
            string parentPropertyPath = SerializedPropertyExtensions.GetParentPropertyPath(propertyPath, skipArrays: true);
            
            warning = string.Empty;
            foreach (var targetObject in property.serializedObject.targetObjects)
            {
                // Get the object that our property is a part of (either the base asset or some class or struct it's nested inside)
                object parentObject = string.IsNullOrEmpty(parentPropertyPath) ? 
                    targetObject : 
                    SerializedPropertyExtensions.GetPropertyValue(targetObject, parentPropertyPath);
                
                if (parentObject == null)
                {
                    continue;
                }

                if (!ShouldRenderField(parentObject, conditional, out warning))
                {
                    return false;
                }
            }

            // Default to showing the field
            return true;
        }

        private bool ShouldRenderField(object parentObject, ConditionalFieldAttribute conditionalField, out string warning)
        {
            var targetName = conditionalField.targetName;

            if (ProcessTargetObject(conditionalField, parentObject, targetName, out bool show, out warning))
            {
                if (!show)
                {
                    return false;
                }
            }

            if ((conditionalField.options & Conditional.Options.Chain) != 0)
            {
                if (!cachedAttributeDict.TryGetValue(parentObject.GetType(), out var dict))
                {
                    dict = new Dictionary<string, ConditionalFieldAttribute>();
                    cachedAttributeDict[parentObject.GetType()] = dict;
                }

                if (!dict.TryGetValue(targetName, out var attr))
                {
                    FieldInfo otherProperty = parentObject.GetType().GetField(targetName, BINDING_FLAGS);
                    if (otherProperty != null)
                    {
                        attr = otherProperty.GetCustomAttribute<ConditionalFieldAttribute>();
                    }
                    dict[targetName] = attr;
                }

                if (attr != null && attr != attribute)
                {
                    return ShouldRenderField(parentObject, attr, out _);
                }
            }

            return true;
        }

        /// <summary>
        /// Processes a particular object. Returns true if the object is forcing a specific visibility state
        /// </summary>
        private bool ProcessTargetObject(ConditionalFieldAttribute conditionalField, object targetObject, string targetName, out bool shouldShow, out string warning)
        {
            bool hasTarget = false;
            object targetValue = null;
            shouldShow = false;
            warning = null;

            System.Type objectType = targetObject.GetType();

            if (!hasTarget)
            {
                FieldInfo targetFieldInfo = null;
                for (Type type = objectType; targetFieldInfo == null && type != null; type = type.BaseType)
                {
                    targetFieldInfo = type.GetField(targetName, BINDING_FLAGS);
                }

                if (targetFieldInfo != null)
                {
                    targetValue = targetFieldInfo.GetValue(targetObject);
                    hasTarget = true;
                }
            }

            if (!hasTarget)
            {
                var targetPropertyInfo = objectType.GetProperty(targetName, BINDING_FLAGS);
                if (targetPropertyInfo != null && targetPropertyInfo.GetIndexParameters().Length == 0)
                {
                    targetValue = targetPropertyInfo.GetValue(targetObject, null);
                    hasTarget = true;
                }
            }

            if (!hasTarget)
            {
                var targetMethodInfo = objectType.GetMethod(targetName, BINDING_FLAGS, null, System.Type.EmptyTypes, null);

                if (targetMethodInfo != null)
                {
                    targetValue = targetMethodInfo.Invoke(targetObject, null);
                    hasTarget = true;
                }
            }

            if (hasTarget)
            {
                var matchedExpectedValue = false;

                // Conditionals can have more than one expected value (they are ORed together).
                foreach (var expectedValue in conditionalField.expectedValues)
                {
                    if (CompareTargetWithExpectedValue(targetValue, conditionalField.hasExpectedValue, expectedValue))
                    {
                        matchedExpectedValue = true;
                        break;
                    }
                }

                if ((conditionalField.options & Conditional.Options.Invert) != 0)
                {
                    matchedExpectedValue = !matchedExpectedValue;
                }

                if (!matchedExpectedValue)
                {
                    // If any object in a multi-selection is hiding this value, have them all hide it
                    shouldShow = false;
                    return true;
                }
            }
            else
            {
                // If the conditional is broken, show it along with a warning
                warning = string.Format("Missing conditional target field '{0}'", targetName);
                shouldShow = true;
                return true;
            }

            return false;
        }

        private bool CompareTargetWithExpectedValue(object targetValue, bool hasExpectedValue, object expectedValue)
        {
            if (!hasExpectedValue)
            {
                // Assume a default expected value of boolean true
                expectedValue = true;
            }

            // Switch comparison logic as a function of the provided expected value type.
            if (expectedValue is bool)
            {
                if (targetValue is UnityEngine.Object targetObject)
                {
                    return targetObject == (bool)expectedValue;
                }
                
                return Convert.ToBoolean(targetValue) == (bool)expectedValue;
            }

            if (expectedValue == null) return targetValue == null;
            if (targetValue == null) return expectedValue == null;

            Type targetType = targetValue.GetType();
            Type expectedType = expectedValue.GetType();
            
            if (targetType != expectedType)
            {
                //support using ints in place of enums and vice versa
                if (targetType.IsEnum && !expectedType.IsEnum)
                {
                    expectedValue = Enum.ToObject(targetType, expectedValue);
                }
                else if (!targetType.IsEnum && expectedType.IsEnum)
                {
                    targetValue = Enum.ToObject(expectedType, targetValue);
                }
                else if (typeof(IConvertible).IsAssignableFrom(targetType) && typeof(IConvertible).IsAssignableFrom(expectedType))
                {
                    //support implicit casts between int, float, etc.
                    expectedValue = Convert.ChangeType(expectedValue, targetValue.GetType());
                }
            }

            return targetValue.Equals(expectedValue);
        }
    }
}