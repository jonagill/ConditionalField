using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ConditionalField
{
    /// <summary>
    /// Reflection-based accessors for Unity's internal ScriptAttributeUtility class
    /// </summary>
    internal static class ScriptAttributeUtilityInternal
    {
        private static readonly MethodInfo ScriptAttributeUtility_GetFieldInfoAndStaticTypeFromProperty;
        private static readonly MethodInfo ScriptAttributeUtility_GetScriptTypeFromProperty;
        private static readonly MethodInfo ScriptAttributeUtility_GetFieldInfoFromProperty;
        private static readonly MethodInfo ScriptAttributeUtility_GetFieldAttributesMethod;
        private static readonly MethodInfo ScriptAttributeUtility_GetDrawerTypeForType;
        private static readonly MethodInfo ScriptAttributeUtility_GetDrawerTypeForPropertyAndType;


        static ScriptAttributeUtilityInternal()
        {
            var scriptAttributeUtilityType =
                typeof(EditorApplication).Assembly.GetType( "UnityEditor.ScriptAttributeUtility" );
            ScriptAttributeUtility_GetFieldInfoAndStaticTypeFromProperty =
                scriptAttributeUtilityType.GetMethod( "GetFieldInfoAndStaticTypeFromProperty", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static );
            ScriptAttributeUtility_GetScriptTypeFromProperty =
                scriptAttributeUtilityType.GetMethod( "GetScriptTypeFromProperty", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static );
            ScriptAttributeUtility_GetFieldInfoFromProperty =
                scriptAttributeUtilityType.GetMethod( "GetFieldInfoFromProperty", BindingFlags.NonPublic | BindingFlags.Static);
            ScriptAttributeUtility_GetFieldAttributesMethod =
                scriptAttributeUtilityType.GetMethod( "GetFieldAttributes", BindingFlags.NonPublic | BindingFlags.Static);
            ScriptAttributeUtility_GetDrawerTypeForType =
                scriptAttributeUtilityType.GetMethod( "GetDrawerTypeForType", BindingFlags.NonPublic | BindingFlags.Static);
            ScriptAttributeUtility_GetDrawerTypeForPropertyAndType =
                scriptAttributeUtilityType.GetMethod( "GetDrawerTypeForPropertyAndType", BindingFlags.NonPublic | BindingFlags.Static);
        }

        public static FieldInfo GetFieldInfoAndStaticTypeFromProperty(SerializedProperty property, out Type type)
        {
            var parameters = new object[] { property, null };
            var fieldInfo =
                (FieldInfo)ScriptAttributeUtility_GetFieldInfoAndStaticTypeFromProperty.Invoke( null, parameters );

            type = (Type) parameters[1];
            return fieldInfo;
        }

        public static FieldInfo GetFieldInfoFromProperty(SerializedProperty serializedProperty, out Type fieldType)
        {
            fieldType = null;

            object[] parameters = {serializedProperty, fieldType};
            var fieldInfo = (FieldInfo) ScriptAttributeUtility_GetFieldInfoFromProperty.Invoke(null, parameters);
            if (fieldInfo != null)
            {
                // The 'out' parameter gets written into the parameters array
                fieldType = (Type) parameters[1];
            }

            return fieldInfo;
        }

        public static Type GetScriptTypeFromProperty(SerializedProperty property)
        {
            return (Type) ScriptAttributeUtility_GetScriptTypeFromProperty.Invoke( null, new object[] { property } );
        }

        public static List<PropertyAttribute> GetFieldAttributes(FieldInfo fieldInfo)
        {
            return (List<PropertyAttribute>) ScriptAttributeUtility_GetFieldAttributesMethod.Invoke(null, new object[] { fieldInfo });
        }

        public static IEnumerable<T> GetFieldAttributes<T>(FieldInfo fieldInfo) where T : PropertyAttribute
        {
            var attributes = GetFieldAttributes( fieldInfo );
            if ( attributes != null )
            {
                return attributes
                    .Select( a => a as T )
                    .Where( a => a != null );
            }

            return Array.Empty<T>();
        }

        public static Type GetDrawerTypeForType(Type type)
        {
            return (Type) ScriptAttributeUtility_GetDrawerTypeForType.Invoke( null, new object[] { type } );
        }

        public static Type GetDrawerTypeForPropertyAndType(SerializedProperty property, Type type)
        {
            return (Type) ScriptAttributeUtility_GetDrawerTypeForType.Invoke( null, new object[] { property, type } );
        }
    }    
}