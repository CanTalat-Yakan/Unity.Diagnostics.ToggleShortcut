using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace UnityEssentials
{
    [DefaultExecutionOrder(-9000)]
    public sealed class ToggleShortcutHost : GlobalSingleton<ToggleShortcutHost>
    {
        private struct Binding
        {
            public Key Key;
            public ToggleModifiers Modifiers;
            public MonoBehaviour Target;
        }

        private readonly List<Binding> _bindings = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize() =>
            Instance.RefreshBindings();

        public void RefreshBindings()
        {
            var monoBehaviours = RuntimeDiscovery.AllMonoBehavioursCached;
            for (int i = 0; i < monoBehaviours.Length; i++)
            {
                var mb = monoBehaviours[i];
                if (mb == null) continue;
                Register(mb);
            }
        }

        private void Register(MonoBehaviour mb)
        {
            var attr = mb.GetType().GetCustomAttribute<ToggleShortcutAttribute>(true);
            if (attr == null)
                return;

            _bindings.Add(new Binding
            {
                Key = (Key)attr.Key,
                Modifiers = attr.Modifiers,
                Target = mb
            });
        }

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null)
                return;

            foreach (var b in _bindings)
            {
                if(!kb[b.Key].wasPressedThisFrame)
                    continue;

                if (!ModifiersHeld(kb, b.Modifiers))
                    continue;

                b.Target.enabled = !b.Target.enabled;
            }
        }

        private static bool ModifiersHeld(Keyboard kb, ToggleModifiers modifiers)
        {
            if ((modifiers & ToggleModifiers.Control) != 0 &&
                !kb.ctrlKey.isPressed)
                return false;

            if ((modifiers & ToggleModifiers.Shift) != 0 &&
                !kb.shiftKey.isPressed)
                return false;

            if ((modifiers & ToggleModifiers.Alt) != 0 &&
                !kb.altKey.isPressed)
                return false;

            return true;
        }
    }
}