﻿using System;
using System.Reflection;

namespace HaE_Hamtweaks_Torch
{
    public static class ReflectionUtils
    {
        private const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        private const BindingFlags StaticFlags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;

        public static MethodInfo Method(Type type, string name, BindingFlags flags)
        {
            return type.GetMethod(name, flags) ?? throw new Exception($"Couldn't find method {name} on {type}");
        }

        public static MethodInfo InstanceMethod(Type t, string name)
        {
            return Method(t, name, InstanceFlags);
        }

        public static MethodInfo StaticMethod(Type t, string name)
        {
            return Method(t, name, StaticFlags);
        }
    }
}

