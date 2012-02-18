﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace ProtoChannel.Web.Util
{
    internal static class ReflectionOptimizer
    {
        private static readonly object _syncLock = new object();
        private static readonly Dictionary<Type, MethodInfo> _converters = new Dictionary<Type, MethodInfo>();

        public delegate object Getter(object obj);
        public delegate void Setter(object obj, object value);

        public static Getter BuildGetter(MemberInfo memberInfo)
        {
            if (memberInfo == null)
                throw new ArgumentNullException("memberInfo");

            if (memberInfo is FieldInfo)
                return BuildGetter((FieldInfo)memberInfo);
            else if (memberInfo is PropertyInfo)
                return BuildGetter((PropertyInfo)memberInfo);
            else
                throw new ArgumentOutOfRangeException("memberInfo");
        }

        public static Getter BuildGetter(FieldInfo fieldInfo)
        {
            if (fieldInfo == null)
                throw new ArgumentNullException("fieldInfo");

            var method = new DynamicMethod(
                fieldInfo.DeclaringType.Name + "_" + fieldInfo.Name + "_Getter",
                typeof(object),
                new[] { typeof(object) },
                true
            );

            var il = method.GetILGenerator();

            // Load argument

            il.Emit(OpCodes.Ldarg_0);

            // Cast the argument to the correct type

            il.Emit(OpCodes.Castclass, fieldInfo.DeclaringType);

            // Call load field.

            il.Emit(OpCodes.Ldfld, fieldInfo);

            // Box value types

            if (fieldInfo.FieldType.IsValueType)
                il.Emit(OpCodes.Box, fieldInfo.FieldType);

            // Return result

            il.Emit(OpCodes.Ret);

            return (Getter)method.CreateDelegate(typeof(Getter));
        }

        public static Getter BuildGetter(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
                throw new ArgumentNullException("propertyInfo");

            var getterMethod = propertyInfo.DeclaringType.GetMethod(
                "get_" + propertyInfo.Name,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
            );

            var method = new DynamicMethod(
                propertyInfo.DeclaringType.Name + "_" + propertyInfo.Name + "_Getter",
                typeof(object),
                new[] { typeof(object) },
                true
            );

            var il = method.GetILGenerator();

            // Load argument

            il.Emit(OpCodes.Ldarg_0);

            // Cast the argument to the correct type

            il.Emit(OpCodes.Castclass, propertyInfo.DeclaringType);

            // Call the getter on the argument

            il.Emit(OpCodes.Callvirt, getterMethod);

            // Box value types

            if (propertyInfo.PropertyType.IsValueType)
                il.Emit(OpCodes.Box, propertyInfo.PropertyType);

            // Return result

            il.Emit(OpCodes.Ret);

            return (Getter)method.CreateDelegate(typeof(Getter));
        }

        public static Setter BuildSetter(MemberInfo memberInfo)
        {
            return BuildSetter(memberInfo, false);
        }

        public static Setter BuildSetter(MemberInfo memberInfo, bool convert)
        {
            if (memberInfo == null)
                throw new ArgumentNullException("memberInfo");

            if (memberInfo is FieldInfo)
                return BuildSetter((FieldInfo)memberInfo, convert);
            else if (memberInfo is PropertyInfo)
                return BuildSetter((PropertyInfo)memberInfo, convert);
            else
                throw new ArgumentOutOfRangeException("memberInfo");
        }

        public static Setter BuildSetter(FieldInfo fieldInfo)
        {
            return BuildSetter(fieldInfo, false);
        }

        public static Setter BuildSetter(FieldInfo fieldInfo, bool convert)
        {
            if (fieldInfo == null)
                throw new ArgumentNullException("fieldInfo");

            var method = new DynamicMethod(
                fieldInfo.DeclaringType.Name + "_" + fieldInfo.Name + "_Setter",
                typeof(void),
                new[] { typeof(object), typeof(object) },
                true
            );

            var il = method.GetILGenerator();

            // Cast the target object

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Castclass, fieldInfo.DeclaringType);

            // Convert the value

            il.Emit(OpCodes.Ldarg_1);

            if (convert)
                il.Emit(OpCodes.Call, GetConverter(fieldInfo.FieldType));
            else if (fieldInfo.FieldType.IsValueType)
                il.Emit(OpCodes.Unbox_Any, fieldInfo.FieldType);
            else
                il.Emit(OpCodes.Castclass, fieldInfo.FieldType);

            // Set the value

            il.Emit(OpCodes.Stfld, fieldInfo);

            // Return

            il.Emit(OpCodes.Ret);

            return (Setter)method.CreateDelegate(typeof(Setter));
        }

        public static Setter BuildSetter(PropertyInfo propertyInfo)
        {
            return BuildSetter(propertyInfo, false);
        }

        public static Setter BuildSetter(PropertyInfo propertyInfo, bool convert)
        {
            if (propertyInfo == null)
                throw new ArgumentNullException("propertyInfo");

            var setterMethod = propertyInfo.DeclaringType.GetMethod(
                "set_" + propertyInfo.Name,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
            );

            var method = new DynamicMethod(
                propertyInfo.DeclaringType.Name + "_" + propertyInfo.Name + "_Setter",
                typeof(void),
                new[] { typeof(object), typeof(object) },
                true
            );

            var il = method.GetILGenerator();

            // Cast the target object

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Castclass, propertyInfo.DeclaringType);

            // Convert the value

            il.Emit(OpCodes.Ldarg_1);

            if (convert)
                il.Emit(OpCodes.Call, GetConverter(propertyInfo.PropertyType));
            else if (propertyInfo.PropertyType.IsValueType)
                il.Emit(OpCodes.Unbox_Any, propertyInfo.PropertyType);
            else
                il.Emit(OpCodes.Castclass, propertyInfo.PropertyType);

            // Set the value

            il.Emit(OpCodes.Callvirt, setterMethod);

            // Return

            il.Emit(OpCodes.Ret);

            return (Setter)method.CreateDelegate(typeof(Setter));
        }

        private static MethodInfo GetConverter(Type type)
        {
            lock (_syncLock)
            {
                MethodInfo converter;

                if (!_converters.TryGetValue(type, out converter))
                {
                    var method = typeof(ReflectionOptimizer).GetMethod(
                        "Convert",
                        BindingFlags.Static | BindingFlags.NonPublic
                    );

                    converter = method.MakeGenericMethod(
                        type,
                        Nullable.GetUnderlyingType(type) ?? type
                    );

                    _converters.Add(type, converter);
                }

                return converter;
            }
        }

        private static TResult Convert<TResult, TTarget>(object value)
        {
            if (value == null)
                return default(TResult);
            else if (value is TTarget)
                return (TResult)value;
            else
                return (TResult)System.Convert.ChangeType(value, typeof(TTarget));
        }
    }
}
